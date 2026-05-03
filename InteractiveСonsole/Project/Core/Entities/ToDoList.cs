namespace InteractiveСonsole
{
    /// <summary>
    /// Список задач (анемичная модель)
    /// </summary>
    public class ToDoList
    {
        public Guid Id { get; set; }
        
        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public Guid UserId { get; set; }
        
        /// <summary>
        /// Ссылка на пользователя (для LINQ)
        /// </summary>
        public ToDoUser? User { get; set; }
        
        /// <summary>
        /// Название списка
        /// </summary>
        public string? Name { get; set; }
        
        /// <summary>
        /// Время создания
        /// </summary>
        public DateTime CreateAt { get; set; }
    }
}