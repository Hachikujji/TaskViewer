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
        private ObservableCollection<MainTask> _mainTasks;
        public ObservableCollection<MainTask> MainTasks
        {
            get => _mainTasks;
            set => SetProperty(ref _mainTasks, value);
        }

        private string _text;
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public TaskManagerViewModel()
        {
            var Scol = new ObservableCollection<SubTask>();
            for(var i = 0; i < 10; i++)
            {
                Scol.Add(new SubTask($"{i}"));
            }
            var Mcol = new ObservableCollection<MainTask>();
            for (int i = 0; i < 10; i++)
            {
                Mcol.Add(new MainTask($"{i}", Scol));
            }
            MainTasks = Mcol;
            Text = "123";
        }
    }
}
