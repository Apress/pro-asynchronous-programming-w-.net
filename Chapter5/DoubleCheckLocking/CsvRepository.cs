using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DoubleCheckLocking
{
   
    

    public class CsvRepository
    {
        public class VirtualEnunemerable<T>:IEnumerable<T>
        {
            private object creationLock = new object();
            private readonly Func<IEnumerable<T>> create;
            private IEnumerable<T> realEnumerable = null;

            public VirtualEnunemerable(Func<IEnumerable<T>> create )
            {
                this.create = create;
            }

            public IEnumerator<T> GetEnumerator()
            {
                if (realEnumerable == null)
                {
                    IEnumerable<T> enumerable = null;
                    lock (creationLock)
                    {
                        if (realEnumerable == null)
                        {
                            enumerable = create();
                        }
                    }
                    if (enumerable != null)
                    {
                        realEnumerable = enumerable;
                    }
                }
                return realEnumerable.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public class VirtualCsv
        {
            private readonly Func<List<string[]>> load;

            public VirtualCsv(Func<List<string[]>> load )
            {
                this.load = load;
            }

            private object loadLock = new object();
            private  List<string[]> value;
 
            public List<string[]> Value
            {
                get
                {
                    if (value == null)
                    {
                        lock (loadLock)
                        {
                            if (value == null)
                            {
                                value = load();
                            }
                        }
                    }

                    return value;
                }
            }
        }

        private readonly string directory;
        private Dictionary<string, VirtualCsv> csvFiles;

        public CsvRepository(string directory)
        {
            this.directory = directory;
            csvFiles = new DirectoryInfo(directory)
                .GetFiles("*.csv")
               // .ToDictionary(f => f.Name, f => LoadData(f.Name).ToList());
                .ToDictionary<FileInfo,string,VirtualCsv>(f => f.Name, f => new VirtualCsv(() => LoadData(f.Name).ToList()));
        }
        public IEnumerable<string> Files { get { return csvFiles.Keys; }}

        public IEnumerable<T> Map<T>(string dataFile,Func<string[],T> map )
        {
            return csvFiles[dataFile].Value.Skip(1).Select(map);

            // return LazyLoadData(dataFile).Skip(1).Select(map);

            
        }


        private IEnumerable<string[]> LoadData(string filename)
        {
            using (var reader = new StreamReader(Path.Combine(directory,filename)))
            {
                while (!reader.EndOfStream)
                {
                    yield return reader.ReadLine().Split(',');
                }
            }
        }

        //private IEnumerable<string[]> LazyLoadData(string filename)
        //{
           

        //    List<string[]> csvFile = csvFiles[filename].Value;

        //    if (csvFile == null)
        //    {
        //        lock (csvFiles[filename])
        //        {
        //            csvFile = csvFiles[filename].Value;
        //            if (csvFile == null)
        //            {
        //                csvFile = LoadData(filename).ToList();

        //                csvFiles[filename].Value = csvFile;
        //            }
        //        }
        //    }

        //    return csvFile;
        //}
    }
}



//private IEnumerable<string[]> NotSafe_LazyLoadData(string filename)
//{
//     List<string[]> csvFile = null;

//     csvFile = csvFiles[filename];

//    if (csvFile == null)
//    {
//        csvFile = LoadData(Path.Combine(directory, filename)).ToList();
//        csvFiles[filename] = csvFile;
//    }
//    return csvFile;
//}

//private IEnumerable<string[]> TooSafe_LazyLoadData(string filename)
//{
//    lock (csvFiles)
//    {
//        List<string[]> csvFile = null;

//        csvFile = csvFiles[filename];

//        if (csvFile == null)
//        {
//            csvFile = LoadData(Path.Combine(directory, filename)).ToList();
//            csvFiles[filename] = csvFile;
//        }
//        return csvFile;
//    }
//}