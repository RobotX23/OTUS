using LinqToDB.Mapping;

namespace InteractiveСonsole.Project.Core.DataAccess.Models
{
    /// <summary>
    /// Модель списка задач для работы с базой данных
    /// </summary>
    [Table("ToDoLists")]
    public class ToDoListModel
    {
        /// <summary>
        /// Идентификатор списка (первичный ключ)
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
        /// Название списка
        /// </summary>
        [Column("Name")]
        public string? Name { get; set; }
        
        /// <summary>
        /// Время создания
        /// </summary>
        [Column("CreateAt")]
        public DateTime CreateAt { get; set; }
    }
}
