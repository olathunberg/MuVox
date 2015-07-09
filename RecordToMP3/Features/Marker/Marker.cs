using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordToMP3.Features.Marker
{
    public class Marker
    {
        public bool HasMarkerFile(string baseFilename)
        {
            var markerFile = GetMarkerFilename(baseFilename);
            return File.Exists(markerFile);
        }

        public void AddMarkerToFile(string baseFilename, int mark)
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

        public List<int> GetMarkersFromFile(string filename)
        {
            var markerFile = GetMarkerFilename(filename);
            var markers = new List<int>();
            if (!HasMarkerFile(filename)) return markers;

            using (var file = File.OpenText(markerFile))
            {
                string line = null;
                while ((line = file.ReadLine()) != null)
                {
                    markers.Add(int.Parse(line));
                }
            }

            return markers;
        }

        public void CreateFileFromList(string baseFilename, IList<int> markers)
        {
            var markerFile = GetMarkerFilename(baseFilename);
            using (var sw = File.CreateText(markerFile))
            {
                foreach (var mark in markers.OrderBy(x => x))
                    sw.WriteLine(mark.ToString());
            }
        }

        private string GetMarkerFilename(string baseFilename)
        {
            return Path.ChangeExtension(baseFilename, ".markers");
        }
    }
}
