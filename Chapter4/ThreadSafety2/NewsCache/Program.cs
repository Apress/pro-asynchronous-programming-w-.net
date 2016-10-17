using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NewsCache
{
    public class NewsItem
    {
        public NewsItem(string item, IEnumerable<string> tags)
        {
            Item = item;
            Tags = tags;
        }
        public string Item { get; private set; }
        public IEnumerable<string> Tags { get; private set; }
    }

    public class Cache
    {
        private readonly List<NewsItem> items = new List<NewsItem>();
 
        ReaderWriterLockSlim guard = new ReaderWriterLockSlim();

        public IEnumerable<NewsItem> GetNews(string tag)
        {
            guard.EnterReadLock();
            try
            {
                return items.Where(ni => ni.Tags.Contains(tag)).ToList();
            }
            finally
            {
                guard.ExitReadLock();
            }
        }

        public void AddNewsItem(NewsItem item)
        {
            guard.EnterWriteLock();
            try
            {
                items.Add(item);
            }
            finally
            {
                guard.ExitWriteLock();
            }
        }
    }

    public class LockFreeCache
    {
        private List<NewsItem> items = new List<NewsItem>();

        readonly object writeGuard = new object();

        public IEnumerable<NewsItem> GetNews(string tag)
        {
            return items.Where(ni => ni.Tags.Contains(tag));
        }

        public void AddNewsItem(NewsItem item)
        {
            lock (writeGuard)
            {
                var copy = new List<NewsItem>(items);
                copy.Add(item);
                items = copy;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
        }
    }
}
