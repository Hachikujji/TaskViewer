using Microsoft.Xaml.Behaviors;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using TaskViewer.Database.Models;
using TaskViewer.Database.Services;
using TaskViewer.Tasks.Models;
using TaskViewer.Tasks.Resources;
using TaskViewer.Tasks.Views;
using Timer = System.Timers.Timer;

namespace TaskViewer.Tasks.ViewModels
{
    public class TasksWindowViewModel : BindableBase
    {
        #region Private Fields

        private IDatabaseService _databaseService;
        private int _currentUserId;
        private int _headerSelectedTabIndex;
        private int _selectedListItemIndex;
        private int _selectedTabItemIndex;
        private List<Task> _tasklist = new List<Task>();
        private ObservableCollection<TaskObject> _tabTaskList;
        private string _addTaskName;
        private string _username;
        private string _authorizationErrorLog;
        private Visibility _authorizationErrorLogVisibility;
        private Timer _authorizationErrorLogTimer;
        private Dictionary<string, CultureInfo> _languages;
        private KeyValuePair<string, CultureInfo> _selectedLanguage;

        #endregion Private Fields

        #region Public Constructors

        public TasksWindowViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            OpenSubTaskTabEvent = new DelegateCommand(OpenSubTaskTab);
            UpdateTaskAfterEditingEvent = new DelegateCommand(UpdateTaskAfterEditing);
            LanguageChangedEvent = new DelegateCommand<object>(LanguageChanged);
            AddTaskEvent = new DelegateCommand(AddTask);
            DeleteTaskEvent = new DelegateCommand(DeleteTask);
            DeleteTaskTabEvent = new DelegateCommand(DeleteTaskTab);
            LogInButtonEvent = new DelegateCommand<object>(LogInButton);
            RegistrationButtonEvent = new DelegateCommand<object>(RegistrationButton);
            SelectedTabItemIndex = -1;
            SelectedListItemIndex = -1;
            _authorizationErrorLogTimer = new Timer(2000);
            _authorizationErrorLogTimer.AutoReset = false;
            _authorizationErrorLogTimer.Elapsed += TimerElapsedEvent;
            AuthorizationErrorLogVisibility = Visibility.Hidden;

            _languages = new Dictionary<string, CultureInfo>
            {
                {
                    "Русский", new CultureInfo("ru-RU", false)
                },
                {
                    "English", new CultureInfo("en-US", false)
                }
            };
        }

        #endregion Public Constructors

        #region Public Properties

        #region Authorization

        public Visibility AuthorizationErrorLogVisibility
        {
            get => _authorizationErrorLogVisibility;
            set => SetProperty(ref _authorizationErrorLogVisibility, value);
        }

        public string AuthorizationErrorLog
        {
            get => _authorizationErrorLog;
            set => SetProperty(ref _authorizationErrorLog, value);
        }

        public DelegateCommand<object> LogInButtonEvent { get; }
        public DelegateCommand<object> RegistrationButtonEvent { get; }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        #endregion Authorization

        public DelegateCommand UpdateTaskAfterEditingEvent { get; }
        public DelegateCommand<object> LanguageChangedEvent { get; }
        public DelegateCommand TestButtonEvent { get; }
        public DelegateCommand AddTaskEvent { get; }

        public DelegateCommand DeleteTaskEvent { get; }

        public DelegateCommand DeleteTaskTabEvent { get; }
        public DelegateCommand OpenSubTaskTabEvent { get; }

        public Dictionary<string, CultureInfo> Languages
        {
            get => _languages;
            set => SetProperty(ref _languages, value);
        }

        public KeyValuePair<string, CultureInfo> SelectedLanguage
        {
            get => _selectedLanguage;
            set => SetProperty(ref _selectedLanguage, value);
        }

        public ObservableCollection<TaskObject> TabTaskList
        {
            get => _tabTaskList;
            set => SetProperty(ref _tabTaskList, value);
        }

        public string AddTaskName
        {
            get => _addTaskName;
            set => SetProperty(ref _addTaskName, value);
        }

        public int HeaderSelectedTabIndex
        {
            get => _headerSelectedTabIndex;
            set => SetProperty(ref _headerSelectedTabIndex, value);
        }

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

        #endregion Public Properties

        #region Private Methods

        #region Authorization

        private void LogInButton(object passBox)
        {
            int id;
            string password = (passBox as PasswordBox).Password;
            if ((id = _databaseService.IsUserPasswordCorrect(Username, password)) != -1)
            {
                CreateTasksTabTemplate();
                _currentUserId = id;
                HeaderSelectedTabIndex = 1;
                _tasklist = _databaseService.GetTaskListAsync(_currentUserId).Result;
                ConvertTaskListToTaskObjectObservableCol(ref _tasklist, TabTaskList);
                CloseAllSubTabs();
            }
            else
            {
                AuthorizationErrorLog = "Wrong username or password.";
                AuthorizationErrorLogVisibility = Visibility.Visible;
                _authorizationErrorLogTimer.Start();
            }
        }

        private void RegistrationButton(object passBox)
        {
            string password = (passBox as PasswordBox).Password;
            if (_databaseService.IsUserExists(Username) == false)
            {
                CreateTasksTabTemplate();
                var user = new User(Username, password);
                _databaseService.AddUserAsync(user);
                HeaderSelectedTabIndex = 1;
                _currentUserId = user.Id;
                _tasklist = _databaseService.GetTaskListAsync(_currentUserId).Result;
                ConvertTaskListToTaskObjectObservableCol(ref _tasklist, TabTaskList);
                CloseAllSubTabs();
            }
            else
            {
                AuthorizationErrorLog = "This username already exists";
                AuthorizationErrorLogVisibility = Visibility.Visible;
                _authorizationErrorLogTimer.Start();
            }
        }

        private void TimerElapsedEvent(object sender, ElapsedEventArgs e)
        {
            AuthorizationErrorLogVisibility = Visibility.Hidden;
            _authorizationErrorLogTimer.Stop();
        }

        #endregion Authorization

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
                    DeleteReferencedTabs(item);
                }
            }
        }

        private void DeleteReferencedTabs(Task task)
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
                DeleteReferencedTabs(task);
            }
        }

        private void DeleteTaskTab()
        {
            if (SelectedTabItemIndex >= 1)
            {
                TabTaskList.RemoveAt(SelectedTabItemIndex);
            }
        }

        private void OpenSubTaskTab()
        {
            if (SelectedListItemIndex >= 0 && SelectedTabItemIndex >= 0)
            {
                TabTaskList.Add(TabTaskList[SelectedTabItemIndex].SubTasks[SelectedListItemIndex]);
            }
        }

        private void UpdateTaskAfterEditing()
        {
            _databaseService.UpdateTaskAsync(TabTaskList[SelectedTabItemIndex].SubTasks[SelectedListItemIndex].Task);
        }

        private void LanguageChanged(object userControl)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(SelectedLanguage.Value.Name);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(SelectedLanguage.Value.Name);

            CultureInfo cultureInfo = new CultureInfo(SelectedLanguage.Value.Name);
            CultureResources.ChangeCulture(cultureInfo);
        }

        private void CloseAllSubTabs()
        {
            for (int i = TabTaskList.Count - 1; i > 0; i--)
            {
                TabTaskList.RemoveAt(i);
            }
        }

        #endregion Private Methods
    }
}