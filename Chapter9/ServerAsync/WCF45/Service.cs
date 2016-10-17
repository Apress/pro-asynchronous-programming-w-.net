using System;
using System.Linq;
using System.ServiceModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Repository45;
using WCF45;

namespace WCF40
{
    public class Service : IGetPubs
    {
        AuthorRepository authorRepo = new AuthorRepository();
        TitleRepository titleRepo = new TitleRepository();

        public async Task<List<AuthorDTO>> GetAuthors()
        {
            IEnumerable<Author> authors = await authorRepo.GetAuthorsAsync();

            return authors.Select(a => new AuthorDTO
                {
                    FirstName = a.FirstName,
                    LastName = a.LastName
                }).ToList();
        }

        public async Task<FullDetails> GetAuthorsAndTitles()
        {
            var authorTask = authorRepo.GetAuthorsAsync();
            var titleTask = titleRepo.GetTitlesAsync();

            await Task.WhenAll(authorTask, titleTask);

            var response = new FullDetails
                {
                    Authors = authorTask.Result.Select(a => new AuthorDTO
                        {
                            FirstName = a.FirstName,
                            LastName = a.LastName
                        }).ToList(),

                    Titles = titleTask.Result.Select(t => new TitleDTO()
                    {
                        Name = t.Name,
                        Price = t.Price == 0.0m ? (decimal?)null : t.Price
                    }).ToList(),
                };
            
            return response;
        }
    }
}