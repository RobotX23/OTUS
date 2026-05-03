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
        private readonly IScenarioContextRepository _scenarioContextRepository;

        public DeleteListScenario(IUserService userService, IToDoListService toDoListService, IToDoService toDoService, IScenarioContextRepository scenarioContextRepository)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _toDoListService = toDoListService ?? throw new ArgumentNullException(nameof(toDoListService));
            _toDoService = toDoService ?? throw new ArgumentNullException(nameof(toDoService));
            _scenarioContextRepository = scenarioContextRepository;
        }

        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.DeleteList;

        public async Task<ScenarioResult> HandleMessageAsync(
            ITelegramBotClient botClient,
            ScenarioContext context,
            Message message,
            CancellationToken ct,
            CallbackQuery callbackQuery)
        {
            switch (context.CurretStep)
            {
                case null:
                    // ✅ Безопасное получение ID пользователя
                    var userId = callbackQuery?.From?.Id ?? message?.From?.Id;
                    if (userId == null)
                    {
                        await botClient.SendMessage(
                            callbackQuery?.From?.Id ?? message?.Chat.Id ?? 0,
                            "Не удалось определить пользователя.",
                            cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }

                    var user = await _userService.GetUser(userId.Value, ct);

                    if (user == null)
                    {
                        await botClient.SendMessage(
                            callbackQuery?.From?.Id ?? message?.Chat.Id ?? 0,
                            "Пользователь не найден! Пожалуйста, используйте /start для регистрации.",
                            cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }

                    context.Data["user"] = user;

                    var lists = await _toDoListService.GetUserLists(user.UserId, ct) ?? Array.Empty<ToDoList>();

                    // ✅ Если списков нет — сразу завершаем
                    if (!lists.Any())
                    {
                        await botClient.SendMessage(
                            message?.Chat.Id ?? callbackQuery?.From?.Id ?? 0,
                            "У вас нет списков для удаления.",
                            cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }

                    var rows = new List<IEnumerable<InlineKeyboardButton>>();
                    foreach (var l in lists)
                    {
                        var dto1 = new ToDoListCallbackDto { Action = "deletelist", ToDoListId = l.Id };
                        var callback1 = dto1.ToString();
                        if (callback1.Length > 64)
                            callback1 = $"{dto1.Action}|{l.Id.ToString("N")}";
                        rows.Add(new[] { InlineKeyboardButton.WithCallbackData(l.Name, callback1) });
                    }

                    // ❌ Убран noListDto с Guid.Empty — не имеет смысла удалять "без списка"

                    var markup = new InlineKeyboardMarkup(rows);

                    var newKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton("Назад")
                    })
                    {
                        ResizeKeyboard = true
                    };

                    var chatId = message?.Chat.Id ?? callbackQuery?.Message?.Chat.Id ?? 0;
                    await botClient.SendMessage(chatId, "Режим удаления списков", replyMarkup: newKeyboard, cancellationToken: ct);
                    await botClient.SendMessage(chatId, "Выберите список для удаления:", replyMarkup: markup, cancellationToken: ct);

                    context.CurretStep = "Approve";
                    return ScenarioResult.Transition;

                case "Approve":
                    if (message?.Text == "Назад")
                        return ScenarioResult.Completed;

                    var data = callbackQuery?.Data ?? string.Empty;
                    if (string.IsNullOrEmpty(data))
                        return ScenarioResult.Transition;

                    var dto = ToDoListCallbackDto.FromString(data);

                    var callbackUserId = callbackQuery?.From?.Id;
                    if (callbackUserId == null)
                        return ScenarioResult.Completed;

                    var registeredUser = await _userService.GetUser(callbackUserId.Value, ct);
                    if (registeredUser == null)
                        return ScenarioResult.Completed;

                    var all = await _toDoListService.GetUserLists(registeredUser.UserId, ct) ?? Array.Empty<ToDoList>();
                    var selected = all.FirstOrDefault(l => l.Id == dto.ToDoListId);

                    // ✅ Проверка: если список не найден по ID — завершаем
                    if (selected == null)
                    {
                        await botClient.SendMessage(registeredUser.TelegramUserId, "Список не найден.", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }

                    context.SetData("list", selected);
                    context.SetData("user", registeredUser);
                    context.CurretStep = "Delete";
                    await _scenarioContextRepository.SetContext(registeredUser.TelegramUserId, context, ct);

                    var confirm = new InlineKeyboardMarkup(new[]
                    {
                        InlineKeyboardButton.WithCallbackData("✅Да", "yes"),
                        InlineKeyboardButton.WithCallbackData("❌Нет", "no")
                    });

                    var msgChatId = callbackQuery?.Message?.Chat.Id ?? registeredUser.TelegramUserId;
                    var msgId = callbackQuery?.Message?.MessageId;

                    if (msgId != null)
                        await botClient.EditMessageText(msgChatId, msgId.Value,
                            $"Подтверждаете удаление списка \"{selected.Name}\" и всех его задач?",
                            replyMarkup: confirm, cancellationToken: ct);
                    else
                        await botClient.SendMessage(registeredUser.TelegramUserId,
                            $"Подтверждаете удаление списка \"{selected.Name}\" и всех его задач?",
                            replyMarkup: confirm, cancellationToken: ct);

                    return ScenarioResult.Transition;

                case "Delete":
                    if (message?.Text == "Назад")
                        return ScenarioResult.Completed;

                    if (callbackQuery?.Data == null)
                        return ScenarioResult.Transition;

                    var regUserId = callbackQuery?.From?.Id;
                    if (regUserId == null)
                        return ScenarioResult.Completed;

                    var reg = await _userService.GetUser(regUserId.Value, ct);
                    if (reg == null)
                        return ScenarioResult.Completed;

                    var storedCtx = await _scenarioContextRepository.GetContext(reg.TelegramUserId, ct);
                    var storedList = storedCtx?.GetData<ToDoList>("list");

                    // ✅ Проверка: если список в контексте = null — завершаем
                    if (storedList == null)
                    {
                        await botClient.SendMessage(reg.TelegramUserId, "Контекст потерян. Попробуйте снова.", cancellationToken: ct);
                        await _scenarioContextRepository.ResetContext(reg.TelegramUserId, ct);
                        return ScenarioResult.Completed;
                    }

                    var answer = callbackQuery.Data;
                    if (answer == "no")
                    {
                        var editChatId = callbackQuery?.Message?.Chat.Id ?? reg.TelegramUserId;
                        var editMsgId = callbackQuery?.Message?.MessageId;

                        if (editMsgId != null)
                            await botClient.EditMessageText(editChatId, editMsgId.Value, "Удаление отменено.", cancellationToken: ct);
                        else
                            await botClient.SendMessage(reg.TelegramUserId, "Удаление отменено.", cancellationToken: ct);

                        await _scenarioContextRepository.ResetContext(reg.TelegramUserId, ct);
                        return ScenarioResult.Completed;
                    }

                    if (answer == "yes")
                    {
                        var items = await _toDoService.GetByUserIdAndList(reg.UserId, storedList.Id, ct) ?? Array.Empty<ToDoItem>();
                        foreach (var it in items)
                            await _toDoService.Delete(it.Id, ct);

                        await _toDoListService.Delete(storedList.Id, ct);

                        var editChatId = callbackQuery?.Message?.Chat.Id ?? reg.TelegramUserId;
                        var editMsgId = callbackQuery?.Message?.MessageId;

                        if (editMsgId != null)
                            await botClient.EditMessageText(editChatId, editMsgId.Value,
                                $"Список \"{storedList.Name}\" и все его задачи были удалены.",
                                cancellationToken: ct);
                        else
                            await botClient.SendMessage(reg.TelegramUserId,
                                $"Список \"{storedList.Name}\" и все его задачи были удалены.",
                                cancellationToken: ct);

                        await _scenarioContextRepository.ResetContext(reg.TelegramUserId, ct);
                        return ScenarioResult.Completed;
                    }

                    return ScenarioResult.Completed;

                default:
                    return ScenarioResult.Completed;
            }
        }
    }
}