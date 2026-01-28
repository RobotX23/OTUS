using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveСonsole
{
    public class ToDoItem
    {
        public Guid Id { get; }
        public ToDoUser User { get; }
        public string Name { get; }
        public DateTime CreateAt { get; }
        public ToDoItemState State { get; set; }
        public DateTime? StateChangeAt { get; set; }

        public ToDoItem(ToDoUser user, string name) 
        {
            Id = Guid.NewGuid();
            User = user;
            Name = name;
            CreateAt = DateTime.UtcNow;
            State = ToDoItemState.Active;
            StateChangeAt = null;


        }
        public void ChangeState (ToDoItemState newStat)
        {
            State = newStat;
            StateChangeAt = DateTime.UtcNow;
        }
    }


    public enum ToDoItemState
    {
        Active,
        Completed
    }
}
