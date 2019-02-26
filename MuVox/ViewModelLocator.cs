using System;
using TTech.Muvox.Features.Marker;
using TTech.Muvox.Features.Processor;
using TTech.Muvox.Features.Recorder;
using TTech.Muvox.Features.Settings;

namespace TTech.Muvox
{
    public class ViewModelLocator : IDisposable
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_markerViewModel != null)
                        _markerViewModel.Dispose();
                    if (_recorderViewModel != null)
                        _recorderViewModel.Dispose();
                    if (_settings != null)
                        _settings.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
