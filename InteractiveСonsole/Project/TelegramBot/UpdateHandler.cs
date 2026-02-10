using InteractiveСonsole.Project.Core.Exceptions;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;


namespace InteractiveСonsole
{
    internal class UpdateHandler : IUpdateHandler
    {
        private int maxtasks = 0;
        private int maxline = 0;

        private string? name = null;
        private InMemoryUserRepository userRepository = new InMemoryUserRepository();
        private InMemoryToDoRepository toDoRepository = new InMemoryToDoRepository();
        private UserService users;
        private IToDoReportService toDoReportService;
        private ToDoService toDoService;    

        private ToDoUser user2 = null;

        private ITelegramBotClient _botClient;
        private Update _update;

        public UpdateHandler()
        {
            toDoService = new ToDoService(toDoRepository, maxtasks, maxline);
            users = new UserService(userRepository);
        }

        bool flag = false;
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            _botClient = botClient;
            _update = update;
            while (true)
            {
                try
                {
                    var taskline = await toDoService.LineTasks();
                    int maxtasks = taskline.Item1;
                    int maxline = taskline.Item2;
                    if (maxtasks == 0)
                    {
                        if (flag == false)
                        {
                            await _botClient.SendMessage(_update.Message.Chat, "Введите максимальное допустимое количество задач: ", ct);
                            flag = true;
                            break;
                        }
                        flag = false;
                        maxtasks = ParseAndValidatelnt(_update.Message.Text, 1, 100);
                        await _botClient.SendMessage(_update.Message.Chat, $"Вы ввели: {maxtasks} количество задач.", ct);
                        toDoService.maxtasks = maxtasks;
                    }

                    if (maxline == 0)
                    {
                        if (flag == false)
                        {
                            await _botClient.SendMessage(_update.Message.Chat, "Введите максимальную длинну задач: ", ct);
                            flag = true;
                            break;
                        }
                        flag = false;
                        maxline = ParseAndValidatelnt(_update.Message.Text, 1, 100);
                        await _botClient.SendMessage(_update.Message.Chat, $"Вы введи: {maxline} длинну задачи.", ct);
                        toDoService.maxline = maxline;
                        if (name == null)
                        {
                            if (flag == false)
                            {
                                await _botClient.SendMessage(_update.Message.Chat, "Привет!\nВведи следующие команды\n/start, /help, /info, /exit.", ct);
                                flag = true;
                                break;
                            }
                            flag = false;

                        }
                    }
                    string? text = _update.Message.Text;
                    if( await Returne(text, ct))
                    {
                        name = null;
                        user2 = null;
                        await _botClient.SendMessage(_update.Message.Chat, "Вы вышли из сессии. Для продолжения введите /start", ct);
                    }
                    else
                    {
                        break;
                    }
                    
                    


                }
                catch (TaskCountLimitException ex)
                {
                    HandleErrorAsync(_botClient, ex, ct);
                    await _botClient.SendMessage(_update.Message.Chat, ex.Message, ct);
                    break;

                }
                catch (TaskLengthLimitException ex)
                {
                    HandleErrorAsync(_botClient, ex, ct);
                    await _botClient.SendMessage(_update.Message.Chat, ex.Message, ct);
                    break;
                }
                catch (DublicateTaskException ex)
                {
                    HandleErrorAsync(_botClient, ex, ct);
                    await _botClient.SendMessage(_update.Message.Chat, ex.Message, ct);
                    break;
                }
                catch (FormatException)
                {
                    Exception myEx = new Exception("Ошибка: вы ввели не корректное число.");
                    HandleErrorAsync(_botClient, myEx, ct);
                    await _botClient.SendMessage(_update.Message.Chat, "Ошибка: вы ввели не корректное число.", ct);
                    flag = true;
                    break;
                }
                catch (ArgumentException ex)
                {
                    HandleErrorAsync(_botClient, ex, ct);
                    await _botClient.SendMessage(_update.Message.Chat, ex.Message, ct);
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

                    var user = _update.Message.From;
                    long userId = user.Id;
                    string userName = user.Username;

                    if(await users.GetUser(userId) == null)
                    {
                        user2 = await users.RegisterUser(userId, userName);
                        name =  user2.TelegramUserName;
                    }
                    else
                    {
                        user2 = await users.GetUser(userId);
                        name = user2.TelegramUserName;
                    }
                    NameVerification("Не получилось определить имя чата", name, ct);

                        return false;
                case "/help": //Обработка команды help
                    NameVerification(Help, name, ct);
                    return false;
                case "/info": //Обработка команды info
                    NameVerification(Info, name, ct);
                    return false;
                case "/exit":
                    return true; //Обработка команды exid
                case "/report":
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        await _botClient.SendMessage(_update.Message.Chat, "Команда не распознана", ct);
                        return false;
                    }
                    else 
                    {
                        toDoReportService = new ToDoReportService(toDoRepository);
                        var report = await toDoReportService.GetUserStats(user2.UserId);
                        await _botClient.SendMessage(_update.Message.Chat, $"Статистика по задачам на {report.generatedAt}. Всего: {report.total}; Завершено {report.completed}; Активных: {report.active};", ct);
                        return false;
                    }
                case string command when command.StartsWith("/find"):
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        await _botClient.SendMessage(_update.Message.Chat, "Команда не распознана", ct);
                        return false;
                    }
                    else
                    {
                        List<string> partOne = new List<string>();
                        partOne.AddRange(command.Split(' ', 2)); //Разделение строки по пробелу после команды
                        partOne.Add(" ");
                        ValidateString(partOne[1]);
                        string task_2 = partOne[1].Trim(); //Используем только вторую часть команды
                        var taski = await toDoService.Find(user2, task_2);

                        int i = 1;
                        if (taski != null)
                        {
                            await _botClient.SendMessage(_update.Message.Chat, "Ваш список задач:", ct);
                            foreach (var tasks in taski)
                            {
                                await _botClient.SendMessage(_update.Message.Chat, $"Задача {i++}:{tasks.Name} - {tasks.CreateAt} - {tasks.Id}", ct);
                            }
                        }
                        else
                        {
                            await _botClient.SendMessage(_update.Message.Chat, $"Список задач пуст!", ct);
                        }

                        return false;
                    }
                case string command when command.StartsWith("/addtask"):
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        await _botClient.SendMessage(_update.Message.Chat, "Команда не распознана", ct);
                        return false;
                    }
                    else
                    {
                        List<string> partOne = new List<string>();
                        partOne.AddRange(command.Split(' ', 2)); //Разделение строки по пробелу после команды
                        partOne.Add(" ");
                        ValidateString(partOne[1]);
                        string task_2 = partOne[1].Trim(); //Используем только вторую часть команды
                        var task_1 =await toDoService.Add(user2, task_2); // Вызов переданного метода
                        await _botClient.SendMessage(_update.Message.Chat, $"Задача \"{task_1.Name}\" успешно добавлена", ct);
                        return false;
                    }
                case "/showtasks":
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        await _botClient.SendMessage(_update.Message.Chat, "Команда не распознана", ct);
                        return false;
                    }
                    else
                    {
                        var taski = await toDoService.GetActiveByUserId(user2.UserId); // Вызов переданного метода

                        int i = 1;
                        if (taski != null)
                        {
                            await _botClient.SendMessage(_update.Message.Chat, "Ваш список задач:", ct);
                            foreach (var tasks in taski)
                            {
                                await _botClient.SendMessage(_update.Message.Chat, $"Задача {i++}:{tasks.Name} - {tasks.CreateAt} - {tasks.Id}", ct);
                            }
                        }
                        else
                        {
                            await _botClient.SendMessage(_update.Message.Chat, $"Список задач пуст!", ct);
                        }

                        return false;
                    }
                case string command when command.StartsWith("/remowetask"):
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        await _botClient.SendMessage(_update.Message.Chat, "Команда не распознана", ct);
                        return false;
                    }
                    else
                    {
                        List<string> parts = new List<string>();
                        parts.AddRange(command.Split(' ', 2)); //Разделение строки по пробелу после команды
                        parts.Add(" ");
                        ValidateString(parts[1]);
                        string number = parts[1].Trim(); //Используем только вторую часть команды

                        var taskess = await toDoService.GetAllByUserId(user2.UserId);


                        int numberr;
                        if (int.TryParse(number, out numberr))
                        {
                            if (numberr >= 1 && numberr <= taskess.Count)
                            {
                                var scan_task = taskess[Convert.ToInt32(number) - 1];
                                toDoService.Delete(scan_task.Id); // Вызов переданного метода
                                await _botClient.SendMessage(_update.Message.Chat, $"Задача - {scan_task.Name} удалена!", ct);
                            }
                            else
                            {
                                await _botClient.SendMessage(_update.Message.Chat, "Ошибка: введено не корректнок число.", ct);
                            }
                        }
                        else
                        {
                            await _botClient.SendMessage(_update.Message.Chat, "Ошибка: введено не число.", ct);
                        }

                        return false;
                    }
            
                case string command when command.StartsWith("/completetask"):
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        await _botClient.SendMessage(_update.Message.Chat, "Команда не распознана", ct);
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
                        await _botClient.SendMessage(_update.Message.Chat, $"Задача - {parts[1].Trim()} завершена!", ct);
                        return false;
                    }
                case "/showalltasks":
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        await _botClient.SendMessage(_update.Message.Chat, "Команда не распознана", ct);
                        return false;
                    }
                    else
                    {
                        var taski = await toDoService.GetAllByUserId(user2.UserId); // Вызов переданного метода

                        int i = 1;
                        if (taski != null)
                        {
                            await _botClient.SendMessage(_update.Message.Chat, "Ваш список задач:", ct);
                            foreach (var tasks in taski)
                            {
                                await _botClient.SendMessage(_update.Message.Chat, $"Задача {i++}:({tasks.State}) {tasks.Name} - {tasks.CreateAt} - {tasks.Id}", ct);
                            }
                        }
                        else
                        {
                            await _botClient.SendMessage(_update.Message.Chat, $"Список задач пуст!", ct);
                        }

                        return false;
                    }
                default: //если команды не распозднаны то выводим сообщение
                    await _botClient.SendMessage(_update.Message.Chat, "Команда не распознана", ct);
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
                await _botClient.SendMessage(_update.Message.Chat, $"Приветствую: {name}", ct);
                await _botClient.SendMessage(_update.Message.Chat, massege, ct);
            }
            else
            {
                await _botClient.SendMessage(_update.Message.Chat, massege, ct);
            }
        }

        void ValidateString(string? str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentException("Строка не может быть пустой, null или содержать только пробелы.");
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
        {
            Console.WriteLine($"HandleError: {exception})");
            return Task.CompletedTask;
        }


        string Help { get; set; } = "Просто вводи команды\n/start, /help, /info, /exit.\nЕсли авторизовался, то вводи команду /addtask, /showtasks, /remowetask (фрмат ввода '№ задачи'), /completetask (фрмат ввода 'команда id задачи'), /showalltasks, /find (вводи часть задачи и получай список задач начинающийся на данное слово), /report (вывод статистики)\nУдачи!!!!!";
        string Info { get; set; } = "Версия: 2\nДата создания: 14.11.2025\nДата обновления: 29.01.2026";

    }
}
