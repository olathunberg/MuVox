using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using RecordToMP3.Features.Marker;
using RecordToMP3.Features.Messages;
using RecordToMP3.Features.Processor;
using RecordToMP3.Features.Recorder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordToMP3
{
    public class MainWindowModel : ViewModelBase
    {
        private ViewModelLocator viewModelLocator = (ViewModelLocator)App.Current.Resources["ViewModelLocator"];
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

        public override void Cleanup()
        {
            base.Cleanup();

            viewModelLocator.Recorder.Cleanup();
            viewModelLocator.Processor.Cleanup();
            viewModelLocator.Marker.Cleanup();
        }
    }
}
