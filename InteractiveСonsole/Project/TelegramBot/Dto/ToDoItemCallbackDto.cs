using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveСonsole.Project.TelegramBot.Dto
{
    internal class ToDoItemCallbackDto : CallbackDto
    {
        public Guid? ToDoItemId { get; set; }
        public static new ToDoItemCallbackDto FromString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new ToDoItemCallbackDto { Action = string.Empty };

            if (input == "back")
                return new ToDoItemCallbackDto { Action = "back" };

            var parts = input.Split('|', StringSplitOptions.None);
            var dto = new ToDoItemCallbackDto { Action = parts[0] };

            if (parts.Length > 1 && Guid.TryParse(parts[1], out var id))
                dto.ToDoItemId = id;
            else
                dto.ToDoItemId = null;

            return dto;
        }

        public override string ToString()
        {
            if (Action == "back") return "back";
            return ToDoItemId == null ? base.ToString() : $"{base.ToString()}|{ToDoItemId}";
        }
    }
}
