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

namespace RecordToMP3.Features.Marker
{
    public class MarkerViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        #region Fields
        private string fileName;
        #endregion

        #region Constructors
        public MarkerViewModel()
        {
        }
        #endregion

        #region Commands
        private RelayCommand processCommand;
        public ICommand Process
        {
            get
            {
                return processCommand ?? (processCommand = new RelayCommand(
                    () =>
                    {
                        Messenger.Default.Send<GotoPageMessage>(new GotoPageMessage(Pages.Processor));
                    },
                    () => true));
            }
        }
        #endregion

        #region Properties
        public string FileName { get { return Properties.Settings.Default.RECORDER_LastFile; } }

        public WaveStream WaveStream { get { return new NAudio.Wave.WaveFileReader(FileName); } }
        #endregion

        #region Overrides
        public override void Cleanup()
        {
            base.Cleanup();
        }
        #endregion

        #region Private methods
        #endregion

        #region Public Methods
        #endregion

        #region Events
        #endregion
    }
}
