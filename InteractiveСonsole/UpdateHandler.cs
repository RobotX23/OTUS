using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;


namespace InteractiveСonsole
{
    internal class UpdateHandler : IUpdateHandler
    {

        string? name = null;
        InMemoryUserRepository userRepository = new InMemoryUserRepository();
        InMemoryToDoRepository toDoRepository = new InMemoryToDoRepository();
        UserService users;
        ToDoService toDoService;
        IToDoReportService toDoReportService;

        List<ToDoItem> taskes = new List<ToDoItem>();
        ToDoUser user2 = null;

        ITelegramBotClient _botClient;
        Update _update;



        int maxtasks = 0;
        int maxline = 0;
        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            users = new UserService(userRepository);
            toDoService = new ToDoService(toDoRepository);
            _botClient = botClient;
            _update = update;


            while (true)
            {
                try
                {
                    if (maxtasks == 0)
                    {
                        _botClient.SendMessage(_update.Message.Chat, "Введите максимальное допустимое количество задач: ");
                        string? imput = Console.ReadLine();
                        maxtasks = ParseAndValidatelnt(imput, 1, 100);
                        _botClient.SendMessage(_update.Message.Chat, $"Вы ввели: {maxtasks} количество задач.");
                        toDoService.maxtasks = maxtasks;
                    }

                    if (maxline == 0)
                    {
                        _botClient.SendMessage(_update.Message.Chat, "Введите максимальную длинну задач: ");
                        string? imput_text = Console.ReadLine();
                        maxline = ParseAndValidatelnt(imput_text, 1, 100);
                        _botClient.SendMessage(_update.Message.Chat, $"Вы введи: {maxline} длинну задачи.");
                        toDoService.maxline = maxline;
                        if (name == null)
                            _botClient.SendMessage(_update.Message.Chat, "Привет!\nВведи следующие команды\n/start, /help, /info, /exit.");
                    }

                    if (Returne(Console.ReadLine()))
                    {
                        break;
                    }

                }
                catch (TaskCountLimitException ex)
                {
                    _botClient.SendMessage(_update.Message.Chat, ex.Message);
                }
                catch (TaskLengthLimitException ex)
                {
                    _botClient.SendMessage(_update.Message.Chat, ex.Message);
                }
                catch (DublicateTaskException ex)
                {
                    _botClient.SendMessage(_update.Message.Chat, ex.Message);
                }
                catch (FormatException)
                {
                    _botClient.SendMessage(_update.Message.Chat, "Ошибка: вы ввели не корректное число.");
                }
                catch (ArgumentException ex)
                {
                    _botClient.SendMessage(_update.Message.Chat, ex.Message);
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
        bool Returne(string? text)
        {
            switch (text)
            {
                case "/start"://Обработка команды start

                    var user = _update.Message.From;
                    long userId = user.Id;
                    string userName = user.Username;

                    if(users.GetUser(userId) == null)
                    {
                        user2 = users.RegisterUser(userId, userName);
                        name =  user2.TelegramUserName;
                    }
                    else
                    {
                        user2 = users.GetUser(userId);
                        name = user2.TelegramUserName;
                    }
                    NameVerification("Не получилось определить имя чата", name);

                        return false;
                case "/help": //Обработка команды help
                    NameVerification(Help, name);
                    return false;
                case "/info": //Обработка команды info
                    NameVerification(Info, name);
                    return false;
                case "/exit":
                    return true; //Обработка команды exid
                case "/report":
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        _botClient.SendMessage(_update.Message.Chat, "Команда не распознана");
                        return false;
                    }
                    else 
                    {
                        toDoReportService = new ToDoReportService(toDoRepository);
                        var report = toDoReportService.GetUserStats(user2.UserId);
                        _botClient.SendMessage(_update.Message.Chat, $"Статистика по задачам на {report.generatedAt}. Всего: {report.total}; Завершено {report.completed}; Активных: {report.active};");
                        return false;
                    }
                case string command when command.StartsWith("/addtask"):
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        _botClient.SendMessage(_update.Message.Chat, "Команда не распознана");
                        return false;
                    }
                    else
                    {
                        List<string> parts_1 = new List<string>();
                        parts_1.AddRange(command.Split(' ', 2)); //Разделение строки по пробелу после команды
                        parts_1.Add(" ");
                        ValidateString(parts_1[1]);
                        string task_2 = parts_1[1].Trim(); //Используем только вторую часть команды
                        var task_1 =toDoService.Add(user2, task_2); // Вызов переданного метода
                        _botClient.SendMessage(_update.Message.Chat, $"Задача \"{task_1.Name}\" успешно добавлена");
                        return false;
                    }
                case "/showtasks":
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        _botClient.SendMessage(_update.Message.Chat, "Команда не распознана");
                        return false;
                    }
                    else
                    {
                        var taski = toDoService.GetActiveByUserId(user2.UserId); // Вызов переданного метода

                        int i = 1;
                        if (taski != null)
                        {
                            _botClient.SendMessage(_update.Message.Chat, "Ваш список задач:");
                            foreach (var tasks in taski)
                            {
                                _botClient.SendMessage(_update.Message.Chat, $"Задача {i++}:{tasks.Name} - {tasks.CreateAt} - {tasks.Id}");
                            }
                        }
                        else
                        {
                            _botClient.SendMessage(_update.Message.Chat, $"Список задач пуст!");
                        }

                        return false;
                    }
                case string command when command.StartsWith("/remowetask"):
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        _botClient.SendMessage(_update.Message.Chat, "Команда не распознана");
                        return false;
                    }
                    else
                    {
                        List<string> parts = new List<string>();
                        parts.AddRange(command.Split(' ', 2)); //Разделение строки по пробелу после команды
                        parts.Add(" ");
                        ValidateString(parts[1]);
                        string number = parts[1].Trim(); //Используем только вторую часть команды

                        var taskess = toDoService.GetAllByUserId(user2.UserId);


                        int numberr;
                        if (int.TryParse(number, out numberr))
                        {
                            if (numberr >= 1 && numberr <= taskess.Count)
                            {
                                var scan_task = taskess[Convert.ToInt32(number) - 1];
                                toDoService.Delete(scan_task.Id); // Вызов переданного метода
                                _botClient.SendMessage(_update.Message.Chat, $"Задача - {scan_task.Name} удалена!");
                            }
                            else
                            {
                                _botClient.SendMessage(_update.Message.Chat, "Ошибка: введено не корректнок число.");
                            }
                        }
                        else
                        {
                            _botClient.SendMessage(_update.Message.Chat, "Ошибка: введено не число.");
                        }

                        return false;
                    }
            
                case string command when command.StartsWith("/completetask"):
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        _botClient.SendMessage(_update.Message.Chat, "Команда не распознана");
                        return false;
                    }
                    else
                    {
                        List<string> parts = new List<string>();
                        parts.AddRange(command.Split(' ', 2)); //Разделение строки по пробелу после команды
                        parts.Add(" ");
                        ValidateString(parts[1]);
                        Guid id = Guid.Parse(parts[1].Trim()); //Используем только вторую часть команды
                        toDoService.MarkCompleted(id); // Вызов переданного метода
                        _botClient.SendMessage(_update.Message.Chat, $"Задача - {parts[1].Trim()} завершена!");
                        return false;
                    }
                case "/showalltasks":
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        _botClient.SendMessage(_update.Message.Chat, "Команда не распознана");
                        return false;
                    }
                    else
                    {
                        var taski = toDoService.GetAllByUserId(user2.UserId); // Вызов переданного метода

                        int i = 1;
                        if (taski != null)
                        {
                            _botClient.SendMessage(_update.Message.Chat, "Ваш список задач:");
                            foreach (var tasks in taski)
                            {
                                _botClient.SendMessage(_update.Message.Chat, $"Задача {i++}:({tasks.State}) {tasks.Name} - {tasks.CreateAt} - {tasks.Id}");
                            }
                        }
                        else
                        {
                            _botClient.SendMessage(_update.Message.Chat, $"Список задач пуст!");
                        }

                        return false;
                    }
                default: //если команды не распозднаны то выводим сообщение
                    _botClient.SendMessage(_update.Message.Chat, "Команда не распознана");
                    return false;
            }
        }

        /// <summary>
        /// Метод который определяет авторизован пользователь и выводит преведственное сообщение
        /// </summary>
        void NameVerification(string massege, string? name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _botClient.SendMessage(_update.Message.Chat, $"Приветствую: {name}");
                _botClient.SendMessage(_update.Message.Chat, massege);
            }
            else
            {
                _botClient.SendMessage(_update.Message.Chat, massege);
            }
        }

        void ValidateString(string? str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentException("Строка не может быть пустой, null или содержать только пробелы.");
            }
        }

        string Help { get; set; } = "Просто вводи команды\n/start, /help, /info, /exit.\nЕсли авторизовался, то вводи команду /addtask, /showtasks, /remowetask (фрмат ввода '№ задачи'), /completetask (фрмат ввода 'команда id задачи'), /showalltasks\nУдачи!!!!!";
        string Info { get; set; } = "Версия: 2\nДата создания: 14.11.2025\nДата обновления: 11.01.2026";

    }
}
