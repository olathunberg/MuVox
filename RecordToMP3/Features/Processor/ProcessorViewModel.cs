using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using RecordToMP3.Features.Messages;
using RecordToMP3.Features.Processor.Effects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RecordToMP3.Features.Processor
{
    public class ProcessorViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        #region Fields
        private string progressText;
        #endregion

        #region Constructors
        public ProcessorViewModel()
        {
            ProgressBarMaximum = Properties.Settings.Default.UI_MinutesOnProgressBar * 600;
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
                    () =>
                    {
                        Task.Run(async () =>
                            {
                                ProgressText = "Started processing";
                                ProgressText += Environment.NewLine + "Running compressor...";

                                using (var reader = new WaveFileReader(FileName))
                                {
                                    var sampleReader = new Pcm16BitToSampleProvider(reader);
                                    var compressor = new Effects.FastAttackCompressor1175(sampleReader);
                                    WaveFileWriter.CreateWaveFile16(FileName + ".compress", compressor);
                                }

                                ProgressText += Environment.NewLine + "Splitting into tracks...";
                                await WaveFileCutter.CutWavFileFromMarkersFile(
                                    Path.ChangeExtension(FileName, ".markers"),
                                    FileName,
                                    message => ProgressText += Environment.NewLine + message);

                                ProgressText += Environment.NewLine + "Running normalizer on segments...";

                                // Skapa en klass som normaliserar en fil och kör den på alla filer som ovan splitter returnerat
                                await Task.Delay(1000);

                                ProgressText += Environment.NewLine + "Finished processing";
                            });
                    },
                    () => true));
            }
        }
        #endregion



        #region Properties
        public uint ProgressBarMaximum { get; set; }

        public string FileName { get { return Properties.Settings.Default.RECORDER_LastFile; } }

        public string ProgressText
        {
            get { return progressText; }
            set
            {
                progressText = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Overrides
        public override void Cleanup()
        {
            base.Cleanup();
        }
        #endregion

        #region Events
        #endregion
    }
}
