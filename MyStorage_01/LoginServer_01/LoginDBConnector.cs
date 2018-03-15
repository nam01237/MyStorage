using System;
using System.Data.SqlClient;

namespace LoginServer
{
    class LoginDBConnector
    {
        private static LoginDBConnector instance;

        private SqlDataReader sdr;
        private SqlConnection conn;
        private SqlCommand cmd;

        private LoginDBConnector()
        {
            this.conn = new SqlConnection();
            string strConn = "server=localhost; database=WebHard; uid=WHADMIN; pwd=562389;";
            conn.ConnectionString = strConn;
            conn.Open();
            cmd = new SqlCommand();
            cmd.Connection = conn;
            Console.WriteLine("==== DB 연결 ===== ");
        }

        public static LoginDBConnector GetInstance()
        {
            if( instance == null )
            {
                instance = new LoginDBConnector();
            }

            return instance;
        }

        public SqlDataReader SelectDB(string query)
        {
            cmd.CommandText = query;
            sdr = cmd.ExecuteReader();
            return sdr;
        }

        public void InsertDB(string query)
        {
            cmd.CommandText = query;
            cmd.ExecuteNonQuery();
        }
    }
}
