
using InteractiveСonsole.Project.Core.Services;
using InteractiveСonsole.Project.TelegramBot.Dto;
using Microsoft.VisualBasic;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace InteractiveСonsole.Project.TelegramBot.Scenarios
{
    internal class AddTaskScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;
        private readonly IToDoListService _todoListService;

        public AddTaskScenario(IUserService userService, IToDoService todoService, IToDoListService toDoListService)
        {
            _userService = userService;
            _todoService = todoService;
            _todoListService = toDoListService;
        }

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.AddTask;
        }

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Message message, CancellationToken ct, CallbackQuery callbackQuery)
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
                    if (taskName == "Назад")
                        return ScenarioResult.Completed;

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
                    if (message.Text == "Назад")
                        return ScenarioResult.Completed;
                    if (!DateTime.TryParseExact(message.Text, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var deadline))
                    {
                        await bot.SendMessage(message.Chat.Id, "Неверный формат даты. Пожалуйста, введите дату в формате dd.MM.yyyy:", cancellationToken: ct);
                        return ScenarioResult.Transition;
                    }

                    if (deadline.Date < DateTime.Now.Date)
                    {
                        await bot.SendMessage(message.Chat.Id, "Дедлайн не может быть в прошлом. Пожалуйста введите дату в будущем:", cancellationToken: ct);
                        return ScenarioResult.Transition;
                    }

                    context.Data["Deadline"] = deadline;


                    var user2 = (ToDoUser)context.Data["user"];
                    // Получаем списки пользователя 
                    var lists = await _todoListService.GetUserLists(user2.UserId, ct) ?? Array.Empty<ToDoList>();

                    // Список рядов кнопок для InlineKeyboardMarkup
                    var rows = new List<IEnumerable<InlineKeyboardButton>>();

                    // Кнопка "📌Без списка" — Action = "show", ToDoListId = null
                    var noListDto = new ToDoListCallbackDto { Action = "show", ToDoListId = null };
                    rows.Add(new[] { InlineKeyboardButton.WithCallbackData("📌Без списка", noListDto.ToString()) });
                    // Кнопки для каждого списка пользователя — Action = "show", ToDoListId = list.Id
                    foreach (var l in lists)
                    {
                        var dto1 = new ToDoListCallbackDto { Action = "show", ToDoListId = l.Id };
                        var callback = dto1.ToString();

                        rows.Add(new[] { InlineKeyboardButton.WithCallbackData(l.Name, callback) });
                    }

                    var markup = new InlineKeyboardMarkup(rows);

                    // Отправляем сообщение с клавиатурой
                    await bot.SendMessage(message.Chat.Id, "Выберите список для задачи:", replyMarkup: markup, cancellationToken: ct);

                    context.CurretStep = "SelectList";
                    return ScenarioResult.Transition;

                case "SelectList": // Пользователь вводит название нового списка
                    if (message != null)
                    {
                        if (message.Text == "Назад")
                            return ScenarioResult.Completed;
                    }
                    
                    var data = callbackQuery.Data ?? string.Empty;
                    var dto = ToDoListCallbackDto.FromString(data);


                    var listName = dto.ToDoListId;


                    if (listName == null)
                    {
                        await TaskAdd(context, bot, callbackQuery, ct, null);
                        return ScenarioResult.Completed;
                    }
                    else
                    {

                        var name = _todoListService.Get((Guid)listName, ct).Result;


                        if (string.IsNullOrWhiteSpace(name.Name))
                        {
                            await bot.SendMessage(message.Chat.Id, "Название списка не может быть пустым. Попробуйте ещё раз:", cancellationToken: ct);
                            return ScenarioResult.Transition;
                        }
                        var user3 = (ToDoUser)context.Data["user"];
                        // Создаём список через сервис
                        ToDoList newList = new ToDoList(name.User, name.Name);
                        newList.Id = name.Id;
                        newList.CreateAt = name.CreateAt;

                        await TaskAdd(context, bot, callbackQuery, ct, newList);
                        return ScenarioResult.Completed;
                    }

                    

                default:
                    await bot.SendMessage(message.Chat.Id, "Неподдерживаемый шаг сценария.", cancellationToken: ct);
                    return ScenarioResult.Completed;
            }

            
        }

        private async Task TaskAdd(ScenarioContext context, ITelegramBotClient bot, CallbackQuery callbackQuery, CancellationToken ct, ToDoList newList)
        {
            var nameForTask1 = (string)context.Data["TaskName"];
            var dl1 = (DateTime)context.Data["Deadline"];

            var task = await _todoService.Add((ToDoUser)context.Data["user"], nameForTask1, dl1, newList, ct);
            var registeredUser = await _userService.GetUser(callbackQuery.From.Id);
            if (newList == null) 
                await bot.SendMessage(registeredUser.TelegramUserId, $"Задача \"{task.Name}\" успешно добавлена без списка с дедлайном {task.Deadline:dd.MM.yyyy}.", cancellationToken: ct);
            else
                await bot.SendMessage(registeredUser.TelegramUserId, $"Задача \"{task.Name}\" успешно добавлена в список \"{newList.Name}\" с дедлайном {task.Deadline:dd.MM.yyyy}.", cancellationToken: ct);
        }



    }
}
