using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ifElseSwitchLinking
{
    public interface ILedgerEntry
    {
        bool IsCredit { get; }
        bool IsDebit { get; }
    };

    public class CsvImporter
    {
        private TransformManyBlock<string, object[]> databaseQueryBlock;
        private TransformManyBlock<object[], ILedgerEntry> rowTGoLedgerBlock;
        private ActionBlock<ILedgerEntry> debitBlock;
        private ActionBlock<ILedgerEntry> creditBlock;
        private ActionBlock<ILedgerEntry> unknownLedgerEntryBlock;

        public CsvImporter()
        {
            databaseQueryBlock = new TransformManyBlock<string, object[]>((Func<string, IEnumerable<object[]>>) ExecuteQuery);
            rowTGoLedgerBlock = new TransformManyBlock<object[], ILedgerEntry>((Func<object[], IEnumerable<ILedgerEntry>>) MapDatabaseRowToObject);
            debitBlock = new ActionBlock<ILedgerEntry>((Action<ILedgerEntry>) WriteDebitEntry);
            creditBlock = new ActionBlock<ILedgerEntry>((Action<ILedgerEntry>) WriteCreditEntry);

            unknownLedgerEntryBlock = new ActionBlock<ILedgerEntry>((Action<ILedgerEntry>) LogUnknownLedgerEntryType);

            databaseQueryBlock.LinkTo(rowTGoLedgerBlock);
            rowTGoLedgerBlock.LinkTo(debitBlock, le => le.IsDebit); //if IsDebit
            rowTGoLedgerBlock.LinkTo(creditBlock, le => le.IsCredit); // else if IsCredit
            rowTGoLedgerBlock.LinkTo(unknownLedgerEntryBlock); // else
        }

        private void LogUnknownLedgerEntryType(ILedgerEntry obj)
        {
            
        }

        public void Export(string connectionString)
        {
            databaseQueryBlock.Post(connectionString);
        }

        
        private IEnumerable<object[]> ExecuteQuery(string arg){ yield break; }
        private IEnumerable<ILedgerEntry> MapDatabaseRowToObject(object[] arg) { yield break;}
        private void WriteDebitEntry(ILedgerEntry debitEntry) { }
        private void WriteCreditEntry(ILedgerEntry creditEntry) { }
       
       
    }

    public class DirectoryWalker
    {
        private ActionBlock<string> fileActionBlock;
        private TransformManyBlock<string, string> directoryBrowseBlock;
       
        private long directoriesRemaining;

        public DirectoryWalker(Action<string> fileAction)
        {
            directoryBrowseBlock = new TransformManyBlock<string, string>((Func<string, IEnumerable<string>>)(GetFilesInDirectory));
            fileActionBlock = new ActionBlock<string>(fileAction);

            directoryBrowseBlock.LinkTo(directoryBrowseBlock, Directory.Exists);
            directoryBrowseBlock.LinkTo(fileActionBlock,new DataflowLinkOptions() { PropagateCompletion = true});
        }

        public Task WalkAsync(string path)
        {     
            directoriesRemaining = 1;
            
            directoryBrowseBlock.Post(path);

            return fileActionBlock.Completion;
        }

        private IEnumerable<string> GetFilesInDirectory(string path)
        {
            var dir = new DirectoryInfo(path);
           
            var subDirectories = dir.GetDirectories().Select(fi => fi.FullName).ToArray();

            directoriesRemaining += subDirectories.Length;
            if (--directoriesRemaining == 0)
            {
                directoryBrowseBlock.Complete();
            }

            return dir
                .GetFiles().Select(fi => fi.FullName)
                .Concat(subDirectories);
        }

    }

    public class DirectoryWalkerLite
    {
        private ActionBlock<string> fileActionBlock;
        private TransformManyBlock<string, string> directoryBrowseBlock;
        
        public DirectoryWalkerLite(Action<string> fileAction)
        {
            directoryBrowseBlock = new TransformManyBlock<string, string>((Func<string, IEnumerable<string>>)(GetFilesInDirectory));
            fileActionBlock = new ActionBlock<string>(fileAction);

            directoryBrowseBlock.LinkTo(directoryBrowseBlock, Directory.Exists);
            directoryBrowseBlock.LinkTo(fileActionBlock);
        }

        public void Walk(string path)
        {
            directoryBrowseBlock.Post(path);
        }

        private IEnumerable<string> GetFilesInDirectory(string path)
        {
            var dir = new DirectoryInfo(path);

            return dir.EnumerateFileSystemInfos().Select(fi => fi.FullName);
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            
            
            var walker = new DirectoryWalker(Console.WriteLine);
            walker.WalkAsync(@"D:\repositories").Wait();
           

            //List<string> files = new List<string>();
            //foreach (string file in GetFiles(@"D:\repositories"))
            //{
            //    //Console.WriteLine(file);
            //    files.Add(file);
            //}

           

            //List<string> dwFiles = new List<string>();
            //var walker = new DirectoryWalker(f => dwFiles.Add(f));
            //walker.WalkAsync(@"D:\repositories").Wait();
            //Console.WriteLine("{0} == {1} : {2}",dwFiles.Count , files.Count , dwFiles.Count == files.Count);
        }

        
        private static IEnumerable<string> GetFiles(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            foreach (FileInfo file in dir.GetFiles())
            {
                yield return dir.FullName;
            }

            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                foreach (string file in GetFiles(subDir.FullName))
                {
                    yield return file;
                }
            }
        }
    }
}
