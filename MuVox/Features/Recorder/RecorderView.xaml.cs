using System.Windows.Controls;

namespace TTech.MuVox.Features.Recorder
{
    /// <summary>
    /// Interaction logic for RecorderView.xaml
    /// </summary>
    public partial class RecorderView : UserControl
    {
        public RecorderView()
        {
            InitializeComponent();

            if(DataContext is RecorderViewModel recorderViewModel)
            {
               LeftVu.Settings = RightVu.Settings = recorderViewModel.Settings.VolumeMeterSettings;
            }

            this.Loaded += RecorderView_Loaded;
        }

        void RecorderView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Focus();
        }
    }
}
