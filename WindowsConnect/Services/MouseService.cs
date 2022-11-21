using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;

namespace WindowsConnect.Services
{
    public class MouseService
    {
        public struct POINT
        {
            public int X;
            public int Y;
        }

        static int _x, _y;
        static int _singleDownX, _singleDownY;
        static int _multiplyDownX, _multiplyDownY;
        static int _upX, _upY;

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern long SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const int MOUSEEVENTF_LEFTUP = 0x0004;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const int MOUSEEVENTF_RIGHTUP = 0x0010;
        private const int MOUSEEVENTF_WHEEL = 0x0800;

        public static void SetMousePosition()
        {
            POINT point;
            if (GetCursorPos(out point) && point.X != _x && point.Y != _y)
            {
                _x = point.X;
                _y = point.Y;
            }
        }

        //This simulates a left mouse click
        public static void LeftMouseClick()
        {
            SetMousePosition();
            mouse_event(MOUSEEVENTF_LEFTDOWN, _x, _y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, _x, _y, 0, 0);
        }

        public static void RigthMouseClick()
        {
            SetMousePosition();
            mouse_event(MOUSEEVENTF_RIGHTDOWN, _x, _y, 0, 0);
            mouse_event(MOUSEEVENTF_RIGHTUP, _x, _y, 0, 0);
        }
       

        public static void setDownCoordinates(int x, int y)
        {
            SetMousePosition();
            _singleDownX = x;
            _singleDownY = y;

            _multiplyDownX = x;
            _multiplyDownY = y;
        }

        public static void MoveCursor(int x, int y)
        {
            var p = new POINT();

            p.X = _x + (x - _singleDownX);
            p.Y = _y + (y - _singleDownY);

            ClientToScreen(Cursor.Current.Handle, ref p);
            SetCursorPos(p.X, p.Y);
        }

        public static void MoveMouseWheel(int x, int y)
        {
            int a = (int)((y - _multiplyDownY) * 1.5);
            mouse_event(MOUSEEVENTF_WHEEL, 0, 0, a, 0);
            _multiplyDownY = y;
        }
    }
}
