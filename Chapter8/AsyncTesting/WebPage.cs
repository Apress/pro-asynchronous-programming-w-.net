using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AsyncTesting
{
    public class SynchronousWebPage
    {
        private readonly Func<string, string> pageLoader;

        public SynchronousWebPage()
        {
            pageLoader = uri =>
                {
                    using (var c = new WebClient())
                    {
                        return c.DownloadString(uri);
                    }
                };
        }

        public SynchronousWebPage(Func<string, string> pageLoader)
        {
            this.pageLoader = pageLoader;
        }

        private string url;
        public string Url
        {
            get { return url; }
            set
            {
                Document = pageLoader(url);
                url = value;
            }
        }

        public string Document { get; private set; }

    }

    public class AsyncWebPage
    {
        private readonly Func<string, Task<string>> pageLoader;

        public AsyncWebPage()
        {
            pageLoader = uri =>
            {
                using (var c = new WebClient())
                {
                    return c.DownloadStringTaskAsync(uri);
                }
            };
        }

        public AsyncWebPage(Func<string, Task<string>> pageLoader)
        {
            this.pageLoader = pageLoader;
        }

        public Task DocumentLoadingTask { get; private set; }
        private string url;
        public string Url
        {
            get { return url; }
            set
            {
                Document = null;
                
                pageLoader(url)
                    .ContinueWith(dt => Document = dt.Result,
                    TaskContinuationOptions.ExecuteSynchronously);
                
                url = value;
            }
        }

        public string Document { get; private set; }
    }
}
