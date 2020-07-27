using System.Collections.Generic;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Shell;

namespace TTech.MuVox
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        private const string Unique = "TTech.MuVox";

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                base.Shutdown();
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SingleInstance<App>.Cleanup();
            base.OnExit(e);
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
