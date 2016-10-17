using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsyncTesting.Tests
{

    public static class TestingAsyncUtil
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
    public class WebDownloaderTests
    {
        [TestMethod]
        [Ignore]
        public void GetPages_CalledWithMultipleUrls_ShouldReturnContentFetchFromEachUrl()
        {
            string[] urls = new string[] {"http://myserver.com/", "http:anotherserver.com/"};
            string[] expectedContent = new string[] {urls[0], urls[1]};

            var sut = new WebDownloader(u => TestingAsyncUtil.FromResult<string>(u));

            string[] content = sut.GetPages(urls);
            
        
            CollectionAssert.AreEquivalent(expectedContent,content);
        }
         
    }
}