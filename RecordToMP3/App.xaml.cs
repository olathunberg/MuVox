using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Shell;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Shell;
using RecordToMP3.Features.Messages;

namespace RecordToMP3
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        private const string Unique = "RecordToMP3";

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

            // Activate settings
            Messenger.Default.Send(new GotoPageMessage(Pages.Settings));

            return true;
        }
        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            var task = new JumpTask
            {
                Title = "Configure",
                Arguments = "-configure",
                Description = "Open program settings",
                CustomCategory = "Actions",
                IconResourcePath = Assembly.GetEntryAssembly().CodeBase,
                ApplicationPath = Assembly.GetEntryAssembly().CodeBase
            };

            JumpList jumpList = new JumpList();
            jumpList.JumpItems.Add(task);
            jumpList.ShowFrequentCategory = false;
            jumpList.ShowRecentCategory = false;

            JumpList.SetJumpList(Application.Current, jumpList);
        }
    }
}
