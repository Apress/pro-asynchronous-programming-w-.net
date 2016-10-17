using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Dotnet40
{
    public partial class AsyncPageThreading : System.Web.UI.Page
    {
        private SqlConnection conn;
        private SqlCommand cmd;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                AddOnPreRenderCompleteAsync(StartGetData, EndGetData);
            }
        }

        private void EndGetData(IAsyncResult ar)
        {
            outputT2.Text = Thread.CurrentThread.ManagedThreadId.ToString();
            output.Text = (string)HttpContext.Current.Items["foo"];
        }

        private IAsyncResult StartGetData(object sender, EventArgs e, AsyncCallback cb, object extradata)
        {
            outputT1.Text = Thread.CurrentThread.ManagedThreadId.ToString();
            HttpContext.Current.Items["foo"] = "bar";
            Task t = null;
            t = new Task(() =>
                {
                    Thread.Sleep(2000);
                    cb(t);
                });
            t.Start();

            return t;
        }
    }
}