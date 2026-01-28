using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveСonsole
{
    internal class ToDoReportService : IToDoReportService
    {
        private readonly IToDoRepository _toDoRepository;

        public ToDoReportService(IToDoRepository toDoRepository)
        {
            _toDoRepository = toDoRepository;
        }
        /// <summary>
        /// Метод вывода сервисного отчета
        /// </summary>
        public (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId)
        {
            int total = _toDoRepository.GetAllByUserId(userId).Count;
            int completed = _toDoRepository.GetAllByUserId(userId).Where(x=> x.State == ToDoItemState.Completed).ToList().Count;
            int active = _toDoRepository.GetActiveByUserId(userId).Count;
            DateTime generatedAt = DateTime.UtcNow;
            return (total, completed, active, generatedAt);


        }
    }
}
