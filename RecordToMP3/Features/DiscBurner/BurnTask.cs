using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using IMAPI2.Interop;
using IMAPI2.MediaItem;

namespace RecordToMP3.Features.DiscBurner
{
    internal class BurnTask
    {
        private bool closeMedia;
        private bool ejectMedia;
        private BurnData burnData;
        private IProgress<Tuple<int, BurnData>> burnProgress;
        private IList<IMediaItem> mediaItems;
        private CancellationToken cancellationToken;

        public bool IsBurning { get; private set; }

        private async Task<int> StartBurnProcess(IDiscRecorder2 discRecorder, bool closeMedia, bool ejectMedia,
            IList<IMediaItem> mediaItems, IMAPI_BURN_VERIFICATION_LEVEL verificationLevel,
            CancellationToken cancellationToken, IProgress<Tuple<int, BurnData>> progress)
        {
            IsBurning = true;
            this.closeMedia = closeMedia;
            this.ejectMedia = ejectMedia;
            this.burnProgress = progress;
            this.mediaItems = mediaItems;
            this.cancellationToken = cancellationToken;

            burnData.uniqueRecorderId = discRecorder.ActiveDiscRecorder;

            var burnResult = await Task.Run(() => DoBurn(burnData.uniqueRecorderId, verificationLevel));

            progress.Report(new Tuple<int, BurnData>(0, burnData));

            IsBurning = false;

            return burnResult;
        }

        private int DoBurn(string activeDiscRecorder, IMAPI_BURN_VERIFICATION_LEVEL verificationLevel)
        {
            MsftDiscRecorder2 discRecorder = null;
            MsftDiscFormat2Data discFormatData = null;
            int result = 0;

            try
            {
                discRecorder = new MsftDiscRecorder2();
                discRecorder.InitializeDiscRecorder(burnData.uniqueRecorderId);

                discFormatData = new MsftDiscFormat2Data
                {
                    Recorder = discRecorder,
                    ClientName = "ClientName",
                    ForceMediaToBeClosed = closeMedia
                };

                var burnVerification = (IBurnVerification)discFormatData;
                burnVerification.BurnVerificationLevel = verificationLevel;

                object[] multisessionInterfaces = null;
                if (!discFormatData.MediaHeuristicallyBlank)
                    multisessionInterfaces = discFormatData.MultisessionInterfaces;

                IStream fileSystem;
                if (!CreateMediaFileSystem(discRecorder, multisessionInterfaces, out fileSystem))
                    return -1;

                discFormatData.Update += discFormatData_Update;

                try
                {
                    discFormatData.Write(fileSystem);
                    result = 0;
                }
                catch (COMException ex)
                {
                    result = ex.ErrorCode;
                    MessageBox.Show(ex.Message, "IDiscFormat2Data.Write failed",
                        MessageBoxButton.OK, MessageBoxImage.Stop);
                }
                finally
                {
                    if (fileSystem != null)
                        Marshal.FinalReleaseComObject(fileSystem);
                }

                discFormatData.Update -= discFormatData_Update;

                if (ejectMedia)
                    discRecorder.EjectMedia();
            }
            catch (COMException exception)
            {
                MessageBox.Show(exception.Message);
                result = exception.ErrorCode;
            }
            finally
            {
                if (discRecorder != null)
                    Marshal.ReleaseComObject(discRecorder);

                if (discFormatData != null)
                    Marshal.ReleaseComObject(discFormatData);
            }

            return result;
        }

        private bool CreateMediaFileSystem(IDiscRecorder2 discRecorder, object[] multisessionInterfaces,
                    out IStream dataStream)
        {
            MsftFileSystemImage fileSystemImage = null;
            try
            {
                fileSystemImage = new MsftFileSystemImage();
                fileSystemImage.ChooseImageDefaults(discRecorder);
                fileSystemImage.FileSystemsToCreate = FsiFileSystems.FsiFileSystemJoliet | FsiFileSystems.FsiFileSystemISO9660;
                fileSystemImage.VolumeName = "";

                fileSystemImage.Update += fileSystemImage_Update;

                if (multisessionInterfaces != null)
                {
                    fileSystemImage.MultisessionInterfaces = multisessionInterfaces;
                    fileSystemImage.ImportFileSystem();
                }

                var rootItem = fileSystemImage.Root;

                foreach (var mediaItem in mediaItems)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    mediaItem.AddToFileSystem(rootItem);
                }

                fileSystemImage.Update -= fileSystemImage_Update;

                if (cancellationToken.IsCancellationRequested)
                {
                    dataStream = null;
                    return false;
                }

                dataStream = fileSystemImage.CreateResultImage().ImageStream;
            }
            catch (COMException exception)
            {
                MessageBox.Show(exception.Message, "Create File System Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                dataStream = null;
                return false;
            }
            finally
            {
                if (fileSystemImage != null)
                    Marshal.ReleaseComObject(fileSystemImage);
            }

            return true;
        }

        void discFormatData_Update([In, MarshalAs(UnmanagedType.IDispatch)] object sender, [In, MarshalAs(UnmanagedType.IDispatch)] object progress)
        {
            // Check if we've cancelled
            if (cancellationToken.IsCancellationRequested)
            {
                var format2Data = (IDiscFormat2Data)sender;
                format2Data.CancelWrite();
                return;
            }

            var eventArgs = (IDiscFormat2DataEventArgs)progress;

            burnData.task = BURN_MEDIA_TASK.BURN_MEDIA_TASK_WRITING;

            burnData.elapsedTime = eventArgs.ElapsedTime;
            burnData.remainingTime = eventArgs.RemainingTime;
            burnData.totalTime = eventArgs.TotalTime;

            burnData.currentAction = eventArgs.CurrentAction;
            burnData.startLba = eventArgs.StartLba;
            burnData.sectorCount = eventArgs.SectorCount;
            burnData.lastReadLba = eventArgs.LastReadLba;
            burnData.lastWrittenLba = eventArgs.LastWrittenLba;
            burnData.totalSystemBuffer = eventArgs.TotalSystemBuffer;
            burnData.usedSystemBuffer = eventArgs.UsedSystemBuffer;
            burnData.freeSystemBuffer = eventArgs.FreeSystemBuffer;

            burnProgress.Report(new Tuple<int, BurnData>(0, burnData));
        }

        void fileSystemImage_Update([In, MarshalAs(UnmanagedType.IDispatch)] object sender,
            [In, MarshalAs(UnmanagedType.BStr)]string currentFile, [In] int copiedSectors, [In] int totalSectors)
        {
            var percentProgress = 0;
            if (copiedSectors > 0 && totalSectors > 0)
            {
                percentProgress = (copiedSectors * 100) / totalSectors;
            }

            if (!string.IsNullOrEmpty(currentFile))
            {
                var fileInfo = new FileInfo(currentFile);
                burnData.statusMessage = "Adding \"" + fileInfo.Name + "\" to image...";

                burnData.task = BURN_MEDIA_TASK.BURN_MEDIA_TASK_FILE_SYSTEM;
                burnProgress.Report(new Tuple<int, BurnData>(percentProgress, burnData));
            }
        }
    }
}
