using System;
using TTech.MuVox.Features.Editor;
using TTech.MuVox.Features.Processor;
using TTech.MuVox.Features.Recorder;
using TTech.MuVox.Features.Settings;

namespace TTech.MuVox
{
    public sealed class ViewModelLocator : IDisposable
    {
        public ViewModelLocator()
        {
        }

        public RecorderViewModel Recorder { get; } = new RecorderViewModel();

        public EditorViewModel Editor { get; } = new EditorViewModel();

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
                    if (Editor != null)
                        Editor.Dispose();
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
