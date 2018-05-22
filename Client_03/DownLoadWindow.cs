using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using WHP;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Client_03
{
    public partial class DownLoadWindow : Form
    {
        public DownLoadWindow()
        {
            InitializeComponent();
        }

        private bool done = false;  // 다운로드가 완료 되었는지를 체크한다.
        private bool go = true;     // 다운로드를 계속 진행할지 여부를 체크한다.

        public async void DownLoad(ListViewItem selectedItem, string downPath, string selectedPath, TcpClient client)
        {

            BinaryFormatter serializer = new BinaryFormatter();
            NetworkStream stream = client.GetStream();

            string selectFile = selectedItem.SubItems[0].Text;              // 다운로드 받을 파일이름
            ulong totalSize = ulong.Parse(selectedItem.SubItems[4].Text);   // 다운로드 받을 파일 크기
            string filePath = selectedPath + "\\" + selectFile;             // 현재 선택된 경로를 포함한 전체경로
            ReqDownLoadPack reqPack = new ReqDownLoadPack                   // 요청보낼 Pack 인스턴스
            {
                PackType = CONSTANTS.TYPE_REQ_DOWNLOAD,
                Path = filePath,
            };
            // 운로드 요청 보냈다 //
            serializer.Serialize(stream, reqPack);

            // 파일 받을 준비 //

            ulong recvSize = 0;                         // 현재까지 다운로드 받은 크기
            SendDataPack resPack = new SendDataPack();
            Pack flagPack = new Pack // 서버에 다운로드를 계속할지 여부를 알려주는 요청 Pack이다. FALG를 CONSTANTS.FLAG_NO 보내면 다운로드를 중단할것이다.
            { 
                PackType = CONSTANTS.TYPE_BASIC,
                Flag = CONSTANTS.FLAG_YES
            };
            serializer.Serialize(stream, flagPack); // 시작신호를 보낸다.

            await Task.Run(() =>
            {

                try
                {
                    using (FileStream fileStream = new FileStream(downPath + "\\" + selectFile, FileMode.Create)) // 다운로드 경로에 파일이름으로 새파일 생성
                    {
                        while ((resPack = (SendDataPack)serializer.Deserialize(stream)) != null) // 파일서버에서 보내는 Pack을 계속받는다.
                        {
                            fileStream.Write(resPack.Data, 0, resPack.Data.Length);
                            recvSize += (ulong)resPack.Data.Length;

                            double per = recvSize / (double)totalSize * 100;

                            progressBar1.Value = (int)per;
                            lbl_FileName.Text = string.Format("다운로드 파일 : {0}", selectFile);
                            lbl_State.Text = string.Format("{0} / \t {1}", recvSize, totalSize);
                            lbl_Percent.Text = string.Format("{0:F2} %", per);

                            if (resPack.Last == CONSTANTS.LAST) // 보낸 Pack이 마지막이면 반복문을 나간다.
                                break;
                            else if (!go) // 이 경우는 사용자가 취소버튼을 누른경우이다.
                            {
                                flagPack.Flag = CONSTANTS.FLAG_NO; // 서버에 다운로드 중지요청을 보낸다.
                                serializer.Serialize(stream, flagPack);
                                break;
                            }

                            serializer.Serialize(stream, flagPack); // 신호를 보낸다.
                        }
                    }

                    if (go) // 반복문을 빠져나오고 go == true 이면 정상 다운로드 완료
                    {
                        lbl_State.Text = lbl_FileName.Text = string.Format("다운로드 완료 : {0}", selectFile);
                        btn_Cancle.Text = "확인";
                        done = true;
                    }
                    else // 취소를했다. 바로 종료한다.
                    {
                        this.Close();
                    }
                }
                catch (IOException e)
                {
                    if (e.HResult == -2147024864)
                        MessageBox.Show("지정된 경로를 엑세스할 수 없습니다.", "오류");

                    done = true;
                    btn_Cancle.Text = "닫기";
                }
                finally
                {
                    stream.Close();
                    client.Close();
                }


            });
        }

        private void btn_Cancle_Click(object sender, EventArgs e)
        {
            if (done)           // 다운로드 완료한 상태면 닫기버튼이다.
                this.Close();
            else                // 그게 아니라면 취소 요청이다. go == false이면 다음 반복에서 취소 요청을 보낸다.
                this.go = false;
        }

    }
}
