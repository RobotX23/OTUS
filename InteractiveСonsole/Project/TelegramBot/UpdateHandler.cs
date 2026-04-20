using InteractiveСonsole.Project.Core.Exceptions;
using InteractiveСonsole.Project.Core.Helpers;
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
        private static int _pageSize = 5;
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


        private async Task<bool> HandleShowCommandAsync(CancellationToken ct)
        {
            var lists = await _toDoListService.GetUserLists(user2.UserId, ct) ?? Array.Empty<ToDoList>();
            var rows = new List<IEnumerable<InlineKeyboardButton>>();

            var noListDto = new ToDoListCallbackDto { Action = "show", ToDoListId = null };
            rows.Add(new[] { InlineKeyboardButton.WithCallbackData("📌Без списка", noListDto.ToString()) });

            foreach (var l in lists)
            {
                var dto = new ToDoListCallbackDto { Action = "show", ToDoListId = l.Id };
                var cb = dto.ToString();
                if (cb.Length > 64) cb = $"show|{l.Id.ToString("N")}";
                rows.Add(new[] { InlineKeyboardButton.WithCallbackData(l.Name, cb) });
            }

            rows.Add(new[] { InlineKeyboardButton.WithCallbackData("🆕Добавить", "addlist") });
            rows.Add(new[] { InlineKeyboardButton.WithCallbackData("❌Удалить", "deletelist") });

            var markup = new InlineKeyboardMarkup(rows);

            // ✅ Для текстовой команды используем _update.Message (оно не null)
            await _botClient.SendMessage(_update.Message.Chat.Id, "Выберите список", replyMarkup: markup, cancellationToken: ct);

            return false;
        }

        private async Task OnCallbackQuery(Update update, CallbackQuery callback, CancellationToken ct)
        {
            async Task AnswerIfNeeded()
            {
                if (!string.IsNullOrEmpty(callback.Id))
                    await _botClient.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
            }

            if (callback.From == null) { await AnswerIfNeeded(); return; }

            var registeredUser = await _userService.GetUser(callback.From.Id, ct);
            if (registeredUser == null) { await AnswerIfNeeded(); return; }

            var data = callback.Data ?? string.Empty;

            // 🔹 1. Обработка кнопки "Назад к спискам"
            if (data == "back|lists")
            {
                await AnswerIfNeeded();
                await ShowListsMenuAsync(registeredUser, callback, ct);
                return;
            }



            // 🔹 3. Обработка показа задач СПИСКА с пагинацией
            var listDto = PagedListCallbackDto.FromString(data);
            if (listDto.Action == "show")
            {
                var items = await _toDoService.GetByUserIdAndList(registeredUser.UserId, listDto.ToDoListId, ct) ?? Array.Empty<ToDoItem>();

                var itemsActev = items.Where(x => x.State == ToDoItemState.Active);


                // Формируем кнопки задач
                var taskButtons = new List<KeyValuePair<string, string>>();
                foreach (var item in itemsActev)
                {
                    var taskDto = new ToDoItemCallbackDto { Action = "showtask", ToDoItemId = item.Id };
                    var cb = SafeCallback(taskDto.Action, item.Id);
                    taskButtons.Add(new KeyValuePair<string, string>(item.Name, cb));
                }

                string text = itemsActev.Count() == 0
                    ? "📭 Список пуст!"
                    : $"📋 Задачи (стр. {listDto.Page + 1}):";

                var markup = BuildPagedButtons(taskButtons.AsReadOnly(), listDto);

                if (callback.Message != null)
                    await _botClient.EditMessageText(
                        callback.Message.Chat.Id,
                        callback.Message.MessageId,
                        text,
                        replyMarkup: markup,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: ct);
                else
                    await _botClient.SendMessage(
                        registeredUser.TelegramUserId,
                        text,
                        replyMarkup: markup,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: ct);

                await AnswerIfNeeded();
                return;
            }

            if (listDto.Action == "show_completed")
            {
                var items = await _toDoService.GetByUserIdAndList(registeredUser.UserId, listDto.ToDoListId, ct) ?? Array.Empty<ToDoItem>();

                var itemsActev = items.Where(x => x.State == ToDoItemState.Completed);


                // Формируем кнопки задач
                var taskButtons = new List<KeyValuePair<string, string>>();
                foreach (var item in itemsActev)
                {
                    var taskDto = new ToDoItemCallbackDto { Action = "showtask", ToDoItemId = item.Id };
                    var cb = SafeCallback(taskDto.Action, item.Id);
                    taskButtons.Add(new KeyValuePair<string, string>(item.Name, cb));
                }

                string text = itemsActev.Count() == 0
                    ? "📭 Список пуст!"
                    : $"✅ Выполненные (стр. {listDto.Page + 1}):";

                var markup = BuildPagedButtons(taskButtons.AsReadOnly(), listDto);

                if (callback.Message != null)
                    await _botClient.EditMessageText(
                        callback.Message.Chat.Id,
                        callback.Message.MessageId,
                        text,
                        replyMarkup: markup,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: ct);
                else
                    await _botClient.SendMessage(
                        registeredUser.TelegramUserId,
                        text,
                        replyMarkup: markup,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: ct);

                await AnswerIfNeeded();
                return;
            }








            // 🔹 2. Обработка действий с ЗАДАЧАМИ (showtask, completetask, deletetask)
            var itemDto = ToDoItemCallbackDto.FromString(data);
            if (!string.IsNullOrEmpty(itemDto.Action) && itemDto.ToDoItemId.HasValue)
            {
                var task = await _toDoService.Get(itemDto.ToDoItemId.Value, ct);

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
                    if (task.State == ToDoItemState.Active)
                    {
                        var cb = SafeCallback("completetask", task.Id);
                        rows.Add(new[] { InlineKeyboardButton.WithCallbackData("✅ Выполнить", cb) });
                    }
                    var cbDel = SafeCallback("deletetask", task.Id);
                    rows.Add(new[] { InlineKeyboardButton.WithCallbackData("❌ Удалить", cbDel) });
                    rows.Add(new[] { InlineKeyboardButton.WithCallbackData("◀️ Назад", "back|lists") });

                    var markup = new InlineKeyboardMarkup(rows);
                    if (callback.Message != null)
                        await _botClient.EditMessageText(callback.Message.Chat.Id, callback.Message.MessageId, text, replyMarkup: markup, parseMode: ParseMode.Markdown, cancellationToken: ct);
                    else
                        await _botClient.SendMessage(registeredUser.TelegramUserId, text, replyMarkup: markup, parseMode: ParseMode.Markdown, cancellationToken: ct);
                    await AnswerIfNeeded();
                    return;
                }

                if (itemDto.Action == "completetask")
                {
                    await _toDoService.MarkCompleted(itemDto.ToDoItemId.Value, ct);
                    await AnswerIfNeeded();
                    string msg = $"✅ *{task.Name}* завершена!";
                    if (callback.Message != null)
                        await _botClient.EditMessageText(callback.Message.Chat.Id, callback.Message.MessageId, msg, parseMode: ParseMode.Markdown, cancellationToken: ct);
                    else
                        await _botClient.SendMessage(registeredUser.TelegramUserId, msg, parseMode: ParseMode.Markdown, cancellationToken: ct);
                    return;
                }

                if (itemDto.Action == "deletetask")
                {
                    // Создаем контекст для сценария удаления
                    var ctx = new ScenarioContext(ScenarioType.DeleteTask);
                    ctx.Data["taskId"] = itemDto.ToDoItemId.Value; // Передаем ID задачи в сценарий

                    await _scenarioContextRepository.SetContext(registeredUser.TelegramUserId, ctx, ct);

                    // Запускаем сценарий (он покажет запрос подтверждения)
                    if (callback.Message != null)
                        await ProcessScenario(ctx, callback.Message, ct, callback);
                    else
                        await ProcessScenario(ctx, null, ct, callback); // Если message null, передаем null

                    await AnswerIfNeeded();
                    return;

                }



            }



                // 🔹 4. Обработка addlist / deletelist
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

                if (data == "deletelist")
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
        


        /// <summary>
        /// Показывает меню выбора списков (без пагинации)
        /// </summary>
        private async Task ShowListsMenuAsync(ToDoUser registeredUser, CallbackQuery callback, CancellationToken ct)
        {
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
                if (cb.Length > 64) cb = $"show|{l.Id.ToString("N")}";
                rows.Add(new[] { InlineKeyboardButton.WithCallbackData(l.Name, cb) });
            }

            // Системные кнопки
            rows.Add(new[] { InlineKeyboardButton.WithCallbackData("🆕Добавить", "addlist") });
            rows.Add(new[] { InlineKeyboardButton.WithCallbackData("❌Удалить", "deletelist") });

            var markup = new InlineKeyboardMarkup(rows);

            // ✅ Используем callback.Message, а не _update.Message
            if (callback.Message != null)
                await _botClient.EditMessageText(
                    callback.Message.Chat.Id,
                    callback.Message.MessageId,
                    "Выберите список",
                    replyMarkup: markup,
                    cancellationToken: ct);
            else
                await _botClient.SendMessage(
                    registeredUser.TelegramUserId,
                    "Выберите список",
                    replyMarkup: markup,
                    cancellationToken: ct);
        }



        // Вспомогательный метод для сокращения callback_data до 64 символов
        private string SafeCallback(string action, Guid id)
        {
            var full = $"{action}|{id}";
            return full.Length <= 64 ? full : $"{action}|{id.ToString("N")}";
        }

        private InlineKeyboardMarkup BuildPagedButtons(
            IReadOnlyList<KeyValuePair<string, string>> taskButtons,
            PagedListCallbackDto listDto,
            bool showCompletedButton = true) // ← новый параметр
        {
            int totalPages = taskButtons.Count == 0 ? 1 : (int)Math.Ceiling(taskButtons.Count / (double)_pageSize);
            var pageItems = taskButtons.GetBatchByNumber(_pageSize, listDto.Page).ToList();

            var rows = new List<IEnumerable<InlineKeyboardButton>>();

            // Кнопки задач текущей страницы
            foreach (var item in pageItems)
                rows.Add(new[] { InlineKeyboardButton.WithCallbackData(item.Key, item.Value) });

            // Навигация
            var navRow = new List<InlineKeyboardButton>();

            // Кнопка "◀️ К спискам"
            navRow.Add(InlineKeyboardButton.WithCallbackData("◀️ К спискам", "back|lists"));

            // ✅ Кнопка "☑️ Выполненные" (только для активных задач)
            if (showCompletedButton && listDto.Action == "show")
            {
                var completedDto = new PagedListCallbackDto("show_completed", listDto.ToDoListId, 0);
                navRow.Add(InlineKeyboardButton.WithCallbackData("☑️ Выполненные", completedDto.ToString()));
            }

            // ✅ Кнопка "📋 Активные" (только для выполненных задач)
            if (showCompletedButton && listDto.Action == "show_completed")
            {
                var activeDto = new PagedListCallbackDto("show", listDto.ToDoListId, 0);
                navRow.Add(InlineKeyboardButton.WithCallbackData("📋 Активные", activeDto.ToString()));
            }

            // Стрелки пагинации
            if (listDto.Page > 0)
            {
                var prev = new PagedListCallbackDto(listDto.Action, listDto.ToDoListId, listDto.Page - 1);
                navRow.Add(InlineKeyboardButton.WithCallbackData("⬅️", prev.ToString()));
            }
            if (listDto.Page < totalPages - 1)
            {
                var next = new PagedListCallbackDto(listDto.Action, listDto.ToDoListId, listDto.Page + 1);
                navRow.Add(InlineKeyboardButton.WithCallbackData("➡️", next.ToString()));
            }

            if (navRow.Count > 0)
                rows.Add(navRow);

            return new InlineKeyboardMarkup(rows);
        }



        string Help { get; set; } = "Просто вводи команды\n/start, /help, /info, /exit.\nЕсли авторизовался, то вводи команду /addtask, /show (вывод списка задач), /find (вводи часть задачи и получай список задач начинающийся на данное слово), /report (вывод статистики), /cansel (выход из цикла добавления задачи)\nУдачи!!!!!";
            string Info { get; set; } = "Версия: 4\nДата создания: 14.11.2025\nДата обновления: 19.04.2026";

        }
    }

