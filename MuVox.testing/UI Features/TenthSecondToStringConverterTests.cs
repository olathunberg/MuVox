using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TTech.Muvox.UI_Features.Converters;

namespace MuVox.Testing.UI_Features
{
    [TestClass]
    public class TenthSecondToStringConverterTests
    {
        private TenthSecondToStringConverter converter;

        [TestInitialize]
        public void TestInitialize()
        {
            converter = new TenthSecondToStringConverter();
        }

        [TestMethod]
        public void Convert_Null_EmptyString()
        {
            var text = converter.Convert(null, null, null, null);

            Assert.AreEqual(string.Empty, text);
        }

        [TestMethod]
        public void Convert_Zero_ZeroText()
        {
            var text = converter.Convert(0, null, null, null);

            Assert.AreEqual("00:00.0", text);
        }

        [TestMethod]
        public void Convert_OneHour_String()
        {
            var text = converter.Convert(36000, null, null, null);

            Assert.AreEqual("60:00.0", text);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Convert_Negative_ThrowsException()
        {
            var text = converter.Convert(-12, null, null, null);
        }

        [TestMethod]
        public void Convert_BigNumber_String()
        {
            var text = converter.Convert(123456, null, null, null);

            Assert.AreEqual("205:45.6", text);
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void ConvertBack_ThrowsException()
        {
            var text = converter.ConvertBack(123456, null, null, null);
        }
    }
}
