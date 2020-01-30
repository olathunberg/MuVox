using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TTech.MuVox.UI_Features.Helpers
{
    public static class TimeMarkerHelper
    {
        public static IEnumerable<UIElement> CalcTimeMarkers(int averageBytesPerSecond, int startPosition, int actualWidth, int actualHeight, double samplesPerPixel, Color lineColor)
        {
            var startTime = TimeHelper.PositionToTime(0, averageBytesPerSecond, startPosition, samplesPerPixel);
            var endTime = TimeHelper.PositionToTime(actualWidth, averageBytesPerSecond, startPosition, samplesPerPixel);

            var scale = (int)Math.Round((endTime - startTime) * 100m / actualWidth, MidpointRounding.AwayFromZero) / 5;
            scale -= scale % 100;

            if (scale == 0)
                scale = 1;

            var firstMark = scale - (startTime % scale);
            for (int i = 0; i < (endTime - startTime) / scale; i++)
            {
                var x = TimeHelper.TimeToPosition(firstMark + i * scale, averageBytesPerSecond, startPosition, samplesPerPixel);
                yield return NewTimeMark(x, 5, lineColor, actualHeight);
            }

            scale *= 10;
            firstMark = scale - (startTime % scale);
            for (int i = 0; i < (endTime - startTime) / scale; i++)
            {
                var x = TimeHelper.TimeToPosition(firstMark + i * scale, averageBytesPerSecond, startPosition, samplesPerPixel);
                string timeText = GetTimeText((firstMark + i * scale) / scale);

                yield return NewTimeLabel(x, timeText, lineColor, actualHeight);
                yield return NewTimeMark(x, 10, lineColor, actualHeight);
            }
        }

        private static string GetTimeText(int tenthsOfSecond)
        {
            if (tenthsOfSecond > 600)
                return $"{tenthsOfSecond / 600}m";

            return $"{tenthsOfSecond}s";
        }


        private static Label NewTimeLabel(int x, string content, Color lineColor, int actualHeight)
        {
            return new Label
            {
                Foreground = new SolidColorBrush(lineColor),
                SnapsToDevicePixels = true,
                Content = content,
                Margin = new Thickness(x - 7, actualHeight - 30, 0, 0),
                Cursor = Cursors.None
            };
        }

        private static Line NewTimeMark(int x, int height, Color lineColor, int actualHeight)
        {
            return new Line
            {
                Stroke = new SolidColorBrush(lineColor),
                SnapsToDevicePixels = true,
                StrokeThickness = 1,
                X1 = x,
                Y1 = actualHeight - height,
                X2 = x,
                Y2 = actualHeight,
                Cursor = Cursors.None
            };
        }
    }

}
