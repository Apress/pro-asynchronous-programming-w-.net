using System.Collections.Generic;
using Repository40;

namespace Dotnet40MVC.Models
{
    public class FullViewModel
    {
        public IEnumerable<Author> Authors { get; set; }
        public IEnumerable<Title> Titles { get; set; }
    }
}