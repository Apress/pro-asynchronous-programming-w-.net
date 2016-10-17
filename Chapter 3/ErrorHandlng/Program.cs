using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ErrorHandlng
{
    class Program
    {
        private static void Main(string[] args)
        {
           UnobserveredTaskException();


             //ErrorHandling();


            //MultiFileImport();
        }

        private static void ErrorHandling()
        {
            Task task = Task.Factory.StartNew(() => Import(@"..\..\data\2.xml"));


            try
            {
                task.Wait();
            }
            catch (AggregateException errors)
            {
                foreach (Exception error in errors.Flatten().InnerExceptions)
                {
                    Console.WriteLine("{0} : {1}", error.GetType().Name, error.Message);
                }
                errors.Handle(IgnoreXmlErrors);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error : {0}", e);
            }
        }

        private static void UnobserveredTaskException()
        {
            NonObservedExceptions();

          //  NonObserveredExceptionsTerminateOnError();
        }

        private static void NonObserveredExceptionsTerminateOnError()
        {
            Task t = Task.Factory.StartNew(() => TerminateOnUnHandledException(() => { throw new Exception("Boom!"); }));
            t = null;
            object[] garbage = new object[10000];
            Random rnd = new Random();
            while (true)
            {
                if (rnd.Next(garbage.Length) == 1) Thread.Sleep(1);
                garbage[rnd.Next(garbage.Length)] = new object();
            }
        }

        private static void NonObservedExceptions()
        {
//TaskScheduler.UnobservedTaskException += HandleTaskExceptions;

            Task t = Task.Factory.StartNew(() =>
            {
                Console.WriteLine("About to blow..");
                throw new Exception("Boom!");
            });
            t = null;
            object[] garbage = new object[100];
            Random rnd = new Random();
            while (true)
            {
                if ( rnd.Next(garbage.Length) == 1) Thread.Sleep(1 );
                garbage[rnd.Next(garbage.Length)] = new object();
            }

        }

        private static void TerminateOnUnHandledException(Action body)
        {
            try
            {
                body();
            }
            catch (Exception error)
            {
                // Log it
                Environment.FailFast(error.Message);
            }
        }

        static void HandleTaskExceptions(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Console.WriteLine(sender is Task);
            foreach (Exception error in e.Exception.InnerExceptions)
            {
                Console.WriteLine(error.Message);
            }
            e.SetObserved();
        }

        private static bool IgnoreXmlErrors(Exception arg)
        {
            return (arg is XmlException);
        }


        private static void Import(string fullName)
        {
            XElement doc = XElement.Load(fullName);
            // process xml document
        }

        private static void MultiFileImport()
        {
            Task[] importTasks = (from file in new DirectoryInfo(@"..\..\data").GetFiles("*.xml")
                                  select Task.Factory.StartNew(() => Import(file.FullName)))
                .ToArray();

            Task.WaitAll(importTasks);
        }
    }
}
