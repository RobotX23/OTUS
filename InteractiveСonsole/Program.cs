using InteractiveСonsole;
using InteractiveСonsole.Project.Core.Services;
using InteractiveСonsole.Project.Infrastructure.DataAccess;
using InteractiveСonsole.Project.TelegramBot.Scenarios;
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

    var userRepository = new FileUserRepository();
    var toDoRepository = new FileToDoRepository();
    var toDoListRepository = new FileToDoListRepository();
    var toDoService = new ToDoService(toDoRepository, 50, 100);
    var userService = new UserService(userRepository);
    var botClient = new TelegramBotClient(token);
    var scenarioRepository = new InMemoryScenarioContextRepository();
    var toDoListServace = new ToDoListService(toDoListRepository);
    var scenario = new List<IScenario>
    {
        new AddTaskScenario(userService, toDoService, toDoListServace),
        new AddListScenario(userService, toDoListServace),
        new DeleteListScenario(userService, toDoListServace, toDoService,scenarioRepository)
    };


    var handler = new UpdateHandler(toDoService, userService, toDoRepository, botClient, scenario, scenarioRepository, toDoListServace);
    botClient.StartReceiving(handler);

    var me = await botClient.GetMe();
    Console.WriteLine($"{me.FirstName} запущен!");

    var receiverOptions = new ReceiverOptions
    {
        AllowedUpdates = [UpdateType.Message], 
        DropPendingUpdates = true
    };

    Console.WriteLine("Нажмите клавишу A для выхода");

    // Ожидание нажатия клавиши
    while (true)
    {
        var key = Console.ReadKey(true).Key; // Считываем нажатую клавишу
        if (key == ConsoleKey.A)
        {
            Console.WriteLine("Выход из программы...");
            ctr.Cancel(); // Отмена всех асинхронных операций
            break;
        }
        else
        {
            Console.WriteLine($"Информация о боте: {me.FirstName} (ID: {me.Id})");
        }
    }

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






