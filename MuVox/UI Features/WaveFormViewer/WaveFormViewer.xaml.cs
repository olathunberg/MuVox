using GalaSoft.MvvmLight.Command;
using NAudio.Wave;
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
using TTech.MuVox.Features.Marker;
using TTech.MuVox.UI_Features.Helpers;

namespace TTech.MuVox.UI_Features.WaveFormViewer
{
    /// <summary>
    /// Interaction logic for WaveFormViewer.xaml
    ///
    /// All time in 1/10 s
    /// </summary>
    public partial class WaveFormViewer : UserControl, INotifyPropertyChanged
    {
        private const string markerColor = "#FFFED262";

        private Point? dragStart = null;
        private bool isLoading;
        private int samplesPerPixel = 0;
        private short[]? streamData;
        private WaveStream? waveStream;
        private WriteableBitmap? bitmap;
        private int averageBytesPerSecond;

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
        private RelayCommand? deleteSelectedMarkerCommand;
        public ICommand DeleteSelectedMarker => deleteSelectedMarkerCommand ?? (deleteSelectedMarkerCommand = new RelayCommand(
            () =>
            {
                var selectedLine = markers.Children.OfType<Line>().FirstOrDefault(x => x.Stroke == Brushes.Red);
                if (selectedLine != null)
                {
                    RemoveMarker(selectedLine);
                    RemoveFromMarkersCollection(selectedLine.Tag as Marker);
                    UpdateRemovalShadows();
                }
            },
            () => true));

        private RelayCommand? changeMarkerType;
        public ICommand ChangeMarkerType => changeMarkerType ?? (changeMarkerType = new RelayCommand(
            () =>
            {
                var selectedLine = markers.Children.OfType<Line>().FirstOrDefault(x => x.Stroke == Brushes.Red);
                if (selectedLine != null)
                {
                    if (selectedLine.Tag is Marker marker)
                    {
                        switch (marker.Type)
                        {
                            case Marker.MarkerType.Mark:
                                marker.Type = Marker.MarkerType.RemoveBefore;
                                break;
                            case Marker.MarkerType.RemoveBefore:
                                marker.Type = Marker.MarkerType.RemoveAfter;
                                break;
                            case Marker.MarkerType.RemoveAfter:
                                marker.Type = Marker.MarkerType.Mark;
                                break;
                            default:
                                break;
                        }
                        UpdateRemovalShadows();
                    }
                }
            },
            () => true));
        #endregion

        #region Events
        private void WaveFormViewer_Loaded(object sender, RoutedEventArgs e)
        {
            var window = FindVisualAncestorOfType<UserControl>(this);
            if (window != null)
            {
                window.InputBindings.Add(new KeyBinding(DeleteSelectedMarker, Key.Delete, ModifierKeys.None));
                window.InputBindings.Add(new KeyBinding(ChangeMarkerType, Key.F2, ModifierKeys.None));
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e == null || streamData == null || IsLoading) return;

            var x = (int)e.GetPosition(this).X;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    StartPosition += (int)e.GetPosition(this).X * SamplesPerPixel;
                    samplesPerPixel /= 2;
                    StartPosition -= (int)this.ActualWidth * samplesPerPixel / 2;
                    if (StartPosition < 0)
                        StartPosition = 0;

                    Draw();
                }
                else
                {
                    var mark = new Marker(PositionToTime(x), Marker.MarkerType.Mark);

                    MarkersCollection.Add(mark);
                    AddNewMarker(mark);
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

            if (element is Line line)
                line.Stroke = Brushes.Red;
            if (dragStart != null)
            {
                SelectedPosition = PositionToTime(dragStart.Value.X);
            }
            e.Handled = true;
        }

        private new void MouseMove(object sender, MouseEventArgs args)
        {
            if (dragStart != null && args.LeftButton == MouseButtonState.Pressed)
            {
                var element = (UIElement)sender;
                var p2 = args.GetPosition(this);
                if (element is Line line)
                {
                    line.X1 = line.X2 = p2.X;
                    if (line.Tag is Marker marker)
                    {
                        marker.Time = PositionToTime(line.X1);
                    }
                    UpdateRemovalShadows();
                }
            }
        }

        private new void MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is UIElement element)
            {
                element.ReleaseMouseCapture();

                if (element is Line line)
                {
                    if (line.X1 >= this.ActualWidth || line.X1 < 0)
                    {
                        if (dragStart != null)
                        {
                            RemoveMarker(line);
                            RemoveFromMarkersCollection(line.Tag as Marker);
                        }
                    }
                    else
                    {
                        RemoveFromMarkersCollection(line.Tag as Marker);
                        MarkersCollection.Add(line.Tag as Marker);
                    }
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
            DependencyProperty.Register("MarkersCollection", typeof(ObservableCollection<Marker>), typeof(WaveFormViewer), new PropertyMetadata(null, (s, e) =>
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
                    view.ReadStream().ContinueWith(a => view.Draw(), TaskScheduler.FromCurrentSynchronizationContext());
            }));

        public ObservableCollection<Marker> MarkersCollection
        {
            get { return (ObservableCollection<Marker>)GetValue(MarkersCollectionProperty); }
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
            return default;
        }

        private void AddNewMarker(Marker marker)
        {
            var position = TimeToPosition(marker);
            var newLine = new Line
            {
                Stroke = Brushes.Red,
                SnapsToDevicePixels = true,
                StrokeThickness = 2,
                X1 = position,
                Y1 = 0,
                X2 = position,
                Y2 = (int)this.ActualHeight,
                Cursor = Cursors.SizeWE,
                Tag = marker
            };

            foreach (var item in markers.Children.OfType<Line>())
                item.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(markerColor));

            SelectedPosition = marker.Time;

            EnableDrag(newLine);
            markers.Children.Add(newLine);

            AddRemovalShadow(marker);
        }

        private void UpdateRemovalShadows()
        {
            foreach (var line in markers.Children.OfType<Line>())
            {
                if (line.Tag is Marker marker)
                {
                    var shadow = removalShadows.Children.OfType<Rectangle>().FirstOrDefault(x => x.Tag == marker);
                    if (shadow == null)
                    {
                        if (marker.Type != Marker.MarkerType.Mark)
                        {
                            AddRemovalShadow(marker);
                        }
                        continue;
                    }

                    shadow.Fill = CreateShadowFill(marker);

                    Canvas.SetTop(shadow, 0);

                    if (marker.Type == Marker.MarkerType.RemoveBefore)
                    {
                        shadow.Width = TimeToPosition(marker) - 1;

                        var left = MarkersCollection.Where(x => x.Time < marker.Time).OrderBy(x => x.Time).LastOrDefault();
                        if (left == null)
                        {
                            Canvas.SetLeft(shadow, 0);
                        }
                        else
                        {
                            Canvas.SetLeft(shadow, TimeToPosition(left));
                            shadow.Width -= TimeToPosition(left);
                        }
                    }
                    else if (marker.Type == Marker.MarkerType.RemoveAfter)
                    {
                        Canvas.SetLeft(shadow, TimeToPosition(marker) + 1);
                        shadow.Width = (int)this.ActualWidth - TimeToPosition(marker) - 1;

                        var right = MarkersCollection.Where(x => x.Time > marker.Time).OrderBy(x => x.Time).FirstOrDefault();
                        if (right != null)
                        {
                            shadow.Width = TimeToPosition(right) - TimeToPosition(marker) - 1;
                        }
                    }
                    else
                    {
                        RemoveShadow(shadow);
                    }
                }
            }
        }

        private int TimeToPosition(Marker marker)
        {
            return TimeHelper.TimeToPosition(marker.Time, averageBytesPerSecond, (int)StartPosition, SamplesPerPixel);
        }
   
        private int PositionToTime(double x)
        {
            return TimeHelper.PositionToTime(x, averageBytesPerSecond, (int)StartPosition, SamplesPerPixel);
        }

        private void AddRemovalShadow(Marker marker)
        {
            var shadow = new Rectangle
            {
                Fill = CreateShadowFill(marker),
                StrokeThickness = 0,
                SnapsToDevicePixels = true,
                Height = (int)this.ActualHeight,
                Opacity = 0.5f,

                Tag = marker
            };

            removalShadows.Children.Add(shadow);

            UpdateRemovalShadows();
        }

        private Brush CreateShadowFill(Marker marker)
        {
            if (marker.Type == Marker.MarkerType.RemoveAfter)
                return new LinearGradientBrush(Colors.LightSeaGreen, Colors.Transparent, 0);
            else
                return new LinearGradientBrush(Colors.Transparent, Colors.LightSeaGreen, 0);
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

                        if (streamData == null)
                            return;

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
                    }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void DrawMarkers()
        {
            var startTime = PositionToTime(0);
            var endTime = PositionToTime((int)this.ActualWidth);

            markers.Children.Clear();
            foreach (var mark in MarkersCollection)
            {
                if (mark.Time <= endTime && mark.Time >= startTime)
                    AddNewMarker(mark);
            }
            foreach (var item in markers.Children.OfType<Line>())
                item.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(markerColor));
            SelectedPosition = 0;
        }

        private void DrawTimeMarkers()
        {
            timeMarks.Children.Clear();

            foreach (var item in TimeMarkerHelper.GenerateTimeMarkers(averageBytesPerSecond, (int)StartPosition, (int)ActualWidth, (int)ActualHeight, SamplesPerPixel, LineColor))
            {
                timeMarks.Children.Add(item);
            }
        }

        private void EnableDrag(UIElement element)
        {
            element.MouseDown += MouseDown;
            element.MouseMove += MouseMove;
            element.MouseUp += MouseUp;
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
            {
                markers.Children.Remove(marker);
                RemoveShadow(marker.Tag as Rectangle);
            }
        }

        private void RemoveShadow(Rectangle shadow)
        {
            if (removalShadows.Children.Contains(shadow))
                removalShadows.Children.Remove(shadow);
        }

        private void RemoveFromMarkersCollection(Marker mark)
        {
            if (MarkersCollection.Contains(mark))
                MarkersCollection.Remove(mark);
        }
        #endregion

        #region Eventhandlers
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion
    }
}
