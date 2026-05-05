using LinqToDB.Mapping;

namespace InteractiveСonsole.Project.Core.DataAccess.Models
{
    /// <summary>
    /// Модель задачи для работы с базой данных
    /// </summary>
    [Table("ToDoItems")]
    public class ToDoItemModel
    {
        /// <summary>
        /// Идентификатор задачи (первичный ключ)
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
        /// Название задачи
        /// </summary>
        [Column("Name")]
        public string? Name { get; set; }
        
        /// <summary>
        /// Время создания
        /// </summary>
        [Column("CreateAt")]
        public DateTime CreateAt { get; set; }
        
        /// <summary>
        /// Статус задачи
        /// </summary>
        [Column("State")]
        public ToDoItemState State { get; set; }
        
        /// <summary>
        /// Время изменения состояния
        /// </summary>
        [Column("StateChangeAt")]
        public DateTime? StateChangeAt { get; set; }
        
        /// <summary>
        /// Дата завершения
        /// </summary>
        [Column("Deadline")]
        public DateTime Deadline { get; set; }
        
        /// <summary>
        /// Идентификатор списка задач (внешний ключ)
        /// </summary>
        [Column("ListId")]
        public Guid? ListId { get; set; }
        
        /// <summary>
        /// Ссылка на список задач
        /// </summary>
        [Association(ThisKey = nameof(ListId), OtherKey = nameof(ToDoListModel.Id))]
        public ToDoListModel? List { get; set; }
    }

    /// <summary>
    /// Статус задачи
    /// </summary>
    public enum ToDoItemState
    {
        Active,
        Completed
    }
}
