using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

using Win32.Shared.Interop;

namespace Win32.Shared
{
    /// <summary>
    ///     MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class WindowPicker : Window
    {
        private static readonly string[] IgnoreProcesses = { "applicationframehost", "shellexperiencehost", "systemsettings", "winstore.app", "searchui" };

        public WindowPicker()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        public static IntPtr FindWindow(string windowTitle, Point? location = null)
        {
            var result = IntPtr.Zero;
            NativeMethods.EnumWindows((hwnd, lParam) =>
            {
                // ignore invisible windows
                if (!NativeMethods.IsWindowVisible(hwnd))
                    return true;

                // ignore windows not containing the input string
                var title = new StringBuilder(1024);
                NativeMethods.GetWindowText(hwnd, title, title.Capacity);
                var titleStr = title.ToString();
                if (string.IsNullOrWhiteSpace(titleStr) || !titleStr.Contains(windowTitle))
                    return true;

                NativeMethods.GetWindowThreadProcessId(hwnd, out var processId);

                // ignore by process name
                var process = Process.GetProcessById((int)processId);
                if (IgnoreProcesses.Contains(process.ProcessName.ToLower()))
                    return true;

                if (location is Point loc)
                {
                    NativeMethods.GetWindowRect(hwnd, out var rect);
                    if (!NativeMethods.PtInRect(ref rect, new NativeMethods.POINT() { X = (int)loc.X, Y = (int)loc.Y }))
                        return true;
                }

                result = hwnd;
                return true;

            }, IntPtr.Zero);

            return result;
        }


        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            FindWindows();
        }

        public IntPtr PickCaptureTarget(IntPtr hWnd)
        {
            new WindowInteropHelper(this).Owner = hWnd;
            ShowDialog();

            return ((CapturableWindow?) Windows.SelectedItem)?.Handle ?? IntPtr.Zero;
        }

        private void FindWindows()
        {
            var wih = new WindowInteropHelper(this);
            NativeMethods.EnumWindows((hWnd, lParam) =>
            {
                // ignore invisible windows
                if (!NativeMethods.IsWindowVisible(hWnd))
                    return true;

                // ignore untitled windows
                var title = new StringBuilder(1024);
                NativeMethods.GetWindowText(hWnd, title, title.Capacity);
                if (string.IsNullOrWhiteSpace(title.ToString()))
                    return true;

                // ignore me
                if (wih.Handle == hWnd)
                    return true;

                NativeMethods.GetWindowThreadProcessId(hWnd, out var processId);

                // ignore by process name
                var process = Process.GetProcessById((int) processId);
                if (IgnoreProcesses.Contains(process.ProcessName.ToLower()))
                    return true;

                Windows.Items.Add(new CapturableWindow
                {
                    Handle = hWnd,
                    Name = $"{title} ({process.ProcessName}.exe)"
                });

                return true;
            }, IntPtr.Zero);
        }

        private void WindowsOnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }

    public struct CapturableWindow
    {
        public string Name { get; set; }
        public IntPtr Handle { get; set; }
    }
}