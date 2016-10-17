using System.Collections.Generic;
using Repository45;

namespace Dotnet45WebAPI.Models
{
    public class FullResponse
    {
        public IEnumerable<Author> Authors { get; set; }
        public IEnumerable<Title> Titles { get; set; }
    }
}