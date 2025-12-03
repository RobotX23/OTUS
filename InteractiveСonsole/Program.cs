using InteractiveСonsole;

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
                Console.WriteLine("Теперь ты авторизован. Удачи. Пиши /echo\n");
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
            if (string.IsNullOrWhiteSpace(name)) //Если пользователь всё таки ввёл данную команду то идём по сценарию, что команда не распознана
            {
                Console.WriteLine("Команда не распознана\n");
                return false;
            }
            else
            {
                if (command.Length == 5 || command[5] != ' ') //Проверка что сообщение введено корректно. Сообщение есть и оно указано через пробел после команды
                {
                    NameVerification("Пожалуйста, введите сообщение после команды /echo через пробел.\n", name);
                    return false; 
                }
                string[] parts = command.Split(' ', 2); //Разделение строки по пробелу после команды
                string message = parts[1].Trim(); //Используем только вторую часть команды
                NameVerification($"Вы ввели: {message}", name);
                return false;
            }
        case "/addtask":
            if (string.IsNullOrWhiteSpace(name)) //Если пользователь всё таки ввёл данную команду то идём по сценарию, что команда не распознана
            {
                Console.WriteLine("Команда не распознана\n");
                return false;
            }
            else
            {
                TaskAdd();
                return false;
            }
        default: //если команды не распозднаны то выводим сообщение
            Console.WriteLine("Команда не распознана\n");
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
void TaskAdd()
{
    Console.WriteLine("Введите описание задачи\n");
    string? input = Console.ReadLine();

    // Проверка на null или пустую строку
    if (string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine("Вы не ввели задачу\n");
    }

    task.Add(input); // Добавление элемента в список
    Console.WriteLine("Задача успешно добавлена\n");

}

namespace InteractiveСonsole
{
    /// <summary>
    /// Это класс, который в котором хранятся готовые команды.
    /// </summary>
    public static class Commands
    {
        public static string Help { get; set; } = "Просто вводи команды\n/start, /help, /info, /exit.\nЕсли авторизовался, то вводи команду /echo\nУдачи!!!!!";
        public static string Info { get; set; } = "Версия: 2\nДата создания: 14.11.2025\nДата обновления: 04.12.2025";
        public static string StartGud { get; set; } = "Ты уже авторизованы";

    }


}



