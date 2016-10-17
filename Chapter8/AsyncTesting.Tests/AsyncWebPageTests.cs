using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsyncTesting.Tests
{
    public static class AsyncStubs
    {
        public static Task<T> FromResult<T>(T result)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(result);

            return tcs.Task;
        }

        public static Task<T> FromException<T>(Exception e)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetException(e);

            return tcs.Task;
        }
    }

    [TestClass]
    public class AsyncWebPageTests
    {
        [TestMethod]
        public void Url_PropertyIsChanged_ShouldDownloadNewPageContent()
        {
            string expectedPageContent = "<html><i>Dummy content</i></html>";

            var sut = new AsyncWebPage(uri => AsyncStubs.FromResult(expectedPageContent));

            sut.Url = "http://dummy.com";
            
            Assert.AreEqual(expectedPageContent,sut.Document);
            
        }
    }
}
