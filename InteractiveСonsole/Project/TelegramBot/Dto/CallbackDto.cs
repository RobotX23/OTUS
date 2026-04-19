using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveСonsole.Project.TelegramBot.Dto
{
    internal class CallbackDto
    {
        public string Action { get; set; } = string.Empty;

        public static CallbackDto FromString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new CallbackDto { Action = string.Empty };

            var idx = input.IndexOf('|');
            if (idx < 0)
                return new CallbackDto { Action = input };
            var action = input.Substring(0, idx);
            return new CallbackDto { Action = action };
        }

        public override string ToString() => Action;
    }
}
