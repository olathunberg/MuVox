using System;

namespace RecordToMP3.Features.Settings
{
    public class Settings :GalaSoft.MvvmLight.ObservableObject, ISettings
    {
        public string FILE_PATH
        {
            get { return @"\Settings\Settings.json"; }
        }

        public bool Verify()
        {
            return true;
        }
    }
}
