using System;
using TTech.Muvox.Features.Marker;
using TTech.Muvox.Features.Processor;
using TTech.Muvox.Features.Recorder;
using TTech.Muvox.Features.Settings;

namespace TTech.Muvox
{
    public class ViewModelLocator : IDisposable
    {
        public RecorderViewModel Recorder { get; } = new RecorderViewModel();

        public MarkerViewModel Marker { get; } = new MarkerViewModel();

        public ProcessorViewModel Processor { get; } = new ProcessorViewModel();

        public SettingsViewModel Settings { get; } = new SettingsViewModel();

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (Marker != null)
                        Marker.Dispose();
                    if (Recorder != null)
                        Recorder.Dispose();
                    if (Settings != null)
                        Settings.Dispose();
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
