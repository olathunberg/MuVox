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

namespace RecordToMP3.Features.Recorder
{
    /// <summary>
    /// Interaction logic for RecorderView.xaml
    /// </summary>
    public partial class RecorderView : UserControl
    {
        public RecorderView()
        {
            InitializeComponent();

            var mainViewModel = MainGrid.DataContext as RecorderViewModel;
            if (mainViewModel != null)
                mainViewModel.Owner = this;
        }
    }
}
