using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RecordToMP3.UI_Features.VolumeMeter
{
    internal class VolumeMeter : FrameworkElement
    {
        private Brush accentColor;
        private Brush foreground;
        private Brush background;

        #region Dependency properties
        public float Amplitude
        {
            get { return (float)GetValue(AmplitudeProperty); }
            set { SetValue(AmplitudeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Amplitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AmplitudeProperty =
            DependencyProperty.Register("Amplitude", typeof(float), typeof(VolumeMeter), new PropertyMetadata(-3.0f, (s, e) => (s as FrameworkElement).InvalidateVisual()));
        
        #endregion

        /// <summary>
        /// Basic volume meter
        /// </summary>
        public VolumeMeter()
        {
            MinDb = -24;
            MaxDb = 12;
            Orientation = Orientation.Vertical;
        }

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
        [DefaultValue(-60.0)]
        public float MinDb { get; set; }

        /// <summary>
        /// Maximum decibels
        /// </summary>
        [DefaultValue(18.0)]
        public float MaxDb { get; set; }

        /// <summary>
        /// Meter orientation
        /// </summary>
        [DefaultValue(Orientation.Vertical)]
        public Orientation Orientation { get; set; }
        #endregion   

        // <summary>
        /// Paints the volume meter
        /// </summary>
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (this.ActualWidth <= 2 || this.ActualHeight <= 2)
                return;

            drawingContext.DrawRectangle(Background, new Pen(Background, 0), new Rect(0, 0, this.ActualWidth, this.ActualHeight));

            double db = 20 * Math.Log10(Amplitude);
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
            }
            else
            {
                height = (int)(height * percent);
                if (this.ActualHeight - 1 - height > 0)
                {
                    drawingContext.DrawRectangle(Foreground, new Pen(Foreground, 0), new Rect(1, this.ActualHeight - 1 - height, width, height));
                    drawingContext.DrawLine(new Pen(accentColor,2),new Point(1, this.ActualHeight - 1 - height), new Point(width+1, this.ActualHeight - 1 - height));
                }
            }
        }
    }
}
