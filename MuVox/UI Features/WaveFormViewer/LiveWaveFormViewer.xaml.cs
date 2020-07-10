using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TTech.MuVox.UI_Features.WaveFormViewer
{
    public partial class LiveWaveFormViewer : UserControl
    {
        private readonly int blankZone = 2;
        private readonly double xScale = 2;
        private WriteableBitmap? bitmap;
        private int[] maxPoints = Array.Empty<int>();
        private int[] minPoints = Array.Empty<int>();
        private int renderPosition;
        private int yScale = 40;
        private int yTranslate = 40;

        public LiveWaveFormViewer()
        {
            this.SizeChanged += OnSizeChanged;
            InitializeComponent();
        }

        public Color AccentColor { get; set; } = Colors.Red;

        public (float, float) AddPoint
        {
            get { return ((float, float))GetValue(AddPointProperty); }
            set { SetValue(AddPointProperty, value); }
        }

        public Color LineColor { get; set; }

        public void AddValue(float maxValue, float minValue)
        {
            if (bitmap == null)
                return;

            if (maxPoints.Length > 0)
            {
                if (renderPosition < maxPoints.Length - blankZone)
                {
                    using (bitmap.GetBitmapContext())
                    {
                        var i = renderPosition - 1;
                        bitmap.FillRectangle(i * (int)xScale, SampleToYPosition(-1), (i + blankZone + 2) * (int)xScale, SampleToYPosition(1), 0);
                    }
                }

                CreatePoint(maxValue, minValue);

                if (renderPosition > 1)
                {
                    using (bitmap.GetBitmapContext())
                    {
                        var i = renderPosition - 1;
                        bitmap.DrawLine((i - 1) * (int)xScale, maxPoints[i - 1], (i) * (int)xScale, maxPoints[i], LineColor);
                        if (maxPoints[i] != minPoints[i])
                            bitmap.DrawLine((i - 1) * (int)xScale, minPoints[i - 1], (i) * (int)xScale, minPoints[i], LineColor);
                    }
                }

                if (renderPosition >= maxPoints.Length)
                    renderPosition = 0;
            }
        }

        private void ClearAllPoints()
        {
            for (int i = 0; i < maxPoints.Length; i++)
            {
                maxPoints[i] = SampleToYPosition(0);
                minPoints[i] = SampleToYPosition(0);
            }
        }

        private void CreatePoint(float topValue, float bottomValue)
        {
            var topYPos = SampleToYPosition(topValue);
            var bottomYPos = SampleToYPosition(bottomValue);

            maxPoints[renderPosition] = topYPos;
            minPoints[renderPosition] = bottomYPos;
            renderPosition++;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            renderPosition = 0;
            maxPoints = new int[(int)(ActualWidth / xScale)];
            minPoints = new int[(int)(ActualWidth / xScale)];

            this.yTranslate = (int)(this.ActualHeight / 2);
            this.yScale = (int)(this.ActualHeight / 2);

            bitmap = null;
            bitmap = BitmapFactory.New((int)this.ActualWidth, (int)this.ActualHeight);
            bitmap.Lock();
            bitmap.Clear();
            bitmap.Unlock();
            mainCanvas.Source = bitmap;

            ClearAllPoints();
        }

        private int SampleToYPosition(float value)
        {
            return (int)(yTranslate + value * yScale);
        }

        public static readonly DependencyProperty AddPointProperty = DependencyProperty.Register(nameof(AddPoint), typeof((float, float)), typeof(LiveWaveFormViewer), new PropertyMetadata((0f, 0f), (s, e) =>
        {
            if (e.NewValue == null) return;

            var newValue = ((float, float))e.NewValue;
            if (s is LiveWaveFormViewer liveWaveFormViewer)
                liveWaveFormViewer.AddValue(newValue.Item1, newValue.Item2);
        }));
    }
}