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

        public float Amplitude
        {
            get { return (float)GetValue(AmplitudeProperty); }
            set { SetValue(AmplitudeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Amplitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AmplitudeProperty =
            DependencyProperty.Register(nameof(Amplitude), typeof(float), typeof(Fader), new PropertyMetadata(0f, (s, e) =>
            {
                if (s is Fader fader)
                {
                    fader.Meter.Amplitude = (float)e.NewValue;
                }
                if (s is FrameworkElement frameworkElement)
                    frameworkElement.InvalidateVisual();
            }));

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
