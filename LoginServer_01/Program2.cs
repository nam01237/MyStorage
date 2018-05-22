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

/*
 * 클라이언트와의 로그인 상태를 유지하기위한 클래스가있는 네임스페이스입니다.
 * 지속적으로 Pack를 주고받습니다.
 * 회원가입 기능도 제공합니다.
 */
 /*
namespace LoginServer
{
    class ServerMainService
    {
        // 접속한 클라이언트 ID
        private string id;
        // 네트워크 관련
        private NetworkStream clientStream;
        private TcpClient client;
        // DB관련 인스턴스
        private SqlConnection conn;
        private SqlCommand cmd;
        private SqlDataReader sdr;
        // 직렬화를 위한 인스턴스
        private BinaryFormatter serializer;
        // 유지관리 관련
        private bool aLogin = false;                                    // 다른로그인으로 인해서 연결이 종료되는 경우를 체크
        private static List<string> logoutList = new List<string>();    // 중복 로그인으로 인해 로그아웃이 필요할때 해당 ID를 저정시킨다.
        private static object thisLock = new object();                  // 임계영역 관리를 위한 인스턴스

        // === 생성자 === //
        public ServerMainService(TcpClient client)
        {
            this.client = client;
            this.clientStream = client.GetStream();
            this.serializer = new BinaryFormatter(); // 직렬화기

            try
            {
                this.conn = new SqlConnection();
                string strConn = "server=localhost; database=WebHard; uid=WHADMIN; pwd=562389;";
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

        // === Service() 로그인/회원가입 유지관리 === //
        public void Service()
        {
            Pack successPack = new Pack(); // 성공/실패 여부를 보내주는 Pack
            Pack resMait = new Pack();     // 로그인 유지를 위해 건내줄 Pack
            Pack reqMait = new Pack();     // 로그인 유지를 위해 받을 Pack

            Pack reqPack = (Pack)serializer.Deserialize(clientStream); // 처음 요청 Pack 받는다.

            if (reqPack.PACK_TYPE == CONSTANTS.TYPE_REQ_LOGIN) // =============== 로그인 요청처리
            {
                Console.WriteLine("<->로그인 요청 : {0}", ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
                try
                {
                    id = ((ReqLoginPack)reqPack).ID;         // 요청에 담긴 id, pw 저장
                    string pw = ((ReqLoginPack)reqPack).PW;

                    cmd.CommandText = "select *, PWDCOMPARE('" + pw + "', pw) as pwc from users where id COLLATE Korean_Wansung_CS_AS = '" + id + "'";
                    sdr = cmd.ExecuteReader();  // ㄴ해당 id존재 여부와 pw 일치여부 검사 쿼리 전달

                    if (!(sdr.Read())) // 요청한 id가 회원테이블에 없는경우
                    {
                        successPack.PACK_TYPE = CONSTANTS.TYPE_ERROR;
                        successPack.FLAG = CONSTANTS.ERROR_INVALID_ID;
                        serializer.Serialize(clientStream, successPack);
                        Console.WriteLine("<!>없는 아이디 입력으로 로그인실패 / ID : {0} IP : {1}", ((ReqLoginPack)reqPack).ID, ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
                        sdr.Close();
                        return;
                    }
                    else if (((int)sdr["pwc"]) == 0) // pw가 틀린경우
                    {
                        successPack.PACK_TYPE = CONSTANTS.TYPE_ERROR;
                        successPack.FLAG = CONSTANTS.ERROR_INVALID_PASSWORD;
                        serializer.Serialize(clientStream, successPack);
                        Console.WriteLine("<!>비밀번호 불일치로 로그인실패 / ID : {0} IP : {1}", ((ReqLoginPack)reqPack).ID, ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
                        sdr.Close();
                        return;
                    }
                    else
                    {
                        if (sdr["logch"].ToString().Equals("Y")) // 이미 해당 id로 다른곳에서 로그인을 한 경우
                        {
                            Console.WriteLine("<!>중복 로그인 시도 / ID : {0} IP :", ((ReqLoginPack)reqPack).ID, ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
                            // 일단 중복로그인임을 클라이언트에 알린다.
                            successPack.PACK_TYPE = CONSTANTS.TYPE_ERROR;
                            successPack.FLAG = CONSTANTS.ERROR_CONNECTED_ID;
                            serializer.Serialize(clientStream, successPack);

                            // 클라이언트에서 기존로그인을 끊기를 원하는지 여부를 체크한다.
                            reqPack = (Pack)serializer.Deserialize(clientStream);
                            if (reqPack.FLAG == CONSTANTS.FLAG_NO)
                                return; // 원하지 않으면 그대로 종료

                            // 로그아웃 대기 리스트에 해당 id를 추가한다.
                            lock (thisLock)
                            {
                                logoutList.Add(id);
                            }

                            // 해당 id가 로그아웃 리스트에서 사라질때까지 반복문 수행
                            bool loop = true;
                            while (loop)
                            {
                                loop = false;
                                lock (thisLock)
                                {
                                    foreach (string s in logoutList)
                                    {
                                        if (s == id)
                                        {
                                            loop = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            // 로그인 성공 응답 보낸다
                            successPack.PACK_TYPE = CONSTANTS.TYPE_BASIC;
                            successPack.FLAG = CONSTANTS.FLAG_SUCCESS;
                            serializer.Serialize(clientStream, successPack);
                        }
                        else
                        {
                            // 중복 로그인이 아닐경우 그냥간다.
                            successPack.FLAG = CONSTANTS.FLAG_SUCCESS;
                            serializer.Serialize(clientStream, successPack);

                        }

                    }
                    // Login Success ------------- 여기까지 return을 안만났다면 로그인이 성공한것.
                    Console.WriteLine("<->로그인 성공 / ID:{0} IP:{1}", ((ReqLoginPack)reqPack).ID, ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
                    // 로그인 여부를 Y로 바꾸어준다.
                    sdr.Close();
                    cmd.CommandText = "UPDATE users set logch='Y' where id COLLATE Korean_Wansung_CS_AS ='" + id + "'";
                    cmd.ExecuteNonQuery();

                    // 로그인 유지를위해 Pack 주고받는 반복문
                    while (true)
                    {
                        for (int i = 0; i < logoutList.Count; i++) // 로그아웃 대기 리스트 검색해가며 로그인유지
                        {
                            if (id == logoutList[i])
                            {
                                throw new ComeAnotherLoginException(); // 만약 중복로그인이 감지되는경우 사용자지정 예외 발생
                            }
                        }

                        reqMait = (Pack)serializer.Deserialize(clientStream); // 받고
                        serializer.Serialize(clientStream, resMait);          // 주고

                        Thread.Sleep(500);
                    }
                }
                catch (ComeAnotherLoginException)
                {
                    resMait.PACK_TYPE = CONSTANTS.TYPE_ERROR;
                    resMait.FLAG = CONSTANTS.ERROR_ANOTHER_LOGIN;
                    serializer.Serialize(clientStream, resMait);
                    Console.WriteLine("<!>다른 로그인으로 연결 종료 / ID:{0} IP:{1}", ((ReqLoginPack)reqPack).ID, ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
                    aLogin = true; // 중복 로그인으로 종료되면 true로
                    lock (thisLock)
                    {
                        logoutList.Remove(id);
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("<!>연결 종료 / ID:{0} IP:{1}", ((ReqLoginPack)reqPack).ID, ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("<!>연결 종료 / ID:{0} IP:{1}", ((ReqLoginPack)reqPack).ID, ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
                }
                finally
                {
                    if (!aLogin) // 중복로그인으로 종료되는경우 테이블의 로그인여부를 바꿔주지 않는다.
                    {
                        cmd.CommandText = "UPDATE users SET logch='N' where id COLLATE Korean_Wansung_CS_AS ='" + id + "'";
                        cmd.Connection = conn;
                        cmd.ExecuteNonQuery();
                    }
                    sdr.Close();
                    conn.Close();
                    clientStream.Close();
                    client.Close();
                }
            }
            else if (reqPack.PACK_TYPE == CONSTANTS.TYPE_REQ_REGISTER) // =================== 회워가입 요청처리
            {
                Console.WriteLine("<->회원가입 요청 : {0}", ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
                // Pack에담긴 정보 담는 인스턴스
                string regId = ((ReqRegisterPack)reqPack).ID;
                string regPw = ((ReqRegisterPack)reqPack).PW;
                string regEmail = ((ReqRegisterPack)reqPack).EMAIL;

                successPack.PACK_TYPE = CONSTANTS.TYPE_ERROR;

                try
                {   // 회원가입 요청한 id가 이미 있는지 여부를 검사한다.
                    cmd.CommandText = "select * from users where (id COLLATE Korean_Wansung_CS_AS ='" + regId + "'OR email COLLATE Korean_Wansung_CS_AS ='" + regEmail + "')";
                    sdr = cmd.ExecuteReader();

                    // 이부분은 정규식으로 id와 email을 검사하는 부분이다.
                    Regex regex_id = new Regex(@"^[a-zA-Z0-9]{4,20}$"); // 영문자 또는 숫자로 구성된 4~20 글자수
                    Regex regex_email = new Regex(@"^[0-9a-zA-Z]([-_\.]?[0-9a-zA-Z])*@[0-9a-zA-Z]([-_\.]?[0-9a-zA-Z])*\.[a-zA-Z]{2,3}$"); // email 무결성검사 (그냥 퍼왔음)
                    if (!(regex_id.IsMatch(regId))) // id 규칙이 위반시
                    {
                        successPack.FLAG = CONSTANTS.ERROR_ID_AGUMENT;
                        serializer.Serialize(clientStream, successPack);
                        Console.WriteLine("<!>형식에 맞지않는 ID : {0}", ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
                        return;
                    }
                    else if (!(regex_email.IsMatch(regEmail))) // 올바르지않은 email일시
                    {
                        successPack.FLAG = CONSTANTS.ERROR_EMAIL_AGUMENT;
                        serializer.Serialize(clientStream, successPack);
                        Console.WriteLine("<!>유효하지않은 e-mail : {0}", ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
                        return;
                    }

                    if ((sdr.Read())) // 이미 해당 id가 있다면
                    {
                        if (sdr["id"].ToString() == regId)
                            successPack.FLAG = CONSTANTS.ERROR_DUPLICATE_ID;
                        else
                            successPack.FLAG = CONSTANTS.ERROR_DUPLICATE_EMAIL;

                        serializer.Serialize(clientStream, successPack);
                        Console.WriteLine("<!>아이디가 이미있으므로 회원가입 실패 / ID : {0} IP : ", regId, ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
                        return;
                    }

                    // ---------- 여기까지오면 계정을 추가해도 된다.
                    // 테이블에 요청한 정보에따라 insert
                    sdr.Close();
                    Console.WriteLine(regPw);
                    cmd.CommandText = "insert into users(id, pw, email, logch) values('" + regId + "', PWDENCRYPT('" + regPw + "'), '" + regEmail + "', 'N')";
                    cmd.ExecuteNonQuery();

                    successPack.PACK_TYPE = CONSTANTS.TYPE_BASIC;
                    Console.WriteLine("<->회원가입 성공 / ID : {0} IP : {1} ", regId, ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
                    serializer.Serialize(clientStream, successPack);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    successPack.PACK_TYPE = CONSTANTS.TYPE_ERROR;
                    successPack.FLAG = CONSTANTS.ERROR_UNKNOWN_ERROR;
                    serializer.Serialize(clientStream, successPack);
                    Console.WriteLine("<!>연결 종료 / ID : {0} IP : {1}", ((ReqLoginPack)reqPack).ID, ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
                }
                finally
                {
                    sdr.Close();
                    conn.Close();
                    clientStream.Close();
                    client.Close();
                }

            }

        }
    }

    // 중복 로그인 발생시 그것을 알리는 예외
    class ComeAnotherLoginException : Exception
    {
        public ComeAnotherLoginException()
        { }

        public ComeAnotherLoginException(string message)
            : base(message)
        { }
    }

    class Program
    {
        private const int logPort = 4453; // 로그인 서버 포트
        
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

                ServerMainService servMain = new ServerMainService(client); // servMain : 로그인 유지/회원가입을 위한 클래스
                Thread thread = new Thread(new ThreadStart(servMain.Service));

                thread.Start();

            }
        }
    }
}
*/