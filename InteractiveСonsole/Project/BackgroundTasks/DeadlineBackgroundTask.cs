using InteractiveСonsole.Project.BackgroundTasks;
using InteractiveСonsole.Project.Core.Services;
namespace InteractiveСonsole.Project.BackgroundTasks
{
    internal class DeadlineBackgroundTask : BackgroundTask
    {
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly IToDoRepository _toDoRepository;

        public DeadlineBackgroundTask(
            INotificationService notificationService,
            IUserRepository userRepository,
            IToDoRepository toDoRepository)
            : base(TimeSpan.FromHours(1), nameof(DeadlineBackgroundTask))
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _toDoRepository = toDoRepository ?? throw new ArgumentNullException(nameof(toDoRepository));
        }

        protected override async Task Execute(CancellationToken ct)
        {
            // Получаем всех пользователей
            var users = await _userRepository.GetUsers(ct);

            // Диапазон: вчерашний день (полные сутки)
            var from = DateTime.UtcNow.AddDays(-1).Date;
            var to = DateTime.UtcNow.Date;

            foreach (var user in users)
            {
                // Получаем просроченные активные задачи
                var overdueTasks = await _toDoRepository.GetActiveWithDeadline(
                    user.UserId, from, to, ct);

                foreach (var task in overdueTasks)
                {
                    // Создаём уникальную нотификацию для каждой задачи
                    var type = $"Deadline_{task.Id}";
                    var text = $"Ой! Вы пропустили дедлайн по задаче {task.Name}";

                    // Планируем отправку немедленно
                    await _notificationService.ScheduleNotification(
                        user.UserId,
                        type,
                        text,
                        DateTime.UtcNow,
                        ct);
                }
            }
        }
    }
}