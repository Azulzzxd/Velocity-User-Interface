using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;
using VelocityLite.Controls;
using System;

namespace VelocityLite
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static NotificationOverlayWindow? Overlay { get; private set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Overlay = new NotificationOverlayWindow();
            Overlay.Show();
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowErrorDialog(e.Exception);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Current.Dispatcher.Invoke(() => ShowErrorDialog(ex));
            }
        }

        private void ShowErrorDialog(Exception ex)
        {
            var errorWindow = new ErrorMessageBox
            {
                Owner = Current.MainWindow
            };

            errorWindow.ErrorDetails = ex.ToString();

            errorWindow.ShowDialog();
        }
    }

}
