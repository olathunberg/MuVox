using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TTech.MuVox.Features.Processor.SampleProviders;

namespace TTech.MuVox.Features.Processor.Tools
{
    public class SimpleDsp
    {
        public Task Process(string baseFilename, Action<string> addLogMessage, Action<long> sourceLengthCallback, Action<long> progressCallback)
        {
            return Task.Run(() => DoProcess(baseFilename, addLogMessage, sourceLengthCallback, progressCallback));
        }

        private void DoProcess(string baseFilename, Action<string> addLogMessage, Action<long> sourceLengthCallback, Action<long> progressCallback)
        {
            Debug.Assert(addLogMessage != null);
            Debug.Assert(sourceLengthCallback != null);
            Debug.Assert(progressCallback != null);

            if (addLogMessage == null)
                return;
            if (sourceLengthCallback == null)
                return;
            if (progressCallback == null)
                return;

            addLogMessage("Running compressor...");

            var tempFile = Path.ChangeExtension(baseFilename, ".temp");
            var maxValue = 0f;
            // Try to remove files, use some memory stream
            using (var reader = new WaveFileReader(baseFilename))
            {
                sourceLengthCallback(reader.Length);

                var sampleReader = new Pcm16BitToSampleProvider(reader);
                var compressor = new FastAttackCompressor1175(sampleReader);
                var aggregator = new MaxSampleAggregator(compressor);
                var sampleWriter = new SampleToWaveProvider16(aggregator);

                FileCreator.CreateWaveFile(tempFile, sampleWriter, progressCallback);
                maxValue = aggregator.MaxValue;
                addLogMessage("Found max: " + aggregator.MaxValue.ToString());
            }

            File.Delete(baseFilename);

            addLogMessage("Normalizing...");
            using (var reader = new WaveFileReader(tempFile))
            {
                sourceLengthCallback(reader.Length);

                var sampleReader = new Pcm16BitToSampleProvider(reader);
                var normalizer = new SimpleNormalizer(sampleReader, .98f, maxValue);
                var sampleWriter = new SampleToWaveProvider16(normalizer);

                FileCreator.CreateWaveFile(baseFilename, sampleWriter, progressCallback);
            }

            File.Delete(tempFile);
        }
    }
}
