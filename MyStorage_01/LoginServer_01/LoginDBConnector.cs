using System;
using System.Data.SqlClient;

namespace LoginServer
{
    class LoginDBConnector
    {
        private static LoginDBConnector ldbc =  new LoginDBConnector();

        private SqlConnection conn;
        private SqlCommand cmd;
        private SqlDataReader sdr;

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
            return ldbc;
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
            sdr = cmd.ExecuteReader();
        }
    }
}
