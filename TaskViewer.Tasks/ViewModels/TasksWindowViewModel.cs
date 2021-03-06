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

        // How long will the error be seen
        private readonly int _timerDuration;

        // Error visibility timer
        private readonly Timer _authorizationErrorLogTimer;

        // db services
        private IDatabaseService _databaseService;

        // id of logged user
        private int _currentUserId;

        // selected tab of [ Authorization | Tasks ] TabControl
        private int _headerSelectedTabIndex;

        // selected task index in task list
        private int _selectedListItemIndex;

        // selected task in task list
        private TaskObject _selectedListItem;

        // selected tab index
        private int _selectedTabItemIndex;

        // selected tab item
        private TaskObject _selectedTabItem;

        // selected(on ComboBox first click) ComboBox index
        private int _selectedComboBoxIndex;

        // list of all user tasks
        private List<Task> _tasklist = new List<Task>();

        // All tabs like: All tasks, in progress, completed
        private ObservableCollection<TaskObject> _tabControlTabs;

        // Name textbox field
        private string _addTaskName;

        // Authorization username textbox field
        private string _username;

        // Authorization password textbox field
        private string _password;

        // Authorization error message
        private string _authorizationErrorLog;

        // Visibility of authorization error message
        private Visibility _authorizationErrorLogVisibility;

        // Languages dictionary
        private Dictionary<string, CultureInfo> _languages;

        // Status dictionary
        private Dictionary<string, int> _statuses;

        // Selected language
        private KeyValuePair<string, CultureInfo> _selectedLanguage;

        // Amount of Main tabs ( Tasks, Completed, In progress, etc.)
        private int _mainTabsCount;

        #endregion Private Fields

        #region Public Constructors

        public TasksWindowViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            OpenSubTaskTabEvent = new DelegateCommand(OpenSubTaskTab);
            UpdateTaskAfterEditingEvent = new DelegateCommand<object>(UpdateTaskAfterEditing);
            UpdateTaskStatusEvent = new DelegateCommand<object>(UpdateTaskStatus);
            LanguageChangedEvent = new DelegateCommand(LanguageChanged);
            SaveNotChangedTaskStatusEvent = new DelegateCommand<object>(SaveNotChangedTaskStatus);
            AddTaskEvent = new DelegateCommand(AddTask);
            DeleteTaskEvent = new DelegateCommand(DeleteTask);
            DeleteTaskTabEvent = new DelegateCommand(DeleteTaskTab);
            LogInButtonEvent = new DelegateCommand(LogInButton);
            LogOutEvent = new DelegateCommand(LogOutButton);
            RegistrationButtonEvent = new DelegateCommand(RegistrationButton);
            _timerDuration = 3000;
            SelectedTabItemIndex = -1;
            SelectedListItemIndex = -1;
            _authorizationErrorLogTimer = new Timer(_timerDuration);
            _authorizationErrorLogTimer.AutoReset = false;
            _authorizationErrorLogTimer.Elapsed += TimerElapsedEvent;
            AuthorizationErrorLogVisibility = Visibility.Hidden;

            _mainTabsCount = Enum.GetNames(typeof(TabsEnum)).Length;

            Languages = new Dictionary<string, CultureInfo>
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

        #region Private Methods

        #region Authorization

        /// <summary>
        /// Action when login button pressed
        /// </summary>
        private void LogInButton()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                return;
            int id;
            if (_databaseService.IsUserPasswordCorrect(Username, Password))
            {
                id = _databaseService.GetUserId(Username);
                AfterAuthorizationInitialization();
                _currentUserId = id;
                HeaderSelectedTabIndex = 1;
                _tasklist = _databaseService.GetTaskListAsync(_currentUserId).Result;
                ConvertTaskListToTaskObjectObservableCol(ref _tasklist, TabControlTabs[(int)TabsEnum.AllTasks].SubTasks);
                SelectedTabItemIndex = 0;
            }
            else
            {
                AuthorizationErrorLog = languages.LoginError;
                AuthorizationErrorLogVisibility = Visibility.Visible;
                _authorizationErrorLogTimer.Start();
            }
        }

        /// <summary>
        /// Action when logout button pressed
        /// </summary>
        private void LogOutButton()
        {
            HeaderSelectedTabIndex = 0;
        }

        /// <summary>
        /// Action when registration button pressed
        /// </summary>
        private void RegistrationButton()
        {
            Password = string.Empty;
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                return;
            if (_databaseService.IsUserExists(Username) == false)
            {
                AfterAuthorizationInitialization();
                var user = new User(Username, Password);
                _databaseService.AddUserAsync(user);
                HeaderSelectedTabIndex = 1;
                _currentUserId = _databaseService.GetUserId(Username);
                SelectedTabItemIndex = 0;
            }
            else
            {
                AuthorizationErrorLog = languages.RegistrationError;
                AuthorizationErrorLogVisibility = Visibility.Visible;
                _authorizationErrorLogTimer.Start();
            }
        }

        /// <summary>
        /// Action when timer elapsed
        /// </summary>
        private void TimerElapsedEvent(object sender, ElapsedEventArgs e)
        {
            AuthorizationErrorLogVisibility = Visibility.Hidden;
            _authorizationErrorLogTimer.Stop();
        }

        #endregion Authorization

        /// <summary>
        /// Add new task button
        /// </summary>
        private void AddTask()
        {
            if ((SelectedTabItemIndex == 0 || SelectedTabItemIndex > _mainTabsCount - 1) && !string.IsNullOrWhiteSpace(AddTaskName))
            {
                int mainTaskId;
                mainTaskId = SelectedTabItem.Task.Id;
                var task = new Task(_currentUserId, mainTaskId, AddTaskName, DateTime.Now);
                _databaseService.AddTaskAsync(task);
                _tasklist.Add(task);
                var taskObject = new TaskObject(task, new ObservableCollection<TaskObject>());
                if (SelectedTabItem != null)
                    SelectedTabItem.SubTasks.Add(taskObject);
            }
        }

        /// <summary>
        /// Create task tree and fill "completed" & "In progress" tabs from task list
        /// </summary>
        private void ConvertTaskListToTaskObjectObservableCol(ref List<Task> taskList, ObservableCollection<TaskObject> taskObjects, int mainTaskId = 0)
        {
            foreach (var item in taskList)
                if (item.MainTaskId == mainTaskId)
                {
                    var taskObject = new TaskObject(item, new ObservableCollection<TaskObject>());
                    taskObjects.Add(taskObject);
                    if (item.Status == (int)StatusEnum.Completed)
                        TabControlTabs[(int)TabsEnum.CompletedTasks].SubTasks.Add(taskObject);
                    if (item.Status == (int)StatusEnum.InProgress)
                        TabControlTabs[(int)TabsEnum.InProgressTasks].SubTasks.Add(taskObject);
                }
            foreach (var item in taskObjects)
                ConvertTaskListToTaskObjectObservableCol(ref taskList, item.SubTasks, item.Task.Id);
        }

        /// <summary>
        /// Create clear list of tasks and statuses for new logged user
        /// </summary>
        private void AfterAuthorizationInitialization()
        {
            TabControlTabs = new ObservableCollection<TaskObject>();
            TabControlTabs.Add(new TaskObject(new Task(_currentUserId, 0, languages.AllTasks, DateTime.Now), new ObservableCollection<TaskObject>()));
            TabControlTabs.Add(new TaskObject(new Task(_currentUserId, 0, languages.InProgress, DateTime.Now), new ObservableCollection<TaskObject>()));
            TabControlTabs.Add(new TaskObject(new Task(_currentUserId, 0, languages.Completed, DateTime.Now), new ObservableCollection<TaskObject>()));
            Statuses = new Dictionary<string, int>
            {
                {
                    languages.TaskUnassigned, 0
                },
                {
                    languages.TaskInProgress, 1
                },
                {
                    languages.TaskCompleted, 2
                }
            };
        }

        /// <summary>
        /// Delete all task root
        /// </summary>
        private void DeleteAllTaskRoot(ref List<Task> taskList, Task task)
        {
            if (task.Status != (int)TabsEnum.AllTasks)
                DeleteTaskFromFolders(task);
            foreach (var item in taskList)
            {
                if (item.MainTaskId == task.Id)
                {
                    DeleteAllTaskRoot(ref taskList, item);
                    if (task.Status != (int)TabsEnum.AllTasks)
                        DeleteTaskFromFolders(task);
                    _databaseService.RemoveTaskAsync(item);
                    DeleteReferencedTabs(item);
                }
            }
        }

        /// <summary>
        /// Close all tabs of deleted tasks
        /// </summary>
        /// <param name="task"></param>
        private void DeleteReferencedTabs(Task task)
        {
            List<TaskObject> deleteTabList = new List<TaskObject>();
            foreach (var tab in TabControlTabs)
            {
                if (task.Id == tab.Task.Id)
                    deleteTabList.Add(tab);
            }
            foreach (var delTask in deleteTabList)
                TabControlTabs.Remove(delTask);
        }

        /// <summary>
        /// Delete all Task ref from folders
        /// </summary>
        /// <param name="task"></param>
        private void DeleteTaskFromFolders(Task task)
        {
            List<TaskObject> deleteTabList = new List<TaskObject>();
            foreach (var item in TabControlTabs[task.Status].SubTasks)
                if (task.Id == item.Task.Id)
                    deleteTabList.Add(item);
            foreach (var delTask in deleteTabList)
                TabControlTabs[task.Status].SubTasks.Remove(delTask);
        }

        /// <summary>
        /// Delete task context menu
        /// </summary>
        private void DeleteTask()
        {
            if ((SelectedListItemIndex >= 0) && (SelectedTabItemIndex == 0 || SelectedTabItemIndex > _mainTabsCount - 1))
            {
                var task = SelectedListItem.Task;
                DeleteAllTaskRoot(ref _tasklist, task);
                _databaseService.RemoveTaskAsync(task);
                SelectedTabItem.SubTasks.Remove(SelectedListItem);
                DeleteReferencedTabs(task);
            }
        }

        /// <summary>
        /// Close tab button(image)
        /// </summary>
        private void DeleteTaskTab()
        {
            if (SelectedTabItemIndex > _mainTabsCount - 1)
                TabControlTabs.Remove(SelectedTabItem);
        }

        /// <summary>
        /// Open Subtask context menu button.
        /// </summary>
        private void OpenSubTaskTab()
        {
            if (SelectedListItemIndex >= 0 && SelectedTabItemIndex >= 0)
                TabControlTabs.Add(SelectedListItem);
        }

        /// <summary>
        /// Action when task was edited
        /// </summary>
        private void UpdateTaskAfterEditing(object taskUI)
        {
            //if ComboBox
            if (taskUI != null)
            {
                TaskObject task = taskUI as TaskObject;
                _databaseService.UpdateTaskAsync(task.Task);
            }
            else
                _databaseService.UpdateTaskAsync(SelectedListItem.Task);
        }

        /// <summary>
        /// Action when task status was edited
        /// </summary>
        private void UpdateTaskStatus(object taskUI)
        {
            TaskObject task = taskUI as TaskObject;
            UpdateTaskAfterEditing(task);
            if (SelectedComboBoxIndex != (int)TabsEnum.AllTasks)
                TabControlTabs[SelectedComboBoxIndex].SubTasks.Remove(task);
            if (task.Task.Status != (int)StatusEnum.Unassigned)
                TabControlTabs[task.Task.Status].SubTasks.Add(task);
        }

        /// <summary>
        /// Save not changed task(with old status)
        /// </summary>
        private void SaveNotChangedTaskStatus(object taskUI)
        {
            TaskObject task = taskUI as TaskObject;
            SelectedComboBoxIndex = task.Task.Status;
        }

        /// <summary>
        /// Action when other language was chosen
        /// </summary>
        private void LanguageChanged()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(SelectedLanguage.Value.Name);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(SelectedLanguage.Value.Name);

            CultureInfo cultureInfo = new CultureInfo(SelectedLanguage.Value.Name);
            CultureResources.ChangeCulture(cultureInfo);
        }

        #endregion Private Methods

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

        public DelegateCommand LogInButtonEvent { get; }
        public DelegateCommand RegistrationButtonEvent { get; }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        #endregion Authorization

        public DelegateCommand<object> UpdateTaskAfterEditingEvent { get; }
        public DelegateCommand<object> UpdateTaskStatusEvent { get; }
        public DelegateCommand LanguageChangedEvent { get; }
        public DelegateCommand LogOutEvent { get; }
        public DelegateCommand TestButtonEvent { get; }
        public DelegateCommand AddTaskEvent { get; }
        public DelegateCommand<object> SaveNotChangedTaskStatusEvent { get; }
        public DelegateCommand DeleteTaskEvent { get; }

        public DelegateCommand DeleteTaskTabEvent { get; }
        public DelegateCommand OpenSubTaskTabEvent { get; }

        public Dictionary<string, CultureInfo> Languages
        {
            get => _languages;
            set => SetProperty(ref _languages, value);
        }

        public Dictionary<string, int> Statuses
        {
            get => _statuses;
            set => SetProperty(ref _statuses, value);
        }

        public KeyValuePair<string, CultureInfo> SelectedLanguage
        {
            get => _selectedLanguage;
            set => SetProperty(ref _selectedLanguage, value);
        }

        public ObservableCollection<TaskObject> TabControlTabs
        {
            get => _tabControlTabs;
            set => SetProperty(ref _tabControlTabs, value);
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

        public TaskObject SelectedListItem
        {
            get => _selectedListItem;
            set => SetProperty(ref _selectedListItem, value);
        }

        public int SelectedTabItemIndex
        {
            get => _selectedTabItemIndex;
            set => SetProperty(ref _selectedTabItemIndex, value);
        }

        public TaskObject SelectedTabItem
        {
            get => _selectedTabItem;
            set => SetProperty(ref _selectedTabItem, value);
        }

        public int SelectedComboBoxIndex
        {
            get => _selectedComboBoxIndex;
            set => SetProperty(ref _selectedComboBoxIndex, value);
        }

        #endregion Public Properties
    }
}