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

        // selected tab index
        private int _selectedTabItemIndex;

        // list of all user tasks
        private List<Task> _tasklist = new List<Task>();

        // task tree
        private ObservableCollection<TaskObject> _tabTaskList;

        // Name textbox field
        private string _addTaskName;

        // Authorization username textbox field
        private string _username;

        // Authorization error message
        private string _authorizationErrorLog;

        // Visibility of authorization error message
        private Visibility _authorizationErrorLogVisibility;

        // Languages dictionary
        private Dictionary<string, CultureInfo> _languages;

        // Selected language
        private KeyValuePair<string, CultureInfo> _selectedLanguage;

        #endregion Private Fields

        #region Public Constructors

        public TasksWindowViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            OpenSubTaskTabEvent = new DelegateCommand(OpenSubTaskTab);
            UpdateTaskAfterEditingEvent = new DelegateCommand(UpdateTaskAfterEditing);
            LanguageChangedEvent = new DelegateCommand(LanguageChanged);
            AddTaskEvent = new DelegateCommand(AddTask);
            DeleteTaskEvent = new DelegateCommand(DeleteTask);
            DeleteTaskTabEvent = new DelegateCommand(DeleteTaskTab);
            LogInButtonEvent = new DelegateCommand<object>(LogInButton);
            RegistrationButtonEvent = new DelegateCommand<object>(RegistrationButton);
            _timerDuration = 3000;
            SelectedTabItemIndex = -1;
            SelectedListItemIndex = -1;
            _authorizationErrorLogTimer = new Timer(_timerDuration);
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
        public DelegateCommand LanguageChangedEvent { get; }
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

        /// <summary>
        /// Action when login button pressed
        /// </summary>
        /// <param name="PasswordBox"></param>
        private void LogInButton(object PasswordBox)
        {
            string password = (PasswordBox as PasswordBox).Password;
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
                return;
            int id;
            if (_databaseService.IsUserPasswordCorrect(Username, password) != false)
            {
                id = _databaseService.GetUserId(Username);
                CreateTasksTabTemplate();
                _currentUserId = id;
                HeaderSelectedTabIndex = 1;
                _tasklist = _databaseService.GetTaskListAsync(_currentUserId).Result;
                ConvertTaskListToTaskObjectObservableCol(ref _tasklist, TabTaskList);
                CloseAllSubTabs();
            }
            else
            {
                AuthorizationErrorLog = languages.LoginError;
                AuthorizationErrorLogVisibility = Visibility.Visible;
                _authorizationErrorLogTimer.Start();
            }
        }

        /// <summary>
        /// Action when registration button pressed
        /// </summary>
        /// <param name="PasswordBox"></param>
        private void RegistrationButton(object PasswordBox)
        {
            string password = (PasswordBox as PasswordBox).Password;
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
                return;
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
                AuthorizationErrorLog = languages.RegistrationError;
                AuthorizationErrorLogVisibility = Visibility.Visible;
                _authorizationErrorLogTimer.Start();
            }
        }

        /// <summary>
        /// Action when timer elapsed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            if (SelectedTabItemIndex >= 0)
            {
                var task = new Task(_currentUserId, AddTaskName, TabTaskList[SelectedTabItemIndex].Task.Id);
                _databaseService.AddTaskAsync(task);
                _tasklist.Add(task);
                TabTaskList[SelectedTabItemIndex].SubTasks.Add(new TaskObject(task, new ObservableCollection<TaskObject>()));
            }
        }

        /// <summary>
        /// Create task tree from task list
        /// </summary>
        /// <param name="taskList"></param>
        /// <param name="taskObjects"></param>
        /// <param name="mainTaskId"></param>
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

        /// <summary>
        /// Create clear list of tasks for new logged user
        /// </summary>
        private void CreateTasksTabTemplate()
        {
            TabTaskList = new ObservableCollection<TaskObject>();
            TabTaskList.Add(new TaskObject(new Task(_currentUserId, "🏠", 0), new ObservableCollection<TaskObject>()));
        }

        /// <summary>
        /// Delete all task root
        /// </summary>
        /// <param name="taskList"> Task list</param>
        /// <param name="task"> task </param>
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

        /// <summary>
        /// Close all tabs of deleted tasks
        /// </summary>
        /// <param name="task"></param>
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

        /// <summary>
        /// Delete task context menu
        /// </summary>
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

        /// <summary>
        /// Close tab button(image)
        /// </summary>
        private void DeleteTaskTab()
        {
            if (SelectedTabItemIndex >= 1)
            {
                TabTaskList.RemoveAt(SelectedTabItemIndex);
            }
        }

        /// <summary>
        /// Open Subtask context menu button.
        /// </summary>
        private void OpenSubTaskTab()
        {
            if (SelectedListItemIndex >= 0 && SelectedTabItemIndex >= 0)
            {
                TabTaskList.Add(TabTaskList[SelectedTabItemIndex].SubTasks[SelectedListItemIndex]);
            }
        }

        /// <summary>
        /// Action when task was edited
        /// </summary>
        private void UpdateTaskAfterEditing()
        {
            _databaseService.UpdateTaskAsync(TabTaskList[SelectedTabItemIndex].SubTasks[SelectedListItemIndex].Task);
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

        /// <summary>
        /// Remove all tabs when task tree was created.
        /// </summary>
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