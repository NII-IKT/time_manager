using System;
using System.Runtime.InteropServices;
using NLog;

namespace WPFTimeManager
{
    /// <summary>
    /// Класс для перехвата событий с мыши
    /// </summary>
    public class MouseHook
    {
        /// <summary>
        /// Событие перехвата движения мыш
        /// </summary>
        public event EventHandler MouseMovedEvent;

        private IntPtr fhook = IntPtr.Zero;
        private delegate int MouseHookProc(int code, int wParam, ref MouseStruct lParam);
        
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static MouseHookProc callbackDelegate;

        private const int WH_MOUSE_LL = 14;
        private const int WM_MOUSEMOVE = 0x0200;

        private struct MouseStruct
        {
            public int pt;
            public int mouseData;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        ~MouseHook()
        {
            unhook();
        }

        /// <summary>
        /// Включает отслеживание событий мыши
        /// </summary>
        public void hook()
        {
            try
            {
                IntPtr hInstance = LoadLibrary("User32");
                callbackDelegate = new MouseHookProc(findMoving);
                fhook = SetWindowsHookEx(WH_MOUSE_LL, callbackDelegate, hInstance, 0);
            }
            catch (Exception ex)
            {
                logger.Error("ERROR:HOOK M:{0}", ex.Message);
                unhook();
                hook();
            }
        }

        /// <summary>
        /// Выключает отслеживание событий мыши
        /// </summary>
        public void unhook()
        {
            try
            {
                UnhookWindowsHookEx(fhook);
            }
            catch (Exception ex)
            {
                logger.Error("ERROR:UNHOOK M:{0}", ex.Message);
            }
        }

        // основной метод класса - в нем происходит перхват и вызов событий
        private int findMoving(int code, int wParam, ref MouseStruct lParam)
        {
            try
            {
                if (code >= 0)
                {
                    if ((wParam == WM_MOUSEMOVE) && (MouseMovedEvent != null))
                    {
                        MouseMovedEvent(this, new EventArgs());
                    }
                    return CallNextHookEx(fhook, code, wParam, ref lParam);
                }
                return CallNextHookEx(fhook, code, wParam, ref lParam);
            }
            catch (Exception ex)
            {
                logger.Error("ERROR:MOUSE HOOK:{0}", ex.Message);
                return CallNextHookEx(fhook, code, wParam, ref lParam);
            }
        }

        #region импорт методов из user32.dll

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, MouseHookProc callback, IntPtr hInstance, uint threadId);

        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref MouseStruct lParam);

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);
        
        #endregion

    }
}
