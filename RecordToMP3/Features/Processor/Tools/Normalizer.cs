using NAudio.Lame;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordToMP3.Features.Processor.Tools
{
    public class Normalizer
    {
        public Task Normalize(string baseFilename, Action<string> progressCallback)
        {
            return Task.Run(() => DoNormalize(baseFilename, progressCallback));
        }

        private void DoNormalize(string baseFilename, Action<string> progressCallback)
        {
            progressCallback("Running compressor...");

            var tempFile = Path.ChangeExtension(baseFilename, ".temp");
            // Try to remove files, use some memory stream
            using (var reader = new WaveFileReader(baseFilename))
            {
                var sampleReader = new Pcm16BitToSampleProvider(reader);
                var compressor = new FastAttackCompressor1175(sampleReader);
                WaveFileWriter.CreateWaveFile16(tempFile, compressor);
            }

            File.Delete(baseFilename);
            progressCallback("Getting max value...");
            var maxValue = GetMaxValue(tempFile);
            progressCallback("Found max: " + maxValue.ToString());

            progressCallback("Normalizing...");
            using (var reader = new WaveFileReader(tempFile))
            {
                var sampleReader = new Pcm16BitToSampleProvider(reader);
                var compressor = new NormalizeProvider(sampleReader, .98f / maxValue);
                WaveFileWriter.CreateWaveFile16(baseFilename, compressor);
            }

            File.Delete(tempFile);
        }

        private float GetMaxValue(string baseFilename)
        {
            float maxValue = 0f;
            using (var reader = new AudioFileReader(baseFilename))
            {
                var aggregator = new MaxSampleAggregator(reader);
                aggregator.NotificationCount = reader.WaveFormat.SampleRate / 100;
                aggregator.MaximumCalculated += (s, a) => maxValue = Math.Max(maxValue, a.MaxSample);

                var toRead = reader.Length;
                var buffer = new float[8192];
                while (toRead > 0)
                {
                    int bytes = 8192;
                    int bytesRead = aggregator.Read(buffer, 0, bytes);
                    if (bytesRead == 0) break;
                    toRead -= bytesRead;
                }
            }

            return maxValue;
        }
    }
}
