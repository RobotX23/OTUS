using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveСonsole
{
    /// <summary>
    /// Класс задачи
    /// </summary>
    public class ToDoItem
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
        /// <summary>
        /// Статус задачи
        /// </summary>
        public ToDoItemState State { get; set; }
        /// <summary>
        /// Время изменения состояния
        /// </summary>
        public DateTime? StateChangeAt { get; set; }

        /// <summary>
        /// Дата завершения
        /// </summary>
        public DateTime Deadline { get; set; }

        public void ToDoItemNew(ToDoUser user, string name, DateTime deadline) 
        {
            Id = Guid.NewGuid();
            User = user;
            Name = name;
            CreateAt = DateTime.UtcNow;
            State = ToDoItemState.Active;
            StateChangeAt = null;
            Deadline = deadline;


        }
        public void ChangeState (ToDoItemState newStat)
        {
            State = newStat;
            StateChangeAt = DateTime.UtcNow;
        }
    }


    public enum ToDoItemState
    {
        Active,
        Completed
    }
}
