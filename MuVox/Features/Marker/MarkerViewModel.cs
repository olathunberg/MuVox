using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NAudio.Wave;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TTech.Muvox.Features.Messages;

namespace TTech.Muvox.Features.Marker
{
    public class MarkerViewModel : GalaSoft.MvvmLight.ViewModelBase, IDisposable
    {
        private readonly ObservableCollection<int> markers;
        private WaveOut? waveOut = null;

        public MarkerViewModel()
        {
            markers = new ObservableCollection<int>();
            markers.CollectionChanged += Markers_CollectionChanged;
        }

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

                        RaisePropertyChanged(() => PlayButtonText);
                    },
                    () => true));
            }
        }

        public string PlayButtonText
        {
            get
            {
                if (waveOut == null)
                    return "PLAY (Space)";
                else
                    return "STOP (Space)";
            }
        }

        public string FileName { get { return MuVox.Properties.Settings.Default.RECORDER_LastFile; } }

        public WaveStream WaveStream
        {
            get
            {
                markers.CollectionChanged -= Markers_CollectionChanged;
                markers.Clear();
                markers.CollectionChanged += Markers_CollectionChanged;
                return new NAudio.Wave.WaveFileReader(FileName);
            }
        }

        public long SelectedPosition { get; set; }

        public ObservableCollection<int> Markers
        {
            get
            {
                markers.CollectionChanged -= Markers_CollectionChanged;
                markers.Clear();
                foreach (var mark in MarkerHelper.GetMarkersFromFile(FileName))
                    markers.Add(mark);
                markers.CollectionChanged += Markers_CollectionChanged;

                return markers;
            }
        }

        public override void Cleanup()
        {
            base.Cleanup();
        }

        private void Markers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            MarkerHelper.CreateFileFromList(FileName, markers);
        }

        private void WaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (waveOut != null)
            {
                waveOut.PlaybackStopped -= WaveOut_PlaybackStopped;
                waveOut.Dispose();
                waveOut = null;
                RaisePropertyChanged(() => PlayButtonText);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue && disposing)
            {
                if (waveOut != null)
                    waveOut.Dispose();

                if (markers != null)
                    markers.CollectionChanged -= Markers_CollectionChanged;


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
