using System;
using WHP;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace Client_03
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void btn_Login_Click(object sender, EventArgs e)
        {
            if (txt_ID == null)
            {
                lbl_notice.Text = "아이디를 입력해주세요.";
                return;
            }
            else if (txt_PW == null)
            {
                lbl_notice.Text = "패스워드를 입력해주세요.";
                return;
            }

            btn_Login.Enabled = false;
            btn_register.Enabled = false;

            ((MainForm)(this.Owner)).Login(txt_ID.Text, txt_PW.Text);

            btn_register.Enabled = true;
            btn_Login.Enabled = true;
        }

        private void btn_register_Click(object sender, EventArgs e)
        {
            ((MainForm)(this.Owner)).ShowRegister();

        }
    }
}
