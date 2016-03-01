using System;
using System.Windows;
using NLog;

namespace WPFTimeManager
{
    /// <summary>
    /// Логика взаимодействия для WindowChangeIdleTime.xaml
    /// </summary>
    public partial class WindowChangeIdleTime : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string pathToConfig;
        public WindowChangeIdleTime(string pathToConfig)
        {
            InitializeComponent();
            this.pathToConfig = pathToConfig;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CryptedParam.VerifyPass(pathToConfig,passwordBox.Password))
                { MessageBox.Show("Неверный пароль"); }
                else if (textBoxNewIdleTime.Text == "")
                { MessageBox.Show("Пустое время простоя"); }
                else 
                {
                        int d = int.Parse(textBoxNewIdleTime.Text);
                        DayActivity.timeIdle = d;
                        logger.Info("Idle time changed to:{0}", d);
                        string s = String.Format("Время простоя изменено на {0}", d.ToString());
                        MessageBox.Show(s);
                        CryptedParam.SetIdle(pathToConfig,textBoxNewIdleTime.Text);
                        this.Close();       
                }
            }
            catch
            {
                MessageBox.Show("Значение времени простоя неверно");
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
