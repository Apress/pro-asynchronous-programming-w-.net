using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bag
{
    public class ParallelFindFilesWithCallStackRecursion
    {
        public static List<FileInfo> FindAllFiles(string path, string match)
        {

            var fileTasks = new List<Task<List<FileInfo>>>();

            var directories = new ConcurrentBag<DirectoryInfo>();


            foreach (DirectoryInfo dir in new DirectoryInfo(path).GetDirectories())
            {
                fileTasks.Add(Task.Run<List<FileInfo>>(() => Find(dir, match)));
            }

            return (from fileTask in fileTasks
                    from file in fileTask.Result
                    select file).ToList();
        }

        private static List<FileInfo> Find(DirectoryInfo dir, string match)
        {
            var files = new List<FileInfo>();

            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                files.AddRange(Find(subDir, match));
            }

            files.AddRange(dir.GetFiles(match));


            return files;
        } 
    }
}