using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AsyncUtils;
using Repository40;

namespace Dotnet40
{
    public partial class ComplexAsyncPage : System.Web.UI.Page
    {
        private AuthorRepository authorRepo = new AuthorRepository();
        private TitleRepository titleRepo = new TitleRepository();

        private IAsyncResult authorIar;
        private IAsyncResult titleIar;

        private CountingAsyncResult asyncResult;

        protected void Page_Load(object sender, EventArgs e)
        {
            AddOnPreRenderCompleteAsync(StartGetData, EndGetData);
        }

        private void EndGetData(IAsyncResult ar)
        {
            try
            {
                int authorCount = authorRepo.EndGetAuthors(authorIar).Count();
                int titleCount = titleRepo.EndGetTitles(titleIar).Count();

                output.Text = (authorCount + titleCount).ToString();
            }
            finally
            {
                asyncResult.Dispose();
            }
        }

        private IAsyncResult StartGetData(object sender, EventArgs e, AsyncCallback cb, object extradata)
        {
            asyncResult = new CountingAsyncResult(cb, extradata, 2);
            SynchronizationContext ctx = SynchronizationContext.Current;

            authorIar = authorRepo.BeginGetAuthors(iar =>
                            {
                                try
                                {
                                 //   OperationThatCouldThrowException();
                                }
                                catch (Exception x)
                                {
                                    ctx.Send(_ =>
                                        {
                                            throw new WrapperException("An error occurred during processing", x);
                                        }, null);

                                }
                                finally
                                {
                                    asyncResult.OperationComplete();
                                }
                                //ctx.Post(_ =>
                                //    {
                                //        try
                                //        {
                                //            OperationThatCouldThrowException();
                                //        }
                                //        finally
                                //        {
                                //            asyncResult.OperationComplete();                                            
                                //        }
                                //    }, null);
                                                             

                            }, null);

            titleIar = titleRepo.BeginGetTitles(iar =>
                            {
                                asyncResult.OperationComplete();
                            }, null);

            return asyncResult;
        }

        private void OperationThatCouldThrowException()
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public class WrapperException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public WrapperException()
        {
        }

        public WrapperException(string message) : base(message)
        {
        }

        public WrapperException(string message, Exception inner) : base(message, inner)
        {
        }

        protected WrapperException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}