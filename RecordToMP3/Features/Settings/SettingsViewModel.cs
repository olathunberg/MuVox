using System;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using RecordToMP3.Features.Messages;

namespace RecordToMP3.Features.Settings
{
    public class SettingsViewModel : GalaSoft.MvvmLight.ViewModelBase, IDisposable
    {
        #region Fields
        #endregion

        #region Constructors
        public SettingsViewModel()
        {
        }
        #endregion

        #region Properties
        public Settings Settings { get { return SettingsBase<Settings>.Current; } }
        #endregion

        #region Private methods

        #endregion

        #region Commands
        private RelayCommand recordCommand;
        public ICommand Record
        {
            get
            {
                return recordCommand ?? (recordCommand = new RelayCommand(
                    () =>
                    {
                        Messenger.Default.Send(new GotoPageMessage(Pages.Recorder));
                    },
                    () => true));
            }
        }
        #endregion

        #region Overrides
        public override void Cleanup()
        {
            base.Cleanup();
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

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
