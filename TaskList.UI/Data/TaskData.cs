using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskList.Database.Models;

namespace TaskList.UI.Data
{
    public class TaskData
    {
        public ObservableCollection<MainTask> MainTasks { get; set; }

        public TaskData()
        {
            var Scol = new ObservableCollection<SubTask>();
            for (var i = 0; i < 10; i++)
            {
                Scol.Add(new SubTask($"{i}"));
            }
            var Mcol = new ObservableCollection<MainTask>();
            for (int i = 0; i < 10; i++)
            {
                Mcol.Add(new MainTask($"{i}", Scol));
            }
            MainTasks = Mcol;
        }

    }
}
