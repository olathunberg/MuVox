using System;
using System.IO;
using System.Threading.Tasks;
using TTech.MuVox.Features.LogViewer;
using TTech.MuVox.Features.Processor.Tools;

namespace TTech.MuVox.Features.Processor
{
    public class Processor
    {
        private LogViewerModel logViewerModel;
        private readonly Action<long> setDetailProgressBarMaximum;
        private readonly Action<long> updateDetailProgressBar;

        private Settings.Settings Settings { get { return Features.Settings.SettingsBase<Settings.Settings>.Current; } }

        public Processor(LogViewerModel logViewerModel,
            Action<long> setDetailProgressBarMaximum,
            Action<long> updateDetailProgressBar)
        {
            this.logViewerModel = logViewerModel;
            this.setDetailProgressBarMaximum = setDetailProgressBarMaximum;
            this.updateDetailProgressBar = updateDetailProgressBar;
        }

        public async Task Process(string baseFileName)
        {
            if (string.IsNullOrEmpty(Settings.Processor_OutputPath))
            {
                throw new ArgumentException("Processor_OutputPath must be set in settings");
            }

            if (Path.GetDirectoryName(baseFileName) == Path.GetDirectoryName(Settings.Processor_OutputPath))
            {
                throw new ArgumentException("Processor_OutputPath must be another folder than sourcefile");
            }

            logViewerModel.Add("Splitting into tracks...");
            var waveFileCutter = new WaveFileCutter();
            var cuttedFiles = await waveFileCutter.CutWavFileFromMarkersFile(
                 baseFileName,
                 logViewerModel.Add,
                 setDetailProgressBarMaximum,
                 updateDetailProgressBar);

            var normalizer = new Normalizer();
            var waveToMp3Converter = new WaveToMp3Converter();
            var waveFileJoiner = new WaveFileJoiner();
            for (var i = 0; i < cuttedFiles.Count; i++)
            {
                var item = cuttedFiles[i];
                logViewerModel.Add(string.Format("Normalizing segment {0}...", item));
                await normalizer.Normalize(item, logViewerModel.Add, setDetailProgressBarMaximum, updateDetailProgressBar);

                if (AddJingle(i))
                {
                    logViewerModel.Add(string.Format("Adding jingle to segment {0}...", item));

                    string outFile = await waveFileJoiner.Join(new string[] { Settings.Jingle_Path, item }, logViewerModel.Add, setDetailProgressBarMaximum, updateDetailProgressBar);

                    if (item != baseFileName)
                        File.Delete(item);

                    var targetPath = Path.Combine(Path.GetDirectoryName(outFile), Path.GetFileName(item));
                    if (File.Exists(targetPath))
                        File.Delete(targetPath);
                    File.Move(outFile, targetPath);
                    item = targetPath;
                }

                logViewerModel.Add(string.Format("Converting segment {0} to MP3...", item));
                var mp3File = await waveToMp3Converter.Convert(item, logViewerModel.Add, setDetailProgressBarMaximum, updateDetailProgressBar);

                if (item != baseFileName)
                    File.Delete(item);
            }
        }

        private bool AddJingle(int segmentNumber)
        {
            switch (Settings.Add_Jingle)
            {
                case Features.Settings.JingleAdding.None:
                    return false;
                case Features.Settings.JingleAdding.FirstSegment:
                    return segmentNumber == 0;
                case Features.Settings.JingleAdding.AllSegments:
                    return true;
                default:
                    return false;
            }
        }
    }
}
