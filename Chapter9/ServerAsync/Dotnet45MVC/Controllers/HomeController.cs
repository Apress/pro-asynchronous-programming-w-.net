using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Dotnet45MVC.Models;
using Repository45;

namespace Dotnet45MVC.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var repo = new AuthorRepository();

            IEnumerable<Author> authors = await repo.GetAuthorsAsync();
            return View("Index", authors);
        }

        public async Task<ActionResult> Full()
        {
            var authorRepo = new AuthorRepository();
            var titleRepo = new TitleRepository();

            var authorsTask = authorRepo.GetAuthorsAsync();
            var titlesTask = titleRepo.GetTitlesAsync();

            await Task.WhenAll(authorsTask, titlesTask);

            IEnumerable<Author> authors = authorsTask.Result;
            IEnumerable<Title> titles = titlesTask.Result;

            return View("Full", new FullViewModel {Authors = authors, Titles = titles});
        }
    }
}
