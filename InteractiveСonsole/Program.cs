using InteractiveСonsole;
using System.Threading.Tasks;

string? name = null;

List<string> task = new List<string>();


Console.WriteLine("Привет!\nВведи следующие команды\n/start, /help, /info, /exit.\n");
while (true)
{
    if (Returne(Console.ReadLine())) 
    {
        break; 
    }

}

/// <summary>
/// Основной метод работы алгоритма
/// </summary>
bool Returne(string? text)
{
    switch (text)
    {
        case "/start"://Обработка команды start
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Введите имя!\n");
                name = Console.ReadLine();
                Console.WriteLine($"Теперь ты авторизован {name}. Чем могу помочь?\n");
            }
            else
            {
                NameVerification(Commands.StartGud, name);
            }
            return false;
        case "/help": //Обработка команды help
            NameVerification(Commands.Help, name);
            return false;
        case "/info": //Обработка команды info
            NameVerification(Commands.Info, name);
            return false;
        case "/exit":
            return true; //Обработка команды exid
        case string command when command.StartsWith("/echo"):
            return NotName(Echo, command);
        case "/addtask":
            return NotName(TaskAdd, "");
        case "/showtasks":
            return NotName(TaskShow, "");
        case "/remowetask":
            return NotName(TaskRemove, "");
        default: //если команды не распозднаны то выводим сообщение
            Console.WriteLine("Команда не распознана\n");
            return false;
    }
}

void Echo(string command)
{
    string[] parts = command.Split(' ', 2); //Разделение строки по пробелу после команды
    if (parts.Length == 1)
    {
        NameVerification("Пожалуйста, введите сообщение после команды /echo через пробел.\n", name);
    }
    else
    {
        string message = parts[1].Trim(); //Используем только вторую часть команды
        NameVerification($"Вы ввели: {message}", name);
    }
}


bool NotName(Action<string> taskAction, string text)
{
    if (string.IsNullOrWhiteSpace(name))
    {
        Console.WriteLine("Команда не распознана\n");
        return false;
    }
    else
    {
        taskAction(text); // Вызов переданного метода
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
        Console.WriteLine($"Приветствую: {name} \n");
        Console.WriteLine(massege + "\n");
    }
    else
    {
        Console.WriteLine(massege + "\n");
    }
}

/// <summary>
/// Метод добавление задачи
/// </summary>
void TaskAdd(string lol)
{
    Console.WriteLine("Введите описание задачи:");
    string? input = Console.ReadLine();

    // Проверка на null или пустую строку
    if (string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine("Вы не ввели задачу\n");
    }
    else
    {
        task.Add(input); // Добавление элемента в список
        Console.WriteLine($"Задача \"{input}\" успешно добавлена\n");
    }

}


/// <summary>
/// Метод проверки задач
/// </summary>
void TaskShow(string lol)
{

    if (task.Count == 0)
    {
        Console.WriteLine("Список задач пуст\n");
    }
    else
    {
        int i = 1;
        Console.WriteLine("Ваш список задач:\n");
        foreach (var tasks in task)
        {
            Console.WriteLine($"Задача {i++}:{tasks}");
        }
        Console.WriteLine("\n");
    }
}

/// <summary>
/// Метод удаления задач
/// </summary>
void TaskRemove(string lol)
{

    if (task.Count == 0)
    {
        Console.WriteLine("Список задач пуст\n");
    }
    else
    {
        int i = 1;
        foreach (var tasks in task)
        {
            Console.WriteLine($"Задача {i++}:{tasks}");
        }
        Console.WriteLine("Какую задачу удалить? Введите номер задачи\n");

        string? input = Console.ReadLine();

        // Проверка на null или пустую строку
        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Вы не ввели задачу\n");
        }
        else
        {

            int number;

            // Используем TryParse для проверки, является ли ввод числом
            if (int.TryParse(input, out number))
            {
                if (number >=1 && number <= task.Count)
                {
                    string taska = task[number-1];
                    task.RemoveAt(number-1);
                    Console.WriteLine($"Задача \"{taska}\" успешно удалена.\n");
                }
                else
                {
                    Console.WriteLine("Ошибка: введено не корректнок число.\n");
                }
            }
            else
            {
                Console.WriteLine("Ошибка: введено не число.\n");
            }
        }

    }
}

namespace InteractiveСonsole
{
    /// <summary>
    /// Это класс, который в котором хранятся готовые команды.
    /// </summary>
    public static class Commands
    {
        public static string Help { get; set; } = "Просто вводи команды\n/start, /help, /info, /exit.\nЕсли авторизовался, то вводи команду /echo, /addtask, /showtasks, /remowetask\nУдачи!!!!!";
        public static string Info { get; set; } = "Версия: 2\nДата создания: 14.11.2025\nДата обновления: 04.12.2025";
        public static string StartGud { get; set; } = "Ты уже авторизованы";

    }


}



