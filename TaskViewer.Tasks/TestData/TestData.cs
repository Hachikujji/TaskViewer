using Prism.Mvvm;
using System.Collections.ObjectModel;
using TaskViewer.Database.Models;
using TaskViewer.Tasks.Models;

namespace TaskViewer.Tasks.TestData
{
    public class TestData : BindableBase
    {
        private ObservableCollection<TaskObject> _tabTaskList;

        public ObservableCollection<TaskObject> TabTaskList
        {
            get => _tabTaskList;
            set => SetProperty(ref _tabTaskList, value);
        }

        public TestData()
        {
            var c2 = new ObservableCollection<TaskObject>();
            for (int i = 0; i < 20; i++)
            {
                c2.Add(new TaskObject(new Task(0, "Task #" + i.ToString(), 0), new ObservableCollection<TaskObject>()));
            }
            TabTaskList = new ObservableCollection<TaskObject>();
            TabTaskList.Add(new TaskObject(new Task(0, "Tasks", 0), c2));
        }
    }
}