using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using TTech.Muvox.Features.Messages;

namespace TTech.Muvox.Features.Recorder
{
    public class RecorderViewModel : GalaSoft.MvvmLight.ViewModelBase, IDisposable
    {
        #region Fields
        private float rightAmplitude;
        private float leftAmplitude;
        private Recorder recorder;

        private Settings.Settings Settings { get { return Features.Settings.SettingsBase<Settings.Settings>.Current; } }

        // Move to volumemeter
        private Queue<float> amplitudesL = new Queue<float>();
        private Queue<float> amplitudesR = new Queue<float>();
        #endregion

        #region Constructors
        public RecorderViewModel()
        {
            recorder = new Recorder();
            recorder.NewSample = RecorderNewSample;
            ProgressBarMaximum = Settings.Recorder_MinutesOnProgressbar * 600;
            Messenger.Default.Register<SetMarkerMessage>(
               this, (action) =>
               {
                   if (SetMarker.CanExecute(null))
                       SetMarker.Execute(null);
               });
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
                    recorder.SetMarker,
                    () => recorder.RecordingState == RecordingState.Recording || recorder.RecordingState == RecordingState.Paused));
            }
        }

        private RelayCommand processCommand;
        private (float, float) newLeftPoint;
        private (float, float) newRightPoint;
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

        public (float, float) NewLeftPoint
        {
            get { return newLeftPoint; }
            set { newLeftPoint = value; RaisePropertyChanged(); }
        }

        public (float, float) NewRightPoint
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
            NewRightPoint = (maxR, minR);
            NewLeftPoint = (maxL, minL);

            amplitudesL.Enqueue(maxL);
            if (amplitudesL.Count > Settings.UX_VolumeMeter_NoSamples)
                amplitudesL.Dequeue();
            LeftAmplitude = amplitudesL.Sum() / amplitudesL.Count;

            amplitudesR.Enqueue(maxL);
            if (amplitudesR.Count > Settings.UX_VolumeMeter_NoSamples)
                amplitudesR.Dequeue();
            RightAmplitude = amplitudesR.Sum() / amplitudesR.Count;
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (recorder != null)
                        recorder.Dispose();
                }

                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
