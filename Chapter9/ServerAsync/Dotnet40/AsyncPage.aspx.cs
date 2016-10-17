using Repository40;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Dotnet40
{
    public partial class AsyncPage : System.Web.UI.Page
    {
        AuthorRepository repo = new AuthorRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            AddOnPreRenderCompleteAsync(StartGetData, EndGetData);
        }

        private void EndGetData(IAsyncResult ar)
        {
            IEnumerable<Author> authors = repo.EndGetAuthors(ar);
            output.Text = authors.Count().ToString();
        }

        private IAsyncResult StartGetData(object sender, EventArgs e, AsyncCallback cb, object extradata)
        {
            return repo.BeginGetAuthors(cb, extradata);
        }
    }
}