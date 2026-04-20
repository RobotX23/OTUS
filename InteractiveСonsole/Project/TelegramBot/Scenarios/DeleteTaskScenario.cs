using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace InteractiveСonsole.Project.TelegramBot.Scenarios
{
    internal class DeleteTaskScenario : IScenario
    {
        private readonly IToDoService _toDoService;
        private readonly IUserService _userService;
        private readonly IScenarioContextRepository _scenarioContextRepository;

        public DeleteTaskScenario( IUserService userService, IToDoService toDoService, IScenarioContextRepository scenarioContextRepository)
        {
            _toDoService = toDoService ?? throw new ArgumentNullException(nameof(toDoService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _scenarioContextRepository = scenarioContextRepository;
        }

        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.DeleteTask;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct, CallbackQuery callbackQuery)
        {
            // Примечание: В вашем коде был опечатка CurretStep, здесь используем стандартный CurrentStep.
            // Если у вас в классе ScenarioContext свойство называется CurretStep, исправьте здесь.
            switch (context.CurretStep)
            {
                case null:
                    // ШАГ 1: Получаем ID задачи и запрашиваем подтверждение
                    Guid taskId;

                    // Пытаемся получить ID из контекста (передан из UpdateHandler)
                    if (context.Data.TryGetValue("taskId", out var idObj) && Guid.TryParse(idObj.ToString(), out taskId))
                    {
                        // ID найден
                    }
                    else if (callbackQuery.Data != null)
                    {
                        // Если в контексте нет, пробуем достать из callback data (на случай прямого вызова)
                        var parts = callbackQuery.Data.Split('|');
                        if (parts.Length > 1 && Guid.TryParse(parts[1], out taskId))
                        {
                            context.Data["taskId"] = taskId;
                        }
                        else
                        {
                            await botClient.SendMessage(callbackQuery.From.Id, "Ошибка: Не удалось определить задачу.", cancellationToken: ct);
                            return ScenarioResult.Completed;
                        }
                    }
                    else
                    {
                        return ScenarioResult.Transition;
                    }

                    // Получаем информацию о задаче для отображения имени
                    var task = await _toDoService.Get(taskId, ct);
                    if (task == null)
                    {
                        await botClient.SendMessage(callbackQuery.From.Id, "Задача не найдена или уже удалена.", cancellationToken: ct);
                        await _scenarioContextRepository.ResetContext(callbackQuery.From.Id, ct);
                        return ScenarioResult.Completed;
                    }

                    context.Data["taskName"] = task.Name;

                    var keyboard = new InlineKeyboardMarkup(new[]
                    {
                        InlineKeyboardButton.WithCallbackData("✅ Да", "yes"),
                        InlineKeyboardButton.WithCallbackData("❌ Нет", "no")
                    });

                    string text = $"Вы уверены, что хотите удалить задачу:\n*{task.Name}*?";

                    if (callbackQuery.Message != null)
                        await botClient.EditMessageText(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, text, replyMarkup: keyboard, parseMode: ParseMode.Markdown, cancellationToken: ct);
                    else
                        await botClient.SendMessage(callbackQuery.From.Id, text, replyMarkup: keyboard, parseMode: ParseMode.Markdown, cancellationToken: ct);

                    context.CurretStep = "Approve";
                    return ScenarioResult.Transition;

                case "Approve":
                    // ШАГ 2: Обработка ответа пользователя
                    var answer = callbackQuery.Data;
                    var storedTaskId = Guid.Parse(context.Data["taskId"].ToString());
                    var taskName = context.Data["taskName"].ToString();

                    if (answer == "no")
                    {
                        if (callbackQuery.Message != null)
                            await botClient.EditMessageText(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, "Удаление отменено.", cancellationToken: ct);
                        else
                            await botClient.SendMessage(callbackQuery.From.Id, "Удаление отменено.", cancellationToken: ct);

                        await _scenarioContextRepository.ResetContext(callbackQuery.From.Id, ct);
                        return ScenarioResult.Completed;
                    }

                    if (answer == "yes")
                    {
                        await _toDoService.Delete(storedTaskId, ct);

                        if (callbackQuery.Message != null)
                            await botClient.EditMessageText(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, $"Задача *{taskName}* удалена.", parseMode: ParseMode.Markdown, cancellationToken: ct);
                        else
                            await botClient.SendMessage(callbackQuery.From.Id, $"Задача *{taskName}* удалена.", parseMode: ParseMode.Markdown, cancellationToken: ct);

                        await _scenarioContextRepository.ResetContext(callbackQuery.From.Id, ct);
                        return ScenarioResult.Completed;
                    }
                    return ScenarioResult.Completed;

                default:
                    return ScenarioResult.Completed;
            }
        }
    }
}