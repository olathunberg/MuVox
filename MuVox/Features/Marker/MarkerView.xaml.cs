using System.Windows.Controls;

namespace TTech.Muvox.Features.Marker
{
    /// <summary>
    /// Interaction logic for MarkerView.xaml
    /// </summary>
    public partial class MarkerView : UserControl
    {
        public MarkerView()
        {
            InitializeComponent();
            this.Loaded += MarkerView_Loaded;
        }

        void MarkerView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Focus();
        }
    }
}
