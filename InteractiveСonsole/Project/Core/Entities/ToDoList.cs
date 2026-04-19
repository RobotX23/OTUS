using System.Collections.Generic;

namespace InteractiveСonsole
{
    /// <summary>
    /// 
    /// </summary>
    public class ToDoList
    {

        public Guid Id { get; set; }
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public ToDoUser User { get; set; }
        /// <summary>
        /// Название задачи
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Время создания
        /// </summary>
        public DateTime CreateAt { get; set; }

        public ToDoList(ToDoUser user, string name)
        {
            Id = Guid.NewGuid();
            User = user;
            Name = name;
            CreateAt = DateTime.UtcNow;
        }

    }
}
