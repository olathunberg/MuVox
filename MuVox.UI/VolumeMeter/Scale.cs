using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TTech.MuVox.UI.VolumeMeter
{
    public class Scale : Control
    {
        private int steps = 10;

        public VolumeMeterSettings Settings
        {
            get { return (VolumeMeterSettings)GetValue(SettingsProperty); }
            set { SetValue(SettingsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Settings.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings", typeof(VolumeMeterSettings), typeof(Scale), new PropertyMetadata(new VolumeMeterSettings()));

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (this.Height < 0 || this.Width < 0)
                return;

            var dbStep = (Settings.MaxDb - Settings.MinDb) / steps;
            foreach (var (text, index) in GetTexts(steps, dbStep).Select((value, i) => (value, i)))
            {
                drawingContext.DrawText(text, new Point(ActualWidth - text.Width, index * (ActualHeight - text.Height) / steps));
            }
        }

        private List<FormattedText> GetTexts(int steps, float dbStep)
        {
            var texts = new List<FormattedText>();
            for (int i = 0; i <= steps; i++)
            {
                texts.Add(new FormattedText((Settings.MaxDb - (i * dbStep)).ToString("0"),
                    CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    new Typeface(FontFamily, FontStyle, FontWeights.Thin, FontStretch),
                    12,
                    Foreground,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip));
            }

            return texts;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var dbStep = (Settings.MaxDb - Settings.MinDb) / steps;
            var texts = GetTexts(steps, dbStep);

            var width = double.IsPositiveInfinity(constraint.Width)
                ? texts.Select(x => x.Width).Max()
                : Math.Max(texts.Select(x => x.Width).Max(), constraint.Width);
            var height = double.IsPositiveInfinity(constraint.Height)
                ? texts.Sum(x => x.Width)
                : constraint.Height;
            return new Size(width, height);
        }
    }
}
