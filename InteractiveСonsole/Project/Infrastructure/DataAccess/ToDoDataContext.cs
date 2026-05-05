using InteractiveСonsole.Project.Core.DataAccess.Models;
using InteractiveСonsole.Project.Infrastructure.DataAccess.Models;
using LinqToDB;

namespace InteractiveСonsole.Project.Infrastructure.DataAccess
{
    public class ToDoDataContext : LinqToDB.Data.DataConnection
    {
        public ToDoDataContext(string connectionString)
            : base(ProviderName.PostgreSQL, connectionString) { }

        public ITable<ToDoUserModel> ToDoUsers => this.GetTable<ToDoUserModel>();
        public ITable<ToDoListModel> ToDoLists => this.GetTable<ToDoListModel>();
        public ITable<ToDoItemModel> ToDoItems => this.GetTable<ToDoItemModel>();

        public ITable<NotificationModel> Notifications => this.GetTable<NotificationModel>();
    }
}