using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveСonsole.Project.TelegramBot.Dto
{
    internal class PagedListCallbackDto : ToDoListCallbackDto
    {
        public int Page { get; set; }

        public PagedListCallbackDto() : base() { }

        public PagedListCallbackDto(string action, Guid? toDoListId, int page) : base()
        {
            Action = action;
            ToDoListId = toDoListId;
            Page = page;
        }

        public static new PagedListCallbackDto FromString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new PagedListCallbackDto { Action = string.Empty, Page = 0 };

            var parts = input.Split('|', StringSplitOptions.None);
            var dto = new PagedListCallbackDto { Action = parts[0], Page = 0 };

            // Парсим ToDoListId (второй элемент)
            if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]) && Guid.TryParse(parts[1], out var listId))
                dto.ToDoListId = listId;

            // Парсим Page (третий элемент)
            if (parts.Length > 2 && int.TryParse(parts[2], out var page))
                dto.Page = page;

            return dto;
        }

        public override string ToString()
        {
            var baseStr = base.ToString(); // "Action" или "Action|ListId"
            return $"{baseStr}|{Page}";
        }
    }
}