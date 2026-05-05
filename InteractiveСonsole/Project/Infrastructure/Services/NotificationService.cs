using InteractiveСonsole.Project.Core.Entities;
using InteractiveСonsole.Project.Core.Services;
using InteractiveСonsole.Project.Infrastructure.DataAccess;
using InteractiveСonsole.Project.Infrastructure.DataAccess.Models;
using LinqToDB;
using LinqToDB.Async;

namespace InteractiveСonsole.Project.Infrastructure.Services
{
    internal class NotificationService : INotificationService
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public NotificationService(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public async Task<bool> ScheduleNotification(
            Guid userId,
            string type,
            string text,
            DateTime scheduledAt,
            CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            // Проверяем, нет ли уже уведомления с таким userId и type
            var exists = await dbContext.Notifications
                .Where(n => n.UserId == userId && n.Type == type)
                .AnyAsync(ct);

            if (exists)
                return false;

            var model = new NotificationModel
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                Text = text,
                ScheduledAt = scheduledAt,
                IsNotified = false,
                NotifiedAt = null
            };

            await dbContext.InsertAsync(model, token: ct);
            return true;
        }

        public async Task<IReadOnlyList<Notification>> GetScheduledNotification(
            DateTime scheduledBefore,
            CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var models = await dbContext.Notifications
                .Where(n => !n.IsNotified && n.ScheduledAt <= scheduledBefore)
                .LoadWith(n => n.User)
                .ToListAsync(ct);

            return models
                .Select(ModelMapper.MapFromModel)
                .ToList()
                .AsReadOnly();
        }

        public async Task MarkNotified(Guid notificationId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            await dbContext.Notifications
                .Where(n => n.Id == notificationId)
                .Set(n => n.IsNotified, true)
                .Set(n => n.NotifiedAt, DateTime.UtcNow)
                .UpdateAsync(ct);
        }

        // Вспомогательный метод для маппинга модели в сущность
        private static Notification MapFromModel(NotificationModel model)
        {
            if (model == null) return null!;

            return new Notification
            {
                Id = model.Id,
                UserId = model.UserId,
                User = model.User != null ? ModelMapper.MapFromModel(model.User) : null,
                Type = model.Type,
                Text = model.Text,
                ScheduledAt = model.ScheduledAt,
                IsNotified = model.IsNotified,
                NotifiedAt = model.NotifiedAt
            };
        }
    }
}
