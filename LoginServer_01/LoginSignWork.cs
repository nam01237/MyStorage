using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data.SqlClient;
using WHP;
using System.Text.RegularExpressions;
using System.Text;

namespace LoginServer
{
    class LoginSignWork
    {
        // 접속한 클라이언트 ID
        private string loginId;
        // 네트워크 관련
        private TcpClient client;
        private NetworkStream clientStream;
        // 직렬화를 위한 인스턴스
        private BinaryFormatter serializer;
        private SqlDataReader sdr;

        private Pack successPack;
        private Pack reqPack;

        private string ipString;
        private Guid loginGuid;

        private StringBuilder query; 

        // === 생성자 === //
        public LoginSignWork(TcpClient client)
        {
            this.client = client;
            clientStream = client.GetStream();
            serializer = new BinaryFormatter(); // 직렬화기
            ipString = ((IPEndPoint)client.Client.RemoteEndPoint).ToString();
            successPack = new Pack();
            query = new StringBuilder();
        }

        public bool LoginCheck(ReqLoginPack logPack)
        {
            Console.WriteLine("<->로그인 요청 : {0}", ipString);

            bool success = false;
            string reqId = logPack.Id;
            string reqPw = logPack.Pw;

            query.Clear();
            //query.Append("select *, PWDCOMPARE('");
            //query.Append(reqPw);
            //query.Append("', pw) as pwd from users");
            query.Append("select * from users");
            query.Append(" where id COLLATE Korean_Wansung_CS_AS = '");
            query.Append(reqId);
            query.Append("'");

            sdr = Program.ldbc.SelectDB(query);

            if (!(sdr.Read())) // 요청한 id가 회원테이블에 없는경우
            {
                successPack.PackType = CONSTANTS.TYPE_ERROR;
                successPack.Flag = CONSTANTS.ERROR_INVALID_ID;
                serializer.Serialize(clientStream, successPack);
                Console.WriteLine("<!>없는 아이디 입력으로 로그인실패 / ID : {0} IP : {1}", reqId, ipString);
            }
            else if ( (string) sdr["pw"] != reqPw ) // else if (((int)sdr["pwd"]) == 0) // pw가 틀린경우
            {
                successPack.PackType = CONSTANTS.TYPE_ERROR;
                successPack.Flag = CONSTANTS.ERROR_INVALID_PASSWORD;
                serializer.Serialize(clientStream, successPack);
                Console.WriteLine("<!>비밀번호 불일치로 로그인실패 / ID : {0} IP : {1}", reqId, ipString);
            }
            else
            {
                loginGuid = Guid.NewGuid();

                if ( Program.userList.AddUser(reqId, loginGuid) )
                {
                    successPack.Flag = CONSTANTS.FLAG_SUCCESS;
                    serializer.Serialize(clientStream, successPack);

                    Console.WriteLine("<O>로그인 성공 / ID : {0} IP : {1} ", reqId, ipString);

                    loginId = reqId;
                    success = true;
                }
                else
                {
                    successPack.PackType = CONSTANTS.TYPE_ERROR;
                    successPack.Flag = CONSTANTS.ERROR_CONNECTED_ID;
                    serializer.Serialize(clientStream, successPack);

                    // 클라이언트에서 기존로그인을 끊기를 원하는지 여부를 체크한다.
                    reqPack = (Pack)serializer.Deserialize(clientStream);
                    if ( reqPack.Flag != CONSTANTS.FLAG_NO )
                    {
                        Program.userList.RemoveUser(reqId);
                        Program.userList.AddUser(reqId, loginGuid);

                        successPack.PackType = CONSTANTS.TYPE_BASIC;
                        successPack.Flag = CONSTANTS.FLAG_SUCCESS;
                        serializer.Serialize(clientStream, successPack);

                        Console.WriteLine("<O>로그인 성공 / ID : {0} IP : {1}", reqId, ipString);

                        loginId = reqId;
                        success = true;
                    }
                }
            }

            sdr.Close();
            return success;
        }

        public void CheckSign(ReqSignPack signPack)
        {
            Console.WriteLine("<->회원가입 요청 : {0}", ipString);
            // Pack에담긴 정보 담는 인스턴스
            string signId = ((ReqSignPack)signPack).Id;
            string signPw = ((ReqSignPack)signPack).Pw;
            string signEmail = ((ReqSignPack)signPack).Email;

            successPack.PackType = CONSTANTS.TYPE_ERROR;

            Regex regex_id = new Regex(@"^[a-zA-Z0-9]{4,20}$"); // 영문자 또는 숫자로 구성된 4~20 글자수
            Regex regex_email = new Regex(@"^[0-9a-zA-Z]([-_\.]?[0-9a-zA-Z])*@[0-9a-zA-Z]([-_\.]?[0-9a-zA-Z])*\.[a-zA-Z]{2,3}$");

            if (!(regex_id.IsMatch(signId))) // id 규칙이 위반시
            {
                successPack.Flag= CONSTANTS.ERROR_ID_AGUMENT;
                serializer.Serialize(clientStream, successPack);
                Console.WriteLine("<!>형식에 맞지않는 ID : {0}", ipString);
                return;
            }
            else if (!(regex_email.IsMatch(signEmail))) // 올바르지않은 email일시
            {
                successPack.Flag = CONSTANTS.ERROR_EMAIL_AGUMENT;
                serializer.Serialize(clientStream, successPack);
                Console.WriteLine("<!>유효하지않은 e-mail : {0}", ipString);
                return;
            }

            query.Clear();
            query.Append("select * from users where (id COLLATE Korean_Wansung_CS_AS ='");
            query.Append(signId);
            query.Append("' OR email COLLATE Korean_Wansung_CS_AS ='");
            query.Append(signEmail);
            query.Append("')");

            sdr = Program.ldbc.SelectDB(query);

            if ((sdr.Read())) // 이미 해당 id가 있다면
            {
                if (sdr["id"].ToString() == signId)
                    successPack.Flag = CONSTANTS.ERROR_DUPLICATE_ID;
                else
                    successPack.Flag = CONSTANTS.ERROR_DUPLICATE_EMAIL;

                serializer.Serialize(clientStream, successPack);
                Console.WriteLine("<!>아이디가 이미있으므로 회원가입 실패 / ID : {0} IP : ", signId, ipString);
                return;
            }

            sdr.Close();
            // ---------- 여기까지오면 계정을 추가해도 된다.
            // 테이블에 요청한 정보에따라 insert

            query.Clear();
            query.Append("insert into users(id, pw, email) values('");
            query.Append(signId);
            query.Append("', '" +signPw+ "', '");
            query.Append(signEmail);
            query.Append("')");

            Program.ldbc.InsertDB(query);

            successPack.PackType = CONSTANTS.TYPE_BASIC;
            Console.WriteLine("<->회원가입 성공 / ID : {0} IP : {1} ", signId, ipString);
            serializer.Serialize(clientStream, successPack);

            sdr.Close();
        }

        private void CheckLogin()
        {
            while (true)
            {

                if ( !(Program.userList.CorrectLogin(loginId, loginGuid)) )
                {
                    Pack resPack = new Pack();
                    resPack.PackType = CONSTANTS.TYPE_ERROR;
                    resPack.Flag = CONSTANTS.ERROR_ANOTHER_LOGIN;
                    serializer.Serialize(clientStream, resPack);
                    throw new Exception(string.Format("<!>다른 곳에서 로그인 {0}", ipString));
                }
                else
                {
                    Pack resPack = new Pack();
                    resPack.PackType = CONSTANTS.FLAG_SUCCESS;
                    resPack.Flag = CONSTANTS.FLAG_NORMAL;
                    serializer.Serialize(clientStream, resPack);
                }

                Thread.Sleep(1000);
            }
        }

        public void Working()
        {
            try
            {
                while (true)
                {
                    Pack reqPack = (Pack)serializer.Deserialize(clientStream);
                    Console.WriteLine( reqPack.PackType );

                    if (reqPack.PackType == CONSTANTS.TYPE_REQ_LOGIN)
                    {
                        if (LoginCheck((ReqLoginPack)reqPack))
                        {
                            CheckLogin();
                        }
                    }
                    else if (reqPack.PackType == CONSTANTS.TYPE_REQ_SIGNUP)
                    {
                        CheckSign((ReqSignPack)reqPack);
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                //if (loginId != null)
                //    Program.userList.RemoveUser(loginId);

                if (sdr != null)
                    sdr.Close();

                clientStream.Close();
                client.Close();
                Console.WriteLine("<X> 연결종료/ ID : {0} IP : {1}", loginId, ipString);
            }
        }

    }
}
