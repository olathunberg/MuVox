using System.Windows.Controls;

namespace RecordToMP3.Features.Processor
{
    /// <summary>
    /// Interaction logic for ProcessorView.xaml
    /// </summary>
    public partial class ProcessorView : UserControl
    {
        public ProcessorView()
        {
            InitializeComponent();
            this.Loaded += ProcessorView_Loaded;
        }

        void ProcessorView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Focus();
        }
    }
}
