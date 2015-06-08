using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RecordToMP3.Features.LogViewer
{
    /// <summary>
    /// Interaction logic for LogViewerView.xaml
    /// </summary>
    public partial class LogViewerView : UserControl
    {
        public LogViewerView()
        {
            InitializeComponent();

            DataContextChanged += LogViewerView_DataContextChanged;
        }

        void LogViewerView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext != null && DataContext is LogViewerModel)
                (DataContext as LogViewerModel).Entries.CollectionChanged += Entries_CollectionChanged;
        }

        void Entries_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var sv = VisualTreeHelper.GetChild((DependencyObject)EntriesControl, 0) as ScrollViewer;
            sv.ScrollToEnd();
        }
    }
}
