using NAudio.Lame;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordToMP3.Features.Processor.Tools
{
    public class Normalizer
    {
        public Task Normalize(string baseFilename, Action<string> addLogMessage, Action<long> sourceLengthCallback, Action<long> progressCallback)
        {
            return Task.Run(() => DoNormalize(baseFilename, addLogMessage, sourceLengthCallback, progressCallback));
        }

        private void DoNormalize(string baseFilename, Action<string> addLogMessage, Action<long> sourceLengthCallback, Action<long> progressCallback)
        {
            Debug.Assert(addLogMessage != null);
            Debug.Assert(sourceLengthCallback != null);
            Debug.Assert(progressCallback != null);


            addLogMessage("Running compressor...");

            float maxValue = 0f;
            var tempFile = Path.ChangeExtension(baseFilename, ".temp");
            // Try to remove files, use some memory stream
            using (var reader = new WaveFileReader(baseFilename))
            {
                sourceLengthCallback(reader.Length);

                var sampleReader = new Pcm16BitToSampleProvider(reader);
                var compressor = new FastAttackCompressor1175(sampleReader);
                var aggregator = new MaxSampleAggregator(compressor);
                //SimpleCompressorStream
                aggregator.MaximumCalculated += (s, a) => maxValue = Math.Max(maxValue, a.MaxSample);
                var sampleWriter = new SampleToWaveProvider16(aggregator);
                FileCreator.CreateWaveFile(tempFile, sampleWriter, progressCallback);
            }

            File.Delete(baseFilename);
            addLogMessage("Found max: " + maxValue.ToString());

            addLogMessage("Normalizing...");
            using (var reader = new WaveFileReader(tempFile))
            {
                sourceLengthCallback(reader.Length);

                var sampleReader = new Pcm16BitToSampleProvider(reader);
                var normalizer = new NormalizeProvider(sampleReader, .98f, maxValue);
                var sampleWriter = new SampleToWaveProvider16(normalizer);
                FileCreator.CreateWaveFile(baseFilename, sampleWriter, progressCallback);
            }

            File.Delete(tempFile);
        }


    }
}
