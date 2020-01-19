using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;

namespace TTech.Muvox.Features.Processor.Tools
{
    public class WaveToMp3Converter
    {
        private Settings.Settings Settings { get { return Features.Settings.SettingsBase<Settings.Settings>.Current; } }

        public Task<string> Convert(string baseFilename, Action<string> addLogMessage, Action<long> sourceLengthCallback, Action<long> progressCallback)
        {
            Debug.Assert(addLogMessage != null);
            Debug.Assert(sourceLengthCallback != null);
            Debug.Assert(progressCallback != null);
            if (addLogMessage == null)
                return Task.FromResult(string.Empty);
            if (sourceLengthCallback == null)
                return Task.FromResult(string.Empty);
            if (progressCallback == null)
                return Task.FromResult(string.Empty);

            return Task.Run(() => DoConvert(baseFilename, addLogMessage, sourceLengthCallback, progressCallback));
        }

        private string DoConvert(string baseFilename, Action<string> addLogMessage, Action<long> sourceLengthCallback, Action<long> progressCallback)
        {
            var newFilename = Path.ChangeExtension(baseFilename, ".mp3");
            using (var reader = new WaveFileReader(baseFilename))
            {
                sourceLengthCallback(reader.Length);
                FileCreator.CreateMp3File(newFilename, reader, Settings.Processor_Mp3Quality, progressCallback);
            }

            return newFilename;
        }
    }
}
