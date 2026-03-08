using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveСonsole
{
    public class ToDoUser
    {
        public Guid UserId { get; set; }
        public string TelegramUserName { get; set; }
        public DateTime RegistereAt { get; set; }
        public long TelegramUserId { get; set; }

        public void ToDoUserNew(string telegramUserName, long telegramUserId) 
        {
            TelegramUserName = telegramUserName;
            UserId = Guid.NewGuid();
            RegistereAt = DateTime.Now;
            TelegramUserId = telegramUserId;
        }
    }
}
