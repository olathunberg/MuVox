using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Command;
using NAudio.Wave;

namespace TTech.Muvox.UI_Features.WaveFormViewer
{
    /// <summary>
    /// Interaction logic for WaveFormViewer.xaml
    ///
    /// All time in 1/10 s
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
            this.Loaded += WaveFormViewer_Loaded;
            InitializeComponent();
        }

        #region Commands
        private RelayCommand deleteSelectedMarkerCommand;
        public ICommand DeleteSelectedMarker
        {
            get
            {
                return deleteSelectedMarkerCommand ?? (deleteSelectedMarkerCommand = new RelayCommand(
                    () =>
                    {
                        var selectedLine = markers.Children.OfType<Line>().FirstOrDefault(x => x.Stroke == Brushes.Red);
                        if (selectedLine != null)
                        {
                            RemoveMarker(selectedLine);
                            var mark = (int)SelectedPosition;
                            RemoveFromMarkersCollection(mark);
                        }
                    },
                    () => true));
            }
        }
        #endregion

        #region Events
        private void WaveFormViewer_Loaded(object sender, RoutedEventArgs e)
        {
            var window = FindVisualAncestorOfType<UserControl>(this);
            window.InputBindings.Add(new KeyBinding(DeleteSelectedMarker, Key.Delete, ModifierKeys.None));
        }

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

        private new void MouseDown(object sender, MouseEventArgs e)
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

        private new void MouseMove(object sender, MouseEventArgs args)
        {
            if (dragStart != null && args.LeftButton == MouseButtonState.Pressed)
            {
                var element = (UIElement)sender;
                var p2 = args.GetPosition(this);
                (element as Line).X1 = (element as Line).X2 = p2.X;
            }
        }

        private new void MouseUp(object sender, MouseButtonEventArgs e)
        {
            var element = (UIElement)sender;
            element.ReleaseMouseCapture();

            if ((element as Line).X1 >= this.ActualWidth)
            {
                RemoveMarker(element as Line);
                if (dragStart != null)
                {
                    var mark = (int)PositionToTime(dragStart.Value.X);
                    RemoveFromMarkersCollection(mark);
                }
            }
            else
            {
                if (dragStart != null)
                {
                    var mark = (int)PositionToTime(dragStart.Value.X);
                    RemoveFromMarkersCollection(mark);
                }
            }

            dragStart = null;
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        private T FindVisualAncestorOfType<T>(DependencyObject d) where T : DependencyObject
        {
            for (var parent = VisualTreeHelper.GetParent(d); parent != null; parent = VisualTreeHelper.GetParent(parent))
            {
                if (parent is T result)
                    return result;
            }
            return null;
        }

        private void AddNewMarker(int position)
        {
            var newLine = new Line
            {
                Stroke = Brushes.Red,
                SnapsToDevicePixels = true,
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

            EnableDrag(newLine);
            markers.Children.Add(newLine);
        }

        private void Draw()
        {
            IsLoading = true;
            Task.Run(() =>
                {
                    if (streamData == null) return;

                    var points = new ConcurrentBag<(int, int, int, int)>();

                    if (samplesPerPixel == 0)
                        samplesPerPixel = (int)(streamData.Length / this.ActualWidth);

                    Parallel.For(0, (int)this.ActualWidth, x =>
                    {
                        short low = 0;
                        short high = 0;

                        if (((StartPosition / samplesPerPixel) + x + 1) * samplesPerPixel > streamData.Length)
                            return;
                        short sample = 0;
                        for (int n = 0; n < samplesPerPixel; n += 1)
                        {
                            sample = streamData[StartPosition + (x * samplesPerPixel) + n];
                            if (sample < low) low = sample;
                            if (sample > high) high = sample;
                        }
                        float lowPercent = ((((float)low) - short.MinValue) / ushort.MaxValue);
                        float highPercent = ((((float)high) - short.MinValue) / ushort.MaxValue);

                        points.Add((x, (int)(this.ActualHeight * lowPercent), x, (int)(this.ActualHeight * highPercent)));
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
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            DrawMarkers();
                            DrawTimeMarkers();
                        });
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
            SelectedPosition = 0;
        }

        private void DrawTimeMarkers()
        {
            var startTime = (int)PositionToTime(0);
            var endTime = (int)PositionToTime((int)this.ActualWidth);

            var scale = (int)Math.Round((endTime - startTime) * 100 / this.ActualWidth, MidpointRounding.AwayFromZero) / 5;
            scale = scale - scale % 10;

            if (scale == 0)
                scale += 1;

            timeMarks.Children.Clear();
            var firstMark = scale - (startTime % scale);
            for (int i = 0; i < (endTime - startTime) / scale; i++)
                timeMarks.Children.Add(NewTimeMark(TimeToPosition(firstMark + i * scale), 5));

            scale *= 10;
            firstMark = scale - (startTime % scale);
            for (int i = 0; i < (endTime - startTime) / scale; i++)
            {
                var x = TimeToPosition(firstMark + i * scale);
                timeMarks.Children.Add(NewTimeLabel(x, GetTimeText((firstMark + i * scale) / scale)));
                timeMarks.Children.Add(NewTimeMark(x, 10));
            }
        }

        private string GetTimeText(int tenthsOfSecond)
        {
            if (tenthsOfSecond > 600)
                return $"{tenthsOfSecond / 600}m";

            return $"{tenthsOfSecond * 10}s";
        }

        private Label NewTimeLabel(int x, string content)
        {
            return new Label
            {
                Foreground = new SolidColorBrush(LineColor),
                SnapsToDevicePixels = true,
                Content = content,
                Margin = new Thickness(x - 7, this.ActualHeight - 30, 0, 0),
                Cursor = Cursors.None
            };
        }

        private Line NewTimeMark(int x, int height)
        {
            return new Line
            {
                Stroke = new SolidColorBrush(LineColor),
                SnapsToDevicePixels = true,
                StrokeThickness = 1,
                X1 = x,
                Y1 = (int)this.ActualHeight - height,
                X2 = x,
                Y2 = (int)this.ActualHeight,
                Cursor = Cursors.None
            };
        }

        private void EnableDrag(UIElement element)
        {
            element.MouseDown += MouseDown;
            element.MouseMove += MouseMove;
            element.MouseUp += MouseUp;
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

        private void RemoveFromMarkersCollection(int mark)
        {
            if (MarkersCollection.Contains(mark))
                MarkersCollection.Remove(mark);
            else if (MarkersCollection.Contains(mark + 1))
                MarkersCollection.Remove(mark + 1);
            else if (MarkersCollection.Contains(mark - 1))
                MarkersCollection.Remove(mark - 1);
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
