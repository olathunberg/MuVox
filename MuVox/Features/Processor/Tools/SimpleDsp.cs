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
        public Task Process(string baseFilename, Action<string> addLogMessage, IProgress<long> progressMaximum, IProgress<long> progress)
        {
            return Task.Run(() => DoProcess(baseFilename, addLogMessage, progressMaximum, progress));
        }

        private void DoProcess(string baseFilename, Action<string> addLogMessage, IProgress<long> progressMaximum, IProgress<long> progress)
        {
            Debug.Assert(addLogMessage != null);
            Debug.Assert(progressMaximum != null);
            Debug.Assert(progress != null);

            if (addLogMessage == null)
                return;
            if (progressMaximum == null)
                return;
            if (progress == null)
                return;

            addLogMessage("Running compressor...");

            var tempFile = Path.ChangeExtension(baseFilename, ".temp");
            var maxValue = 0f;
            // Try to remove files, use some memory stream
            using (var reader = new WaveFileReader(baseFilename))
            {
                progressMaximum.Report(reader.Length);

                var sampleReader = new Pcm16BitToSampleProvider(reader);
                var compressor = new FastAttackCompressor1175(sampleReader);
                var aggregator = new MaxSampleAggregator(compressor);
                var sampleWriter = new SampleToWaveProvider16(aggregator);

                FileCreator.CreateWaveFile(tempFile, sampleWriter, progress);
                maxValue = aggregator.MaxValue;
                addLogMessage("Found max: " + aggregator.MaxValue.ToString());
            }

            File.Delete(baseFilename);

            addLogMessage("Normalizing...");
            using (var reader = new WaveFileReader(tempFile))
            {
                progressMaximum.Report(reader.Length);

                var sampleReader = new Pcm16BitToSampleProvider(reader);
                var normalizer = new SimpleNormalizer(sampleReader, .98f, maxValue);
                var sampleWriter = new SampleToWaveProvider16(normalizer);

                FileCreator.CreateWaveFile(baseFilename, sampleWriter, progress);
            }

            File.Delete(tempFile);
        }
    }
}
