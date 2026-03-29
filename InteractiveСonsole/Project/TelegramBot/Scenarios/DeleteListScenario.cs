using InteractiveСonsole.Project.Core.Services;
using InteractiveСonsole.Project.TelegramBot.Dto;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace InteractiveСonsole.Project.TelegramBot.Scenarios
{
    internal class DeleteListScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoListService _toDoListService;
        private readonly IToDoService _toDoService;

        public DeleteListScenario(IUserService userService, IToDoListService toDoListService, IToDoService toDoService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _toDoListService = toDoListService ?? throw new ArgumentNullException(nameof(toDoListService));
            _toDoService = toDoService ?? throw new ArgumentNullException(nameof(toDoService));
        }

        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.DeleteList;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            switch (context.CurretStep)
            {
                case null:
                    var user = await _userService.GetUser(message.From!.Id);
                    context.Data["user"] = user;

                    var lists = await _toDoListService.GetUserLists(user.UserId, ct) ?? Array.Empty<ToDoList>();
                    var rows = new List<IEnumerable<InlineKeyboardButton>>();
                    foreach (var l in lists)
                    {
                        var dto = new ToDoListCallbackDto { Action = "deletelist", ToDoListId = l.Id };
                        var callback = dto.ToString();
                        if (callback.Length > 64)
                            callback = $"{dto.Action}|{l.Id.ToString("N")}";
                        rows.Add(new[] { InlineKeyboardButton.WithCallbackData(l.Name, callback) });
                    }

                    if (!rows.Any())
                    {
                        await botClient.SendMessage(message.Chat.Id, "Списков нет.", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }

                    var markup = new InlineKeyboardMarkup(rows);
                    await botClient.SendMessage(message.Chat.Id, "Выберете список для удаления:", replyMarkup: markup, cancellationToken: ct);

                    context.CurretStep = "Approve";
                    return ScenarioResult.Transition;

                case "Approve":
                    // Ожидаем callback; сюда может прийти Message если пользователь ввёл текст — игнорируем
                    return ScenarioResult.Transition;

                case "Delete":
                    // Приходят нажатия callback — но в вашем обработчике CallbackQuery мы будем устанавливать context.Data и переводиь шаг
                    return ScenarioResult.Completed;

                default:
                    return ScenarioResult.Completed;
            }
        }
    }
}
