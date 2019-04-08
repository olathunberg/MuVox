using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTech.Muvox.Features.Processor.Tools
{
    public class NormalizeProvider : ISampleProvider
    {
        #region Fields
        private readonly ISampleProvider sourceProvider;

        private readonly float ratio;
        #endregion

        public NormalizeProvider(ISampleProvider sourceProvider, float ratio, float currentMax)
        {
            this.sourceProvider = sourceProvider;
            this.ratio = ratio / currentMax;
            SampleRate = 44100;
        }

        #region Properties
        public string Name
        {
            get { return ""; }
        }

        public float SampleRate { get; set; }

        public WaveFormat WaveFormat
        {
            get { return sourceProvider.WaveFormat; }
        }
        #endregion

        #region Public methods
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

        public override string ToString()
        {
            return Name;
        }
        #endregion

        #region Private methods
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
        #endregion
    }
}
