using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RecordToMP3.Features.Processor.Tools
{
    public class Mp3ToWaveConverter
    {
        public Task<string> Convert(string baseFilename, Action<string> progressCallback)
        {
            return Task<string>.Run(() => DoConvert(baseFilename, progressCallback));
        }

        private string DoConvert(string baseFilename, Action<string> progressCallback)
        {
            var newFilename = Path.ChangeExtension(baseFilename, ".wav");
            using (var reader = new Mp3FileReader(baseFilename))
            using (var wtr = new WaveFileWriter(newFilename, new WaveFormat(44100, 2)))
                reader.CopyTo(wtr);

            return newFilename;
        }
    }
}
