using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Windows.Input;

namespace RecordToMP3.Features.Recorder
{
    class RecorderViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        #region Fields
        private RecorderView owner;
        private float rightAmplitude = 0f;
        private float leftAmplitude = 0f;
        private Recorder recorder;

        // Move to volumemeter
        private Queue<float> amplitudesL = new Queue<float>();
        private Queue<float> amplitudesR = new Queue<float>();
        #endregion

        #region Constructors
        public RecorderViewModel()
        {
            recorder = new Recorder();
            recorder.NewSample = RecorderNewSample;
            ProgressBarMaximum = Convert.ToUInt32(ConfigurationManager.AppSettings["UI_MinutesOnProgressBar"]) * 600;
        }
        #endregion

        #region Commands
        private RelayCommand startRecordingCommand;
        public ICommand StartRecording
        {
            get
            {
                return startRecordingCommand ?? (startRecordingCommand = new RelayCommand(
                    () =>
                    {
                        if (recorder != null)
                            recorder.StartRecording();
                      
                        RaisePropertyChanged(() => StartButtonText);
                    },
                    () => true));
            }
        }

        private RelayCommand stopRecordingCommand;
        public ICommand StopRecording
        {
            get
            {
                return stopRecordingCommand ?? (stopRecordingCommand = new RelayCommand(
                    () =>
                    {
                        recorder.StopRecording();
                    
                        RaisePropertyChanged(() => StartButtonText);
                    },
                    () => recorder.RecordingState == RecordingState.Recording || recorder.RecordingState == RecordingState.Paused));
            }
        }

        private RelayCommand setMarkerCommand;
        public ICommand SetMarker
        {
            get
            {
                return setMarkerCommand ?? (setMarkerCommand = new RelayCommand(
                    () =>
                    {
                        recorder.SetMarker();
                    },
                    () => recorder.RecordingState == RecordingState.Recording || recorder.RecordingState == RecordingState.Paused));
            }
        }

        #endregion

        #region Properties
        public Recorder Recorder { get { return recorder; } }

        public string StartButtonText
        {
            get
            {
                if (recorder.RecordingState == RecordingState.Recording)
                    return "PAUSE (F1)";
                if (recorder.RecordingState == RecordingState.Paused)
                    return "RESUME (F1)";
                else
                    return "START (F1)";
            }
        }

        public float RightAmplitude
        {
            get { return rightAmplitude; }
            set { rightAmplitude = value; RaisePropertyChanged(); }
        }

        public float LeftAmplitude
        {
            get { return leftAmplitude; }
            set { leftAmplitude = value; RaisePropertyChanged(); }
        }

        public uint ProgressBarMaximum { get; set; }

        public RecorderView Owner
        {
            get { return owner; }
            set
            {
                if (owner == value)
                    return;

                owner = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Overrides
        public override void Cleanup()
        {
            if (recorder != null)
                recorder.Cleanup();
            base.Cleanup();
        }
        #endregion

        #region Events
        private void RecorderNewSample(float minL, float maxL, float minR, float maxR)
        {
            if (owner != null && owner.WaveFormViewerLeft != null)
                owner.WaveFormViewerLeft.AddValue(maxL, minL);
            if (owner != null && owner.WaveFormViewerRight != null)
                owner.WaveFormViewerRight.AddValue(maxR, minR);

            amplitudesL.Enqueue(maxL);
            LeftAmplitude = amplitudesL.Sum() / amplitudesL.Count;
            if (amplitudesL.Count > 3) amplitudesL.Dequeue();

            amplitudesR.Enqueue(maxL);
            RightAmplitude = amplitudesR.Sum() / amplitudesR.Count;
            if (amplitudesR.Count > 3) amplitudesR.Dequeue();
        }
        #endregion
    }
}
