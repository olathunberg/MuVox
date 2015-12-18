using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IMAPI2.Interop;
using IMAPI2.MediaItem;

namespace RecordToMP3.Features.DiscBurner
{
    internal class AudioBurnTask
    {
        private bool ejectMedia;
        private BurnData burnData;
        private IProgress<BurnData> burnProgress;
        private IList<MediaFile> mediaItems;
        private CancellationToken cancellationToken;

        public bool IsBurning { get; private set; }

        private async Task<int> StartBurnProcess(IDiscRecorder2 discRecorder, bool ejectMedia,
            IList<MediaFile> mediaItems, IMAPI_BURN_VERIFICATION_LEVEL verificationLevel,
            CancellationToken cancellationToken, IProgress<BurnData> progress)
        {
            IsBurning = true;
            this.ejectMedia = ejectMedia;
            this.burnProgress = progress;
            this.mediaItems = mediaItems;
            this.cancellationToken = cancellationToken;

            burnData = new BurnData();
            burnData.uniqueRecorderId = discRecorder.ActiveDiscRecorder;

            var burnResult = await Task.Run(() => DoBurn(burnData.uniqueRecorderId));

            progress.Report(burnData);

            IsBurning = false;

            return burnResult;
        }

        private int DoBurn(string activeDiscRecorder)
        {
            int result = 0;

            var discMaster = new MsftDiscMaster2();
            var discRecorder2 = new MsftDiscRecorder2();

            discRecorder2.InitializeDiscRecorder(activeDiscRecorder);

            var trackAtOnce = new MsftDiscFormat2TrackAtOnce();
            trackAtOnce.ClientName = "";
            trackAtOnce.Recorder = discRecorder2;
            burnData.totalTracks = mediaItems.Count;
            burnData.currentTrackNumber = 0;

            // Prepare the wave file streams
            foreach (MediaFile mediaFile in mediaItems)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                // Report back to the UI that we're preparing stream
                burnData.task = BURN_MEDIA_TASK.BURN_MEDIA_TASK_PREPARING;
                burnData.filename = mediaFile.ToString();
                burnData.currentTrackNumber++;

                burnProgress.Report(burnData);
                mediaFile.PrepareStream();
            }

            trackAtOnce.Update += trackAtOnce_Update;

            trackAtOnce.PrepareMedia();

            foreach (MediaFile mediaFile in mediaItems)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    result = -1;
                    break;
                }

                burnData.filename = mediaFile.ToString();
                var stream = mediaFile.GetTrackIStream();
                trackAtOnce.AddAudioTrack(stream);
            }

            trackAtOnce.Update -= trackAtOnce_Update;

            trackAtOnce.ReleaseMedia();

            if (ejectMedia)
                discRecorder2.EjectMedia();

            return result;
        }

        void trackAtOnce_Update(object sender, object progress)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                var trackAtOnce = (IDiscFormat2TrackAtOnce)sender;
                trackAtOnce.CancelAddTrack();
                return;
            }

            var eventArgs = (IDiscFormat2TrackAtOnceEventArgs)progress;

            burnData.task = BURN_MEDIA_TASK.BURN_MEDIA_TASK_WRITING;

            // IDiscFormat2TrackAtOnceEventArgs Interface
            burnData.currentTrackNumber = eventArgs.CurrentTrackNumber;
            burnData.elapsedTime = eventArgs.ElapsedTime;
            burnData.remainingTime = eventArgs.RemainingTime;

            // IWriteEngine2EventArgs Interface
            burnData.currentTaoAction = eventArgs.CurrentAction;
            burnData.startLba = eventArgs.StartLba;
            burnData.sectorCount = eventArgs.SectorCount;
            burnData.lastReadLba = eventArgs.LastReadLba;
            burnData.lastWrittenLba = eventArgs.LastWrittenLba;
            burnData.totalSystemBuffer = eventArgs.TotalSystemBuffer;
            burnData.usedSystemBuffer = eventArgs.UsedSystemBuffer;
            burnData.freeSystemBuffer = eventArgs.FreeSystemBuffer;

            burnProgress.Report(burnData);
        }
    }
}
