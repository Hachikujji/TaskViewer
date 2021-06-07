using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using TaskViewer.Database.Models;
using TaskViewer.Database.Services;
using TaskViewer.Tasks.Views;

namespace TaskViewer.Tasks
{
    public class TasksModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RequestNavigate("MainRegion", nameof(Views.TasksWindow));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<TasksWindow>();
            containerRegistry.RegisterScoped<IDatabaseService, DatabaseService>();
            containerRegistry.RegisterScoped<TaskViewerEntities>();
        }
    }
}