using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace TTech.MuVox.Features.Settings
{
    /// <summary>
    /// Provides a singleton with lazy loading of settings
    ///
    /// Persists to JSON
    ///
    /// Observes PropertyChange on T and saves entire object
    /// </summary>
    /// <typeparam name="T">Class containing settings</typeparam>
    public class Settings : GalaSoft.MvvmLight.ObservableObject
    {
        private const string PROCESSOR = "Processor";
        private const string RECORDER = "Recorder";
        private const string FILE_PATH = "Settings.json";

        private static Settings? current;

        public static Settings Current
        {
            get
            {
                if (current == null)
                    current = LoadCurrent();
                return current;
            }
        }

        [Category(PROCESSOR)]
        [DisplayName("Output path")]
        public string Processor_OutputPath { get; set; } = string.Empty;

        [Category(PROCESSOR)]
        [DisplayName("MP3 Quality")]
        public int Processor_Mp3Quality { get; set; } = 160;

        [Category(PROCESSOR)]
        [DisplayName("Jingle Path")]
        public string Jingle_Path { get; set; } = string.Empty;

        [Category(PROCESSOR)]
        [DisplayName("Jingle Adding")]
        public JingleAdding Add_Jingle { get; set; } = JingleAdding.None;

        [Category(RECORDER)]
        [DisplayName("Minutes on pregressbar")]
        public uint Recorder_MinutesOnProgressbar { get; set; } = 120;

        [Category(RECORDER)]
        [DisplayName("Filename")]
        public string Recorder_FileName { get; set; } = "MuVox {0:yyyy-MM-dd HHmmss}";

        [Category(RECORDER)]
        [DisplayName("Output path")]
        public string Recorder_OutputPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "MuVox");

        [ExpandableObject]
        public UI.VolumeMeter.VolumeMeterSettings VolumeMeterSettings { get; set; } = new UI.VolumeMeter.VolumeMeterSettings();

        [Browsable(false)]
        public string Recorder_LastFile { get; set; } = string.Empty;

        public static void Save()
        {
            if (current == null)
                return;

            var serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            var dir = Path.GetDirectoryName(FILE_PATH);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(FILE_PATH, JsonConvert.SerializeObject(current, serializerSettings));
        }

        public IEnumerable<string> Verify()
        {
            if (Add_Jingle != JingleAdding.None)
            {
                if (string.IsNullOrEmpty(Jingle_Path) || !File.Exists(Jingle_Path))
                    yield return $"'{nameof(Jingle_Path)}' must be a valid file when '{nameof(Add_Jingle)}' is not '{nameof(JingleAdding.None)}'";
            }
        }

        private static Settings LoadCurrent()
        {
            Settings newSettings;

            if (File.Exists(FILE_PATH))
                newSettings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(FILE_PATH));
            else
                newSettings = new Settings();

            return newSettings;
        }
    }
}
