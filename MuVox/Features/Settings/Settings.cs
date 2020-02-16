﻿using System;
using System.ComponentModel;
using System.IO;
using NAudio.Wave;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace TTech.MuVox.Features.Settings
{
    public class Settings : GalaSoft.MvvmLight.ObservableObject, ISettings
    {
        private const string PROCESSOR = "Processor";
        private const string RECORDER = "Recorder";
        private const string UX = "UX";

        public string FILE_PATH
        {
            get { return @"Settings.json"; }
        }

        public bool AutoSave
        {
            get { return false; }
        }

        public bool Verify()
        {
            return true;
        }


        [Category(PROCESSOR)]
        [DisplayName("Output path")]
        public string Processor_OutputPath { get; set; } = string.Empty;

        [Category(PROCESSOR)]
        [DisplayName("MP3 Quality")]
        public int Processor_Mp3Quality { get; set; } = 160;

        [Category(RECORDER)]
        [DisplayName("Minutes on pregressbar")]
        public uint Recorder_MinutesOnProgressbar { get; set; } = 200;

        [Category(RECORDER)]
        [DisplayName("WaveInDevice")]
        [ItemsSource(typeof(WaveInDeviceItemsSource))]
        public int Recorder_WaveInDevice { get; set; } = 0;

        [Category(RECORDER)]
        [DisplayName("Filename")]
        public string Recorder_FileName { get; set; } = "MuVox {0:yyyy-MM-dd HHmmss}";

        [Category(RECORDER)]
        [DisplayName("Output path")]
        public string Recorder_OutputPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "MuVox");

        [Category(UX)]
        [DisplayName("Volumemeter, Peekmark fallbackspeed")]
        public int UX_VolumeMeter_PeakMarkFallBackSpeed { get; set; } = 2;

        [Category(UX)]
        [DisplayName("Volumemeter, Peekmark holdtime (ms)")]
        public int UX_VolumeMeter_PeakMarkHoldTime { get; set; } = 200;

        [Category(UX)]
        [DisplayName("Volumemeter, MinDb")]
        public float UX_VolumeMeter_MinDb { get; set; } = -24;

        [Category(UX)]
        [DisplayName("Volumemeter, MaxDb")]
        public float UX_VolumeMeter_MaxDb { get; set; } = 8;

        [Category(UX)]
        [DisplayName("Volumemeter, Num of samples")]
        public byte UX_VolumeMeter_NoSamples { get; set; } = 8;


        [Category(UX)]
        [DisplayName("Display meters as mono")]
        public bool UX_MonoDisplay { get; set; } = false;
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
