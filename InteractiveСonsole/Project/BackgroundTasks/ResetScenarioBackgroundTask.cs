using InteractiveСonsole.Project.TelegramBot.Scenarios;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace InteractiveСonsole.Project.BackgroundTasks
{
    internal class ResetScenarioBackgroundTask : BackgroundTask
    {
        private readonly TimeSpan _resetScenarioTimeout;
        private readonly IScenarioContextRepository _scenarioRepository;
        private readonly ITelegramBotClient _bot;

        public ResetScenarioBackgroundTask(
            TimeSpan resetScenarioTimeout,
            IScenarioContextRepository scenarioRepository,
            ITelegramBotClient bot)
            : base(TimeSpan.FromHours(1), nameof(ResetScenarioBackgroundTask))
        {
            _resetScenarioTimeout = resetScenarioTimeout;
            _scenarioRepository = scenarioRepository ?? throw new ArgumentNullException(nameof(scenarioRepository));
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var contexts = await _scenarioRepository.GetContexts(ct);
            var now = DateTime.UtcNow;

            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton("/addtask"),
                new KeyboardButton("/show"),
                new KeyboardButton("/report")
            })
            {
                ResizeKeyboard = true
            };

            foreach (var context in contexts)
            {
                if (now - context.CreatedAt >= _resetScenarioTimeout)
                {
                    await _scenarioRepository.ResetContext(context.UserId, ct);

                    try
                    {
                        await _bot.SendMessage(
                            context.UserId,
                            $"Сценарий отменен, так как не поступил ответ в течение {_resetScenarioTimeout}",
                            replyMarkup: keyboard,
                            cancellationToken: ct);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"[ResetScenarioBackgroundTask] Не удалось отправить сообщение пользователю {context.UserId}: {ex.Message}");
                    }
                }
            }
        }
    }
}
