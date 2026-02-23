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
        public async Task<ToDoUser?> GetUser(long telegramUserId)
        {
            return await _userRepository.GetUserByTelegramUserId(telegramUserId);
        }
        /// <summary>
        /// Регистрация пользователя
        /// </summary>
        public async Task<ToDoUser> RegisterUser(long telegramUserId, string telegrsmUserName)
        {
            var existingUser = await _userRepository.GetUserByTelegramUserId(telegramUserId);
            if (existingUser != null)
            {
                return  existingUser;
            }

            var user = new ToDoUser(telegrsmUserName, telegramUserId);
            await _userRepository.Add(user);
            return await Task.FromResult(user);
            
        }
    }
}
