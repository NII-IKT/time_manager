using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using NDde.Client;
using System.Windows.Automation;
using System.Text.RegularExpressions;
using System.Drawing;
using NLog;

namespace WPFTimeManager
{
    /// <summary>
    /// Класс для поиска текущего активного окна
    /// </summary>
    public static class ProcessHook
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Возвращает информацию о текущем активном окне
        /// </summary>
        /// <param name="title">Иконка окна процесса</param>
        /// <param name="icon">Заголовок окна процесса</param>
        /// <returns>Имя процесса</returns>
        public static string FindActiveWindow(out string title, out Icon icon)
        {
            IntPtr hWnd = GetForegroundWindow();
            title = null;
            icon = null;
            int pid;
            GetWindowThreadProcessId(hWnd, out pid);
            using (Process p = Process.GetProcessById((int)pid))
            {
                try
                {
                    title = p.MainWindowTitle;
                    //TODO реализовать перехват ICO exe файле процесса
                    //icon = Icon.ExtractAssociatedIcon(p.MainModule.FileName);
                    return p.ProcessName;
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    logger.Error("ОШИБКА:ВЗЯТИЕ АКТИВНОГО ПРОЦЕССА:{0}", ex.Message);
                    return p.ProcessName;
                }
                catch (Exception ex)
                {
                    logger.Error("ОШИБКА:ВЗЯТИЕ АКТИВНОГО ПРОЦЕССА:{0}", ex.Message);
                    return p.ProcessName;
                }
            }
        }

        /// <summary>
        /// Возвращет название активной вкладки браузера
        /// </summary>
        /// <param name="browser">название браузера</param>
        /// <param name="ptr"></param>
        /// <returns>Название вкладки</returns>
        public static string GetBrowserURL(string browser, IntPtr ptr = new IntPtr())
        {
            if (browser == "firefox")
            {
                try
                {
                    DdeClient dde = new DdeClient(browser, "WWW_GetWindowInfo");
                    dde.Connect();
                    string url = dde.Request("URL", int.MaxValue);
                    string[] text = url.Split(new string[] { "\",\"" }, StringSplitOptions.RemoveEmptyEntries);
                    dde.Disconnect();
                    return text[0].Substring(1);
                }
                catch
                {
                    return null;
                }
            }
            else if (browser == "chrome")
            {
                {
                    string ret = "";
                    
                    AutomationElement elm = AutomationElement.FromHandle(ptr);
                    AutomationElement elmUrlBar = null;
                    try
                    {
                        var elm1 = elm.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Google Chrome"));
                        if (elm1 == null) { return null; }
                        var elm2 = TreeWalker.RawViewWalker.GetLastChild(elm1); // I don't know a Condition for this for finding
                        var elm3 = elm2.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, ""));
                        var elm4 = TreeWalker.RawViewWalker.GetNextSibling(elm3); // I don't know a Condition for this for finding
                        var elm5 = elm4.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ToolBar));
                        var elm6 = elm5.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, ""));
                        elmUrlBar = elm6.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));
                    }
                    catch
                    {
                        return null;
                    }
                    AutomationPattern[] patterns = elmUrlBar.GetSupportedPatterns();
                    if (patterns.Length == 1)
                    {
                        try
                        {
                            ret = ((ValuePattern)elmUrlBar.GetCurrentPattern(patterns[0])).Current.Value;
                            //return ret;
                        }
                        catch { }
                        if (ret != "")
                        {
                            if (Regex.IsMatch(ret, @"^(https:\/\/)?[a-zA-Z0-9\-\.]+(\.[a-zA-Z]{2,4}).*$"))
                            {
                                if (!ret.StartsWith("http"))
                                {
                                    ret = "http://" + ret;
                                }
                                return ret;
                            }
                        }
                    }
                    return null;
                }
            }
            else return null;
        }

        public static IntPtr ReturnPointer()
        {
            return GetForegroundWindow();
        }

        #region импорт методов из user32.dll

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int pid);

        #endregion

    }
}
