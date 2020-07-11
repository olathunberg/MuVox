using NAudio.Lame;
using NAudio.Wave;
using System;
using System.IO;

namespace TTech.MuVox.Features.Processor.Tools
{
    internal static class FileCreator
    {
        internal static void CreateWaveFile(string filename, IWaveProvider sourceProvider, IProgress<long> progress)
        {
            using (var writer = new WaveFileWriter(filename, sourceProvider.WaveFormat))
            {
                long outputLength = 0;
                var buffer = new byte[sourceProvider.WaveFormat.AverageBytesPerSecond * 4];
                while (true)
                {
                    int bytesRead = sourceProvider.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        // end of source provider
                        break;
                    }
                    outputLength += bytesRead;
                    // Write will throw exception if WAV file becomes too large
                    writer.Write(buffer, 0, bytesRead);

                    if (progress != null)
                        progress.Report(bytesRead);
                }
            }
        }

        internal static void CreateMp3File(string filename, IWaveProvider sourceProvider, int bitRate, IProgress<long> progress)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filename)))
                Directory.CreateDirectory(Path.GetDirectoryName(filename));

            using (var writer = new LameMP3FileWriter(filename, sourceProvider.WaveFormat, bitRate))
            {
                long outputLength = 0;
                var buffer = new byte[sourceProvider.WaveFormat.AverageBytesPerSecond * 4];
                while (true)
                {
                    int bytesRead = sourceProvider.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        // end of source provider
                        break;
                    }
                    outputLength += bytesRead;
                    // Write will throw exception if WAV file becomes too large
                    writer.Write(buffer, 0, bytesRead);

                    if (progress != null)
                        progress.Report(bytesRead);
                }
            }
        }
    }
}
