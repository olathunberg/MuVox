using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NAudio.Lame;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using RecordToMP3.Features.Messages;
using RecordToMP3.Features.Processor.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RecordToMP3.Features.Marker
{
    public class MarkerViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        #region Fields
        Marker marker = new Marker();
        ObservableCollection<int> markers;
        #endregion

        #region Constructors
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
        private WaveOut waveOut = null;
        private RelayCommand playFromCurrentCommand;
        public ICommand PlayFromCurrent
        {
            get
            {
                return playFromCurrentCommand ?? (playFromCurrentCommand = new RelayCommand(
                    () =>
                    {
                        if (waveOut == null)
                        {
                            waveOut = new WaveOut();

                            var reader = new NAudio.Wave.WaveFileReader(FileName);
                            reader.CurrentTime = TimeSpan.FromSeconds(SelectedPosition / 10.0);
                            waveOut.Init(reader);
                            waveOut.Play();
                            waveOut.PlaybackStopped += WaveOut_PlaybackStopped;
                        }
                        else
                        {
                            waveOut.Stop();
                            waveOut.Dispose();
                            waveOut = null;
                        }
                    },
                    () => true));
            }
        }

        private void WaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            waveOut.PlaybackStopped -= WaveOut_PlaybackStopped;
            waveOut.Dispose();
            waveOut = null;
        }
        #endregion

        #region Properties
        public string FileName { get { return Properties.Settings.Default.RECORDER_LastFile; } }

        public WaveStream WaveStream
        {
            get
            {
                if (markers != null)
                {
                    markers.CollectionChanged -= markers_CollectionChanged;
                    markers = null;
                }
                return new NAudio.Wave.WaveFileReader(FileName);
            }
        }

        public long SelectedPosition { get; set; }

        public ObservableCollection<int> Markers
        {
            get
            {
                if (markers == null)
                {
                    markers = new ObservableCollection<int>(marker.GetMarkersFromFile(FileName));
                    markers.CollectionChanged += markers_CollectionChanged;
                }
                return markers;
            }
        }

        void markers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            marker.CreateFileFromList(FileName, Markers);
        }
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
