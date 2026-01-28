using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveСonsole
{
    internal class InMemoryUserRepository : IUserRepository
    {
        private readonly List<ToDoUser> _user = new List<ToDoUser>();
        public void Add(ToDoUser user)
        {
            _user.Add(user);
        }

        public ToDoUser? GetUser(Guid userId)
        {
            return _user.FirstOrDefault(x => x.UserId == userId);
        }

        public ToDoUser? GetUserByTelegramUserId(long telegramUserId)
        {
            return _user.FirstOrDefault(user => user.TelegramUserId == telegramUserId);
        }
    }
}
