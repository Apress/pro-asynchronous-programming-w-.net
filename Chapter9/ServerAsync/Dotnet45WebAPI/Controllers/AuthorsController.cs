using System.Threading.Tasks;
using Dotnet45WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Repository45;

namespace Dotnet45WebAPI.Controllers
{
    public class AuthorsController : ApiController
    {
        // GET api/authors
        public Task<IEnumerable<Author>> Get()
        {
            var repo = new AuthorRepository();

            return repo.GetAuthorsAsync();
        }
    }
}