//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.IO;
//using System.Text;
//using System.Windows.Forms;
//using IMAPI2.Interop;
//using System.Runtime.InteropServices;
//using System.Runtime.InteropServices.ComTypes;

//namespace MusicCD
//{
//    public partial class MainForm : Form
//    {
//        private string m_clientName = "MusicCD";
//        private bool m_isBurning = false;
//        private BurnData m_burnData = new BurnData();


//        /// <summary>
//        /// Initialize the form
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        private void MainForm_Load(object sender, EventArgs e)
//        {
//            //
//            // Determine the current recording devices
//            //
//            MsftDiscMaster2 discMaster = new MsftDiscMaster2();
//            if (!discMaster.IsSupportedEnvironment)
//                return;
//            //foreach (string uniqueRecorderID in discMaster)
//            //{
//            //    MsftDiscRecorder2 discRecorder2 = new MsftDiscRecorder2();
//            //    discRecorder2.InitializeDiscRecorder(uniqueRecorderID);

//            //    devicesComboBox.Items.Add(discRecorder2);
//            //}
//            //if (devicesComboBox.Items.Count > 0)
//            //{
//            //    devicesComboBox.SelectedIndex = 0;
//            //}

//            UpdateCapacity();

//            //labelCDProgress.Text = string.Empty;
//            //labelStatusText.Text = string.Empty;
//        }

//        #region Device ComboBox Methods
//        private void devicesComboBox_SelectedIndexChanged(object sender, EventArgs e)
//        {
//            IDiscRecorder2 discRecorder =
//                (IDiscRecorder2)devicesComboBox.Items[devicesComboBox.SelectedIndex];

//            //
//            // Verify recorder is supported
//            //
//            IDiscFormat2Data discFormatData = new MsftDiscFormat2Data();
//            if (!discFormatData.IsRecorderSupported(discRecorder))
//            {
//                MessageBox.Show("Recorder not supported", m_clientName,
//                    MessageBoxButtons.OK, MessageBoxIcon.Error);
//                return;
//            }
//        }

//        private void devicesComboBox_Format(object sender, ListControlConvertEventArgs e)
//        {
//            IDiscRecorder2 discRecorder2 = (IDiscRecorder2)e.ListItem;
//            string devicePaths = string.Empty;
//            string volumePath = (string)discRecorder2.VolumePathNames.GetValue(0);
//            foreach (string volPath in discRecorder2.VolumePathNames)
//            {
//                if (!string.IsNullOrEmpty(devicePaths))
//                {
//                    devicePaths += ",";
//                }
//                devicePaths += volumePath;
//            }

//            e.Value = string.Format("{0} [{1}]", devicePaths, discRecorder2.ProductId);
//        }

//        #endregion

//        /// <summary>
//        /// User clicked the "Burn" button
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        private void buttonBurn_Click(object sender, EventArgs e)
//        {
//            if (m_isBurning)
//            {
//                //
//                // If we're burning, then the user has pressed the cancel button
//                //
//                buttonBurn.Enabled = false;
//                backgroundWorker.CancelAsync();
//            }
//            else
//            {
//                //
//                // We want to burn, so disable many selection controls
//                //
//                m_isBurning = true;
//                EnableBurnUI(false);

//                //
//                // Get the unique recorder id and send it to the worker thread
//                //
//                IDiscRecorder2 discRecorder =
//                    (IDiscRecorder2)devicesComboBox.Items[devicesComboBox.SelectedIndex];
//                m_burnData.uniqueRecorderId = discRecorder.ActiveDiscRecorder;
//                backgroundWorker.RunWorkerAsync(m_burnData);
//            }
//        }

//        /// <summary>
//        /// The thread that does the burning of the media
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
//        {
//            e.Result = 0;

//            MsftDiscMaster2 discMaster = new MsftDiscMaster2();
//            MsftDiscRecorder2 discRecorder2 = new MsftDiscRecorder2();

//            BurnData burnData = (BurnData)e.Argument;
//            discRecorder2.InitializeDiscRecorder(burnData.uniqueRecorderId);

//            MsftDiscFormat2TrackAtOnce trackAtOnce = new MsftDiscFormat2TrackAtOnce();
//            trackAtOnce.ClientName = m_clientName;
//            trackAtOnce.Recorder = discRecorder2;
//            m_burnData.totalTracks = listBoxFiles.Items.Count;
//            m_burnData.currentTrackNumber = 0;

//            //
//            // Prepare the wave file streams
//            //
//            foreach (MediaFile mediaFile in listBoxFiles.Items)
//            {
//                //
//                // Check if we've cancelled
//                //
//                if (backgroundWorker.CancellationPending)
//                {
//                    break;
//                }

//                //
//                // Report back to the UI that we're preparing stream
//                //
//                m_burnData.task = BURN_MEDIA_TASK.BURN_MEDIA_TASK_PREPARING;
//                m_burnData.filename = mediaFile.ToString();
//                m_burnData.currentTrackNumber++;

//                backgroundWorker.ReportProgress(0, m_burnData);
//                mediaFile.PrepareStream();
//            }


//            //
//            // Add the Update event handler
//            //
//            trackAtOnce.Update += new DiscFormat2TrackAtOnce_EventHandler(trackAtOnce_Update);

//            trackAtOnce.PrepareMedia();

//            //
//            // Add Files and Directories to File System Image
//            //
//            foreach (MediaFile mediaFile in listBoxFiles.Items)
//            {
//                //
//                // Check if we've cancelled
//                //
//                if (backgroundWorker.CancellationPending)
//                {
//                    e.Result = -1;
//                    break;
//                }

//                //
//                // Add audio track
//                //
//                m_burnData.filename = mediaFile.ToString();
//                IStream stream = mediaFile.GetTrackIStream();
//                trackAtOnce.AddAudioTrack(stream);
//            }


//            //
//            // Remove the Update event handler
//            //
//            trackAtOnce.Update -= new DiscFormat2TrackAtOnce_EventHandler(trackAtOnce_Update);

//            trackAtOnce.ReleaseMedia();

//            discRecorder2.EjectMedia();
//        }

//        /// <summary>
//        /// Update notification from IDiscFormat2TrackAtOnce
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="progress"></param>
//        void trackAtOnce_Update(object sender, object progress)
//        {
//            //
//            // Check if we've cancelled
//            //
//            if (backgroundWorker.CancellationPending)
//            {
//                IDiscFormat2TrackAtOnce trackAtOnce = (IDiscFormat2TrackAtOnce)sender;
//                trackAtOnce.CancelAddTrack();
//                return;
//            }

//            IDiscFormat2TrackAtOnceEventArgs eventArgs = (IDiscFormat2TrackAtOnceEventArgs)progress;

//            m_burnData.task = BURN_MEDIA_TASK.BURN_MEDIA_TASK_WRITING;

//            //
//            // IDiscFormat2TrackAtOnceEventArgs Interface
//            //
//            m_burnData.currentTrackNumber = eventArgs.CurrentTrackNumber;
//            m_burnData.elapsedTime = eventArgs.ElapsedTime;
//            m_burnData.remainingTime = eventArgs.RemainingTime;

//            //
//            // IWriteEngine2EventArgs Interface
//            //
//            m_burnData.currentAction = eventArgs.CurrentAction;
//            m_burnData.startLba = eventArgs.StartLba;
//            m_burnData.sectorCount = eventArgs.SectorCount;
//            m_burnData.lastReadLba = eventArgs.LastReadLba;
//            m_burnData.lastWrittenLba = eventArgs.LastWrittenLba;
//            m_burnData.totalSystemBuffer = eventArgs.TotalSystemBuffer;
//            m_burnData.usedSystemBuffer = eventArgs.UsedSystemBuffer;
//            m_burnData.freeSystemBuffer = eventArgs.FreeSystemBuffer;

//            //
//            // Report back to the UI
//            //
//            backgroundWorker.ReportProgress(0, m_burnData);
//        }

//        /// <summary>
//        /// Notification that the burn background thread has completed
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
//        {
//            if ((int)e.Result == 0)
//            {
//                labelCDProgress.Text = "Finished Burning Disc!";
//            }
//            else
//            {
//                labelCDProgress.Text = "Error Burning Disc!";
//            }
//            labelStatusText.Text = string.Empty;
//            statusProgressBar.Value = 0;
//            progressBarCD.Value = 0;

//            m_isBurning = false;
//            EnableBurnUI(true);
//            buttonBurn.Enabled = true;
//        }

//        /// <summary>
//        /// Update the user interface with the current progress
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
//        {
//            BurnData burnData = (BurnData)e.UserState;

//            if (burnData.task == BURN_MEDIA_TASK.BURN_MEDIA_TASK_PREPARING)
//            {
//                //
//                // Notification that we're preparing a stream
//                //
//                labelCDProgress.Text = string.Format("Preparing stream for {0}", burnData.filename);
//                progressBarCD.Value = (int)burnData.currentTrackNumber;
//                progressBarCD.Maximum = burnData.totalTracks;
//            }
//            else if (burnData.task == BURN_MEDIA_TASK.BURN_MEDIA_TASK_WRITING)
//            {
//                switch (burnData.currentAction)
//                {
//                    case IMAPI_FORMAT2_TAO_WRITE_ACTION.IMAPI_FORMAT2_TAO_WRITE_ACTION_PREPARING:
//                        labelCDProgress.Text = string.Format("Writing Track {0} - {1} of {2}",
//                            burnData.filename, burnData.currentTrackNumber, burnData.totalTracks);
//                        progressBarCD.Value = (int)burnData.currentTrackNumber;
//                        progressBarCD.Maximum = burnData.totalTracks;
//                        break;

//                    case IMAPI_FORMAT2_TAO_WRITE_ACTION.IMAPI_FORMAT2_TAO_WRITE_ACTION_WRITING:
//                        long writtenSectors = burnData.lastWrittenLba - burnData.startLba;

//                        if (writtenSectors > 0 && burnData.sectorCount > 0)
//                        {
//                            int percent = (int)((100 * writtenSectors) / burnData.sectorCount);
//                            labelStatusText.Text = string.Format("Progress: {0}%", percent);
//                            statusProgressBar.Value = percent;
//                        }
//                        else
//                        {
//                            labelStatusText.Text = "Track Progress 0%";
//                            statusProgressBar.Value = 0;
//                        }
//                        break;

//                    case IMAPI_FORMAT2_TAO_WRITE_ACTION.IMAPI_FORMAT2_TAO_WRITE_ACTION_FINISHING:
//                        labelStatusText.Text = "Finishing...";
//                        break;
//                }
//            }
//        }


//        #region Add/Remove Media to ListBox
//        private void buttonAdd_Click(object sender, EventArgs e)
//        {
//            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
//            {
//                foreach (string filename in openFileDialog.FileNames)
//                {
//                    if (MediaFile.IsWavProperFormat(filename))
//                    {
//                        MediaFile mediaFile = new MediaFile(filename);
//                        listBoxFiles.Items.Add(mediaFile);
//                    }
//                }

//                UpdateCapacity();
//                EnableBurnButton();
//            }

//        }

//        private void buttonRemove_Click(object sender, EventArgs e)
//        {
//            MediaFile mediaFile = (MediaFile)listBoxFiles.SelectedItem;
//            if (mediaFile == null)
//                return;

//            if (MessageBox.Show("Are you sure you want to remove \"" + mediaFile + "\"?",
//                "Remove item", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
//            {
//                listBoxFiles.Items.Remove(mediaFile);

//                EnableBurnButton();
//                UpdateCapacity();
//            }
//        }

//        /// <summary>
//        /// Enable the Burn Button if items in the file listbox
//        /// </summary>
//        private void EnableBurnButton()
//        {
//            buttonBurn.Enabled = (listBoxFiles.Items.Count > 0);
//        }


//        /// <summary>
//        /// Enables/Disables the "Burn" User Interface
//        /// </summary>
//        /// <param name="enable"></param>
//        void EnableBurnUI(bool enable)
//        {
//            buttonBurn.Text = enable ? "&Burn" : "&Cancel";

//            devicesComboBox.Enabled = enable;
//            listBoxFiles.Enabled = enable;

//            buttonAdd.Enabled = enable;
//            buttonRemove.Enabled = enable;
//        }


//        /// <summary>
//        /// Updates the capacity progressbar
//        /// </summary>
//        private void UpdateCapacity()
//        {
//            //
//            // Calculate the size of the files
//            //
//            Int64 totalMediaSize = 0;
//            foreach (MediaFile mediaFile in listBoxFiles.Items)
//            {
//                totalMediaSize += mediaFile.SizeOnDisc;
//            }

//            if (totalMediaSize == 0)
//            {
//                progressBarCapacity.Value = 0;
//                progressBarCapacity.ForeColor = SystemColors.Highlight;
//            }
//            else
//            {
//                int percent = (int)((totalMediaSize * 100) / 700000000);
//                if (percent > 100)
//                {
//                    progressBarCapacity.Value = 100;
//                    progressBarCapacity.ForeColor = Color.Red;
//                }
//                else
//                {
//                    progressBarCapacity.Value = percent;
//                    progressBarCapacity.ForeColor = SystemColors.Highlight;
//                }
//            }
//        }

//        /// <summary>
//        /// DrawItem event for listBoxFiles ListBox
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        private void listBoxFiles_DrawItem(object sender, DrawItemEventArgs e)
//        {
//            if (e.Index == -1)
//            {
//                return;
//            }

//            MediaFile mediaFile = (MediaFile)listBoxFiles.Items[e.Index];
//            if (mediaFile == null)
//            {
//                return;
//            }

//            e.DrawBackground();

//            if ((e.State & DrawItemState.Focus) != 0)
//            {
//                e.DrawFocusRectangle();
//            }

//            e.Graphics.DrawIcon(mediaFile.FileIcon, 4, e.Bounds.Y + 4);

//            RectangleF rectF = new RectangleF(e.Bounds.X + 24, e.Bounds.Y,
//                e.Bounds.Width - 24, e.Bounds.Height);

//            Font font = new Font(FontFamily.GenericSansSerif, 10);

//            StringFormat stringFormat = new StringFormat();
//            stringFormat.LineAlignment = StringAlignment.Center;
//            stringFormat.Alignment = StringAlignment.Near;
//            stringFormat.Trimming = StringTrimming.EllipsisCharacter;

//            e.Graphics.DrawString(mediaFile.ToString(), font, new SolidBrush(e.ForeColor),
//                rectF, stringFormat);

//        }
//        #endregion

//    }

//    public enum BURN_MEDIA_TASK
//    {
//        BURN_MEDIA_TASK_PREPARING,
//        BURN_MEDIA_TASK_WRITING
//    }

//    public class BurnData
//    {
//        public string uniqueRecorderId;
//        public int totalTracks;
//        public string filename;
//        public BURN_MEDIA_TASK task;

//        // IDiscFormat2DataEventArgs Interface
//        public long elapsedTime;		// Elapsed time in seconds
//        public long remainingTime;		// Remaining time in seconds
//        public long currentTrackNumber;	// current track
//        // IWriteEngine2EventArgs Interface
//        public IMAPI_FORMAT2_TAO_WRITE_ACTION currentAction;
//        public long startLba;			// the starting lba of the current operation
//        public long sectorCount;		// the total sectors to write in the current operation
//        public long lastReadLba;		// the last read lba address
//        public long lastWrittenLba;	// the last written lba address
//        public long totalSystemBuffer;	// total size of the system buffer
//        public long usedSystemBuffer;	// size of used system buffer
//        public long freeSystemBuffer;	// size of the free system buffer
//    }
//}
