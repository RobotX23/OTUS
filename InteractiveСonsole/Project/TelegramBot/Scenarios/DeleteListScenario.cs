using InteractiveСonsole.Project.Core.Services;
using InteractiveСonsole.Project.TelegramBot.Dto;
using Microsoft.VisualBasic;
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

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct, CallbackQuery callbackQuery)
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
                        var dto1 = new ToDoListCallbackDto { Action = "deletelist", ToDoListId = l.Id };
                        var callback1 = dto1.ToString();
                        if (callback1.Length > 64)
                            callback1 = $"{dto1.Action}|{l.Id.ToString("N")}";
                        rows.Add(new[] { InlineKeyboardButton.WithCallbackData(l.Name, callback1) });
                    }

                    // определяем noListDto с Guid.Empty
                    var noListDto = new ToDoListCallbackDto { Action = "deletelist", ToDoListId = Guid.Empty };
                    var noListCallback = noListDto.ToString();
                    if (noListCallback.Length > 64)
                        noListCallback = $"{noListDto.Action}|{Guid.Empty.ToString("N")}";

                    if (!rows.Any())
                    {
                        await botClient.SendMessage(message.Chat.Id, "Списков нет.", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }

                    var markup = new InlineKeyboardMarkup(rows);


                    var newKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                    new KeyboardButton("Назад") // Добавим кнопку для возврата на основное меню
                    })
                    {
                        ResizeKeyboard = true
                    };
                    await botClient.SendMessage(message.Chat.Id, "Режим удаление задач", replyMarkup: newKeyboard);
                    await botClient.SendMessage(message.Chat.Id, "Выберете список для удаления:", replyMarkup: markup, cancellationToken: ct);

                    context.CurretStep = "Approve";
                    return ScenarioResult.Transition;

                case "Approve":
                    if (message != null)
                        if (message.Text == "Назад")
                            return ScenarioResult.Completed;
                    // Ожидаем callback; сюда может прийти Message если пользователь ввёл текст — игнорируем
                    var data = callbackQuery.Data ?? string.Empty;
                    var dto = ToDoListCallbackDto.FromString(data);
                    var registeredUser = await _userService.GetUser(callbackQuery.From.Id);
                    var all = await _toDoListService.GetUserLists(registeredUser.UserId, ct) ?? Array.Empty<ToDoList>();
                    var selected = all.FirstOrDefault(l => l.Id == dto.ToDoListId);

                    context.SetData("list", selected);
                    context.SetData("user", registeredUser);
                    context.CurretStep = "Delete";
                    await _scenarioContextRepository.SetContext(registeredUser.TelegramUserId, context, ct);

                    var confirm = new InlineKeyboardMarkup(new[]
                    {
            InlineKeyboardButton.WithCallbackData("✅Да", "yes"),
            InlineKeyboardButton.WithCallbackData("❌Нет", "no")
        });

                    if (callbackQuery.Message != null)
                        await botClient.EditMessageText(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId,
                            $"Подтверждаете удаление списка {selected.Name} и всех его задач?", replyMarkup: confirm, cancellationToken: ct);
                    else
                        await botClient.SendMessage(registeredUser.TelegramUserId,
                            $"Подтверждаете удаление списка {selected.Name} и всех его задач?", replyMarkup: confirm, cancellationToken: ct);

                    context.CurretStep = "Delete";
                    return ScenarioResult.Transition;
                case "Delete":
                    if (message != null)
                        if (message.Text == "Назад") return ScenarioResult.Completed;
                    if (callbackQuery?.Data == null) return ScenarioResult.Transition;

                    var reg = await _userService.GetUser(callbackQuery.From.Id);
                    var storedCtx = await _scenarioContextRepository.GetContext(reg.TelegramUserId, ct);
                    var storedList = storedCtx?.GetData<ToDoList>("list");
                    if (storedList == null)
                    {
                        await botClient.SendMessage(reg.TelegramUserId, "Контекст потерян. Попробуйте снова.", cancellationToken: ct);
                        await _scenarioContextRepository.ResetContext(reg.TelegramUserId, ct);
                        return ScenarioResult.Completed;
                    }

                    var answer = callbackQuery.Data;
                    if (answer == "no")
                    {
                        if (callbackQuery.Message != null)
                            await botClient.EditMessageText(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, "Удаление отменено.", cancellationToken: ct);
                        else
                            await botClient.SendMessage(reg.TelegramUserId, "Удаление отменено.", cancellationToken: ct);

                        await _scenarioContextRepository.ResetContext(reg.TelegramUserId, ct);
                        return ScenarioResult.Completed;
                    }

                    if (answer == "yes")
                    {
                        var items = await _toDoService.GetByUserIdAndList(reg.UserId, storedList.Id, ct) ?? Array.Empty<ToDoItem>();
                        foreach (var it in items) await _toDoService.Delete(it.Id);
                        var names = await _toDoListService.Get(storedList.Id, ct);
                        await _toDoListService.Delete(storedList.Id, ct);

                        if (callbackQuery.Message != null)
                            await botClient.EditMessageText(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId,
                                $"Список {names.Name} и все его задачи были удалены.", cancellationToken: ct);
                        else
                            await botClient.SendMessage(reg.TelegramUserId,
                                $"Список {names.Name} и все его задачи были удалены.", cancellationToken: ct);

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

