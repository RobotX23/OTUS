using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace InteractiveСonsole.Project.Infrastructure.DataAccess
{
    internal class FileToDoRepository : IToDoRepository
    {

        private readonly string _baseFolder;
        private Dictionary<Guid, string> _index;

        public FileToDoRepository (string baseFolder = "ToDOItem")
        {
            _baseFolder = baseFolder;
            Directory.CreateDirectory(_baseFolder);
            LoadIndex();
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

            AddIndex(item.Id, item.User.UserId);
        }



        private async Task LoadIndex()
        {

            string jsonIndex = Directory.GetFiles(_baseFolder, "index.json", SearchOption.AllDirectories).FirstOrDefault();

            if (jsonIndex ==  null)
            {
                await IndexSerch();
            }
            else 
            {
                File.Delete($"{_baseFolder}//index.json");
                await IndexSerch();

            }
            
        }

        private async Task IndexSerch()
        {
            string[] jsonFile = Directory.GetFiles(_baseFolder, "*.json", SearchOption.AllDirectories);

            if (jsonFile.Length > 0)
            {
                foreach (var file1 in jsonFile)
                {

                    using (var stream = new FileStream(file1, FileMode.Open))
                    {
                        var index1 = await JsonSerializer.DeserializeAsync<ToDoItem>(stream);

                        AddIndex(index1.Id, index1.User.UserId);
                    }


                }
            }
        }

        /// <summary>
        /// Добавление связки ключ-значени ToDoItem = UserId
        /// </summary>
        public async Task AddIndex(Guid key, Guid value)
        {

            var inddex = new Dictionary<Guid, Guid>();


            var filePathUserId = Index();

            if (File.Exists(filePathUserId))
            {
                // Чтение содержимого файла
                string jsonString = File.ReadAllText(filePathUserId);

                // Десериализация JSON в Dictionary<Guid, Guid>
                inddex = JsonSerializer.Deserialize<Dictionary<Guid, Guid>>(jsonString);

            }
            else
            {
                inddex = new Dictionary<Guid, Guid>();
            }

            inddex[key] = value;

            string jsonString1 = JsonSerializer.Serialize(inddex, new JsonSerializerOptions { WriteIndented = true });

            // Запись JSON в файл
            File.WriteAllText(filePathUserId, jsonString1);


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
        /// Создание файла связки UserID Item
        /// </summary>
        private string Index()
        {
            return Path.Combine(_baseFolder, "index.json");
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
            var inddex = new Dictionary<Guid, Guid>();

            var filePathUserId = Index();

            if (File.Exists(filePathUserId))
            {
                // Чтение содержимого файла
                string jsonString = File.ReadAllText(filePathUserId);

                // Десериализация JSON в Dictionary<Guid, Guid>
                inddex = JsonSerializer.Deserialize<Dictionary<Guid, Guid>>(jsonString);

                var userId = inddex[id];
                string filePath = _baseFolder;
                string namefile = $"{id}.json";

                var file = $"{_baseFolder}//{userId}//{namefile}";

                if (file.Length > 0)
                {
                    File.Delete(file);
                }

                inddex.Remove(id);
                string jsonString1 = JsonSerializer.Serialize(inddex, new JsonSerializerOptions { WriteIndented = true });
                // Запись JSON в файл
                File.WriteAllText(filePathUserId, jsonString1);


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
            var inddex = new Dictionary<Guid, Guid>();

            var filePathUserId = Index();

            if (File.Exists(filePathUserId))
            {
                // Чтение содержимого файла
                string jsonString = File.ReadAllText(filePathUserId);

                // Десериализация JSON в Dictionary<Guid, Guid>
                inddex = JsonSerializer.Deserialize<Dictionary<Guid, Guid>>(jsonString);

                var userId = inddex[id];
                string filePath = _baseFolder;
                string namefile = $"{id}.json";

                var file = $"{_baseFolder}//{userId}//{namefile}";


                using (var stream = new FileStream(file, FileMode.Open))
                {
                    return await JsonSerializer.DeserializeAsync<ToDoItem>(stream); ;
                }
            }
            else
            {
                return null;
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
