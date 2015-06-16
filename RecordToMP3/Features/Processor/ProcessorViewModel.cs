using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NAudio.Lame;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using RecordToMP3.Features.Messages;
using RecordToMP3.Features.Processor.Tools;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RecordToMP3.Features.Processor
{
    public class ProcessorViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        #region Fields
        private LogViewer.LogViewerModel logViewerModel;
        private bool isProcessing;
        private long progressBarMaximum;
        private long progress;
        #endregion

        #region Constructors
        public ProcessorViewModel()
        {
            LogViewerModel = new LogViewer.LogViewerModel();
        }
        #endregion

        #region Commands
        private RelayCommand recordCommand;
        public ICommand Record
        {
            get
            {
                return recordCommand ?? (recordCommand = new RelayCommand(
                    () =>
                    {
                        Messenger.Default.Send<GotoPageMessage>(new GotoPageMessage(Pages.Recorder));
                    },
                    () => !IsProcessing));
            }
        }

        private RelayCommand startProcessingCommand;
        public ICommand StartProcessing
        {
            get
            {
                return startProcessingCommand ?? (startProcessingCommand = new RelayCommand(
                    async () => await ProcessFile(),
                    () => !IsProcessing));
            }
        }
        #endregion

        #region Properties
        public bool IsProcessing
        {
            get { return isProcessing; }
            set { isProcessing = value; RaisePropertyChanged(); }
        }

        public long ProgressBarMaximum
        {
            get { return progressBarMaximum; }
            set { progressBarMaximum = value; RaisePropertyChanged(); }
        }

        public long Progress
        {
            get { return progress; }
            set { progress = value; RaisePropertyChanged(); }
        }

        public string FileName { get { return Properties.Settings.Default.RECORDER_LastFile; } }

        public LogViewer.LogViewerModel LogViewerModel
        {
            get { return logViewerModel; }
            set
            {
                logViewerModel = value;
                RaisePropertyChanged(() => LogViewerModel);
            }
        }
        #endregion

        #region Overrides
        public override void Cleanup()
        {
            base.Cleanup();
        }
        #endregion

        #region Private methods
        private async Task ProcessFile()
        {
            LogViewerModel.Add("Started processing");

            try
            {
                IsProcessing = true;
                var baseFileName = FileName;

                var preConvert = Path.GetExtension(baseFileName) == ".mp3";
                if (preConvert)
                    using (var reader = new Mp3FileReader(baseFileName))
                        TotalProgressMaximum = reader.Length * (preConvert ? 4 : 3);
                else
                    using (var reader = new WaveFileReader(baseFileName))
                        TotalProgressMaximum = reader.Length * (preConvert ? 4 : 3);

                if (preConvert)
                {
                    LogViewerModel.Add("Converting input MP3 to wave...");

                    var mp3ToWave = new Mp3ToWaveConverter();
                    baseFileName = await mp3ToWave.Convert(baseFileName, message => logViewerModel.Add(message), max => SetDetailProgressBarMaximum(max), progress => UpdateDetailProgressBar(progress));
                }

                LogViewerModel.Add("Splitting into tracks...");
                var waveFileCutter = new WaveFileCutter();
                var cuttedFiles = await waveFileCutter.CutWavFileFromMarkersFile(
                     Path.ChangeExtension(baseFileName, ".markers"),
                     baseFileName,
                     message => LogViewerModel.Add(message),
                     max => SetDetailProgressBarMaximum(max),
                     progress => UpdateDetailProgressBar(progress));

                var normalizer = new Normalizer();
                var waveToMp3Converter = new WaveToMp3Converter();
                foreach (var item in cuttedFiles)
                {
                    LogViewerModel.Add(string.Format("Normalizing segment {0}...", item));
                    await normalizer.Normalize(item, message => LogViewerModel.Add(message), max => SetDetailProgressBarMaximum(max), progress => UpdateDetailProgressBar(progress));

                    LogViewerModel.Add(string.Format("Converting segment {0} to MP3...", item));
                    await waveToMp3Converter.Convert(item, message => LogViewerModel.Add(message), max => SetDetailProgressBarMaximum(max), progress => UpdateDetailProgressBar(progress));

                    //File.Delete(item);
                }
            }
            finally
            {
                IsProcessing = false;
            }

            LogViewerModel.Add("Finished processing");
        }

        private long totalProgress;
        public long TotalProgress
        {
            get { return totalProgress; }
            set { totalProgress = value; RaisePropertyChanged(); }
        }

        private long totalProgressMaximum;

        public long TotalProgressMaximum
        {
            get { return totalProgressMaximum; }
            set { totalProgressMaximum = value; RaisePropertyChanged(); }
        }


        private void SetDetailProgressBarMaximum(long max)
        {
            ProgressBarMaximum = max;
            Progress = 0;
        }

        private void UpdateDetailProgressBar(long increment)
        {
            TotalProgress += increment;
            Progress += increment;
        }
        #endregion

        #region Events
        #endregion
    }
}
