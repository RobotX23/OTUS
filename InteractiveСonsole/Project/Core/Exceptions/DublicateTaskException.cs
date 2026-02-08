using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveСonsole.Project.Core.Exceptions
{
    public class DublicateTaskException : Exception
    {
        public DublicateTaskException(string task) : base($"Задача {task} уже существует.")
        {
        }

    }
}
