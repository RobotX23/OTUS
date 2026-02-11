using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveСonsole
{
    internal interface IUserRepository
    {
        Task<ToDoUser?> GetUser(Guid userId);
        Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId);
        Task Add(ToDoUser user);
    }
}
