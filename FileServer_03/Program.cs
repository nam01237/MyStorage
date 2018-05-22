using System;
using WHP; // WebHardProtocol
using System.Net; // IPEndPorint;
using System.Net.Sockets; // TcpListner
using System.IO; //NetworkStream // DirectoryInfo
using System.Runtime.Serialization.Formatters.Binary; // BinaryFomatter
using System.Threading; //Thread
using System.Windows.Forms; // TreeView // TreeViewNode

/*  
 *  파일 서비스를 제공하는 네임스페이스 입니다.
 *  클라이언트에서 오는 하나의 요청단위(BinaryFomatter로 한번들어오는)로 스레드를 만들어서 처리한다.  
 *  요청에대한 정의는 WHP 클래스 라이브러리에 있음.
 *  클라이언트 접속 유지는 LoginServer가 관리한다.
 */

namespace FileServer_03
{
    // ServiceUtil Class ===============================================================================================================================//

    class Program
    {
        const int serverPort = 4454; // 파일서버 포트

        static void Main(string[] args)
        {
            IPEndPoint serverAdress = new IPEndPoint(0, serverPort); 
            TcpListener serverListner = new TcpListener(serverAdress);
            TcpClient clientListner;

            //-----------------//
            serverListner.Start(); // 서버 시작
            Console.WriteLine("==== 파일 서버 가동 ===== ");

            //클라이언트 요청단위로 스레드 반복생성// 
            while (true)
            {
                clientListner = serverListner.AcceptTcpClient();
                Console.WriteLine("<!>접속확인 IP : {0}", ((IPEndPoint)clientListner.Client.RemoteEndPoint).ToString());

                ServiceUtil servUtil = new ServiceUtil(clientListner); // ServUtil : 클라에서 받은 메시지 받고 처리하는 메소드들 있는 클래스
                Thread thread = new Thread(new ThreadStart(servUtil.Service));

                thread.Start(); // 스레드 시작

            }

        }

    }



}
