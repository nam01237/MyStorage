using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data.SqlClient;
using WHP;
using System.Text.RegularExpressions;

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

        // === 생성자 === //
        public LoginSignWork(TcpClient client)
        {
            this.client = client;
            clientStream = client.GetStream();
            serializer = new BinaryFormatter(); // 직렬화기
            ipString = ((IPEndPoint)client.Client.RemoteEndPoint).ToString();
            successPack = new Pack();

        }

        public bool LoginCheck(ReqLoginPack logPack)
        {
            Console.WriteLine("<->로그인 요청 : {0}", ipString);

            bool success = false;
            string reqId = logPack.ID;
            string reqPw = logPack.PW;
            string query = "select *, PWDCOMPARE('" + reqPw + "', pw) as pwd " +
                "from users " +
                "where id COLLATE Korean_Wansung_CS_AS = '" + reqId + "'";

            sdr = Program.ldbc.SelectDB(query);

            if (!(sdr.Read())) // 요청한 id가 회원테이블에 없는경우
            {
                successPack.PACK_TYPE = CONSTANTS.TYPE_ERROR;
                successPack.FLAG = CONSTANTS.ERROR_INVALID_ID;
                serializer.Serialize(clientStream, successPack);
                Console.WriteLine("<!>없는 아이디 입력으로 로그인실패 / ID : {0} IP : {1}", reqId, ipString);
            }
            else if (((int)sdr["pwd"]) == 0) // pw가 틀린경우
            {
                successPack.PACK_TYPE = CONSTANTS.TYPE_ERROR;
                successPack.FLAG = CONSTANTS.ERROR_INVALID_PASSWORD;
                serializer.Serialize(clientStream, successPack);
                Console.WriteLine("<!>비밀번호 불일치로 로그인실패 / ID : {0} IP : {1}", reqId, ipString);
            }
            else
            {
                loginGuid = Guid.NewGuid();

                if ( Program.userList.AddUser(reqId, loginGuid) )
                {
                    successPack.FLAG = CONSTANTS.FLAG_SUCCESS;
                    serializer.Serialize(clientStream, successPack);

                    Console.WriteLine("<O>로그인 성공 / ID : {0} IP : {1} ", reqId, ipString);

                    loginId = reqId;
                    success = true;
                }
                else
                {
                    successPack.PACK_TYPE = CONSTANTS.TYPE_ERROR;
                    successPack.FLAG = CONSTANTS.ERROR_CONNECTED_ID;
                    serializer.Serialize(clientStream, successPack);

                    // 클라이언트에서 기존로그인을 끊기를 원하는지 여부를 체크한다.
                    reqPack = (Pack)serializer.Deserialize(clientStream);
                    if ( reqPack.FLAG != CONSTANTS.FLAG_NO )
                    {
                        Program.userList.RemoveUser(reqId);
                        Program.userList.AddUser(reqId, loginGuid);

                        successPack.PACK_TYPE = CONSTANTS.TYPE_BASIC;
                        successPack.FLAG = CONSTANTS.FLAG_SUCCESS;
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
            string signId = ((ReqSignPack)signPack).ID;
            string signPw = ((ReqSignPack)signPack).PW;
            string signEmail = ((ReqSignPack)signPack).EMAIL;
            string query;

            successPack.PACK_TYPE = CONSTANTS.TYPE_ERROR;

            Regex regex_id = new Regex(@"^[a-zA-Z0-9]{4,20}$"); // 영문자 또는 숫자로 구성된 4~20 글자수
            Regex regex_email = new Regex(@"^[0-9a-zA-Z]([-_\.]?[0-9a-zA-Z])*@[0-9a-zA-Z]([-_\.]?[0-9a-zA-Z])*\.[a-zA-Z]{2,3}$");

            if (!(regex_id.IsMatch(signId))) // id 규칙이 위반시
            {
                successPack.FLAG = CONSTANTS.ERROR_ID_AGUMENT;
                serializer.Serialize(clientStream, successPack);
                Console.WriteLine("<!>형식에 맞지않는 ID : {0}", ipString);
                return;
            }
            else if (!(regex_email.IsMatch(signEmail))) // 올바르지않은 email일시
            {
                successPack.FLAG = CONSTANTS.ERROR_EMAIL_AGUMENT;
                serializer.Serialize(clientStream, successPack);
                Console.WriteLine("<!>유효하지않은 e-mail : {0}", ipString);
                return;
            }

            query = "select * from users where (id COLLATE Korean_Wansung_CS_AS ='" + signId + "'OR email COLLATE Korean_Wansung_CS_AS ='" + signEmail + "')";

            sdr = Program.ldbc.SelectDB(query);

            if ((sdr.Read())) // 이미 해당 id가 있다면
            {
                if (sdr["id"].ToString() == signId)
                    successPack.FLAG = CONSTANTS.ERROR_DUPLICATE_ID;
                else
                    successPack.FLAG = CONSTANTS.ERROR_DUPLICATE_EMAIL;

                serializer.Serialize(clientStream, successPack);
                Console.WriteLine("<!>아이디가 이미있으므로 회원가입 실패 / ID : {0} IP : ", signId, ipString);
                return;
            }

            sdr.Close();
            // ---------- 여기까지오면 계정을 추가해도 된다.
            // 테이블에 요청한 정보에따라 insert
            query = "insert into users(id, pw, email, logch) values('" + signId + "', PWDENCRYPT('" + signPw + "'), '" + signEmail + "', 'N')";
            Program.ldbc.InsertDB(query);

            successPack.PACK_TYPE = CONSTANTS.TYPE_BASIC;
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
                    resPack.PACK_TYPE = CONSTANTS.TYPE_ERROR;
                    resPack.FLAG = CONSTANTS.ERROR_ANOTHER_LOGIN;
                    serializer.Serialize(clientStream, resPack);
                    throw new Exception(string.Format("<!>다른 곳에서 로그인 {0}", ipString));
                }
                else
                {
                    Pack resPack = new Pack();
                    resPack.PACK_TYPE = CONSTANTS.FLAG_SUCCESS;
                    resPack.FLAG = CONSTANTS.FLAG_NORMAL;
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
                    Console.WriteLine( reqPack.PACK_TYPE );

                    if (reqPack.PACK_TYPE == CONSTANTS.TYPE_REQ_LOGIN)
                    {
                        if (LoginCheck((ReqLoginPack)reqPack))
                        {
                            CheckLogin();
                        }
                    }
                    else if (reqPack.PACK_TYPE == CONSTANTS.TYPE_REQ_SIGNUP)
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
                if (loginId != null)
                    Program.userList.RemoveUser(loginId);

                if (sdr != null)
                    sdr.Close();

                clientStream.Close();
                client.Close();
                Console.WriteLine("<X> 연결종료/ ID : {0} IP : {1}", loginId, ipString);
            }
        }

        //public void LoginService()
        //{
        //    Pack successPack = new Pack(); // 성공/실패 여부를 보내주는 Pack
        //    Pack resMait = new Pack();     // 로그인 유지를 위해 건내줄 Pack
        //    Pack reqMait = new Pack();     // 로그인 유지를 위해 받을 Pack

        //    Pack reqPack = (Pack)serializer.Deserialize(clientStream); // 처음 요청 Pack 받는다.

        //    if (reqPack.PACK_TYPE == CONSTANTS.TYPE_REQ_LOGIN) // =============== 로그인 요청처리
        //    {
        //        Console.WriteLine("<->로그인 요청 : {0}", ipString);
        //        try
        //        {
        //            id = ((ReqLoginPack)reqPack).ID;         // 요청에 담긴 id, pw 저장
        //            string pw = ((ReqLoginPack)reqPack).PW;

        //            cmd.CommandText = "select *, PWDCOMPARE('" + pw + "', pw) as pwc from users where id COLLATE Korean_Wansung_CS_AS = '" + id + "'";
        //            sdr = cmd.ExecuteReader();  // ㄴ해당 id존재 여부와 pw 일치여부 검사 쿼리 전달

        //            if (!(sdr.Read())) // 요청한 id가 회원테이블에 없는경우
        //            {
        //                successPack.PACK_TYPE = CONSTANTS.TYPE_ERROR;
        //                successPack.FLAG = CONSTANTS.ERROR_INVALID_ID;
        //                serializer.Serialize(clientStream, successPack);
        //                Console.WriteLine("<!>없는 아이디 입력으로 로그인실패 / ID : {0} IP : {1}", ((ReqLoginPack)reqPack).ID, ipString);
        //                sdr.Close();
        //                return;
        //            }
        //            else if (((int)sdr["pwc"]) == 0) // pw가 틀린경우
        //            {
        //                successPack.PACK_TYPE = CONSTANTS.TYPE_ERROR;
        //                successPack.FLAG = CONSTANTS.ERROR_INVALID_PASSWORD;
        //                serializer.Serialize(clientStream, successPack);
        //                Console.WriteLine("<!>비밀번호 불일치로 로그인실패 / ID : {0} IP : {1}", ((ReqLoginPack)reqPack).ID, ipString);
        //                sdr.Close();
        //                return;
        //            }
        //            else
        //            {
        //                if (sdr["logch"].ToString().Equals("Y")) // 이미 해당 id로 다른곳에서 로그인을 한 경우
        //                {
        //                    Console.WriteLine("<!>중복 로그인 시도 / ID : {0} IP :", ((ReqLoginPack)reqPack).ID, ipString);
        //                    // 일단 중복로그인임을 클라이언트에 알린다.
        //                    successPack.PACK_TYPE = CONSTANTS.TYPE_ERROR;
        //                    successPack.FLAG = CONSTANTS.ERROR_CONNECTED_ID;
        //                    serializer.Serialize(clientStream, successPack);

        //                    // 클라이언트에서 기존로그인을 끊기를 원하는지 여부를 체크한다.
        //                    reqPack = (Pack)serializer.Deserialize(clientStream);
        //                    if (reqPack.FLAG == CONSTANTS.FLAG_NO)
        //                        return; // 원하지 않으면 그대로 종료

        //                    // 로그아웃 대기 리스트에 해당 id를 추가한다.
        //                    lock (thisLock)
        //                    {
        //                        logoutList.Add(id);
        //                    }

        //                    // 해당 id가 로그아웃 리스트에서 사라질때까지 반복문 수행
        //                    bool loop = true;
        //                    while (loop)
        //                    {
        //                        loop = false;
        //                        lock (thisLock)
        //                        {
        //                            foreach (string s in logoutList)
        //                            {
        //                                if (s == id)
        //                                {
        //                                    loop = true;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                    }
        //                    // 로그인 성공 응답 보낸다
        //                    successPack.PACK_TYPE = CONSTANTS.TYPE_BASIC;
        //                    successPack.FLAG = CONSTANTS.FLAG_SUCCESS;
        //                    serializer.Serialize(clientStream, successPack);
        //                }
        //                else
        //                {
        //                    // 중복 로그인이 아닐경우 그냥간다.
        //                    successPack.FLAG = CONSTANTS.FLAG_SUCCESS;
        //                    serializer.Serialize(clientStream, successPack);

        //                }

        //            }
        //            // Login Success ------------- 여기까지 return을 안만났다면 로그인이 성공한것.
        //            Console.WriteLine("<->로그인 성공 / ID:{0} IP:{1}", ((ReqLoginPack)reqPack).ID, ipString);
        //            // 로그인 여부를 Y로 바꾸어준다.
        //            sdr.Close();
        //            cmd.CommandText = "UPDATE users set logch='Y' where id COLLATE Korean_Wansung_CS_AS ='" + id + "'";
        //            cmd.ExecuteNonQuery();

        //            // 로그인 유지를위해 Pack 주고받는 반복문
        //            while (true)
        //            {
        //                for (int i = 0; i < logoutList.Count; i++) // 로그아웃 대기 리스트 검색해가며 로그인유지
        //                {
        //                    if (id == logoutList[i])
        //                    {
        //                        throw new ComeAnotherLoginException(); // 만약 중복로그인이 감지되는경우 사용자지정 예외 발생
        //                    }
        //                }

        //                reqMait = (Pack)serializer.Deserialize(clientStream); // 받고
        //                serializer.Serialize(clientStream, resMait);          // 주고

        //                Thread.Sleep(500);
        //            }
        //        }
        //        catch (ComeAnotherLoginException)
        //        {
        //            resMait.PACK_TYPE = CONSTANTS.TYPE_ERROR;
        //            resMait.FLAG = CONSTANTS.ERROR_ANOTHER_LOGIN;
        //            serializer.Serialize(clientStream, resMait);
        //            Console.WriteLine("<!>다른 로그인으로 연결 종료 / ID:{0} IP:{1}", ((ReqLoginPack)reqPack).ID, ipString);
        //            aLogin = true; // 중복 로그인으로 종료되면 true로
        //            lock (thisLock)
        //            {
        //                logoutList.Remove(id);
        //            }
        //        }
        //        catch (IOException)
        //        {
        //            Console.WriteLine("<!>연결 종료 / ID:{0} IP:{1}", ((ReqLoginPack)reqPack).ID, ipString);
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e);
        //            Console.WriteLine("<!>연결 종료 / ID:{0} IP:{1}", ((ReqLoginPack)reqPack).ID, ipString);
        //        }
        //        finally
        //        {
        //            if (!aLogin) // 중복로그인으로 종료되는경우 테이블의 로그인여부를 바꿔주지 않는다.
        //            {
        //                cmd.CommandText = "UPDATE users SET logch='N' where id COLLATE Korean_Wansung_CS_AS ='" + id + "'";
        //                cmd.Connection = conn;
        //                cmd.ExecuteNonQuery();
        //            }
        //            sdr.Close();
        //            conn.Close();
        //            clientStream.Close();
        //            client.Close();
        //        }
        //    }
        //    else if (reqPack.PACK_TYPE == CONSTANTS.TYPE_REQ_REGISTER) // =================== 회워가입 요청처리
        //    {
        //        Console.WriteLine("<->회원가입 요청 : {0}", ipString);
        //        // Pack에담긴 정보 담는 인스턴스
        //        string signId = ((ReqRegisterPack)reqPack).ID;
        //        string signPw = ((ReqRegisterPack)reqPack).PW;
        //        string signEmail = ((ReqRegisterPack)reqPack).EMAIL;

        //        successPack.PACK_TYPE = CONSTANTS.TYPE_ERROR;

        //        try
        //        {   // 회원가입 요청한 id가 이미 있는지 여부를 검사한다.
        //            cmd.CommandText = "select * from users where (id COLLATE Korean_Wansung_CS_AS ='" + signId + "'OR email COLLATE Korean_Wansung_CS_AS ='" + signEmail + "')";
        //            sdr = cmd.ExecuteReader();

        //            // 이부분은 정규식으로 id와 email을 검사하는 부분이다.
        //            Regex regex_id = new Regex(@"^[a-zA-Z0-9]{4,20}$"); // 영문자 또는 숫자로 구성된 4~20 글자수
        //            Regex regex_email = new Regex(@"^[0-9a-zA-Z]([-_\.]?[0-9a-zA-Z])*@[0-9a-zA-Z]([-_\.]?[0-9a-zA-Z])*\.[a-zA-Z]{2,3}$"); // email 무결성검사 (그냥 퍼왔음)
        //            if (!(regex_id.IsMatch(signId))) // id 규칙이 위반시
        //            {
        //                successPack.FLAG = CONSTANTS.ERROR_ID_AGUMENT;
        //                serializer.Serialize(clientStream, successPack);
        //                Console.WriteLine("<!>형식에 맞지않는 ID : {0}", ipString);
        //                return;
        //            }
        //            else if (!(regex_email.IsMatch(signEmail))) // 올바르지않은 email일시
        //            {
        //                successPack.FLAG = CONSTANTS.ERROR_EMAIL_AGUMENT;
        //                serializer.Serialize(clientStream, successPack);
        //                Console.WriteLine("<!>유효하지않은 e-mail : {0}", ipString);
        //                return;
        //            }

        //            if ((sdr.Read())) // 이미 해당 id가 있다면
        //            {
        //                if (sdr["id"].ToString() == signId)
        //                    successPack.FLAG = CONSTANTS.ERROR_DUPLICATE_ID;
        //                else
        //                    successPack.FLAG = CONSTANTS.ERROR_DUPLICATE_EMAIL;

        //                serializer.Serialize(clientStream, successPack);
        //                Console.WriteLine("<!>아이디가 이미있으므로 회원가입 실패 / ID : {0} IP : ", signId, ipString);
        //                return;
        //            }

        //            // ---------- 여기까지오면 계정을 추가해도 된다.
        //            // 테이블에 요청한 정보에따라 insert
        //            sdr.Close();
        //            Console.WriteLine(signPw);
        //            cmd.CommandText = "insert into users(id, pw, email, logch) values('" + signId + "', PWDENCRYPT('" + signPw + "'), '" + signEmail + "', 'N')";
        //            cmd.ExecuteNonQuery();

        //            successPack.PACK_TYPE = CONSTANTS.TYPE_BASIC;
        //            Console.WriteLine("<->회원가입 성공 / ID : {0} IP : {1} ", signId, ipString);
        //            serializer.Serialize(clientStream, successPack);
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e);
        //            successPack.PACK_TYPE = CONSTANTS.TYPE_ERROR;
        //            successPack.FLAG = CONSTANTS.ERROR_UNKNOWN_ERROR;
        //            serializer.Serialize(clientStream, successPack);
        //            Console.WriteLine("<!>연결 종료 / ID : {0} IP : {1}", ((ReqLoginPack)reqPack).ID, ipString);
        //        }
        //        finally
        //        {
        //            sdr.Close();
        //            conn.Close();
        //            clientStream.Close();
        //            client.Close();
        //        }

        //    }
        //}


    }
}
