//
// FileItem.cs
//
// by Eric Haddan
//
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using IMAPI2.Interop;

namespace IMAPI2.MediaItem
{
    class FileItem : IMediaItem
    {
        private const Int64 SECTOR_SIZE = 2048;

        private readonly string displayName;
        private readonly string filePath;
        private System.Drawing.Image fileIconImage = null;
        private Int64 m_fileLength = 0;

        public FileItem(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("The file added to FileItem was not found!", path);
            }

            filePath = path;

            var fileInfo = new FileInfo(filePath);
            displayName = fileInfo.Name;
            m_fileLength = fileInfo.Length;

            // Get the File icon
            var shinfo = new SHFILEINFO();
            IntPtr hImg = Win32.SHGetFileInfo(filePath, 0, ref shinfo,
                (uint)Marshal.SizeOf(shinfo), Win32.SHGFI_ICON | Win32.SHGFI_SMALLICON);

            //The icon is returned in the hIcon member of the shinfo struct
            var imageConverter = new System.Drawing.IconConverter();
            var icon = System.Drawing.Icon.FromHandle(shinfo.hIcon);
            try
            {
                fileIconImage = (System.Drawing.Image)
                    imageConverter.ConvertTo(icon, typeof(System.Drawing.Image));
            }
            catch (NotSupportedException)
            {
            }

            Win32.DestroyIcon(shinfo.hIcon);
        }

        public System.Drawing.Image FileIconImage
        {
            get { return fileIconImage; }
        }

        public string Path
        {
            get { return filePath; }
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

        public bool AddToFileSystem(IFsiDirectoryItem rootItem)
        {
            IStream stream = null;

            try
            {
                Win32.SHCreateStreamOnFile(filePath, Win32.STGM_READ | Win32.STGM_SHARE_DENY_WRITE, ref stream);

                if (stream != null)
                {
                    rootItem.AddFile(displayName, stream);
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error adding file",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (stream != null)
                    Marshal.FinalReleaseComObject(stream);
            }

            return false;
        }

        public override string ToString()
        {
            return displayName;
        }
    }
}
