using NAudio.Wave;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTech.MuVox.Features.Processor.Tools
{
    public class WaveFileCutter
    {
        public Task<List<string>> CutWavFileFromMarkersFile(string baseFilename, Action<string> addLogMessage, Action<long> sourceLengthCallback, Action<long> progressCallback)
        {
            return Task.Run<List<string>>(() => DoCutWavFileFromMarkersFile(baseFilename, addLogMessage, sourceLengthCallback, progressCallback));
        }

        public void CutWavFileToEnd(string inPath, string outPath, TimeSpan cutFrom, Action<long> progressCallback)
        {
            Debug.Assert(progressCallback != null);
            if (progressCallback == null)
                return;
            using (var reader = new WaveFileReader(inPath))
            using (var writer = new WaveFileWriter(outPath, reader.WaveFormat))
            {
                int bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000;

                int startPos = (int)cutFrom.TotalMilliseconds * bytesPerMillisecond;
                startPos -= startPos % reader.WaveFormat.BlockAlign;

                var endPos = (int)reader.Length;

                CutWavFile(reader, writer, startPos, endPos, progressCallback);
            }
        }

        public void CutWavFile(string inPath, string outPath, TimeSpan cutFrom, TimeSpan cutTo, Action<long> progressCallback)
        {
            Debug.Assert(progressCallback != null);
            if (progressCallback == null)
                return;

            using (var reader = new WaveFileReader(inPath))
            using (var writer = new WaveFileWriter(outPath, reader.WaveFormat))
            {
                int bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000;

                int startPos = (int)cutFrom.TotalMilliseconds * bytesPerMillisecond;
                startPos -= startPos % reader.WaveFormat.BlockAlign;

                int endPos = (int)cutTo.TotalMilliseconds * bytesPerMillisecond;
                endPos -= endPos % reader.WaveFormat.BlockAlign;

                CutWavFile(reader, writer, startPos, endPos, progressCallback);
            }
        }

        private List<string> DoCutWavFileFromMarkersFile(string baseFilename, Action<string> addLogMessage, Action<long> sourceLengthCallback, Action<long> progressCallback)
        {
            if (Marker.MarkerHelper.HasMarkerFile(baseFilename))
            {
                using (var reader = new WaveFileReader(baseFilename))
                    sourceLengthCallback(reader.Length);

                var markers = Marker.MarkerHelper.GetMarkersFromFile(baseFilename);
                var newFiles = new ConcurrentBag<string>();

                addLogMessage("Creating " + (markers.Count + 1) + " segments");

                Parallel.For(0, markers.Count + 1, i =>
                {
                    if (i == markers.Count)
                    {
                        var start2 = new TimeSpan(0, 0, 0, 0, markers[i - 1] * 100);

                        var lastFilename = Path.ChangeExtension(baseFilename, "." + (markers.Count) + ".wav");
                        CutWavFileToEnd(baseFilename, lastFilename, start2, progressCallback);
                        newFiles.Add(lastFilename);
                    }
                    else
                    {
                        var marker = i == 0 ? 0 : markers[i - 1];
                        var start = new TimeSpan(0, 0, 0, 0, marker * 100);
                        var end = new TimeSpan(0, 0, 0, 0, markers[i] * 100);

                        var newFilename = Path.ChangeExtension(baseFilename, "." + i + ".wav");
                        CutWavFile(baseFilename, newFilename, start, end, progressCallback);
                        newFiles.Add(newFilename);
                    }
                });

                return newFiles.ToList();
            }
            using (var reader = new WaveFileReader(baseFilename))
                progressCallback(reader.Length);

            return new List<string> { baseFilename };
        }

        private void CutWavFile(WaveFileReader reader, WaveFileWriter writer, int startPos, int endPos, Action<long> progressCallback)
        {
            reader.Position = startPos;
            byte[] buffer = new byte[1024];
            while (reader.Position < endPos)
            {
                var bytesRequired = (int)(endPos - reader.Position);
                if (bytesRequired > 0)
                {
                    int bytesToRead = Math.Min(bytesRequired, buffer.Length);
                    int bytesRead = reader.Read(buffer, 0, bytesToRead);
                    if (bytesRead > 0)
                    {
                        writer.Write(buffer, 0, bytesRead);
                    }

                    progressCallback(bytesRead);
                }
            }
        }
    }
}
