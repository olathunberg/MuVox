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
        private string progressText;
        private LogViewer.LogViewerModel logViewerModel;
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
                    () => true));
            }
        }

        private RelayCommand startProcessingCommand;
        public ICommand StartProcessing
        {
            get
            {
                return startProcessingCommand ?? (startProcessingCommand = new RelayCommand(
                   async () => await ProcessFile(),
                    () => true));
            }
        }
        #endregion

        #region Properties
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
          
            LogViewerModel.Add("Splitting into tracks...");
            var waveFileCutter = new WaveFileCutter();
            var cuttedFiles = await waveFileCutter.CutWavFileFromMarkersFile(
                 Path.ChangeExtension(FileName, ".markers"),
                 FileName,
                 message => LogViewerModel.Add(message));

            var normalizer = new Normalizer();
            foreach (var item in cuttedFiles)
            {
               LogViewerModel.Add( string.Format("Processing segment {0}...", item));
                await normalizer.Normalize(item, message => LogViewerModel.Add(message));

                // Create MP3
                using (var reader = new WaveFileReader(item))
                using (var wtr = new LameMP3FileWriter(Path.ChangeExtension(item, ".mp3"), reader.WaveFormat, Properties.Settings.Default.PROCESSOR_MP3Quality))
                    reader.CopyTo(wtr);

                File.Delete(item);
             }

            LogViewerModel.Add("Finished processing");
        }
        #endregion

        #region Events
        #endregion
    }
}
