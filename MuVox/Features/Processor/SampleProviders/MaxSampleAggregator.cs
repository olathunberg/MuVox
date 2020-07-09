using System;
using NAudio.Wave;

namespace TTech.MuVox.Features.Processor.SampleProviders
{
    public class MaxSampleAggregator : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly int channels;

        public MaxSampleAggregator(ISampleProvider source)
        {
            channels = source.WaveFormat.Channels;
            this.source = source;
        }

        public WaveFormat WaveFormat { get { return source.WaveFormat; } }

        public float MaxValue { get; private set; }

        public int Read(float[] buffer, int offset, int count)
        {
            var samplesRead = source.Read(buffer, offset, count);

            for (int n = 0; n < samplesRead; n += channels)
                Add(buffer[n + offset]);

            return samplesRead;
        }

        private void Add(float value)
        {
            MaxValue = Math.Max(MaxValue, value);
        }
    }
}
