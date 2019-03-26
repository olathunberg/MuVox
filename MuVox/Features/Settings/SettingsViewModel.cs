using System;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using TTech.Muvox.Features.Messages;

namespace TTech.Muvox.Features.Settings
{
    public class SettingsViewModel : GalaSoft.MvvmLight.ViewModelBase, IDisposable
    {
        public Settings Settings { get { return SettingsBase<Settings>.Current; } }

        private RelayCommand recordCommand;
        public ICommand Record
        {
            get
            {
                return recordCommand ?? (recordCommand = new RelayCommand(
                    () =>
                    {
                        SettingsBase<Settings>.Save();
                        Messenger.Default.Send(new GotoPageMessage(Pages.Recorder));
                    },
                    () => true));
            }
        }

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
