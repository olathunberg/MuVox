using GalaSoft.MvvmLight.Messaging;
using Microsoft.Shell;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using TTech.MuVox.Features.Messages;

namespace TTech.MuVox
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        private const string Unique = "TTech.MuVox";

        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                var application = new App();

                Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

                application.InitializeComponent();

                application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }

        #region ISingleInstanceApp Members
        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            this.MainWindow.Activate();

            return true;
        }
        #endregion
    }
}
