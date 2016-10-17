using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TaskRelationships
{

    class Program
    {
        static void Main(string[] args)
        {
            //SimpleNestedTask();
           // SimpleChildTask();

           // FailedChildTask();


          //  WhenAny();

            //   PointLessContinuation();

           WhenAll();
        }

        private static void WhenAny()
        {
            Console.WriteLine(DownloadWebPageAsync(new string[]
            {
                "http://www.rocksolidknowledge2.com",
                "http://www.bbc.co.uk2",
                "http://www.apress.com"
            }).Result);
        }

        private static void WhenAll()
        {
            Task[] importTasks =
                (from file in new DirectoryInfo(@"..\..\data").GetFiles("*.xml")
                 select Task.Run(() => ProcessFile(file.FullName))).ToArray();




            Task.Factory.ContinueWhenAll(importTasks, _ => Console.WriteLine("All done")).Wait();


            //ParentChildWhenAll()
            //     .ContinueWith(_ => PrintThreadIdAndLabel("Continue"))
            //     .Wait();

         
        }

       
        private static void SimpleNestedTask()
        {
            Task.Factory.StartNew(() =>
                {
                    Task nested = Task.Factory.StartNew((() => Console.WriteLine("Nested..")));
                }).Wait();
            Console.WriteLine("Parent complete..");
        }

        private static void SimpleChildTask()
        {
            Task.Factory.StartNew(() =>
                {
                    Task child = Task.Factory.StartNew(() => Console.WriteLine("Child task.."),
                                                       TaskCreationOptions.AttachedToParent);
                }).Wait();
            Console.WriteLine("Child and Parent completed...");
        }

        private static void FailedChildTask()
        {
            Task.Run(() =>
                {
                    Task child = Task.Factory.StartNew(() => Console.WriteLine("Nested.."),
                                                       TaskCreationOptions.AttachedToParent);
                }).Wait();
            Console.WriteLine("Expecting Child and Parent completed...");
        }

        private static Task ParentChildWhenAll()
        {
            Task parent = Task.Factory.StartNew(() =>
            {
                PrintThreadIdAndLabel("Master");

                foreach (FileInfo file in new DirectoryInfo(@"C:\data").GetFiles("*.xml"))
                {
                    Task.Factory.StartNew(() => ProcessFile(file.FullName),
                                          TaskCreationOptions.AttachedToParent);
                }
              
            });

            return parent;
        }

        private static void PrintThreadIdAndLabel(string label)
        {
            Console.WriteLine("{1} {0}", Thread.CurrentThread.ManagedThreadId,label);
        }

        private static void ProcessFile(string fileProcess)
        {
            PrintThreadIdAndLabel("ProcessFile");
            XElement.Load(fileProcess);
           // Thread.Sleep(5000);
        }

        private static void PointLessContinuation()
        {
            Task<int> firstTask = Task.Factory.StartNew<int>(() => { Console.WriteLine("First Task"); throw new Exception("Boom!");return 42;});

           
            Task secondTask = firstTask.ContinueWith(ft => Console.WriteLine("Second Task, First task returned {0}" , ft.Result) , 
                                TaskContinuationOptions.OnlyOnRanToCompletion);

            Task errorHandler = firstTask.ContinueWith(st => Console.WriteLine(st.Exception),TaskContinuationOptions.OnlyOnFaulted);

            secondTask.Wait();

            Console.ReadLine();
        }

        private static void ScatterAndJoin()
        {
            Task[] algorithmTasks = new Task[4];
            for (int nTask = 0; nTask < algorithmTasks.Length; nTask++)
            {
                int partToProcess = nTask;
                algorithmTasks[nTask] = Task.Factory.StartNew(() => ProcessPart(partToProcess));
            }

            Task.Factory.ContinueWhenAll(algorithmTasks, antecedentTasks => ProduceSummary());
        }

        private static void ProduceSummary()
        {
            throw new NotImplementedException();
        }


        private static void ProcessPart(int partToProcess)
        {
            
        }

        private static Task<string> DownloadWebPageAsync(string[] urls)
        {
            List<Task<WebResponse>> requestsOutStanding =
                (from url in urls
                 select WebRequest.Create(url).GetResponseAsync()
                ).ToList();



            return Task.Factory.ContinueWhenAny(requestsOutStanding.ToArray(), completedRequest =>
                {
                    requestsOutStanding.Remove(completedRequest);

                    while ((completedRequest.Status != TaskStatus.RanToCompletion) && ( requestsOutStanding.Count > 0))
                    {
                        completedRequest = requestsOutStanding[Task.WaitAny(requestsOutStanding.ToArray())];
                        requestsOutStanding.Remove(completedRequest);
                    }

                    if ( completedRequest.Status != TaskStatus.RanToCompletion ) return "";

                    using (var reader = new StreamReader(completedRequest.Result.GetResponseStream()))
                    {
                        return reader.ReadToEnd();
                    }

                  
                });
        }

        private static Task<string> BadDownloadWebPageAsync(string url)
        {
            return Task.Factory.StartNew<string>(()=>
            {
                WebRequest request = WebRequest.Create(url);
                Task<WebResponse> response = request.GetResponseAsync();

                using (var reader = new StreamReader(response.Result.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            });
        }

        private static Task<string> BetterDownloadWebPageAsync45DIY(string url)
        {
            WebRequest request = WebRequest.Create(url);
            Task<WebResponse> response = request.GetResponseAsync();

            return response
                          .ContinueWith<string>(dt =>
                          {
                              using (var reader = new StreamReader(dt.Result.GetResponseStream()))
                              {
                                  return reader.ReadToEnd();
                              }
                          });
        }

        public Task ImportXmlFilesAsync(string dataDirectory, CancellationToken ct)
        {
            return Task.Factory.StartNew(() =>
            {
                foreach (FileInfo file in new DirectoryInfo(dataDirectory).GetFiles("*.xml"))
                {
                    string fileToProcess = file.FullName;
                    Task.Factory.StartNew( _ =>
                        {
                            // convienent point to check for cancellation

                            XElement doc = XElement.Load(fileToProcess);

                            InternalProcessXml(doc,ct);
                        }, ct, TaskCreationOptions.AttachedToParent);
                }
            }, ct);
        }

        private void InternalProcessXml(XElement doc, CancellationToken ct)
        {
            Thread.Sleep(5000);
        }
    }
}
