using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace InteractiveСonsole
{
    internal class UserService : IUserService
    {
        private List<ToDoUser> users = new List<ToDoUser>();

        public UserService() 
        {
            users = new List<ToDoUser>();
        }
        public ToDoUser? GetUser(long telegramUserId)
        {
            foreach (ToDoUser namers in users)
            {
                if (namers.TelegramUserId == telegramUserId)
                {
                    return namers;
                }
            }
            return null;
        }

        public ToDoUser RegisterUser(long telegramUserId, string telegrsmUserName)
        {
            var user = new ToDoUser(telegrsmUserName, telegramUserId);
            users.Add(user);
            return user;
            
        }
    }
}
