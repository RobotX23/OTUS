namespace InteractiveСonsole.Project.TelegramBot.Scenarios
{
    internal class ScenarioContext
    {
        public ScenarioType CurrentScenario {  get; set; }
        public string? CurretStep { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
        public ScenarioContext(ScenarioType scenario) 
        {
            CurrentScenario = scenario;
        }
    }
}
