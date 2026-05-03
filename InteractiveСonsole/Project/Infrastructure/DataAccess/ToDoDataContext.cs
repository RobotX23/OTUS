using LinqToDB;


namespace InteractiveСonsole.Project.Infrastructure.DataAccess
{
    /// <summary>
    /// Контекст данных для работы с базой данных PostgreSQL через linq2db
    /// </summary>
    public class ToDoDataContext : LinqToDB.Data.DataConnection
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ToDoDataContext"/>
        /// </summary>
        /// <param name="connectionString">Строка подключения к базе данных PostgreSQL</param>
        public ToDoDataContext(string connectionString) 
            : base(ProviderName.PostgreSQL, connectionString)
        {
        }

        /// <summary>
        /// Таблица пользователей (ToDoUsers)
        /// </summary>
        public ITable<ToDoUser> ToDoUsers => this.GetTable<ToDoUser>();

        /// <summary>
        /// Таблица списков задач (ToDoLists)
        /// </summary>
        public ITable<ToDoList> ToDoLists => this.GetTable<ToDoList>();

        /// <summary>
        /// Таблица задач (ToDoItems)
        /// </summary>
        public ITable<ToDoItem> ToDoItems => this.GetTable<ToDoItem>();
    }
}
