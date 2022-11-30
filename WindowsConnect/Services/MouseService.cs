using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsConnect.Models;

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
        static int _bufferX, _bufferY;

        int w = SystemInformation.VirtualScreen.Width;
        int h = SystemInformation.VirtualScreen.Height;

        static Cursor _cursor = Cursor.Current;

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern long SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const int MOUSEEVENTF_LEFTUP = 0x0004;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const int MOUSEEVENTF_RIGHTUP = 0x0010;
        private const int MOUSEEVENTF_WHEEL = 0x0800;

        private const int TIME_LEFT_MOUSE_CLICK = 100;
        private const int TIME_DOUBLE_HOOK_CLICK = 150;
        private const int TIME_HOOK_CLICK = 150;
        private const int TIME_MULTI_CLICK = 100;


        static bool hookClick = false; // для иммитации зажатой левой мышки при удержании
        static bool doubleHookClick = false; // для иммитации зажатой левой мышки  
        static bool leftClick = false; //для иммитации левого клика мыши
        static bool multiClick = false; //для иммитации правого клика мыши
        static bool doubleUp = false; //для иммитации правого клика мыши

        public static void VirtualTouchPadChanged(int x, int y, int action, int pointer)
        {
            switch (action)
            {
                case MouseEvent.ACTION_MOVE:

                    if (doubleUp)
                    {
                        doubleUp = false;
                        setDownCoordinates(x, y);
                    }
                    if (pointer > 1 )
                    {
                        MoveMouseWheel(x, y);
                    }
                    else
                    {
                        if (_bufferX != x || _bufferY != y)
                        {
                            hookClick = false;
                        }

                        if (hookClick)
                        {
                            LeftMouseClickDown();
                            hookClick = false;
                        }
                        
                        MoveCursor(x, y);
                    }
                    break;
                case MouseEvent.ACTION_DOWN:
                    setDownCoordinates(x, y);
                    _bufferX = x; _bufferY = y;
                    hookClick = false;
                    Task.Delay(TIME_HOOK_CLICK).ContinueWith(_ => { hookClick = true; });

                  
                    if (doubleHookClick)
                    {
                        LeftMouseClickDown();
                    }
                    else
                    {
                        leftClick = true;
                        Task.Delay(TIME_LEFT_MOUSE_CLICK).ContinueWith(_ => { leftClick = false; });
                    }

                   
                    break;
                case MouseEvent.ACTION_UP:
                    if (!multiClick)
                    {
                        LeftMouseClickUp();
                    }
                    hookClick = false;
                    doubleHookClick = false;

                    if (leftClick && !multiClick)
                    {
                        doubleHookClick = true;
                      
                        Task.Delay(TIME_DOUBLE_HOOK_CLICK).ContinueWith(_ => { doubleHookClick = false; });
                        LeftMouseClick();
                    }

                    break;
                case MouseEvent.ACTION_POINTER_DOWN:
                    multiClick = true;
                    Task.Delay(TIME_MULTI_CLICK).ContinueWith(_ => {
                        multiClick = false;
                    });
                    break;
                case MouseEvent.ACTION_POINTER_UP:
                    doubleUp = true;
                    setDownCoordinates(x, y);
                    if (multiClick)
                    {
                        RigthMouseClick();
                    }
                   
                    break;
            }
        }

        private static void MoveCursor(int x, int y)
        {
            var p = new POINT();

            p.X = _x + (x - _singleDownX);
            p.Y = _y + (y - _singleDownY);

            ClientToScreen(_cursor.Handle, ref p);
            SetCursorPos(p.X, p.Y);
        }

        public static void SetMousePosition()
        {
            POINT point;
            if (GetCursorPos(out point) && point.X != _x && point.Y != _y)
            {
                _x = point.X;
                _y = point.Y;
            }
        }

        private static void setDownCoordinates(int x, int y)
        {
            SetMousePosition();
            _singleDownX = x;
            _singleDownY = y;

            _multiplyDownX = x;
            _multiplyDownY = y;
        }

        private static void LeftMouseClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, _x, _y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, _x, _y, 0, 0);
        }

        private static void RigthMouseClick()
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN, _x, _y, 0, 0);
            mouse_event(MOUSEEVENTF_RIGHTUP, _x, _y, 0, 0);
        }

        private static void LeftMouseClickDown()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, _x, _y, 0, 0);
        } 

        private static void LeftMouseClickUp()
        {
 
            mouse_event(MOUSEEVENTF_LEFTUP, _x, _y, 0, 0);
        }
 
        private static void MoveMouseWheel(int x, int y)
        {
            int a = (int)((y - _multiplyDownY) * 2);
            mouse_event(MOUSEEVENTF_WHEEL, 0, 0, a, 0);
            _multiplyDownY = y;
        }
    }
}
