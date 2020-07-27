using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TTech.MuVox.UI.Helpers
{
    public static class TimeMarkerHelper
    {
        public static IEnumerable<UIElement> GenerateTimeMarkers(int averageBytesPerSecond, int startPosition, int actualWidth, int actualHeight, double samplesPerPixel, Color lineColor)
        {
            var startTime = TimeHelper.PositionToTime(0, averageBytesPerSecond, startPosition, samplesPerPixel);
            var endTime = TimeHelper.PositionToTime(actualWidth, averageBytesPerSecond, startPosition, samplesPerPixel);

            var timeMarkers = CalcTimeMarkers(startTime, endTime);

            foreach (var mark in timeMarkers)
            {
                var x = TimeHelper.TimeToPosition(mark.time, averageBytesPerSecond, startPosition, samplesPerPixel);

                if (mark.index % 10 != 0)
                    yield return NewTimeMark(x, 5, lineColor, actualHeight);
                else
                {
                    string timeText = GetTimeText(mark.time);

                    yield return NewTimeLabel(x, timeText, lineColor, actualHeight);
                    yield return NewTimeMark(x, 10, lineColor, actualHeight);
                }
            }
        }

        public static IEnumerable<(int index, int time)> CalcTimeMarkers(int startTime, int endTime)
        {
            var inc = (endTime - startTime) / 32.0;
            var index = 0;
            for (var i = startTime + inc; i < endTime; i += inc)
            {
                yield return (index++, (int)i);
            }
        }

        private static string GetTimeText(int tenthsOfSecond)
        {
            if (tenthsOfSecond > 600)
                return $"{tenthsOfSecond / 600}m";

            return $"{tenthsOfSecond / 10}s";
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
