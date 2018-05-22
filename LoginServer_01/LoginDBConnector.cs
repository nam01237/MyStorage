using System;
using System.Data.SqlClient;
using System.Text;

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
            string strConn = "server=.\\SQLEXPRESS; database=WebHard; uid=WHADMIN; pwd=562389;";
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

        public SqlDataReader SelectDB(StringBuilder query)
        {
            cmd.CommandText = query.ToString() ;
            sdr = cmd.ExecuteReader();
            return sdr;
        }

        public void InsertDB(StringBuilder query)
        {
            cmd.CommandText = query.ToString();
            cmd.ExecuteNonQuery();
        }
    }
}
