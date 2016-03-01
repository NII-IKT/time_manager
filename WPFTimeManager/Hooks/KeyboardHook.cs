using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NLog;

namespace WPFTimeManager
{

    /// <summary>
    /// Класс для перехвата событий с клавиатуры
    /// </summary>
    public class KeyboardHook
    {
        /// <summary>
        /// Событие перехвата нажатия комбинации клавиш для открытия окна программы
        /// </summary>
        public event EventHandler OpenEvent;

        /// <summary>
        /// Событие нажатия клавиши пользователем
        /// </summary>
        public event EventHandler KeyPressedEvent;

        private IntPtr fhook = IntPtr.Zero;
        private delegate int keyboardHookProc(int code, int wParam, ref keyboardHookStruct lParam);
        
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static keyboardHookProc callbackDelegate;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP = 0x105;
        
        private struct keyboardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        ~KeyboardHook()
        {
            unhook();
        }
        
        /// <summary>
        /// Включает отслеживание событий клавиатуры
        /// </summary>
        public void hook()
        {
            try
            {
                IntPtr hInstance = LoadLibrary("User32");
                callbackDelegate = new keyboardHookProc(findPressing);
                fhook = SetWindowsHookEx(WH_KEYBOARD_LL, callbackDelegate, hInstance, 0);
            }
            catch (Exception ex)
            {
                logger.Error("ERROR:HOOK K:{0}", ex.Message);
                unhook();
                hook();
            }
        }

        /// <summary>
        /// Выключает отслеживание событий клавиатуры
        /// </summary>
        public void unhook()
        {
            try
            {
                UnhookWindowsHookEx(fhook);
            }
            catch (Exception ex)
            {
                logger.Error("ERROR:UNHOOK K:{0}", ex.Message);
                unhook();
                hook();
            }
        }

        // основной метод класса - в нем происходит перхват и вызов событий
        private int findPressing(int code, int wParam, ref keyboardHookStruct lParam)
        {
            try
            {
                if (code >= 0)
                {
                    Keys key = (Keys)lParam.vkCode;
                    {
                        if ((key == Keys.F11) && (Control.ModifierKeys == (Keys.Alt|Keys.Shift|Keys.Control)) && (GetAsyncKeyState(Keys.LControlKey) < 0) && (GetAsyncKeyState(Keys.RShiftKey) < 0))
                        {
                            if ((wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN) && (OpenEvent != null))
                            {
                                OpenEvent(this, new EventArgs());
                            }
                            return CallNextHookEx(fhook, code, wParam, ref lParam);
                        }
                        if ((wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN) && (KeyPressedEvent != null))
                        {
                            KeyPressedEvent(this, new EventArgs());
                        }
                        return CallNextHookEx(fhook, code, wParam, ref lParam);
                    }
                }
                return CallNextHookEx(fhook, code, wParam, ref lParam);
            }
            catch (Exception ex)
            {
                logger.Error("ERROR:KEYBOARD HOOK:{0}", ex.Message);
                return CallNextHookEx(fhook, code, wParam, ref lParam);
            }
        }

        #region импорт методов из user32.dll

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, keyboardHookProc callback, IntPtr hInstance, uint threadId);

        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref keyboardHookStruct lParam);

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys key);

        #endregion

    }
}
