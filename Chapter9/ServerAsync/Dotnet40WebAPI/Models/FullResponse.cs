using System.Collections.Generic;
using Repository40;

namespace Dotnet40WebAPI.Models
{
    public class FullResponse
    {
        public IEnumerable<Author> Authors { get; set; }
        public IEnumerable<Title> Titles { get; set; }
    }
}