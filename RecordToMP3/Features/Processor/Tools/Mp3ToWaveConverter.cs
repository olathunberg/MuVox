using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace RecordToMP3.Features.Processor.Tools
{
    public class Mp3ToWaveConverter
    {
        public Task<string> Convert(string baseFilename, Action<string> addLogMessage, Action<long> sourceLengthCallback, Action<long> progressCallback)
        {
            Debug.Assert(addLogMessage != null);
            Debug.Assert(sourceLengthCallback != null);
            Debug.Assert(progressCallback != null);
            
            return Task<string>.Run(() => DoConvert(baseFilename, addLogMessage, sourceLengthCallback, progressCallback));
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
