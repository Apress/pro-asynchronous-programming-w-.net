using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AsyncTesting
{
    public class WebDownloader
    {
        private readonly Func<string, Task<string>> downloadAsync;

        public WebDownloader( Func<string,Task<string>> downloadAsync)
        {
            this.downloadAsync = downloadAsync;
        }

        public WebDownloader()
        {
            downloadAsync = DownloadAsync;
        }
        
        public string[] GetPages(params string[] urls)
        {
            Task<string>[] downloadTasks =
                urls.Select(url => downloadAsync(url))
                       .ToArray();
            
            Task.WaitAll(downloadTasks);

            return downloadTasks.Select(t => t.Result).ToArray();
        }

        private Task<string> DownloadAsync(string url)
        {
            using (var client = new WebClient())
            {
                return client.DownloadStringTaskAsync(url);
            }
        }
    }
}