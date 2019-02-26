using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTech.Muvox.Features.LogViewer
{
    public class LogEntryModel : GalaSoft.MvvmLight.ObservableObject
    {
        public LogEntryModel(string message)
        {
            DateTime = DateTime.Now;
            Message = message;
        }

        public DateTime DateTime { get; set; }

        public string Message { get; set; }
    }
}
