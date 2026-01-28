using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveСonsole
{
    public interface IUserService
    {
        ToDoUser RegisterUser(long telegramUserId, string telegrsmUserName);
        ToDoUser? GetUser(long telegramUserId);

    }
}
