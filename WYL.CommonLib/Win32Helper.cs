﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Edu.CommonLibCore
{
    /// <summary>
    /// Win32操作的工具类
    /// <para>author:wangyulong</para>
    /// </summary>
    public class Win32Helper
    {
        // Sent to a window when the size or position of the window is about to change
        public const int WM_GETMINMAXINFO = 0x0024;

        // Retrieves a handle to the display monitor that is nearest to the window
        public const int MONITOR_DEFAULTTONEAREST = 2;


        // Point structure, Point information used by MINMAXINFO structure
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;

            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        // MINMAXINFO structure, Window's maximum size and position information
        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize; // The maximized size of the window
            public POINT ptMaxPosition; // The position of the maximized window
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        // Retrieves a handle to the display monitor
        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, int dwFlags);


        // MONITORINFOEX structure, Monitor information used by GetMonitorInfo function
        [StructLayout(LayoutKind.Sequential)]
        public class MONITORINFOEX
        {
            public int cbSize;
            public RECT rcMonitor; // The display monitor rectangle
            public RECT rcWork; // The working area rectangle
            public int dwFlags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
            public char[] szDevice;
        }

        // Get the working area of the specified monitor
        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfo(HandleRef hmonitor, [In, Out] MONITORINFOEX monitorInfo);



        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        /// <summary>
        /// 调用windows API实现展台最小化
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="nCmdShow">1 恢复 2 最小化 3 最大化</param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "ShowWindow", CharSet = CharSet.Auto)]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

        /// <summary>
        /// 查找窗口
        /// </summary>
        /// <param name="lpClassName"></param>
        /// <param name="lpWindowName"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr ptr);

        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(
          IntPtr hObject, int nXDest, int nYDest, int nWidth,
         int nHeight, IntPtr hObjSource, int nXSrc, int nYSrc,
          TernaryRasterOperations dwRop);


        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        [DllImport("user32.dll")]
        public static extern int GetWindowRgn(IntPtr hWnd, IntPtr hRgn);


        #region 剪切板相关


        [DllImport("User32")]
        public static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("User32")]
        public static extern bool CloseClipboard();

        [DllImport("User32")]
        public static extern bool EmptyClipboard();

        [DllImport("User32")]
        public static extern bool IsClipboardFormatAvailable(int format);

        [DllImport("User32")]
        public static extern IntPtr GetClipboardData(int uFormat);

        [DllImport("User32", CharSet = CharSet.Unicode)]
        public static extern IntPtr SetClipboardData(int uFormat, IntPtr hMem);

        /// <summary>
        /// 复制文本
        /// </summary>
        /// <param name="value"></param>
        public static void Copy(string value)
        {
            if (!OpenClipboard(IntPtr.Zero))
            {
                Copy(value);
                return;
            }
            EmptyClipboard();
            SetClipboardData(13, Marshal.StringToHGlobalUni(value));
            CloseClipboard();
        }

        #endregion


        #region 设置系统时间

        [DllImportAttribute("Kernel32.dll")]
        public static extern void GetLocalTime(SystemTime st);

        [DllImportAttribute("Kernel32.dll")]

        public static extern void SetLocalTime(SystemTime st);

        #endregion

    }


    [StructLayoutAttribute(LayoutKind.Sequential)]
    public class SystemTime

    {

        public ushort vYear;

        public ushort vMonth;

        public ushort vDayOfWeek;

        public ushort vDay;

        public ushort vHour;

        public ushort vMinute;

        public ushort vSecond;

    }


    public enum TernaryRasterOperations
    {
        SRCCOPY = 0x00CC0020, /* dest = source*/
        SRCPAINT = 0x00EE0086, /* dest = source OR dest*/
        SRCAND = 0x008800C6, /* dest = source AND dest*/
        SRCINVERT = 0x00660046, /* dest = source XOR dest*/
        SRCERASE = 0x00440328, /* dest = source AND (NOT dest )*/
        NOTSRCCOPY = 0x00330008, /* dest = (NOT source)*/
        NOTSRCERASE = 0x001100A6, /* dest = (NOT src) AND (NOT dest) */
        MERGECOPY = 0x00C000CA, /* dest = (source AND pattern)*/
        MERGEPAINT = 0x00BB0226, /* dest = (NOT source) OR dest*/
        PATCOPY = 0x00F00021, /* dest = pattern*/
        PATPAINT = 0x00FB0A09, /* dest = DPSnoo*/
        PATINVERT = 0x005A0049, /* dest = pattern XOR dest*/
        DSTINVERT = 0x00550009, /* dest = (NOT dest)*/
        BLACKNESS = 0x00000042, /* dest = BLACK*/
        WHITENESS = 0x00FF0062, /* dest = WHITE*/
    }

    public class Gdi32
    {
        public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter
        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
        int nWidth, int nHeight, IntPtr hObjectSource,
        int nXSrc, int nYSrc, int dwRop);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
        int nHeight);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hDC);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
    }
}
