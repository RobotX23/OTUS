namespace InteractiveСonsole.Project.Core.Entities
{
    /// <summary>
    /// Сущность уведомления для пользователя
    /// </summary>
    public class Notification
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Пользователь, которому предназначено уведомление
        /// </summary>
        public ToDoUser User { get; set; } = null!;

        /// <summary>
        /// Идентификатор пользователя (внешний ключ)
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Тип нотификации. Например: DeadLine_{ToDoItem.Id}, Today_{DateOnly.FromDateTime(DateTime.UtcNow)}
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Текст, который будет отправлен пользователю
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Запланированная дата отправки
        /// </summary>
        public DateTime ScheduledAt { get; set; }

        /// <summary>
        /// Флаг отправки уведомления
        /// </summary>
        public bool IsNotified { get; set; }

        /// <summary>
        /// Фактическая дата отправки
        /// </summary>
        public DateTime? NotifiedAt { get; set; }
    }
}
