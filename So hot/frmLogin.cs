using ModernUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace So_hot
{
    public partial class frmLogin : MetroForm
    {
        public frmLogin()
        {
            InitializeComponent();
            this.Select();
        }

        private void txtPass_TextChanged(object sender, EventArgs e)
        {
            if(txtPass.Text =="798889")
            {
                Users user = new Users();
                user.Type = true;
                UserManagement.CreateSession(user);
                frmPrimary frmprimary = new frmPrimary();
                frmprimary.Show();
                this.Hide();
            }
        }


        private void txtPass_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode.ToString()=="Return")
            {
                if(!string.IsNullOrWhiteSpace(txtPass.Text))
                {
                    Users user = new Users();
                    user.Type = false;
                    UserManagement.CreateSession(user);
                    frmPrimary frmprimary = new frmPrimary();
                    frmprimary.Show();
                    this.Hide();
                }
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtPass.Text))
            {
                Users user = new Users();
                user.Type = false;
                UserManagement.CreateSession(user);
                frmPrimary frmprimary = new frmPrimary();
                frmprimary.Show();
                this.Hide();
            }
        }
    }
}
