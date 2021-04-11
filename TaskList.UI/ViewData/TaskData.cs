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
            
            var Mcol = new ObservableCollection<MainTask>();
            for (int i = 0; i < 15; i++)
            {
                var Scol = new ObservableCollection<SubTask>();
                for (var j = 0; j < 5; j++)
                {
                    Scol.Add(new SubTask($"SubTask #{j}"));
                }
                Mcol.Add(new MainTask($"MainTask #{i}", Scol));
            }
            MainTasks = Mcol;
        }

    }
}
