using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaskList.Database.Models;

namespace TaskList.UI.ViewModels
{
    public class TaskManagerViewModel : BindableBase
    {
        //ctor
        public TaskManagerViewModel()
        {

            AddMainTaskButtonEvent = new DelegateCommand(AddMainTask);
            AddSubTaskButtonEvent = new DelegateCommand(AddSubTask);

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
            Text = "123";
        }
        public DelegateCommand AddMainTaskButtonEvent { get; }
        public DelegateCommand AddSubTaskButtonEvent { get; }

        #region Binding properties

        private ObservableCollection<MainTask> _mainTasks;
        public ObservableCollection<MainTask> MainTasks
        {
            get => _mainTasks;
            set => SetProperty(ref _mainTasks, value);
        }
        public DelegateCommand EmployeeDetailsCommand { get; }

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
            MainTasks.Add(new MainTask("asd", new ObservableCollection<SubTask>()));
        }
        private void AddSubTask()
        {
            MainTasks.Add(new MainTask("asd", new ObservableCollection<SubTask>()));
        }


    }
}
