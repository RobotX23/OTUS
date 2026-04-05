using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveСonsole
{
    internal interface IToDoReportService
    {
        //Вывод сервисного отчёта
        Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStats(Guid userId);
    }
}
