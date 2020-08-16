using System.Windows;
using System.Windows.Controls;

namespace MuVox.MultiTrack.Fader
{
    /// <summary>
    /// Interaction logic for Fader.xaml
    /// </summary>
    public partial class Fader : UserControl
    {
        public Fader()
        {
            InitializeComponent();
        }

        public void SetAmplitude(float amplitude)
        {
            Application.Current.Dispatcher.Invoke(() => Meter.Amplitude = amplitude);
        }

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Label.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(Fader), new PropertyMetadata("Track x"));
    }
}
