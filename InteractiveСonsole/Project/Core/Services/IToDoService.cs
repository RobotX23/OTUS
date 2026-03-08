using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveСonsole
{
    public interface IToDoService
    {
        Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId);
        Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId);
        Task<ToDoItem> Add(ToDoUser user, string name, DateTime deadline);
        Task MarkCompleted(Guid id);
        Task Delete (Guid id);
        Task<IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix);

        Task<(int maxtasks, int maxline)> LineTasks();

        public int maxtasks { get; set; }
        public int maxline { get; set; }
    }
}
