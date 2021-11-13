using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using TTech.MuVox.Shared;

namespace MuVox.Metering
{
    public class MainViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private WasapiCapture waveIn;
        private float desiredVolume = 0.0f;
        private float fadeStep = 0.04f;
        private int fadeDelayTime = 1;

        public MainViewModel()
        {
            waveIn = new WasapiLoopbackCapture();
            waveIn.DataAvailable += WaveIn_DataAvailable;

            var trackConfig = ChannelConfiguration.Read() ?? new ChannelConfiguration();

            Meters = new System.Collections.ObjectModel.ObservableCollection<Meter.Meter>();
            for (int i = 0; i < waveIn.WaveFormat.Channels; i++)
            {
                var label = trackConfig.Channels[i]?.Label ?? $"Track {i + 1}";
                Meters.Add(new Meter.Meter { Label = label });
            }

            FadeText = GetFadeText(new WasapiOut());
            waveIn.StartRecording();

            HotKeyManager.RegisterHotKey(System.Windows.Forms.Keys.F1, KeyModifiers.Control);
            HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;
        }

        public System.Collections.ObjectModel.ObservableCollection<Meter.Meter> Meters { get; set; }

        public string FadeText { get; private set; }

        private string GetFadeText(WasapiOut waveOut)
        {
            return waveOut.Volume < 0.05f ? "Fade up" : "Fade down";
        }

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
                Meters[channel].SetAmplitude(max[channel]);
            }
        }

        private async void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e)
        {
            await HandleFade();
        }

        private RelayCommand<CancelEventArgs>? fadeCommand;
        public ICommand FadeCommand
        {
            get
            {
                return fadeCommand ?? (fadeCommand = new RelayCommand<CancelEventArgs>(
                    async args =>
                    {
                        await HandleFade();
                    }));
            }
        }

        private async Task HandleFade()
        {
            var waveOut = new WasapiOut();

            if (desiredVolume < 0.01f)
            {
                desiredVolume = 1.0f;
            }

            // Fade up to last or max
            if (waveOut.Volume < (desiredVolume - (fadeStep / 2.0)))
            {
                var volumeFade = desiredVolume - waveOut.Volume;
                while (waveOut.Volume < desiredVolume - fadeStep)
                {
                    waveOut.Volume += fadeStep;
                    await Task.Delay(fadeDelayTime);
                }

                waveOut.Volume = desiredVolume;
            }
            // Fade down to zero
            else
            {
                var volumeFade = waveOut.Volume;
                desiredVolume = waveOut.Volume;
                while (waveOut.Volume > fadeStep)
                {
                    waveOut.Volume -= fadeStep;
                    await Task.Delay(fadeDelayTime);
                }

                waveOut.Volume = 0;
            }

            FadeText = GetFadeText(waveOut);
            RaisePropertyChanged(() => FadeText);
        }

        public override void Cleanup()
        {
            if (waveIn != null)
            {
                waveIn.StopRecording();
            }
        }
    }
}
