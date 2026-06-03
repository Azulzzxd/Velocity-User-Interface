using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VelocityLite.Controls
{
    /// <summary>
    /// Interaction logic for NotificationOverlayWindow.xaml
    /// </summary>
    public partial class NotificationOverlayWindow : Window
    {
        public NotificationOverlayWindow()
        {
            InitializeComponent();
            Loaded += (_, _) => PositionBottomRight();
        }

        private void PositionBottomRight()
        {
            var area = SystemParameters.WorkArea;

            Left = area.Right - Width;
            Top = area.Bottom - Height;
        }

        public void ShowToast(NotificationControl toast)
        {
            ToastHost.Children.Add(toast);
        }

        public void RemoveToast(NotificationControl toast)
        {
            ToastHost.Children.Remove(toast);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;

            const int GWL_EXSTYLE = -20;
            const int WS_EX_TOOLWINDOW = 0x00000080;

            var style = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, style | WS_EX_TOOLWINDOW);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int value);

    }
}
