namespace InteractiveСonsole.Project.TelegramBot.Scenarios
{
    internal class InMemoryScenarioContextRepository : IScenarioContextRepository
    {
        private readonly Dictionary<long, ScenarioContext> _storege = new();

        public Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
        {
            _storege.TryGetValue(userId, out var context);
            return Task.FromResult(context);
        }

        public Task ResetContext(long userId, CancellationToken ct)
        {
            _storege.Remove(userId);
            return Task.CompletedTask;
        }

        public Task SetContext(long userId, ScenarioContext context, CancellationToken ct)
        {
            _storege[userId] = context;
            return Task.FromResult(context);
        }
    }
}
