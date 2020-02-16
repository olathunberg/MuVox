using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTech.MuVox.Features.LogViewer;

namespace MuVox.Testing.Features
{
    [TestClass]
    public class LogViewerTests
    {
        [TestMethod]
        public void AddEntry_String_StoresAddedEntry()
        {
            var model = new LogViewerModel();

            model.Add("Testmessage");

            Assert.IsTrue(model.Entries != null);
            Assert.IsTrue(model.Entries.Count == 1);
        }

        [TestMethod]
        public void AddEntry_Null_StoresNoEntry()
        {
            var model = new LogViewerModel();

            model.Add(null);
        }
    }
}
