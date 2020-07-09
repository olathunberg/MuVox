using NAudio.Wave;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TTech.MuVox.Features.Processor.Tools;

namespace TTech.MuVox.Features.Processor.Converters
{
    public class Mp3ToWaveConverter
    {
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
            var newFilename = Path.ChangeExtension(baseFilename, ".wav");
            using (var reader = new Mp3FileReader(baseFilename))
            {
                progressMaximum.Report(reader.Length);
                FileCreator.CreateWaveFile(newFilename, reader, progress);
            }

            return newFilename;
        }
    }
}
