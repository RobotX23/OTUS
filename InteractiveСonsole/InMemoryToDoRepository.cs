using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveСonsole
{
    internal class InMemoryToDoRepository : IToDoRepository
    {
        private readonly  List<ToDoItem> toDoItems = new List<ToDoItem>();
        /// <summary>
        /// Удаление задачи
        /// </summary>
        public void Add(ToDoItem item)
        {
            toDoItems.Add(item);
        }
        /// <summary>
        /// Количество активных задач пользователя
        /// </summary>
        public int CountActive(Guid userId)
        {
            return toDoItems.Where(x=> x.User.UserId == userId).Count();
        }

        public void Delete(Guid id)
        {
            var zadacha = Get(id);
            if (zadacha != null)
            {
                toDoItems.Remove(zadacha);
            }
        }
        /// <summary>
        /// Проверка на дубликат
        /// </summary>
        public bool ExistsByName(Guid userId, string name)
        {
            if (toDoItems.FirstOrDefault(x => x.Name == name && x.User.UserId == userId) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Список задач пользователя по условию предиката
        /// </summary>
        public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
        {
            return toDoItems.Where(x=> x.User.UserId == userId && predicate(x)).ToList();
        }


        /// <summary>
        /// Вывод задачи по id
        /// </summary>
        public ToDoItem? Get(Guid id)
        {
            return toDoItems.FirstOrDefault(x=> x.Id == id);
        }
        /// <summary>
        /// Вывод активных задач пользователя
        /// </summary>
        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return toDoItems.Where(x=> x.User.UserId == userId && x.State == ToDoItemState.Active).ToList();
        }
        /// <summary>
        /// Получить список все задач
        /// </summary>
        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            var task = toDoItems.Where(x => x.User.UserId == userId).ToList();
            return task;
        }
        /// <summary>
        /// Изменение задачи
        /// </summary>
        public void Update(ToDoItem item)
        {
            var updateItem = Get(item.Id);
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
