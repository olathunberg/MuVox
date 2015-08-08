using NAudio.Lame;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace RecordToMP3.Features.Processor.Tools
{
    public class WaveToMp3Converter
    {
        public Task<string> Convert(string baseFilename, Action<string> addLogMessage, Action<long> sourceLengthCallback, Action<long> progressCallback)
        {
            Debug.Assert(addLogMessage != null);
            Debug.Assert(sourceLengthCallback != null);
            Debug.Assert(progressCallback != null);
            
            return Task.Run(() => DoConvert(baseFilename, addLogMessage, sourceLengthCallback, progressCallback));
        }

        private string DoConvert(string baseFilename, Action<string> addLogMessage, Action<long> sourceLengthCallback, Action<long> progressCallback)
        {
            var newFilename = Path.ChangeExtension(baseFilename, ".mp3");
            using (var reader = new WaveFileReader(baseFilename))
            {
                sourceLengthCallback(reader.Length);
                FileCreator.CreateMp3File(newFilename, reader, Properties.Settings.Default.PROCESSOR_MP3Quality, progressCallback);
            }

            return newFilename;
        }
    }
}
