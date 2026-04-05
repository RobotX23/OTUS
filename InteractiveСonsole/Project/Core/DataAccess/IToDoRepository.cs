using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveСonsole
{
    internal interface IToDoRepository
    {
        Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId);
        //Возвращает ToDoItem для UserId со статусом Active
        Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId);
        Task<ToDoItem?> Get(Guid id);
        Task Add(ToDoItem item);
        Task Update(ToDoItem item);
        Task Delete(Guid id);
        //Проверяет есть ли задача с таким именем у пользователя
        Task<bool> ExistsByName(Guid userId, string name);
        //Возвращает количество активных задач у пользователя
        Task<int> CountActive(Guid userId);
        Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate);

    }
}
