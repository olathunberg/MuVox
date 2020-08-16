using System;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace MuVox.MultiTrack
{
    public class MainViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private WasapiCapture waveIn;

        public MainViewModel()
        {
            waveIn = new WasapiLoopbackCapture();
            waveIn.DataAvailable += WaveIn_DataAvailable;

            Faders = new System.Collections.ObjectModel.ObservableCollection<Fader.Fader>();
            for (int i = 0; i < waveIn.WaveFormat.Channels; i++)
            {
                Faders.Add(new Fader.Fader { Label = $"Track {i + 1}" });
            }

            waveIn.StartRecording();
        }

        public System.Collections.ObjectModel.ObservableCollection<Fader.Fader> Faders { get; set; }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (e == null)
                return;

            var buffer = new WaveBuffer(e.Buffer);

            float[] max = new float[waveIn.WaveFormat.Channels];
            for (int index = 0; index < e.BytesRecorded / (waveIn.WaveFormat.BitsPerSample / 8); index += waveIn.WaveFormat.Channels)
            {
                for (int channel = 0; channel < waveIn.WaveFormat.Channels; channel++)
                {
                    var sample32 = buffer.FloatBuffer[index + channel];
                    max[channel] = Math.Max(sample32, max[channel]);
                }
            }

            for (int channel = 0; channel < waveIn.WaveFormat.Channels; channel++)
            {
                Faders[channel].SetAmplitude(max[channel]);
            }
        }

        public override void Cleanup()
        {
            if (waveIn != null)
            {
                waveIn.StopRecording();
                waveIn.Dispose();
            }
        }
    }
}
