using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordToMP3.Features.Processor.Effects
{
    public static class WaveFileCutter
    {
        public static Task CutWavFileFromMarkersFile(string markerFilename, string baseFilename, Action<string> progressCallback)
        {
            return Task.Run(() => DoCutWavFileFromMarkersFile(markerFilename, baseFilename, progressCallback));
        }

        public static void CutWavFileToEnd(string inPath, string outPath, TimeSpan cutFrom)
        {
            using (var reader = new WaveFileReader(inPath))
            using (var writer = new WaveFileWriter(outPath, reader.WaveFormat))
            {
                int bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000;

                int startPos = (int)cutFrom.TotalMilliseconds * bytesPerMillisecond;
                startPos = startPos - startPos % reader.WaveFormat.BlockAlign;

                int endPos = (int)reader.Length;

                CutWavFile(reader, writer, startPos, endPos);
            }
        }

        public static void CutWavFile(string inPath, string outPath, TimeSpan cutFrom, TimeSpan cutTo)
        {
            using (var reader = new WaveFileReader(inPath))
            using (var writer = new WaveFileWriter(outPath, reader.WaveFormat))
            {
                int bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000;

                int startPos = (int)cutFrom.TotalMilliseconds * bytesPerMillisecond;
                startPos = startPos - startPos % reader.WaveFormat.BlockAlign;

                int endPos = (int)cutTo.TotalMilliseconds * bytesPerMillisecond;
                endPos = endPos - endPos % reader.WaveFormat.BlockAlign;

                CutWavFile(reader, writer, startPos, endPos);
            }
        }

        private static void DoCutWavFileFromMarkersFile(string markerFilename, string baseFilename, Action<string> progressCallback)
        {
            if (File.Exists(markerFilename))
            {
                List<int> markers = new List<int>();

                using (var file = File.OpenText(markerFilename))
                {
                    string line = null;
                    while ((line = file.ReadLine()) != null)
                    {
                        markers.Add(int.Parse(line));
                    }
                }
                progressCallback("Found " + (markers.Count + 1) + " segments");

                int marker = 0;
                int nextMarker = 0;
                for (int i = 0; i < markers.Count; i++)
                {
                    nextMarker = markers[i];
                    var start = new TimeSpan(0, 0, 0, 0, marker * 100);
                    var end = new TimeSpan(0, 0, 0, 0, nextMarker * 100);
                    progressCallback("Creating segment " + (i + 1));
                    CutWavFile(baseFilename, baseFilename + "." + i.ToString() + ".wav", start, end);
                    marker = nextMarker;
                }
                progressCallback("Creating segment " + (markers.Count + 1));
                var start2 = new TimeSpan(0, 0, 0, 0, marker * 100);
                CutWavFileToEnd(baseFilename, baseFilename + "." + (markers.Count).ToString() + ".wav", start2);
            }
        }

        private static void CutWavFile(WaveFileReader reader, WaveFileWriter writer, int startPos, int endPos)
        {
            reader.Position = startPos;
            byte[] buffer = new byte[1024];
            while (reader.Position < endPos)
            {
                int bytesRequired = (int)(endPos - reader.Position);
                if (bytesRequired > 0)
                {
                    int bytesToRead = Math.Min(bytesRequired, buffer.Length);
                    int bytesRead = reader.Read(buffer, 0, bytesToRead);
                    if (bytesRead > 0)
                    {
                        writer.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }
    }
}
