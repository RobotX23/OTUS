using InteractiveСonsole.Project.Core.DataAccess.Models;
using LinqToDB.Mapping;

namespace InteractiveСonsole.Project.Infrastructure.DataAccess.Models
{
    /// <summary>
    /// Модель уведомления для работы с базой данных
    /// </summary>
    [Table("Notifications")]
    public class NotificationModel
    {
        /// <summary>
        /// Идентификатор уведомления (первичный ключ)
        /// </summary>
        [PrimaryKey]
        [Column("Id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор пользователя (внешний ключ)
        /// </summary>
        [Column("UserId")]
        public Guid UserId { get; set; }

        /// <summary>
        /// Ссылка на пользователя
        /// </summary>
        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.UserId))]
        public ToDoUserModel? User { get; set; }

        /// <summary>
        /// Тип нотификации
        /// </summary>
        [Column("Type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Текст уведомления
        /// </summary>
        [Column("Text")]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Запланированная дата отправки
        /// </summary>
        [Column("ScheduledAt")]
        public DateTime ScheduledAt { get; set; }

        /// <summary>
        /// Флаг отправки уведомления
        /// </summary>
        [Column("IsNotified")]
        public bool IsNotified { get; set; }

        /// <summary>
        /// Фактическая дата отправки
        /// </summary>
        [Column("NotifiedAt")]
        public DateTime? NotifiedAt { get; set; }
    }
}
