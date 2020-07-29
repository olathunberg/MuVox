﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace TTech.MuVox.UI.VolumeMeter
{
    public class VolumeMeter : FrameworkElement
    {
        private double maxMark;
        private DateTime maxTime = DateTime.Now;

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

        /// <summary>
        /// Minimum decibels
        /// </summary>
        public float MinDb { get { return Settings.MinDb; } }

        /// <summary>
        /// Maximum decibels
        /// </summary>
        public float MaxDb { get { return Settings.MaxDb; } }

        /// <summary>
        /// Number of milliseconds before peak starts to fall
        /// </summary>
        public int PeakHoldTime { get { return Settings.PeakHoldTime; } }

        private Brush darkGreen = new SolidColorBrush(Color.FromRgb(0x00, 0x4d, 0x0d));
        private Brush darkOrange = new SolidColorBrush(Color.FromRgb(0x4d, 0x4d, 0x00));
        private Brush brightRed = new SolidColorBrush(Color.FromRgb(0xff, 0x00, 0x00));
        private Brush brightGreen = new SolidColorBrush(Color.FromRgb(0x00, 0xe6, 0x00));
        private Brush brightOrange = new SolidColorBrush(Color.FromRgb(0xe6, 0xe6, 0x00));
        private Brush darkRed = new SolidColorBrush(Color.FromRgb(0x8b, 0x00, 0x00));
        private double width;
        private int blockHeight = 5;
        private int blockSpacing = 2;
        private int numBlocks;

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            width = this.ActualWidth - 2;
            numBlocks = (int)((this.ActualHeight + blockSpacing - 1) / blockHeight);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (this.Height < 0 || this.Width < 0)
                return;
            if (this.ActualWidth <= 2 || this.ActualHeight <= blockSpacing)
                return;
            if (numBlocks == 0)
            {
                numBlocks = (int)((this.ActualHeight + blockSpacing - 1) / blockHeight);
                width = this.ActualWidth - 2;
                for (int i = 0; i < numBlocks; i++)
                {
                    PaintBlock(drawingContext, i, false);
                }
            }

            var db = DesignerProperties.GetIsInDesignMode(this)
                ? NAudio.Utils.Decibels.LinearToDecibels(0) //new Random(DateTime.Now.Millisecond).NextDouble())
                : NAudio.Utils.Decibels.LinearToDecibels(Amplitude);
            if (db < MinDb)
                db = MinDb;
            if (db > MaxDb)
                db = MaxDb;

            if (db > maxMark)
            {
                maxMark = db;
                maxTime = DateTime.Now;
            }

            if ((DateTime.Now - maxTime).TotalMilliseconds > PeakHoldTime && maxMark > MinDb)
                maxMark -= 2;

            var numBlocksToPaint = ((maxMark - MinDb) / (MaxDb - MinDb)) * numBlocks;

            for (int i = 0; i < numBlocks; i++)
            {
                PaintBlock(drawingContext, i, i < numBlocksToPaint);
            }
        }

        private void PaintBlock(DrawingContext drawingContext, int i, bool isOn)
        {
            var color = i switch
            {
                _ when !isOn => i switch
                {
                    _ when i < numBlocks - 4 => darkGreen,  // Config
                    _ when i < numBlocks - 1 => darkOrange, // Config
                    _ => darkRed,
                },
                _ when i < numBlocks - 4 => brightGreen,  // Config
                _ when i < numBlocks - 1 => brightOrange, // Config
                _ => brightRed
            };

            drawingContext.DrawRectangle(color, null, new Rect(1, this.ActualHeight - ((i + 1) * blockHeight) + blockSpacing - 1, width, blockHeight - blockSpacing));
        }
    }
}