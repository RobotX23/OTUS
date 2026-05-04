using Telegram.Bot.Types;

namespace InteractiveСonsole.Project.TelegramBot.Scenarios
{
    internal class ScenarioContext
    {
        public ScenarioType CurrentScenario { get; set; }
        public string? CurretStep { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
        public DateTime CreatedAt { get; }
        public long UserId { get; }

        public ScenarioContext(ScenarioType scenario, long userId)
        {
            CurrentScenario = scenario;
            CreatedAt = DateTime.UtcNow;
            UserId = userId;
        }

        public void SetData(string key, object? value)
        {
            Data[key] = value!;
        }

        public T? GetData<T>(string key) where T : class
        {
            return Data.TryGetValue(key, out var value) ? value as T : null;
        }
    }
}

