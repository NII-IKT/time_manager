using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Collections.ObjectModel;
using NLog;
using Ionic.Zip;
using System.Diagnostics;
using System.Reflection;
using System.Security.Permissions;

namespace WPFTimeManager
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region vars

        public static bool isMouseActive = false, isKeyboardActive = false;
        public List<DateTime> dateList = new List<DateTime>();
        public DateActivity dateActivity = new DateActivity();
        public string pathDirectory;

        static Logger logger = LogManager.GetCurrentClassLogger();

        Object l = new object(); //for lock

        KeyboardHook globalKeyboardHook = new KeyboardHook();
        MouseHook globalMouseHook = new MouseHook();

        System.Windows.Threading.DispatcherTimer timerLog;
        System.Windows.Threading.DispatcherTimer timer;

        List<KeyValuePair<string, int>> valueList = new List<KeyValuePair<string, int>>();
        List<KeyValuePair<string, long>> valueListGraph;

        ObservableCollection<TableRows> tableRows = new ObservableCollection<TableRows>();
        ObservableCollection<forGraph> graphItems = new ObservableCollection<forGraph>();

        #endregion

        //ad  private static Mutex m_instance;
        private const string m_appName = "WPFTimeManager";

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        public MainWindow()
        {
            try
            {
                Process process = RunningInstance();
                if (process != null)
                {
                    MessageBox.Show("Программа уже запущена!");
                    Application.Current.Shutdown();
                    return;
                }
                InitializeComponent();
                #region init
                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandlerHandler);
                this.Closing += this.MyWindow_Closing;
                pathDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string ExePath = Assembly.GetExecutingAssembly().Location;
                logger.Info("APPLICATION PATH:{0},EXE PATH:{1},CONFIG PATH:{2}", pathDirectory, ExePath, pathDirectory + "\\config.conf");
                RegistryKey reg;
                reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
                reg.SetValue("SimpleTimeManager", ExePath);
                if (!File.Exists(pathDirectory + "\\config.conf")) CryptedParam.CreateDefaultConfig(pathDirectory + "\\config.conf");
                if (!CryptedParam.CheckPass(pathDirectory + "\\config.conf"))
                {
                    MessageBox.Show("Странный пароль.Он точно не \"поправлен\" в конфигах?");
                    Application.Current.Shutdown();
                }

                DayActivity.timeIdle = CryptedParam.GetIdle(pathDirectory + "\\config.conf");

                ClearLogFiles();
                Deserialize(DateTime.Today, DateTime.Today, pathDirectory);

                valueListGraph = new List<KeyValuePair<string, long>>();

                globalKeyboardHook.KeyPressedEvent += onKeyPressed;
                globalKeyboardHook.OpenEvent += onOpen;
                globalMouseHook.MouseMovedEvent += onMouseMove;
                globalKeyboardHook.hook();
                globalMouseHook.hook();

                timerLog = new System.Windows.Threading.DispatcherTimer();
                //           timerRehook = new System.Windows.Threading.DispatcherTimer();
                timer = new System.Windows.Threading.DispatcherTimer();

                //           timerRehook.Tick += new EventHandler(timerRehook_Tick);
                //           timerRehook.Interval = new TimeSpan(0, 0, 0, 0, 500);

                timerLog.Tick += new EventHandler(timerLog_Tick);
                timerLog.Interval = new TimeSpan(0, 10, 0);

                timer.Tick += new EventHandler(timer_Tick);
                timer.Interval += new TimeSpan(0, 0, 1);


                timer.Start();
                //           timerRehook.Start();
                timerLog.Start();

                #endregion
                logger.Info("END:INIT");
                dateList.Add(DateTime.Today);
                UpdateGraph();
                DataGridUpdate();

                //для того чтобы окна не было видно TODO start
                this.Visibility = Visibility.Hidden;
                this.IsEnabled = false; 

            }
            catch (Exception ex)
            {
                logger.Fatal("ERROR INIT:{0}", ex.Message);
            }
        }

        private void UnhandlerHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            logger.Error("UNHADLED ERROR:{0}   \nRUNTIME TERMINATING:{1}  \nSTACK TRACE:{2}          \nSOURSE:{3}           \nTARGET SITE:{4}", e.Message, args.IsTerminating, e.StackTrace, e.Source, e.TargetSite);
            MessageBox.Show("В логах что-то должно быть.");
        }




        #region hooks events

        private void onKeyPressed(Object o, EventArgs e) //При нажатии клавиши
        {
            isKeyboardActive = true;
            globalKeyboardHook.unhook();
            globalKeyboardHook.hook();
        }

        private void onOpen(Object o, EventArgs e) //При нажатии CTRL+F11
        {
            if (IsVisible == true) //TODO hide-show
            {
                logger.Info("window:hide");
                //this.Hide();
                this.Visibility = Visibility.Hidden;
                this.IsEnabled = false;               
            }
            else
            { 
                logger.Info("window:open");
                this.IsEnabled = true; 
                this.Visibility = Visibility.Visible;
                DataGridUpdate();

                //актириуем его
                this.Show();
                this.Activate();
            }

            globalKeyboardHook.unhook();
            globalKeyboardHook.hook();
        }

        private void onMouseMove(Object o, EventArgs e) //Активность мыши
        {
            isMouseActive = true;
        }

        void MyWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) //Отмена закрытия окна
        {
            logger.Info("window:hide");
            this.Visibility = Visibility.Hidden;
            this.IsEnabled = false;
            e.Cancel = true;
        }
        #endregion

        #region timer functions

        private void timerLog_Tick(object sender, EventArgs e) //Таймер логов
        {
            Serialize(pathDirectory);
        }

        private void timerRehook_Tick(object sender, EventArgs e) // Отвязка/привязка хуков. 
        {
            try
            {
                globalMouseHook.unhook();
                globalMouseHook.hook();
                globalKeyboardHook.unhook();
                globalKeyboardHook.hook();
            }
            catch (Exception ex)
            {
                logger.Error("ERROR:hook/unhook:{0}", ex.Message);
            }
        }

        private void timer_Tick(object sender, EventArgs e) //Проверка активности каждую секунду.
        {
            try
            {
                lock (l)
                {
                    dateActivity.checkWork();
                }
            }
            catch (Exception ex)
            {
                logger.Error("ERROR:ACTIVITY TIMER:{0}", ex.Message);
            }
        }

        #endregion

        #region Serialize


        private void Serialize(string path) //Запись в файл. 
        {
            logger.Info("start:serialize");
            try
            {
                string fullPath = path + "\\" + DateTime.Today.ToString("MM.dd.yyyy") + ".log";//.Replace(':', '.') + ".log";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                StreamWriter file = new StreamWriter(fullPath);
                lock (l)
                {
                    if (dateActivity.log.ContainsKey(DateTime.Today))
                    {
                        file.WriteLine(dateActivity.log[DateTime.Today].data.Count);
                        foreach (var z in dateActivity.log[DateTime.Today].data)
                        {
                            file.WriteLine(z.Key);
                            file.WriteLine(z.Value.Count);
                            foreach (var q in z.Value)
                            {
                                file.WriteLine(q.Key);
                                file.WriteLine(q.Value.title);
                                file.WriteLine(q.Value.idleTime);
                                file.WriteLine(q.Value.sumActiveTime);
                            }
                        }

                        file.Close();
                        //Crypter.Crypt(fullPath, path + "\\" + DateTime.Today.ToString("MM.dd.yyyy") + ".crypt");
                        if (File.Exists(path + "\\logs.all"))
                        {
                            using (ZipFile zip = new ZipFile("logs.all"))
                            {
                                zip.CompressionLevel = Ionic.Zlib.CompressionLevel.None;
                                zip.Password = "tuwea rhv6i27dhco";
                                zip.UpdateFile(DateTime.Today.ToString("MM.dd.yyyy") + ".log");//.Replace(':', '.') + ".log");
                                zip.Save("logs.all");
                                zip.Dispose();
                            }
                        }
                        else
                        {
                            using (ZipFile zip = new ZipFile())
                            {
                                zip.CompressionLevel = Ionic.Zlib.CompressionLevel.None;
                                zip.Password = "tuwea rhv6i27dhco";
                                zip.AddFile(DateTime.Today.ToString("MM.dd.yyyy") + ".log");//.Replace(':', '.') + ".log");
                                zip.Save("logs.all");
                            }

                        }
                        File.Delete(fullPath);
                        timerLog.Interval = new TimeSpan(0, 10, 0);
                    }
                }
                logger.Info("end:serialize,path:{0}", fullPath);
            }
            catch (Exception ex)
            {
                logger.Error("ERROR:SERIALIZE.Timer set:1 minute:{0}", ex.Message);
                timerLog.Interval = new TimeSpan(0, 1, 0);
            }
            finally
            {
                ClearLogFiles();
            }
        }

        private void Deserialize(DateTime begin, DateTime end, string path, string filename = "logs.all") //Вывод из файла
        {
            logger.Info("start:deserialize");
            try
            {
                DayActivity tempDayActivity = null;
                string name, subpage, title;
                long idle, sum;
                int count, countIn;
                if (dateActivity.log.ContainsKey(DateTime.Today))
                    tempDayActivity = dateActivity.log[DateTime.Today];
                dateActivity.log.Clear();
                lock (l)
                {
                    for (DateTime tDateTime = begin; tDateTime.DayOfYear <= end.DayOfYear || tDateTime.Year < end.Year; tDateTime = tDateTime.AddDays(1))
                    {
                        string pathLog = path + "\\" + tDateTime.ToString("MM.dd.yyyy") + ".log";
                        //string pathCrypt = path + "\\"+tDateTime.ToString("MM.dd.yyyy") + ".crypt";
                        //if (File.Exists(pathCrypt))
                        //{
                        //    Crypter.Decrypt(path, tDateTime.ToString("MM.dd.yyyy") + ".crypt", tDateTime.ToString("MM.dd.yyyy") + ".log");
                        if (File.Exists(path + "\\" + filename))
                            using (ZipFile zip = ZipFile.Read(path + "\\" + filename))
                            {
                                if (zip.ContainsEntry(tDateTime.ToString("MM.dd.yyyy") + ".log"))
                                {
                                    zip.Password = "tuwea rhv6i27dhco";
                                    zip[tDateTime.ToString("MM.dd.yyyy") + ".log"].Extract(path);
                                    StreamReader file = new StreamReader(path + "\\" + tDateTime.ToString("MM.dd.yyyy") + ".log");
                                    DayActivity temp = new DayActivity();
                                    count = int.Parse(file.ReadLine());
                                    DateTime d = new DateTime(tDateTime.Year, tDateTime.Month, tDateTime.Day, 0, 0, 0);
                                    dateActivity.log.Add(d, temp);
                                    for (int i = 0; i < count; i++)
                                    {
                                        name = file.ReadLine();
                                        countIn = int.Parse(file.ReadLine());
                                        for (int j = 0; j < countIn; j++)
                                        {
                                            subpage = file.ReadLine();
                                            title = file.ReadLine();
                                            idle = long.Parse(file.ReadLine());
                                            sum = long.Parse(file.ReadLine());
                                            dateActivity.log[d].addValues(name, subpage, title, idle, sum);
                                        }
                                    }


                                    file.Close();
                                    File.Delete(path + "\\" + tDateTime.ToString("MM.dd.yyyy") + ".log");
                                }
                            }
                    }
                    if ((tempDayActivity != null) && !dateActivity.log.ContainsKey(DateTime.Today))
                        dateActivity.log.Add(DateTime.Today, tempDayActivity);
                    else if (tempDayActivity != null)
                        dateActivity.log[DateTime.Today] = tempDayActivity;
                    logger.Info("finish:deserialize");
                }
            }
            catch (Exception ex)
            {
                logger.Error("ERROR:DESERIALIZE FAIL:{0}", ex.Message);
            }
            finally
            {
                ClearLogFiles();
            }
        }

        /*public void OpenReport(string PathToReport)
        {
            logger.Info("start:open report at:{0}",PathToReport);
            try
            {
                string name, subpage, title;
                long idle, sum;
                int count, countIn;
                String[] mas;
                dateActivity.log.Clear();
                lock (l)
                Directory.CreateDirectory("logs");
                {
                    using (ZipFile zip = ZipFile.Read(PathToReport))
                    {
                        zip.Password = "tuwea rhv6i27dhco";
                        foreach (ZipEntry e in zip)
                        {
                            mas=e.FileName.Split(' ','.');
                            DateTime d = new DateTime(int.Parse(mas[2]), int.Parse(mas[1]), int.Parse(mas[0]),0,0,0);
                            DayActivity temp = new DayActivity();
                            e.Extract(pathDirectory + "\\logs");
                            StreamReader file = new StreamReader(pathDirectory + "\\logs\\" +e.FileName);
                            count = int.Parse(file.ReadLine());
                            dateActivity.log.Add(d, temp);
                            for (int i = 0; i < count; i++)
                            {
                                name = file.ReadLine();
                                countIn = int.Parse(file.ReadLine());
                                for (int j = 0; j < countIn; j++)
                                {
                                    subpage = file.ReadLine();
                                    title = file.ReadLine();
                                    idle = long.Parse(file.ReadLine());
                                    sum = long.Parse(file.ReadLine());
                                    dateActivity.log[d].addValues(name, subpage, title, idle, sum);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("ERROR:OPEN REPORT FAIL:{0}", ex.Message);
            }
        }*/
        #endregion

        #region graph

        public class forGraph
        {
            public string Key { get; set; }
            public long Value { get; set; }
            public string FullKey { get; set; }
            public long FullValue { get; set; }
        }

        private void columnSeries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            logger.Info("Graph:selection change");
            try
            {
                if (columnSeries.SelectedItem != null)
                {
                    string s = columnSeries.SelectedItem.ToString();
                    s = s.Remove(s.IndexOf(','), s.Length - s.IndexOf(','));
                    s = s.Remove(0, 1);
                    Dictionary<string, long> valueDictGraph = new Dictionary<string, long>();
                    foreach (var z in dateList)
                        if (dateActivity.log.ContainsKey(z))
                            if (dateActivity.log[z].data.ContainsKey(s))
                                foreach (var v in dateActivity.log[z].data[s])
                                {
                                    string q;
                                    q = v.Key.ToString();
                                    if (q != "Суммарно" && q != "")
                                    {
                                        if (columnChart.Title.ToString().Contains("суммарный"))
                                        {
                                            if (valueDictGraph.ContainsKey(q))
                                            {
                                                valueDictGraph[q] += (v.Value.sumActiveTime + v.Value.idleTime);
                                            }
                                            else
                                            {
                                                valueDictGraph.Add(q, (v.Value.sumActiveTime + v.Value.idleTime));
                                            }
                                        }
                                        else if (columnChart.Title.ToString().Contains("активности"))
                                        {
                                            if (valueDictGraph.ContainsKey(q))
                                            {
                                                valueDictGraph[q] += v.Value.sumActiveTime;
                                            }
                                            else
                                            {
                                                valueDictGraph.Add(q, v.Value.sumActiveTime);
                                            }
                                        }
                                        else if (columnChart.Title.ToString().Contains("безделья"))
                                        {
                                            {
                                                if (valueDictGraph.ContainsKey(q))
                                                {
                                                    valueDictGraph[q] += v.Value.idleTime;
                                                }
                                                else
                                                {
                                                    valueDictGraph.Add(q, v.Value.idleTime);
                                                }
                                            }
                                        }
                                    }
                                }
                    graphItems.Clear();
                    if (valueDictGraph.Count > 1)
                    {
                        List<forGraph> newValueListGraph = new List<forGraph>();
                        foreach (var v in valueDictGraph)
                        {
                            if (v.Value > 60)
                            {
                                if (v.Key.Length < 40)
                                    graphItems.Add(new forGraph { Key = v.Key, FullKey = v.Key, Value = v.Value / 60, FullValue = v.Value / 60 });
                                else graphItems.Add(new forGraph { Key = v.Key.Remove(40) + "...", FullKey = v.Key, Value = v.Value / 60, FullValue = v.Value / 60 });
                            }
                        }
                        /*                          
                                                  if (valueListGraph.Count != 0)
                                                  {
                                                      valueListGraph = SortValueGraphList(valueListGraph);
                                                      graphMin = 0;
                                                      graphMax = valueListGraph.Count < 5 ? valueListGraph.Count : 5;

                                                      newValueListGraph = valueListGraph.GetRange(graphMin, graphMax);

                                                      List<KeyValuePair<string, long>> shortValueListGraph = new List<KeyValuePair<string, long>>();
                                                      foreach (var v in newValueListGraph)
                                                      {
                                                          if (v.Key.Length > 40)
                                                              shortValueListGraph.Add(new KeyValuePair<string, long>(v.Key.Remove(40) + "...", v.Value));
                                                          else shortValueListGraph.Add(new KeyValuePair<string, long>(v.Key, v.Value));
                                                      }


                                                      columnSeries.DataContext = shortValueListGraph;
                                                      labelValueList.Content = "От " + graphMin + " до" + graphMax;
                                                  }
                                                  */
                        if (graphItems.Count > 0)
                            columnSeries.DataContext = graphItems;
                    }
                }
                logger.Info("Graph:selection change ends");
            }
            catch (Exception ex)
            {
                logger.Error("ERROR:GRAPH SELECTION:{0}", ex.Message);
            }
        }

        private void UpdateGraph()
        {
            RoutedEventArgs r;
            if (columnChart.Title.ToString() == "График безделья")
            {
                r = new RoutedEventArgs(null, buttonIdle);
            }
            else if (columnChart.Title.ToString() == "График активности")
            {
                r = new RoutedEventArgs(null, buttonActive);
            }
            else
            {
                r = new RoutedEventArgs(null, buttonSum);
            }
            buttonGraph_Click(null, r);
        }


        private void buttonGraph_Click(object sender, RoutedEventArgs e) //Нажатие на любую из трех кнопок построить график
        {
            logger.Info("Graph:type change");
            lock (l)
            {
                valueListGraph.Clear();
                Button tempBut = e.Source as Button;
                Dictionary<string, long> valueDictGraph = new Dictionary<string, long>();
                long sumTime = 0;


                if (tempBut.Name == "buttonSum")//Если нажато "суммарно"
                {
                    valueDictGraph.Add("Idle", 0);
                    valueDictGraph.Add("Безделье (суммарное)", 0);
                    valueDictGraph.Add("Активность (суммарное)", 0);
                    foreach (var z in dateList)
                        if (dateActivity.log.ContainsKey(z))
                        {
                            foreach (var v in dateActivity.log[z].data)
                            {
                                if (v.Key != "Idle")
                                {
                                    valueDictGraph["Безделье (суммарное)"] += v.Value["Суммарно"].idleTime;
                                    sumTime += v.Value["Суммарно"].idleTime;
                                    valueDictGraph["Активность (суммарное)"] += v.Value["Суммарно"].sumActiveTime;
                                    sumTime += v.Value["Суммарно"].sumActiveTime;
                                }
                                if (v.Key == "Idle")
                                {
                                    valueDictGraph["Idle"] += (v.Value["Суммарно"].idleTime + v.Value["Суммарно"].sumActiveTime);
                                    sumTime += v.Value["Суммарно"].idleTime + v.Value["Суммарно"].sumActiveTime;
                                }
                            }
                        }
                    columnChart.Title = "График суммарный (";
                }

                else if (tempBut.Name == "buttonIdle")//Если нажато "Безделье"
                {
                    foreach (var z in dateList)
                        if (dateActivity.log.ContainsKey(z))
                            foreach (var v in dateActivity.log[z].data)
                            {
                                if (v.Key != "Idle")
                                {
                                    if (valueDictGraph.ContainsKey(v.Key))
                                    {
                                        valueDictGraph[v.Key] += v.Value["Суммарно"].idleTime;
                                    }
                                    else
                                    {
                                        valueDictGraph.Add(v.Key, v.Value["Суммарно"].idleTime);
                                    }
                                    sumTime += v.Value["Суммарно"].idleTime;
                                }
                            }
                    columnChart.Title = "График безделья (";
                }

                else if (tempBut.Name == "buttonActive")//Если нажато "В работе"
                {
                    foreach (var z in dateList)
                        if (dateActivity.log.ContainsKey(z))
                            foreach (var v in dateActivity.log[z].data)
                            {
                                if (v.Key != "Idle") //А вдруг?
                                {
                                    if (valueDictGraph.ContainsKey(v.Key))
                                    {
                                        valueDictGraph[v.Key] += v.Value["Суммарно"].sumActiveTime;
                                    }
                                    else
                                    {
                                        valueDictGraph.Add(v.Key, v.Value["Суммарно"].sumActiveTime);
                                    }
                                }
                                sumTime += v.Value["Суммарно"].sumActiveTime;
                            }
                    columnChart.Title = "График активности (";
                }

                foreach (var v in valueDictGraph)
                {
                    valueListGraph.Add(new KeyValuePair<string, long>(v.Key, v.Value));
                }
                valueListGraph = SortValueGraphList(valueListGraph);
                List<KeyValuePair<string, long>> newValueListGraph = new List<KeyValuePair<string, long>>();
                foreach (var v in valueListGraph)
                {
                    if (v.Value > 60)
                        newValueListGraph.Add(new KeyValuePair<string, long>(v.Key, v.Value / 60));
                }
                /*
                graphMin = 0;
                graphMax = valueListGraph.Count < 5 ? valueListGraph.Count : 5;

                newValueListGraph = valueListGraph.GetRange(graphMin, graphMax - graphMin);

                List<KeyValuePair<string, long>> shortValueListGraph = new List<KeyValuePair<string, long>>();

                foreach (var v in newValueListGraph)
                {
                    if (v.Key.Length > 40)
                        shortValueListGraph.Add(new KeyValuePair<string, long>(v.Key.Remove(40) + "...", v.Value));
                    else shortValueListGraph.Add(new KeyValuePair<string, long>(v.Key, v.Value));
                }
                try
                {
                    columnSeries.DataContext = shortValueListGraph;
                }
                catch { }
                labelValueList.Content = "От " + graphMin + " до " + graphMax;
                */
                sumTime /= 60;
                columnChart.Title += (sumTime / 1440).ToString().PadLeft(2, '0') + ":" + ((sumTime / 60) % 24).ToString().PadLeft(2, '0') + ":" + (sumTime % 60).ToString().PadLeft(2, '0') + " = " + sumTime.ToString() + "мин.):"; //Давно бы уже написал конвертер,но все ещё лень
                columnSeries.DataContext = newValueListGraph;
                DataGridUpdate();
            }
            logger.Info("Graph:type chenged");
        }

        private void graphPrev_Click(object sender, RoutedEventArgs e)
        {
            /*
            logger.Info("График:предыдущие 5 значений");
            newValueListGraph = new List<KeyValuePair<string, long>>();
            graphMin = graphMin - 5 > 0 ? graphMin - 5 : 0;
            graphMax = graphMin + 5 < valueListGraph.Count ? graphMin + 5 : valueListGraph.Count;

            newValueListGraph = valueListGraph.GetRange(graphMin, graphMax - graphMin);
            List<KeyValuePair<string, long>> shortValueListGraph = new List<KeyValuePair<string, long>>();

            foreach (var v in newValueListGraph)
            {
                if (v.Key.Length > 20)
                    shortValueListGraph.Add(new KeyValuePair<string, long>(v.Key.Remove(20) + "...", v.Value));
                else shortValueListGraph.Add(new KeyValuePair<string, long>(v.Key, v.Value));
            }

            columnSeries.DataContext = shortValueListGraph;
            labelValueList.Content = "От " + graphMin + " до " + graphMax;
            logger.Info("График:Успешно");
            */
        }

        private void graphNext_Click(object sender, RoutedEventArgs e)
        {
            /*
            logger.Info("График:следующие 5 значений");
            newValueListGraph = new List<KeyValuePair<string, long>>();
            graphMax = (graphMax + 5 < valueListGraph.Count) ? graphMax + 5 : valueListGraph.Count;
            graphMin = graphMax - 5 > 0 ? graphMax - 5 : 0;
            newValueListGraph = valueListGraph.GetRange(graphMin, graphMax - graphMin);
            List<KeyValuePair<string, long>> shortValueListGraph = new List<KeyValuePair<string, long>>();

            foreach (var v in newValueListGraph)
            {
                if (v.Key.Length > 20)
                    shortValueListGraph.Add(new KeyValuePair<string, long>(v.Key.Remove(20) + "...", v.Value));
                else shortValueListGraph.Add(new KeyValuePair<string, long>(v.Key, v.Value));
            }

            columnSeries.DataContext = shortValueListGraph;
            labelValueList.Content = "От " + graphMin + " до " + graphMax;
            logger.Info("График:Успешно!");
            */
        }

        #endregion

        #region combobox functions

        private void comboBoxOneDay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxOneDay.SelectedIndex != -1)
            {
                dateList.Clear();
                dateList.Add((DateTime)comboBoxOneDay.SelectedItem);
                Deserialize((DateTime)comboBoxOneDay.SelectedItem, (DateTime)comboBoxOneDay.SelectedItem, pathDirectory);
                DataGridUpdate();
                UpdateGraph();

                labelToday.Foreground = Brushes.Blue;
                labelY.Foreground = Brushes.Blue;
                ClearAllLabels();
            }
        }

        private void comboBoxOneDay_DropDownOpened(object sender, EventArgs e)
        {
            comboBoxOneDay.Items.Clear();
            foreach (var v in dateActivity.log)
            {
                comboBoxOneDay.Items.Add(v.Key);
            }
        }

        #endregion

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            logger.Info("Date change");
            if (firstDatePicker.SelectedDate != null && secondDatePicker.SelectedDate != null)
            {
                {
                    DateTime first = firstDatePicker.SelectedDate.Value;
                    DateTime second = secondDatePicker.SelectedDate.Value;

                    if (first.DayOfYear >= second.DayOfYear && first.Year >= second.Year)
                        System.Windows.MessageBox.Show("Некорректный выбор даты");
                    else
                    {
                        Deserialize(first, second, pathDirectory);
                        dateList.Clear();
                        for (; first.DayOfYear <= second.DayOfYear || first.Year < second.Year; first = first.AddDays(1))
                        {
                            dateList.Add(first);
                        }
                    }
                }
                ClearAllLabels();
                DataGridUpdate();
                UpdateGraph();
                comboBoxOneDay.SelectedIndex = -1;
            }
            logger.Info("Date changed");
        }


        #region labels
        //"За сегодня","За вчера","За неделю","За месяц"
        private void labelWeek_MouseDown(object sender, MouseButtonEventArgs e)
        {
            dateList.Clear();
            for (int i = 0; i < 7; i++)
            {
                dateList.Add(DateTime.Today.Subtract(new TimeSpan(i, 0, 0, 0)));
            }
            Deserialize(DateTime.Today.Subtract(new TimeSpan(7, 0, 0, 0)), DateTime.Today, pathDirectory);
            DataGridUpdate();
            UpdateGraph();
            ClearAllLabels();
            labelWeek.Foreground = Brushes.Black;
            ClearComboBoxes();
        }

        private void labelMonth_MouseDown(object sender, MouseButtonEventArgs e)
        {
            dateList.Clear();
            for (int i = 0; i < 30; i++)
            {
                dateList.Add(DateTime.Today.Subtract(new TimeSpan(i, 0, 0, 0)));
            }
            Deserialize(DateTime.Today.Subtract(new TimeSpan(30, 0, 0, 0)), DateTime.Today, pathDirectory);
            DataGridUpdate();
            UpdateGraph();
            ClearAllLabels();
            labelMonth.Foreground = Brushes.Black;
            ClearComboBoxes();
        }

        private void labelY_MouseDown(object sender, MouseButtonEventArgs e)
        {
            dateList.Clear();
            dateList.Add(DateTime.Today.Subtract(new TimeSpan(1, 0, 0, 0)));
            Deserialize(DateTime.Today.Subtract(new TimeSpan(1, 0, 0, 0)), DateTime.Today.Subtract(new TimeSpan(1, 0, 0, 0)), pathDirectory);
            DataGridUpdate();
            UpdateGraph();
            ClearAllLabels();
            labelY.Foreground = Brushes.Black;
            ClearComboBoxes();
        }

        private void labelToday_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            dateList.Clear();
            dateList.Add(DateTime.Today);
            Deserialize(DateTime.Today, DateTime.Today, pathDirectory);
            DataGridUpdate();
            UpdateGraph();
            ClearAllLabels();
            labelToday.Foreground = Brushes.Black;
            ClearComboBoxes();
        }
        #endregion

        #region system functions


        public void ClearComboBoxes()
        {
            comboBoxOneDay.Items.Clear();
            firstDatePicker.SelectedDate = null;
            secondDatePicker.SelectedDate = null;
        } //Очистка комбобоксов
        public void ClearAllLabels()
        {
            labelToday.Foreground = Brushes.Blue;
            labelY.Foreground = Brushes.Blue;
            labelWeek.Foreground = Brushes.Blue;
            labelMonth.Foreground = Brushes.Blue;
        } //Заливка лейблов синим цветом
        public List<KeyValuePair<string, long>> SortValueGraphList(List<KeyValuePair<string, long>> unsorted) //Реализация сортировки по значению,а не ключу.
        {
            List<KeyValuePair<string, long>> sorted = unsorted;
            sorted.Sort(CompareValueGraphList);
            return sorted;
        }
        public static int CompareValueGraphList(KeyValuePair<string, long> x, KeyValuePair<string, long> y)
        {
            if (x.Value > y.Value)
                return -1;
            else if (x.Value < y.Value)
                return 1;
            else return 0;
        }
        public static int CompareValueGraphListByName(KeyValuePair<string, long> x, KeyValuePair<string, long> y)
        {
            if (x.Key == y.Key)
                return 0;
            else return 1;
        }


        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            DataGridUpdate();
            UpdateGraph();
        }



        private void buttonWorker_Click(object sender, RoutedEventArgs e)
        {
            logger.Debug("Open report");
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Log crypted files (*.all)|*.all";
                openFileDialog.FilterIndex = 2;
                openFileDialog.Multiselect = false;
                if (openFileDialog.ShowDialog() != null)
                    if (openFileDialog.FileName != "")
                    {


                        MainWindow newWindow = new MainWindow();
                        newWindow.setPaths(Path.GetDirectoryName(openFileDialog.FileName));
                        newWindow.dateActivity.log.Clear();
                        //newWindow.OpenReport(openFileDialog.FileName);
                        newWindow.timer.Stop();
                        newWindow.timerLog.Stop();
                        newWindow.Title = "ОТЧЕТ СОТРУДНИКА - закрывается по кресту";
                        newWindow.labelHelp.Content = "ОТЧЕТ СОТРУДНИКА";
                        newWindow.Closing -= newWindow.MyWindow_Closing;
                        newWindow.buttonClear.Visibility = Visibility.Hidden;
                        newWindow.buttonWorker.Visibility = Visibility.Hidden;
                        newWindow.buttonRefresh.Visibility = Visibility.Hidden;
                        newWindow.buttonChangePassword.Visibility = Visibility.Hidden;
                        newWindow.buttonIdleTime.Visibility = Visibility.Hidden;
                        newWindow.Deserialize(DateTime.Today, DateTime.Today, newWindow.pathDirectory, Path.GetFileName(openFileDialog.FileName));
                        newWindow.DataGridUpdate();
                        newWindow.Show();
                    }
                logger.Debug("report opened");
            }
            catch (Exception ex)
            {
                logger.Error("ERROR:OPEN REPORT:{0}", ex.Message);
            }
        } //Открыть чужой отчет


        private void ClearLogs(object sender, RoutedEventArgs e)
        {
            try
            {
                logger.Info("Clear logs");
                WindowPassword dialog = new WindowPassword("Введите пароль:");
                if (dialog.ShowDialog() == true)
                {
                    if (CryptedParam.VerifyPass(pathDirectory + "\\config.conf", dialog.Answer))
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(pathDirectory);
                        dateActivity.log.Clear();
                        if (File.Exists(pathDirectory + "\\logs.all"))
                            lock (l)
                            {
                                File.Delete(pathDirectory + "\\logs.all");
                                dateActivity.log.Clear();
                            }
                        DataGridUpdate();
                        MessageBox.Show("Логи очищены.");
                    }
                    else
                    {
                        MessageBox.Show("Неверный пароль");
                    }
                }
                logger.Info("Logs cleared");
            }
            catch (Exception ex)
            {
                logger.Error("ERROR:LOGS CLEAR:{0}", ex.Message);
            }
        }


        public void clearExceptToday()
        {
            foreach (var v in dateActivity.log)
            {
                if (v.Key != DateTime.Today)
                {
                    dateActivity.log.Remove(v.Key);
                }
            }
        }

        public void setPaths(string path)
        {
            pathDirectory = path;
            DataGridUpdate();
        }

        #endregion

        #region datagrid functions

        public class TableRows
        {
            public System.Drawing.Bitmap icon { get; set; }

            public string process { get; set; }
            public long idle { get; set; }
            public long work { get; set; }
            public long sum { get; set; }
            public int subs { get; set; }
        }



        private void dataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IInputElement element = e.MouseDevice.DirectlyOver;
            if (element != null && element is FrameworkElement)
            {
                if (((FrameworkElement)element).Parent is DataGridCell)
                {
                    if (e.ClickCount == 1)
                        e.Handled = true;
                }
            }
        }

        public void DataGridUpdate() //Обновление таблицы
        {
            logger.Info("Start: datagrid update");
            try
            {
                lock (l)
                {
                    int selected = -1;
                    TableRows temp;
                    if (dataGrid.SelectedIndex != -1)
                        selected = dataGrid.SelectedIndex;
                    Dictionary<string, TableRows> tempL = new Dictionary<string, TableRows>();
                    tempL.Add("Суммарное", new TableRows());
                    foreach (var dates in dateList)
                    {
                        if (dateActivity.log.ContainsKey(dates))
                            foreach (var v in dateActivity.log[dates].data)
                            {
                                if (tempL.ContainsKey(v.Key))
                                {
                                    temp = tempL[v.Key];
                                }

                                else
                                {
                                    temp = new TableRows();
                                    temp.icon = (v.Value["Суммарно"].icon != default(System.Drawing.Icon)) ? v.Value["Суммарно"].icon.ToBitmap() : default(System.Drawing.Bitmap);
                                }
                                //
                                temp.sum += v.Value["Суммарно"].idleTime + v.Value["Суммарно"].sumActiveTime;
                                temp.idle += v.Value["Суммарно"].idleTime;
                                temp.work += v.Value["Суммарно"].sumActiveTime;
                                temp.subs = (temp.subs > v.Value.Count) ? temp.subs : v.Value.Count;
                                //Суммарное!
                                tempL["Суммарное"].sum += v.Value["Суммарно"].idleTime + v.Value["Суммарно"].sumActiveTime;
                                tempL["Суммарное"].idle += v.Value["Суммарно"].idleTime;
                                tempL["Суммарное"].work += v.Value["Суммарно"].sumActiveTime;

                                tempL[v.Key] = temp;

                            }
                    }
                    tableRows.Clear();
                    foreach (var v in tempL)
                    {
                        string t;
                        if (v.Value.subs <= 2)
                            t = v.Key == null ? string.Empty : v.Key;
                        else t = "-> " + (v.Key == null ? string.Empty : v.Key);

                        tableRows.Add(new TableRows
                        {
                            icon = v.Value.icon,
                            process = t == null ? String.Empty : t,
                            idle = (int)(v.Value.idle / 3600) * 10000 + (int)(v.Value.idle % 3600 / 60) * 100 + v.Value.idle % 60,
                            work = (int)(v.Value.work / 3600) * 10000 + (int)(v.Value.work % 3600 / 60) * 100 + v.Value.work % 60,
                            sum = (int)(v.Value.sum / 3600) * 10000 + (int)(v.Value.sum % 3600 / 60) * 100 + v.Value.sum % 60
                        });
                    }
                    dataGrid.DataContext = tableRows;
                    dataGrid.SelectedIndex = selected;
                    logger.Info("Datagrid updated");
                }
            }
            catch (Exception ex)
            {
                logger.Error("ERROR:DATAGRID UPDATE:{0}", ex.Message);
            }
        }

        private void dataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e) //Сброс выделения.
        {
            dataGrid.SelectedItem = null;
            e.Handled = true;
        }
        private void dataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            dataGrid.SelectedItem = null;
            e.Handled = true;
        }

        private void buttonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                logger.Info("Try to change password");
                WindowChangePass window = new WindowChangePass(pathDirectory + "\\config.conf");
                window.Show();
            }
            catch (Exception ex)
            {
                logger.Error("ERROR:PasswordChange:{0}", ex.Message);
            }
        }

        private void buttonIdleTime_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("Change idle time");
            WindowChangeIdleTime window = new WindowChangeIdleTime(pathDirectory + "\\config.conf");
            window.Show();
        }

        private void dataGrid_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e) //Загрузка таблицы 
        {
            try
            {
                logger.Info("Open rowdetails");
                DataGrid temp = (DataGrid)e.DetailsElement.FindName("dataGridSon");
                dynamic row = dataGrid.SelectedItem;
                object t1 = row.process;
                string t;
                e.DetailsElement.Visibility = Visibility.Collapsed;
                if (t1.ToString().Contains("-> "))
                {
                    e.DetailsElement.Visibility = Visibility.Visible;
                    Dictionary<string, DayActivity.Activity> l = new Dictionary<string, DayActivity.Activity>();
                    t = t1.ToString().Remove(0, 3);
                    foreach (DateTime v in dateList)
                    {
                        if (dateActivity.log.ContainsKey(v))
                            if (dateActivity.log[v].data.ContainsKey(t))
                            {
                                foreach (var q in dateActivity.log[v].data[t])
                                {
                                    if (l.ContainsKey(q.Key))
                                    {
                                        DayActivity.Activity a = l[q.Key];
                                        a.idleTime += q.Value.idleTime;
                                        a.sumActiveTime += q.Value.sumActiveTime;
                                        l[q.Key] = a;
                                    }
                                    else
                                    {
                                        l.Add(q.Key, q.Value);
                                    }
                                }
                            }
                    }

                    foreach (var v in l)
                    {
                        temp.Items.Add
                            (
                            new
                            {
                                processSon = v.Key,
                                idleSon = (int)(v.Value.idleTime / 3600) * 10000 + (int)(v.Value.idleTime % 3600 / 60) * 100 + v.Value.idleTime % 60,
                                workSon = (int)(v.Value.sumActiveTime / 3600) * 10000 + (int)(v.Value.sumActiveTime % 3600 / 60) * 100 + v.Value.sumActiveTime % 60,
                                sumSon = (int)((v.Value.idleTime + v.Value.sumActiveTime) / 3600) * 10000 + (int)((v.Value.idleTime + v.Value.sumActiveTime) % 3600 / 60) * 100 + (v.Value.idleTime + v.Value.sumActiveTime) % 60
                            }
                            );
                    }
                }
                logger.Info("Rowdetails opened");
            }
            catch (Exception ex)
            {
                logger.Error("ERROR:ROWDETAILS {0}", ex.Message);
            }
        }
        #endregion

        public void ClearLogFiles()
        {
            try
            {
                string[] logList = Directory.GetFiles(pathDirectory, "*.log");
                foreach (string v in logList)
                {
                    File.Delete(v);
                }
            }
            catch (Exception ex) { logger.Error("ERROR:CLEAR *.LOG FILES:{0}", ex.Message); }
        }

        public static Process RunningInstance()
        {
            Process current = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(current.ProcessName);

            foreach (Process process in processes)
            {
                if (process.Id != current.Id)
                {
                    return process;
                }
            }
            return null;
        }
        
    }
}