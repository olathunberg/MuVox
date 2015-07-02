using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Linq;
using System;
using NAudio.Wave;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Windows.Input;

namespace RecordToMP3.UI_Features.WaveFormViewer
{
    /// <summary>
    /// Interaction logic for WaveFormViewer.xaml
    /// </summary>
    public partial class WaveFormViewer : UserControl
    {
        #region Fields
        private System.Windows.Media.Imaging.WriteableBitmap bitmap { get; set; }
        private Point? dragStart = null;

        private WaveStream waveStream;
        private int samplesPerPixel = 0;

        private short[] streamData;

        #endregion

        #region Properties
        public Color LineColor { get; set; }

        /// <summary>
        /// The zoom level, in samples per pixel
        /// </summary>
        public int SamplesPerPixel
        {
            get { return samplesPerPixel; }
            set
            {
                if (value < 1)
                    samplesPerPixel = 1;
                else
                    samplesPerPixel = value;
                Draw();
            }
        }

        /// <summary>
        /// Start position (currently in bytes)
        /// </summary>
        public long StartPosition { get; set; }

        private bool isLoading;

        public bool IsLoading
        {
            get { return isLoading; }
            set
            {
                isLoading = value;
                //App.Current.Dispatcher.Invoke(() =>
                //    {
                //        if (value) LoadingPanel.Visibility = System.Windows.Visibility.Visible; else LoadingPanel.Visibility = System.Windows.Visibility.Collapsed;
                //    });
            }
        }

        #endregion

        public WaveFormViewer()
        {
            this.SizeChanged += OnSizeChanged;
            InitializeComponent();
        }

        #region Events
        private void mouseDown(object sender, MouseEventArgs e)
        {
            var element = (UIElement)sender;
            dragStart = e.GetPosition(element);
            element.CaptureMouse();
            e.Handled = true;
        }

        private void mouseUp(object sender, MouseButtonEventArgs e)
        {
            var element = (UIElement)sender;
            dragStart = null;
            element.ReleaseMouseCapture();
            if ((element as Line).X1 >= this.ActualWidth)
                RemoveMarker(element as Line);
        }

        private void mouseMove(object sender, MouseEventArgs args)
        {
            if (dragStart != null && args.LeftButton == MouseButtonState.Pressed)
            {
                var element = (UIElement)sender;
                var p2 = args.GetPosition(this);
                (element as Line).X1 = (element as Line).X2 = p2.X;
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            bitmap = null;
            bitmap = BitmapFactory.New((int)this.ActualWidth, (int)this.ActualHeight);

            mainCanvas.Source = bitmap;
            Draw();
        }

        protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            var x = (int)e.GetPosition(this).X;
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    StartPosition = StartPosition + (int)e.GetPosition(this).X * SamplesPerPixel;
                    StartPosition -= (int)this.ActualWidth * samplesPerPixel / 2;
                    if (StartPosition < 0)
                        StartPosition = 0;
                    SamplesPerPixel /= 2;
                }
                else
                    AddNewMarker(x);
            }
            else if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                StartPosition = 0;
                SamplesPerPixel = (int)(streamData.Length / this.ActualWidth);
                Draw();
            }
        }
        #endregion

        #region Dependency properties
        public WaveStream WaveStream
        {
            get { return (WaveStream)GetValue(WaveStreamProperty); }
            set { SetValue(WaveStreamProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WaveStream.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WaveStreamProperty =
            DependencyProperty.Register("WaveStream", typeof(WaveStream), typeof(WaveFormViewer), new PropertyMetadata(null, (s, e) =>
            {
                if (e.NewValue == null) return;
                var newValue = e.NewValue as WaveStream;

                var view = (s as WaveFormViewer);
                if (view == null) return;
                view.IsLoading = true;

                view.waveStream = newValue;
                if (newValue != null)
                {
                    view.samplesPerPixel = (int)(newValue.Length / (view.ActualWidth * 2));
                    view.ReadStream().ContinueWith(a => view.Draw());
                }
                view.IsLoading = false;
            }));
        #endregion

        #region Private methods
        private void enableDrag(UIElement element)
        {
            element.MouseDown += mouseDown;
            element.MouseMove += mouseMove;
            element.MouseUp += mouseUp;
        }

        private void AddNewMarker(int position)
        {
            var newLine = new Line()
            {
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFED262")),
                StrokeThickness = 2,
                X1 = position,
                Y1 = 0,
                X2 = position,
                Y2 = (int)this.ActualHeight,
                Cursor = System.Windows.Input.Cursors.SizeWE
            };

            enableDrag(newLine);
            markers.Children.Add(newLine);
        }

        private void RemoveMarker(Line marker)
        {
            if (markers.Children.Contains(marker))
                markers.Children.Remove(marker);
        }

        private Task ReadStream()
        {
            return Task.Run(() =>
                {
                    if (waveStream != null)
                    {
                        streamData = new short[waveStream.Length / 2];
                        waveStream.Position = 0;

                        int bytesRead;
                        var waveData = new byte[8192];
                        int pos = 0;
                        while ((bytesRead = waveStream.Read(waveData, 0, 8192)) == 8192)
                        {
                            for (int n = 0; n < bytesRead; n += 2)
                                streamData[pos++] = BitConverter.ToInt16(waveData, n);
                        }

                        waveStream.Dispose();
                        waveStream = null;
                    }
                });
        }

        private void Draw()
        {
            IsLoading = true;
            Task.Run(() =>
                {
                    if (streamData == null) return;

                    App.Current.Dispatcher.Invoke(() =>
                        {
                            bitmap.Lock();
                            bitmap.Clear();
                            bitmap.Unlock();
                        });

                    var points = new ConcurrentBag<Tuple<int, int, int, int>>();

                    Parallel.For(0, (int)this.ActualWidth, x =>
                    {
                        short low = 0;
                        short high = 0;

                        if (((StartPosition / samplesPerPixel) + x + 1) * samplesPerPixel > streamData.Length)
                            return;
                        for (int n = 0; n < samplesPerPixel; n += 1)
                        {
                            short sample = streamData[StartPosition + (x * samplesPerPixel) + n];
                            if (sample < low) low = sample;
                            if (sample > high) high = sample;
                        }
                        float lowPercent = ((((float)low) - short.MinValue) / ushort.MaxValue);
                        float highPercent = ((((float)high) - short.MinValue) / ushort.MaxValue);

                        points.Add(new Tuple<int, int, int, int>((int)x, (int)(this.ActualHeight * lowPercent), (int)x, (int)(this.ActualHeight * highPercent)));
                    });
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        using (bitmap.GetBitmapContext())
                            foreach (var point in points)
                                bitmap.DrawLine(point.Item1, point.Item2, point.Item3, point.Item4, LineColor);
                    });
                })
                .ContinueWith(a => IsLoading = false);
        }
        #endregion

        #region Public methods

        #endregion
    }
}
