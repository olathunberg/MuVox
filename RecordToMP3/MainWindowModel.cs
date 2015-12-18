using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using RecordToMP3.Features.Messages;
using System.Windows;

namespace RecordToMP3
{
    public class MainWindowModel : ViewModelBase
    {
        private readonly ViewModelLocator viewModelLocator = (ViewModelLocator)System.Windows.Application.Current.Resources["ViewModelLocator"];
        private ViewModelBase _currentViewModel;

        public ViewModelBase CurrentViewModel
        {
            get { return _currentViewModel; }
            set
            {
                if (_currentViewModel == value)
                    return;
                _currentViewModel = value;
                RaisePropertyChanged(() => CurrentViewModel);
            }
        }

        public MainWindowModel()
        {
            Helpers.HotKeyManager.RegisterHotKey(System.Windows.Forms.Keys.F3, Helpers.KeyModifiers.Alt);
            Helpers.HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;

            CurrentViewModel = viewModelLocator.Recorder;

            Messenger.Default.Register<GotoPageMessage>(
                this, (action) =>
                {
                    if (action.GotoPage == Pages.Recorder)
                        CurrentViewModel = viewModelLocator.Recorder;
                    if (action.GotoPage == Pages.Processor)
                        CurrentViewModel = viewModelLocator.Processor;
                    if (action.GotoPage == Pages.Marker)
                        CurrentViewModel = viewModelLocator.Marker;
                });
        }

        private void HotKeyManager_HotKeyPressed(object sender, Helpers.HotKeyEventArgs e)
        {
            Messenger.Default.Send<SetMarkerMessage>(new SetMarkerMessage());
        }

        public override void Cleanup()
        {
            base.Cleanup();

            viewModelLocator.Recorder.Cleanup();
            viewModelLocator.Processor.Cleanup();
            viewModelLocator.Marker.Cleanup();
        }
    }
}
