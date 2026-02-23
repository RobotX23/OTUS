using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;

namespace InteractiveСonsole.Project.Infrastructure.DataAccess
{
    internal class FileToDORepository : IToDoRepository
    {

        private readonly string _baseFolder;

        public FileToDORepository (string baseFolder = "ToDOItem")
        {
            _baseFolder = baseFolder;
            Directory.CreateDirectory(_baseFolder);
        }

        /// <summary>
        /// Добавление задачи
        /// </summary>
        public async Task Add(ToDoItem item)
        {
            var filePath = GetFilePath(item.Id);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await JsonSerializer.SerializeAsync(stream, item);
            }
        }

        private string GetFilePath(Guid id)
        {
            return Path.Combine(_baseFolder, $"{id}.json");
        }

        /// <summary>
        /// Количество активных задач пользователя
        /// </summary>
        public async Task<int> CountActive(Guid userId)
        {
            var actevitem = await GetActiveByUserId(userId);
            return actevitem.Count;
        }

        public async Task Delete(Guid id)
        {
            var filePath = GetFilePath(id);
            if (filePath != null)
            {
                File.Delete(filePath);
            }
        }
        /// <summary>
        /// Проверка на дубликат
        /// </summary>
        public async Task<bool> ExistsByName(Guid userId, string name)
        {
            var item = await GetAllByUserId(userId);
            if (item.FirstOrDefault(x => x.Name == name && x.User.UserId == userId) != null)
            {
                return await Task.FromResult(true);
            }
            else
            {
                return await Task.FromResult(false);
            }
        }
        /// <summary>
        /// Список задач пользователя по условию предиката
        /// </summary>
        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate)
        {
            var item = await GetAllByUserId(userId);
            return item.Where(predicate).ToList().AsReadOnly();
        }


        /// <summary>
        /// Вывод задачи по id
        /// </summary>
        public async Task<ToDoItem?> Get(Guid id)
        {
            var filePath = GetFilePath(id);
            if (!File.Exists(filePath))
            {
                return null;
            }

            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                return await JsonSerializer.DeserializeAsync<ToDoItem>(stream); ;
            }
        }
        /// <summary>
        /// Вывод активных задач пользователя
        /// </summary>
        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId)
        {
            var items = await GetAllByUserId(userId);
            return items.Where(x => x.State == ToDoItemState.Active).ToList().AsReadOnly();
        }
        /// <summary>
        /// Получить список все задач
        /// </summary>
        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId)
        {
            var files = Directory.GetFiles(_baseFolder, "*.json");
            var items = new List<ToDoItem>();

            foreach (var file in files)
            {
                var item = await Get(Guid.Parse(Path.GetFileNameWithoutExtension(file)));

                if (item?.User.UserId == userId)
                {
                    items.Add(item);
                }
            }

            return items.AsReadOnly();
        }
        /// <summary>
        /// Изменение задачи
        /// </summary>
        public async Task Update(ToDoItem item)
        {
            var updateItem = await Get(item.Id);
            if (updateItem != null)
            {
                updateItem.State = item.State;
                updateItem.Name = item.Name;
                updateItem.StateChangeAt = item.StateChangeAt;
                updateItem.User = item.User;

            }
        }
    }
}
