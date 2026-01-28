using Microsoft.VisualBasic;
using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveСonsole
{
    internal class ToDoService : IToDoService
    {
        private readonly  IToDoRepository _toDoRepository;

        public ToDoService(IToDoRepository toDoRepository)
        {
            _toDoRepository = toDoRepository;
        }

        public int maxtasks;
        public int maxline;

        /// <summary>
        /// Метод добавление задачи
        /// </summary>
        public ToDoItem Add(ToDoUser user, string name)
        {
            var task = _toDoRepository.GetAllByUserId(user.UserId);

            if (task.Count > maxtasks - 1)
            {
                throw new TaskCountLimitException(maxtasks);
            }

            if (name.Length > maxline)
            {
                throw new TaskLengthLimitException(name.Length, maxline);
            }

            if (_toDoRepository.ExistsByName(user.UserId, name))
            {
                throw new DublicateTaskException(name);
            }

            else
            {
                var newTask = new ToDoItem(user, name);
                _toDoRepository.Add(newTask);
                return newTask;
            }
        }
        /// <summary>
        /// Метод удаления задач
        /// </summary>
        public void Delete(Guid id)
        {
            _toDoRepository.Delete(id);
            
        }
        /// <summary>
        /// Метод вывода активных задач
        /// </summary>
        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            var task = _toDoRepository.GetActiveByUserId(userId);
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
        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            var task = _toDoRepository.GetAllByUserId(userId);
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
        public void MarkCompleted(Guid id)
        {
            var zadacha = _toDoRepository.Get(id);
            if (zadacha != null)
            {
                zadacha.ChangeState(ToDoItemState.Completed);
            }
        }
    }
}
