using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace KDSUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Page EditOrdersPage { get; set; }
        public static Page DashboardPage { get; set; }
        public static Page EditLayoutPage { get; set; }
        public static Page Analytics { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
    }

}
