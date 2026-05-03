using LinqToDB.Mapping;

namespace InteractiveСonsole.Project.Core.DataAccess.Models
{
    /// <summary>
    /// Модель пользователя для работы с базой данных
    /// </summary>
    [Table("ToDoUsers")]
    public class ToDoUserModel
    {
        /// <summary>
        /// Идентификатор пользователя (первичный ключ)
        /// </summary>
        [PrimaryKey]
        [Column("UserId")]
        public Guid UserId { get; set; }
        
        /// <summary>
        /// Имя пользователя Telegram
        /// </summary>
        [Column("TelegramUserName")]
        public string? TelegramUserName { get; set; }
        
        /// <summary>
        /// Дата регистрации
        /// </summary>
        [Column("RegistereAt")]
        public DateTime RegistereAt { get; set; }
        
        /// <summary>
        /// Идентификатор пользователя в Telegram
        /// </summary>
        [Column("TelegramUserId")]
        public long TelegramUserId { get; set; }
    }
}
