using InteractiveСonsole;
using Otus.ToDoList.ConsoleBot;



try
{
    var handler = new UpdateHandler();
    var botClient = new ConsoleBotClient();
    botClient.StartReceiving(handler);

}
catch (Exception ex)
{
    Console.WriteLine("Произошла непридвиденная ошибка: ");
    Console.WriteLine($"Type: {ex.GetType()}");
    Console.WriteLine($"Message6 {ex.Message}");
    Console.WriteLine($"StackTrace: {ex.StackTrace}");
    if (ex.InnerException != null)
    {
        Console.WriteLine("InnerException: ");
        Console.WriteLine($"Type: {ex.InnerException.GetType()}");
        Console.WriteLine($"Message6 {ex.InnerException.Message}");
        Console.WriteLine($"StackTrace: {ex.InnerException.StackTrace}");
    }
}


public class TaskCountLimitException : Exception
{ 
    public TaskCountLimitException(int taskCountLimit) : base( $"Превышено максимальное количество задач равное {taskCountLimit}.") 
    {
    } 

}

public class TaskLengthLimitException : Exception
{
    public TaskLengthLimitException(int taskLength, int taskLengthLimit) : base($"Длинна задачи '{taskLength}' превышает максимальное допустимое значение {taskLengthLimit}.")
    {
    }

}

public class DublicateTaskException : Exception
{
    public DublicateTaskException(string task) : base($"Задача {task} уже существует.")
    {
    }

}



