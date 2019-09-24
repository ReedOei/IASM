using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

class MouseOutput
{
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

    private static int MouseLeftDown = 0x02;
    private static int MouseLeftUp = 0x04;
    private static int MouseRightDown = 0x08;
    private static int MouseRightUp = 0x10;

    public static void LeftClick(int x, int y)
    {
        Cursor.Position = new Point(x, y);

        mouse_event(MouseLeftDown, x, y, 0, 0);
        mouse_event(MouseLeftUp, x, y, 0, 0);
    }

    public static void RightClick(int x, int y)
    {
        Cursor.Position = new Point(x, y);

        mouse_event(MouseRightDown, x, y, 0, 0);
        mouse_event(MouseRightUp, x, y, 0, 0);
    }
}
