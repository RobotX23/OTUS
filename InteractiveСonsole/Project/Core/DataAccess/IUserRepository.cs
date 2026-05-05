using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveСonsole
{
    internal interface IUserRepository
    {
        Task<ToDoUser?> GetUser(Guid userId, CancellationToken ct = default);
        Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken ct = default);
        Task Add(ToDoUser user, CancellationToken ct = default);
        Task<IReadOnlyList<ToDoUser>> GetUsers(CancellationToken ct);
    }
}
