using InteractiveСonsole;
using System.Threading.Tasks;

string? name = null;

int maxtasks;
int maxline;

List<string> task = new List<string>();


try
{

    while (true)
    {
        try
        {
            Console.WriteLine("Введите максимальное допустимое количество задач: ");
            string? imput = Console.ReadLine();
            maxtasks = int.Parse(imput);

            if (maxtasks < 1 || maxtasks > 100)
            {
                throw new ArgumentException("Количество задач должно быть от 1 до 100.\n");
            }
            Console.WriteLine($"Вы введи: {maxtasks} задачу(и).");
            break;

        }
        catch (FormatException)
        {
            Console.WriteLine("Ошибка: вы ввели не корректное число.\n");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    while (true)
    {
        try
        {
            Console.WriteLine("Введите максимальную длинну задач: ");
            string? imput = Console.ReadLine();
            maxline = int.Parse(imput);

            if (maxline < 1 || maxline > 100)
            {
                throw new ArgumentException("Количество задач должно быть от 1 до 100.\n");
            }
            Console.WriteLine($"Вы введи: {maxline} длинну задачи.");
            break;

        }
        catch (FormatException)
        {
            Console.WriteLine("Ошибка: вы ввели не корректное число.\n");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }


    Console.WriteLine("Привет!\nВведи следующие команды\n/start, /help, /info, /exit.\n");
    while (true)
    {
        try
        {
            if (Returne(Console.ReadLine()))
            {
                break;
            }
        }
           
        catch (TaskCountLimitException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (TaskLengthLimitException ex)
        {
            Console.WriteLine(ex.Message); 
        }
        catch (DublicateTaskException ex) 
        {
            Console.WriteLine(ex.Message);
        }
        catch(ArgumentException ex)
        { 
            Console.WriteLine(ex.Message); 
        }
    }
 
}
catch (Exception ex) 
{
    Console.WriteLine("Произошла непридвиденная ошибка: ");
    Console.WriteLine($"Type: {ex.GetType()}");
    Console.WriteLine($"Message6 {ex.Message}");
    Console.WriteLine($"StackTrace: {ex.StackTrace}");
    if(ex.InnerException != null)
    {
        Console.WriteLine("InnerException: ");
        Console.WriteLine($"Type: {ex.InnerException.GetType()}");
        Console.WriteLine($"Message6 {ex.InnerException.Message}");
        Console.WriteLine($"StackTrace: {ex.InnerException.StackTrace}");
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

bool Echo(string command)
{
    string[] parts = command.Split(' ', 2); //Разделение строки по пробелу после команды
    if (ParseAndValidatelnt(parts[1], 1, 100) == 0)
    {
        return false;
    }
    else
    {
        string message = parts[1].Trim(); //Используем только вторую часть команды
        NameVerification($"Вы ввели: {message}", name);
        return true;
    }
}


bool NotName(Func<string, bool> taskAction, string text)
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
bool TaskAdd(string lol)
{
    
    if (task.Count > maxtasks-1)
    {
        throw new TaskCountLimitException(maxtasks);
    }
    Console.WriteLine("Введите описание задачи:");
    string? input = Console.ReadLine();

    // Проверка на null или пустую строку
    if (string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine("Вы не ввели задачу\n");
        return false;
    }

    if (ParseAndValidatelnt(input, 1, maxline) > maxline)
    {
        throw new TaskLengthLimitException(input.Length, maxline);
    }

    if(task.Contains(input))
    {
        throw new DublicateTaskException(input);
    }

    else
    {
        task.Add(input); // Добавление элемента в список
        Console.WriteLine($"Задача \"{input}\" успешно добавлена\n");
        return true;
    }

}


/// <summary>
/// Метод проверки задач
/// </summary>
bool TaskShow(string lol)
{

    if (task.Count == 0)
    {
        Console.WriteLine("Список задач пуст\n"); 
        return false;
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
        return true;
    }
}


int ParseAndValidatelnt(string? str, int min, int max)
{
    int result = str.Length;
    if (result < min || result > max)
    {
        throw new ArgumentException($"Количество строк в сообщение должно быть от {min} до {max}.\n");
    }
    return result;
}

/// <summary>
/// Метод удаления задач
/// </summary>
bool TaskRemove(string lol)
{
    if (TaskShow(""))
    {
        Console.WriteLine("Какую задачу удалить? Введите номер задачи\n");

        string? input = Console.ReadLine();

        int number;

        // Используем TryParse для проверки, является ли ввод числом
        if (int.TryParse(input, out number))
        {
            if (number >= 1 && number <= task.Count)
            {
                string taska = task[number - 1];
                task.RemoveAt(number - 1);
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
        
        return true;
    }
    else
    {
        return false;
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

public class TaskCountLimitException : Exception
{ 
    public TaskCountLimitException(int taskCountLimit) : base( $"Превышено максимальное количество задач равное {taskCountLimit}. \n") 
    {
    } 

}

public class TaskLengthLimitException : Exception
{
    public TaskLengthLimitException(int taskLength, int taskLengthLimit) : base($"Длинна задачи '{taskLength}' превышает максимальное допустимое значение {taskLengthLimit}. \n")
    {
    }

}

public class DublicateTaskException : Exception
{
    public DublicateTaskException(string task) : base($"Задача {task} уже существует.\n")
    {
    }

}



