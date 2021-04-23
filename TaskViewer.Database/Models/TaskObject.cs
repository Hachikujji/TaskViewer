using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskList.Database.Models
{
    public class TaskObject
    {
        public MainTask Task { get; set; }
        public ObservableCollection<TaskObject> SubTasks { get; set; }

        public TaskObject(MainTask task, ObservableCollection<TaskObject> taskObjects)
        {
            Task = task;
            SubTasks = taskObjects;
        }
    }

}
