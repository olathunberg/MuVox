using System;
using System.IO;
using System.Threading.Tasks;
using TTech.MuVox.Features.LogViewer;
using TTech.MuVox.Features.Processor.Converters;
using TTech.MuVox.Features.Processor.Tools;
using TTech.MuVox.Features.Settings;

namespace TTech.MuVox.Features.Processor
{
    public class Processor
    {
        private LogViewerModel logViewerModel;
        private readonly IProgress<long> progressMax;
        private readonly IProgress<long> detailProgress;

        private Settings.Settings Settings => Features.Settings.Settings.Current;

        public Processor(LogViewerModel logViewerModel,
            IProgress<long> progressMax,
            IProgress<long> detailProgress)
        {
            this.logViewerModel = logViewerModel;
            this.progressMax = progressMax;
            this.detailProgress = detailProgress;
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
                 progressMax,
                 detailProgress);

            var simpleDsp = new SimpleDsp();
            var waveToMp3Converter = new WaveToMp3Converter();
            var waveFileJoiner = new WaveFileJoiner();

            for (var i = 0; i < cuttedFiles.Count; i++)
            {
                var item = cuttedFiles[i];
                logViewerModel.Add(string.Format("Normalizing segment {0}...", item));
                await simpleDsp.Process(item, logViewerModel.Add, progressMax, detailProgress);

                if (AddJingle(i))
                {
                    if (string.IsNullOrEmpty(Settings.Jingle_Path))
                    {
                        throw new ArgumentException($"Jingle_Path must be when {nameof(JingleAdding)} is not {nameof(JingleAdding.None)}");
                    }
                    logViewerModel.Add(string.Format("Adding jingle to segment {0}...", item));

                    string outFile = await waveFileJoiner.Join(new string[] { Settings.Jingle_Path, item }, logViewerModel.Add, progressMax, detailProgress);

                    if (item != baseFileName)
                        File.Delete(item);

                    var targetPath = Path.Combine(Path.GetDirectoryName(outFile), Path.GetFileName(item));
                    if (File.Exists(targetPath))
                        File.Delete(targetPath);
                    File.Move(outFile, targetPath);
                    item = targetPath;
                }

                logViewerModel.Add(string.Format("Converting segment {0} to MP3...", item));
                var mp3File = await waveToMp3Converter.Convert(item, logViewerModel.Add, progressMax, detailProgress);

                if (item != baseFileName)
                    File.Delete(item);
            }
        }

        private bool AddJingle(int segmentNumber)
        {
            switch (Settings.Add_Jingle)
            {
                case JingleAdding.None:
                    return false;
                case JingleAdding.FirstSegment:
                    return segmentNumber == 0;
                case JingleAdding.AllSegments:
                    return true;
                default:
                    return false;
            }
        }
    }
}
