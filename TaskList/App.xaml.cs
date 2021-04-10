using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using TaskList.Database;
using TaskList.Logic;
using TaskList.UI;
using TaskList.Views;

namespace TaskList
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<ServicesModule>();
            moduleCatalog.AddModule<DatabaseModule>();
            moduleCatalog.AddModule<UIModule>();
        }
    }
}
