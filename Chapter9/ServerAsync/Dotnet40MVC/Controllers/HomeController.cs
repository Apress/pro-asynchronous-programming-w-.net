using System.Threading;
using Dotnet40MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Repository40;

namespace Dotnet40MVC.Controllers
{
    //public class HomeController : Controller
    //{
    //    public ActionResult Index()
    //    {
    //        var repo = new AuthorRepository();

    //        IEnumerable<Author> authors = repo.GetAuthors();

    //        return View("Index", authors);
    //    }
    //}

    public class HomeController : AsyncController
    {
        public void IndexAsync()
        {
            AsyncManager.OutstandingOperations.Increment();

            var repo = new AuthorRepository();

            repo.GetAuthorsCompleted += (s, e) =>
                {
                    AsyncManager.Parameters["authors"] = e.Authors;
                    AsyncManager.OutstandingOperations.Decrement();
                };
            repo.GetAuthorsEAP();

        }

        public ActionResult IndexCompleted(IEnumerable<Author> authors)
        {
            return View("Index", authors);            
        }

        public void FullAsync()
        {
            AsyncManager.OutstandingOperations.Increment(2);

            var authorRepo = new AuthorRepository();

            authorRepo.GetAuthorsCompleted += (s, e) =>
            {
                AsyncManager.Parameters["authors"] = e.Authors;
                AsyncManager.OutstandingOperations.Decrement();
            };
            authorRepo.GetAuthorsEAP();


            var titleRepo = new TitleRepository();

            titleRepo.GetTitlesCompleted += (s, e) =>
            {
                AsyncManager.Parameters["titles"] = e.Titles;
                AsyncManager.OutstandingOperations.Decrement();
            };
            titleRepo.GetTitlesEAP();
        }

        public ActionResult FullCompleted(IEnumerable<Author> authors, IEnumerable<Title> titles )
        {
            return View("Full", new FullViewModel{Authors = authors, Titles = titles});
        }
    }

}
