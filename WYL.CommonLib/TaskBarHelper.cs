using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Edu.CommonLibCore
{
    /// <summary>
    /// 任务栏操作工具类
    /// </summary>
    public class TaskBarHelper
    {
        struct RECT
        {
            public int left, top, right, bottom;
        }

        /// <summary>
        /// 刷新托盘的区域
        /// </summary>
        public static void RefreshNotificationArea()
        {
            try
            {
                var notificationAreaHandle = GetNotificationAreaHandle();
                if (notificationAreaHandle != IntPtr.Zero)
                    RefreshWindow(notificationAreaHandle);

                var notifyOverHandle = GetNotifyOverHandle();
                if (notifyOverHandle != IntPtr.Zero)
                    RefreshWindow(notifyOverHandle);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //向托盘所在区域模拟发送wmMousemove消息，清楚无效托盘图标
        private static void RefreshWindow(IntPtr windowHandle)
        {
            try
            {
                const uint wmMousemove = 0x0200;
                RECT rect;
                GetClientRect(windowHandle, out rect);
                for (var x = 0; x < rect.right; x += 5)
                    for (var y = 0; y < rect.bottom; y += 5)
                        SendMessage(windowHandle, wmMousemove, 0, (y << 16) + x);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //获取托盘所在折叠区域的句柄
        static IntPtr GetNotifyOverHandle()
        {
            try
            {
                var OverHandle = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "NotifyIconOverflowWindow", string.Empty);

                List<string> toolAearNameList = GetToolAreaList();

                IntPtr NotifyOverHandle = IntPtr.Zero;
                NotifyOverHandle = FindWindowEx(OverHandle, IntPtr.Zero, "ToolbarWindow32", null);
                return NotifyOverHandle;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return IntPtr.Zero;
        }

        //托盘区域的名称，可使用Spy++查看
        static List<string> GetToolAreaList()
        {
            List<string> toolAearNameList = new List<string>();
            //win10
            toolAearNameList.Add("Notification Area");
            toolAearNameList.Add("用户提示通知区域");

            //win7
            toolAearNameList.Add("User Promoted Notification Area");
            toolAearNameList.Add("用户显示的通知区域");
            return toolAearNameList;
        }

        //获取托盘所在通知区域的句柄
        static IntPtr GetNotificationAreaHandle()
        {
            try
            {
                List<string> toolAearNameList = GetToolAreaList();

                var systemTrayContainerHandle = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", string.Empty);
                var systemTrayHandle = FindWindowEx(systemTrayContainerHandle, IntPtr.Zero, "TrayNotifyWnd", string.Empty);

                var sysPagerHandle = FindWindowEx(systemTrayHandle, IntPtr.Zero, "SysPager", string.Empty);
                IntPtr notificationAreaHandle = IntPtr.Zero;
                foreach (var itemAreaName in toolAearNameList)
                {
                    notificationAreaHandle = FindWindowEx(sysPagerHandle, IntPtr.Zero, "ToolbarWindow32", itemAreaName);
                    if (notificationAreaHandle != IntPtr.Zero) break;
                }
                return notificationAreaHandle;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return IntPtr.Zero;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);
        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr handle, out RECT rect);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr handle, UInt32 message, Int32 wParam, Int32 lParam);

    }
}
