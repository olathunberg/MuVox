using RecordToMP3.Features.Marker;
using RecordToMP3.Features.Processor;
using RecordToMP3.Features.Recorder;
using RecordToMP3.Features.Settings;

namespace RecordToMP3
{
    public class ViewModelLocator
    {
        private readonly RecorderViewModel _recorderViewModel = new RecorderViewModel();
        private readonly ProcessorViewModel _processorViewModel = new ProcessorViewModel();
        private readonly MarkerViewModel _markerViewModel = new MarkerViewModel();
        private readonly SettingsViewModel _settings = new SettingsViewModel();

        public RecorderViewModel Recorder
        {
            get { return _recorderViewModel; }
        }

        public MarkerViewModel Marker
        {
            get { return _markerViewModel; }
        }

        public ProcessorViewModel Processor
        {
            get { return _processorViewModel; }
        }

        public SettingsViewModel Settings
        {
            get { return _settings; }
        }
    }
}
