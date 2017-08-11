using System.Windows;

namespace WPFImageViewer
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow wnd = new MainWindow(e.Args);
            wnd.Show();
        }
    }
}
