using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using TaskViewer.Database.Models;
using TaskViewer.Tasks.Models;
using TaskViewer.Tasks.Resources;

namespace TaskViewer.Tasks.TestData
{
    public class TestViewData
    {
        public ObservableCollection<TaskObject> TabControlTabs { get; set; }

        public TestViewData()
        {
            TabControlTabs = new ObservableCollection<TaskObject>();
            TabControlTabs.Add(new TaskObject(new Task(0, 0, languages.AllTasks, DateTime.Now), new ObservableCollection<TaskObject>()));
            TabControlTabs.Add(new TaskObject(new Task(0, 0, languages.InProgress, DateTime.Now), new ObservableCollection<TaskObject>()));
            TabControlTabs.Add(new TaskObject(new Task(0, 0, languages.Completed, DateTime.Now), new ObservableCollection<TaskObject>()));
            TabControlTabs[0].SubTasks.Add(new TaskObject(new Task(0, 0, "test", DateTime.Now, DateTime.Now.AddDays(1)), new ObservableCollection<TaskObject>()));
            TabControlTabs[0].SubTasks.Add(new TaskObject(new Task(0, 0, "test", DateTime.Now, DateTime.Now.AddDays(1)), new ObservableCollection<TaskObject>()));
        }
    }
}