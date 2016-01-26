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
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.IO;

namespace RecordToMP3.UI_Features.WaveFormViewer
{
    /// <summary>
    /// Interaction logic for WaveFormViewer.xaml
    /// </summary>
    public partial class WaveFormViewer : UserControl, INotifyPropertyChanged
    {
        private const string markerColor = "#FFFED262";

        #region Fields
        private Point? dragStart = null;

        private bool isLoading;

        private int samplesPerPixel = 0;

        private short[] streamData;

        private WaveStream waveStream;

        private WriteableBitmap bitmap { get; set; }

        private int averageBytesPerSecond;
        #endregion

        #region Properties
        public bool IsLoading
        {
            get { return isLoading; }
            set
            {
                isLoading = value;
                NotifyPropertyChanged();
            }
        }

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
        #endregion

        public WaveFormViewer()
        {
            this.SizeChanged += OnSizeChanged;
            InitializeComponent();
        }

        #region Events
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (streamData == null || IsLoading) return;

            var x = (int)e.GetPosition(this).X;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    StartPosition = StartPosition + (int)e.GetPosition(this).X * SamplesPerPixel;
                    samplesPerPixel /= 2;
                    StartPosition -= (int)this.ActualWidth * samplesPerPixel / 2;
                    if (StartPosition < 0)
                        StartPosition = 0;

                    Draw();
                }
                else
                {
                    var mark = PositionToTime(x);
                    MarkersCollection.Add((int)mark);

                    AddNewMarker(x);
                }
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                StartPosition = 0;
                SamplesPerPixel = (int)(streamData.Length / this.ActualWidth);
                Draw();
            }
        }

        private void mouseDown(object sender, MouseEventArgs e)
        {
            var element = (UIElement)sender;
            dragStart = e.GetPosition(element);
            element.CaptureMouse();

            foreach (var item in markers.Children.OfType<Line>())
                item.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(markerColor));

            (element as Line).Stroke = Brushes.Red;
            if (dragStart != null)
                SelectedPosition = PositionToTime(dragStart.Value.X);
            e.Handled = true;
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

        private void mouseUp(object sender, MouseButtonEventArgs e)
        {
            var element = (UIElement)sender;
            element.ReleaseMouseCapture();

            if ((element as Line).X1 >= this.ActualWidth)
            {
                RemoveMarker(element as Line);
                if (dragStart != null)
                {
                    var mark = (int)PositionToTime(dragStart.Value.X);
                    if (MarkersCollection.Contains(mark))
                        MarkersCollection.Remove(mark);
                    else if (MarkersCollection.Contains(mark + 1))
                        MarkersCollection.Remove(mark + 1);
                    else if (MarkersCollection.Contains(mark - 1))
                        MarkersCollection.Remove(mark - 1);
                }
            }
            else
            {
                if (dragStart != null)
                {
                    var mark = (int)PositionToTime(dragStart.Value.X);
                    if (MarkersCollection.Contains(mark))
                        MarkersCollection.Remove(mark);
                    else if (MarkersCollection.Contains(mark + 1))
                        MarkersCollection.Remove(mark + 1);
                    else if (MarkersCollection.Contains(mark - 1))
                        MarkersCollection.Remove(mark - 1);
                }
                var newMark = (int)PositionToTime((element as Line).X1);
                MarkersCollection.Add(newMark);
            }
            dragStart = null;
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            bitmap = null;
            bitmap = BitmapFactory.New((int)this.ActualWidth, (int)this.ActualHeight);

            mainCanvas.Source = bitmap;
        }
        #endregion

        #region Dependency properties
        public double SelectedPosition
        {
            get { return (double)GetValue(SelectedPositionProperty); }
            set { SetValue(SelectedPositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedPositionProperty =
            DependencyProperty.Register("SelectedPosition", typeof(double), typeof(WaveFormViewer), new PropertyMetadata(0.0));

        // Using a DependencyProperty as the backing store for MarkersCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MarkersCollectionProperty =
            DependencyProperty.Register("MarkersCollection", typeof(ObservableCollection<int>), typeof(WaveFormViewer), new PropertyMetadata(null, (s, e) =>
            {
                if (e.NewValue == null) return;
                var newValue = e.NewValue as ObservableCollection<int>;

                var view = (s as WaveFormViewer);
                if (view == null) return;
            }));

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
                    view.ReadStream().ContinueWith(a => view.Draw());
            }));

        public ObservableCollection<int> MarkersCollection
        {
            get { return (ObservableCollection<int>)GetValue(MarkersCollectionProperty); }
            set { SetValue(MarkersCollectionProperty, value); }
        }
        public WaveStream WaveStream
        {
            get { return (WaveStream)GetValue(WaveStreamProperty); }
            set { SetValue(WaveStreamProperty, value); }
        }
        #endregion

        #region Private methods
        private void AddNewMarker(int position)
        {
            var newLine = new Line
            {
                Stroke = Brushes.Red,
                StrokeThickness = 2,
                X1 = position,
                Y1 = 0,
                X2 = position,
                Y2 = (int)this.ActualHeight,
                Cursor = Cursors.SizeWE
            };

            foreach (var item in markers.Children.OfType<Line>())
                item.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(markerColor));

            SelectedPosition = PositionToTime(position);

            enableDrag(newLine);
            markers.Children.Add(newLine);
        }

        private void Draw()
        {
            IsLoading = true;
            Task.Run(() =>
                {
                    if (streamData == null) return;

                    var points = new ConcurrentBag<Tuple<int, int, int, int>>();

                    if (samplesPerPixel == 0)
                        SamplesPerPixel = (int)(streamData.Length / this.ActualWidth);

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
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        using (bitmap.GetBitmapContext())
                        {
                            bitmap.Clear();
                            foreach (var point in points)
                                bitmap.DrawLine(point.Item1, point.Item2, point.Item3, point.Item4, LineColor);
                        }
                    });
                })
                .ContinueWith(a =>
                    {
                        Application.Current.Dispatcher.Invoke(() => DrawMarkers());
                        IsLoading = false;
                    });
        }

        private void DrawMarkers()
        {
            var startTime = (int)PositionToTime(0);
            var endTime = (int)PositionToTime((int)this.ActualWidth);

            markers.Children.Clear();
            foreach (var mark in MarkersCollection)
            {
                if (mark <= endTime && mark >= startTime)
                    AddNewMarker(TimeToPosition(mark));
            }
            foreach (var item in markers.Children.OfType<Line>())
                item.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(markerColor));
        }

        private void enableDrag(UIElement element)
        {
            element.MouseDown += mouseDown;
            element.MouseMove += mouseMove;
            element.MouseUp += mouseUp;
        }

        private double PositionToTime(double position)
        {
            if (averageBytesPerSecond == 0)
                return 0;

            return ((StartPosition + position * SamplesPerPixel) * 2) / (averageBytesPerSecond / 10);
        }

        private Task ReadStream()
        {
            return Task.Run(() =>
                {
                    if (waveStream != null)
                    {
                        streamData = new short[waveStream.Length / 2];
                        waveStream.Position = 0;
                        averageBytesPerSecond = waveStream.WaveFormat.AverageBytesPerSecond;

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

        private void RemoveMarker(Line marker)
        {
            if (markers.Children.Contains(marker))
                markers.Children.Remove(marker);
        }

        private int TimeToPosition(int mark)
        {
            if (SamplesPerPixel == 0)
                return 0;

            return (int)((((mark * (averageBytesPerSecond / 10)) / 2) - StartPosition) / SamplesPerPixel);
        }
        #endregion

        #region Public methods

        #endregion

        #region Eventhandlers
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
