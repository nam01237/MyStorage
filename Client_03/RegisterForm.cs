using System;
using System.Windows.Forms;

namespace Client_03
{
    public partial class RegisterForm : Form
    {
        public RegisterForm()
        {
            InitializeComponent();
        }

        private void btn_register_Click(object sender, EventArgs e)
        {
            ((MainForm)(this.Owner)).Register(txt_ID.Text, txt_PW.Text, txt_checkPW.Text, txt_email.Text);
        }
    }
}
