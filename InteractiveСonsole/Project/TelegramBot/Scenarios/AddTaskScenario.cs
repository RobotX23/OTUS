
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

                case "Name": // Пользователь ввел название задачи
                    var taskName = message.Text?.Trim();
                    if (string.IsNullOrWhiteSpace(taskName))
                    {
                        await bot.SendMessage(message.Chat.Id, "Название задачи не может быть пустым. Попробуйте еще раз:");
                        return ScenarioResult.Transition;
                    }

                    context.Data["TaskName"] = taskName;
                    await bot.SendMessage(message.Chat.Id, "Введите срок выполнения задачи в формате dd.MM.yyyy:");
                    context.CurretStep = "Deadline";
                    return ScenarioResult.Transition;

                case "Deadline": // Пользователь ввел дату
                    if (DateTime.TryParseExact(message.Text, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var deadline))
                    {
                        if(deadline.Date < DateTime.Now)
                        {
                            await bot.SendMessage(message.Chat.Id, "Дедлайн не может быть в прошлом. Пожалуста введите дату в будующем");
                            return ScenarioResult.Transition;
                        }
                        var user2 = (ToDoUser)context.Data["user"];
                        var name = (string)context.Data["TaskName"];

                        var task = await _todoService.Add(user2, name, deadline);
                        await bot.SendMessage(message.Chat.Id, $"Задача \"{task.Name}\" успешно добавлена с дедлайном {task.Deadline:dd.MM.yyyy}.");

                        return ScenarioResult.Completed;
                    }
                    else 
                    {
                        await bot.SendMessage(message.Chat.Id, "Неверный формат даты. Пожалуйста, введите дату в формате dd.MM.yyyy:");
                        return ScenarioResult.Transition;
                    }

            }
            return ScenarioResult.Completed;
        }

    }
}
