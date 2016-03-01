using System;
using System.Windows;

namespace WPFTimeManager
{
    /// <summary>
    /// Логика взаимодействия для WindowPassword.xaml
    /// </summary>
    public partial class WindowPassword : Window
    {
        public WindowPassword(string question)
        {
            InitializeComponent();
            lblQuestion.Content = question;
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            txtAnswer.SelectAll();
            txtAnswer.Focus();
        }

        public string Answer
        {
            get { return txtAnswer.Password; }
        }
    }
}
