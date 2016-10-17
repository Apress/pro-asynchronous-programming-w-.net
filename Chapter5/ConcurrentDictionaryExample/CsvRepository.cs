using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConcurrentDictionaryExample
{

    public delegate bool TryParse<T>(string toParse, out T val);

    public class CsvRepository : ICSVRepository
    {
        private readonly string directory;
        private ConcurrentDictionary<string, Lazy<List<string[]>>> csvFiles;

        public CsvRepository(string directory)
        {
            this.directory = directory;
            csvFiles = new ConcurrentDictionary<string, Lazy<List<string[]>>>(1, 100);
        }
        public IEnumerable<string> Files { get { return new DirectoryInfo(directory).GetFiles().Select(fi => fi.FullName); } }

        public IEnumerable<T> Map<T>(string dataFile, Func<string[], T> map)
        {
            var csvFile = new Lazy<List<string[]>>(() => LoadData(dataFile).ToList());

            csvFile = csvFiles.GetOrAdd(dataFile, csvFile);

            return csvFile.Value.Skip(1).Select(map);
        }

        private List<string> Loaded = new List<string>(); 
        
        public bool VerifyEachFileOnlyLoadedOnce()
        {
            return Loaded.Count == Loaded.Distinct().Count();
        }
        
        private IEnumerable<string[]> LoadData(string filename)
        {
            lock (Loaded)
            {
                Loaded.Add(filename);
            }

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