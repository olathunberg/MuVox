using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TTech.MuVox.UI.VolumeMeter
{
    public class VolumeMeter : FrameworkElement
    {
        private Brush accentColor = Brushes.Transparent;
        private Brush foreground = Brushes.Transparent;
        private Brush background = Brushes.Transparent;
        private double maxMark;
        private DateTime maxTime = DateTime.Now;

        /// <summary>
        /// Basic volume meter
        /// </summary>
        public VolumeMeter()
        {
        }

        public float Amplitude
        {
            get { return (float)GetValue(AmplitudeProperty); }
            set { SetValue(AmplitudeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Amplitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AmplitudeProperty =
            DependencyProperty.Register("Amplitude", typeof(float), typeof(VolumeMeter), new PropertyMetadata(0f, (s, e) =>
                {
                    if (e.OldValue != e.NewValue && s is FrameworkElement frameworkElement)
                        frameworkElement.InvalidateVisual();
                }));

        public VolumeMeterSettings Settings
        {
            get { return (VolumeMeterSettings)GetValue(SettingsProperty); }
            set { SetValue(SettingsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Settings.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings", typeof(VolumeMeterSettings), typeof(VolumeMeter), new PropertyMetadata(new VolumeMeterSettings()));

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
        /// Peakmark fallback speed
        /// </summary>
        [DefaultValue(2)]
        public int PeakMarkFallBackSpeed { get { return Settings.UX_VolumeMeter_PeakMarkFallBackSpeed; } }

        /// <summary>
        /// Color of peak mark
        /// </summary>
        public Brush PeakMarkColor { get; set; } = Brushes.Red;

        /// <summary>
        /// Number of milliseconds befor peak starts to fall
        /// </summary>
        public int PeakMarkHoldTime { get { return Settings.UX_VolumeMeter_PeakMarkHoldTime; } }

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
