using InteractiveСonsole;
using Telegram.Bot;




try
{
    using var ctr = new CancellationTokenSource();
    string token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN_EX1", EnvironmentVariableTarget.User);
    var userRepository = new InMemoryUserRepository();
    var toDoRepository = new InMemoryToDoRepository();
    var toDoService = new ToDoService(toDoRepository, 0, 0);
    var userService = new UserService(userRepository);
    var botClient = new TelegramBotClient(token);
    var handler = new UpdateHandler(toDoService, userService, toDoRepository, botClient);
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






