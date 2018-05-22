using System;
using WHP;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

/*
 * 중간중간 있는 ~~Pack 클래스들은 WHP 참조 
 * 
 */

namespace Client_03
{
    public partial class MainForm : Form
    {
        // 네트워크 관련
        private IPEndPoint fileAdress = new IPEndPoint(IPAddress.Parse("192.168.101.128"), 4454);   // 파일서버 주소
        private IPEndPoint loginAdress = new IPEndPoint(IPAddress.Parse("192.168.101.128"), 4453);  // 로그인서버 주소 
        private IPEndPoint clientAdress = new IPEndPoint(0, 0);  // 클라이언트 (이 프로그램) 주소
        private string downPath = "";   // 다운로드 경로 저장하는 문자열
        // 
        private TreeNode selectedNode = new TreeNode();                     // 좌측 TreeView에서 선택한 노드(폴더)
        private ListViewItem selectedItem = new ListViewItem();             // 우측 ListView에서 선택한 아이템(파일/폴더)
        private string selectedPath = "";                                   // 선택한 노드/아이템으로 경로를 저장
        private bool runUp = false;                                         // 현재 업로드를 수행중인지 체크 (취소 할때 사용)
        private string userID;                                              // 접속한 ID
        public string input = "";                                           // 새 폴더/파일명 변경에 쓰이는 입력 문자열
        public List<DownLoadWindow> dwList = new List<DownLoadWindow>();    // 이 프로그램에서 실행된 다운로드창을 저장 (강종할때 전부 닫기위해서)
        private LoginForm loginForm = new LoginForm();                      // 로그인 폼
        private RegisterForm regForm = new RegisterForm();                  // 회원가입 폼

        public MainForm()
        {
            InitializeComponent();
        }

        // fileConnect()  // 
        private TcpClient fileConnect() // 파일서버에 연결하고 연결한 TcpClient 인스턴스 반환
        {
            TcpClient client = new TcpClient();

            client = new TcpClient(clientAdress);
            client.Connect(fileAdress);

            return client;
        }

        private void Form1_Shown(object sender, EventArgs e) // 처음 메인폼이 켜졌을때, 로그인폼을 띄우고 초기 경로 등을 세팅한다.
        {
            loginForm.ShowDialog(this);

            string startPath = Application.StartupPath;  // 이 프로그램이 실행되는 경로
            if (startPath.Substring(startPath.Length - 1) == "\\") // 프로그램을 드라이브에서(C:\\ D:\\ ...) 실행하는지에 따라 다운로드 경로가다르다.
                this.txt_DownLoadPath.Text = startPath + "Download";
            else
                this.txt_DownLoadPath.Text = startPath + "\\Download";

            downPath = txt_DownLoadPath.Text;   // 만약 실행경로에 Download 폴더가 없으면 만들어준다.
            if (!(Directory.Exists(downPath)))
            {
                Directory.CreateDirectory(downPath);
            }
        }

        private void RefreshList(string path) // 선택한 폴더가 바뀌거나, 새 폴더를 추가하는 작업이있을때 ListView를 갱신한다.
        {
            BinaryFormatter serializer = new BinaryFormatter();
            TcpClient client = fileConnect();
            NetworkStream stream = client.GetStream();

            double size;
            string stringSize;
            string dirName = path.Substring(path.LastIndexOf("\\") + 1, path.Length - (path.LastIndexOf("\\") + 1));

            ReqDirInfoPack reqPack = new ReqDirInfoPack(); // 폴더에있는 파일/하위폴더 정보를 요청할 Pack
            reqPack.Path = path; //
            serializer.Serialize(stream, reqPack);

            ResDirInfoPack resPack = (ResDirInfoPack)serializer.Deserialize(stream);
            
            list_File.Items.Clear();    // ListView 초기화
            selectedItem = null;        // 선택 아이템 없음
            btn_Rename.Enabled = false; // 이름바꾸기
            btn_Delete.Enabled = false; // 삭제 버튼 비활성화

            // --- 용량 단위 변환 ---//
            foreach (FileInfoStructure fis in resPack.FilesInfo)
            {
                int img = 0; // 파일 아이콘 인덱스 (0)
                if (fis.FileType == 'd') // 폴더 아이콘 인덱스 (1)
                {
                    stringSize = "";
                    img = 1;
                }
                else if (fis.FileSize >= 1024 * 1024 * 1024)
                {
                    size = (double)fis.FileSize / (double)(1024 * 1024 * 1024);
                    stringSize = string.Format("{0:F2}GB", size);
                }
                else if (fis.FileSize >= 1024 * 1024)
                {
                    size = fis.FileSize / (1024 * 1024);
                    stringSize = string.Format("{0}MB", size);
                }
                else if (fis.FileSize >= 1024)
                {
                    size = fis.FileSize / 1024;
                    stringSize = string.Format("{0}KB", size);
                }
                else
                {
                    stringSize = "1KB";
                }
                // -------------------//

                // 리스트 뷰에 추가할 아이템 만듬 //
                ListViewItem item = new ListViewItem(new string[]
                {
                    fis.FileName,
                    fis.AccessDate,
                    stringSize,
                    string.Format("{0}",fis.FileType),
                    string.Format("{0}", fis.FileSize)
                }, img);

                list_File.Items.Add(item);
            }

            stream.Close(); client.Close();
        }

        private void tree_Directory_AfterSelect(object sender, TreeViewEventArgs e) // 선택한 폴더를 트리노드를 바꾼다.
        {
            selectedNode = ((TreeView)sender).SelectedNode;
            selectedPath = selectedNode.FullPath;
            selectedNode.Expand(); // 선택하면 펼쳐준다.
            RefreshList(selectedPath); // 선택한 폴더가 바뀌었으니 ListView도 초기화
        }

        private void list_File_DoubleClick(object sender, EventArgs e) // ListView 아이템을 더블클릭했을때, 파일은 다운로드 폴더는 해당 경로로 이동한다.
        {
            char type = (selectedItem.SubItems[3].Text.ToCharArray())[0]; // 아이템이 파일인지 폴더인지 type에 저장
            string name = selectedItem.SubItems[0].Text;

            if (type == 'd') // 폴더를 더블클릭한 경우
            {
                int index = 0; // 현재 경로의 몇번째 폴더인지를 저장

                // 지금 선택한 이름의 하위 노드가 몇번째인지 검색
                foreach (TreeNode n in selectedNode.Nodes) // 선택된 노드의 하위노드수 = 현재 경로의 폴더수 이므로 하위노드만큼 반복
                {                                          // * (! selecteItem.index를 사용하지 않는 이유 !)
                    if (n.Text == name)                    // * 기본적으로 경로가 이동되면 ListView의 아이템들은 이름순 폴더 - 이름순 파일로 정렬된다.
                        break;                             // * 경로가 이동될 때 마다 ListView는 서버에서 정보를 받아서 갱신하지만 TreeView는 처음받은 노드에서 갱신되지않는다.
                    index++;                               // * 새 폴더를 만들거나 이름을 바꾼경우 클라이언트에서 노드를 추가하고 이름을 바꾸어주지만 이름순으로 재정렬되는 ListView와달리
                }                                          // * TreeView에서는 이름순 재정렬이 되지않기때문에 같은 이름의 폴더의 인덱스가 서로 다를 수 있으므로 이름으로 따로 검색해주는것이다.

                selectedNode = selectedNode.Nodes[index];  // 선택된 경로에 맞는 노드를 선택해주고 펼치는 부분
                selectedPath = selectedNode.FullPath;
                tree_Directory.SelectedNode = selectedNode;
                selectedNode.Expand();
                lbl_SelectPath.Text = "업로드 경로 : " + selectedNode.Text;

                RefreshList(selectedPath); // ListView 새로고침
            }
            else if (type == 'f') // 파일을 더블클릭한 경우
            {
                if (MessageBox.Show("선택한 항목을 내려받으시겠습니까??", "알림", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    DownLoad(); // 다운로드 호출
                }
                else
                    return;
            }
        }

        private void btn_Refresh_Click(object sender, EventArgs e) // 새로고침 버튼
        {
            RefreshList(selectedPath);
        }

        private void btn_Back_Click(object sender, EventArgs e) // 상위폴더로 간다.
        {
            if (selectedPath != userID) // userID(접속한 ID)는 최상위 폴더명이므로 상위폴더가 없다. 
            {
                string path = selectedPath.Substring(0, selectedPath.LastIndexOf("\\"));

                selectedNode = selectedNode.Parent;
                selectedPath = selectedNode.FullPath;
                tree_Directory.SelectedNode = selectedNode;
                selectedNode.Expand();

                lbl_SelectPath.Text = "업로드 경로 : " + selectedNode.Text;
                tree_Directory.Focus(); // 포커스 되면서 선택된 노드가 새로 눌린것으로 취급된다. -> tree_DirectoryClick 메소드가 호출됨
            }
        }

        private void btn_Down_Click(object sender, EventArgs e) // 내려받기 버튼을 누른경우
        {
            if (selectedItem == null || selectedItem.SubItems[3].Text == "d")
            {
                MessageBox.Show("다운로드할 항목을 선택해주세요.", "알림");
                return;
            }

            DownLoad(); // 다운로드 해주는 메소드 호출

        }

        private void list_File_Click(object sender, EventArgs e) // ListView에서 아이템(파일/폴더)를 누를때마다 선택 아이템변경을 한다.
        {
            ListView listView = (ListView)sender;
            selectedItem = listView.FocusedItem;
            lbl_State.Text = selectedItem.SubItems[0].Text;

            btn_Delete.Enabled = true;
            btn_Rename.Enabled = true;
        }

        private void btn_NewDir_Click(object sender, EventArgs e) // 새 폴더를 만든다.
        {
            BinaryFormatter serializer = new BinaryFormatter();
            input = "";
            
            InputForm inForm = new InputForm();// 입력을 받을 폼                    
            inForm.label1.Text = "새 폴더 이름을 입력해주세요.";
            inForm.ShowDialog(this); // 모달창으로 부른다.

            if (input == "") // 입력한게 없으면 리턴
                return;

            TcpClient client = fileConnect();
            NetworkStream stream = client.GetStream();
            ReqNewDirPack reqPack = new ReqNewDirPack // 해당 경로에 새 폴더 만드는 요청 전송
            {
                Path= selectedPath + "\\" + input
            };
            serializer.Serialize(stream, reqPack);

            Pack resPack = (Pack)serializer.Deserialize(stream);
            if (resPack.PackType == CONSTANTS.TYPE_ERROR)
            {
                MessageBox.Show(CONSTANTS.Err_String[resPack.Flag], "알림");
                return;
            }

            selectedNode.Nodes.Add(input); // 좌측 TreeView에 하위 노드추가
            lbl_State.Text = string.Format("새 폴더 : {0}", input);
            stream.Close(); client.Close();
            RefreshList(selectedPath); // ListView 새로고침
        }

        private void btn_Delete_Click(object sender, EventArgs e) // 파일/폴더를 삭제한다.
        {
            BinaryFormatter serializer = new BinaryFormatter();
            if (selectedItem == null) // ListView에서 선택한 아이템이 없다면 리턴
            {
                MessageBox.Show("삭제할 항목을 선택해 주세요.", "알림");
                return;
            }

            if (MessageBox.Show("삭제한 폴더및 파일은 복구가 불가능 합니다.\n정말 삭제 하시겠습니까?", "알림", MessageBoxButtons.YesNo)
                == DialogResult.Yes)
            {
                ReqDeletePack reqPack = new ReqDeletePack
                {
                    Path = selectedPath + "\\" + selectedItem.SubItems[0].Text,
                    FileType= selectedItem.SubItems[3].Text.ToCharArray()[0]
                };
                TcpClient client = fileConnect();
                NetworkStream stream = client.GetStream();
                serializer.Serialize(stream, reqPack);

                Pack resPack = (Pack)serializer.Deserialize(stream);
                if (resPack.PackType == CONSTANTS.TYPE_ERROR)
                {
                    MessageBox.Show(CONSTANTS.Err_String[resPack.Flag], "알림");
                    return;
                }

                lbl_State.Text = string.Format("삭제 성공 : {0}", reqPack.Path);
                stream.Close(); client.Close();
                RefreshList(selectedPath);
            }
            else
            {
                return;
            }
        }

        private void btn_Rename_Click(object sender, EventArgs e) // 파일/폴더의 이름을 바꾼다.
        {
            BinaryFormatter serializer = new BinaryFormatter();
            if (selectedItem == null) // 선택 아이템 없으면 리턴
            {
                MessageBox.Show("삭제할 항목을 선택해 주세요.", "알림");
                return;
            }

            InputForm inForm = new InputForm();
            input = "";
            inForm.label1.Text = "바꿀 이름을 선택해 주세요(확장자명 제외)";
            inForm.ShowDialog(this);

            if (input == "") // 아무것도 입력안하면 리턴
                return;

            string prevName = selectedItem.SubItems[0].Text; // 원래 이름

            TcpClient client = fileConnect();
            NetworkStream stream = client.GetStream();
            ReqReNamePack reqPack = new ReqReNamePack
            { 
                PrevName = selectedPath + "\\" + prevName,
                ReName = selectedPath + "\\" + input,
                FileType = selectedItem.SubItems[3].Text.ToCharArray()[0]
            };
            serializer.Serialize(stream, reqPack);  // 요청 전송

            Pack resPack = (Pack)serializer.Deserialize(stream);
            if (resPack.PackType == CONSTANTS.TYPE_ERROR)
            {
                MessageBox.Show(CONSTANTS.Err_String[resPack.Flag], "알림");
                return;
            }

            if (selectedItem.SubItems[3].Text == "f") // 바꾼것이 파일일경우 아이템의 Name 값을 바꾼다. 아래는 확장자명 떼서 붙이는 문자열 추출
                selectedItem.SubItems[0].Text = input + prevName.Substring(prevName.LastIndexOf('.'), prevName.Length - prevName.LastIndexOf('.'));
            else
            {
                selectedItem.SubItems[0].Text = input;
                selectedNode.Nodes[selectedItem.Index].Text = input;
            }

            list_File.Focus(); // 바뀐 아이템이 선택되도록 한다.
            stream.Close();
            client.Close();
        }

        private async void btn_Up_Click(object sender, EventArgs e) // 올리기 버튼을 누른경우 업로드를 해준다.
        {
            BinaryFormatter serializer = new BinaryFormatter();
            Pack flagPack = new Pack();
            FileInfo fileInfo;
            string fileName = "";
            string filePath = "";

            if (!runUp) // runUp = true 이면 이미 업로드가 진행중이라는 뜻이다.
            {
                if (selectedPath == "") // 선택한 디렉토리가 없으면 리턴 //
                {
                    MessageBox.Show("업로드할 폴더를 선택해주세요", "알림");
                    return;
                }

                if (upDialog.ShowDialog() == DialogResult.OK) // OpenFileDialog를 띄우고 //
                {
                    runUp = true;
                    btn_Up.Text = "취소"; // '올리기'버튼을 '취소'로 만든다.

                    int bsIndex = upDialog.FileName.LastIndexOf("\\"); // 선택한 파일 정보를 가져온다 //
                    fileName = upDialog.FileName.Substring(bsIndex + 1, upDialog.FileName.Length - bsIndex - 1);
                    filePath = selectedPath + "\\" + fileName;
                    fileInfo = new FileInfo(upDialog.FileName);
                }
                else
                    return;

                ReqUpLoadPack reqPack = new ReqUpLoadPack // 요청을 보낸다 //
                {
                    Path = filePath,
                    FileSize = (ulong)fileInfo.Length
                };
                TcpClient client = fileConnect();
                NetworkStream stream = client.GetStream();
                serializer.Serialize(stream, reqPack); // <!> 요청을 보낸다 //

                flagPack = (Pack)serializer.Deserialize(stream); // 서버에서 보내지말라하면 메소드 나감 //
                if (flagPack.Flag == CONSTANTS.FLAG_NO)
                {
                    return;
                }

                await Task.Run(() => // 비동기로 처리하는 이유 : 업로드 도중 다른작업을 가능하게 해준다. (비동기로 하지않으면 이 메소드가 반환되기전까지 창이 굳는다.)
               {
                   progressBar1.Value = 0;

                   using (Stream fileStream = new FileStream(upDialog.FileName, FileMode.Open))
                   {
                       byte[] buffer = new byte[1024 * 1024]; // 읽어서 보낼 데이터 저장 배열
                       long readSize = 0;  // 읽어서(보낸) 크기 저장

                       while (true)
                       {
                           int read = fileStream.Read(buffer, 0, (1024 * 1024));
                           readSize += read; // 읽은 크기늘린다.
                           SendDataPack resPack = new SendDataPack()
                           {
                               Data = new byte[read],
                               Last = CONSTANTS.NOT_LAST
                           };
                           // 퍼센트바, 진행률 레이블 갱신
                           double per = readSize / (double)fileStream.Length * 100;
                           progressBar1.Value = (int)per;
                           lbl_Size.Text = string.Format("{0} /\t{1}", readSize, fileStream.Length);
                           lbl_Percent.Text = string.Format("{0:F2} %", per);
                           Update();

                           Array.Copy(buffer, 0, resPack.Data, 0, read); // 읽은 배열 보낼 Pack에 복사
                           if (readSize >= fileStream.Length || !runUp)  // 원본 파일 크기만큼 읽었다면 마지막 Pack을 보내기위해 표시한다.
                               resPack.Last = CONSTANTS.LAST;            // ㄴ 또는 '취소' 버튼을 눌러서 중단해도 마지막 표시를한다. ( 취소하면 runUp이 false가 된다.)
                           serializer.Serialize(stream, resPack);

                           if (resPack.Last == CONSTANTS.LAST) // 보낸 Pack이 마지막이면 반복문 종료
                               break;
                       }
                   }

                   if (runUp) // 이건 정상적으로 업로드했을 경우다.
                   {
                       lbl_State.Text = "올리기 완료 : " + fileName;
                       runUp = false;
                   }
                   else if (!runUp) // 이건 취소버튼을 눌렀을 경우이다.
                   {
                       MessageBox.Show("작업취소", "알림");
                       lbl_State.Text = "올리기가 취소 되었습니다.";
                   }

                   btn_Up.Text = "올리기";         // 다시 업로드 버튼으로 만든다.
                   stream.Close(); client.Close();
               });
            }
            else if (runUp)
            {
                runUp = false; // 이러면 다음반복에서 멈춘다.
            }
        }

        private void DownLoad() // 다운로드 메소드
        {
            TcpClient client = new TcpClient();

            try
            {
                client = fileConnect();
                DownLoadWindow downForm = new DownLoadWindow(); // 다운로드 진행상황을 볼 수 있는 창을 만든다. 여러개 띄울 수 있다.
                dwList.Add(downForm);
                downForm.Show(this); // 모달창이 아니다. 여러개 실행 가능하다.
                downForm.DownLoad(selectedItem, txt_DownLoadPath.Text, selectedPath, client);
                downPath = txt_DownLoadPath.Text;
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show("내려받을 경로가 올바르지 않습니다.", "알림");
                return;
            }
        }

        private void btn_SetDownPath_Click(object sender, EventArgs e) // 다운로드 버튼을 누른경우 다운로드를 해준다.
        {
            folderBrowserDialog1.ShowDialog();
            string path = folderBrowserDialog1.SelectedPath;
            txt_DownLoadPath.Text = path;
        }

        public async void Login(string id, string pw) // 로그인 
        {
            BinaryFormatter serializer = new BinaryFormatter();
            if (id == "")
            {
                loginForm.lbl_notice.Text = "아이디를 입력해주세요.";
                return;
            }
            else if (pw == "")
            {
                loginForm.lbl_notice.Text = "비밀번호를 입력해주세요.";
                return;
            }

            lbl_Size.Text = "0";

            // 로그인 서버와 연결
            TcpClient logClient;
            NetworkStream logStream;
            Pack flagPack = new Pack();
            Pack reqMait = new Pack();
            Pack resMait = new Pack();
            logClient = new TcpClient(clientAdress);
            logClient.Connect(loginAdress);
            logStream = logClient.GetStream();

            ReqLoginPack reqLoginPack = new ReqLoginPack() // 보낼 요청 인스턴스
            {
                Id = id,
                Pw = pw
            };
            serializer.Serialize(logStream, reqLoginPack);

            Pack resPack = (Pack)serializer.Deserialize(logStream);
            if (resPack.PackType == CONSTANTS.TYPE_ERROR) // 오류판별
            {
                if (resPack.Flag == CONSTANTS.ERROR_CONNECTED_ID)
                {
                    if (MessageBox.Show("이미 접속중인 ID 입니다. \n 기존 연결을 끊고 접속하시겠습니까?", "알림",
                        MessageBoxButtons.YesNo) == DialogResult.Yes) // 중복 로그인인 경우 사용자가 기존 연결끊고 접속할지 여부를 선택한다.
                    {
                        flagPack.Flag = CONSTANTS.FLAG_YES;
                        serializer.Serialize(logStream, flagPack);
                        resPack = (Pack)serializer.Deserialize(logStream);

                        if (resPack.PackType == CONSTANTS.TYPE_ERROR)
                            return;
                    }
                    else
                    {
                        flagPack.Flag = CONSTANTS.FLAG_NO; // 
                        serializer.Serialize(logStream, flagPack);
                        return;
                    }
                }
                else
                {
                    loginForm.lbl_notice.Text = CONSTANTS.Err_String[resPack.Flag]; // 로그인 폼에 오류이유를 알린다.
                    return;
                }
            }
            // 여기까지오면 로그인에 성공했다는 것이다.
            loginForm.Close(); // 로그인폼을 닫고
            // 로그인에 성공하면 초기 노드를 받고 컨트롤들을 활성화한다.
            startControl();
            btn_Login.Enabled = false; // 로그인/회원가입 버튼은 비활성화
            btn_Register.Enabled = false;
            userID = reqLoginPack.Id;
            SetStartNode();
            // 로그인 유지를 하는 반복문을 비동기로 처리하고 메소드를 탈출한다.
            await Task.Run(() =>
            {
                while (true)
                {
                    //serializer.Serialize(logStream, reqMait); // 요청을 주고
                    resMait = (Pack)serializer.Deserialize(logStream); // 받는다.

                    if (resMait.PackType == CONSTANTS.TYPE_ERROR) // 만약 받은 Pack에 에러가있다면 서버에서 연결을 끊이려는 것이다.
                    {
                        runUp = false; // 끊기전에 업로드를 종료한다.
                        foreach (DownLoadWindow dw in dwList) // 열려있는 다운로드창도 모두 닫는다.
                        {
                            dw.Close();
                        }
                        MessageBox.Show(CONSTANTS.Err_String[resMait.Flag], "알림");
                        Application.Exit(); // 프로그램 종료
                    }

                }
            });
        }

        private void btn_Login_Click(object sender, EventArgs e) // 로그인 버튼을 누른경우 로그인창을 띄운다.
        {
            loginForm = new LoginForm();
            loginForm.ShowDialog(this);
        }

        private void startControl() // 로그인이 성공했을때 컨트롤들을 활성화 시킨다.
        {
            groupBox1.Enabled = true;
            groupBox2.Enabled = true;
            groupBox3.Enabled = true;
            tree_Directory.Enabled = true;
            list_File.Enabled = true;
        }

        private void SetStartNode() // 로그인이 성공했을때 TreeView노드에 경로로 보여줄 초기 노드를 받는다.
        {
            BinaryFormatter serializer = new BinaryFormatter();
            TcpClient fileClient = fileConnect();
            NetworkStream fileStream = fileClient.GetStream();
            ReqStartNodePack reqNodePack = new ReqStartNodePack
            {
                Id = userID // 본인이 접속한 id명의 폴더가 자신의 공간이다. 이 폴더의 하위폴더를 노드형태로 받아올거다.
            };
            serializer.Serialize(fileStream, reqNodePack);

            Pack resNodePack = (Pack)serializer.Deserialize(fileStream);
            if (resNodePack.PackType == CONSTANTS.TYPE_ERROR)
            {
                MessageBox.Show(CONSTANTS.Err_String[resNodePack.Flag], "알림");
                return;
            }

            TreeNode node = ((ResStartNode)resNodePack).ROOT_NODE;
            tree_Directory.Nodes.Add(node);
            selectedNode = node;         // 최상위 폴더를 선택한다/
            selectedPath = node.FullPath;
            node.Expand();
            fileStream.Close(); fileClient.Close();

            lbl_SelectPath.Text = "업로드 경로 : " + selectedNode.Text;

            RefreshList(userID); // 파일목록도 새로고침.
        }

        public void ShowRegister() // 회원가입 창을 띄운다.
        {
            regForm = new RegisterForm();
            regForm.ShowDialog(this);
        }

        public void Register(string id, string pw1, string pw2, string email) // 회원가입
        {
            // 요청을 보내기전에 기본적인 사항들을체크한다.
            if (id == "")
            {
                regForm.lbl_state.Text = "아이디를 입력해주세요";
                return;
            }
            else if (pw1 == "")
            {
                regForm.lbl_state.Text = "비밀번호를 입력해주세요";
                return;
            }
            else if (!(pw1.Equals(pw2)))
            {
                regForm.lbl_state.Text = "비밀번호가 일치하지 않습니다.";
                return;
            }
            else if (email == "")
            {
                regForm.lbl_state.Text = "e-mail을 입력해주세요.";
                return;
            }

            BinaryFormatter serializer = new BinaryFormatter();
            TcpClient regClient = new TcpClient(clientAdress);
            regClient.Connect(loginAdress);
            NetworkStream regStream = regClient.GetStream();
            ReqSignPack reqPack = new ReqSignPack
            {
                Id = id,
                Pw = pw1,
                Email = email
            };
            serializer.Serialize(regStream, reqPack);

            Pack flagPack = (Pack)serializer.Deserialize(regStream);
            if (flagPack.PackType == CONSTANTS.TYPE_ERROR) // 에러검사
            {
                regForm.lbl_state.Text = CONSTANTS.Err_String[flagPack.Flag];
                return;
            }
            else if (flagPack.PackType == CONSTANTS.TYPE_BASIC)
            {
                MessageBox.Show("회원가입이 완료 되었습니다.", "알림");
                regForm.Close();
            }

        }

        private void btn_Register_Click(object sender, EventArgs e) // 회원가입 버튼 누르면 회원가입창을 띄운다.
        {
            ShowRegister();
        }
    }

}
