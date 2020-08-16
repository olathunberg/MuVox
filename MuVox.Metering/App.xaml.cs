using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace MuVox.Metering
{
    public partial class App : Application
    {
        private ServiceProvider serviceProvider;

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        private void ConfigureServices(ServiceCollection services)
        {
            //services.AddSingleton<MainWindow>();
        }
    }
}
