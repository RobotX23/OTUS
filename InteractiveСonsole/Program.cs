using InteractiveСonsole;
using Otus.ToDoList.ConsoleBot;



try
{
    using var ctr = new CancellationTokenSource();

    var userRepository = new InMemoryUserRepository();
    var toDoRepository = new InMemoryToDoRepository();
    var toDoService = new ToDoService(toDoRepository, 0, 0);
    var userService = new UserService(userRepository);
    var handler = new UpdateHandler(toDoService, userService, toDoRepository);
    var botClient = new ConsoleBotClient();
    botClient.StartReceiving(handler, ctr.Token);

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






