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
    public partial class AsyncPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(GetDataAsync));
        }

        async Task GetDataAsync()
        {
            var repo = new AuthorRepository();

            IEnumerable<Author> authors = await repo.GetAuthorsAsync();

            output.Text = authors.Count().ToString();
        }
    }
}