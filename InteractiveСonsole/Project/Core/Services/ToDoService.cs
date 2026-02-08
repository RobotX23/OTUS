using InteractiveСonsole.Project.Core.Exceptions;

namespace InteractiveСonsole
{
    internal class ToDoService : IToDoService
    {
        private readonly  IToDoRepository _toDoRepository;
        public int maxtasks {  get; set; }
        public int maxline { get; set; }

        public ToDoService(IToDoRepository toDoRepository, int maxtasks, int maxline)
        {
            _toDoRepository = toDoRepository;
            this.maxtasks = maxtasks;
            this.maxline = maxline;
        }


        public async Task<(int, int)> LineTasks()
        {
            return (maxtasks, maxline);
        }



        /// <summary>
        /// Метод добавление задачи
        /// </summary>
        public async Task<ToDoItem> Add(ToDoUser user, string name)
        {
            var task = await _toDoRepository.GetAllByUserId(user.UserId);

            if (task.Count > maxtasks - 1)
            {
                throw new TaskCountLimitException(maxtasks);
            }

            if (name.Length > maxline)
            {
                throw new TaskLengthLimitException(name.Length, maxline);
            }

            if (await _toDoRepository.ExistsByName(user.UserId, name))
            {
                throw new DublicateTaskException(name);
            }

            else
            {
                var newTask = new ToDoItem(user, name);
                await _toDoRepository.Add(newTask);
                return newTask;
            }
        }
        /// <summary>
        /// Метод удаления задач
        /// </summary>
        public async Task Delete(Guid id)
        {
            await _toDoRepository.Delete(id);
            
        }
        /// <summary>
        /// Метод вывода активных задач
        /// </summary>
        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId)
        {
            var task = await _toDoRepository.GetActiveByUserId(userId);
            if (task.Count == 0)
            {
                return null;
            }
            else
            {
                return task;
            }
        }
        /// <summary>
        /// Метод вывода всех задач
        /// </summary>
        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId)
        {
            var task = await _toDoRepository.GetAllByUserId(userId);
            if (task.Count == 0)
            {
                return null;
            }
            else
            {
                return task;
            }
        }
        /// <summary>
        /// Завершение задачи
        /// </summary>
        public async Task MarkCompleted(Guid id)
        {
            var zadacha = await _toDoRepository.Get(id);
            if (zadacha != null)
            {
                zadacha.ChangeState(ToDoItemState.Completed);
            }
        }
        /// <summary>
        /// Вывод задач по части слова
        /// </summary>
        public async Task<IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix)
        {
            return await _toDoRepository.Find(user.UserId, x => x.Name.StartsWith(namePrefix));
        }
    }
}
