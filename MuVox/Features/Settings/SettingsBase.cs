using System;
using System.IO;
using Newtonsoft.Json;

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
    public static class SettingsBase<T> where T : GalaSoft.MvvmLight.ObservableObject, ISettings, new()
    {
        private static T? current;

        public static T Current
        {
            get
            {
                if (current == null)
                    current = LoadCurrent();
                return current;
            }
        }

        public static void Save()
        {
            if (current == null)
                return;

            var serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            var dir = Path.GetDirectoryName(current.FILE_PATH);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(current.FILE_PATH, JsonConvert.SerializeObject(current, serializerSettings));
        }

        private static void AutoSave()
        {
            if (current == null || !current.AutoSave)
                return;

            Save();
        }

        private static T LoadCurrent()
        {
            T newSettings;

            if (File.Exists(new T().FILE_PATH))
                newSettings = JsonConvert.DeserializeObject<T>(File.ReadAllText(new T().FILE_PATH));
            else
                newSettings = new T();

            newSettings.PropertyChanged += (s, e) => AutoSave();

            return newSettings;
        }
    }
}
