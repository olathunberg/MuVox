using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using NAudio.Wave;
using TTech.Muvox.Features.Messages;
using TTech.Muvox.Features.Processor.Tools;

namespace TTech.Muvox.Features.Processor
{
    public class ProcessorViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        #region Fields
        private bool isProcessing;
        private long progressBarMaximum;
        private long progress;
        private long totalProgress;
        private long totalProgressMaximum;
        #endregion

        #region Constructors
        public ProcessorViewModel()
        {
            LogViewerModel = new LogViewer.LogViewerModel();
        }
        #endregion

        #region Commands
        private RelayCommand? recordCommand;
        public ICommand Record
        {
            get
            {
                return recordCommand ?? (recordCommand = new RelayCommand(
                    () =>
                    {
                        Messenger.Default.Send(new GotoPageMessage(Pages.Recorder));
                    },
                    () => !IsProcessing));
            }
        }

        private RelayCommand? editMarkersCommand;
        public ICommand EditMarkers
        {
            get
            {
                return editMarkersCommand ?? (editMarkersCommand = new RelayCommand(
                    () =>
                    {
                        Messenger.Default.Send<GotoPageMessage>(new GotoPageMessage(Pages.Marker));
                    },
                    () => !IsProcessing));
            }
        }

        private RelayCommand? startProcessingCommand;
        public ICommand StartProcessing
        {
            get
            {
                return startProcessingCommand ?? (startProcessingCommand = new RelayCommand(
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
                    async () => await ProcessFile(),
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
                    () => !IsProcessing));
            }
        }

        private RelayCommand? chooseFileCommand;
        public ICommand ChooseFile
        {
            get
            {
                return chooseFileCommand ?? (chooseFileCommand = new RelayCommand(
                    ShowFileDialog,
                    () => !IsProcessing));
            }
        }
        #endregion

        #region Properties
        public bool IsProcessing
        {
            get { return isProcessing; }
            set { isProcessing = value; RaisePropertyChanged(); RaiseCanExecuteChanged(); }
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

        public string FileName
        {
            get { return MuVox.Properties.Settings.Default.RECORDER_LastFile; }
            set
            {
                if (value == null)
                    return;

                MuVox.Properties.Settings.Default.RECORDER_LastFile = value;
                MuVox.Properties.Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public LogViewer.LogViewerModel LogViewerModel { get; set; }

        public long TotalProgress
        {
            get { return totalProgress; }
            set { totalProgress = value; RaisePropertyChanged(); RaiseCanExecuteChanged(); }
        }

        private void RaiseCanExecuteChanged()
        {
            recordCommand?.RaiseCanExecuteChanged();
            editMarkersCommand?.RaiseCanExecuteChanged();
            startProcessingCommand?.RaiseCanExecuteChanged();
            chooseFileCommand?.RaiseCanExecuteChanged();
        }

        public long TotalProgressMaximum
        {
            get { return totalProgressMaximum; }
            set { totalProgressMaximum = value; RaisePropertyChanged(); }
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
            if (!File.Exists(FileName))
            {
                LogViewerModel.Add("File does not exist");
                return;
            }

            try
            {
                TotalProgress = 0;

                IsProcessing = true;
                var baseFileName = FileName;

                var preConvert = Path.GetExtension(baseFileName) == ".mp3";
                if (preConvert)
                    using (var reader = new Mp3FileReader(baseFileName))
                        TotalProgressMaximum = reader.Length * 5;
                else
                    using (var reader = new WaveFileReader(baseFileName))
                        TotalProgressMaximum = reader.Length * 4;

                if (preConvert)
                {
                    LogViewerModel.Add("Converting input MP3 to wave...");

                    var mp3ToWave = new Mp3ToWaveConverter();
                    baseFileName = await mp3ToWave.Convert(baseFileName, LogViewerModel.Add, SetDetailProgressBarMaximum, UpdateDetailProgressBar);
                }

                LogViewerModel.Add("Splitting into tracks...");
                var waveFileCutter = new WaveFileCutter();
                var cuttedFiles = await waveFileCutter.CutWavFileFromMarkersFile(
                     baseFileName,
                     LogViewerModel.Add,
                     SetDetailProgressBarMaximum,
                     UpdateDetailProgressBar);

                var normalizer = new Normalizer();
                var waveToMp3Converter = new WaveToMp3Converter();
                foreach (var item in cuttedFiles)
                {
                    LogViewerModel.Add(string.Format("Normalizing segment {0}...", item));
                    await normalizer.Normalize(item, LogViewerModel.Add, SetDetailProgressBarMaximum, UpdateDetailProgressBar);

                    LogViewerModel.Add(string.Format("Converting segment {0} to MP3...", item));
                    await waveToMp3Converter.Convert(item, LogViewerModel.Add, SetDetailProgressBarMaximum, UpdateDetailProgressBar);

                    if (item != baseFileName)
                        File.Delete(item);
                }
            }
            catch (Exception ex)
            {
                LogViewerModel.Add(ex.Message);
            }
            finally
            {
                IsProcessing = false;
            }

            LogViewerModel.Add("Finished processing");
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

        private void ShowFileDialog()
        {
            var fileDialog = new OpenFileDialog
            {
                CheckFileExists = true,
                AddExtension = true,
                DefaultExt = ".wav",
                Multiselect = false,
                Filter = "Wave|*.wav|MP3|*.mp3",
                InitialDirectory = string.IsNullOrEmpty(MuVox.Properties.Settings.Default.RECORDER_LastFile)
                    ? null
                    : System.IO.Path.GetDirectoryName(MuVox.Properties.Settings.Default.RECORDER_LastFile)
            };

            var dlgResult = fileDialog.ShowDialog();

            if (dlgResult.HasValue && dlgResult.Value)
                FileName = fileDialog.FileName;
        }
        #endregion

        #region Events
        #endregion
    }
}
