using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveСonsole
{
    internal interface IUserRepository
    {
        ToDoUser? GetUser(Guid userId);
        ToDoUser? GetUserByTelegramUserId(long telegramUserId);
        void Add(ToDoUser user);
    }
}
