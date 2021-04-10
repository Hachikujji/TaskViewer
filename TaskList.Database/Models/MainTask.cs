using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskList.Database.Models
{
    public class MainTask
    {
        public string Name { get; set; }
        public ObservableCollection<SubTask> SubTasks { get; set; }

        public MainTask(string name, ObservableCollection<SubTask> subTasks)
        {
            Name = name;
            SubTasks = subTasks;
        }
    }

}
