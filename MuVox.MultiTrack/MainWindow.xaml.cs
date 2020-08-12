using System.Linq;
using System.Windows;

namespace MuVox.MultiTrack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Width = (DataContext as MainViewModel).Faders.Sum(x => x.Width) + 38;
        }
    }
}
