using System;
using WHP;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
namespace LoginServer
{

class Program
    {
        private const int logPort = 4453; // 로그인 서버 포트
        public static LoginDBConnector ldbc = LoginDBConnector.GetInstance();
        public static LoginUserList userList = LoginUserList.GetInstance();

        static void Main(string[] args)
        {
            IPEndPoint logAddress = new IPEndPoint(0, logPort);
            TcpListener logListener = new TcpListener(logAddress);
            TcpClient client;

            logListener.Start();

            Console.WriteLine("==== 로그인 서버 가동 ===== ");

            // ==== 클라이언트 요청 반복 수행 ==== //
            while (true)
            {
                client = logListener.AcceptTcpClient();
                Console.WriteLine("<->접속확인 IP : {0}", ((IPEndPoint)client.Client.RemoteEndPoint).ToString());

                LoginSignWork work = new LoginSignWork(client); // servMain : 로그인 유지/회원가입을 위한 클래스
                Thread thread = new Thread(new ThreadStart(work.Working)); 

                thread.Start();

            }
        }
    }
}
