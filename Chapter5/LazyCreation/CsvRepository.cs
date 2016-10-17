using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LazyCreation
{
    public class CsvRepository
    {
        private readonly string directory;
        private Dictionary<string, Lazy<List<string[]>>> csvFiles;

        public CsvRepository(string directory)
        {
            this.directory = directory;
            csvFiles = new DirectoryInfo(directory)
                .GetFiles("*.csv")
                .ToDictionary(f => f.Name, f => new Lazy<List<string[]>>(() => LoadData(f.Name).ToList()));
        }
        public IEnumerable<string> Files { get { return csvFiles.Keys; } }

        public IEnumerable<T> Map<T>(string dataFile, Func<string[], T> map)
        {
            return csvFiles[dataFile].Value.Skip(1).Select(map);
        }

        private IEnumerable<string[]> LoadData(string filename)
        {
            using (var reader = new StreamReader(Path.Combine(directory, filename)))
            {
                while (!reader.EndOfStream)
                {
                    yield return reader.ReadLine().Split(',');
                }
            }
        }
    }
}