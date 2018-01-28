using System;
using System.Runtime.InteropServices;

namespace BackgroundProgram
{
    class Program
    {
        public static void Main(string[] args)
        {
            var program = FindWindow(null, "Program Manager");
            IntPtr result = IntPtr.Zero;
            SendMessageTimeout(program, 0x052C, new IntPtr(0), IntPtr.Zero, SendMessageTimeoutFlags.SMTO_NORMAL, 1000, out result);

            var workerw = IntPtr.Zero;

            EnumWindows((station, param) =>
            {
                var p = FindWindowEx(station, IntPtr.Zero, "SHELLDLL_DefView", null);

                if (p != IntPtr.Zero)
                {
                    workerw = FindWindowEx(IntPtr.Zero, station, "WorkerW", null);
                }
                return true;
            }, IntPtr.Zero);



            var overwatch = FindWindow(null, "Overwatch");

            SetParent(overwatch, workerw);
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowStationsDelegate lpEnumFunc,
            IntPtr lParam);

        private delegate bool EnumWindowStationsDelegate(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string className, string windowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

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
    }
}
