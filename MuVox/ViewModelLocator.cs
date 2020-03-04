using System;
using TTech.MuVox.Features.Marker;
using TTech.MuVox.Features.Processor;
using TTech.MuVox.Features.Recorder;
using TTech.MuVox.Features.Settings;

namespace TTech.MuVox
{
    public sealed class ViewModelLocator : IDisposable
    {
        public RecorderViewModel Recorder { get; } = new RecorderViewModel();

        public MarkerViewModel Marker { get; } = new MarkerViewModel();

        public ProcessorViewModel Processor { get; } = new ProcessorViewModel();

        public SettingsViewModel Settings { get; } = new SettingsViewModel();

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        public void Dispose(bool disposing)
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
