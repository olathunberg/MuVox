using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

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
        private Polygon waveForm = new Polygon();
        private double xScale = 2;
        private double yScale = 40;
        private double yTranslate = 40;
        #endregion

        #region Dependency properties
        // Using a DependencyProperty as the backing store for Fill.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(WaveFormViewer), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));

        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }
        #endregion

        public WaveFormViewer()
        {
            this.SizeChanged += OnSizeChanged;
            InitializeComponent();
            waveForm.StrokeThickness = 1;
            mainCanvas.Children.Add(waveForm);
        }
        #region Events
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            renderPosition = 0;
            ClearAllPoints();

            this.yTranslate = this.ActualHeight / 2;
            this.yScale = this.ActualHeight / 2;
        }
        #endregion

        #region Private methods
        private int Points
        {
            get { return waveForm.Points.Count / 2; }
        }

        private int BottomPointIndex(int position)
        {
            return waveForm.Points.Count - position - 1;
        }

        private void ClearAllPoints()
        {
            waveForm.Points.Clear();
        }

        private void CreatePoint(float topValue, float bottomValue)
        {
            double topYPos = SampleToYPosition(topValue);
            double bottomYPos = SampleToYPosition(bottomValue);
            double xPos = renderPosition * xScale;
            if (renderPosition >= Points)
            {
                int insertPos = Points;
                waveForm.Points.Insert(insertPos, new Point(xPos, topYPos));
                waveForm.Points.Insert(insertPos + 1, new Point(xPos, bottomYPos));
            }
            else
            {
                waveForm.Points[renderPosition] = new Point(xPos, topYPos);
                waveForm.Points[BottomPointIndex(renderPosition)] = new Point(xPos, bottomYPos);
            }
            renderPosition++;
        }

        private double SampleToYPosition(float value)
        {
            return yTranslate + value * yScale;
        }
        #endregion

        #region Public methods
        public void AddValue(float maxValue, float minValue)
        {
            waveForm.Stroke = this.Foreground;
            waveForm.Fill = this.Fill;
            int visiblePixels = (int)((ActualWidth / xScale));
            if (visiblePixels > 0)
            {
                CreatePoint(maxValue, minValue);

                if (renderPosition >= visiblePixels)
                    renderPosition = 0;

                int erasePosition = (renderPosition + blankZone) % visiblePixels;
                if (erasePosition < Points)
                {
                    double yPos = SampleToYPosition(0);
                    waveForm.Points[erasePosition] = new Point(erasePosition * xScale, yPos);
                    waveForm.Points[BottomPointIndex(erasePosition)] = new Point(erasePosition * xScale, yPos);
                }
            }
        }

        /// <summary>
        /// Clears the waveform and repositions on the left
        /// </summary>
        public void Reset()
        {
            renderPosition = 0;
            ClearAllPoints();
        }
        #endregion
    }
}
