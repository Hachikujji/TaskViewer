using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaskViewer.Database.Models;
using TaskViewer.Database.Services;
using TaskViewer.Tasks.Models;

namespace TaskViewer.Tasks.ViewModels
{
    public class TasksWindowViewModel : BindableBase
    {
        #region Private Fields

        private IDatabaseService _databaseService = new DatabaseService();
        private int _currentUserId;
        private int _headerSelectedTabIndex;
        private int _selectedListItemIndex;
        private int _selectedTabItemIndex;
        private List<Task> _tasklist = new List<Task>();
        private ObservableCollection<TaskObject> _tabTaskList;
        private string _addTaskName;
        private string _password;
        private string _username;

        #endregion Private Fields

        #region Public Constructors

        public TasksWindowViewModel()
        {
            TestButtonEvent = new DelegateCommand(TestButton);
            OpenSubTaskTabEvent = new DelegateCommand(OpenSubTaskTab);
            UpdateTaskAfterEditingEvent = new DelegateCommand(UpdateTaskAfterEditing);
            AddTaskEvent = new DelegateCommand(AddTask);
            DeleteTaskEvent = new DelegateCommand(DeleteTask);
            DeleteTaskTabEvent = new DelegateCommand(DeleteTaskTab);
            LogInButtonEvent = new DelegateCommand(LogInButton);
            RegistrationButtonEvent = new DelegateCommand(RegistrationButton);
            SelectedTabItemIndex = -1;
            SelectedListItemIndex = -1;
        }

        #endregion Public Constructors

        #region Public Properties

        public DelegateCommand AddTaskEvent { get; }

        public string AddTaskName
        {
            get => _addTaskName;
            set => SetProperty(ref _addTaskName, value);
        }

        public DelegateCommand DeleteTaskEvent { get; }
        public DelegateCommand DeleteTaskTabEvent { get; }

        public int HeaderSelectedTabIndex
        {
            get => _headerSelectedTabIndex;
            set => SetProperty(ref _headerSelectedTabIndex, value);
        }

        public DelegateCommand LogInButtonEvent { get; }
        public DelegateCommand OpenSubTaskTabEvent { get; }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public DelegateCommand RegistrationButtonEvent { get; }

        public int SelectedListItemIndex
        {
            get => _selectedListItemIndex;
            set => SetProperty(ref _selectedListItemIndex, value);
        }

        public int SelectedTabItemIndex
        {
            get => _selectedTabItemIndex;
            set => SetProperty(ref _selectedTabItemIndex, value);
        }

        public ObservableCollection<TaskObject> TabTaskList
        {
            get => _tabTaskList;
            set => SetProperty(ref _tabTaskList, value);
        }

        public DelegateCommand TestButtonEvent { get; }
        public DelegateCommand UpdateTaskAfterEditingEvent { get; }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        #endregion Public Properties

        #region Private Methods

        private void AddTask()
        {
            if (SelectedTabItemIndex >= 0)
            {
                var task = new Task(_currentUserId, AddTaskName, TabTaskList[SelectedTabItemIndex].Task.Id);
                _databaseService.AddTaskAsync(task);
                _tasklist.Add(task);
                TabTaskList[SelectedTabItemIndex].SubTasks.Add(new TaskObject(task, new ObservableCollection<TaskObject>()));
            }
        }

        private void CloseAllSubTabs()
        {
            for (int i = TabTaskList.Count - 1; i > 0; i--)
            {
                TabTaskList.RemoveAt(i);
            }
        }

        private void ConvertTaskListToTaskObjectObservableCol(ref List<Task> taskList, ObservableCollection<TaskObject> taskObjects, int mainTaskId = 0)
        {
            foreach (var item in taskList)
            {
                if (item.MainTaskId == mainTaskId)
                    taskObjects.Add(new TaskObject(item, new ObservableCollection<TaskObject>()));
            }
            foreach (var item in taskObjects)
            {
                ConvertTaskListToTaskObjectObservableCol(ref taskList, item.SubTasks, item.Task.Id);
            }
        }

        private void CreateTasksTabTemplate()
        {
            TabTaskList = new ObservableCollection<TaskObject>();
            TabTaskList.Add(new TaskObject(new Task(_currentUserId, "Tasks", 0), new ObservableCollection<TaskObject>()));
        }

        private void DeleteAllTaskRoot(ref List<Task> taskList, Task task)
        {
            foreach (var item in taskList)
            {
                if (item.MainTaskId == task.Id)
                {
                    DeleteAllTaskRoot(ref taskList, item);
                    _databaseService.RemoveTaskAsync(item);
                    DeleteRefTabs(item);
                }
            }
        }

        private void DeleteRefTabs(Task task)
        {
            List<TaskObject> deleteTabList = new List<TaskObject>();
            foreach (var tab in TabTaskList)
            {
                if (task.Id == tab.Task.Id)
                {
                    deleteTabList.Add(tab);
                }
            }
            foreach (var delTask in deleteTabList)
                TabTaskList.Remove(delTask);
        }

        private void DeleteTask()
        {
            if (SelectedListItemIndex >= 0 && SelectedTabItemIndex >= 0)
            {
                var task = TabTaskList[SelectedTabItemIndex].SubTasks[SelectedListItemIndex].Task;
                DeleteAllTaskRoot(ref _tasklist, task);
                _databaseService.RemoveTaskAsync(task);
                TabTaskList[SelectedTabItemIndex].SubTasks.RemoveAt(SelectedListItemIndex);
                DeleteRefTabs(task);
            }
        }

        private void DeleteTaskTab()
        {
            if (SelectedTabItemIndex >= 1)
            {
                TabTaskList.RemoveAt(SelectedTabItemIndex);
            }
        }

        private void LogInButton()
        {
            int id;
            if ((id = _databaseService.IsUserPasswordCorrect(Username, Password)) != -1)
            {
                CreateTasksTabTemplate();
                _currentUserId = id;
                HeaderSelectedTabIndex = 1;
                Password = "";
                _tasklist = _databaseService.GetTaskListAsync(_currentUserId).Result;
                ConvertTaskListToTaskObjectObservableCol(ref _tasklist, TabTaskList);
                CloseAllSubTabs();
            }
        }

        private void OpenSubTaskTab()
        {
            if (SelectedListItemIndex >= 0 && SelectedTabItemIndex >= 0)
            {
                TabTaskList.Add(TabTaskList[SelectedTabItemIndex].SubTasks[SelectedListItemIndex]);
            }
        }

        private void RegistrationButton()
        {
            if (_databaseService.IsUserExists(Username) == false)
            {
                CreateTasksTabTemplate();
                var user = new User(Username, Password);
                _databaseService.AddUserAsync(user);
                HeaderSelectedTabIndex = 1;
                Password = "";
                _currentUserId = user.Id;
                _tasklist = _databaseService.GetTaskListAsync(_currentUserId).Result;
                ConvertTaskListToTaskObjectObservableCol(ref _tasklist, TabTaskList);
                CloseAllSubTabs();
            }
        }

        private void TestButton()
        {
            System.Console.WriteLine("TEST BUTTON");
        }

        private void UpdateTaskAfterEditing()
        {
            _databaseService.UpdateTaskAsync(TabTaskList[SelectedTabItemIndex].SubTasks[SelectedListItemIndex].Task);
        }

        #endregion Private Methods
    }
}