using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NAudio.Wave;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TTech.MuVox.Core;
using TTech.MuVox.Features.Messages;

namespace TTech.MuVox.Features.Editor
{
    public class EditorViewModel : GalaSoft.MvvmLight.ViewModelBase, IDisposable
    {
        private readonly ObservableCollection<Core.Marker> markers;
        private WaveOut? waveOut = null;

        public EditorViewModel()
        {
            var observableCollections = new ObservableCollection<Core.Marker>();
            markers = observableCollections;
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
                        Messenger.Default.Send(new GotoPageMessage(Pages.Processor));
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

                            var reader = new WaveFileReader(FileName)
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

        public string FileName { get { return Settings.Settings.Current.Recorder_LastFile; } }

        public WaveStream WaveStream
        {
            get
            {
                markers.CollectionChanged -= Markers_CollectionChanged;
                markers.Clear();
                markers.CollectionChanged += Markers_CollectionChanged;
                return new WaveFileReader(FileName);
            }
        }

        public long SelectedPosition { get; set; }

        public ObservableCollection<Core.Marker> Markers
        {
            get
            {
                markers.CollectionChanged -= Markers_CollectionChanged;
                markers.Clear();
                foreach (var mark in MarkersHelper.GetMarkersFromFile(FileName))
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
            MarkersHelper.CreateFileFromList(FileName, markers);
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
