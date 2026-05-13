using System.Text;
using InteractiveСonsole.Project.Core.Services;

namespace InteractiveСonsole.Project.BackgroundTasks
{
    internal class TodayBackgroundTask : BackgroundTask
    {
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly IToDoRepository _toDoRepository;

        public TodayBackgroundTask(
            INotificationService notificationService,
            IUserRepository userRepository,
            IToDoRepository toDoRepository)
            : base(TimeSpan.FromDays(1), nameof(TodayBackgroundTask))
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _toDoRepository = toDoRepository ?? throw new ArgumentNullException(nameof(toDoRepository));
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var users = await _userRepository.GetUsers(ct);

            // Диапазон "сегодня": от 00:00 до 23:59 текущего дня по UTC
            var todayStart = DateTime.UtcNow.Date;
            var todayEnd = todayStart.AddDays(1);
            var notificationType = $"Today_{DateOnly.FromDateTime(DateTime.UtcNow)}";

            foreach (var user in users)
            {
                // Получаем активные задачи, дедлайн которых попадает на сегодня
                var tasksToday = await _toDoRepository.GetActiveWithDeadline(
                    user.UserId, todayStart, todayEnd, ct);

                if (tasksToday.Count == 0)
                    continue;

                // Формируем текст уведомления со списком задач
                var sb = new StringBuilder();
                sb.AppendLine("📅 Задачи на сегодня:");
                foreach (var task in tasksToday)
                {
                    sb.AppendLine($"• {task.Name}");
                }

                var text = sb.ToString().TrimEnd();

                // ⚠️ Примечание: ScheduleNotification проверяет уникальность (userId + type).
                // Поэтому создаём ОДНО уведомление на пользователя со списком всех задач.
                await _notificationService.ScheduleNotification(
                    user.UserId,
                    notificationType,
                    text,
                    DateTime.UtcNow, // Отправить сразу
                    ct);
            }
        }
    }
}
