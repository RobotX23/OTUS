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

    }
}
