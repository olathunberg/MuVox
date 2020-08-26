using System.Windows;
using System.Windows.Controls;

namespace MuVox.Metering.Meter
{
    /// <summary>
    /// Interaction logic for Meter.xaml
    /// </summary>
    public partial class Meter : UserControl
    {
        public Meter()
        {
            InitializeComponent();
        }

        public void SetAmplitude(float amplitude)
        {
            if (Application.Current != null)
                Application.Current.Dispatcher.Invoke(() => VolumeMeter.Amplitude = amplitude);
        }

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Label.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(Meter), new PropertyMetadata("Track x"));
    }
}
