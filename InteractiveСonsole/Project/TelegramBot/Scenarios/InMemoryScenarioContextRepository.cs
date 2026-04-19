using System.Collections.Concurrent;

namespace InteractiveСonsole.Project.TelegramBot.Scenarios
{
    internal class InMemoryScenarioContextRepository : IScenarioContextRepository
    {
        private readonly ConcurrentDictionary<long, ScenarioContext> _storege = new();

        public Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
        {
            _storege.TryGetValue(userId, out var context);
            return Task.FromResult(context);
        }

        public Task ResetContext(long userId, CancellationToken ct)
        {
            _storege.TryRemove(userId, out _);
            return Task.CompletedTask;
        }

        public Task SetContext(long userId, ScenarioContext context, CancellationToken ct)
        {
            _storege[userId] = context;
            return Task.CompletedTask;
        }
    }
}
