using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using NAudio.Lame;
using NAudio.Wave;

namespace TTech.Muvox.Features.Recorder
{
    public class Recorder : ObservableObject, ICleanup, IDisposable
    {
        #region Fields
        private WaveIn? waveIn;
        private WaveFileWriter? writer;
        private string outputFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "MuVox");
        private string? outputFilenameBase;
        private Settings.Settings Settings { get { return Features.Settings.SettingsBase<Settings.Settings>.Current; } }
        #endregion

        #region Constructors
        public Recorder()
        {
            RecordingState = RecordingState.Monitoring;

            waveIn = new WaveIn();
            waveIn.DataAvailable += WaveIn_DataAvailable;
            waveIn.RecordingStopped += WaveIn_RecordingStopped;
            waveIn.BufferMilliseconds = 15;
            waveIn.WaveFormat = new WaveFormat(44100, 16, 2);

            if (!(bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue))
                waveIn.StartRecording();
        }
        #endregion

        #region Events
        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (RecordingState == RecordingState.Recording && writer != null && e != null)
            {
                Task.Run(async () =>
                    {
                        if (writer != null)
                            await writer.WriteAsync(e.Buffer, 0, e.BytesRecorded);
                    });
            }
            else if (RecordingState == RecordingState.RequestedStop)
            {
                RecordingState = RecordingState.Monitoring;
                if (writer != null)
                {
                    writer.Dispose();
                    writer = null;
                }
            }

            float maxL = 0;
            float minL = 0;
            float maxR = 0;
            float minR = 0;

            for (int index = 0; index < e.BytesRecorded; index += 4)
            {
                var sample = (short)((e.Buffer[index + 1] << 8) | (e.Buffer[index]));
                var sample32 = sample / 32768f;

                maxL = Math.Max(sample32, maxL);
                minL = Math.Min(sample32, minL);

                sample = (short)((e.Buffer[index + 3] << 8) | (e.Buffer[index + 2]));
                sample32 = sample / 32768f;

                maxR = Math.Max(sample32, maxR);
                minR = Math.Min(sample32, minR);
            }

            NewSample?.Invoke(minL, maxL, minR, maxR);

            RaisePropertyChanged(() => TenthOfSecondsRecorded);
        }

        private void WaveIn_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (writer != null)
            {
                writer.Dispose();
                writer = null;
            }

            RaisePropertyChanged(() => TenthOfSecondsRecorded);
        }
        #endregion

        #region Private methods
        private int GetTenthOfSecondsRecorded()
        {
            if (writer != null)
                return (int)(writer.Length / (writer.WaveFormat.AverageBytesPerSecond / 10));
            else
                return 0;
        }
        #endregion

        #region Properties
        public Action<float, float, float, float>? NewSample { get; set; }

        public RecordingState RecordingState { get; private set; }

        public ObservableCollection<int> Markers { get; set; } = new ObservableCollection<int>();

        public int TenthOfSecondsRecorded
        {
            get { return GetTenthOfSecondsRecorded(); }
        }
        #endregion

        #region Public methods
        public void SetMarker()
        {
            Markers.Add(GetTenthOfSecondsRecorded());

            var fileName = Path.Combine(outputFolder, outputFilenameBase);
            new Marker.Marker().AddMarkerToFile(fileName, Markers.Last());

            RaisePropertyChanged(() => Markers);
        }

        public void StartRecording()
        {
            if (waveIn == null)
                throw new InvalidDataException(nameof(waveIn));
            if (writer == null)
            {
                outputFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "MuVox");
                Directory.CreateDirectory(outputFolder);
                outputFilenameBase = string.Format(Settings.Recorder_FileName, DateTime.Now);
                writer = new WaveFileWriter(Path.Combine(outputFolder, outputFilenameBase) + ".wav", waveIn.WaveFormat);

                MuVox.Properties.Settings.Default.RECORDER_LastFile = writer.Filename;
                MuVox.Properties.Settings.Default.Save();

                Markers.Clear();
            }

            switch (RecordingState)
            {
                case RecordingState.Monitoring:
                    RecordingState = RecordingState.Recording;
                    break;
                case RecordingState.Recording:
                    RecordingState = RecordingState.Paused;
                    break;
                case RecordingState.Paused:
                    RecordingState = RecordingState.Recording;
                    break;
            }
        }

        public void StopRecording()
        {
            RecordingState = RecordingState.RequestedStop;
            if (Markers != null)
                Markers.Clear();
        }

        public void Cleanup()
        {
            if (waveIn != null)
            {
                waveIn.StopRecording();
                waveIn.Dispose();
                waveIn = null;
            }
        }

        #region IDisposable Support
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (waveIn != null)
                        waveIn.Dispose();
                    if (writer != null)
                        writer.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
        #endregion
    }
}
