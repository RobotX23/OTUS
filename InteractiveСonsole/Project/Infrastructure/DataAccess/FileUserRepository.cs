using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Telegram.Bot.Types;


namespace InteractiveСonsole.Project.Infrastructure.DataAccess
{
    internal class FileUserRepository:IUserRepository
    {
        private readonly string _baseFolder;
        private string GetFilePath(Guid id)
        {
            return Path.Combine(_baseFolder, $"{id}.json");
        }

        public FileUserRepository(string baseFolder = "ToDoUser")
        {
            _baseFolder = baseFolder;
            Directory.CreateDirectory(_baseFolder);
        }
        public async Task Add(ToDoUser user, CancellationToken ct = default)
        {

            var filePath = GetFilePath(user.UserId);
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await JsonSerializer.SerializeAsync(stream, user, cancellationToken: ct);
            }

        }

        public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken ct = default)
        {
            var filePath = GetFilePath(userId);
            if (!File.Exists(filePath))
            {
                return null;
            }

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            {
                return await JsonSerializer.DeserializeAsync<ToDoUser?>(stream, cancellationToken: ct);
            }
        }

        public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken ct = default)
        {
            var files = Directory.GetFiles(_baseFolder, "*.json");

            ToDoUser? user = null;

            foreach (var file in files)
            {
                user = await GetUser(Guid.Parse(Path.GetFileNameWithoutExtension(file)), ct);

                if (user?.TelegramUserId == telegramUserId)
                {
                    break;
                }
            }

            return user;

        }
    }
}
