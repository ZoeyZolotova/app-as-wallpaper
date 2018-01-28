using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackgroundProgramForm
{
    public class InfoWindow
    {
        public IntPtr Handle = IntPtr.Zero;
        public FileInfo File = new FileInfo(Application.ExecutablePath);
        public string Title = Application.ProductName;
        public override string ToString()
        {
            return File.Name + "\t>\t" + Title;
        }
    }//CLASS

    /// <summary>Contains functionality to get info on the open windows.</summary>
    public static class RunningWindows
    {
        internal static IntPtr _workerw = IntPtr.Zero;

        internal static event EventHandler WindowActivatedChanged;
        internal static Timer TimerWatcher = new Timer();
        internal static InfoWindow WindowActive = new InfoWindow();
        internal static void DoStartWatcher()
        {
            TimerWatcher.Interval = 500;
            TimerWatcher.Tick += TimerWatcher_Tick;
            TimerWatcher.Start();
        }

        /// <summary>Returns a dictionary that contains the handle and title of all the open windows.</summary>
        /// <returns>A dictionary that contains the handle and title of all the open windows.</returns>
        public static IDictionary<IntPtr, InfoWindow> GetOpenedWindows()
        {
            IntPtr shellWindow = GetShellWindow();
            Dictionary<IntPtr, InfoWindow> windows = new Dictionary<IntPtr, InfoWindow>();

            EnumWindows(new EnumWindowsProc(delegate (IntPtr hWnd, int lParam) {
                if (hWnd == shellWindow) return true;
                if (!IsWindowVisible(hWnd)) return true;
                int length = GetWindowTextLength(hWnd);
                if (length == 0) return true;
                StringBuilder builder = new StringBuilder(length);
                GetWindowText(hWnd, builder, length + 1);
                var info = new InfoWindow();
                info.Handle = hWnd;
                //info.File = new FileInfo(GetProcessPath(hWnd));
                info.Title = builder.ToString();
                windows[hWnd] = info;
                return true;
            }), IntPtr.Zero);
            return windows;
        }

        public static IDictionary<IntPtr, InfoWindow> GetBackgroundWindows()
        {
            IntPtr shellWindow = GetOrCreateBackground();
            Dictionary<IntPtr, InfoWindow> windows = new Dictionary<IntPtr, InfoWindow>();

            EnumChildWindows(shellWindow, new EnumWindowsProc(delegate (IntPtr hWnd, int lParam) {
                if (hWnd == shellWindow) return true;
                if (!IsWindowVisible(hWnd)) return true;
                int length = GetWindowTextLength(hWnd);
                if (length == 0) return true;
                StringBuilder builder = new StringBuilder(length);
                GetWindowText(hWnd, builder, length + 1);
                var info = new InfoWindow();
                info.Handle = hWnd;
                //info.File = new FileInfo(GetProcessPath(hWnd));
                info.Title = builder.ToString();
                windows[hWnd] = info;
                return true;
            }), IntPtr.Zero);
            return windows;
        }

        private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

        public static string GetProcessPath(IntPtr hwnd)
        {
            uint pid = 0;
            GetWindowThreadProcessId(hwnd, out pid);
            if (hwnd != IntPtr.Zero)
            {
                if (pid != 0)
                {
                    var process = Process.GetProcessById((int)pid);
                    if (process != null)
                    {
                        return process.MainModule.FileName.ToString();
                    }
                }
            }
            return "";
        }

        public static IntPtr GetOrCreateBackground()
        {
            if (_workerw == IntPtr.Zero)
            {
                var workerw = IntPtr.Zero;

                EnumWindows((station, param) =>
                {
                    var p = RunningWindows.FindWindowEx(station, IntPtr.Zero, "SHELLDLL_DefView", null);

                    if (p != IntPtr.Zero)
                    {
                        workerw = RunningWindows.FindWindowEx(IntPtr.Zero, station, "WorkerW", null);
                    }
                    return true;
                }, IntPtr.Zero);

                if (workerw == IntPtr.Zero)
                {
                    var program = RunningWindows.FindWindow(null, "Program Manager");
                    IntPtr result = IntPtr.Zero;
                    RunningWindows.SendMessageTimeout(program, 0x052C, new IntPtr(0), IntPtr.Zero, RunningWindows.SendMessageTimeoutFlags.SMTO_NORMAL, 1000, out result);

                    return GetOrCreateBackground();
                }

                _workerw = workerw;
            }

            return _workerw;
        }

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumChildWindows(IntPtr window, EnumWindowsProc callback, IntPtr lParam);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, IntPtr lParam);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        //WARN: Only for "Any CPU":
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out uint processId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string className, string windowName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(
            IntPtr windowHandle,
            uint Msg,
            IntPtr wParam,
            IntPtr lParam,
            SendMessageTimeoutFlags flags,
            uint timeout,
            out IntPtr result);

        [Flags]
        public enum SendMessageTimeoutFlags : uint
        {
            SMTO_NORMAL = 0x0,
            SMTO_BLOCK = 0x1,
            SMTO_ABORTIFHUNG = 0x2,
            SMTO_NOTIMEOUTIFNOTHUNG = 0x8,
            SMTO_ERRORONEXIT = 0x20
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);


        static void TimerWatcher_Tick(object sender, EventArgs e)
        {
            var windowActive = new InfoWindow();
            windowActive.Handle = GetForegroundWindow();
            string path = GetProcessPath(windowActive.Handle);
            if (string.IsNullOrEmpty(path)) return;
            windowActive.File = new FileInfo(path);
            int length = GetWindowTextLength(windowActive.Handle);
            if (length == 0) return;
            StringBuilder builder = new StringBuilder(length);
            GetWindowText(windowActive.Handle, builder, length + 1);
            windowActive.Title = builder.ToString();
            if (windowActive.ToString() != WindowActive.ToString())
            {
                //fire:
                WindowActive = windowActive;
                if (WindowActivatedChanged != null) WindowActivatedChanged(sender, e);
                Console.WriteLine("Window: " + WindowActive.ToString());
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    }//CLASS
}
