

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Edu.CommonLibCore
{
    /// <summary>
    /// WPF窗体操作工具类
    /// <para>author:wangyulong</para>
    /// </summary>
    public class WindowHelper
    {
        #region 重新初始化窗口大小，显示任务栏

        private static Window currentWindow;
        /// <summary>
        /// 重新初始化Windows的大小
        /// </summary>
        public static void ReInitWindowSize(Window window)
        {
            currentWindow = window;
            window.SourceInitialized += Window_SourceInitialized;
        }

        private static void Window_SourceInitialized(object sender, EventArgs e)
        {
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(currentWindow).Handle);
            // Should never be null
            if (source == null) throw new Exception("Cannot get HwndSource instance.");
            source.AddHook(new HwndSourceHook(WndProc));
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case Win32Helper.WM_GETMINMAXINFO: // WM_GETMINMAXINFO message
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }

            return IntPtr.Zero;
        }

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            try
            {
                double roomfactor = GetZoomFactor();
                // MINMAXINFO structure
                Win32Helper.MINMAXINFO mmi = (Win32Helper.MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(Win32Helper.MINMAXINFO));

                // Get handle for nearest monitor to this window
                WindowInteropHelper wih = new WindowInteropHelper(currentWindow);
                IntPtr hMonitor = Win32Helper.MonitorFromWindow(wih.Handle, Win32Helper.MONITOR_DEFAULTTONEAREST);

                // Get monitor info
                Win32Helper.MONITORINFOEX monitorInfo = new Win32Helper.MONITORINFOEX();
                monitorInfo.cbSize = Marshal.SizeOf(monitorInfo);
                Win32Helper.GetMonitorInfo(new HandleRef(currentWindow, hMonitor), monitorInfo);

                // Get HwndSource
                HwndSource source = HwndSource.FromHwnd(wih.Handle);
                if (source == null)
                    // Should never be null
                    throw new Exception("Cannot get HwndSource instance.");
                if (source.CompositionTarget == null)
                    // Should never be null
                    throw new Exception("Cannot get HwndTarget instance.");

                // Get transformation matrix
                Matrix matrix = source.CompositionTarget.TransformFromDevice;

                // Convert working area
                Win32Helper.RECT workingArea = monitorInfo.rcMonitor;
                Point dpiIndependentSize =
                    matrix.Transform(new Point(
                            workingArea.Right - workingArea.Left,
                            workingArea.Bottom - workingArea.Top
                            ));

                // Convert minimum size
                Point dpiIndenpendentTrackingSize = matrix.Transform(new Point(
                    currentWindow.MinWidth,
                    currentWindow.MinHeight
                    ));

                // Set the maximized size of the window
                mmi.ptMaxSize.x = (int)(dpiIndependentSize.X * roomfactor);
                mmi.ptMaxSize.y = (int)(dpiIndependentSize.Y * roomfactor);

                // Set the position of the maximized window
                mmi.ptMaxPosition.x = 0;
                mmi.ptMaxPosition.y = 0;

                // Set the minimum tracking size
                mmi.ptMinTrackSize.x = (int)dpiIndenpendentTrackingSize.X;
                mmi.ptMinTrackSize.y = (int)dpiIndenpendentTrackingSize.Y;

                Marshal.StructureToPtr(mmi, lParam, true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static double GetZoomFactor()
        {
            double zoomfactor = 1;
            PresentationSource source = PresentationSource.FromVisual(currentWindow);
            double dpiX, dpiY;
            if (source != null)
            {
                dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;
                zoomfactor = dpiX / 96;
            }
            return zoomfactor;
        }

        #endregion
    }
}
