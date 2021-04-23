using System.Collections.ObjectModel;
using TaskViewer.Database.Models;

namespace TaskViewer.Tasks.Models
{
    public class TaskObject
    {
        public Task Task { get; set; }

        public ObservableCollection<TaskObject> SubTasks { get; set; }

        public TaskObject(Task task, ObservableCollection<TaskObject> taskObjects)
        {
            Task = task;
            SubTasks = taskObjects;
        }
    }
}