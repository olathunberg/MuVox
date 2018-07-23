using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RecordToMP3.Features.Settings;

namespace RecordToMP3.UI_Features.VolumeMeter
{
    internal class VolumeMeter : FrameworkElement
    {
        #region Fields
        private Brush accentColor;
        private Brush foreground;
        private Brush background;
        private double maxMark;
        private DateTime maxTime = DateTime.Now;

        private Settings Settings { get { return SettingsBase<Settings>.Current; } }
        #endregion

        /// <summary>
        /// Basic volume meter
        /// </summary>
        public VolumeMeter()
        {
            Orientation = Orientation.Vertical;
        }

        #region Dependency properties
        public float Amplitude
        {
            get { return (float)GetValue(AmplitudeProperty); }
            set { SetValue(AmplitudeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Amplitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AmplitudeProperty =
            DependencyProperty.Register("Amplitude", typeof(float), typeof(VolumeMeter), new PropertyMetadata(0f, (s, e) =>
                {
                    if (e.OldValue != e.NewValue)
                        (s as FrameworkElement).InvalidateVisual();
                }));

        #endregion

        #region Properties
        public Brush AccentColor
        {
            get { return accentColor; }
            set
            {
                accentColor = value;
                this.InvalidateVisual();
            }
        }

        public Brush Foreground
        {
            get { return foreground; }
            set
            {
                foreground = value;
                this.InvalidateVisual();
            }
        }

        public Brush Background
        {
            get { return background; }
            set
            {
                background = value;
                this.InvalidateVisual();
            }
        }

        /// <summary>
        /// Minimum decibels
        /// </summary>
        public float MinDb { get { return Settings.UX_VolumeMeter_MinDb; } }

        /// <summary>
        /// Maximum decibels
        /// </summary>
        public float MaxDb { get { return Settings.UX_VolumeMeter_MaxDb; } }

        /// <summary>
        /// Meter orientation
        /// </summary>
        [DefaultValue(Orientation.Vertical)]
        public Orientation Orientation { get; set; }

        /// <summary>
        /// Peakmark fallback speed
        /// </summary>
        [DefaultValue(2)]
        public int PeakMarkFallBackSpeed { get { return Settings.UX_VolumeMeter_PeakMarkFallBackSpeed; } }

        /// <summary>
        /// Color of peak mark
        /// </summary>
        public Brush PeakMarkColor { get; set; }

        /// <summary>
        /// Number of milliseconds befor peak starts to fall
        /// </summary>
        public int PeakMarkHoldTime { get { return Settings.UX_VolumeMeter_PeakMarkHoldTime; } }
        #endregion

        /// <summary>
        /// Paints the volume meter
        /// </summary>
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (this.Height < 0 || this.Width < 0)
                return;
            if (this.ActualWidth <= 2 || this.ActualHeight <= 2)
                return;

            drawingContext.DrawRectangle(Background, new Pen(Background, 0), new Rect(0, 0, this.ActualWidth, this.ActualHeight));

            double db = NAudio.Utils.Decibels.LinearToDecibels(Amplitude);
            if (db < MinDb)
                db = MinDb;
            if (db > MaxDb)
                db = MaxDb;
            double percent = (db - MinDb) / (MaxDb - MinDb);

            var width = this.ActualWidth - 2;
            var height = this.ActualHeight - 2;
            if (Orientation == Orientation.Horizontal)
            {
                width = (int)(width * percent);

                drawingContext.DrawRectangle(Foreground, new Pen(Foreground, 0), new Rect(1, 1, width, height));
                // TODO: Draw "top" mark
            }
            else
            {
                double zeroDb = (-MinDb) / (MaxDb - MinDb);
                var zeroHeight = (int)(height * zeroDb);

                height = (int)(height * percent);
                if (height > maxMark)
                {
                    maxMark = height;
                    maxTime = DateTime.Now;
                }
                if ((DateTime.Now - maxTime).TotalMilliseconds > PeakMarkHoldTime && maxMark > 1)
                    maxMark -= PeakMarkFallBackSpeed;

                if (this.ActualHeight - 1 - maxMark > 1)
                    drawingContext.DrawLine(new Pen(PeakMarkColor, 2), new Point(1, this.ActualHeight - 1 - maxMark), new Point(width + 1, this.ActualHeight - 1 - maxMark));

                if (this.ActualHeight - 1 - height > 0)
                    drawingContext.DrawRectangle(Foreground, new Pen(Foreground, 0), new Rect(1, this.ActualHeight - 1 - height, width, height));

                // 0db mark
                drawingContext.DrawLine(new Pen(Brushes.Red, 1), new Point(1, this.ActualHeight - 1 - zeroHeight), new Point(width + 1, this.ActualHeight - 1 - zeroHeight));
            }
        }
    }
}
