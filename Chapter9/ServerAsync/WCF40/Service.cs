using System;
using System.Linq;
using System.ServiceModel;
using Repository40;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace WCF40
{
    public class Service : IGetPubs
    {
        AuthorRepository authorRepo = new AuthorRepository();
        TitleRepository titleRepo = new TitleRepository();

        //public List<AuthorDTO> GetAuthors()
        //{
        //    return authorRepo.GetAuthors()
        //                     .Select(a => new AuthorDTO
        //                         {
        //                             FirstName = a.FirstName,
        //                             LastName = a.LastName
        //                         })
        //                     .ToList();
        //}

        
        public IAsyncResult BeginGetAuthors(AsyncCallback callback, object state)
        {
            return authorRepo.BeginGetAuthors(callback, state);
        }

        public List<AuthorDTO> EndGetAuthors(IAsyncResult iar)
        {
            return authorRepo.EndGetAuthors(iar)
                             .Select(a => new AuthorDTO
                                 {
                                     FirstName = a.FirstName,
                                     LastName = a.LastName
                                 })
                             .ToList();
        }

        FullDetails response = new FullDetails();
        private string faultMessage = null;

        public IAsyncResult BeginGetAuthorsAndTitles(AsyncCallback callback, object state)
        {
            var tcs = new TaskCompletionSource<FullDetails>(state);
            int outstandingOperations = 2;

            authorRepo.BeginGetAuthors(iar =>
                {
                    try
                    {
                        response.Authors = authorRepo.EndGetAuthors(iar)
                                                     .Select(a => new AuthorDTO
                                                         {
                                                             FirstName = a.FirstName,
                                                             LastName = a.LastName
                                                         })
                                                     .ToList();
                    }
                    catch (Exception x)
                    {
                        faultMessage = "Error retrieving authors";
                    }
                    finally
                    {
                        int currentOutstanding = Interlocked.Decrement(ref outstandingOperations);
                        if (currentOutstanding == 0)
                        {
                            tcs.SetResult(response);
                            callback(tcs.Task);
                        }
                    }
                }, null);

            titleRepo.BeginGetTitles(iar =>
            {
                try
                {
                    response.Titles = titleRepo.EndGetTitles(iar)
                                                           .Select(a => new TitleDTO()
                                                           {
                                                               Name = a.Name,
                                                               Price = a.Price == 0.0m ? (decimal?)null : a.Price
                                                           })
                                                           .ToList();
                }
                catch (Exception x)
                {
                    faultMessage = "Error retrieving titles";
                }
                finally
                {
                    int currentOutstanding = Interlocked.Decrement(ref outstandingOperations);
                    if (currentOutstanding == 0)
                    {
                        tcs.SetResult(response);
                        callback(tcs.Task);
                    }
                }
            }, null);

            return tcs.Task;
        }

        public FullDetails EndGetAuthorsAndTitles(IAsyncResult iar)
        {
            if (faultMessage != null)
            {
                throw new FaultException(faultMessage);
            }

            return response;
        }
    }
}