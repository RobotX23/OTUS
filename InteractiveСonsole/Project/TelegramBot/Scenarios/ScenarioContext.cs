namespace InteractiveСonsole.Project.TelegramBot.Scenarios
{
    internal class ScenarioContext
    {
        public ScenarioType CurrentScenario { get; set; }
        public string? CurretStep { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();

        public ScenarioContext(ScenarioType scenario)
        {
            CurrentScenario = scenario;
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

