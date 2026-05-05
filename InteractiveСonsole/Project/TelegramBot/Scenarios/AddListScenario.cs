using InteractiveСonsole.Project.Core.Services;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace InteractiveСonsole.Project.TelegramBot.Scenarios
{
    internal class AddListScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoListService _toDoListService;

        public AddListScenario(IUserService userService, IToDoListService toDoListService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _toDoListService = toDoListService ?? throw new ArgumentNullException(nameof(toDoListService));
        }

        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.AddList;

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
                    // ✅ Безопасное получение ID: приоритет callbackQuery, затем message
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

                    // ✅ КРИТИЧЕСКАЯ ПРОВЕРКА: если пользователь не найден — не сохраняем null в контекст
                    if (user == null)
                    {
                        await botClient.SendMessage(
                            callbackQuery?.From?.Id ?? message?.Chat.Id ?? 0,
                            "Пользователь не найден! Пожалуйста, используйте /start для регистрации.",
                            cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }

                    context.Data["user"] = user;
                    context.CurretStep = "Name";
                    return ScenarioResult.Transition;

                case "Name":
                    if (message?.Text == "Назад")
                        return ScenarioResult.Completed;

                    ValidateName(message?.Text);

                    // ✅ Безопасное извлечение пользователя из контекста
                    if (!context.Data.TryGetValue("user", out var userObj) || userObj is not ToDoUser toDoUser)
                    {
                        // Исправлена опечатка: "Пользовотель" → "Пользователь"
                        throw new InvalidOperationException("Пользователь отсутствует в контексте сценария");
                    }

                    await _toDoListService.Add(toDoUser, message!.Text!.Trim(), ct);
                    await botClient.SendMessage(message.Chat.Id, "Список успешно создан.", cancellationToken: ct);
                    return ScenarioResult.Completed;

                default:
                    return ScenarioResult.Completed;
            }
        }

        private void ValidateName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название списка не может быть пустым.");
        }
    }
}