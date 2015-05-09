using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System;

namespace RecordToMP3.UI_Features.WaveFormViewer
{
    /// <summary>
    /// Interaction logic for WaveFormViewer.xaml
    /// </summary>
    public partial class WaveFormViewer : UserControl
    {
        #region Fields
        private int blankZone = 2;
        private int renderPosition;
        private double xScale = 2;
        private double yScale = 40;
        private double yTranslate = 40;
        private System.Windows.Media.Imaging.WriteableBitmap bitmap { get; set; }
        private int[] maxPoints;
        private int[] minPoints;

        #endregion

        #region Properties
        public Color LineColor { get; set; }
        #endregion

        public WaveFormViewer()
        {
            this.SizeChanged += OnSizeChanged;
            InitializeComponent();
        }

        #region Events
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            renderPosition = 0;
            maxPoints = new int[(int)((ActualWidth / xScale))];
            minPoints = new int[(int)((ActualWidth / xScale))];

            this.yTranslate = this.ActualHeight / 2;
            this.yScale = this.ActualHeight / 2;

            bitmap = null;
            bitmap = BitmapFactory.New((int)this.ActualWidth, (int)this.ActualHeight);
            bitmap.Lock();
            bitmap.Clear();
            bitmap.Unlock();
            mainCanvas.Source = bitmap;

            ClearAllPoints();
        }
        #endregion

        #region Dependency properties
        public Tuple<float, float> AddPoint
        {
            get { return (Tuple<float, float>)GetValue(AddPointProperty); }
            set { SetValue(AddPointProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Point.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AddPointProperty =
            DependencyProperty.Register("AddPoint", typeof(Tuple<float, float>), typeof(WaveFormViewer), new PropertyMetadata(new Tuple<float, float>(0f, 0f),(s, e) =>
                {
                    if (e.NewValue == null) return;
                    var newValue = e.NewValue as Tuple<float, float>;
                    (s as WaveFormViewer).AddValue(newValue.Item1, newValue.Item2);
                }));
        #endregion

        #region Private methods
        private int Points
        {
            get { return maxPoints.Length; }
        }

        private void ClearAllPoints()
        {
            for (int i = 0; i < Points; i++)
            {
                maxPoints[i] = SampleToYPosition(0);
                minPoints[i] = SampleToYPosition(0);
            }
        }

        private void CreatePoint(float topValue, float bottomValue)
        {
            var topYPos = SampleToYPosition(topValue);
            var bottomYPos = SampleToYPosition(bottomValue);

            maxPoints[renderPosition] = (int)topYPos;
            minPoints[renderPosition] = (int)bottomYPos;
            renderPosition++;
        }

        private int SampleToYPosition(float value)
        {
            return (int)(yTranslate + value * yScale);
        }
        #endregion

        #region Public methods
        public void AddValue(float maxValue, float minValue)
        {
            if (bitmap == null)
                return;

            if (Points > 0)
            {
                if (renderPosition < Points - blankZone)

                    using (bitmap.GetBitmapContext())
                    {
                        var i = renderPosition - 1;
                        bitmap.FillRectangle((i) * (int)xScale, SampleToYPosition(-1), (i + blankZone + 2) * (int)xScale, SampleToYPosition(1), 0);
                    }

                CreatePoint(maxValue, minValue);

                if (renderPosition > 1)
                    using (bitmap.GetBitmapContext())
                    {
                        var i = renderPosition - 1;
                        bitmap.DrawLineAa((i - 1) * (int)xScale, maxPoints[i - 1], (i) * (int)xScale, maxPoints[i], LineColor);
                        if (maxPoints[i] != minPoints[i])
                            bitmap.DrawLineAa((i - 1) * (int)xScale, minPoints[i - 1], (i) * (int)xScale, minPoints[i], LineColor);
                    }

                if (renderPosition >= Points)
                    renderPosition = 0;

                //int erasePosition = (renderPosition + blankZone) % Points;
                //if (erasePosition < Points)
                //{
                //    double yPos = SampleToYPosition(0);
                //    maxPoints[erasePosition] = (int)yPos;
                //    minPoints[erasePosition] = (int)yPos;
                //}
            }
        }
        #endregion
    }
}
