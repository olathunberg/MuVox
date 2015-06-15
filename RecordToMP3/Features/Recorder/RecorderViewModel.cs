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

namespace RecordToMP3.Features.Recorder
{
    public class RecorderViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        #region Fields
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
            ProgressBarMaximum = Properties.Settings.Default.UI_MinutesOnProgressBar * 600;
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

        private RelayCommand processCommand;
        private Tuple<float, float> newLeftPoint;
        private Tuple<float, float> newRightPoint;
        public ICommand Process
        {
            get
            {
                return processCommand ?? (processCommand = new RelayCommand(
                    () =>
                    {
                        Messenger.Default.Send<GotoPageMessage>(new GotoPageMessage(Pages.Processor));
                    },
                    () => recorder.RecordingState != RecordingState.Recording || recorder.RecordingState != RecordingState.Paused));
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

        public Tuple<float, float> NewLeftPoint
        {
            get { return newLeftPoint; }
            set { newLeftPoint = value; RaisePropertyChanged(); }
        }

        public Tuple<float, float> NewRightPoint
        {
            get { return newRightPoint; }
            set { newRightPoint = value; RaisePropertyChanged(); }
        }
        
        public uint ProgressBarMaximum { get; set; }
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
            NewRightPoint = new Tuple<float, float>(maxR, minR);
            NewLeftPoint = new Tuple<float, float>(maxL, minL);
            
            amplitudesL.Enqueue(maxL);
            if (amplitudesL.Count > Properties.Settings.Default.UI_LEVELMETER_NO_SAMPLES) amplitudesL.Dequeue();
            LeftAmplitude = amplitudesL.Sum() / amplitudesL.Count;

            amplitudesR.Enqueue(maxL);
            if (amplitudesR.Count > Properties.Settings.Default.UI_LEVELMETER_NO_SAMPLES) amplitudesR.Dequeue();
            RightAmplitude = amplitudesR.Sum() / amplitudesR.Count;
        }
        #endregion
    }
}
