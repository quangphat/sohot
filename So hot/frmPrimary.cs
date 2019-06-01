using ModernUI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dapper;
using System.Data.SQLite;
using System.IO;
using System.Diagnostics;

namespace So_hot
{
    public partial class frmPrimary : MaterialForm
    {
        public IDbConnection db;
        string select;
        List<string> lstFolder = new List<string>();
        public frmPrimary()
        {
            InitializeComponent();
            select = string.Empty;
            db = null;
        }

        private void frmPrimary_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void frmPrimary_Load(object sender, EventArgs e)
        {
            string appPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Sohot\\dbinfo";
            string dbpath = string.Empty;
            using (StreamReader sr = new StreamReader(appPath))
            {
                dbpath = sr.ReadLine();
                sr.Close();
            }
            if (!string.IsNullOrEmpty(dbpath))
            {
                db = new SQLiteConnection($"Data Source={dbpath};Version=3;");
                if (UserManagement.UserSession.Type == false)
                    select = string.Format("select * from tblLink where Type =0");
                else
                    if (UserManagement.UserSession.Type == true)
                    select = string.Format("select * from tblLink where (Type is null or Type = 1)");
                LoadData(select);
            }
        }
        public void delgLoadData()
        {
            LoadData(select);
        }
        private void LoadData(string select)
        {
            if (db == null) return;
            List<tblLink> lstLink;
            int type = UserManagement.UserSession.Type == false ? 0 : 1;

            lstLink = db.Query<tblLink>(select).ToList();
            if (lstLink == null)
                lstLink = new List<tblLink>();
            int stt = 1;
            foreach (tblLink link in lstLink)
            {
                link.Stt = stt;
                stt++;
            }
            BindingList<tblLink> bi = new BindingList<tblLink>(lstLink);
            gridControl1.DataSource = null;
            gridControl1.DataSource = bi;
        }

        private void gridView1_InitNewRow(object sender, DevExpress.XtraGrid.Views.Grid.InitNewRowEventArgs e)
        {
            IList<tblLink> lstsource = gridView1.DataSource as IList<tblLink>;
            gridView1.SetRowCellValue(e.RowHandle, gridView1.Columns["Stt"], lstsource.Count + 1);
        }

        private void gridView1_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
        {
            gridView1.Appearance.SelectedRow.BackColor = ColorTranslator.FromHtml("#45CAEA");
            gridView1.Appearance.FocusedCell.BackColor = ColorTranslator.FromHtml("#45CAEA");
            gridView1.Appearance.FocusedRow.BackColor = ColorTranslator.FromHtml("#45CAEA");
            gridView1.Appearance.SelectedRow.Options.UseBackColor = true;
            gridView1.Appearance.SelectedRow.ForeColor = Color.White;
            gridView1.Appearance.FocusedRow.ForeColor = Color.White;
            gridView1.Appearance.SelectedRow.Options.UseForeColor = true;
        }

        private void gridView1_RowUpdated(object sender, DevExpress.XtraGrid.Views.Base.RowObjectEventArgs e)
        {
            tblLink link = e.Row as tblLink;
            if (link != null)
            {
                if (link.Id > 0)
                {
                    link.Type = UserManagement.UserSession.Type;
                    string update = string.Format("update tblLink set Link =@Link,LinkDown=@LinkDown,Dienvien=@Dienvien,Note=@Note,Status=@Status,Type=@Type where Id =@Id");
                    db.Execute(update, link);
                }
                if (link.Id <= 0)
                {
                    link.Type = UserManagement.UserSession.Type;
                    string insert = string.Format("insert into tblLink(Link,LinkDown,Dienvien,Note,Status,Type) values(@Link,@LinkDown,@Dienvien,@Note,@Status,@Type)");
                    db.Execute(insert, link);
                }
            }
        }

        private void gridView1_RowCellClick(object sender, DevExpress.XtraGrid.Views.Grid.RowCellClickEventArgs e)
        {
            if (e.Column.FieldName == "Link")
            {
                int i = gridView1.GetSelectedRows().FirstOrDefault();
                tblLink link = gridView1.GetRow(i) as tblLink;
                if (string.IsNullOrEmpty(link.Link)) return;
                if (link.Link.Length > 10 && link.Link.Substring(0, 10).Contains("http"))
                    Process.Start(@"chrome.exe", "--incognito " + link.Link);
                else
                {

                    Process.Start(@"chrome.exe", "--incognito " + "https://www.google.com/search?q=" + Uri.EscapeDataString(link.Link));
                }
            }
        }
        private async Task handleOnSearch()
        {
            TabPage tab = materialTabControl1.SelectedTab;
            if (tab.Text == "Link")
            {
                string search = metroTextBox1.Text;
                select = string.Format("select * from tblLink where (Link like '%{0}%' or LinkDown like '%{1}%' or Dienvien like '%{2}%' or Note like '%{3}%')", search, search, search, search);
                if (UserManagement.UserSession.Type == false)
                    select = string.Format(select + " and Type =0");
                else
                    if (UserManagement.UserSession.Type == true)
                    select = string.Format(select + " and (Type is null or Type = 1)");
                LoadData(select);
            }
            else if (tab.Text == "Downloaded")
            {
                await LoadImg();
            }
        }
        private async void metroTextBox1_ButtonClick(object sender, EventArgs e)
        {
            await handleOnSearch();
        }

        private async void metroTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.ToString() == "Return")
            {
                await handleOnSearch();
            }
        }
        private void deleteFolder()
        {
            string infofilepath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Sohot\\info";
            string folderimg;
            using (StreamReader sr = new StreamReader(infofilepath))
            {
                folderimg = sr.ReadLine();
                sr.Close();
            }
            if (Directory.Exists(folderimg))
            {
                try
                {
                    Directory.Delete(folderimg, true);
                }
                catch (Exception ex)
                {

                }

            }
        }
        private void btnRemove_Click(object sender, EventArgs e)
        {
            TabPage tab = materialTabControl1.SelectedTab;
            if (tab.Text == "Link")
            {
                int i = gridView1.GetSelectedRows().FirstOrDefault();
                tblLink link = gridView1.GetRow(i) as tblLink;
                if (link != null)
                {
                    db.Execute("delete from tblLink where Id =" + link.Id);
                }
                gridView1.DeleteRow(i);
            }
            else
            {
                if (tab.Text == "Downloaded")
                {
                    string fileName = "";
                    ListViewItem item = lstViewContent.SelectedItems[0];
                    if (item == null) return;
                    if (item.Tag == null)
                        item.Tag = "";
                    lstViewContent.Items.Remove(item);
                    fileName = Path.GetFileName(item.Tag.ToString());
                    string appPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Sohot\\info";
                    string path = string.Empty;
                    
                    using (StreamReader sr = new StreamReader(appPath))
                    {
                        path = sr.ReadLine();
                        sr.Close();
                    }
                    File.Delete(Path.Combine(path, fileName));
                    db.Execute("delete from Movies where FullPath =" + item.Tag.ToString());
                    //LoadImg();
                }
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            TabPage tab = materialTabControl1.SelectedTab;
            if (tab.Text == "Link")
            {
                LoadData(select);
            }
            else
                if (tab.Text == "Downloaded")
            {
                LoadImg();
            }
        }
        private List<string> GetFolderContainMovies()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            if (drives == null) return null;
            List<string> validFolder = new List<string>();
            List<string> folders = getFolder();
            for (int i = 0; i < drives.Length; i++)
            {
                for (int j = 0; j < folders.Count; j++)
                {
                    var fullPath = $"{drives[i]}{folders[j]}";
                    if (Directory.Exists(fullPath))
                    {
                        validFolder.Add(fullPath);
                    }
                }
            }
            return validFolder;
        }
        private Image base64ToImage(string base64)
        {
            byte[] imageBytes = Convert.FromBase64String(base64);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
            return image;
        }
        private async Task LoadImg()
        {
            var select = string.Empty;
            List<Movies> allMovies = new List<Movies>();
            List<Movies> movies = new List<Movies>();
            IEnumerable<Movies> lstMovie = new List<Movies>();
            select = "select * from Movies";
            try
            {
                lstMovie = await db.QueryAsync<Movies>(select);
            }
            catch (Exception ex)
            {

            }

            allMovies = lstMovie != null ? lstMovie.ToList() : new List<Movies>();
            if (UserManagement.UserSession.Type == false) return;
            string folderimg = string.Empty;
            ImageList imgList, imgListAll;
            List<string> validFolders = GetFolderContainMovies();
            if (validFolders == null) return;
            if (cbResetData.Checked)
            {
                List<Movies> lstBase64 = Video.createPreviewFolder(validFolders);
                if (lstBase64 != null && lstBase64.Any())
                {
                    string insert = "";
                    foreach (Movies mv in lstBase64)
                    {
                        try
                        {
                            var exist = allMovies.Where(p => p.FullPath == mv.FullPath).FirstOrDefault();
                            if (exist == null)
                            {
                                insert = $"insert into Movies(Name,FullPath,Image) values('{mv.Name}','{mv.FullPath}','{mv.Image}')";
                                await db.ExecuteAsync(insert);
                            }
                        }

                        catch (Exception ex)
                        {
                            var x = mv.FullPath;
                        }
                    }
                }
            }


            imgList = new ImageList();
            imgListAll = new ImageList();

            List<Movies> moviesBySearch = new List<Movies>();

            var query = metroTextBox1.Text != null ? metroTextBox1.Text.Trim() : string.Empty;
            if (string.IsNullOrWhiteSpace(query))
            {
                if (allMovies == null || !allMovies.Any())
                {
                    lstMovie = await db.QueryAsync<Movies>(select);
                    allMovies = lstMovie != null ? lstMovie.ToList() : new List<Movies>();
                    movies = allMovies;
                }
                else
                {
                    movies = allMovies;
                }
            }
            else
            {
                if (allMovies != null && allMovies.Any())
                {
                    moviesBySearch = allMovies.Where(p => p.Name.Contains(query)).ToList();
                }
                else
                {
                    select = $"select * from Movies where Name like '%{query}%'";
                    lstMovie = await db.QueryAsync<Movies>(select);
                    moviesBySearch = lstMovie != null ? lstMovie.ToList() : new List<Movies>();

                }
                movies = moviesBySearch;
            }

            if (movies != null)
            {
                foreach (var mv in movies)
                {

                    try
                    {
                        var img = Base64ToImage(mv.Image);
                        imgList.ImageSize = new Size(90, 90);
                        imgList.Images.Add(img);

                    }
                    catch (Exception ex)
                    {

                    }

                }
                addtoListView(imgList, movies);
            }
            cbResetData.Checked = false;
            deleteFolder();
        }
        public Image Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0,
              imageBytes.Length);

            // Convert byte[] to Image
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms);
            return image;
        }
        public Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }
        private void btnCreate_Click(object sender, EventArgs e)
        {
            int i = gridView1.GetSelectedRows().FirstOrDefault();
            tblLink link = gridView1.GetRow(i) as tblLink;
            frmEditLink frmedit = new frmEditLink();
            frmedit.Link = link;
            frmedit.Show();
        }

        private void btnCreate_Click_1(object sender, EventArgs e)
        {
            frmEditLink frmcreate = new frmEditLink();
            frmcreate.Show();
        }
        //hàm thêm ảnh từ imagelist vào listview.
        private void addtoListView(ImageList imglist, List<Movies> movies)
        {
            lstViewContent.Items.Clear();
            lstViewContent.View = View.LargeIcon;
            lstViewContent.LargeImageList = imglist;
            int k = 0;
            foreach (Movies mv in movies)
            {
                ListViewItem item = new ListViewItem();
                item.ImageIndex = k;
                item.Text = mv.Name;
                item.Tag = mv.FullPath;
                lstViewContent.Items.Add(item);
                k++;
            }
            label1.Text = movies.Count.ToString() + " phim";
        }
        private void addtoListView(ImageList imglist, List<FileInfo> fileList)
        {
            lstViewContent.Items.Clear();
            lstViewContent.View = View.LargeIcon;
            lstViewContent.LargeImageList = imglist;
            int k = 0;
            foreach (FileInfo fi in fileList)
            {
                ListViewItem item = new ListViewItem();
                item.ImageIndex = k;
                item.Text = fi.Name;
                item.Tag = fi.Name;

                lstViewContent.Items.Add(item);
                k++;
            }
            label1.Text = fileList.Count.ToString() + " phim";
        }
        private List<String> getFolder()
        {
            lstFolder = new List<string>();
            if (UserManagement.UserSession.Type == false) return null;

            List<Info> lstinfo = db.Query<Info>("select * from tblInfo").ToList();
            foreach (Info info in lstinfo)
            {
                lstFolder.Add(info.Path);
            }
            return lstFolder;
        }

        private void btnFolder_Click(object sender, EventArgs e)
        {
            if (UserManagement.UserSession.Type == false) return;
            frmFolder frmFolder = new frmFolder();
            frmFolder.Show();
        }

        private void lstViewContent_DoubleClick(object sender, EventArgs e)
        {

            ProcessStartInfo startInfo = new ProcessStartInfo(getListviewItem());
            Process.Start(startInfo);
        }
        private string getListviewItem()
        {
            string fileName = "";
            ListViewItem item = lstViewContent.SelectedItems[0];
            fileName = item != null && item.Tag != null ? item.Tag.ToString() : string.Empty;

            return fileName;
        }

        private void lstViewContent_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                contextMenuStrip1.Show(Cursor.Position);
            }
        }

        private void openFileLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string location = getListviewItem();
            if (!string.IsNullOrEmpty(location))
            {
                string folder = Path.GetDirectoryName(location);
                Process.Start(folder);
            }
        }

        private void deleteThisMovieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string location = getListviewItem();
            if (!string.IsNullOrEmpty(location))
            {
                DialogResult result = MessageBox.Show("Delete this movie?", "Warning", MessageBoxButtons.YesNo);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    ListViewItem item = lstViewContent.SelectedItems[0];
                    lstViewContent.Items.Remove(item);
                    File.Delete(location);
                }
            }
        }
    }
}
