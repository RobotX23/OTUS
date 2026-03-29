using InteractiveСonsole.Project.Core.Exceptions;
using InteractiveСonsole.Project.TelegramBot.Scenarios;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
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
        private readonly IToDoRepository _toDoRepository;
        private readonly IEnumerable<IScenario> _scenarios;
        private readonly IScenarioContextRepository _scenarioContextRepository;

        public UpdateHandler(IToDoService toDoService, IUserService userService, IToDoRepository toDoRepository, ITelegramBotClient botClient, IEnumerable<IScenario> scenarios, IScenarioContextRepository scenarioContextRepository)
        {
            _toDoService = toDoService ?? throw new ArgumentNullException(nameof(toDoService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _toDoRepository = toDoRepository ?? throw new ArgumentNullException(nameof(toDoRepository));
            _botClient = botClient;

            _scenarios = scenarios;
            _scenarioContextRepository = scenarioContextRepository;
        }


        private IScenario GetScenario(ScenarioType scenario)
        {
            var result = _scenarios.FirstOrDefault(x => x.CanHandle(scenario));
            if ( result == null)
            {
                throw new Exception($"Сценарий {scenario} не найден!");
            }
            return result;
        }



        private async Task ProcessScenario(ScenarioContext context, Message message, CancellationToken ct)
        {
            var scenario = GetScenario(context.CurrentScenario);

            var result = await scenario.HandleMessageAsync(_botClient, context, message, ct);

            if (result == ScenarioResult.Completed)
            {
                await _scenarioContextRepository.ResetContext(message.From!.Id, ct);
                ChangeKeyboard(_update.Message.Chat.Id, _botClient);
            }
            else
            {
                await _scenarioContextRepository.SetContext(message.From!.Id,context, ct);
            }

        }

        bool flag = false;
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            _update = update;


            string? textEdit = _update.Message.Text;

            if (textEdit == "/cancel" || textEdit == "Назад")
            {
                await _scenarioContextRepository.ResetContext(_update.Message.From!.Id, ct);
                ChangeKeyboard(_update.Message.Chat.Id, _botClient);
                return;
            }


            if (update.Message == null)
                return;

            var userId = _update.Message.From!.Id;

            var context = await _scenarioContextRepository.GetContext(userId, ct);

  

            if (context != null)
            {
                await ProcessScenario(context, _update.Message, ct);
                return;
            }

            while (true)
            {

                try
                {
                    var taskline = await _toDoService.LineTasks();
                    int maxtasks = taskline.Item1;
                    int maxline = taskline.Item2;
                    if (maxtasks == 0)
                    {
                        if (flag == false)
                        {
                            await _botClient.SendMessage(_update.Message.Chat, "Введите максимальное допустимое количество задач: ");
                            flag = true;
                            break;
                        }
                        flag = false;
                        maxtasks = ParseAndValidatelnt(_update.Message.Text, 1, 100);
                        await _botClient.SendMessage(_update.Message.Chat, $"Вы ввели: {maxtasks} количество задач.");
                        _toDoService.maxtasks = maxtasks;
                    }

                    if (maxline == 0)
                    {
                        if (flag == false)
                        {
                            await _botClient.SendMessage(_update.Message.Chat, "Введите максимальную длинну задач: ");
                            flag = true;
                            break;
                        }
                        flag = false;
                        maxline = ParseAndValidatelnt(_update.Message.Text, 1, 100);
                        await _botClient.SendMessage(_update.Message.Chat, $"Вы введи: {maxline} длинну задачи.");
                        _toDoService.maxline = maxline;
                        if (name == null)
                        {
                            if (flag == false)
                            {
                                SendMainMenu(_update.Message.Chat.Id, _botClient);
                                flag = true;
                                break;
                            }
                            flag = false;

                        }
                    }

                    var commands = new[]
{
                                new BotCommand{ Command = "start", Description = "Авторизация"},
                                new BotCommand{ Command = "help", Description = "Помощь"},
                                new BotCommand{ Command = "info", Description = "Информация о релизе"},
                                new BotCommand{ Command = "exit", Description = "Выход из сессии"},
                                new BotCommand{ Command = "addtask", Description = "Добавить задачу"},
                                new BotCommand{ Command = "showtasks", Description = "Вывести активные задачи"},
                                new BotCommand{ Command = "remowetask", Description = "Удалить задачу"},
                                new BotCommand{ Command = "completetask", Description = "Закрыть задачу"},
                                new BotCommand{ Command = "showalltasks", Description = "Вывести все задачи"},
                                new BotCommand{ Command = "find", Description = "Поиск задачи по слову"},
                                new BotCommand{ Command = "report", Description = "Отчет статистики"},
                                new BotCommand{ Command = "cansel", Description = "выход из цикла добавления задачи"}

                            };
                    await _botClient.SetMyCommands(commands);





                    string? text = _update.Message.Text;
                    if (await Returne(text, ct))
                    {
                        name = null;
                        user2 = null;
                        break;
                    }
                    else
                    {
                        break;
                    }




                }
                catch (TaskCountLimitException ex)
                {
                    await HandleErrorAsync(_botClient, ex, HandleErrorSource.HandleUpdateError, ct);
                    await _botClient.SendMessage(_update.Message.Chat, ex.Message);
                    break;

                }
                catch (TaskLengthLimitException ex)
                {
                    await HandleErrorAsync(_botClient, ex, HandleErrorSource.HandleUpdateError, ct);
                    await _botClient.SendMessage(_update.Message.Chat, ex.Message);
                    break;
                }
                catch (DublicateTaskException ex)
                {
                    await HandleErrorAsync(_botClient, ex, HandleErrorSource.HandleUpdateError, ct);
                    await _botClient.SendMessage(_update.Message.Chat, ex.Message);
                    break;
                }
                catch (FormatException)
                {
                    Exception myEx = new Exception("Ошибка: вы ввели не корректное число.");
                    await HandleErrorAsync(_botClient, myEx, HandleErrorSource.HandleUpdateError, ct);
                    await _botClient.SendMessage(_update.Message.Chat, "Ошибка: вы ввели не корректное число.");
                    flag = true;
                    break;
                }
                catch (ArgumentException ex)
                {
                    await HandleErrorAsync(_botClient, ex, HandleErrorSource.HandleUpdateError, ct);
                    await _botClient.SendMessage(_update.Message.Chat, ex.Message);
                    break;
                }
                
            }
        }
            
            
        
        int ParseAndValidatelnt(string? str, int min, int max)
        {
            if (!int.TryParse(str, out int result))
            {
                throw new FormatException("Ошибка: вы ввели некорректное число.");
            }

            if (result < min || result > max)
            {
                throw new ArgumentException("Количество задач должно быть от 1 до 100.");
            }
            return result;
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

                    if(await _userService.GetUser(userId) == null)
                    {
                        user2 = await _userService.RegisterUser(userId, userName);
                        name =  user2.TelegramUserName;
                    }
                    else
                    {
                        user2 = await _userService.GetUser(userId);
                        name = user2?.TelegramUserName;
                    }
                    await NameVerification("Не получилось определить имя чата", name, ct);
                    ChangeKeyboard(_update.Message.Chat.Id, _botClient);

                    return false;
                case "/help": //Обработка команды help
                    await NameVerification(Help, name, ct);
                    return false;
                case "/info": //Обработка команды info
                    await NameVerification(Info, name, ct);
                    return false;
                case "/exit":
                    SendMainMenuExit(_update.Message.Chat.Id, _botClient);
                    return true; //Обработка команды exid
                case "/report":
                case "Отчет":
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        await _botClient.SendMessage(_update.Message.Chat, "Команда не распознана");
                        return false;
                    }
                    else 
                    {

                        var toDoReportService = new ToDoReportService(_toDoRepository);
                        var report = await toDoReportService.GetUserStats(user2.UserId);
                        await _botClient.SendMessage(_update.Message.Chat, $"Статистика по задачам на {report.generatedAt}. Всего: {report.total}; Завершено {report.completed}; Активных: {report.active};");
                        return false;
                    }
                case string command when command.StartsWith("/find"):
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        await _botClient.SendMessage(_update.Message.Chat, "Команда не распознана");
                        return false;
                    }
                    else
                    {
                        List<string> partOne = new List<string>();
                        partOne.AddRange(command.Split(' ', 2)); //Разделение строки по пробелу после команды
                        partOne.Add(" ");
                        ValidateString(partOne[1]);
                        string task_2 = partOne[1].Trim(); //Используем только вторую часть команды
                        var taski = await _toDoService.Find(user2, task_2);

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

                    var newContext = new ScenarioContext(ScenarioType.AddTask);

                    await _scenarioContextRepository.SetContext(_update.Message.From!.Id, newContext, ct);

                    await ProcessScenario(newContext, _update.Message, ct);
                    ChangeKeyboardExid(_update.Message.Chat.Id, _botClient);
                    return false;
                case "/showtasks":
                case "Активные задачи":
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        await _botClient.SendMessage(_update.Message.Chat, "Команда не распознана");
                        return false;
                    }
                    else
                    {
                        var taski = await _toDoService.GetActiveByUserId(user2.UserId); // Вызов переданного метода

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
                case string command when command.StartsWith("/remowetask"):
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        await _botClient.SendMessage(_update.Message.Chat, "Команда не распознана");
                        return false;
                    }
                    else
                    {
                        List<string> parts = new List<string>();
                        parts.AddRange(command.Split(' ', 2)); //Разделение строки по пробелу после команды
                        parts.Add(" ");
                        ValidateString(parts[1]);
                        string number = parts[1].Trim(); //Используем только вторую часть команды

                        var taskess = await _toDoService.GetAllByUserId(user2.UserId);


                        int numberr;
                        if (int.TryParse(number, out numberr))
                        {
                            if (numberr >= 1 && numberr <= taskess.Count)
                            {
                                var scan_task = taskess[Convert.ToInt32(number) - 1];
                                await _toDoService.Delete(scan_task.Id); // Вызов переданного метода
                                await _botClient.SendMessage(_update.Message.Chat, $"Задача - {scan_task.Name} удалена!");
                            }
                            else
                            {
                                await _botClient.SendMessage(_update.Message.Chat, "Ошибка: введено не корректнок число.");
                            }
                        }
                        else
                        {
                            await _botClient.SendMessage(_update.Message.Chat, "Ошибка: введено не число.");
                        }

                        return false;
                    }
            
                case string command when command.StartsWith("/completetask"):
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        await _botClient.SendMessage(_update.Message.Chat, "Команда не распознана");
                        return false;
                    }
                    else
                    {
                        List<string> parts = new List<string>();
                        parts.AddRange(command.Split(' ', 2)); //Разделение строки по пробелу после команды
                        parts.Add(" ");
                        ValidateString(parts[1]);
                        Guid id = Guid.Parse(parts[1].Trim()); //Используем только вторую часть команды
                        await _toDoService.MarkCompleted(id); // Вызов переданного метода
                        await _botClient.SendMessage(_update.Message.Chat, $"Задача - {parts[1].Trim()} завершена!");
                        return false;
                    }
                case "/showalltasks":
                case "Все задачи":
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        await _botClient.SendMessage(_update.Message.Chat, "Команда не распознана");
                        return false;
                    }
                    else
                    {
                        var taski = await _toDoService.GetAllByUserId(user2.UserId); // Вызов переданного метода

                        int i = 1;
                        if (taski != null)
                        {
                            await _botClient.SendMessage(_update.Message.Chat, "Ваш список задач:");
                            foreach (var tasks in taski)
                            {
                                await _botClient.SendMessage(_update.Message.Chat, $"Задача {i++}:({tasks.State}) {tasks.Name} - {tasks.CreateAt} - '{tasks.Id}'");
                            }
                        }
                        else
                        {
                            await _botClient.SendMessage(_update.Message.Chat, $"Список задач пуст!");
                        }

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



        private static async Task SendMainMenu(long chatId, ITelegramBotClient botClient)
        {
            var replyKeyboard = new ReplyKeyboardMarkup(new[]
            {
            new KeyboardButton("Старт"),
        })
            {
                ResizeKeyboard = true
            };

            await botClient.SendMessage(chatId, "\"Привет!\"", replyMarkup: replyKeyboard);
            await botClient.SendMessage(chatId, "Введи следующие команды /start, /help, /info, /exit.", replyMarkup: replyKeyboard);
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
                new KeyboardButton("Все задачи"),
                new KeyboardButton("Активные задачи"),
                new KeyboardButton("Отчет") // Добавим кнопку для возврата на основное меню
            })
            {
                ResizeKeyboard = true
            };

            await botClient.SendMessage(chatId, "Введите команды", replyMarkup: newKeyboard);
        }


        private static async Task ChangeKeyboardExid(long chatId, ITelegramBotClient botClient)
        {
            var newKeyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton("Назад") // Добавим кнопку для возврата на основное меню
            })
            {
                ResizeKeyboard = true
            };

            await botClient.SendMessage(chatId, "Введите название задачи", replyMarkup: newKeyboard);
        }


        string Help { get; set; } = "Просто вводи команды\n/start, /help, /info, /exit.\nЕсли авторизовался, то вводи команду /addtask, /showtasks, /remowetask (фрмат ввода '№ задачи'), /completetask (фрмат ввода 'команда id задачи'), /showalltasks, /find (вводи часть задачи и получай список задач начинающийся на данное слово), /report (вывод статистики), /cansel (выход из цикла добавления задачи)\nУдачи!!!!!";
        string Info { get; set; } = "Версия: 2\nДата создания: 14.11.2025\nДата обновления: 08.03.2026";

    }
}
