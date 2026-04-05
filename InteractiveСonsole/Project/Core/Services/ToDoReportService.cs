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
        public async Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStats(Guid userId, CancellationToken ct = default)
        {

            var allItem = await _toDoRepository.GetAllByUserId(userId, ct);
            int total = allItem.Count;
            int completed = allItem.Where(x=> x.State == ToDoItemState.Completed).ToList().Count;
            int active = allItem.Where(x=> x.State == ToDoItemState.Active).ToList().Count;
            DateTime generatedAt = DateTime.UtcNow;
            return (total, completed, active, generatedAt);


        }
    }
}
