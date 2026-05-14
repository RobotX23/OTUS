using InteractiveСonsole.Project.Core.Entities;

namespace InteractiveСonsole.Project.Core.Services
{
    internal interface INotificationService
    {
        /// <summary>
        /// Создает нотификацию. Если запись с userId и type уже есть, возвращает false и не добавляет запись.
        /// </summary>
        Task<bool> ScheduleNotification(
            Guid userId,
            string type,
            string text,
            DateTime scheduledAt,
            CancellationToken ct);

        /// <summary>
        /// Возвращает нотификации, у которых IsNotified = false && ScheduledAt <= scheduledBefore
        /// </summary>
        Task<IReadOnlyList<Notification>> GetScheduledNotification(
            DateTime scheduledBefore,
            CancellationToken ct);

        /// <summary>
        /// Отмечает нотификацию как отправленную
        /// </summary>
        Task MarkNotified(Guid notificationId, CancellationToken ct);
    }
}
