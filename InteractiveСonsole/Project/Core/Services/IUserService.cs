using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveСonsole
{
    public interface IUserService
    {
        Task <ToDoUser> RegisterUser(long telegramUserId, string? telegrsmUserName);
        Task <ToDoUser?> GetUser(long telegramUserId);

    }
}
