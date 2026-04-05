using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveСonsole
{
    internal interface IToDoRepository
    {
        Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct = default);
        //Возвращает ToDoItem для UserId со статусом Active
        Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct = default);
        Task<ToDoItem?> Get(Guid id, CancellationToken ct = default);
        Task Add(ToDoItem item, CancellationToken ct = default);
        Task Update(ToDoItem item, CancellationToken ct = default);
        Task Delete(Guid id, CancellationToken ct = default);
        //Проверяет есть ли задача с таким именем у пользователя
        Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct = default);
        //Возвращает количество активных задач у пользователя
        Task<int> CountActive(Guid userId, CancellationToken ct = default);
        Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct = default);

    }
}
