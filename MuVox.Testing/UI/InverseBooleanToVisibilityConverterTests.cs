using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;
using TTech.MuVox.UI.Converters;

namespace MuVox.Testing.UI
{
    [TestClass]
    public class InverseBooleanToVisibilityConverterTests
    {
        private InverseBooleanToVisibilityConverter converter;

        [TestInitialize]
        public void TestInitialize()
        {
            converter = new InverseBooleanToVisibilityConverter();
        }

        [TestMethod]
        public void Convert_Null_Visible()
        {
            var visibility = converter.Convert(null, null, null, null);

            Assert.AreEqual(Visibility.Visible, visibility);
        }

        [TestMethod]
        public void Convert_False_Visible()
        {
            var visibility = converter.Convert(false, null, null, null);

            Assert.AreEqual(Visibility.Visible, visibility);
        }

        [TestMethod]
        public void Convert_True_Collapsed()
        {
            var visibility = converter.Convert(true, null, null, null);

            Assert.AreEqual(Visibility.Collapsed, visibility);
        }
    }
}
