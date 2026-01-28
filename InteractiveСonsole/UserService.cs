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
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository) 
        {
            _userRepository = userRepository ?? throw new ArgumentException(nameof(userRepository));
        }
        /// <summary>
        /// Поиск существующего пользователя
        /// </summary>
        public ToDoUser? GetUser(long telegramUserId)
        {
            return _userRepository.GetUserByTelegramUserId(telegramUserId);
        }
        /// <summary>
        /// Регистрация пользователя
        /// </summary>
        public ToDoUser RegisterUser(long telegramUserId, string telegrsmUserName)
        {
            var existingUser = _userRepository.GetUserByTelegramUserId(telegramUserId);
            if (existingUser != null)
            {
                return existingUser;
            }

            var user = new ToDoUser(telegrsmUserName, telegramUserId);
            _userRepository.Add(user);
            return user;
            
        }
    }
}
