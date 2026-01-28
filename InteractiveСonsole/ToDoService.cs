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
        private List<ToDoItem> taskes = new List<ToDoItem>();

        public int maxtasks;
        public int maxline;

        public ToDoService()
        {
            taskes = new List<ToDoItem>();
        }

        /// <summary>
        /// Метод добавление задачи
        /// </summary>
        public ToDoItem Add(ToDoUser user, string name)
        {
            var task = taskes.Where(x => x.User == user).ToList();

            if (task.Count > maxtasks - 1)
            {
                throw new TaskCountLimitException(maxtasks);
            }

            if (name.Length > maxline)
            {
                throw new TaskLengthLimitException(name.Length, maxline);
            }

            if (task.FirstOrDefault(x => x.Name == name) != null)
            {
                throw new DublicateTaskException(name);
            }

            else
            {
                taskes.Add(new ToDoItem(user, name));
                return taskes[taskes.Count-1];
            }
        }
        /// <summary>
        /// Метод удаления задач
        /// </summary>
        public void Delete(Guid id)
        {
            var zadacha = taskes.FirstOrDefault(x => x.Id == id);
            if (zadacha != null)
            {
                taskes.Remove(zadacha);
            }
            
        }
        /// <summary>
        /// Метод вывода активных задач
        /// </summary>
        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            var task = taskes.Where(x => x.User.UserId == userId && x.State == ToDoItemState.Active).OrderByDescending(x => x.Name).ToList();
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
            var task = taskes.Where(x => x.User.UserId == userId).OrderByDescending(x => x.Name).ToList();
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
            var zadacha = taskes.FirstOrDefault(x => x.Id == id);
            if (zadacha != null)
            {
                zadacha.ChangeState(ToDoItemState.Completed);
            }
        }
    }
}
