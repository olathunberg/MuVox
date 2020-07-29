using System.Windows.Controls;

namespace TTech.MuVox.Features.Editor
{
    public partial class EditorView : UserControl
    {
        public EditorView()
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
