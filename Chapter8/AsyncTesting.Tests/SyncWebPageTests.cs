using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsyncTesting.Tests
{
    [TestClass]
    public class SyncWebPageTests
    {
        [TestMethod]
        public void Url_PropertyIsChanged_ShouldDownloadNewPageContent()
        {
            string expectedPageContent = "<html><i>Dummy content</i></html>";

            var sut = new SynchronousWebPage(uri => expectedPageContent);

            sut.Url = "http://dummy.com";

            //        sut.DocumentChangingTask.Wait();

            Assert.AreEqual(expectedPageContent, sut.Document);

        }
         
    }
}