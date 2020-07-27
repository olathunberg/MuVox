using System;

namespace TTech.MuVox.Core
{
    public class Marker : IComparable
    {
        public Marker(int time, MarkerType type)
        {
            Time = time;
            Type = type;
        }

        public int Time { get; set; }

        public MarkerType Type { get; set; }

        public static Marker Parse(string data)
        {
            if (string.IsNullOrEmpty(data))
                throw new ArgumentNullException(nameof(data));

            var split = data.Split('|');
            if (split.Length != 2)
            {
                if (!int.TryParse(data, out var mark))
                    throw new ArgumentException($"{data} malformed");

                return new Marker(mark, MarkerType.Mark);
            }

            if (!int.TryParse(split[0], out var time))
                throw new ArgumentException($"{data} malformed");

            if (!MarkerType.TryParse<MarkerType>(split[1], out var markerType))
                throw new ArgumentException($"{data} malformed");

            return new Marker(time, markerType);
        }

        public override string ToString()
        {
            return $"{Time}|{Type.ToString("f")}";
        }

        public override bool Equals(object obj)
        {
            if (obj is Marker marker)
            {
                return marker.Time == this.Time && marker.Type == this.Type;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Time.GetHashCode() ^ Type.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            if (obj is Marker marker)
            {
                return Time.CompareTo(marker.Time);
            }

            return 0;
        }

        public enum MarkerType
        {
            Mark,
            RemoveBefore,
            RemoveAfter
        }
    }
}
