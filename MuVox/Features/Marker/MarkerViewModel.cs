using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NAudio.Wave;
using TTech.Muvox.Features.Messages;

namespace TTech.Muvox.Features.Marker
{
    public class MarkerViewModel : GalaSoft.MvvmLight.ViewModelBase, IDisposable
    {
        #region Fields
        private readonly Marker marker;
        private readonly ObservableCollection<int> markers;
        private WaveOut? waveOut = null;
        #endregion

        public MarkerViewModel()
        {
            marker = new Marker();
            markers = new ObservableCollection<int>();
            markers.CollectionChanged += Markers_CollectionChanged;
        }

        #region Commands
        private RelayCommand? processCommand;
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

        private RelayCommand? playFromCurrentCommand;
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

                            var reader = new NAudio.Wave.WaveFileReader(FileName)
                            {
                                CurrentTime = TimeSpan.FromSeconds(SelectedPosition / 10.0)
                            };
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
        #endregion

        #region Properties
        public string FileName { get { return MuVox.Properties.Settings.Default.RECORDER_LastFile; } }

        public WaveStream WaveStream
        {
            get
            {
                markers.Clear();
                return new NAudio.Wave.WaveFileReader(FileName);
            }
        }

        public long SelectedPosition { get; set; }

        public ObservableCollection<int> Markers
        {
            get
            {
                markers.Clear();
                foreach (var mark in marker.GetMarkersFromFile(FileName))
                    markers.Add(mark);

                return markers;
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
        private void Markers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            marker.CreateFileFromList(FileName, Markers);
        }
        #endregion

        #region Public Methods
        #endregion

        #region Events
        private void WaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (waveOut != null)
            {
                waveOut.PlaybackStopped -= WaveOut_PlaybackStopped;
                waveOut.Dispose();
                waveOut = null;
            }
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue && disposing)
            {
                if (waveOut != null)
                    waveOut.Dispose();

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
