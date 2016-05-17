using System;
using System.ComponentModel;
using NAudio.Wave;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace RecordToMP3.Features.Settings
{
    public class Settings : GalaSoft.MvvmLight.ObservableObject, ISettings
    {
        private const string PROCESSOR = "Processor";
        private const string RECORDER = "Recorder";
        private const string UX = "UX";

        public string FILE_PATH
        {
            get { return @"\Settings\Settings.json"; }
        }

        public bool AutoSave
        {
            get { return true; }
        }

        public bool Verify()
        {
            return true;
        }

        [DisplayName("Output path")]
        [Category(PROCESSOR)]
        public string OutputPath { get; set; } = string.Empty;

        [DisplayName("Peekmark fallbackspeed")]
        [Category(UX)]
        public int PeakMarkFallBackSpeed { get; set; } = 2;

        [DisplayName("Peekmark holdtime (ms)")]
        [Category(UX)]
        public int PeakMarkHoldTime { get; set; } = 200;

        [Category(RECORDER)]
        [DisplayName("WaveInDevice")]
        [ItemsSource(typeof(WaveInDeviceItemsSource))]
        public int WaveInDevice { get; set; } = 1;
    }

    public class WaveInDeviceItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            var items = new ItemCollection();

            for (int waveInDevice = 0; waveInDevice < WaveIn.DeviceCount; waveInDevice++)
                items.Add(waveInDevice, WaveIn.GetCapabilities(waveInDevice).ProductName);

            return items;
        }
    }
}
