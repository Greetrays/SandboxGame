using UnityEngine;
using System.Runtime.InteropServices;

namespace ThirdPersonCamera
{
    public class MouseCursorHelper
    {
#if UNITY_STANDALONE_WIN
        [StructLayout(LayoutKind.Sequential)]
        public struct MousePoint
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out MousePoint lpMousePoint);
        private static MousePoint cursorPoint;
        private static bool locked = false;

        private bool firstHide = true;

        public void Hide()
        {
            if (!locked)
            {
                GetCursorPos(out cursorPoint);

                Cursor.visible = false;
                locked = true;
            }
        }

        public void Show()
        {
            if (locked)
            {
                Cursor.visible = true;
                locked = false;
            }
        }

        public void Update()
        {
            if (locked)
            {
                SetCursorPos(cursorPoint.x, cursorPoint.y);
            }
        }

        public void Toggle()
        {
            if (locked)
                Show();
            else
                Hide();
        }
#endif    
    }
}
