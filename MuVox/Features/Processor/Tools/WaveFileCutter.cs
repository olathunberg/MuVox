using NAudio.Wave;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TTech.MuVox.Features.Marker;

namespace TTech.MuVox.Features.Processor.Tools
{
    public class WaveFileCutter
    {
        private Settings.Settings Settings { get { return Features.Settings.SettingsBase<Settings.Settings>.Current; } }

        public Task<List<string>> CutWavFileFromMarkersFile(string baseFilename, Action<string> addLogMessage, Action<long> sourceLengthCallback, Action<long> progressCallback)
        {
            return Task.Run<List<string>>(() => DoCutWavFileFromMarkersFile(baseFilename, addLogMessage, sourceLengthCallback, progressCallback));
        }

        public void CutWavFileToEnd(string inPath, string outPath, TimeSpan cutFrom, Action<long> progressCallback)
        {
            Debug.Assert(progressCallback != null);
            if (progressCallback == null)
                return;

            EnsureOutputDirectory(outPath);

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

            EnsureOutputDirectory(outPath);

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

        private static void EnsureOutputDirectory(string outPath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(outPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outPath));
            }
        }

        private string GetTargetFileName(string baseFilename, int fileIndex)
        {
            if (!string.IsNullOrEmpty(Settings.Processor_OutputPath))
                baseFilename = Path.Combine(Settings.Processor_OutputPath, Path.GetFileName(baseFilename));

            return Path.ChangeExtension(baseFilename, "." + fileIndex + ".wav");
        }

        private List<string> DoCutWavFileFromMarkersFile(string baseFilename, Action<string> addLogMessage, Action<long> sourceLengthCallback, Action<long> progressCallback)
        {
            if (MarkerHelper.HasMarkerFile(baseFilename))
            {
                using (var reader = new WaveFileReader(baseFilename))
                    sourceLengthCallback(reader.Length);

                var markers = MarkerHelper
                    .GetMarkersFromFile(baseFilename);

                var newFiles = new ConcurrentBag<string>();

                addLogMessage("Creating segments");
                var fileIndex = 0;
                Parallel.For(0, markers.Count + 1, i =>
                {
                    fileIndex++;
                    if (i == markers.Count)
                    {
                        if (markers[i - 1].Type == Marker.Marker.MarkerType.RemoveAfter)
                            return;

                        var start2 = new TimeSpan(0, 0, 0, 0, markers[i - 1].Time * 100);

                        var lastFilename = GetTargetFileName(baseFilename, fileIndex);
                        CutWavFileToEnd(baseFilename, lastFilename, start2, progressCallback);
                        newFiles.Add(lastFilename);
                    }
                    else
                    {
                        if (markers[i].Type == Marker.Marker.MarkerType.RemoveBefore || (i > 0 && markers[i - 1].Type == Marker.Marker.MarkerType.RemoveAfter))
                            return;

                        var marker = i == 0 ? 0 : markers[i - 1].Time;
                        var start = new TimeSpan(0, 0, 0, 0, marker * 100);
                        var end = new TimeSpan(0, 0, 0, 0, markers[i].Time * 100);

                        var newFilename = GetTargetFileName(baseFilename, fileIndex);
                        CutWavFile(baseFilename, newFilename, start, end, progressCallback);
                        newFiles.Add(newFilename);
                    }
                });

                newFiles = EnsureSingleFileFilename(newFiles);

                return newFiles.ToList();
            }

            using (var reader = new WaveFileReader(baseFilename))
                progressCallback(reader.Length);

            return new List<string> { baseFilename };
        }

        private static ConcurrentBag<string> EnsureSingleFileFilename(ConcurrentBag<string> newFiles)
        {
            if (newFiles.Count() == 1)
            {
                var splitFilename = newFiles.First().Split('.');
                var firstPart = splitFilename.Take(splitFilename.Count() - 2).Select(x => x).ToList();
                firstPart.Add(splitFilename.Last());
                var newFilename = string.Join(".", firstPart.ToArray());

                if(File.Exists(newFilename))
                {
                    File.Delete(newFilename);
                }
                File.Move(newFiles.First(), newFilename);
                newFiles = new ConcurrentBag<string>() { newFilename };
            }

            return newFiles;
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
