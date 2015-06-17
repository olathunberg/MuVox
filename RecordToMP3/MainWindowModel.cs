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
        private ViewModelBase _currentViewModel;

        private RecorderViewModel _recorderViewModel = new RecorderViewModel();
        private ProcessorViewModel _processorViewModel = new ProcessorViewModel();
        private MarkerViewModel _markerViewModel = new MarkerViewModel();

        public ViewModelBase CurrentViewModel
        {
            get
            {
                return _currentViewModel;
            }
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
            CurrentViewModel = _recorderViewModel;

            Messenger.Default.Register<GotoPageMessage>(
                this, (action) =>
                {
                    if (action.GotoPage == Pages.Recorder)
                        CurrentViewModel = _recorderViewModel;
                    if (action.GotoPage == Pages.Processor)
                        CurrentViewModel = _processorViewModel;
                    if (action.GotoPage == Pages.Marker)
                        CurrentViewModel = _markerViewModel;
                });
        }

        public override void Cleanup()
        {
            base.Cleanup();

            _recorderViewModel.Cleanup();
            _processorViewModel.Cleanup();
            _markerViewModel.Cleanup();
        }
    }
}
