using GalaSoft.MvvmLight;
using System.Windows;

namespace TTech.Muvox
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
            (DataContext as ICleanup).Cleanup();

            base.OnClosing(e);
        }
    }
}
