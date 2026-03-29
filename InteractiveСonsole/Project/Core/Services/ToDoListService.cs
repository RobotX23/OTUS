using InteractiveСonsole.Project.Core.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveСonsole.Project.Core.Services
{
    internal class ToDoListService : IToDoListService
    {
        private readonly IToDoListRepository _repo;

        public ToDoListService(IToDoListRepository repo)
        {
            _repo = repo;
        }
        public async Task<ToDoList> Add(ToDoUser user, string name, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Требуется имя", nameof(name));
            if (name.Length > 10) throw new ArgumentException("Размер имени списка превышает 10.", nameof(name));

            if (await _repo.ExistsByName(user.UserId, name, ct))
                throw new InvalidOperationException("У пользователя уже есть список с таким имененм.");

            var list = new ToDoList
            {
                Id = Guid.NewGuid(),
                User = user,
                Name = name,
                CreateAt = DateTime.UtcNow
            };

            await _repo.Add(list, ct);
            return list;
        }

        public Task<ToDoList?> Get(Guid id, CancellationToken ct) => _repo.Get(id, ct);

        public Task Delete(Guid id, CancellationToken ct) => _repo.Delete(id, ct);

        public Task<IReadOnlyList<ToDoList>> GetUserLists(Guid userId, CancellationToken ct) =>
            _repo.GetByUserId(userId, ct);
    }
}
