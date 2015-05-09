using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using RecordToMP3.Features.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Windows.Input;

namespace RecordToMP3.Features.Processor
{
    public class ProcessorViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        #region Fields
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

        #endregion

        #region Properties
        public uint ProgressBarMaximum { get; set; }
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
