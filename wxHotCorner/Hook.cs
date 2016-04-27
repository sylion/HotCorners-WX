using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace HCWX
{
    static class MouseHook
    {
        #region Import
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion
        private static LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private const int WH_MOUSE_LL = 14;
        private static bool hooked = false;

        public static void Hook()
        {
            if (!hooked)
            {
                _hookID = SetHook(_proc);
                hooked = true;
            }
        }

        public static void unHook()
        {
            if (hooked)
            {
                UnhookWindowsHookEx(_hookID);
                hooked = false;
            }
        }

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && MouseMessages.WM_MOUSEMOVE == (MouseMessages)wParam)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                HotCorner.CornerAction(hookStruct.pt.x, hookStruct.pt.y);
            }
            //Mouse wheel action
            if (nCode >= 0 && MouseMessages.WM_MOUSEWHEEL == (MouseMessages)wParam)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                //Mouse wheel down
                if (hookStruct.mouseData < 0)
                {
                    //IntPtr hwnd = WindowFromPoint(hookStruct.pt.x, hookStruct.pt.y);
                    //if (hwnd != IntPtr.Zero)
                    //{
                    //    MessageBox.Show();
                    //}
                    //IntPtr hwnd = WindowFromPoint(hookStruct.pt.x, hookStruct.pt.y);
                    //if (hwnd != IntPtr.Zero)
                    //{
                    //    IntPtr hWndParent = GetParent(hwnd);
                    //    if (hWndParent != IntPtr.Zero)
                    //    {
                    //        MessageBox.Show(hwnd.ToString());
                    //    }
                    //}
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        #region TEST
        [DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(int x, int y);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);
        #endregion


        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public int mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
    }

    public static class SendHotKey
    {
        #region Import
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        #endregion

        private const int KEYEVENTF_EXTENDEDKEY = 1;
        private const int KEYEVENTF_KEYUP = 2;

        public static void KeyDown(Keys vKey)
        {
            keybd_event((byte)vKey, 0, KEYEVENTF_EXTENDEDKEY, 0);
        }

        public static void KeyUp(Keys vKey)
        {
            keybd_event((byte)vKey, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }
    }

    static class HotCorner
    {
        private static bool Locked = false;

        public static void CornerAction(int x, int y)
        {
            Corner corner = getCorner(x, y);
            switch (corner)
            {
                case Corner.TopLeft:
                    if (!Locked)
                        doThis((Action)Properties.Settings.Default.TL);
                    break;
                case Corner.TopRight:
                    if (!Locked)
                        doThis((Action)Properties.Settings.Default.TR);
                    break;
                case Corner.BottomLeft:
                    if (!Locked)
                        doThis((Action)Properties.Settings.Default.BL);
                    break;
                case Corner.BottomRight:
                    if (!Locked)
                        doThis((Action)Properties.Settings.Default.BR);
                    break;
                default:
                    Unlock(x, y);
                    break;
            }
        }

        private static void doThis(Action act)
        {
            Locked = true;
            switch (act)
            {
                case Action.ShowDesktop:
                    SendHotKey.KeyDown(Keys.LWin);
                    SendHotKey.KeyDown(Keys.D);
                    SendHotKey.KeyUp(Keys.LWin);
                    SendHotKey.KeyUp(Keys.D);
                    break;
                case Action.ShowNotifications:
                    SendHotKey.KeyDown(Keys.LWin);
                    SendHotKey.KeyDown(Keys.A);
                    SendHotKey.KeyUp(Keys.LWin);
                    SendHotKey.KeyUp(Keys.A);
                    break;
                case Action.StartMenu:
                    SendHotKey.KeyDown(Keys.LWin);
                    SendHotKey.KeyUp(Keys.LWin);
                    break;
                case Action.SwicthWindow:
                    SendHotKey.KeyDown(Keys.LWin);
                    SendHotKey.KeyDown(Keys.Tab);
                    SendHotKey.KeyUp(Keys.LWin);
                    SendHotKey.KeyUp(Keys.Tab);
                    break;
                default:
                    break;
            }
        }

        private static Corner getCorner(int x, int y)
        {
            int Y = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            int X = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;

            if (x <= 0 + Properties.Settings.Default.numTL && y <= 0 + Properties.Settings.Default.numTL)
                return Corner.TopLeft;
            if (x >= X - Properties.Settings.Default.numTR && y <= 0 + Properties.Settings.Default.numTR)
                return Corner.TopRight;
            if (x <= 0 + Properties.Settings.Default.numBL && y >= Y - Properties.Settings.Default.numBL)
                return Corner.BottomLeft;
            if (x >= X - Properties.Settings.Default.numBR && y >= Y - Properties.Settings.Default.numBR)
                return Corner.BottomRight;

            return Corner.None;
        }

        private static void Unlock(int x, int y)
        {
            int Y = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            int X = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            if (y > 5 && y <= Y - 5 && x > 5 && x <= X - 5)
                Locked = false;
        }

        enum Corner
        {
            None = 0,
            TopLeft = 1,
            TopRight = 2,
            BottomLeft = 3,
            BottomRight = 4
        }

        enum Action
        {
            None = 0,
            SwicthWindow = 1,
            StartMenu = 2,
            ShowDesktop = 3,
            ShowNotifications = 4
        }
    }
}
