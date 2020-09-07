using System;
using System.Threading.Tasks;
using System.Windows;

namespace SkiSlopeMotionDetection.PresentationLayer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //https://stackoverflow.com/questions/793100/globally-catch-exceptions-in-a-wpf-application
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SetupExceptionHandling();
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");
            };

            DispatcherUnhandledException += (s, e) =>
            {
                e.Handled = LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };
        }

        private bool LogUnhandledException(Exception exception, string source)
        {
            if(exception is ArgumentException)
            {
                MessageBox.Show(exception.Message, "Invalid argument", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return true;
            }
            else
            {
                MessageBox.Show(exception.Message, "Appwide error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
