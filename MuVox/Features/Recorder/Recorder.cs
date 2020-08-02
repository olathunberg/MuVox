using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using TTech.MuVox.Core;

namespace TTech.MuVox.Features.Recorder
{
    public class Recorder : ObservableObject, ICleanup, IDisposable
    {
        #region Fields
        private WasapiLoopbackCapture waveIn;
        //private WaveIn waveIn;
        private WaveFileWriter? writer;
        private string outputFolder = string.Empty;
        private string? outputFilenameBase;
        #endregion

        #region Constructors
        public Recorder()
        {
            RecordingState = RecordingState.Monitoring;

            waveIn = new WasapiLoopbackCapture(); //new WaveIn();
            waveIn.DataAvailable += WaveIn_DataAvailable;
            waveIn.RecordingStopped += WaveIn_RecordingStopped;
            //waveIn.BufferMilliseconds = 15;
            //waveIn.WaveFormat = new WaveFormat(44100, 16, 2);

            var enumerator = new MMDeviceEnumerator();
            var wasapi = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).FirstOrDefault(x => x.FriendlyName.StartsWith(WaveIn.GetCapabilities(0).ProductName));
            DeviceName = wasapi.FriendlyName;
            Format = waveIn.WaveFormat.ToString();

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

            if (e == null)
                return;

            float maxL = 0;
            float maxR = 0;

            var buffer = new WaveBuffer(e.Buffer);

            for (int index = 0; index < e.BytesRecorded / (waveIn.WaveFormat.BitsPerSample / 8); index += 2)
            {
                var sample32 = buffer.FloatBuffer[index];

                maxL = Math.Max(sample32, maxL);

                sample32 = buffer.FloatBuffer[index + 1];

                maxR = Math.Max(sample32, maxR);
            }

            NewSample?.Invoke(maxL, maxR);

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
        public Action<float, float>? NewSample { get; set; }

        public RecordingState RecordingState { get; private set; }

        public ObservableCollection<Core.Marker> Markers { get; set; } = new ObservableCollection<Core.Marker>();

        public int TenthOfSecondsRecorded => GetTenthOfSecondsRecorded();

        public string DeviceName { get; private set; }

        public string Format { get; private set; }
        #endregion

        #region Public methods
        public void SetMarker()
        {
            Markers.Add(new Core.Marker(GetTenthOfSecondsRecorded(), Core.Marker.MarkerType.Mark));

            var fileName = Path.Combine(outputFolder, outputFilenameBase);
            MarkersHelper.AddMarkerToFile(fileName, Markers.Last());

            RaisePropertyChanged(() => Markers);
        }

        public void StartRecording()
        {
            if (waveIn == null)
                throw new InvalidDataException(nameof(waveIn));
            if (writer == null)
            {
                outputFolder = Settings.Settings.Current.Recorder_OutputPath;
                Directory.CreateDirectory(outputFolder);
                outputFilenameBase = string.Format(Settings.Settings.Current.Recorder_FileName, DateTime.Now);
                writer = new WaveFileWriter(Path.Combine(outputFolder, outputFilenameBase) + ".wav", waveIn.WaveFormat);

                Settings.Settings.Current.Recorder_LastFile = writer.Filename;
                Settings.Settings.Save();

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
            }
        }
        #endregion

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
    }
}
