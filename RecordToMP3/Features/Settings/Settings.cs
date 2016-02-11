using System;
using System.ComponentModel;

namespace RecordToMP3.Features.Settings
{
    public class Settings : GalaSoft.MvvmLight.ObservableObject, ISettings
    {
        public string FILE_PATH
        {
            get { return @"\Settings\Settings.json"; }
        }

        public bool AutoSave
        {
            get { return false; }
        }

        public bool Verify()
        {
            return true;
        }

        private string  outputPath = "";
        [DisplayName("Output path")]
        [Category("Processor")]
        public string OutputPath
        {
            get { return outputPath; }
            set { Set(ref outputPath, value); }
        }
    }
}
