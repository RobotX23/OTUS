using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveСonsole.Project.Core.DataAccess
{
    internal interface IToDoListRepository
    {
        //Если списка нет, то возвращаем null
        Task<ToDoList?> Get(Guid id, CancellationToken ct);
        Task<IReadOnlyList<ToDoList>>GetByUserId(Guid userId, CancellationToken ct);
        Task Add(ToDoList list, CancellationToken ct);
        Task Delete(Guid id, CancellationToken ct);
        //Проверяем, если лм у пользователя список с таким именем
        Task<bool>ExistsByName(Guid userId, string name, CancellationToken ct);
    }
}
