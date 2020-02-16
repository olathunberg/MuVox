using NAudio.Wave;
using System;
using System.Diagnostics;

namespace TTech.MuVox.Features.Processor.Tools
{
    public class MaxSampleAggregator : ISampleProvider
    {
        public event EventHandler<MaxSampleEventArgs>? MaximumCalculated;
        private float maxValue;
        private float minValue;

        private readonly ISampleProvider source;

        private readonly int channels;

        public MaxSampleAggregator(ISampleProvider source)
        {
            channels = source.WaveFormat.Channels;
            this.source = source;
        }

        private void Add(float value)
        {
            maxValue = Math.Max(maxValue, value);
            minValue = Math.Min(minValue, value);
        }

        public WaveFormat WaveFormat { get { return source.WaveFormat; } }

        public int Read(float[] buffer, int offset, int count)
        {
            var samplesRead = source.Read(buffer, offset, count);

            for (int n = 0; n < samplesRead; n += channels)
                Add(buffer[n + offset]);

            MaximumCalculated?.Invoke(this, new MaxSampleEventArgs(minValue, maxValue));

            return samplesRead;
        }
    }

    public class MaxSampleEventArgs : EventArgs
    {
        [DebuggerStepThrough]
        public MaxSampleEventArgs(float minValue, float maxValue)
        {
            this.MaxSample = maxValue;
            this.MinSample = minValue;
        }
        public float MaxSample { get; private set; }
        public float MinSample { get; private set; }
    }

}
