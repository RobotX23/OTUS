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
        Guid Name;



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
                    if (message.Text == "Назад")
                        return ScenarioResult.Completed;
                    // Ожидаем callback; сюда может прийти Message если пользователь ввёл текст — игнорируем
                    var data = callbackQuery.Data ?? string.Empty;
                    var dto = ToDoListCallbackDto.FromString(data);
                    var registeredUser = await _userService.GetUser(callbackQuery.From.Id);
                    // Получаем список (вариант: GetById или искать в GetUserLists)
                    var all = await _toDoListService.GetUserLists(registeredUser.UserId, ct) ?? Array.Empty<ToDoList>();
                    var selected = all.FirstOrDefault(l => l.Id == dto.ToDoListId);

                    // Создаём/обновляем контекст сценария DeleteList и сохраняем выбранный список
                    var ctx = new ScenarioContext(ScenarioType.DeleteList) { CurretStep = "Delete" };
                    ctx.Data = ctx.Data ?? new Dictionary<string, object>();
                    ((Dictionary<string, object>)ctx.Data!)["list"] = selected;
                    ((Dictionary<string, object>)ctx.Data!)["user"] = registeredUser;
                    await _scenarioContextRepository.SetContext(registeredUser.TelegramUserId, ctx, ct);

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

                    Name = selected.Id;

                    var ll = selected;

                    context.CurretStep = "Delete";
                    return ScenarioResult.Transition;
                case "Delete":
                    if (message.Text == "Назад")
                        return ScenarioResult.Completed;
                    var data1 = callbackQuery.Data ?? string.Empty;
                    registeredUser = await _userService.GetUser(callbackQuery.From.Id);

                    // Подтверждение yes/no на шаге Delete
                    if (data1 == "yes" || data1 == "no")
                    {
                        Guid lol = Name;


                        if (data1 == "no")
                        {
                            if (callbackQuery.Message != null)
                                await botClient.EditMessageText(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, "Удаление отменено.", cancellationToken: ct);
                            else
                                await botClient.SendMessage(registeredUser.TelegramUserId, "Удаление отменено.", cancellationToken: ct);

                            await _scenarioContextRepository.ResetContext(registeredUser.TelegramUserId, ct);

                            return ScenarioResult.Completed;
                        }

                        // yes — удаляем задачи и список
                        var items = await _toDoService.GetByUserIdAndList(registeredUser.UserId, lol, ct) ?? Array.Empty<ToDoItem>();
                        foreach (var it in items)
                            await _toDoService.Delete(it.Id);

                        var names = await _toDoListService.Get(lol, ct);

                        await _toDoListService.Delete(lol, ct);


                        if (callbackQuery.Message != null)
                            await botClient.EditMessageText(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId,
                                $"Список {names.Name} и все его задачи были удалены.", cancellationToken: ct);
                        else
                            await botClient.SendMessage(registeredUser.TelegramUserId,
                                $"Список {names.Name} и все его задачи были удалены.", cancellationToken: ct);

                        await _scenarioContextRepository.ResetContext(registeredUser.TelegramUserId, ct);

                        
                    }
                    return ScenarioResult.Completed;

                default:
                    return ScenarioResult.Completed;
            }
        }
    }
}
