using NAudio.Wave;
using NPSMLib;
using System.Threading.Tasks;

namespace MuVox.Metering
{
    public class MediaPlaybackInfo : GalaSoft.MvvmLight.ObservableObject
    {
        private NowPlayingSessionManager? npsm;

        public string? FadeText { get; set; }

        public string? Artist { get; set; }

        public string? Track { get; set; }

        public System.IO.Stream? Image { get; set; }

        internal async Task Initialize()
        {
            FadeText = GetFadeText(new WasapiOut());

            await Task.Run(() =>
            {
                npsm = new NowPlayingSessionManager();
            });

            if(npsm is null)
            {
                return;
            }

            var playBackDataSource = npsm.CurrentSession.ActivateMediaPlaybackDataSource();
            playBackDataSource.MediaPlaybackDataChanged += MainViewModel_MediaPlaybackDataChanged;
            UpdateMediaPlaybackData(playBackDataSource);
        }

        internal void Play()
        {
            npsm?.CurrentSession.ActivateMediaPlaybackDataSource().SendMediaPlaybackCommand(MediaPlaybackCommands.Play);
        }

        internal void Pause()
        {
            npsm?.CurrentSession.ActivateMediaPlaybackDataSource().SendMediaPlaybackCommand(MediaPlaybackCommands.Pause);
        }

        internal void UpdateFadeText(WasapiOut wasapiOut)
        {
            FadeText = GetFadeText(wasapiOut);
            RaisePropertyChanged(() => FadeText);
        }

        private void MainViewModel_MediaPlaybackDataChanged(object sender, MediaPlaybackDataChangedArgs e)
        {
            UpdateMediaPlaybackData(e.MediaPlaybackDataSource);
        }

        private void UpdateMediaPlaybackData(MediaPlaybackDataSource playBackDataSource)
        {
            var playInfo = playBackDataSource.GetMediaObjectInfo();
            Artist = playInfo.Artist;
            Track = playInfo.Title;
            Image = playBackDataSource.GetThumbnailStream();

            RaisePropertyChanged();
        }

        private void RaisePropertyChanged()
        {
            RaisePropertyChanged(() => Artist);
            RaisePropertyChanged(() => Track);
            RaisePropertyChanged(() => Image);
        }

        private string GetFadeText(WasapiOut waveOut)
        {
            return waveOut.Volume < 0.05f ? "Fade up" : "Fade down";
        }
    }
}
