using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NAudio.Wave;

namespace TTech.MuVox.Features.Processor.Tools
{
    public class WaveFileJoiner
    {
        private Settings.Settings Settings { get { return Features.Settings.SettingsBase<Settings.Settings>.Current; } }

        public async Task<string> Join(string[] files, Action<string> addLogMessage, Action<long> sourceLengthCallback, Action<long> progressCallback)
        {
            if (files.Length < 2)
                return string.Empty;

            WaveFileWriter? waveFileWriter = null;

            var newFilename = Path.GetFileNameWithoutExtension(Path.GetRandomFileName() + Path.GetExtension(files.First()));
            if (!string.IsNullOrEmpty(Settings.Processor_OutputPath))
                newFilename = Path.Combine(Settings.Processor_OutputPath, Path.GetFileName(newFilename));

            if (!Directory.Exists(Path.GetDirectoryName(newFilename)))
                Directory.CreateDirectory(Path.GetDirectoryName(newFilename));

            try
            {
                foreach (string sourceFile in files)
                {
                    using (WaveFileReader reader = new WaveFileReader(sourceFile))
                    {
                        if (waveFileWriter == null)
                        {
                            // first time in create new Writer
                            waveFileWriter = new WaveFileWriter(newFilename, reader.WaveFormat);
                        }
                        else if (!reader.WaveFormat.Equals(waveFileWriter.WaveFormat))
                        {
                            throw new InvalidOperationException("Can't concatenate WAV Files that don't share the same format");
                        }

                        var buffer = new byte[reader.WaveFormat.AverageBytesPerSecond * 4];

                        while (true)
                        {
                            int bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length);
                            if (bytesRead == 0)
                            {
                                // end of source provider
                                break;
                            }
                            // Write will throw exception if WAV file becomes too large
                            await waveFileWriter.WriteAsync(buffer, 0, bytesRead);

                            progressCallback(bytesRead);
                        }
                    }
                }
            }
            finally
            {
                waveFileWriter?.Dispose();
            }

            return newFilename;
        }
    }
}
