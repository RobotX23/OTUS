using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using Telegram.Bot.Types;

namespace InteractiveСonsole.Project.Infrastructure.DataAccess
{
    internal class FileToDoRepository : IToDoRepository
    {

        private readonly string _baseFolder;

        public FileToDoRepository (string baseFolder = "ToDOItem")
        {
            _baseFolder = baseFolder;
            Directory.CreateDirectory(_baseFolder);
        }


        /// <summary>
        /// Добавление задачи
        /// </summary>
        public async Task Add(ToDoItem item)
        {
            var filePathUserId = GetFilePathUserId(item.User.UserId ,item.Id);
            using (var stream = new FileStream(filePathUserId, FileMode.Create))
            {
                await JsonSerializer.SerializeAsync(stream, item);
            }
        }

        /// <summary>
        /// Создание файла связки UserID Item
        /// </summary>
        private string GetFilePathUserId(Guid userId, Guid id)
        {
            string filepath = $"{_baseFolder}\\{userId}";
            return Path.Combine(filepath, $"{id}.json");
        }


        /// <summary>
        /// Количество активных задач пользователя
        /// </summary>
        public async Task<int> CountActive(Guid userId)
        {
            var actevitem = await GetActiveByUserId(userId);
            return actevitem.Count;
        }


        /// <summary>
        /// Удаление задачи
        /// </summary>
        public async Task Delete(Guid id)
        {
            string filePath = _baseFolder;
            string namefile = $"{id}.json";

            var file = Directory.GetFiles(filePath, namefile, SearchOption.AllDirectories);

            if ( file.Length > 0)
            {
                foreach(var fail in file)
                {
                    File.Delete(fail);
                }
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
            string filePath = _baseFolder;
            string namefile = $"{id}.json";

            var file = Directory.GetFiles(filePath, namefile, SearchOption.AllDirectories).FirstOrDefault();
            if (!File.Exists(file))
            {
                return null;
            }

            using (var stream = new FileStream(file, FileMode.Open))
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
            string pathFile = $"{_baseFolder}\\{userId}";
            DirectoryInfo di = Directory.CreateDirectory(pathFile);
            var files = Directory.GetFiles(pathFile, "*.json");
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
            var filePathUserId = GetFilePathUserId(item.User.UserId, item.Id);
            using (var stream = new FileStream(filePathUserId, FileMode.Create))
            {
                await JsonSerializer.SerializeAsync(stream, item);
            }
        }
    }
}
