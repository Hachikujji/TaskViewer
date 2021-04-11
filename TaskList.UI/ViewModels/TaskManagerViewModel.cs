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
            _timer.AutoReset = false;
            AddMainTaskButtonEvent = new DelegateCommand(AddMainTask);
            AddSubTaskButtonEvent = new DelegateCommand(AddSubTask);
            ShowAddButtonsEvent = new DelegateCommand(ShowAddButtons);
            DeleteItemButtonEvent = new DelegateCommand(DeleteItemButton);
            ResetTabIndexEvent = new DelegateCommand(ResetTabIndex);
            ShowTabEditEvent = new DelegateCommand(ShowTabEdit);
            HideTabEditEvent = new DelegateCommand(HideTabEdit);
            SelectedItemIndex = -1;
            var Mcol = new ObservableCollection<MainTask>();
            MainTasks = Mcol;

            HideAddButtons();
        }
        #region Binding commands

        public DelegateCommand AddMainTaskButtonEvent { get; }
        public DelegateCommand AddSubTaskButtonEvent { get; }
        public DelegateCommand ShowAddButtonsEvent { get; }
        public DelegateCommand DeleteItemButtonEvent { get; }
        public DelegateCommand ResetTabIndexEvent { get; }
        public DelegateCommand ShowTabEditEvent { get; }
        public DelegateCommand HideTabEditEvent { get; }

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

        private Visibility _isTabEditVisible;
        public Visibility IsTabEditVisibleEvent
        {
            get => _isTabEditVisible;
            set => SetProperty(ref _isTabEditVisible, value);
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

        private int _selectedItemIndex;
        public int SelectedItemIndex
        {
            get => _selectedItemIndex;
            set => SetProperty(ref _selectedItemIndex, value);
        }

        #endregion

        private void AddMainTask()
        {
            SelectedTabIndex = 0;
            SelectedItemIndex = -1;
            MainTasks.Add(new MainTask($"New Task #{MainTasks.Count}", new ObservableCollection<SubTask>()));
            HideAddButtons();
        }
        private void AddSubTask()
        {
            if(SelectedTabIndex!=-1)
                MainTasks[SelectedTabIndex].SubTasks.Add(new SubTask($"New SubTask #{MainTasks[SelectedTabIndex].SubTasks.Count}"));
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

        private void DeleteItemButton()
        {
            if (MainTasks.Count != 0)
                if (SelectedItemIndex == -1)
                    MainTasks.RemoveAt(SelectedTabIndex);
                else
                    MainTasks[SelectedTabIndex].SubTasks.RemoveAt(SelectedItemIndex);
                
        }
        private void ResetTabIndex()
        {
            SelectedTabIndex = -1;
        }
        private void ShowTabEdit()
        {
            IsTabEditVisibleEvent = Visibility.Visible;
        }
        private void HideTabEdit()
        {
            IsTabEditVisibleEvent = Visibility.Hidden;
        }

    }
}
