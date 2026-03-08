using Telegram.Bot;
using Telegram.Bot.Types;

namespace InteractiveСonsole.Project.TelegramBot.Scenarios
{
    internal interface IScenario
    {
        bool CanHandle(ScenarioType scenario);
        Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Message message, CancellationToken ct);
    }
}
