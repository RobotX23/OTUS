using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveСonsole.Project.TelegramBot.Dto
{
    internal class ToDoListCallbackDto : CallbackDto
    {
        public Guid? ToDoListId { get; set; }

        public static new ToDoListCallbackDto FromString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new ToDoListCallbackDto { Action = string.Empty };

            var parts = input.Split('|', StringSplitOptions.None);
            var dto = new ToDoListCallbackDto { Action = parts[0] };

            if (parts.Length > 1 && Guid.TryParse(parts[1], out var id))
                dto.ToDoListId = id;
            else
                dto.ToDoListId = null;

            return dto;
        }

        public override string ToString()
        {
            return ToDoListId == null ? base.ToString() : $"{base.ToString()}|{ToDoListId}";
        }
    }
}
