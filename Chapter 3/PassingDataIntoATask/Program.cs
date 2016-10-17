using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassingDataIntoATask
{
    //public class DataImporter
    //{
    //    public void Import(object o)
    //    {
    //        string directory = (string) o;
    //        // Import files from which directory ?
    //    }
    //}

    //public class DataImporter
    //{
    //    private readonly string directory;

    //    public DataImporter(string directory)
    //    {
    //        this.directory = directory;
    //    }

    //    public void Import()
    //    {
           
    //       // Import files from this.directory
    //    }
    //}

    public class DataImporter
    {
        public void Import(string directory)
        {
            // Import files from this.directory
        }
    }

    public class ImportClosure
    {
        public string importDirectory;
        public DataImporter importer;
        
        public void ClosureMethod()
        {
            importer.Import(importDirectory);    
        }
    }
    class Program
    {
        private static void Main(string[] args)
        {
            //var closure = new ImportClosure();

            //closure.importer = new DataImporter();
            //closure.importDirectory = @"C:\data";

            //Task.Factory.StartNew(closure.ClosureMethod);

            //Task.Factory.StartNew(() => importer.Import(importDirectory));

            //Task.Factory.StartNew(importer.Import,@"C:\data");

            Brok
        }

        private static void WorkingClosures()
        {
            for (int i = 0; i < 10; i++)
            {
                int toCaptureI = i;
                Task.Run(() => Console.WriteLine(toCaptureI));
            }
            Console.ReadLine();
        }

        private static void BrokenClosures()
        {
            for (int i = 0; i < 10; i++)
            {
                Task.Run(() => Console.WriteLine(i));
            }
            Console.ReadLine();
        }
    }
}
