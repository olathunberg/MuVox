using NAudio.Wave;

namespace TTech.MuVox.Features.Processor.SampleProviders
{
    public class SimpleNormalizer : ISampleProvider
    {
        private readonly ISampleProvider sourceProvider;
        private readonly float ratio;

        public SimpleNormalizer(ISampleProvider sourceProvider, float ratio, float currentMax)
        {
            this.sourceProvider = sourceProvider;
            this.ratio = ratio / currentMax;
            SampleRate = 44100;
        }

        public float SampleRate { get; set; }

        public WaveFormat WaveFormat
        {
            get { return sourceProvider.WaveFormat; }
        }

        public void OnSample(ref float left, ref float right)
        {
            left *= ratio;
            right *= ratio;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int read = sourceProvider.Read(buffer, offset, count);

            Process(buffer, offset, read);

            return read;
        }

        private void Process(float[] buffer, int offset, int count)
        {
            int samples = count;

            for (int sample = 0; sample < samples; sample++)
            {
                float sampleLeft = buffer[offset];
                float sampleRight = sampleLeft;
                if (WaveFormat.Channels == 2)
                {
                    sampleRight = buffer[offset + 1];
                    sample++;
                }

                OnSample(ref sampleLeft, ref sampleRight);

                // put them back
                buffer[offset++] = sampleLeft;
                if (WaveFormat.Channels == 2)
                    buffer[offset++] = sampleRight;
            }
        }
    }
}
