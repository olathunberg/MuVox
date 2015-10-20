using RecordToMP3.Features.Marker;
using RecordToMP3.Features.Processor;
using RecordToMP3.Features.Recorder;

namespace RecordToMP3
{
    public class ViewModelLocator
    {
        private RecorderViewModel _recorderViewModel = new RecorderViewModel();
        private ProcessorViewModel _processorViewModel = new ProcessorViewModel();
        private MarkerViewModel _markerViewModel = new MarkerViewModel();

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
    }
}
