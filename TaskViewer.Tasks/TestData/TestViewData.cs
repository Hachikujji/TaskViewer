using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using TaskViewer.Database.Models;
using TaskViewer.Tasks.Models;

namespace TaskViewer.Tasks.TestData
{
    public class TestViewData
    {
        public ObservableCollection<TaskObject> TabTaskList { get; set; }

        public TestViewData()
        {
            TabTaskList = new ObservableCollection<TaskObject>();
            TabTaskList.Add(new TaskObject(new Task(0, "🏠", 0), new ObservableCollection<TaskObject>()));
            for (int i = 0; i < 20; i++)
            {
                TabTaskList[0].SubTasks.Add(new TaskObject(new Task(0, i.ToString(), 0), new ObservableCollection<TaskObject>()));
            }
        }
    }
}