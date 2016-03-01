using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using NLog;

namespace WPFTimeManager
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        protected override void OnStartup(StartupEventArgs e)
        {
            WpfSingleInstance.Make();

            base.OnStartup(e);
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            // Process unhandled exception
            Exception e = args.Exception;
            logger.Error("DISPATCHER UNHADLED ERROR:{0}     \nSTACK TRACE:{1}          \nSOURSE:{2}           \nTARGET SITE:{3}", e.Message, e.StackTrace, e.Source, e.TargetSite);
            MessageBox.Show("В логах что-то должно быть.");
            // Prevent default unhandled exception processing
            args.Handled = true;
        }
    }
}
