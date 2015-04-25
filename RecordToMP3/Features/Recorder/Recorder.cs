using GalaSoft.MvvmLight;
using NAudio.Lame;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordToMP3.Features.Recorder
{
    internal class Recorder : ICleanup
    {
        #region Fields
        private WaveIn waveIn;
        private WaveFileWriter writer;
        private LameMP3FileWriter mp3Writer;
        private RecordingState recordingState;
        #endregion

        #region Constructors
        public Recorder()
        {
            recordingState = RecordingState.Monitoring;

            waveIn = new WaveIn();
            waveIn.DataAvailable += waveIn_DataAvailable;
            waveIn.RecordingStopped += waveIn_RecordingStopped;
            waveIn.BufferMilliseconds = 20;
            waveIn.WaveFormat = new WaveFormat(44100, 16, 2);

            waveIn.StartRecording();
        }
        #endregion

        public Action<float, float, float, float> NewSample { get; set; }

        #region Events
        private void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (recordingState == RecordingState.Recording)
            {
                writer.WriteAsync(e.Buffer, 0, e.BytesRecorded);
                mp3Writer.WriteAsync(e.Buffer, 0, e.BytesRecorded);
            }

            float maxL = 0;
            float minL = 0;
            float maxR = 0;
            float minR = 0;

            for (int index = 0; index < e.BytesRecorded; index += 4)
            {
                short sample = (short)((e.Buffer[index + 1] << 8) | (e.Buffer[index]));
                float sample32 = sample / 32768f;

                maxL = Math.Max(sample32, maxL);
                minL = Math.Min(sample32, minL);

                sample = (short)((e.Buffer[index + 3] << 8) | (e.Buffer[index + 2]));
                sample32 = sample / 32768f;

                maxR = Math.Max(sample32, maxR);
                minR = Math.Min(sample32, minR);
            }

            if (NewSample != null)
                NewSample(minL, maxL, minR, maxR);
        }

        private void waveIn_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (writer != null)
            {
                writer.Dispose();
                writer = null;
            }
            if(mp3Writer != null)
            {
                mp3Writer.Dispose();
                mp3Writer = null;
            }

            if (e.Exception != null)
            {
                //MessageBox.Show(String.Format("A problem was encountered during recording {0}",
                //                              e.Exception.Message));
            }

        }
        #endregion

        public RecordingState RecordingState { get { return recordingState; } }

        public int GetSecondsRecorded()
        {
            if (writer != null)
                return (int)(writer.Length / writer.WaveFormat.AverageBytesPerSecond);
            else
                return 0;
        }

        // Använd SampleProvider för att returnera data till UX

        public void StartRecording()
        {
            if (writer == null)
            {
                var outputFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "RecordToMP3");
                Directory.CreateDirectory(outputFolder);
                var outputFilename = String.Format("RecordToMP3 {0:yyy-MM-dd HH-mm-ss}", DateTime.Now);
                writer = new WaveFileWriter(Path.Combine(outputFolder, outputFilename)+".wav", waveIn.WaveFormat);
                mp3Writer = new LameMP3FileWriter(Path.Combine(outputFolder, outputFilename+".mp3"), waveIn.WaveFormat, 192000);
            }
            // Bryt ut till egen funktion
            if (recordingState == RecordingState.Monitoring)
                recordingState = RecordingState.Recording;
            else if (recordingState == RecordingState.Recording)
                recordingState = RecordingState.Paused;
            else if (recordingState == RecordingState.Paused)
                recordingState = RecordingState.Recording;
        }

        public void StopRecording()
        {
            recordingState = RecordingState.Monitoring;
            writer.Dispose();
            writer = null;
            mp3Writer.Dispose();
            mp3Writer = null;
        }

        #region Public methods
        public void Cleanup()
        {
            waveIn.StopRecording();
            waveIn.Dispose();
            waveIn = null;
        }
        #endregion

    }
}
