using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Repository45;

namespace Dotnet45
{
    public partial class ComplexAsyncPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(GetDataAsync));
        }

        async Task GetDataAsync()
        {
            var authorRepo = new AuthorRepository();
            var titleRepo = new TitleRepository();

            Task<IEnumerable<Author>> authorsTask = authorRepo.GetAuthorsAsync();
            Task<IEnumerable<Title>> titlesTask = titleRepo.GetTitlesAsync();
                    
            await Task.WhenAll(authorsTask, titlesTask);

            int authorCount = authorsTask.Result.Count();
            int titleCount = titlesTask.Result.Count();
 
            output.Text = (authorCount + titleCount).ToString();
        }
    }
}