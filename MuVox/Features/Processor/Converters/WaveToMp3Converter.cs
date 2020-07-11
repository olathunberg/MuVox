using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;
using TTech.MuVox.Features.Processor.Tools;

namespace TTech.MuVox.Features.Processor.Converters
{
    public class WaveToMp3Converter
    {
        private Settings.Settings Settings { get { return Features.Settings.SettingsBase<Settings.Settings>.Current; } }

        public Task<string> Convert(string baseFilename, Action<string> addLogMessage, IProgress<long> progressMaximum, IProgress<long> progress)
        {
            Debug.Assert(addLogMessage != null);
            Debug.Assert(progressMaximum != null);
            Debug.Assert(progress != null);
            if (addLogMessage == null)
                return Task.FromResult(string.Empty);
            if (progressMaximum == null)
                return Task.FromResult(string.Empty);
            if (progress == null)
                return Task.FromResult(string.Empty);

            return Task.Run(() => DoConvert(baseFilename, addLogMessage, progressMaximum, progress));
        }

        private string DoConvert(string baseFilename, Action<string> addLogMessage, IProgress<long> progressMaximum, IProgress<long> progress)
        {
            var newFilename = Path.ChangeExtension(baseFilename, ".mp3");
      
            using (var reader = new WaveFileReader(baseFilename))
            {
                progressMaximum.Report(reader.Length);
                FileCreator.CreateMp3File(newFilename, reader, Settings.Processor_Mp3Quality, progress);
            }

            addLogMessage($"Created {newFilename}");

            return newFilename;
        }
    }
}
