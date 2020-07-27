using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using TTech.MuVox.Features.Messages;

namespace TTech.MuVox.Features.Settings
{
    public class SettingsViewModel : GalaSoft.MvvmLight.ViewModelBase, IDisposable
    {
        private RelayCommand? recordCommand;

        public Settings Settings => Settings.Current;

        public ICommand Record
        {
            get
            {
                return recordCommand ?? (recordCommand = new RelayCommand(
                    () =>
                    {
                        Settings.Save();
                        var validation = Settings.Verify().ToList();
                        if (validation.Any())
                        {
                            MessageBox.Show(string.Join(Environment.NewLine, validation), "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
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
