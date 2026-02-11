using InteractiveСonsole;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;




try
{
    using var ctr = new CancellationTokenSource();
    string token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN_EX1", EnvironmentVariableTarget.User);
    if (string.IsNullOrEmpty(token))
    {
        Console.WriteLine("Токен бота не найден. Убедитесь, что вы установили переменную окружения.");
        return;
    }

    var userRepository = new InMemoryUserRepository();
    var toDoRepository = new InMemoryToDoRepository();
    var toDoService = new ToDoService(toDoRepository, 0, 0);
    var userService = new UserService(userRepository);
    var botClient = new TelegramBotClient(token);

    // Настройка опций получения обновлений
    var receiverOptions = new ReceiverOptions
    {
        AllowedUpdates = [UpdateType.Message], // Укажите необходимые типы обновлений, например, UpdateType.Message
        DropPendingUpdates = true
    };
    var handler = new UpdateHandler(toDoService, userService, toDoRepository, botClient);
    // Начинаем получать обновления
    botClient.StartReceiving(handler);

    var me = await botClient.GetMe();
    Console.WriteLine($"{me.FirstName} запущен!");

    await Task.Delay(-1); // Устанавливаем бесконечную задержку

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






