using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bag
{
    public class ParallelFileFinderWithBag
    {
        public static List<FileInfo> FindAllFiles(string path, string match)
        {

            var fileTasks = new List<Task<List<FileInfo>>>();

            var directories = new ConcurrentBag<DirectoryInfo>();
            
            foreach (DirectoryInfo dir in new DirectoryInfo(path).GetDirectories())
            {
                fileTasks.Add(Task.Run<List<FileInfo>>(() => Find(dir, directories, match)));
            }

            return (from fileTask in fileTasks
                    from file in fileTask.Result
                    select file).ToList();
        }
        
        private static List<FileInfo> Find(DirectoryInfo dir, ConcurrentBag<DirectoryInfo> directories, string match)
        {
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