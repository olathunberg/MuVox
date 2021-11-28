using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using NAudio.Wave;
using TTech.MuVox.Features.Messages;
using TTech.MuVox.Features.Processor.Converters;
using TTech.MuVox.Features.Processor.Tools;

namespace TTech.MuVox.Features.Processor
{
    public class ProcessorViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private bool isProcessing;
        private long progressBarMaximum;
        private long progress;
        private long totalProgress;
        private long totalProgressMaximum;

        public ProcessorViewModel()
        {
            LogViewerModel = new LogViewer.LogViewerModel();
        }

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
                        Messenger.Default.Send<GotoPageMessage>(new GotoPageMessage(Pages.Editor));
                    },
                    () => !IsProcessing && !string.IsNullOrEmpty(FileName)));
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
                    () => !IsProcessing && !string.IsNullOrEmpty(FileName)));
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
            get { return Settings.Settings.Current.Recorder_LastFile; }
            set
            {
                if (value == null)
                    return;

                Settings.Settings.Current.Recorder_LastFile = value;
                Settings.Settings.Save();
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

        public override void Cleanup()
        {
            base.Cleanup();
        }

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
                var progressMaximum = new Progress<long>(SetDetailProgressBarMaximum);
                var progress = new Progress<long>(UpdateDetailProgressBar);
                var processor = new Processor(LogViewerModel, progressMaximum, progress);

                TotalProgress = 0;

                IsProcessing = true;
                var baseFileName = FileName;

                var preConvert = Path.GetExtension(baseFileName) == ".mp3";
                if (preConvert)
                {
                    using var reader = new Mp3FileReader(baseFileName);
                    TotalProgressMaximum = reader.Length * 5;
                }
                else
                {
                    using var reader = new WaveFileReader(baseFileName);
                    TotalProgressMaximum = reader.Length * 4;
                }

                if (preConvert)
                {
                    LogViewerModel.Add("Converting input MP3 to wave...");

                    var mp3ToWave = new Mp3ToWaveConverter();
                    baseFileName = await mp3ToWave.Convert(baseFileName, LogViewerModel.Add, progressMaximum, progress);
                }

                await processor.Process(baseFileName);
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
                InitialDirectory = string.IsNullOrEmpty(Settings.Settings.Current.Recorder_LastFile)
                    ? null
                    : Path.GetDirectoryName(Settings.Settings.Current.Recorder_LastFile)
            };

            var dlgResult = fileDialog.ShowDialog();

            if (dlgResult.HasValue && dlgResult.Value)
                FileName = fileDialog.FileName;
        }

    }
}
