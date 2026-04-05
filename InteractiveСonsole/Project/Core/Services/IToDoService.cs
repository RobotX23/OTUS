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
        Task<ToDoItem> Add(ToDoUser user, string name, DateTime deadline, ToDoList? list);
        Task MarkCompleted(Guid id);
        Task Delete (Guid id);
        Task<IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix);

        Task<(int maxtasks, int maxline)> LineTasks();

        /// <summary>
        /// Устанавливает лимиты на количество и длину задач.
        /// Вызывается один раз при инициализации или при изменении настроек пользователем.
        /// </summary>
        void SetLimits(int maxTasks, int maxLine);

        Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct);
    }
}
