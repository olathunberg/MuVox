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
        #endregion

        #region Constructors
        public ProcessorViewModel()
        {
            ProgressBarMaximum = Properties.Settings.Default.UI_MinutesOnProgressBar * 600;
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

        public uint ProgressBarMaximum { get; set; }

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
                if (Path.GetExtension(baseFileName) == ".mp3")
                {
                    LogViewerModel.Add("Converting input MP3 to wave...");

                    var mp3ToWave = new Mp3ToWaveConverter();
                    baseFileName = await mp3ToWave.Convert(baseFileName, message => logViewerModel.Add(message));
                }

                LogViewerModel.Add("Splitting into tracks...");
                var waveFileCutter = new WaveFileCutter();
                var cuttedFiles = await waveFileCutter.CutWavFileFromMarkersFile(
                     Path.ChangeExtension(baseFileName, ".markers"),
                     baseFileName,
                     message => LogViewerModel.Add(message));

                var normalizer = new Normalizer();
                var waveToMp3Converter = new WaveToMp3Converter();
                foreach (var item in cuttedFiles)
                {
                    LogViewerModel.Add(string.Format("Normalizing segment {0}...", item));
                    await normalizer.Normalize(item, message => LogViewerModel.Add(message));

                    LogViewerModel.Add(string.Format("Converting segment {0} to MP3...", item));
                    await waveToMp3Converter.Convert(item, message => LogViewerModel.Add(message));

                    //File.Delete(item);
                }
            }
            finally
            {
                IsProcessing = false;
            }

            LogViewerModel.Add("Finished processing");
        }
        #endregion

        #region Events
        #endregion
    }
}
