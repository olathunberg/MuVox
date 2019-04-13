using System.Collections.ObjectModel;

namespace TTech.Muvox.Features.LogViewer
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

            if (System.Windows.Application.Current != null)
                System.Windows.Application.Current.Dispatcher.Invoke(() => AddEntry(message));
            else
                AddEntry(message);
        }

        private void AddEntry(string message)
        {
            Entries.Add(new LogEntryModel(message));
        }
    }
}
