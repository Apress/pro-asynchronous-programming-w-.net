using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Dotnet45WebAPI.Models;
using Repository45;

namespace Dotnet45WebAPI.Controllers
{
    public class FullController : ApiController
    {
        // GET api/Full/

        public async Task<FullResponse> Get()
        {
            var authorRepo = new AuthorRepository();
            var titleRepo = new TitleRepository();

            var authorTask = authorRepo.GetAuthorsAsync();
            var titleTask = titleRepo.GetTitlesAsync();

            await Task.WhenAll(authorTask, titleTask);

            var response = new FullResponse
                {
                    Authors = authorTask.Result,
                    Titles = titleTask.Result
                };

            return response;
        }
    }
}
