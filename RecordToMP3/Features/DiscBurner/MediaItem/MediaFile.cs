using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

namespace IMAPI2.MediaItem
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WAV_HEADER
    {
        public Int32 chunkID;
        public Int32 chunkSize;
        public Int32 format;
        public Int32 formatChunkId;
        public Int32 formatChunkSize;
        public Int16 audioFormat;
        public Int16 numChannels;
        public Int32 sampleRate;
        public Int32 byteRate;
        public Int16 blockAlign;
        public Int16 bitsPerSample;
        public Int32 dataChunkId;
        public Int32 dataChunkSize;
    }

    class MediaFile
    {
        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_LARGEICON = 0x0;
        public const uint SHGFI_SMALLICON = 0x1;

        [DllImport("ole32.dll")]
        static extern int CreateStreamOnHGlobal(IntPtr hGlobal, bool fDeleteOnRelease, out IStream ppstm);

        private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;

        private const uint STGM_DELETEONRELEASE = 0x04000000;
        private const uint STGM_SHARE_DENY_WRITE = 0x00000020;
        private const uint STGM_SHARE_DENY_NONE = 0x00000040;
        private const uint STGM_READ = 0x00000000;

        private long SECTOR_SIZE = 2352;
        private Int64 m_fileLength = 0;

        public MediaFile(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("The file added to FileItem was not found!",path);

            filePath = path;

            var fileInfo = new FileInfo(filePath);
            displayName = fileInfo.Name;
            m_fileLength = fileInfo.Length;

            var shinfo = new SHFILEINFO();
            IntPtr hImg = SHGetFileInfo(filePath, 0, ref shinfo,
                (uint)Marshal.SizeOf(shinfo), SHGFI_ICON|SHGFI_SMALLICON);

            //The icon is returned in the hIcon member of the shinfo struct
            fileIcon = System.Drawing.Icon.FromHandle(shinfo.hIcon);
        }

        public Int64 SizeOnDisc
        {
            get
            {
                if (m_fileLength > 0)
                    return ((m_fileLength / SECTOR_SIZE) + 1) * SECTOR_SIZE;

                return 0;
            }
        }

        public string Path
        {
            get
            {
                return filePath;
            }
        }
        private string filePath;

        public System.Drawing.Icon FileIcon
        {
            get
            {
                return fileIcon;
            }
        }
        private System.Drawing.Icon fileIcon = null;


        public override string ToString()
        {
            return displayName;
        }
        private string displayName;

        /// <summary>
        /// Prepares a stream to be written to the media
        /// </summary>
        public void PrepareStream()
        {
            byte[] waveData = new byte[SizeOnDisc];

            //
            // The size of the stream must be a multiple of the sector size 2352
            // SizeOnDisc rounds up to the next sector size
            //
            IntPtr fileData = Marshal.AllocHGlobal((IntPtr)SizeOnDisc);
            FileStream fileStream = File.OpenRead(filePath);

            int sizeOfHeader = Marshal.SizeOf(typeof(WAV_HEADER));

            //
            // Skip over the Wav header data, because it only needs the actual data
            //
            fileStream.Read(waveData, sizeOfHeader, (int)m_fileLength - sizeOfHeader);

            Marshal.Copy(waveData, 0, fileData, (int)m_fileLength - sizeOfHeader);

            CreateStreamOnHGlobal(fileData, true, out wavStream);
        }

        private IStream wavStream = null;
        public IStream GetTrackIStream()
        {
            return wavStream;
        }

        /// <summary>
        /// Determines if the Wav file is the proper format to be written to CD
        /// The proper format is uncompressed PCM, 44.1KHz, Stereo
        /// </summary>
        /// <param name="wavFile">the selected wav file</param>
        /// <returns>true if proper format, otherwise false</returns>
        public static bool IsWavProperFormat(string wavFile)
        {
            FileStream fileStream = null;
            try
            {
                fileStream = File.OpenRead(wavFile);

                // Read the header data
                var binaryReader = new BinaryReader(fileStream);
                byte[] byteData = binaryReader.ReadBytes(Marshal.SizeOf(typeof(WAV_HEADER)));
                GCHandle handle = GCHandle.Alloc(byteData, GCHandleType.Pinned);
                binaryReader.Close();
                fileStream.Close();

                // Convert to the wav header structure
                var wavHeader = (WAV_HEADER)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(WAV_HEADER));

                // Verify the WAV file is a 44.1KHz, Stereo, Uncompressed Wav file.
                if ((wavHeader.chunkID == 0x46464952) &&        // "RIFF"
                    (wavHeader.format == 0x45564157) &&         // "WAVE"
                    (wavHeader.formatChunkId == 0x20746d66) &&  // "fmt "
                    (wavHeader.audioFormat == 1) &&             // 1 = PCM (uncompressed)
                    (wavHeader.numChannels == 2) &&             // 2 = Stereo
                    (wavHeader.sampleRate == 44100))            // 44.1 KHz
                {
                    return true;
                }

                MessageBox.Show(wavFile + " is not the correct format!");

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
    }
}
