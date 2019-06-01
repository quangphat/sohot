using Dapper;
using ModernUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace So_hot
{
    public partial class frmEditLink : MetroForm
    {
        IDbConnection db;
        public tblLink Link;
        public frmEditLink()
        {
            InitializeComponent();
            Link = new tblLink();
            db = null;
        }

        private void frmEditLink_Load(object sender, EventArgs e)
        {
            txtLink.Text = Link.Link;
            txtLinkDown.Text = Link.LinkDown;
            txtDienvien.Text = Link.Dienvien;
            cbDaxem.Checked = Link.Status;
            txtNote.Text = Link.Note;
            frmPrimary frmPrimary = Application.OpenForms["frmPrimary"] as frmPrimary;
            if (frmPrimary != null)
                db = frmPrimary.db;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if(Link.Id<=0)
            {
                Link.Link = txtLink.Text;
                Link.LinkDown = txtLinkDown.Text;
                Link.Dienvien = txtDienvien.Text;
                Link.Status = cbDaxem.Checked;
                Link.Note = txtNote.Text;
                Link.Type = UserManagement.UserSession.Type;
                string insert = string.Format("insert into tblLink(Link,LinkDown,Dienvien,Note,Status,Type) values(@Link,@LinkDown,@Dienvien,@Note,@Status,@Type)");
                db.Execute(insert, Link);
                txtLink.Text = txtLinkDown.Text  = string.Empty;
                
                cbDaxem.Checked = false;
            }
            else
            {
                Link.Link = txtLink.Text;
                Link.LinkDown = txtLinkDown.Text;
                Link.Dienvien = txtDienvien.Text;
                Link.Status = cbDaxem.Checked;
                Link.Note = txtNote.Text;
                Link.Type = UserManagement.UserSession.Type;
                string update = string.Format("update tblLink set Link =@Link,LinkDown=@LinkDown,Dienvien=@Dienvien,Note=@Note,Status=@Status,Type=@Type where Id =@Id");
                db.Execute(update, Link);
            }
        }
        private delegate void delgRefresh();
        private void frmEditLink_FormClosing(object sender, FormClosingEventArgs e)
        {
            frmPrimary frmPrimary = Application.OpenForms["frmPrimary"] as frmPrimary;
            if (frmPrimary != null)
            {
                delgRefresh delRefresh = new delgRefresh(frmPrimary.delgLoadData);
                delRefresh();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void bootstrapButton1_Click(object sender, EventArgs e)
        {
            txtLink.Text = txtLinkDown.Text = txtDienvien.Text = string.Empty;
            txtNote.Clear();
        }
    }
}
