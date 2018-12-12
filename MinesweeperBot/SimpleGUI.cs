using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace MinesweeperBot
{
    public class SimpleGUI 
    {
        #region user32.dll imports
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        [DllImport("user32.dll", EntryPoint = "MoveWindow")]
        public static extern IntPtr MoveWindow(IntPtr hWnd, int x, int Y, int nWidth, int nHeight, bool bRepaint);

        const uint MOUSEEVENTF_WHEEL = 0x0800;
        const uint MOUSEEVENTF_HWHEEL = 0x01000;
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        static readonly IntPtr HWND_TOP = new IntPtr(0);
        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        /// <summary>
        /// Window handles (HWND) used for hWndInsertAfter
        /// </summary>
        public static class HWND
        {
            public static IntPtr
            NoTopMost = new IntPtr(-2),
            TopMost = new IntPtr(-1),
            Top = new IntPtr(0),
            Bottom = new IntPtr(1);
        }

        /// <summary>
        /// SetWindowPos Flags
        /// </summary>
        public static class SWP
        {
            public static readonly int
            NOSIZE = 0x0001,
            NOMOVE = 0x0002,
            NOZORDER = 0x0004,
            NOREDRAW = 0x0008,
            NOACTIVATE = 0x0010,
            DRAWFRAME = 0x0020,
            FRAMECHANGED = 0x0020,
            SHOWWINDOW = 0x0040,
            HIDEWINDOW = 0x0080,
            NOCOPYBITS = 0x0100,
            NOOWNERZORDER = 0x0200,
            NOREPOSITION = 0x0200,
            NOSENDCHANGING = 0x0400,
            DEFERERASE = 0x2000,
            ASYNCWINDOWPOS = 0x4000;
        }
        #endregion

        #region IUIInterface

        public Bitmap GetScreen()
        {
            throw new NotImplementedException();
        }

        public int GetMouseX()
        {
            return Cursor.Position.X;
        }

        public int GetMouseY()
        {
            return Cursor.Position.Y;

        }

        public void SetMouseX(int x)
        {
            Cursor.Position = new Point(x, Cursor.Position.Y);

        }

        public void SetMouseY(int y)
        {
            Cursor.Position = new Point(Cursor.Position.X, y);
        }
        public void Wheel(bool up)
        {
            uint increment = 120;
            mouse_event(MOUSEEVENTF_WHEEL, 0, 0, up ? increment : -increment, 0);
        }
        public void LeftDown()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
        }
        public void RightDown()
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
        }
        public void RightUp()
        {
            mouse_event(MOUSEEVENTF_RIGHTUP, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
        }
        public void LeftUp()
        {
            mouse_event(MOUSEEVENTF_LEFTUP, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
        }
        public void LeftClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
        }

        public void DblLeftClick()
        {
            LeftClick();
            LeftClick();
        }

        public void RightClick()
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
        }

        public void TypeText(string text)
        {
            throw new NotImplementedException();
        }

        public void HoverOn(int x, int y)
        {
            Cursor.Position = new Point(x, y);
        }

        public void HoverOnRel(int dx, int dy)
        {
            Cursor.Position = new Point(Cursor.Position.X + dx, Cursor.Position.Y + dy);
        }

        #endregion


        public void SetPosition(Point position)
        {
            Cursor.Position = new Point(position.X, position.Y);
        }
    }
}
