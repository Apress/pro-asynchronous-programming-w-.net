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
    public class AuthorsController : ApiController
    {
        //// GET api/authors
        //public IEnumerable<Author> Get()
        //{
        //    var repo = new AuthorRepository();

        //    return repo.GetAuthors();
        //}

        // GET api/authors
        public Task<IEnumerable<Author>> Get()
        {
            var repo = new AuthorRepository();

            return Task.Factory.FromAsync<IEnumerable<Author>>(repo.BeginGetAuthors, repo.EndGetAuthors, null);
        }

    }
}