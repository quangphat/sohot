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
using System.Data.SQLite;
using System.IO;
using System.Diagnostics;
using Dapper;
using So_hot.Repository;

namespace So_hot
{
    public partial class frmPrimary : MaterialForm
    {
        public IDbConnection db;
        string dbPath;
        string folderImg;
        List<string> lstFolder = new List<string>();
        LinkRepository rpLink;
        MovieRepository rpMovie;
        InfoRepository rpInfo;
        public frmPrimary()
        {
            InitializeComponent();
            dbPath = string.Empty;
            folderImg = string.Empty;
            db = Init();
            rpLink = new LinkRepository(db);
            rpMovie = new MovieRepository(db);
            rpInfo = new InfoRepository(db);
        }
        private IDbConnection Init()
        {
            string appPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Sohot\\dbinfo";
            using (StreamReader sr = new StreamReader(appPath))
            {
                dbPath = sr.ReadLine();
                UserManagement.UserSession.DbPath = dbPath;
                sr.Close();
            }
            string infofilepath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Sohot\\info";
            using (StreamReader sr = new StreamReader(infofilepath))
            {
                folderImg = sr.ReadLine();
                UserManagement.UserSession.ImgFolder = folderImg;
                sr.Close();
            }
            if (!string.IsNullOrEmpty(dbPath))
            {
                return new SQLiteConnection($"Data Source={dbPath};Version=3;");
            }
            return null;
        }
        private void frmPrimary_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private async void frmPrimary_Load(object sender, EventArgs e)
        {
            List<tblLink> lstLink;
            bool isRealData = UserManagement.UserSession.Type;

            lstLink = await rpLink.GetLinkList(isRealData);
            await LoadData(lstLink);
        }
        public async void delgLoadData()
        {
            List<tblLink> lstLink;
            bool isRealData = UserManagement.UserSession.Type;

            lstLink = await rpLink.GetLinkList(isRealData);
            await LoadData(lstLink);
        }
        private async Task LoadData(List<tblLink> links)
        {
            if (links == null)
                return;
            int stt = 1;
            await Task.Run(() =>
            {
                links.ForEach(p =>
                {
                    p.Stt = stt;
                });
            });

            BindingList<tblLink> bi = new BindingList<tblLink>(links);
            gridControl1.DataSource = null;
            gridControl1.DataSource = bi;
        }

        private async void gridView1_InitNewRow(object sender, DevExpress.XtraGrid.Views.Grid.InitNewRowEventArgs e)
        {
            IList<tblLink> lstsource = gridView1.DataSource as IList<tblLink>;
            await Task.Run(() =>
            {
                gridView1.SetRowCellValue(e.RowHandle, gridView1.Columns["Stt"], lstsource.Count + 1);
            });

        }

        private async void gridView1_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
        {
            await Task.Run(() =>
            {
                gridView1.Appearance.SelectedRow.BackColor = ColorTranslator.FromHtml("#45CAEA");
                gridView1.Appearance.FocusedCell.BackColor = ColorTranslator.FromHtml("#45CAEA");
                gridView1.Appearance.FocusedRow.BackColor = ColorTranslator.FromHtml("#45CAEA");
                gridView1.Appearance.SelectedRow.Options.UseBackColor = true;
                gridView1.Appearance.SelectedRow.ForeColor = Color.White;
                gridView1.Appearance.FocusedRow.ForeColor = Color.White;
                gridView1.Appearance.SelectedRow.Options.UseForeColor = true;
            });

        }

        private async void gridView1_RowUpdated(object sender, DevExpress.XtraGrid.Views.Base.RowObjectEventArgs e)
        {
            tblLink link = e.Row as tblLink;
            if (link == null)
                return;
            link.Type = UserManagement.UserSession.Type;
            if (link.Id > 0)
            {
                await rpLink.Update(link);
            }
            else
            {
                await rpLink.Insert(link);
            }
        }

        private void gridView1_RowCellClick(object sender, DevExpress.XtraGrid.Views.Grid.RowCellClickEventArgs e)
        {
            if (e.Column.FieldName == "Link")
            {
                int i = gridView1.GetSelectedRows().FirstOrDefault();
                tblLink link = gridView1.GetRow(i) as tblLink;
                if (string.IsNullOrEmpty(link.Link))
                    return;
                if (link.Link.Length > 10 && link.Link.Substring(0, 10).Contains("http"))
                    Process.Start(@"CocCoc.exe", "--incognito " + link.Link);
                else
                {
                    Process.Start(@"CocCoc.exe", "--incognito " + "https://www.google.com/search?q=" + Uri.EscapeDataString(link.Link));
                }
            }
        }
        private async Task handleOnSearch()
        {
            TabPage tab = materialTabControl1.SelectedTab;
            if (tab.Text == "Link")
            {
                if (string.IsNullOrEmpty(metroTextBox1.Text))
                    return;
                string search = metroTextBox1.Text.Trim();
                var links = await rpLink.Search(search, UserManagement.UserSession.Type);
                await LoadData(links);
            }
            else
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
        private async Task deleteFolder()
        {
            if (Directory.Exists(folderImg))
            {
                try
                {
                    await Task.Run(()=> {
                        Directory.Delete(folderImg, true);
                    });
                }
                catch (Exception ex)
                {

                }

            }
        }
        private async void btnRemove_Click(object sender, EventArgs e)
        {
            TabPage tab = materialTabControl1.SelectedTab;
            if (tab == null) return;
            if (tab.Text == "Link")
            {
                int i = gridView1.GetSelectedRows().FirstOrDefault();
                tblLink link = gridView1.GetRow(i) as tblLink;
                if (link == null)
                    return;
                await rpLink.DeleteById(link.Id);
                gridView1.DeleteRow(i);
                return;
            }

            if (tab.Text == "Downloaded")
            {
                string fileName = string.Empty;
                ListViewItem item = lstViewContent.SelectedItems[0];
                if (item == null) return;
                if (item.Tag == null)
                    item.Tag = "";
                fileName = Path.GetFileName(item.Tag.ToString());
                await rpMovie.DeleteByFullPath(fileName);
                lstViewContent.Items.Remove(item);
                File.Delete(fileName);
            }

        }

        private async void btnLoad_Click(object sender, EventArgs e)
        {
            TabPage tab = materialTabControl1.SelectedTab;
            if (tab.Text == "Link")
            {
                var links = await rpLink.GetLinkList(UserManagement.UserSession.Type);
                await LoadData(links);
            }
            else
            {
                await LoadImg();
            }
        }
        private async Task<List<string>> getFolderContainMovies()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            if (drives == null) return null;
            List<string> validFolder = new List<string>();
            List<string> folders = await getFolder();
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
            if (UserManagement.UserSession.Type == false) return;
            List<Movies> allMovies = new List<Movies>();
            List<Movies> movies = new List<Movies>();
            IEnumerable<Movies> lstMovie = new List<Movies>();
            try
            {
                lstMovie = await rpMovie.GetAllMovie();
                allMovies = lstMovie != null ? lstMovie.ToList() : new List<Movies>();
            }
            catch (Exception ex)
            {

            }
            ImageList imgList, imgListAll;
            List<string> validFolders = await getFolderContainMovies();
            if (validFolders == null) return;
            if (cbResetData.Checked)
            {
                List<Movies> lstBase64 = await Video.createPreviewFolder(validFolders);
                if (lstBase64 != null && lstBase64.Any())
                {
                    
                    try
                    {
                        var insertTasks = new List<Task>();
                        foreach (Movies mv in lstBase64)
                        {
                            var exist = allMovies.Where(p => p.FullPath == mv.FullPath).FirstOrDefault();
                            if (exist == null)
                                insertTasks.Add(rpMovie.Insert(mv));
                        }
                        await Task.WhenAll(insertTasks);
                    }
                    catch
                    {
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
                    lstMovie = await rpMovie.GetAllMovie();
                    allMovies = lstMovie != null ? lstMovie.ToList() : new List<Movies>();
                   
                }
                movies = allMovies;
            }
            else
            {
                if (allMovies != null && allMovies.Any())
                {
                    moviesBySearch = allMovies.Where(p => p.Name.Contains(query)).ToList();
                }
                else
                {
                    lstMovie = await rpMovie.Search(query);
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
                        var img = await Base64ToImage(mv.Image);
                        imgList.ImageSize = new Size(90, 90);
                        imgList.Images.Add(img);

                    }
                    catch (Exception ex)
                    {

                    }

                }
                await addtoListView(imgList, movies);
            }
            cbResetData.Checked = false;
            await deleteFolder();
        }
        public async Task<Image> Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0,imageBytes.Length);

            // Convert byte[] to Image
            await ms.WriteAsync(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms);
            return image;
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
        // thêm ảnh từ imagelist vào listview.
        private async Task addtoListView(ImageList imglist, List<Movies> movies)
        {
            lstViewContent.Items.Clear();
            lstViewContent.View = View.LargeIcon;
            lstViewContent.LargeImageList = imglist;
            await Task.Run(()=> {
                for (int i=0;i<movies.Count;i++)
                {
                    var mv = movies[i];
                    ListViewItem item = new ListViewItem();
                    item.ImageIndex = i;
                    item.Text = mv.Name;
                    item.Tag = mv.FullPath;
                    lstViewContent.Items.Add(item);
                }
            });
            
            label1.Text = movies.Count.ToString() + " phim";
        }
        private async Task<List<String>> getFolder()
        {
            lstFolder = new List<string>();
            if (UserManagement.UserSession.Type == false)
                return null;

            List<Info> lstinfo = await rpInfo.GetInfoList();
            if (lstinfo == null)
                return lstFolder;
            lstFolder = lstinfo.Select(p => p.Path).ToList();
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
