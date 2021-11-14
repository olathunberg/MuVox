using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight;

namespace MuVox.Metering
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Width = (DataContext as MainViewModel).Meters.Sum(x => x.Width) + 38 + 220;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            if (!e.Cancel && DataContext is ICleanup cleanup)
                cleanup.Cleanup();
        }

        private void Window_Activated(object sender, System.EventArgs e)
        {

        }
    }
}
