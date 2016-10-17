using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace Repository40
{
    public class TitleRepository
    {
        private const string connStr = "Server=.;Database=pubs;Integrated Security=SSPI; Asynchronous Processing=true";

        public void GetTitlesEAP()
        {
            var syncCtx = SynchronizationContext.Current;

            var conn = new SqlConnection(connStr);
            var cmd = new SqlCommand("GetTitles", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            conn.Open();

            cmd.BeginExecuteReader(iar =>
                {
                    try
                    {
                        using (SqlDataReader reader = cmd.EndExecuteReader(iar))
                        {
                            var titles = new List<Title>();
                            while (reader.Read())
                            {
                                titles.Add(new Title
                                    {
                                        Name = (string) reader["title"],
                                        Price = (decimal) (!reader.IsDBNull(1) ? reader["price"] : 0.0m)
                                    });
                            }

                            var args = new GetTitlesCompletedEventArgs(titles);

                            syncCtx.Post(_ =>
                                {
                                    GetTitlesCompleted(this, args);
                                }, null);
                        }
                    }
                    finally
                    {
                        cmd.Dispose();
                        conn.Dispose();
                    }
                }, null);
        }
    
        public event EventHandler<GetTitlesCompletedEventArgs> GetTitlesCompleted = delegate { };

        private SqlConnection apmConn;
        private SqlCommand apmCmd;
        public IAsyncResult BeginGetTitles(AsyncCallback callback, object state)
        {
            apmConn = new SqlConnection(connStr);
            apmCmd = new SqlCommand("GetTitles", apmConn);

            apmConn.Open();
            return apmCmd.BeginExecuteReader(callback, state);
        }

        public IEnumerable<Title> EndGetTitles(IAsyncResult iar)
        {
            try
            {
                var titles = new List<Title>();
                using (SqlDataReader reader = apmCmd.EndExecuteReader(iar))
                {
                    while (reader.Read())
                    {
                        titles.Add(new Title
                        {
                            Name = (string)reader["title"],
                            Price = (decimal)(!reader.IsDBNull(1) ? reader["price"] : 0.0m)
                        });
                    }

                    return titles;
                }
            }
            finally
            {
                apmCmd.Dispose();
                apmConn.Dispose();
            }
        }

    }

    public class GetTitlesCompletedEventArgs : EventArgs
    {
        public GetTitlesCompletedEventArgs(IEnumerable<Title> titles )
        {
            Titles = titles;
        }
        public IEnumerable<Title> Titles { get; private set; }
    }
}