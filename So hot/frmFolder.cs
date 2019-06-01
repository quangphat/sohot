using Dapper;
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
    public partial class frmFolder : MetroForm
    {
        IDbConnection db;
        public frmFolder()
        {
            InitializeComponent();
            db = null;
        }

        private void frmFolder_Load(object sender, EventArgs e)
        {
            frmPrimary frmPrimary = Application.OpenForms["frmPrimary"] as frmPrimary;
            if (frmPrimary != null)
                db = frmPrimary.db;
            if(UserManagement.UserSession.Type==false)
                return;
            if(db!=null)
            {
                List<Info> lstInfo=null;
                lstInfo = db.Query<Info>("select * from tblInfo").ToList();
                if (lstInfo == null)
                    lstInfo = new List<Info>();
                BindingList<Info> bi = new BindingList<Info>(lstInfo);
                gridControl1.DataSource = null;
                gridControl1.DataSource = bi;
            }
        }


        private void gridView1_InitNewRow(object sender, DevExpress.XtraGrid.Views.Grid.InitNewRowEventArgs e)
        {
            gridView1.SetRowCellValue(e.RowHandle, gridView1.Columns["Path"], false);
        }

        private void gridView1_RowUpdated_1(object sender, DevExpress.XtraGrid.Views.Base.RowObjectEventArgs e)
        {
            Info info = e.Row as Info;
            if (info != null)
            {
                if (info.Id > 0)
                {
                    string edit = string.Format("update tblInfo set Path = @Path where Id =@Id");
                   db.Execute(edit, info);
                }
                else
                {
                    string insert = string.Format("insert into tblInfo(Path) values (@Path)");
                    db.Execute(insert, info);
                }
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            int i = gridView1.GetSelectedRows().FirstOrDefault();
            Info info = gridView1.GetRow(i) as Info;
            if(info!=null)
            {
                string delete = string.Format("delete from tblInfo where Id = " + info.Id.ToString());
                db.Execute(delete);
                gridView1.DeleteRow(i);
            }
        }
    }
}
