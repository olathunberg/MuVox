using NAudio.Lame;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RecordToMP3.Features.Processor.Tools
{
    public class WaveToMp3Converter
    {
        public Task<string> Convert(string baseFilename, Action<string> progressCallback)
        {
            return Task<string>.Run(() => DoConvert(baseFilename, progressCallback));
        }

        private string DoConvert(string baseFilename, Action<string> progressCallback)
        {
            var newFilename = Path.ChangeExtension(baseFilename, ".mp3");
            using (var reader = new WaveFileReader(baseFilename))
            using (var wtr = new LameMP3FileWriter(newFilename, reader.WaveFormat, Properties.Settings.Default.PROCESSOR_MP3Quality))
                reader.CopyTo(wtr);

            return newFilename;
        }
    }
}
