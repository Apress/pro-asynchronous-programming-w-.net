using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bag
{
    public class BetterParallelFileFinderWithBag
    {
        public const int CONCURRENCY_LEVEL = 8;

        public static List<FileInfo> FindAllFiles(string path, string match)
        {
            var directories = new ConcurrentBag<DirectoryInfo>();

            return Find(new DirectoryInfo(path), directories, match,CONCURRENCY_LEVEL);
        }

        private static List<FileInfo> Find(DirectoryInfo dir, ConcurrentBag<DirectoryInfo> directories, string match,int concurrencyLevel)
        {
            var fileTasks = new List<Task<List<FileInfo>>>();

            var files = new List<FileInfo>();

            directories.Add(dir);
            DirectoryInfo dirToExamine;
            while (directories.TryTake(out dirToExamine))
            {
                foreach (DirectoryInfo subDir in dirToExamine.GetDirectories())
                {
                    if (fileTasks.Count < CONCURRENCY_LEVEL)
                    {
                        DirectoryInfo capturedSubDir = subDir;
                        fileTasks.Add(Task.Run<List<FileInfo>>(() => TaskFind(capturedSubDir,directories,match)));
                    }
                    else
                    {
                        directories.Add(subDir);
                    }
                }

                files.AddRange(dirToExamine.GetFiles(match));
            }

            return (from fileTask in fileTasks
                    from file in fileTask.Result
                    select file).Concat(files).ToList();
        }

        private static List<FileInfo> TaskFind(DirectoryInfo dir, ConcurrentBag<DirectoryInfo> directories, string match)
        {
            var fileTasks = new List<Task<List<FileInfo>>>();

            var files = new List<FileInfo>();

            directories.Add(dir);
            DirectoryInfo dirToExamine;
            while (directories.TryTake(out dirToExamine))
            {
                foreach (DirectoryInfo subDir in dirToExamine.GetDirectories())
                {
                        directories.Add(subDir);
                }

                files.AddRange(dirToExamine.GetFiles(match));
            }

            return files;
        } 
    }
}