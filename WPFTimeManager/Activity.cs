using System;
using System.Collections.Generic;
using NLog;
using System.Drawing;
using System.Windows.Forms;

namespace WPFTimeManager
{
    /// <summary>
    /// Класс дней активности
    /// </summary>
    public class DateActivity 
    {
        public Dictionary<DateTime, DayActivity> log = new Dictionary<DateTime, DayActivity>();

        public void checkWork()
        {
            if (log.ContainsKey(DateTime.Today))
            {
                log[DateTime.Today].checkActivity();
            }
            else
            {
                log.Add(DateTime.Today, new DayActivity());
                log[DateTime.Today].checkActivity();
            }
        }
    }

    /// <summary>
    /// Класс дневной активности
    /// </summary>
    public class DayActivity
    {

        public static int timeIdle;
        public Dictionary<String, Dictionary<String, Activity>> data = new Dictionary<string, Dictionary<String, Activity>>();

        private int counter = 0;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public struct Activity
        {
            public long idleTime, sumActiveTime;
            public string title;
            public Icon icon;
        }

        public void addValues(string name, string subpage, string title, long idle, long active)
        {
            Activity t;
            t.idleTime = idle;
            t.sumActiveTime = active;
            t.title = title;
            t.icon = null;

            if (data.ContainsKey(name))
            {
                data[name].Add(subpage, t);
            }
            else
            {
                Dictionary<String, Activity> d = new Dictionary<string, Activity>();
                d.Add(subpage, t);
                data.Add(name, d);
            }
        }

        public void checkActivity()
        {
            try
            {
                String t, title, subpage;
                Icon icon;
                ////TODO реализовать проверку на отключение по RDP
                ////if (!SystemInformation.TerminalServerSession)
                ////{
                ////    t = "RDP Disabled";
                ////    subpage = "";
                ////    title = "";
                ////    MainWindow.isMouseActive = false;
                ////    MainWindow.isKeyboardActive = false;
                ////    counter = timeIdle + 1;
                ////}
                ////else 
                ////old code
                {
                    t = ProcessHook.FindActiveWindow(out subpage, out icon);

                    title = "";
                    if (t == "Idle")
                    {
                        MainWindow.isMouseActive = false;
                        MainWindow.isKeyboardActive = false;
                        counter = timeIdle + 1;
                    }
                    else if (t == "firefox")
                    {
                        if (ProcessHook.GetBrowserURL("firefox") != null)
                            subpage = ProcessHook.GetBrowserURL("firefox");
                        else if (subpage.Contains("Tor Browser"))
                        { t = "Tor browser"; }
                    }
                    else if (t == "chrome")
                    {
                        subpage = ProcessHook.GetBrowserURL("chrome", ProcessHook.ReturnPointer());
                    }
                }

                if (data.ContainsKey(t))
                {
                    Dictionary<String, Activity> temp = data[t];
                    if (subpage != default(string))
                        if (temp.ContainsKey(subpage))
                        {
                            Activity tempA = temp[subpage];
                            Activity s = temp["Суммарно"];
                            if (MainWindow.isKeyboardActive || MainWindow.isMouseActive)
                            {
                                tempA.sumActiveTime++; s.sumActiveTime++; counter = 0;
                            }
                            else if (counter < timeIdle)
                            {
                                tempA.sumActiveTime++; s.sumActiveTime++; counter++;
                            }
                            else if (!MainWindow.isMouseActive && !MainWindow.isKeyboardActive) { tempA.idleTime++; s.idleTime++; }
                            temp[subpage] = tempA;
                            //s.icon = icon;
                            temp["Суммарно"] = s;
                        }
                        else
                        {
                            temp.Add(subpage, new Activity());
                        }
                    data[t] = temp;
                    MainWindow.isMouseActive = false; MainWindow.isKeyboardActive = false;
                }
                else
                {
                    Dictionary<string, Activity> d = new Dictionary<string, Activity>();
                    d.Add("Суммарно", new Activity());
                    data.Add(t, d);
                }
            }
            catch (Exception ex)
            {
                logger.Error("ERROR:ACTIVITY CHECK:{0}", ex.Message);
            }
        }
    }

}