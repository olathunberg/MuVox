using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TTech.MuVox.Core
{
    public static class MarkersHelper
    {
        public static bool HasMarkerFile(string baseFilename)
        {
            var markerFile = GetMarkerFilename(baseFilename);
            return File.Exists(markerFile) && File.ReadAllLines(markerFile).Length > 0;
        }

        public static void AddMarkerToFile(string baseFilename, Core.Marker mark)
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

        public static List<Core.Marker> GetMarkersFromFile(string filename)
        {
            var markerFile = GetMarkerFilename(filename);
            var markers = new List<Core.Marker>();
            if (!HasMarkerFile(filename))
                return markers;

            using (var file = File.OpenText(markerFile))
            {
                string? line = null;
                while ((line = file.ReadLine()) != null)
                {
                    markers.Add(Core.Marker.Parse(line));
                }
            }

            return markers;
        }

        public static void CreateFileFromList(string baseFilename, IList<Core.Marker> markers)
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
