using GalaSoft.MvvmLight.CommandWpf;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RecordToMP3.Features.Recorder
{
    class RecorderViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        #region Fields
        private RecorderView owner;
        private float rightAmplitude = 0f;
        private float leftAmplitude = 0f;
        private int secondsRecorded = 0;
        private string timeRecorded;
        private Queue<float> amplitudesL = new Queue<float>();
        private Queue<float> amplitudesR = new Queue<float>();
        private Recorder recorder;
        #endregion

        #region Constructors
        public RecorderViewModel()
        {
            recorder = new Recorder();
            recorder.NewSample = RecorderNewSamlpe;
            SecondsRecorded = 0;
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
                
                        SecondsRecorded = 0;
                      
                        RaisePropertyChanged(() => StartButtonText);
                    },
                    () => recorder.RecordingState == RecordingState.Recording || recorder.RecordingState == RecordingState.Paused));
            }
        }
        #endregion

        #region Properties
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

        public int SecondsRecorded
        {
            get { return secondsRecorded; }
            set
            {
                secondsRecorded = value;
                TimeRecorded = string.Format("{0:00}:{1:00}", secondsRecorded / 60, secondsRecorded % 60);
                RaisePropertyChanged();
            }
        }

        public string TimeRecorded
        {
            get { return timeRecorded; }
            set
            {
                timeRecorded = value;
                RaisePropertyChanged();
            }
        }

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
        private void RecorderNewSamlpe(float minL, float maxL, float minR, float maxR)
        {
            SecondsRecorded = recorder.GetSecondsRecorded();

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
