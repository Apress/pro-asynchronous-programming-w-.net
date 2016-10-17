using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Repository40;

namespace Dotnet40
{
    public partial class SyncPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var repo = new AuthorRepository();

            output.Text = repo.GetAuthors().Count().ToString();
        }
    }
}