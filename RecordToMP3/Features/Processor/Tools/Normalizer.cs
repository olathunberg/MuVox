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

            float maxValue = 0f;
            var tempFile = Path.ChangeExtension(baseFilename, ".temp");
            // Try to remove files, use some memory stream
            using (var reader = new WaveFileReader(baseFilename))
            {
                var sampleReader = new Pcm16BitToSampleProvider(reader);
                //var compressor = new FastAttackCompressor1175(sampleReader);
                var aggregator = new MaxSampleAggregator(sampleReader);
                aggregator.MaximumCalculated += (s, a) => maxValue = Math.Max(maxValue, a.MaxSample);
                WaveFileWriter.CreateWaveFile16(tempFile, aggregator);
            }

            File.Delete(baseFilename);
            progressCallback("Found max: " + maxValue.ToString());

            progressCallback("Normalizing...");
            using (var reader = new WaveFileReader(tempFile))
            {
                var sampleReader = new Pcm16BitToSampleProvider(reader);
                var normalizer = new NormalizeProvider(sampleReader, .98f, maxValue);
                WaveFileWriter.CreateWaveFile16(baseFilename, normalizer);
            }

            File.Delete(tempFile);
        }
    }
}
