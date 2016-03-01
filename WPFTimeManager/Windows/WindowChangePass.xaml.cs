using System.Windows;
using NLog;

namespace WPFTimeManager
{
    /// <summary>
    /// Логика взаимодействия для WindowChangePass.xaml
    /// </summary>
    public partial class WindowChangePass : Window
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string pathToConfig;
        public WindowChangePass(string pathToConfig)
        {
            InitializeComponent();
            this.pathToConfig = pathToConfig;
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            if (CryptedParam.VerifyPass(pathToConfig,passwordBoxOldPass.Password))
            {
                if (passwordBoxNewPass.Password == passwordBoxNewPassAgain.Password)
                {
                    CryptedParam.SetPassword(pathToConfig, passwordBoxNewPass.Password);
                    MessageBox.Show("Пароль успешно измененен.");
                    logger.Info("Password changed");
                    this.Close();
                }
                else MessageBox.Show("Пароли не совпадают.");
            }
            else
            MessageBox.Show("Неверный пароль.");
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
