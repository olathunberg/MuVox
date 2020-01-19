using NAudio.Wave;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace TTech.Muvox.Features.Processor.Tools
{
    public class Mp3ToWaveConverter
    {
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
            var newFilename = Path.ChangeExtension(baseFilename, ".wav");
            using (var reader = new Mp3FileReader(baseFilename))
            {
                sourceLengthCallback(reader.Length);
                FileCreator.CreateWaveFile(newFilename, reader, progressCallback);
            }

            return newFilename;
        }
    }
}
