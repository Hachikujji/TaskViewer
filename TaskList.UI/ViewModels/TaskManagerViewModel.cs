using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaskList.Database.Models;
using System.Timers;

namespace TaskList.UI.ViewModels
{
    public class TaskManagerViewModel : BindableBase
    {
        Timer _timer = new Timer(4000);
        //ctor
        public TaskManagerViewModel()
        {

            AddMainTaskButtonEvent = new DelegateCommand(AddMainTask);
            AddSubTaskButtonEvent = new DelegateCommand(AddSubTask);
            ShowAddButtonsEvent = new DelegateCommand(ShowAddButtons);


            var Mcol = new ObservableCollection<MainTask>();
            for (int i = 0; i < 50; i++)
            {
                var Scol = new ObservableCollection<SubTask>();
                for (var j = 0; j < 20; j++)
                {
                    Scol.Add(new SubTask($"SubTask #{j}"));
                }
                Mcol.Add(new MainTask($"MainTask #{i}", Scol));
            }
            MainTasks = Mcol;
            Text = "123";
            HideAddButtons();
        }
        #region Binding commands

        public DelegateCommand AddMainTaskButtonEvent { get; }
        public DelegateCommand AddSubTaskButtonEvent { get; }
        public DelegateCommand ShowAddButtonsEvent { get; }

        #endregion

        #region Binding properties

        private ObservableCollection<MainTask> _mainTasks;
        public ObservableCollection<MainTask> MainTasks
        {
            get => _mainTasks;
            set => SetProperty(ref _mainTasks, value);
        }

        private Visibility _isAddTaskButtonsVisible;
        public Visibility IsAddTaskButtonsVisibleEvent
        {
            get => _isAddTaskButtonsVisible;
            set => SetProperty(ref _isAddTaskButtonsVisible, value);
        }

        private string _text;
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }
        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        #endregion

        private void AddMainTask()
        {
            MainTasks.Add(new MainTask("New Task", new ObservableCollection<SubTask>()));
            HideAddButtons();
        }
        private void AddSubTask()
        {
            MainTasks[SelectedTabIndex].SubTasks.Add(new SubTask("New SubTask"));
            HideAddButtons();
        }
        private void ShowAddButtons()
        {
            IsAddTaskButtonsVisibleEvent = Visibility.Visible;
            _timer.Enabled=true;
            _timer.Elapsed += OnTimerElapsedEvent;
        }
        private void HideAddButtons()
        {
            IsAddTaskButtonsVisibleEvent = Visibility.Hidden;
        }

        private void OnTimerElapsedEvent(Object source, ElapsedEventArgs e)
        {
            _timer.Enabled = false;
            HideAddButtons();
        }

    }
}
