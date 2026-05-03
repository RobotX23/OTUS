using System;
using System.Threading;
using System.Threading.Tasks;

namespace InteractiveСonsole
{
    internal class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
            => _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

        public async Task<ToDoUser?> GetUser(long telegramUserId, CancellationToken ct = default)
            => await _userRepository.GetUserByTelegramUserId(telegramUserId, ct);

        public async Task<ToDoUser> RegisterUser(long telegramUserId, string? telegramUserName, CancellationToken ct = default)
        {
            var existing = await GetUser(telegramUserId, ct);
            if (existing != null) return existing;

            var user = new ToDoUser
            {
                UserId = Guid.NewGuid(),
                TelegramUserId = telegramUserId,
                TelegramUserName = telegramUserName,
                RegistereAt = DateTime.UtcNow
            };
            await _userRepository.Add(user, ct);
            return user;
        }
    }
}