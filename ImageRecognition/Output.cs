using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System;
using ImageRecognition;
using System.Text;

namespace ImageCompare
{
    public class MouseOutput
    {
        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private static int MouseLeftDown = 0x02;
        private static int MouseLeftUp = 0x04;
        private static int MouseRightDown = 0x08;
        private static int MouseRightUp = 0x10;

        private static int lastX = -1;
        private static int lastY = -1;

        public static void LeftClick(int x, int y)
        {
            CheckMouseMoved(x, y);

            ManualResetEvent Waiter = new ManualResetEvent(false);

            Cursor.Position = new Point(x, y);

            mouse_event(MouseLeftDown, x, y, 0, 0);
            Waiter.WaitOne(250);
            mouse_event(MouseLeftUp, x, y, 0, 0);
        }

        public static void RightClick(int x, int y)
        {
            CheckMouseMoved(x, y);

            Cursor.Position = new Point(x, y);

            ManualResetEvent Waiter = new ManualResetEvent(false);

            mouse_event(MouseRightDown, x, y, 0, 0);
            Waiter.WaitOne(250);
            mouse_event(MouseRightUp, x, y, 0, 0);
        }

        private static void CheckMouseMoved(int x, int y)
        {
            if ((lastX != -1 && lastX != Cursor.Position.X) || (lastY != -1 && lastY != Cursor.Position.Y))
            {
                Console.WriteLine("Mouse moved since last click, please press enter to resume.");
                Console.ReadLine();
            }

            lastX = x;
            lastY = y;
        }
    }
}
