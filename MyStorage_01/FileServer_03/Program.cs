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
    class ServiceUtil // 서버요청을 받아서 요청유형별로 처리해서 보내주는 기능을하는 클래스
    {
        private TcpClient client;                       // 요청해온 클라이언트
        private NetworkStream clientStream;             // 클라이언트에 대한 스트림
        private FileStream fileStream;                  // 서버에서 파일다룰때 쓸 스트림
        BinaryFormatter serializer;                     // 직렬화를 담당한다.

        private TreeView tempTree = new TreeView();     // 클라이언트에 보낼 TreeViewNode(디렉토리 경로 나타냄) 임시로 붙일 TreeView 
        const string rootPath = "D:\\MyStorage\\";                 // 서버 파일들 저장하는 경로 (D:\\계정명 ~ )

        // === 생성자 === //
        public ServiceUtil(TcpClient client) // 요청을 보낸 클라이언트 정보로 TcpClient, NetStream초기화
        {
            this.client = client;
            this.clientStream = client.GetStream();
            this.serializer = new BinaryFormatter();
        }

        // === Service() === //
        public void Service() // 요청의 타입에따라 알맞은 작업을 하도록 메소드를 부른다.
        {
            try
            {
                Pack reqPack = (Pack)serializer.Deserialize(clientStream); 
                Console.WriteLine("<->요청확인 : {0}", ((IPEndPoint)client.Client.RemoteEndPoint).ToString());

                switch (reqPack.PackType) // 요청을 받아서 타입에맞게 메소드에 뿌려주는 switch문
                {
                    case CONSTANTS.TYPE_REQ_STARTNODE:
                        ResNode((ReqStartNodePack)reqPack);
                        break;
                    case CONSTANTS.TYPE_REQ_DIRECROTY:
                        ResDirInfo((ReqDirInfoPack)reqPack);
                        break;
                    case CONSTANTS.TYPE_REQ_NEWDIR:
                        MakeNewDir((ReqNewDirPack)reqPack);
                        break;
                    case CONSTANTS.TYPE_REQ_DELETE:
                        DeleteFile((ReqDeletePack)reqPack);
                        break;
                    case CONSTANTS.TYPE_REQ_RENAME:
                        ReNameFile((ReqReNamePack)reqPack);
                        break;
                    case CONSTANTS.TYPE_REQ_UPLOAD:
                        ReceiveFileData((ReqUpLoadPack)reqPack);
                        break;
                    case CONSTANTS.TYPE_REQ_DOWNLOAD:
                        SendFileData((ReqDownLoadPack)reqPack);
                        break;
                    default:
                        Console.WriteLine("<!>알 수 없는 메시지");
                        break;
                }

            }
            catch (IOException)
            {
                Console.WriteLine("<->연결종료 IP : {0}", ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("<!>오류로 연결 종료 IP : {0}", ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
            }
            finally
            {
                clientStream.Close(); // 리소스 닫아준다.
                if (fileStream != null)
                {
                    fileStream.Close();
                }
            }
        }

        // === MakeTree() === //
        private void MakeTreeNode(TreeNode node) // 보낼 노드를 디렉토리 경로별로 만든다.
        {
            string path = rootPath + node.FullPath;

            DirectoryInfo di = new DirectoryInfo(path);
            foreach (DirectoryInfo d in di.GetDirectories()) // 하위노드를 검색해나가며 노드에 붙인다.
            {
                string s = d.Name;
                TreeNode cNode = node.Nodes.Add(s);
                MakeTreeNode(cNode);
            }
        }

        // ==== ResNode() (TYPE_REQ_LOGIN) ==== //
        private void ResNode(ReqStartNodePack reqPack) // 클라이언트에 초기노드 만들어서 보내는 응답
        {
            TreeNode node = new TreeNode(reqPack.Id); // 보낼 임시노드
            if (!(Directory.Exists(rootPath + reqPack.Id))) // 가입 후 첫 접속일경우 ID이름의 폴더가없으므로 만들어준다
            {
                Directory.CreateDirectory(rootPath + reqPack.Id);
            }
            tempTree.Nodes.Add(node); // 노드에 노드를 추가하기위해서 TreeView에 추가를 해줘야하더라
            MakeTreeNode(node); // 경로검색해서 노드들 붙이는 메소드
            ResStartNode resPack = new ResStartNode
            {
                ROOT_NODE = node
            };
            serializer.Serialize(clientStream, resPack);
            Console.WriteLine("<->시작 노드전송 : {0} ", ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
        }

        // ===  ResDirInfo() (TYPE_REQ_DIRECTORY) === //
        private void ResDirInfo(ReqDirInfoPack reqPack) // 선택한 디렉토리에 있는 파일과 하위폴더정보를 보내준다.
        {
            string path = rootPath + reqPack.Path;
            ResDirInfoPack resPack = new ResDirInfoPack();   // 디렉토리 정보담는 Pack
            FileInfoStructure fis = new FileInfoStructure(); // 파일정보 담는 구조체 인스턴스 ->WHP참조

            DirectoryInfo di = new DirectoryInfo(path);

            foreach (DirectoryInfo d in di.GetDirectories()) // 하위 디렉토리 정보담는다.
            {
                if ((d.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) // 숨김폴더는 제외
                    continue;

                fis.FileName = d.Name;
                fis.AccessDate = d.LastAccessTime.ToString();
                fis.FileSize = 0;
                fis.FileType = 'd';

                resPack.FilesInfo.Add(fis); // FilesInfo는 List
            }

            foreach (FileInfo fi in di.GetFiles()) // 파일들 정보담는다.
            {
                fis.FileName = fi.Name;
                fis.FileSize = fi.Length;
                fis.AccessDate = fi.LastWriteTime.ToString();
                fis.FileType = 'f';

                resPack.FilesInfo.Add(fis);
            }


            serializer.Serialize(clientStream, resPack);

            Console.WriteLine("<->디렉토리 정보 전송 : {0} ", ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
        }

        // === MakeNewDir() (TYPE_REQ_NEWDIR) === //
        private void MakeNewDir(ReqNewDirPack reqPack) // 새로운 디렉토리를 만든다.
        {
            string path = rootPath + reqPack.Path;
            Pack resPack = new Pack(); // 작업결과 응답할 Pack

            try
            {
                if (Directory.Exists(path)) // 같은이름의 폴더있으면 실패
                {
                    resPack.PackType = CONSTANTS.TYPE_ERROR;
                    resPack.Flag = CONSTANTS.ERROR_EXIST_DIR;
                    serializer.Serialize(clientStream, resPack);
                    return;
                }

                Directory.CreateDirectory(path);

            }
            catch (ArgumentException) // 윈도우 폴더이름 규칙위반
            {
                resPack.PackType = CONSTANTS.TYPE_ERROR;
                resPack.Flag = CONSTANTS.ERROR_INVALID_CHAR;
            }

            serializer.Serialize(clientStream, resPack);
            Console.WriteLine("<->새디렉토리 생성 : {0} ", path);
        }

        // === DeleteFile (TYPE_REQ_DELETE) === //
        private void DeleteFile(ReqDeletePack reqPack) // 디렉토리/파일 삭제
        {
            Pack resPack = new Pack();
            reqPack.Flag = CONSTANTS.FLAG_SUCCESS;

            if (reqPack.FileType == 'd') // 디렉토리를 상제하는경우
            {
                Directory.Delete(rootPath + reqPack.Path, true);
            }
            else if (reqPack.FileType == 'f')
            {
                File.Delete(rootPath + reqPack.Path); // 파일을 삭제하는경우
            }

            serializer.Serialize(clientStream, resPack);
            Console.WriteLine("<->디렉토리/파일 삭제 : {0} ", rootPath + reqPack.Path);
        }

        // === ReNameFile() (TYPE_REQ_RENAME) ==== //
        private void ReNameFile(ReqReNamePack reqPack) // 디렉토리/파일 이름변경
        {
            Pack resPack = new Pack();
            string prevPath = rootPath + "\\" + reqPack.PrevName; // 원래 경로
            string ensuPath = rootPath + "\\" + reqPack.ReName; // 바꿀 경로
            reqPack.Flag = CONSTANTS.FLAG_SUCCESS;

            try
            {
                if (reqPack.FileType == 'd')
                {
                    if (Directory.Exists(ensuPath))
                    {
                        resPack.PackType = CONSTANTS.TYPE_ERROR;
                        resPack.Flag = CONSTANTS.ERROR_EXIST_DIR;
                        serializer.Serialize(clientStream, resPack);
                        return;
                    }

                    DirectoryInfo di = new DirectoryInfo(prevPath);
                    di.MoveTo(ensuPath); // 이름바꾸기
                }
                else if (reqPack.FileType == 'f')
                {
                    string ext = prevPath.Substring(prevPath.LastIndexOf('.'), prevPath.Length - prevPath.LastIndexOf('.')); // 확장자명 추출

                    if (File.Exists(ensuPath + ext))
                    {
                        resPack.PackType = CONSTANTS.TYPE_ERROR;
                        resPack.Flag = CONSTANTS.ERROR_EXIST_FILE;
                        serializer.Serialize(clientStream, resPack);
                        return;
                    }
                    FileInfo fi = new FileInfo(prevPath);
                    fi.MoveTo(ensuPath + ext); // 이름바꾸기
                }
            }
            catch (ArgumentException)
            {
                resPack.PackType = CONSTANTS.TYPE_ERROR;
                resPack.Flag = CONSTANTS.ERROR_INVALID_CHAR;
            }

            serializer.Serialize(clientStream, resPack);
            Console.WriteLine("<->디렉토리/파일 이름 변경 : {0} -> {1} ", prevPath, ensuPath);
        }

        // === ReceiveFileData() (TYPE_REQ_UPLOAD) === //
        private void ReceiveFileData(ReqUpLoadPack reqPack) // 요청한 파일 데이터를 받는다. (업로드)
        {
            fileStream = new FileStream(rootPath + reqPack.Path, FileMode.Create); // 요청경로의 파일스트림
            SendDataPack resPack = new SendDataPack(); // 파일 데이터를 계속해서 담는 인스턴스
            ulong totalSize = reqPack.FileSize; // 원래 파일크기
            long recvSize = 0;                  // 받은 크기

            Pack flagPack = new Pack() // 파일전송을 유지여부를 결정하기 위한 Pack 계속받아서 판별한다.
            {
                Flag = CONSTANTS.FLAG_YES
            };

            serializer.Serialize(clientStream, flagPack); // 시작신호 보낸다.
            Console.WriteLine("<->업로드 시작 : {0} ", ((IPEndPoint)client.Client.RemoteEndPoint).ToString());

            while ((resPack = (SendDataPack)serializer.Deserialize(clientStream)) != null) // 받을 Pack이 없을때까지 반복
            {
                fileStream.Write(resPack.Data, 0, resPack.Data.Length); // 받은 Pack의 DATA[]을 파일로쓴다.
                recvSize += resPack.Data.Length; // 받은크기 늘린다.

                if (resPack.Last == CONSTANTS.LAST) // LAST == LAST는 파일의 끝을의미
                    break;
            }

            Console.WriteLine();

            if ((ulong)recvSize == totalSize) // 받은사이즈 == 원래파일크기
            {
                Console.WriteLine("<->업로드 성공 : {0} ", ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
                Console.WriteLine(" ({0} / {1}) ", recvSize, totalSize);
            }
            else
            {
                Console.WriteLine("<!>업로드 중지 : {0} ", ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
                Console.WriteLine(" ({0} / {1}) ", recvSize, totalSize);
            }

            fileStream.Close();
        }

        // === SendFileData() (TYPE_REQ_DOWNLOAD) === //
        private void SendFileData(ReqDownLoadPack reqPack) // 요청한 파일 데이터를 보낸다. (다운로드)
        {
            string path = rootPath + reqPack.Path;
            Pack flagPack = new Pack();

            using (Stream fileStream = new FileStream(path, FileMode.Open)) // 전송요청 받은 파일을 연다.
            {
                byte[] buffer = new byte[1024 * 1024]; // 한번에 읽어들일 바이트수 (1MB)
                int totalRead = 0; // 파일로부터 읽어들은 총 크기

                Console.WriteLine(" === 다운로드 시작 : {0} === ", ((IPEndPoint)client.Client.RemoteEndPoint).ToString());

                while (true)
                {
                    flagPack = (Pack)serializer.Deserialize(clientStream); // 전송을 계속할지 여부를 클라이언트 요청 받으면서 진행
                    if (flagPack.Flag == CONSTANTS.FLAG_NO) 
                        break;

                    int read = fileStream.Read(buffer, 0, (1024 * 1024)); // 파일로부터 데이터 읽는다.
                    totalRead += read;
                    SendDataPack resPack = new SendDataPack()
                    {
                        Last = CONSTANTS.NOT_LAST,
                        Data = new byte[read]
                    };

                    Array.Copy(buffer, 0, resPack.Data, 0, read); // 파일로부터 읽은 데이터 보낼 DATA[]로 복사 

                    if (totalRead >= fileStream.Length) // 총 읽은크기가 파일의 크기 이상일때 (다 읽었을때)
                    {
                        resPack.Last = CONSTANTS.LAST; // 전송의 끝을알림
                        serializer.Serialize(clientStream, resPack);
                        break;
                    }

                    serializer.Serialize(clientStream, resPack);
                }

                Console.WriteLine();

                if (totalRead == fileStream.Length)
                {
                    Console.WriteLine("<->다운로드 성공 : {0}", ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
                    Console.WriteLine(" ({0} / {1}) ", totalRead, fileStream.Length);
                }
                else
                {
                    Console.WriteLine("<!>다운로드 중지 : {0} ", ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
                    Console.WriteLine(" ({0} / {1}) ", totalRead, fileStream.Length);
                }
            }
        }
    }

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
