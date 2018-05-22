using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WHP
{
    public class CONSTANTS
    {
        //==== Pack Types =====// (Pack의 타입을 정의한다.)
        //Special Type
        public const byte TYPE_BASIC = 0;       // 기본 타입
        public const byte TYPE_ERROR = 99;      // 오류 타입
        //Request Types (요청 타입) //
        public const byte TYPE_REQ_LOGIN = 1;       // 로그인 요청 타입
        public const byte TYPE_REQ_SIGNUP = 2;    // 회원가입 요청 타입
        public const byte TYPE_REQ_DIRECROTY = 3;   // 폴더에 있는 하위파일, 폴더 요청 타입
        public const byte TYPE_REQ_NEWDIR = 4;      // 새 폴더 만들기 요청 타입
        public const byte TYPE_REQ_DELETE = 5;      // 선택 폴더 삭제 요청 타입
        public const byte TYPE_REQ_RENAME = 6;      // 선택 폴더 이름바꾸기 요청 타입
        public const byte TYPE_REQ_DOWNLOAD = 7;    // 선택 파일 다운로드 요청 타입
        public const byte TYPE_REQ_UPLOAD = 8;      // 선택한 경로에 파일 업로드 요청 타입
        public const byte TYPE_REQ_STARTNODE = 9;   // 로그인 성공시 유저 공간 경로를 담은 TreeViewNode 요청 타입
        //Response Types (응답 타임) //
        public const byte TYPE_SEND_DATA = 10;      // 업로드/다운로드시에 이진데이터를 주고받는데 쓰일 타입
        public const byte TYPE_RES_DIRECTORY = 11;  // REQ_DIRECTORY에 대한응답
        public const byte TYPE_RES_STARTNODE = 12;  // REQ_STARTNODE에 대한응답

        //==== Signal Bytes ====// (신호의 타입 모든 Pack에 포합된 FLAG 필드에 쓰인다.)
        public const byte LAST = 0;             // 파일전송시 마지막 데이터인지 여부를 표시한다.
        public const byte NOT_LAST = 1;

        public const byte FLAG_NORMAL = 0;      // 기본상태, FLAG의 값을 사용할 필요가 없을때 기본적으로 할단된다.
        public const byte FLAG_SUCCESS = 1;     // 요청에대한 성공여부를 알려주는데 쓰인다.
        public const byte FLAG_YES = 2;         // 전송을 계속할지 여부를 알리는데 쓰인다.
        public const byte FLAG_NO = 3;          // 

        //Error Define (Pack의 PACK_TYPE == TYPE_ERROR 일때 함께 쓰인다. FLAG필드에 에러 정보를 담는다.) //
        public const byte ERROR_INVALID_ID = 0;         // 유효하지않은 ID
        public const byte ERROR_EXIST_DIR = 1;          // 이미 동일한 이름의 폴더 존재
        public const byte ERROR_EXIST_FILE = 2;         // 이미 동일한 이름의 파일 존재
        public const byte ERROR_INVALID_CHAR = 3;       // windows에서 폴더이름에 사용할 수 없는 문자 사용
        public const byte ERROR_CONNECTED_ID = 4;       // 이미 접속된 ID
        public const byte ERROR_ANOTHER_LOGIN = 5;      // 다른 접속으로인해 접속 정료
        public const byte ERROR_INVALID_PASSWORD = 6;   // 비밀번호가 틀리다.
        public const byte ERROR_DUPLICATE_ID = 7;       // 이미 있는 ID
        public const byte ERROR_DUPLICATE_EMAIL = 8;    // 이미 있는 e-mail
        public const byte ERROR_ID_AGUMENT = 9;         // ID 규칙 위반
        public const byte ERROR_EMAIL_AGUMENT = 10;     // 유효하지않은 e-mail
        public const byte ERROR_UNKNOWN_ERROR = 11;     // 알수없는 오류


        public static readonly List<string> Err_String = new List<string> // 에러메시지를 정리한 리스트 사용자가 FLAG 필드를 인덱스로 메시지를 받는다.
        {
            "유효하지 않은 ID 입니다.",                                  // ERROR_INVALID_ID =   0
            "같은 이름의 폴더가 존재합니다.",                            // ERROR_EXIST_DIR =    1
            "같은 이름의 파일이 존재합니다",                             // ERROR_EXIST_FILE =   2;
            ("파일 및 폴더 이름에 다음 문자를 사용할 수 없습니다.\n" +   // ERROR_INVALID_CHAR =   3;
            " \\ / : * ? < > |"),
            "이미 접속중인 ID 입니다.",                                   // ERROR_CONNECTED_ID = 4;
            "다른 곳에서 로그인해서 접속을 종료합니다.",                  // ERROR_ANOTHER_LOGIN = 5;
            "비밀번호가 틀립니다.",                                       // ERROR_INVALID_PASSWORD = 6;
            "중복된 ID 입니다.",                                          // ERROR_DUPLICATE_ID = 7;
            "중복된 e-mail 입니다.",                                       // ERROR_DUPLICATE_EMAIL = 8;
            "아이디는 영문, 숫자만 입력가능 합니다. (글자수 4~20)",            //  ERROR_ID_AGUMENT = 9;
            "올바른 형식의 E-mail을 입력해 주세요.",                       // ERROR_EMAIL_AGUMENT = 10;
            "예기치 못한 오류로인해 연결종료."                             // ERROR_UNKNOWN_ERROR = 11;
    };
    }

    [Serializable]
    public struct FileInfoStructure // 파일 정보를 담는 구조체
    {
        public string FileName; // 파일이름
        public long FileSize;   // 크기 
        public string AccessDate; // 마지막 수정 날짜
        public char FileType;   // 타입(디렉토리/파일)
    }

    [Serializable]
    public class Pack // 기본 요청 Pack 모든 Pack이 상속받는다.
    {
        public Pack()
        {
            type = CONSTANTS.TYPE_BASIC;
            flag = CONSTANTS.FLAG_NORMAL;
        }
        protected uint type; // Pack의 타입
        protected byte flag;      // 부가적인 신호

        public uint PackType
        {
            get { return type; }
            set { type = value; }
        }

        public byte Flag
        {
            get { return flag; }
            set { flag = value; }
        }
    }

    //==== Pack Class ====//

    // ============= Request Type ================ //
    [Serializable]
    public class ReqLoginPack : Pack // 로그인 요청 타입
    {
        public ReqLoginPack()
        {
            type = CONSTANTS.TYPE_REQ_LOGIN;
        }
        private string id;
        private string pw;

        public string Id
        {
            get { return id;  }
            set { id = value;  }
        }

        public string Pw
        {
            get { return pw; }
            set { pw = value; }
        }
    }

    [Serializable]
    public class ReqSignPack : Pack // 회원가입 요청 타입
    {
        public ReqSignPack()
        {
            type = CONSTANTS.TYPE_REQ_SIGNUP;
        }
        private string id;
        private string pw;
        private string email;

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        public string Pw
        {
            get { return pw; }
            set { pw = value; }
        }

        public string Email
        {
            get { return email; }
            set { email = value; }
        }
    }

    [Serializable]
    public class ReqDirInfoPack : Pack // 폴더에 있는 하위파일, 폴더 요청 타입
    {
        public ReqDirInfoPack()
        {
            type = CONSTANTS.TYPE_REQ_DIRECROTY;
        }
        private string path;

        public string Path
        {
            get { return path;  }
            set { path = value;  }
        }
    }

    [Serializable]
    public class ReqNewDirPack : Pack // 새 폴더 만들기 요청 타입
    {
        public ReqNewDirPack()
        {
            type = CONSTANTS.TYPE_REQ_NEWDIR;
        }
        private string path;

        public string Path
        {
            get { return path; }
            set { path = value; }
        }
    }

    [Serializable]
    public class ReqDeletePack : Pack // 선택 폴더 삭제 요청 타입
    {
        public ReqDeletePack()
        {
            type = CONSTANTS.TYPE_REQ_DELETE;
        }
        private string path;
        private char fileType;

        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        public char FileType
        {
            get { return fileType;  }
            set { fileType = value;  }
        }
    }

    [Serializable]
    public class ReqReNamePack: Pack // 선택 폴더 이름바꾸기 요청 타입
    { 
        public ReqReNamePack()
        {
            type = CONSTANTS.TYPE_REQ_RENAME;
        }
        private string prevName;
        private string reName;
        private char fileType;

        public string PrevName
        {
            get { return prevName; }
            set { prevName = value; }
        }

        public string ReName
        {
            get { return reName; }
            set { reName = value; }
        }

        public char FileType
        {
            get { return fileType; }
            set { fileType = value; }
        }
    }

    [Serializable]
    public class ReqDownLoadPack : Pack // 선택 파일 다운로드 요청 타입
    {
        public ReqDownLoadPack()
        {
            type = CONSTANTS.TYPE_REQ_DOWNLOAD;
        }
        private string path;

        public string Path
        {
            get { return path; }
            set { path = value; }
        }
    }

    [Serializable]
    public class ReqUpLoadPack : Pack // 선택한 경로에 파일 업로드 요청 타입
    {
        public ReqUpLoadPack()
        {
            type = CONSTANTS.TYPE_REQ_UPLOAD;
        }
        private string path;
        private ulong fileSize;

        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        public ulong FileSize
        {
            get { return fileSize; }
            set { fileSize = value; }
        }
    }

    [Serializable]
    public class ReqStartNodePack : Pack // 로그인 성공시 유저 공간 경로를 담은 TreeViewNode 요청 타입
    {
        public ReqStartNodePack()
        {
            type = CONSTANTS.TYPE_REQ_STARTNODE;
        }
        private string id;

        public string Id
        {
            get { return id; }
            set { id = value; }
        }
    }


    // ============= Response Type ================ //
    [Serializable]
    public class SendDataPack : Pack // 업로드/다운로드시에 이진데이터를 주고받는데 쓰일 타입
    {
        public SendDataPack()
        {
            type = CONSTANTS.TYPE_SEND_DATA;
            flag = CONSTANTS.FLAG_NORMAL;
        }

        private byte[] data;
        private byte last;

        public byte[] Data
        {
            get { return data; }
            set { data= value; }
        }

        public byte Last
        {
            get { return last; }
            set { last = value; }
        }
    }

    [Serializable]
    public class ResDirInfoPack : Pack // REQ_DIRECTORY에 대한응답
{
        public  ResDirInfoPack()
        {
            type = CONSTANTS.TYPE_RES_DIRECTORY;
        }

        public List<FileInfoStructure> FilesInfo = new List<FileInfoStructure>();
    }

    [Serializable]
    public class ResStartNode : Pack // REQ_STARTNODE에 대한응답
{
        public ResStartNode()
        {
            type = CONSTANTS.TYPE_RES_STARTNODE;
            flag = CONSTANTS.FLAG_SUCCESS;
        }

        public TreeNode ROOT_NODE;
    }
}


