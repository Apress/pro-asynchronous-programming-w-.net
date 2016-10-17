using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Repository45
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

        public async Task<IEnumerable<Title>> GetTitlesAsync()
        {
            var titles = new List<Title>();
            using (var conn = new SqlConnection(connStr))
            {
                using (var cmd = new SqlCommand("GetTitles", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            titles.Add(new Title
                            {
                                Name = (string) reader["title"],
                                Price = (decimal) (!reader.IsDBNull(1) ? reader["price"] : 0.0m)
                            });
                        }
                    }
                }
            }

            return titles;       
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