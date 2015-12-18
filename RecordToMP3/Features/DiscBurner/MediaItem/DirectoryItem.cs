//
// DirectoryItem.cs
//
// by Eric Haddan
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using IMAPI2.Interop;

namespace IMAPI2.MediaItem
{
    class DirectoryItem : IMediaItem
    {
        private readonly string m_directoryPath;
        private string displayName;
        private System.Drawing.Image fileIconImage = null;
        private List<IMediaItem> mediaItems = new List<IMediaItem>();

        public DirectoryItem(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new FileNotFoundException("The directory added to DirectoryItem was not found!", directoryPath);
            }

            m_directoryPath = directoryPath;
            var fileInfo = new FileInfo(m_directoryPath);
            DisplayName = fileInfo.Name;

            string[] files = Directory.GetFiles(m_directoryPath);
            foreach (string file in files)
                mediaItems.Add(new FileItem(file));

            string[] directories = Directory.GetDirectories(m_directoryPath);
            foreach (string directory in directories)
                mediaItems.Add(new DirectoryItem(directory));

            var shinfo = new SHFILEINFO();
            IntPtr hImg = Win32.SHGetFileInfo(m_directoryPath, 0, ref shinfo,
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

        public string DisplayName
        {
            get { return displayName; }
            set { displayName = value; }
        }

        public System.Drawing.Image FileIconImage
        {
            get { return fileIconImage; }
        }

        public string Path
        {
            get { return m_directoryPath; }
        }

        public Int64 SizeOnDisc
        {
            get
            {
                Int64 totalSize = 0;
                foreach (IMediaItem mediaItem in mediaItems)
                {
                    totalSize += mediaItem.SizeOnDisc;
                }
                return totalSize;
            }
        }

        public bool AddToFileSystem(IFsiDirectoryItem rootItem)
        {
            try
            {
                rootItem.AddTree(m_directoryPath, true);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error adding folder",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
