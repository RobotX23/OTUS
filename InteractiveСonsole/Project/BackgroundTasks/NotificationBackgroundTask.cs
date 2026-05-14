using InteractiveСonsole.Project.Core.Services;
using Telegram.Bot;

namespace InteractiveСonsole.Project.BackgroundTasks
{
    internal class NotificationBackgroundTask : BackgroundTask
    {
        private readonly INotificationService _notificationService;
        private readonly ITelegramBotClient _bot;

        public NotificationBackgroundTask(
            INotificationService notificationService,
            ITelegramBotClient bot)
            : base(TimeSpan.FromMinutes(1), nameof(NotificationBackgroundTask))
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
        }

        protected override async Task Execute(CancellationToken ct)
        {
            // Получаем все уведомления, которые нужно отправить (ScheduledAt <= сейчас)
            var notifications = await _notificationService.GetScheduledNotification(DateTime.UtcNow, ct);

            foreach (var notification in notifications)
            {
                try
                {
                    // Отправляем сообщение только если у уведомления есть привязанный пользователь
                    if (notification.User?.TelegramUserId != null)
                    {
                        await _bot.SendMessage(
                            notification.User.TelegramUserId,
                            notification.Text,
                            cancellationToken: ct);

                        // Помечаем уведомление как отправленное
                        await _notificationService.MarkNotified(notification.Id, ct);
                    }
                }
                catch (Exception ex)
                {
                    // Логируем ошибку, но не прерываем обработку остальных уведомлений
                    Console.Error.WriteLine($"[NotificationBackgroundTask] Ошибка отправки уведомления {notification.Id}: {ex.Message}");
                }
            }
        }
    }
}