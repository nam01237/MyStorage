using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {

        // DB관련 인스턴스
        private SqlConnection conn;
        private SqlCommand cmd;
        private SqlDataReader sdr;


        // === 생성자 === //
        public Program()
        {
            try
            {
                this.conn = new SqlConnection();
                string strConn = "server=127.0.0.1\\MSSQLSERVER,1433; database=master; uid=sa; pwd=123;";
                conn.ConnectionString = strConn;
                conn.Open();
                cmd = new SqlCommand();
                cmd.Connection = conn;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static void Main(string[] args)
        {
            new Program();
            Console.WriteLine("Asd");
        }
    }
}
