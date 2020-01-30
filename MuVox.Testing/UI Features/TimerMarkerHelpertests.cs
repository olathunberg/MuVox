using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Windows.Media;
using TTech.MuVox.UI_Features.Helpers;

namespace MuVox.Testing.UI_Features
{
    [TestClass]
    public class TimerMarkerHelperTests
    {
        [TestMethod]
        public void CalcTimeMarkers_ReturnsMarks()
        {
            var markers = TimeMarkerHelper.CalcTimeMarkers(0, 345);

            Assert.AreEqual(31, markers.Count());
        }

        [TestMethod]
        public void GenerateTimeMarkers()
        {
            var markers = TimeMarkerHelper.GenerateTimeMarkers(12, 10, 543, 231, 1, Colors.Lavender);

            Assert.AreEqual(35, markers.Count());
        }
    }
}
