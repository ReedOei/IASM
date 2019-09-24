using ImageRecognition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public class Window
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern int AttachThreadInput(int idAttach, int idAttachTo, bool fAttach);
        [DllImport("user32.dll")]
        public static extern IntPtr GetFocus();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);
        
        public static string GetWindowName(IntPtr h)
        {
            int l = GetWindowTextLength(h);
            StringBuilder builder = new StringBuilder(l + 1);
            GetWindowText(h, builder, builder.Capacity);
            return builder.ToString();
        }
        
        public static IEnumerable<Window> Windows()
        {
            List<Window> windows = new List<Window>();

            EnumWindows(new EnumWindowsProc(new WindowIterator(windows).Iterate), IntPtr.Zero);

            return windows;
        }

        public static Option<Window> FindWindow(string windowName)
        {
            return Windows().Find(w => w.Name().Contains(windowName));
        }

        private readonly IntPtr h;
        private readonly string name;

        public Window(IntPtr h)
        {
            this.h = h;
            this.name = GetWindowName(h);
        }

        public string Name()
        {
            return name;
        }

        public void Activate()
        {
            ShowWindow(h, (int) ShowWindowCommands.SW_SHOW);
            SetForegroundWindow(h);
            SetFocus(h);
        }

        public void Maximize()
        {
            ShowWindow(h, (int)ShowWindowCommands.SW_MAXIMIZE);
        }

        public void Minimize()
        {
            ShowWindow(h, (int) ShowWindowCommands.SW_MAXIMIZE);
        }

        public IntPtr GetHandle()
        {
            return h;
        }
    }

    internal class WindowIterator
    {
        private List<Window> windows;

        public WindowIterator(List<Window> windows)
        {
            this.windows = windows;
        }

        internal bool Iterate(IntPtr hWnd, IntPtr lParam)
        {
            windows.Add(new Window(hWnd));
            // Window.EnumChildWindows(hWnd, Iterate, IntPtr.Zero);
            return true;
        }
    }
}
