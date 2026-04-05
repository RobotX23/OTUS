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
        Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct = default);
        Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct = default);
        Task<ToDoItem> Add(ToDoUser user, string name, DateTime deadline, ToDoList? list, CancellationToken ct = default);
        Task MarkCompleted(Guid id, CancellationToken ct = default);
        Task Delete (Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix, CancellationToken ct = default);

        Task<(int maxtasks, int maxline)> LineTasks();

        /// <summary>
        /// Устанавливает лимиты на количество и длину задач.
        /// Вызывается один раз при инициализации или при изменении настроек пользователем.
        /// </summary>
        Task SetLimits(int maxTasks, int maxLine, CancellationToken ct = default);

        Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct);
    }
}
