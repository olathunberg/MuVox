using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using IMAPI2.Interop;
using IMAPI2.MediaItem;

namespace RecordToMP3.Features.DiscBurner
{
    // http://www.codeproject.com/Articles/24544/Burning-and-Erasing-CD-DVD-Blu-ray-Media-with-C-an
    // http://www.codeproject.com/Articles/25241/Creating-Audio-CDs-using-IMAPI
    public class DiscBurner
    {
        private Int64 totalDiscSize;

        public string labelMediaTypeText { get; private set; }

        public string labelTotalSizeText { get; private set; }

        public int progressBarCapacityValue { get; private set; }

        private MsftDiscRecorder2 discRecorder { get; set; }

        private List<IMediaItem> listBoxFilesItems { get; set; }

        public List<MsftDiscRecorder2> GetRecordingDevices()
        {
            var result = new List<MsftDiscRecorder2>();

            MsftDiscMaster2 discMaster = null;
            try
            {
                discMaster = new MsftDiscMaster2();

                if (!discMaster.IsSupportedEnvironment)
                    return result;
                foreach (string uniqueRecorderId in discMaster)
                {
                    var discRecorder2 = new MsftDiscRecorder2();
                    discRecorder2.InitializeDiscRecorder(uniqueRecorderId);

                    result.Add(discRecorder2);
                }
            }
            catch (COMException ex)
            {
                MessageBox.Show(ex.Message,
                    string.Format("Error:{0} - Please install IMAPI2", ex.ErrorCode),
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return result;
            }
            finally
            {
                if (discMaster != null)
                    Marshal.ReleaseComObject(discMaster);
            }

            return result;
        }

        private void Cleanup()
        {
            if (discRecorder != null)
                Marshal.ReleaseComObject(discRecorder);
        }

        private void DetectMedia()
        {
            MsftFileSystemImage fileSystemImage = null;
            MsftDiscFormat2Data discFormatData = null;

            try
            {
                discFormatData = new MsftDiscFormat2Data();
                if (!discFormatData.IsCurrentMediaSupported(discRecorder))
                {
                    labelMediaTypeText = "Media not supported!";
                    totalDiscSize = 0;
                    return;
                }
                else
                {
                    // Get the media type in the recorder
                    discFormatData.Recorder = discRecorder;
                    IMAPI_MEDIA_PHYSICAL_TYPE mediaType = discFormatData.CurrentPhysicalMediaType;
                    labelMediaTypeText = StringProvider.GetMediaTypeString(mediaType);

                    fileSystemImage = new MsftFileSystemImage();
                    fileSystemImage.ChooseImageDefaultsForMediaType(mediaType);

                    // See if there are other recorded sessions on the disc
                    if (!discFormatData.MediaHeuristicallyBlank)
                    {
                        fileSystemImage.MultisessionInterfaces = discFormatData.MultisessionInterfaces;
                        fileSystemImage.ImportFileSystem();
                    }

                    Int64 freeMediaBlocks = fileSystemImage.FreeMediaBlocks;
                    totalDiscSize = 2048 * freeMediaBlocks;
                }
            }
            catch (COMException exception)
            {
                MessageBox.Show(exception.Message, "Detect Media Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (discFormatData != null)
                    Marshal.ReleaseComObject(discFormatData);

                if (fileSystemImage != null)
                    Marshal.ReleaseComObject(fileSystemImage);
            }

            UpdateCapacity();
        }

        private string GetDisplayString(IDiscRecorder2 discRecorder2)
        {
            var devicePaths = string.Empty;
            var volumePath = (string)discRecorder2.VolumePathNames.GetValue(0);
            foreach (string volPath in discRecorder2.VolumePathNames)
            {
                if (!string.IsNullOrEmpty(devicePaths))
                    devicePaths += ",";

                devicePaths += volumePath;
            }

            return string.Format("{0} [{1}]", devicePaths, discRecorder2.ProductId);
        }

        private string GetSupportedMediaTypes()
        {
            if (discRecorder == null)
                return null;

            // Verify recorder is supported
            IDiscFormat2Data discFormatData = null;
            try
            {
                discFormatData = new MsftDiscFormat2Data();
                if (!discFormatData.IsRecorderSupported(discRecorder))
                    return "Recorder not supported";

                var supportedMediaTypes = new StringBuilder();
                foreach (IMAPI_PROFILE_TYPE profileType in discRecorder.SupportedProfiles)
                {
                    string profileName = StringProvider.GetProfileTypeString(profileType);

                    if (string.IsNullOrEmpty(profileName))
                        continue;

                    if (supportedMediaTypes.Length > 0)
                        supportedMediaTypes.Append(", ");
                    supportedMediaTypes.Append(profileName);
                }

                return supportedMediaTypes.ToString();
            }
            catch (COMException)
            {
                return "Error getting supported types";
            }
            finally
            {
                if (discFormatData != null)
                    Marshal.ReleaseComObject(discFormatData);
            }
        }

        private string GetVolumeLabel()
        {
            var now = DateTime.Now;
            return string.Format("{0}_{1}_{2}", now.Year, now.Month, now.Day);
        }

        private void UpdateCapacity()
        {
            if (totalDiscSize == 0)
            {
                labelTotalSizeText = "0MB";
                return;
            }

            labelTotalSizeText = totalDiscSize < 1000000000 ?
                string.Format("{0}MB", totalDiscSize / 1000000) :
                string.Format("{0:F2}GB", (float)totalDiscSize / 1000000000.0);

            // Calculate the size of the files
            Int64 totalMediaSize = 0;
            foreach (IMediaItem mediaItem in listBoxFilesItems)
                totalMediaSize += mediaItem.SizeOnDisc;

            if (totalMediaSize == 0)
                progressBarCapacityValue = 0;
            else
            {
                var percent = (int)((totalMediaSize * 100) / totalDiscSize);
                if (percent > 100)
                    progressBarCapacityValue = 100;
                else
                    progressBarCapacityValue = percent;
            }
        }
    }
}

