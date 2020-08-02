using System;
using NAudio.Wave;

namespace MuVox.MultiTrack
{
    public class MainViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private WasapiLoopbackCapture waveIn;
        private float amp;

        public MainViewModel()
        {

            waveIn = new WasapiLoopbackCapture(); //new WaveIn();
            waveIn.DataAvailable += WaveIn_DataAvailable;
            waveIn.RecordingStopped += WaveIn_RecordingStopped;

            waveIn.StartRecording();
        }

        public float Amp
        {
            get { return amp; }
            set { amp = value;  RaisePropertyChanged(); }
        }

        private void WaveIn_RecordingStopped(object sender, StoppedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (e == null)
                return;

            var buffer = new WaveBuffer(e.Buffer);

            float max = 0;
            for (int index = 0; index < e.BytesRecorded / (waveIn.WaveFormat.BitsPerSample / 8); index += 2)
            {
                var sample32 = buffer.FloatBuffer[index];
                max = Math.Max(sample32, max);
            }

            Amp = max;
        }
    }
}
