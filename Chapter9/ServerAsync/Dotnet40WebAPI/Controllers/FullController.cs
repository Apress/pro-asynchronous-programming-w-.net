using System.Threading;
using System.Threading.Tasks;
using Dotnet40WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Repository40;

namespace Dotnet40WebAPI.Controllers
{
    public class FullController : ApiController
    {
        //// GET api/authors
        //public IEnumerable<Author> Get()
        //{
        //    var repo = new AuthorRepository();

        //    return repo.GetAuthors();
        //}

        // GET api/authors
        public Task<FullResponse> Get()
        {
            var authorRepo = new AuthorRepository();
            var titleRepo = new TitleRepository();

            var tcs = new TaskCompletionSource<FullResponse>();

            var response = new FullResponse();

            int outstandingOperations = 2;

            Task.Factory.FromAsync(authorRepo.BeginGetAuthors(null, null), iar =>
                {
                    response.Authors = authorRepo.EndGetAuthors(iar);
                    int currentCount = Interlocked.Decrement(ref outstandingOperations);
                    if (currentCount == 0)
                    {
                        tcs.SetResult(response);
                    }
                });

            Task.Factory.FromAsync(titleRepo.BeginGetTitles(null, null), iar =>
            {
                response.Titles = titleRepo.EndGetTitles(iar);
                int currentCount = Interlocked.Decrement(ref outstandingOperations);
                if (currentCount == 0)
                {
                    tcs.SetResult(response);
                }
            });

            return tcs.Task;
        }

    }
}