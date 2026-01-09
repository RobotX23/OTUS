using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;


namespace InteractiveСonsole
{
    internal class UpdateHandler : IUpdateHandler
    {
        string? name = null;
        UserService users = new UserService();

        List<ToDoItem> taskes = new List<ToDoItem>();
        ToDoUser user = null;

        ITelegramBotClient _botClient;
        Update _update;


        int maxtasks = 0;
        int maxline = 0;
        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            _botClient = botClient;
            _update = update;
            try
            {

                if (maxtasks == 0)
                {
                    _botClient.SendMessage(_update.Message.Chat, "Введите максимальное допустимое количество задач: ");
                    string? imput = Console.ReadLine();
                    maxtasks = ParseAndValidatelnt(imput, 1, 100);
                    _botClient.SendMessage(_update.Message.Chat, $"Вы ввели: {maxtasks} количество задач.");
                }

                if (maxline == 0)
                {
                    _botClient.SendMessage(_update.Message.Chat, "Введите максимальную длинну задач: ");
                    string? imput_text = Console.ReadLine();
                    maxline = ParseAndValidatelnt(imput_text, 1, 100);
                    _botClient.SendMessage(_update.Message.Chat, $"Вы введи: {maxline} длинну задачи.");
                    if (name == null)
                        _botClient.SendMessage(_update.Message.Chat, "Привет!\nВведи следующие команды\n/start, /help, /info, /exit.\n");
                }

                while(true)
                {
                    if (Returne(Console.ReadLine()))
                    {
                        break;
                    }
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
                _botClient.SendMessage(_update.Message.Chat, "Ошибка: вы ввели не корректное число.\n");
            }
            catch (ArgumentException ex)
            {
                _botClient.SendMessage(_update.Message.Chat, ex.Message);
            }
        }
        int ParseAndValidatelnt(string? str, int min, int max)
        {
            if (!int.TryParse(str, out int result))
            {
                throw new FormatException("Ошибка: вы ввели некорректное число.\n");
            }

            if (result < min || result > max)
            {
                throw new ArgumentException("Количество задач должно быть от 1 до 100.\n");
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
                        name =  users.RegisterUser(userId, userName).TelegramUserName;
                    }
                    else
                    {
                        name = users.GetUser(userId).TelegramUserName;
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
                case "/addtask":
                    return NotName(TaskAdd, "");
                case "/showtasks":
                    return NotName(TaskShow, "");
                case "/remowetask":
                    return NotName(TaskRemove, "");
                case string command when command.StartsWith("/completetask"):
                    return NotName(CompleteTask, command);
                case "/showalltasks":
                    return NotName(ShowAllTasks, "");
                default: //если команды не распозднаны то выводим сообщение
                    _botClient.SendMessage(_update.Message.Chat, "Команда не распознана\n");
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
                _botClient.SendMessage(_update.Message.Chat, $"Приветствую: {name} \n");
            }
            else
            {
                _botClient.SendMessage(_update.Message.Chat, massege + "\n");
            }
        }

        bool NotName(Func<string, bool> taskAction, string text)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _botClient.SendMessage(_update.Message.Chat, "Команда не распознана\n");
                return false;
            }
            else
            {
                taskAction(text); // Вызов переданного метода
                return false;
            }
        }


        /// <summary>
        /// Метод добавление задачи
        /// </summary>
        bool TaskAdd(string lol)
        {
            var task = taskes.Where(x => x.User == user).ToList();

            if (task.Count > maxtasks - 1)
            {
                throw new TaskCountLimitException(maxtasks);
            }
            _botClient.SendMessage(_update.Message.Chat, "Введите описание задачи:");
            string? input = Console.ReadLine();

            ValidateString(input);

            if (input.Length > maxline)
            {
                throw new TaskLengthLimitException(input.Length, maxline);
            }

            if (task.FirstOrDefault(x => x.Name == input) != null)
            {
                throw new DublicateTaskException(input);
            }

            else
            {
                taskes.Add(new ToDoItem(user, input));
                _botClient.SendMessage(_update.Message.Chat, $"Задача \"{input}\" успешно добавлена\n");
                return true;
            }

        }

        /// <summary>
        /// Метод вывода всех задач
        /// </summary>
        bool ShowAllTasks(string lol)
        {
            var task = taskes.Where(x => x.User == user).OrderByDescending(x => x.Name).ToList();
            if (task.Count == 0)
            {
                _botClient.SendMessage(_update.Message.Chat, "Список задач пуст\n");
                return true;
            }
            else
            {
                int i = 1;
                _botClient.SendMessage(_update.Message.Chat, "Ваш список задач:\n");
                foreach (var tasks in task)
                {
                    _botClient.SendMessage(_update.Message.Chat, $"Задача {i++}:({tasks.State}) {tasks.Name} - {tasks.CreateAt} - {tasks.Id}");
                }
                _botClient.SendMessage(_update.Message.Chat, "\n");
                return true;
            }
        }





        /// <summary>
        /// Завершение задачи
        /// </summary>
        bool CompleteTask(string command)
        {
            var task = taskes.Where(x => x.User == user && x.State == ToDoItemState.Active).OrderBy(x => x.Name).ToList();
            if (task.Count == 0)
            {
                _botClient.SendMessage(_update.Message.Chat, "Список задач пуст\n");
                return true;
            }
            else
            {
                List<string> parts = new List<string>();
                parts.AddRange(command.Split(' ', 2)); //Разделение строки по пробелу после команды
                parts.Add(" ");
                ValidateString(parts[1]);
                string id = parts[1].Trim(); //Используем только вторую часть команды

                var zadacha = task.FirstOrDefault(x => x.Id == Guid.Parse(id));
                if (zadacha != null)
                {
                    zadacha.ChangeState(ToDoItemState.Completed);
                    _botClient.SendMessage(_update.Message.Chat, $"Задача {zadacha.Name} - {zadacha.Id} завершена!\n");
                }

                return true;
            }
        }





        /// <summary>
        /// Метод проверки задач
        /// </summary>
        bool TaskShow(string lol)
        {
            var task = taskes.Where(x => x.User == user && x.State == ToDoItemState.Active).OrderByDescending(x => x.Name).ToList();
            if (task.Count == 0)
            {
                _botClient.SendMessage(_update.Message.Chat, "Список задач пуст\n");
                return true;
            }
            else if (lol == "")
            {
                int i = 1;
                _botClient.SendMessage(_update.Message.Chat, "Ваш список задач:\n");
                foreach (var tasks in task)
                {
                    _botClient.SendMessage(_update.Message.Chat, $"Задача {i++}:{tasks.Name} - {tasks.CreateAt} - {tasks.Id}");
                }
                _botClient.SendMessage(_update.Message.Chat, "\n");
                return true;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Метод удаления задач
        /// </summary>
        bool TaskRemove(string lol)
        {
            if (ShowAllTasks(""))
            {
                var task = taskes.Where(x => x.User == user).OrderByDescending(x => x.Name).ToList();

                _botClient.SendMessage(_update.Message.Chat, "Какую задачу удалить? Введите номер задачи\n");

                string? input = Console.ReadLine();

                int number;

                // Используем TryParse для проверки, является ли ввод числом
                if (int.TryParse(input, out number))
                {
                    if (number >= 1 && number <= task.Count)
                    {
                        ToDoItem taska = task[number - 1];
                        taskes.Remove(taska);
                        _botClient.SendMessage(_update.Message.Chat, $"Задача \"{taska.Name}\" успешно удалена.\n");
                    }
                    else
                    {
                        _botClient.SendMessage(_update.Message.Chat, "Ошибка: введено не корректнок число.\n");
                    }
                }
                else
                {
                    _botClient.SendMessage(_update.Message.Chat, "Ошибка: введено не число.\n");
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        void ValidateString(string? str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentException("Строка не может быть пустой, null или содержать только пробелы.");
            }
        }

        public static string Help { get; set; } = "Просто вводи команды\n/start, /help, /info, /exit.\nЕсли авторизовался, то вводи команду /addtask, /showtasks, /remowetask, /completetask, /showalltasks\nУдачи!!!!!";
        public static string Info { get; set; } = "Версия: 2\nДата создания: 14.11.2025\nДата обновления: 09.01.2026";
        public static string StartGud { get; set; } = "Ты уже авторизованы";

    }
}
