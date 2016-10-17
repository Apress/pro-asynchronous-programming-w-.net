using System.Collections.Generic;
using Repository45;

namespace Dotnet45MVC.Models
{
    public class FullViewModel
    {
        public IEnumerable<Author> Authors { get; set; }
        public IEnumerable<Title> Titles { get; set; }
    }
}