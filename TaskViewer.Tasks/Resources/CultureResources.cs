using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using TaskViewer.Tasks.Resources;
using TaskViewer.Tasks.Views;

namespace TaskViewer.Tasks.Resources
{
    public class CultureResources
    {
        /// <summary>
        /// Backing filed for provider
        /// </summary>
        private static ObjectDataProvider provider;

        /// <summary>
        /// Gets Resource provider
        /// </summary>
        public static ObjectDataProvider ResourceProvider
        {
            get
            {
                if (provider == null)
                {
                    provider = (ObjectDataProvider)TasksWindow.UserControl.FindResource("Resources");
                }

                return provider;
            }
        }

        /// <summary>
        /// Change the current culture used in the application.
        /// If the desired culture is available all localized elements are updated.
        /// </summary>
        public static void ChangeCulture(CultureInfo culture)
        {
            languages.Culture = culture;
            ResourceProvider.Refresh();
        }

        /// <summary>
        /// The Resources ObjectDataProvider uses this method to get an instance of the WPFLocalize.Properties.Resources class
        /// </summary>
        /// <returns>Returns resource instance</returns>
        public languages GetResourceInstance()
        {
            return new languages();
        }
    }
}