using GalaSoft.MvvmLight;
using System.Windows;

namespace TTech.MuVox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            CurrentView.Focus();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            if (!e.Cancel && DataContext is ICleanup cleanup)
                cleanup.Cleanup();
        }
    }
}
