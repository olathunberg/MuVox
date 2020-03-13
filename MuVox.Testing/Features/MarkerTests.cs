using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TTech.MuVox.Features.Marker;

namespace MuVox.Testing.Features
{
    [TestClass]
    public class MarkerTests
    {
        [TestMethod]
        public void ToString_CorrectOutput()
        {
            var mark = new Marker(123, Marker.MarkerType.RemoveAfter);

            var toString = mark.ToString();

            Assert.AreEqual("123|RemoveAfter", toString);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Parse_Empty_Exception()
        {
            Marker.Parse("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Parse_Null_Exception()
        {
            Marker.Parse(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Parse_DecimalTimeNoType_Exception()
        {
            Marker.Parse("12,23");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Parse_DecimalTime_Exception()
        {
            Marker.Parse("12,23|Mark");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Parse_InvalidType_Exception()
        {
            Marker.Parse("1223|InvalidMark");
        }

        [TestMethod]
        public void Parse_WellFormated_Marker()
        {
            var mark = Marker.Parse("42|RemoveAfter");

            Assert.AreEqual(42, mark.Time);
            Assert.AreEqual(Marker.MarkerType.RemoveAfter, mark.Type);
        }
    }
}
