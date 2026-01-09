using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveСonsole
{
    internal class ToDoUser
    {
        public Guid UserId { get; }
        public string TelegramUserName { get; }
        public DateTime RegistereAt { get; }
        public long TelegramUserId { get; }

        public ToDoUser(string telegramUserName, long telegramUserId) 
        {
            TelegramUserName = telegramUserName;
            UserId = Guid.NewGuid();
            RegistereAt = DateTime.Now;
            TelegramUserId = telegramUserId;
        }
    }
}
