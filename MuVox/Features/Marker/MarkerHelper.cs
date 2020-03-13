using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TTech.MuVox.Features.Marker
{
    public static class MarkerHelper
    {
        public static bool HasMarkerFile(string baseFilename)
        {
            var markerFile = GetMarkerFilename(baseFilename);
            return File.Exists(markerFile);
        }

        public static void AddMarkerToFile(string baseFilename, Marker mark)
        {
            var markerFile = GetMarkerFilename(baseFilename);

            if (!HasMarkerFile(baseFilename))
            {
                using (var sw = File.CreateText(markerFile))
                    sw.WriteLine(mark.ToString());
            }
            else
            {
                using (var sw = File.AppendText(markerFile))
                    sw.WriteLine(mark.ToString());
            }
        }

        public static List<Marker> GetMarkersFromFile(string filename)
        {
            var markerFile = GetMarkerFilename(filename);
            var markers = new List<Marker>();
            if (!HasMarkerFile(filename))
                return markers;

            using (var file = File.OpenText(markerFile))
            {
                string? line = null;
                while ((line = file.ReadLine()) != null)
                {
                    markers.Add(Marker.Parse(line));
                }
            }

            return markers;
        }

        public static void CreateFileFromList(string baseFilename, IList<Marker> markers)
        {
            var markerFile = GetMarkerFilename(baseFilename);
            using (var sw = File.CreateText(markerFile))
            {
                foreach (var mark in markers.OrderBy(x => x))
                    sw.WriteLine(mark.ToString());
            }
        }

        private static string GetMarkerFilename(string baseFilename)
        {
            return Path.ChangeExtension(baseFilename, ".markers");
        }
    }
}
