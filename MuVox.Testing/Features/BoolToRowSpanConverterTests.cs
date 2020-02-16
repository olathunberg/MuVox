using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTech.MuVox.Features.Recorder;

namespace MuVox.Testing.Features
{
    [TestClass]
    public class BoolToRowSpanConverterTests
    {
        private BoolToRowSpanConverter converter;

        [TestInitialize]
        public void TestInitialize()
        {
            converter = new BoolToRowSpanConverter();
        }

        [TestMethod]
        public void Convert_Null_1()
        {
            var rowSpan = converter.Convert(null, null, null, null);

            Assert.AreEqual(1, rowSpan);
        }

        [TestMethod]
        public void Convert_False_1()
        {
            var rowSpan = converter.Convert(false, null, null, null);

            Assert.AreEqual(1, rowSpan);
        }

        [TestMethod]
        public void Convert_True_3()
        {
            var rowSpan = converter.Convert(true, null, null, null);

            Assert.AreEqual(3, rowSpan);
        }
    }
}
