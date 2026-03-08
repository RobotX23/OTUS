
using Telegram.Bot;
using Telegram.Bot.Types;

namespace InteractiveСonsole.Project.TelegramBot.Scenarios
{
    internal class AddTaskScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;

        public AddTaskScenario(IUserService userService, IToDoService todoService)
        {
            _userService = userService;
            _todoService = todoService;
        }

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.AddTask;
        }

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Message message, CancellationToken ct)
        {
            switch (context.CurretStep)
            {
                case null:

                    var user = await _userService.GetUser(message.From!.Id);
                    if (user == null)
                    {
                        await bot.SendMessage(message.Chat.Id, "Пользователь не найден!", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }

                    context.Data["user"] = user;
                    context.CurretStep = "Name";

                    return ScenarioResult.Transition;

                case "Name":
                    var name = message.Text;

                    if (string.IsNullOrEmpty(name))
                    {
                        await bot.SendMessage(message.Chat.Id, "Название задачи не может быть пустым!", cancellationToken: ct);
                        return ScenarioResult.Transition;
                    }

                    var todoUser = (ToDoUser)context.Data["user"];

                    var task = await _todoService.Add(todoUser, name);

                    await bot.SendMessage(message.Chat.Id, $"Задача \"{task.Name}\" успешно добавлена!", cancellationToken: ct);

                    return ScenarioResult.Completed;
                
            }
            return ScenarioResult.Completed;
        }

    }
}
