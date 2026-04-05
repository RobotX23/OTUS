using InteractiveСonsole.Project.Core.Exceptions;

namespace InteractiveСonsole
{
    internal class ToDoService : IToDoService
    {
        private readonly  IToDoRepository _toDoRepository;
        private int maxtasks;
        private int maxline;

        public ToDoService(IToDoRepository toDoRepository, int maxtasks, int maxline)
        {
            _toDoRepository = toDoRepository;
            this.maxtasks = maxtasks;
            this.maxline = maxline;
        }

        public async Task SetLimits(int maxTasks, int maxLine, CancellationToken ct = default)
        {
            await Task.CompletedTask;
            maxtasks = maxTasks;
            maxline = maxLine;
        }


        public async Task<(int, int)> LineTasks()
        {
            return await Task.FromResult( (maxtasks, maxline));
        }



        /// <summary>
        /// Метод добавление задачи
        /// </summary>
        public async Task<ToDoItem> Add(ToDoUser user, string name, DateTime deadline, ToDoList? list, CancellationToken ct = default)
        {
            var task = await _toDoRepository.GetAllByUserId(user.UserId, ct);

            if (task.Count > maxtasks - 1)
            {
                throw new TaskCountLimitException(maxtasks);
            }

            if (name.Length > maxline)
            {
                throw new TaskLengthLimitException(name.Length, maxline);
            }

            if (await _toDoRepository.ExistsByName(user.UserId, name, ct))
            {
                throw new DublicateTaskException(name);
            }

            var newTask = new ToDoItem(user, name, deadline, list);
            await _toDoRepository.Add(newTask, ct);
            return newTask;
        }
        /// <summary>
        /// Метод удаления задач
        /// </summary>
        public async Task Delete(Guid id, CancellationToken ct = default)
        {
            await _toDoRepository.Delete(id, ct);
        }
        /// <summary>
        /// Метод вывода активных задач
        /// </summary>
        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct = default)
        {
            var task = await _toDoRepository.GetActiveByUserId(userId, ct);
            if (task.Count == 0)
            {
                return null;
            }
            return task;
        }
        /// <summary>
        /// Метод вывода всех задач
        /// </summary>
        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct = default)
        {
            var task = await _toDoRepository.GetAllByUserId(userId, ct);
            if (task.Count == 0)
            {
                return null;
            }
            return task;
        }
        /// <summary>
        /// Завершение задачи
        /// </summary>
        public async Task MarkCompleted(Guid id, CancellationToken ct = default)
        {
            var zadacha = await _toDoRepository.Get(id, ct);
            if (zadacha != null)
            {
                zadacha.ChangeState(ToDoItemState.Completed);
                await _toDoRepository.Update(zadacha, ct);
            }
        }
        /// <summary>
        /// Вывод задач по части слова
        /// </summary>
        public async Task<IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix, CancellationToken ct = default)
        {
            return await _toDoRepository.Find(user.UserId, x => x.Name.StartsWith(namePrefix), ct);
        }

        public async Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct)
        {
            var items = await _toDoRepository.GetAllByUserId(userId, ct);
            if (items == null || items.Count == 0)
                return Array.Empty<ToDoItem>();

            if (listId == null)
                return items.Where(i => i.List == null).ToList();

            return items.Where(i => i.List != null && i.List.Id == listId).ToList();
        }
    }
}
