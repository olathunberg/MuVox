using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using TTech.MuVox.UI.Converters;

namespace MuVox.Testing.UI
{
    [TestClass]
    public class BooleanToVisibilityConverterTests
    {
        private BooleanToVisibilityConverter converter;

        [TestInitialize]
        public void TestInitialize()
        {
            converter = new BooleanToVisibilityConverter();
        }

        [TestMethod]
        public void Convert_Null_Collapsed()
        {
            var visibility = converter.Convert(null, null, null, null);

            Assert.AreEqual(Visibility.Collapsed, visibility);
        }

        [TestMethod]
        public void Convert_False_Collapsed()
        {
            var visibility = converter.Convert(false, null, null, null);

            Assert.AreEqual(Visibility.Collapsed, visibility);
        }

        [TestMethod]
        public void Convert_True_Visible()
        {
            var visibility = converter.Convert(true, null, null, null);

            Assert.AreEqual(Visibility.Visible, visibility);
        }
    }
}
