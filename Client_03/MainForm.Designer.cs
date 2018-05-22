namespace Client_03
{
    partial class MainForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.list_File = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.txt_DownLoadPath = new System.Windows.Forms.TextBox();
            this.btn_NewDir = new System.Windows.Forms.Button();
            this.lbl_State = new System.Windows.Forms.Label();
            this.btn_Delete = new System.Windows.Forms.Button();
            this.btn_Rename = new System.Windows.Forms.Button();
            this.tree_Directory = new System.Windows.Forms.TreeView();
            this.lbl_SelectPath = new System.Windows.Forms.Label();
            this.btn_Back = new System.Windows.Forms.Button();
            this.btn_Down = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.btn_Up = new System.Windows.Forms.Button();
            this.upDialog = new System.Windows.Forms.OpenFileDialog();
            this.btn_Refresh = new System.Windows.Forms.Button();
            this.btn_SetDownPath = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lbl_Size = new System.Windows.Forms.Label();
            this.lbl_Percent = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btn_Login = new System.Windows.Forms.Button();
            this.btn_Register = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // list_File
            // 
            this.list_File.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.list_File.Enabled = false;
            this.list_File.FullRowSelect = true;
            this.list_File.LargeImageList = this.imageList1;
            this.list_File.Location = new System.Drawing.Point(225, 175);
            this.list_File.MultiSelect = false;
            this.list_File.Name = "list_File";
            this.list_File.Size = new System.Drawing.Size(554, 243);
            this.list_File.SmallImageList = this.imageList1;
            this.list_File.TabIndex = 0;
            this.list_File.UseCompatibleStateImageBehavior = false;
            this.list_File.View = System.Windows.Forms.View.Tile;
            this.list_File.Click += new System.EventHandler(this.list_File_Click);
            this.list_File.DoubleClick += new System.EventHandler(this.list_File_DoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "이름";
            this.columnHeader1.Width = 247;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "수정한 날짜";
            this.columnHeader2.Width = 170;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "크기";
            this.columnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader3.Width = 116;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "file.ico");
            this.imageList1.Images.SetKeyName(1, "forder.ico");
            // 
            // txt_DownLoadPath
            // 
            this.txt_DownLoadPath.Location = new System.Drawing.Point(5, 13);
            this.txt_DownLoadPath.Name = "txt_DownLoadPath";
            this.txt_DownLoadPath.ReadOnly = true;
            this.txt_DownLoadPath.Size = new System.Drawing.Size(365, 21);
            this.txt_DownLoadPath.TabIndex = 1;
            // 
            // btn_NewDir
            // 
            this.btn_NewDir.Location = new System.Drawing.Point(40, 11);
            this.btn_NewDir.Name = "btn_NewDir";
            this.btn_NewDir.Size = new System.Drawing.Size(63, 28);
            this.btn_NewDir.TabIndex = 2;
            this.btn_NewDir.Text = "새 폴더";
            this.btn_NewDir.UseVisualStyleBackColor = true;
            this.btn_NewDir.Click += new System.EventHandler(this.btn_NewDir_Click);
            // 
            // lbl_State
            // 
            this.lbl_State.AutoSize = true;
            this.lbl_State.Location = new System.Drawing.Point(10, 41);
            this.lbl_State.Name = "lbl_State";
            this.lbl_State.Size = new System.Drawing.Size(0, 12);
            this.lbl_State.TabIndex = 3;
            // 
            // btn_Delete
            // 
            this.btn_Delete.Enabled = false;
            this.btn_Delete.Location = new System.Drawing.Point(107, 11);
            this.btn_Delete.Name = "btn_Delete";
            this.btn_Delete.Size = new System.Drawing.Size(63, 28);
            this.btn_Delete.TabIndex = 4;
            this.btn_Delete.Text = "삭제";
            this.btn_Delete.UseVisualStyleBackColor = true;
            this.btn_Delete.Click += new System.EventHandler(this.btn_Delete_Click);
            // 
            // btn_Rename
            // 
            this.btn_Rename.Enabled = false;
            this.btn_Rename.Location = new System.Drawing.Point(174, 11);
            this.btn_Rename.Name = "btn_Rename";
            this.btn_Rename.Size = new System.Drawing.Size(63, 28);
            this.btn_Rename.TabIndex = 5;
            this.btn_Rename.Text = "이름변경";
            this.btn_Rename.UseVisualStyleBackColor = true;
            this.btn_Rename.Click += new System.EventHandler(this.btn_Rename_Click);
            // 
            // tree_Directory
            // 
            this.tree_Directory.Enabled = false;
            this.tree_Directory.Location = new System.Drawing.Point(12, 173);
            this.tree_Directory.Name = "tree_Directory";
            this.tree_Directory.Size = new System.Drawing.Size(198, 245);
            this.tree_Directory.TabIndex = 6;
            this.tree_Directory.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tree_Directory_AfterSelect);
            // 
            // lbl_SelectPath
            // 
            this.lbl_SelectPath.AutoSize = true;
            this.lbl_SelectPath.Location = new System.Drawing.Point(15, 152);
            this.lbl_SelectPath.Name = "lbl_SelectPath";
            this.lbl_SelectPath.Size = new System.Drawing.Size(0, 12);
            this.lbl_SelectPath.TabIndex = 7;
            // 
            // btn_Back
            // 
            this.btn_Back.Location = new System.Drawing.Point(5, 11);
            this.btn_Back.Name = "btn_Back";
            this.btn_Back.Size = new System.Drawing.Size(30, 28);
            this.btn_Back.TabIndex = 8;
            this.btn_Back.Text = "↑";
            this.btn_Back.UseVisualStyleBackColor = true;
            this.btn_Back.Click += new System.EventHandler(this.btn_Back_Click);
            // 
            // btn_Down
            // 
            this.btn_Down.Location = new System.Drawing.Point(470, 13);
            this.btn_Down.Name = "btn_Down";
            this.btn_Down.Size = new System.Drawing.Size(63, 28);
            this.btn_Down.TabIndex = 9;
            this.btn_Down.Text = "내려받기";
            this.btn_Down.UseVisualStyleBackColor = true;
            this.btn_Down.Click += new System.EventHandler(this.btn_Down_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(7, 16);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(526, 23);
            this.progressBar1.TabIndex = 10;
            // 
            // btn_Up
            // 
            this.btn_Up.Location = new System.Drawing.Point(402, 13);
            this.btn_Up.Name = "btn_Up";
            this.btn_Up.Size = new System.Drawing.Size(63, 28);
            this.btn_Up.TabIndex = 11;
            this.btn_Up.Text = "올리기";
            this.btn_Up.UseVisualStyleBackColor = true;
            this.btn_Up.Click += new System.EventHandler(this.btn_Up_Click);
            // 
            // upDialog
            // 
            this.upDialog.FileName = "openFileDialog1";
            this.upDialog.Filter = "모든 파일|*.*";
            this.upDialog.Title = "업로드 파일 선택";
            // 
            // btn_Refresh
            // 
            this.btn_Refresh.Location = new System.Drawing.Point(242, 11);
            this.btn_Refresh.Name = "btn_Refresh";
            this.btn_Refresh.Size = new System.Drawing.Size(63, 28);
            this.btn_Refresh.TabIndex = 12;
            this.btn_Refresh.Text = "새로고침";
            this.btn_Refresh.UseVisualStyleBackColor = true;
            this.btn_Refresh.Click += new System.EventHandler(this.btn_Refresh_Click);
            // 
            // btn_SetDownPath
            // 
            this.btn_SetDownPath.Location = new System.Drawing.Point(374, 13);
            this.btn_SetDownPath.Name = "btn_SetDownPath";
            this.btn_SetDownPath.Size = new System.Drawing.Size(25, 21);
            this.btn_SetDownPath.TabIndex = 13;
            this.btn_SetDownPath.Text = "...";
            this.btn_SetDownPath.UseVisualStyleBackColor = true;
            this.btn_SetDownPath.Click += new System.EventHandler(this.btn_SetDownPath_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btn_Back);
            this.groupBox1.Controls.Add(this.btn_Rename);
            this.groupBox1.Controls.Add(this.btn_Refresh);
            this.groupBox1.Controls.Add(this.btn_Delete);
            this.groupBox1.Controls.Add(this.btn_NewDir);
            this.groupBox1.Enabled = false;
            this.groupBox1.Location = new System.Drawing.Point(225, 125);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(551, 44);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btn_Down);
            this.groupBox2.Controls.Add(this.btn_Up);
            this.groupBox2.Controls.Add(this.btn_SetDownPath);
            this.groupBox2.Controls.Add(this.txt_DownLoadPath);
            this.groupBox2.Controls.Add(this.lbl_State);
            this.groupBox2.Enabled = false;
            this.groupBox2.Location = new System.Drawing.Point(225, 69);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(551, 61);
            this.groupBox2.TabIndex = 15;
            this.groupBox2.TabStop = false;
            // 
            // lbl_Size
            // 
            this.lbl_Size.AutoSize = true;
            this.lbl_Size.Location = new System.Drawing.Point(10, 45);
            this.lbl_Size.Name = "lbl_Size";
            this.lbl_Size.Size = new System.Drawing.Size(0, 12);
            this.lbl_Size.TabIndex = 14;
            // 
            // lbl_Percent
            // 
            this.lbl_Percent.AutoSize = true;
            this.lbl_Percent.Location = new System.Drawing.Point(468, 45);
            this.lbl_Percent.Name = "lbl_Percent";
            this.lbl_Percent.Size = new System.Drawing.Size(0, 12);
            this.lbl_Percent.TabIndex = 15;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lbl_Percent);
            this.groupBox3.Controls.Add(this.progressBar1);
            this.groupBox3.Controls.Add(this.lbl_Size);
            this.groupBox3.Enabled = false;
            this.groupBox3.Location = new System.Drawing.Point(225, 4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(552, 65);
            this.groupBox3.TabIndex = 16;
            this.groupBox3.TabStop = false;
            // 
            // btn_Login
            // 
            this.btn_Login.Location = new System.Drawing.Point(12, 17);
            this.btn_Login.Name = "btn_Login";
            this.btn_Login.Size = new System.Drawing.Size(77, 26);
            this.btn_Login.TabIndex = 2;
            this.btn_Login.Text = "로그인";
            this.btn_Login.UseVisualStyleBackColor = true;
            this.btn_Login.Click += new System.EventHandler(this.btn_Login_Click);
            // 
            // btn_Register
            // 
            this.btn_Register.Location = new System.Drawing.Point(95, 17);
            this.btn_Register.Name = "btn_Register";
            this.btn_Register.Size = new System.Drawing.Size(77, 26);
            this.btn_Register.TabIndex = 17;
            this.btn_Register.Text = "회원가입";
            this.btn_Register.UseVisualStyleBackColor = true;
            this.btn_Register.Click += new System.EventHandler(this.btn_Register_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(791, 432);
            this.Controls.Add(this.btn_Register);
            this.Controls.Add(this.btn_Login);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lbl_SelectPath);
            this.Controls.Add(this.tree_Directory);
            this.Controls.Add(this.list_File);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "NamHard_03";
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView list_File;
        private System.Windows.Forms.TextBox txt_DownLoadPath;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Button btn_NewDir;
        private System.Windows.Forms.Label lbl_State;
        private System.Windows.Forms.Button btn_Delete;
        private System.Windows.Forms.Button btn_Rename;
        private System.Windows.Forms.TreeView tree_Directory;
        private System.Windows.Forms.Label lbl_SelectPath;
        private System.Windows.Forms.Button btn_Back;
        private System.Windows.Forms.Button btn_Down;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button btn_Up;
        private System.Windows.Forms.OpenFileDialog upDialog;
        private System.Windows.Forms.Button btn_Refresh;
        private System.Windows.Forms.Button btn_SetDownPath;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lbl_Percent;
        private System.Windows.Forms.Label lbl_Size;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Button btn_Login;
        private System.Windows.Forms.Button btn_Register;
    }
}

