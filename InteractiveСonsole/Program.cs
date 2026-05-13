using InteractiveСonsole;
using InteractiveСonsole.Project.Core.Services;
using InteractiveСonsole.Project.Infrastructure.DataAccess;
using InteractiveСonsole.Project.TelegramBot.Scenarios;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using InteractiveСonsole.Project.BackgroundTasks;

try
{
    using var ctr = new CancellationTokenSource();

    var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING", EnvironmentVariableTarget.User);

    string token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN_EX1", EnvironmentVariableTarget.User);
    if (string.IsNullOrEmpty(token))
    {
        Console.WriteLine("Токен бота не найден. Убедитесь, что вы установили переменную окружения.");
        return;
    }

    var factory = new DataContextFactory(connectionString);
    var userRepository = new SqlUserRepository(factory);
    var toDoRepository = new SqlToDoRepository(factory);
    var toDoListRepository = new SqlToDoListRepository(factory);

    var toDoService = new ToDoService(toDoRepository, 50, 100);
    var userService = new UserService(userRepository);
    var botClient = new TelegramBotClient(token);
    var scenarioRepository = new InMemoryScenarioContextRepository();
    var toDoListServace = new ToDoListService(toDoListRepository);

    var scenarios = new List<IScenario>
    {
        new AddTaskScenario(userService, toDoService, toDoListServace),
        new AddListScenario(userService, toDoListServace),
        new DeleteListScenario(userService, toDoListServace, toDoService, scenarioRepository),
        new DeleteTaskScenario(userService, toDoService, scenarioRepository)
    };

    var handler = new UpdateHandler(toDoService, userService, toDoRepository, botClient, scenarios, scenarioRepository, toDoListServace);

    var backgroundTaskRunner = new BackgroundTaskRunner();
    backgroundTaskRunner.AddTask(new ResetScenarioBackgroundTask(
        TimeSpan.FromHours(1),   
        scenarioRepository,
        botClient));

    backgroundTaskRunner.StartTasks(ctr.Token);

    var receiverOptions = new ReceiverOptions
    {
        AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery],
        DropPendingUpdates = true
    };

    
    botClient.StartReceiving(handler, receiverOptions, ctr.Token);

    var me = await botClient.GetMe(cancellationToken: ctr.Token);
    Console.WriteLine($"{me.FirstName} запущен!");
    Console.WriteLine("Нажмите клавишу A для выхода");

    while (true)
    {
        var key = Console.ReadKey(true).Key;
        if (key == ConsoleKey.A)
        {
            Console.WriteLine("Выход из программы...");
            await backgroundTaskRunner.StopTasks(CancellationToken.None);
            ctr.Cancel();
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
    Console.WriteLine("Произошла непредвиденная ошибка:");
    Console.WriteLine($"Type: {ex.GetType()}");
    Console.WriteLine($"Message: {ex.Message}"); 
    Console.WriteLine($"StackTrace: {ex.StackTrace}");
    if (ex.InnerException != null)
    {
        Console.WriteLine("InnerException:");
        Console.WriteLine($"Type: {ex.InnerException.GetType()}");
        Console.WriteLine($"Message: {ex.InnerException.Message}");
    }
}