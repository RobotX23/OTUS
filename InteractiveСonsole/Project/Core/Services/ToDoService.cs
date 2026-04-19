using InteractiveСonsole.Project.Core.Exceptions;

namespace InteractiveСonsole
{
    internal class ToDoService : IToDoService
    {
        private readonly IToDoRepository _toDoRepository;
        private readonly int _maxTasks;
        private readonly int _maxLine;

        public ToDoService(IToDoRepository toDoRepository, int maxTasks, int maxLine)
        {
            _toDoRepository = toDoRepository;
            _maxTasks = maxTasks;
            _maxLine = maxLine;
        }

        /// <summary>
        /// Метод добавление задачи
        /// </summary>
        public async Task<ToDoItem> Add(ToDoUser user, string name, DateTime deadline, ToDoList? list, CancellationToken ct = default)
        {
            var task = await _toDoRepository.GetAllByUserId(user.UserId, ct);

            if (task.Count > _maxTasks - 1)
            {
                throw new TaskCountLimitException(_maxTasks);
            }

            if (name.Length > _maxLine)
            {
                throw new TaskLengthLimitException(name.Length, _maxLine);
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
