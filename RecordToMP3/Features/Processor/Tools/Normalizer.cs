using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordToMP3.Features.Processor.Tools
{
    public class Normalizer
    {
        private float maxValue;

        public Task Normalize(string baseFilename, Action<string> progressCallback)
        {
            return Task.Run(() => GetMaxValue(baseFilename, progressCallback));
        }

        protected void OnMaximumCalculated(MaxSampleEventArgs e)
        {
            maxValue = Math.Max(maxValue, e.MaxSample);
        }

        private void GetMaxValue(string baseFilename, Action<string> progressCallback)
        {
            using (var reader = new AudioFileReader(baseFilename))
            {
                var aggregator = new SampleAggregator(reader);
                aggregator.NotificationCount = reader.WaveFormat.SampleRate / 100;
                aggregator.MaximumCalculated += (s, a) => OnMaximumCalculated(a);
                
                var toRead = reader.Length;
                var buffer = new float[8192];
                while (toRead > 0)
                {
                    int bytes = 8192;
                    int bytesRead = aggregator.Read(buffer, 0, bytes);
                    if (bytesRead == 0) break;
                    toRead -= bytesRead;
                }
                progressCallback("Found max: " + maxValue.ToString());
            }
        }
    }
}
