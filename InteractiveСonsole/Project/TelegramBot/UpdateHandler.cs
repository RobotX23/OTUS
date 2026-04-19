using InteractiveСonsole.Project.Core.Exceptions;
using InteractiveСonsole.Project.Core.Services;
using InteractiveСonsole.Project.TelegramBot.Dto;
using InteractiveСonsole.Project.TelegramBot.Scenarios;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;


namespace InteractiveСonsole
{
    internal class UpdateHandler : IUpdateHandler
    {
        private string? name = null;
        private ToDoUser? user2;
        private Update _update;

        private readonly ITelegramBotClient _botClient;
        private readonly IToDoService _toDoService;
        private readonly IUserService _userService;
        private readonly IToDoListService _toDoListService;
        private readonly IToDoRepository _toDoRepository;
        private readonly IEnumerable<IScenario> _scenarios;
        private readonly IScenarioContextRepository _scenarioContextRepository;

        public UpdateHandler(IToDoService toDoService, IUserService userService, IToDoRepository toDoRepository, ITelegramBotClient botClient, IEnumerable<IScenario> scenarios, IScenarioContextRepository scenarioContextRepository, IToDoListService toDoListService)
        {
            _toDoService = toDoService ?? throw new ArgumentNullException(nameof(toDoService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _toDoRepository = toDoRepository ?? throw new ArgumentNullException(nameof(toDoRepository));
            _botClient = botClient;
            _toDoListService = toDoListService ?? throw new ArgumentNullException(nameof(toDoListService));
            _scenarios = scenarios;
            _scenarioContextRepository = scenarioContextRepository;
        }


        private IScenario GetScenario(ScenarioType scenario)
        {
            var result = _scenarios.FirstOrDefault(x => x.CanHandle(scenario));
            if (result == null)
            {
                throw new Exception($"Сценарий {scenario} не найден!");
            }
            return result;
        }



        private async Task ProcessScenario(ScenarioContext context, Message message, CancellationToken ct, CallbackQuery callbackQuery)
        {
            var scenario = GetScenario(context.CurrentScenario);

            try
            {
                var result = await scenario.HandleMessageAsync(_botClient, context, message, ct, callbackQuery);

                if (result == ScenarioResult.Completed)
                {
                    if (message == null)
                    {
                        await _scenarioContextRepository.ResetContext(callbackQuery.From!.Id, ct);
                        var registeredUser = await _userService.GetUser(callbackQuery.From.Id, ct);
                        ChangeKeyboard(registeredUser.TelegramUserId, _botClient);
                    }
                    else
                    {
                        if (_update.Message == null)
                        {
                            await _scenarioContextRepository.ResetContext(callbackQuery.From!.Id, ct);
                            var registeredUser = await _userService.GetUser(callbackQuery.From.Id, ct);
                            ChangeKeyboard(registeredUser.TelegramUserId, _botClient);
                        }
                        else
                        {
                            await _scenarioContextRepository.ResetContext(message.From!.Id, ct);
                            ChangeKeyboard(_update.Message.Chat.Id, _botClient);

                        }
                    }
                }
                else
                {
                    if (message == null)
                    {
                        await _scenarioContextRepository.SetContext(callbackQuery.From!.Id, context, ct);
                    }
                    else
                    {
                        await _scenarioContextRepository.SetContext(message.From!.Id, context, ct);
                    }
                }
            }
            catch (TaskCountLimitException ex)
            {
                await HandleErrorAsync(_botClient, ex, HandleErrorSource.HandleUpdateError, ct);
                var userId = message?.From?.Id ?? callbackQuery.From!.Id;
                await _botClient.SendMessage(userId, $"Ошибка ввода: {ex.Message}", cancellationToken: ct);
                await _scenarioContextRepository.ResetContext(userId, ct);
                var registeredUser = await _userService.GetUser(userId, ct);
                ChangeKeyboard(registeredUser.TelegramUserId, _botClient);
            }
            catch (TaskLengthLimitException ex)
            {
                await HandleErrorAsync(_botClient, ex, HandleErrorSource.HandleUpdateError, ct);
                var userId = message?.From?.Id ?? callbackQuery.From!.Id;
                await _botClient.SendMessage(userId, $"Ошибка ввода: {ex.Message}", cancellationToken: ct);
                await _scenarioContextRepository.ResetContext(userId, ct);
                var registeredUser = await _userService.GetUser(userId, ct);
                ChangeKeyboard(registeredUser.TelegramUserId, _botClient);
            }
            catch (DublicateTaskException ex)
            {
                await HandleErrorAsync(_botClient, ex, HandleErrorSource.HandleUpdateError, ct);
                var userId = message?.From?.Id ?? callbackQuery.From!.Id;
                await _botClient.SendMessage(userId, $"Ошибка ввода: {ex.Message}", cancellationToken: ct);
                await _scenarioContextRepository.ResetContext(userId, ct);
                var registeredUser = await _userService.GetUser(userId, ct);
                ChangeKeyboard(registeredUser.TelegramUserId, _botClient);
            }
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            _update = update;

            long userId = 0;

            if (_update.Message == null)
            {
                userId = _update.CallbackQuery.From!.Id;

                var context = await _scenarioContextRepository.GetContext(userId, ct);

                if (context != null)
                {
                    await ProcessScenario(context, _update.Message, ct, _update.CallbackQuery);
                    return;
                }
            }
            else
            {
                userId = _update.Message.From!.Id;

                var context = await _scenarioContextRepository.GetContext(userId, ct);

                if (context != null)
                {
                    await ProcessScenario(context, _update.Message, ct, _update.CallbackQuery);
                    return;
                }
            }

            await (update switch
            {
                { Message: { } message } => OnMessage(update, message, ct),
                { CallbackQuery: { } callbackQuery } => OnCallbackQuery(update, callbackQuery, ct),
                _ => OnUnknown(update)
            });


        }

        private Task OnUnknown(Update update) => Task.CompletedTask;


        private async Task OnMessage(Update update, Message message, CancellationToken ct)
        {
            if (update.Message == null)
                return;

            try
            {
                var commands = new[]
                {
                    new BotCommand{ Command = "start", Description = "Авторизация"},
                    new BotCommand{ Command = "help", Description = "Помощь"},
                    new BotCommand{ Command = "info", Description = "Информация о релизе"},
                    new BotCommand{ Command = "exit", Description = "Выход из сессии"},
                    new BotCommand{ Command = "addtask", Description = "Добавить задачу"},
                    new BotCommand{ Command = "show", Description = "Вывести задачи"},
                    new BotCommand{ Command = "remowetask", Description = "Удалить задачу"},
                    new BotCommand{ Command = "completetask", Description = "Закрыть задачу"},
                    new BotCommand{ Command = "find", Description = "Поиск задачи по слову"},
                    new BotCommand{ Command = "report", Description = "Отчет статистики"},
                    new BotCommand{ Command = "cansel", Description = "выход из цикла добавления задачи"}
                };
                await _botClient.SetMyCommands(commands);

                string? text = _update.Message.Text;
                await Returne(text, ct);
            }
            catch (TaskCountLimitException ex)
            {
                await HandleErrorAsync(_botClient, ex, HandleErrorSource.HandleUpdateError, ct);
                await _botClient.SendMessage(_update.Message.Chat, ex.Message);
            }
            catch (TaskLengthLimitException ex)
            {
                await HandleErrorAsync(_botClient, ex, HandleErrorSource.HandleUpdateError, ct);
                await _botClient.SendMessage(_update.Message.Chat, ex.Message);
            }
            catch (DublicateTaskException ex)
            {
                await HandleErrorAsync(_botClient, ex, HandleErrorSource.HandleUpdateError, ct);
                await _botClient.SendMessage(_update.Message.Chat, ex.Message);
            }
            catch (ArgumentException ex)
            {
                await HandleErrorAsync(_botClient, ex, HandleErrorSource.HandleUpdateError, ct);
                await _botClient.SendMessage(_update.Message.Chat, ex.Message);
            }
        }







        /// <summary>
        /// Основной метод работы алгоритма
        /// </summary>
        async Task<bool> Returne(string? text, CancellationToken ct)
        {
            switch (text)
            {
                case "/start"://Обработка команды start
                case "Старт":
                    var user = _update.Message.From;
                    long userId = user.Id;
                    string? userName = user.Username;

                    if (await _userService.GetUser(userId, ct) == null)
                    {
                        user2 = await _userService.RegisterUser(userId, userName, ct);
                        name = user2.TelegramUserName;
                    }
                    else
                    {
                        user2 = await _userService.GetUser(userId, ct);
                        name = user2?.TelegramUserName;
                    }
                    await NameVerification("Не получилось определить имя чата", name, ct);
                    ChangeKeyboard(_update.Message.Chat.Id, _botClient);

                    return false;
                case "/help": //Обработка команды help
                    {
                        var registeredUser = await _userService.GetUser(_update.Message.From!.Id);
                        if (registeredUser == null)
                        {
                            await _botClient.SendMessage(_update.Message.Chat, "Требуется авторизация. Используйте /start");
                            return false;
                        }
                        user2 = registeredUser;
                        await NameVerification(Help, registeredUser.TelegramUserName, ct);
                        return false;
                    }
                case "/info": //Обработка команды info
                    {
                        var registeredUser = await _userService.GetUser(_update.Message.From!.Id);
                        if (registeredUser == null)
                        {
                            await _botClient.SendMessage(_update.Message.Chat, "Требуется авторизация. Используйте /start");
                            return false;
                        }
                        user2 = registeredUser;
                        await NameVerification(Info, registeredUser.TelegramUserName, ct);
                        return false;
                    }
                case "/exit":
                    {
                        var registeredUser = await _userService.GetUser(_update.Message.From!.Id);
                        if (registeredUser == null)
                        {
                            await _botClient.SendMessage(_update.Message.Chat, "Требуется авторизация. Используйте /start");
                            return false;
                        }
                        user2 = registeredUser;
                        SendMainMenuExit(_update.Message.Chat.Id, _botClient);
                        return true;
                    }
                case "/report":
                case "Отчет":
                    {
                        var registeredUser = await _userService.GetUser(_update.Message.From!.Id);
                        if (registeredUser == null)
                        {
                            await _botClient.SendMessage(_update.Message.Chat, "Требуется авторизация. Используйте /start");
                            return false;
                        }
                        user2 = registeredUser;
                        var toDoReportService = new ToDoReportService(_toDoRepository);
                        var report = await toDoReportService.GetUserStats(registeredUser.UserId);
                        await _botClient.SendMessage(_update.Message.Chat, $"Статистика по задачам на {report.generatedAt}. Всего: {report.total}; Завершено {report.completed}; Активных: {report.active};");
                        return false;
                    }
                case string command when command.StartsWith("/find"):
                    {
                        var registeredUser = await _userService.GetUser(_update.Message.From!.Id);
                        if (registeredUser == null)
                        {
                            await _botClient.SendMessage(_update.Message.Chat, "Требуется авторизация. Используйте /start");
                            return false;
                        }

                        List<string> partOne = new List<string>();
                        partOne.AddRange(command.Split(' ', 2)); //Разделение строки по пробелу после команды
                        partOne.Add(" ");
                        ValidateString(partOne[1]);
                        string task_2 = partOne[1].Trim(); //Используем только вторую часть команды
                        var taski = await _toDoService.Find(registeredUser, task_2, ct);

                        int i = 1;
                        if (taski != null)
                        {
                            await _botClient.SendMessage(_update.Message.Chat, "Ваш список задач:");
                            foreach (var tasks in taski)
                            {
                                await _botClient.SendMessage(_update.Message.Chat, $"Задача {i++}:{tasks.Name} - {tasks.CreateAt} - '{tasks.Id}'");
                            }
                        }
                        else
                        {
                            await _botClient.SendMessage(_update.Message.Chat, $"Список задач пуст!");
                        }

                        return false;
                    }

                case "/addtask":
                case "Добавить задачу":
                    {
                        var registeredUser = await _userService.GetUser(_update.Message.From!.Id);
                        if (registeredUser == null)
                        {
                            await _botClient.SendMessage(_update.Message.Chat, "Требуется авторизация. Используйте /start");
                            return false;
                        }
                        user2 = registeredUser;
                        var newContext = new ScenarioContext(ScenarioType.AddTask);

                        await _scenarioContextRepository.SetContext(_update.Message.From!.Id, newContext, ct);

                        await ProcessScenario(newContext, _update.Message, ct, _update.CallbackQuery);
                        var scenari = newContext.CurrentScenario;
                        ChangeKeyboardExid(_update.Message.Chat.Id, _botClient, scenari);
                        return false;
                    }
                case "/show":
                case "Задачи":
                    {
                        var registeredUser = await _userService.GetUser(_update.Message.From!.Id);
                        if (registeredUser == null)
                        {
                            await _botClient.SendMessage(_update.Message.Chat, "Требуется авторизация. Используйте /start");
                            return false;
                        }
                        user2 = registeredUser;
                        await HandleShowCommandAsync(ct);
                        return false;
                    }

                default: //если команды не распозднаны то выводим сообщение
                    await _botClient.SendMessage(_update.Message.Chat, "Команда не распознана");
                    return false;
            }
        }

        /// <summary>
        /// Метод который определяет авторизован пользователь и выводит преведственное сообщение
        /// </summary>
        async Task NameVerification(string massege, string? name, CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                await _botClient.SendMessage(_update.Message.Chat, $"Приветствую: {name}");
                await _botClient.SendMessage(_update.Message.Chat, massege);
            }
            else
            {
                await _botClient.SendMessage(_update.Message.Chat, massege);
            }
        }

        void ValidateString(string? str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentException("Строка не может быть пустой, null или содержать только пробелы.");
            }
        }


        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            Console.WriteLine($"HandleError: {exception})");
            return Task.CompletedTask;
        }


        private static async Task SendMainMenuExit(long chatId, ITelegramBotClient botClient)
        {
            var replyKeyboard = new ReplyKeyboardMarkup(new[]
            {
            new KeyboardButton("Старт"),
        })
            {
                ResizeKeyboard = true
            };

            await botClient.SendMessage(chatId, "Вы вышли из сессии!", replyMarkup: replyKeyboard);
        }


        private static async Task ChangeKeyboard(long chatId, ITelegramBotClient botClient)
        {
            var newKeyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton("Добавить задачу"),
                new KeyboardButton("Задачи"),
                new KeyboardButton("Отчет") // Добавим кнопку для возврата на основное меню
            })
            {
                ResizeKeyboard = true
            };

            await botClient.SendMessage(chatId, "Введите команды", replyMarkup: newKeyboard);
        }


        private static async Task ChangeKeyboardExid(long chatId, ITelegramBotClient botClient, ScenarioType scenarioType)
        {
            var newKeyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton("Назад") // Добавим кнопку для возврата на основное меню
            })
            {
                ResizeKeyboard = true
            };

            if (scenarioType == ScenarioType.AddTask)
                await botClient.SendMessage(chatId, "Введите название задачи", replyMarkup: newKeyboard);
            if(scenarioType == ScenarioType.AddList)
                await botClient.SendMessage(chatId, "Введите название списка:", replyMarkup: newKeyboard);
        }


        // Вызывается при получении команды /show или текста "Активные задачи"
        private async Task<bool> HandleShowCommandAsync(CancellationToken ct)
        {
            // Получаем списки пользователя 
            var lists = await _toDoListService.GetUserLists(user2.UserId, ct) ?? Array.Empty<ToDoList>();

            // Список рядов кнопок для InlineKeyboardMarkup
            var rows = new List<IEnumerable<InlineKeyboardButton>>();

            // Кнопка "📌Без списка" — Action = "show", ToDoListId = null
            var noListDto = new ToDoListCallbackDto { Action = "show", ToDoListId = null };
            rows.Add(new[] { InlineKeyboardButton.WithCallbackData("📌Без списка", noListDto.ToString()) });

            // Кнопки для каждого списка пользователя — Action = "show", ToDoListId = list.Id
            foreach (var l in lists)
            {
                var dto = new ToDoListCallbackDto { Action = "show", ToDoListId = l.Id };
                var callback = dto.ToString();

                // Если длина callbackData превышает 64 — используем короткий формат Guid без дефисов
                if (callback.Length > 64)
                    callback = $"{dto.Action}|{l.Id.ToString("N")}";

                rows.Add(new[] { InlineKeyboardButton.WithCallbackData(l.Name, callback) });
            }

            // Кнопки действий: добавить список и удалить список
            rows.Add(new[] { InlineKeyboardButton.WithCallbackData("🆕Добавить", "addlist") });
            rows.Add(new[] { InlineKeyboardButton.WithCallbackData("❌Удалить", "deletelist") });

            var markup = new InlineKeyboardMarkup(rows);

            // Отправляем сообщение с клавиатурой
            await _botClient.SendMessage(_update.Message.Chat.Id, "Выберите список", replyMarkup: markup, cancellationToken: ct);

            return false; // не завершает сценарий
        }

        private async Task OnCallbackQuery(Update update, CallbackQuery callback, CancellationToken ct)
        {
            async Task AnswerIfNeeded()
            {
                if (!string.IsNullOrEmpty(callback.Id))
                    await _botClient.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
            }

            if (callback.From == null)
            {
                await AnswerIfNeeded();
                return;
            }

            var registeredUser = await _userService.GetUser(callback.From.Id, ct);
            if (registeredUser == null)
            {
                await AnswerIfNeeded();
                return;
            }

            var data = callback.Data ?? string.Empty;



            // 🔹 1. Обработка действий с ЗАДАЧАМИ (showtask, completetask, deletetask)
            var itemDto = ToDoItemCallbackDto.FromString(data);

            // 🔹 Обработка кнопки "Назад"
            if (itemDto.Action == "back")
            {
                await AnswerIfNeeded();

                // Возвращаемся к списку задач (показываем все списки пользователя)
                var lists = await _toDoListService.GetUserLists(registeredUser.UserId, ct) ?? Array.Empty<ToDoList>();

                var rows = new List<IEnumerable<InlineKeyboardButton>>();

                // Кнопка "📌Без списка"
                var noListDto = new ToDoListCallbackDto { Action = "show", ToDoListId = null };
                rows.Add(new[] { InlineKeyboardButton.WithCallbackData("📌Без списка", noListDto.ToString()) });

                // Кнопки списков
                foreach (var l in lists)
                {
                    var dto = new ToDoListCallbackDto { Action = "show", ToDoListId = l.Id };
                    var cb = dto.ToString();
                    if (cb.Length > 64) cb = $"{dto.Action}|{l.Id.ToString("N")}";
                    rows.Add(new[] { InlineKeyboardButton.WithCallbackData(l.Name, cb) });
                }

                rows.Add(new[] { InlineKeyboardButton.WithCallbackData("🆕Добавить", "addlist") });
                rows.Add(new[] { InlineKeyboardButton.WithCallbackData("❌Удалить", "deletelist") });

                var markup = new InlineKeyboardMarkup(rows);

                if (callback.Message != null)
                    await _botClient.EditMessageText(callback.Message.Chat.Id, callback.Message.MessageId, "Выберите список", replyMarkup: markup, cancellationToken: ct);
                else
                    await _botClient.SendMessage(registeredUser.TelegramUserId, "Выберите список", replyMarkup: markup, cancellationToken: ct);

                return;
            }



            if (!string.IsNullOrEmpty(itemDto.Action) && itemDto.ToDoItemId.HasValue)
            {



                var task = await _toDoService.Get(itemDto.ToDoItemId.Value, ct);
                // ✅ showtask: показать задачу с кнопками действий
                if (itemDto.Action == "showtask")
                {
                    if (task == null || task.User?.UserId != registeredUser.UserId)
                    {
                        await AnswerIfNeeded();
                        await _botClient.SendMessage(registeredUser.TelegramUserId, "❌ Задача не найдена", cancellationToken: ct);
                        return;
                    }

                    string status = task.State == ToDoItemState.Completed ? "✅ Выполнена" : "⏳ Активна";
                    string text = $"📋 *{task.Name}*\n🆔 `{task.Id}`\n📅 {task.CreateAt:dd.MM.yyyy}\n🔄 {status}";

                    var rows = new List<IEnumerable<InlineKeyboardButton>>();

                    // Кнопка "✅ Выполнить" (только для активных задач)
                    if (task.State == ToDoItemState.Active)
                    {
                        var completeDto = new ToDoItemCallbackDto { Action = "completetask", ToDoItemId = task.Id };
                        var cb = SafeCallback(completeDto.ToString(), completeDto.Action, task.Id);
                        rows.Add(new[] { InlineKeyboardButton.WithCallbackData("✅ Выполнить", cb) });
                    }

                    // Кнопка "❌ Удалить"
                    var deleteDto = new ToDoItemCallbackDto { Action = "deletetask", ToDoItemId = task.Id };
                    var cbDel = SafeCallback(deleteDto.ToString(), deleteDto.Action, task.Id);
                    rows.Add(new[] { InlineKeyboardButton.WithCallbackData("❌ Удалить", cbDel) });

                    // Кнопка "◀️ Назад"
                    var backDto = new ToDoItemCallbackDto { Action = "back" };
                    rows.Add(new[] { InlineKeyboardButton.WithCallbackData("◀️ Назад", backDto.ToString()) });

                    var markup = new InlineKeyboardMarkup(rows);
                    if (callback.Message != null)
                        await _botClient.EditMessageText(callback.Message.Chat.Id, callback.Message.MessageId, text, replyMarkup: markup, parseMode: ParseMode.Markdown, cancellationToken: ct);
                    else
                        await _botClient.SendMessage(registeredUser.TelegramUserId, text, replyMarkup: markup, parseMode: ParseMode.Markdown, cancellationToken: ct);

                    await AnswerIfNeeded();
                    return;
                }

                // ✅ completetask: завершить задачу
                if (itemDto.Action == "completetask")
                {
                    await _toDoService.MarkCompleted(itemDto.ToDoItemId.Value, ct);
                    if (callback.Message != null)
                        await _botClient.EditMessageText(callback.Message.Chat.Id, callback.Message.MessageId, $"✅ *{task.Name}* завершена!", parseMode: ParseMode.Markdown, cancellationToken: ct);
                    else
                        await _botClient.SendMessage(registeredUser.TelegramUserId, $"✅ *{task.Name}* завершена!", parseMode: ParseMode.Markdown, cancellationToken: ct);
                    await AnswerIfNeeded();
                    return;
                }

                // ✅ deletetask: удалить задачу
                if (itemDto.Action == "deletetask")
                {
                    await _toDoService.Delete(itemDto.ToDoItemId.Value, ct);
                    if (callback.Message != null)
                        await _botClient.EditMessageText(callback.Message.Chat.Id, callback.Message.MessageId, $"🗑️ *{task.Name}* удалена!", parseMode: ParseMode.Markdown, cancellationToken: ct);
                    else
                        await _botClient.SendMessage(registeredUser.TelegramUserId, $"🗑️ *{task.Name}* удалена!", parseMode: ParseMode.Markdown, cancellationToken: ct);
                    await AnswerIfNeeded();
                    return;
                }
            }

            // 🔹 2. Обработка действий со СПИСКАМИ (show, addlist, deletelist)
            var listDto = ToDoListCallbackDto.FromString(data);

            if (listDto.Action == "show")
            {
                var items = await _toDoService.GetByUserIdAndList(registeredUser.UserId, listDto.ToDoListId, ct) ?? Array.Empty<ToDoItem>();
                string text = items.Count == 0
                    ? "📭 Список пуст!"
                    : "📋 Задачи:\n" + string.Join("\n", items.Select((t, i) => $"{i + 1}. {t.Name} - `{t.Id}`"));

                var rows = new List<IEnumerable<InlineKeyboardButton>>();
                foreach (var item in items)
                {
                    // ✅ КЛЮЧЕВОЕ: в callbackData пишем ToDoItemCallbackDto с Action = "showtask"
                    var dto = new ToDoItemCallbackDto { Action = "showtask", ToDoItemId = item.Id };
                    var cb = SafeCallback(dto.ToString(), dto.Action, item.Id);
                    rows.Add(new[] { InlineKeyboardButton.WithCallbackData(item.Name, cb) });
                }
                var backDto = new ToDoItemCallbackDto { Action = "back" };
                rows.Add(new[] { InlineKeyboardButton.WithCallbackData("◀️ Назад", backDto.ToString()) });

                var markup = new InlineKeyboardMarkup(rows);
                if (callback.Message != null)
                    await _botClient.EditMessageText(callback.Message.Chat.Id, callback.Message.MessageId, text, replyMarkup: markup, parseMode: ParseMode.Markdown, cancellationToken: ct);
                else
                    await _botClient.SendMessage(registeredUser.TelegramUserId, text, replyMarkup: markup, parseMode: ParseMode.Markdown, cancellationToken: ct);

                await AnswerIfNeeded();
                return;
            }

            if (data == "addlist")
            {
                var ctx = new ScenarioContext(ScenarioType.AddList);
                await _scenarioContextRepository.SetContext(registeredUser.TelegramUserId, ctx, ct);
                if (callback.Message != null)
                    await ProcessScenario(ctx, callback.Message, ct, callback);
                else
                    await _botClient.SendMessage(registeredUser.TelegramUserId, "Введите название списка:", cancellationToken: ct);
                ChangeKeyboardExid(registeredUser.TelegramUserId, _botClient, ctx.CurrentScenario);
                return;
            }

            if (listDto.Action == "deletelist")
            {
                var ctx = await _scenarioContextRepository.GetContext(registeredUser.TelegramUserId, ct);
                if (ctx == null || ctx.CurrentScenario != ScenarioType.DeleteList)
                {
                    ctx = new ScenarioContext(ScenarioType.DeleteList);
                    await _scenarioContextRepository.SetContext(registeredUser.TelegramUserId, ctx, ct);
                }
                if (callback.Message != null)
                    await ProcessScenario(ctx, callback.Message, ct, callback);
                await AnswerIfNeeded();
                return;
            }

            await AnswerIfNeeded();
        }

        // ✅ Вспомогательный метод для безопасного сокращения callback_data до 64 символов
        private string SafeCallback(string callback, string action, Guid id)
        {
            if (callback.Length <= 64) return callback;
            return $"{action}|{id.ToString("N")}"; // 32 символа без дефисов
        }


        string Help { get; set; } = "Просто вводи команды\n/start, /help, /info, /exit.\nЕсли авторизовался, то вводи команду /addtask, /show (вывод списка задач), /remowetask (фрмат ввода '№ задачи'), /completetask (фрмат ввода 'команда id задачи'), /find (вводи часть задачи и получай список задач начинающийся на данное слово), /report (вывод статистики), /cansel (выход из цикла добавления задачи)\nУдачи!!!!!";
            string Info { get; set; } = "Версия: 2\nДата создания: 14.11.2025\nДата обновления: 30.03.2026";

        }
    }

