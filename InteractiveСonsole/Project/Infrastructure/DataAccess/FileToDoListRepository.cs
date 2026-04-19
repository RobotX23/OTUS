using InteractiveСonsole.Project.Core.DataAccess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Telegram.Bot.Types;


namespace InteractiveСonsole.Project.Infrastructure.DataAccess
{
    internal class FileToDoListRepository : IToDoListRepository
    {
        private readonly string _baseFolder;

        private string GetFilePath(Guid? id) => Path.Combine(_baseFolder, $"{id}.json");

        public FileToDoListRepository(string baseFolder = "ToDoLists")
        {
            _baseFolder = baseFolder;
            Directory.CreateDirectory(_baseFolder);
        }

        public async Task Add(ToDoList list, CancellationToken ct)
        {
            var filePath = GetFilePath(list.Id);
            using var stream = new FileStream(filePath, FileMode.Create);
            await JsonSerializer.SerializeAsync(stream, list, cancellationToken: ct);
        }

        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            var filePath = GetFilePath(id);
            if (!File.Exists(filePath)) return null;
            using var stream = new FileStream(filePath, FileMode.Open);
            return await JsonSerializer.DeserializeAsync<ToDoList?>(stream, cancellationToken: ct);
        }

        public async Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            var files = Directory.GetFiles(_baseFolder, "*.json");
            var lists = new List<ToDoList>();
            foreach (var file in files)
            {
                ct.ThrowIfCancellationRequested();
                var id = Guid.Parse(Path.GetFileNameWithoutExtension(file));
                var list = await Get(id, ct);
                if (list != null && list.User.UserId == userId) lists.Add(list);
            }
            return lists;
        }

        public Task Delete(Guid id, CancellationToken ct)
        {
            var filePath = GetFilePath(id);
            if (File.Exists(filePath)) File.Delete(filePath);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            var lists = await GetByUserId(userId, ct);
            return lists.Any(l => string.Equals(l.Name, name, StringComparison.OrdinalIgnoreCase));
        }

    }
}
