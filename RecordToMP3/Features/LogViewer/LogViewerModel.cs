using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordToMP3.Features.LogViewer
{
    public class LogViewerModel : GalaSoft.MvvmLight.ViewModelBase
    {
        public LogViewerModel()
        {
            Entries = new ObservableCollection<LogEntryModel>();
        }

        public ObservableCollection<LogEntryModel> Entries { get; set; }

        public void Add(string message)
        {
            if (Entries == null)
                Entries = new ObservableCollection<LogEntryModel>();

            App.Current.Dispatcher.Invoke(() => Entries.Add(new LogEntryModel(message)));
        }
    }
}
