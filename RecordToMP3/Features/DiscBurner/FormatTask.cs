using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using IMAPI2.Interop;

namespace RecordToMP3.Features.DiscBurner
{
    internal class FormatTask
    {
        private bool quickFormat;
        private bool ejectDisc;
        private IProgress<int> progress;

        public bool IsFormatting { get; private set; }

        internal async Task<int> StartFormat(IDiscRecorder2 discRecorder, bool quickFormat, bool ejectDisc,
            CancellationToken cancellationToken, IProgress<int> progress)
        {
            this.quickFormat = quickFormat;
            this.ejectDisc = ejectDisc;
            this.progress = progress;
            IsFormatting = true;

            var formatResult = await Task.Run(() => DoFormat(discRecorder.ActiveDiscRecorder));

            progress.Report(0);

            IsFormatting = false;

            return formatResult;
        }

        private int DoFormat(string activeDiscRecorder)
        {
            MsftDiscRecorder2 discRecorder = null;
            MsftDiscFormat2Erase discFormatErase = null;
            int result = 0;

            try
            {
                discRecorder = new MsftDiscRecorder2();
                discRecorder.InitializeDiscRecorder(activeDiscRecorder);

                discFormatErase = new MsftDiscFormat2Erase
                {
                    Recorder = discRecorder,
                    ClientName = "clientName",
                    FullErase = !quickFormat
                };

                discFormatErase.Update += discFormatErase_Update;

                try
                {
                    discFormatErase.EraseMedia();
                }
                catch (COMException ex)
                {
                    result = ex.ErrorCode;
                    MessageBox.Show(ex.Message, "IDiscFormat2.EraseMedia failed",
                        MessageBoxButton.OK, MessageBoxImage.Stop);
                }

                discFormatErase.Update -= discFormatErase_Update;

                if (ejectDisc)
                    discRecorder.EjectMedia();
            }
            catch (COMException exception)
            {
                MessageBox.Show(exception.Message);
            }
            finally
            {
                if (discRecorder != null)
                    Marshal.ReleaseComObject(discRecorder);

                if (discFormatErase != null)
                    Marshal.ReleaseComObject(discFormatErase);
            }

            return result;
        }

        void discFormatErase_Update([In, MarshalAs(UnmanagedType.IDispatch)] object sender, int elapsedSeconds, int estimatedTotalSeconds)
        {
            progress.Report(elapsedSeconds * 100 / estimatedTotalSeconds);
        }
    }
}
