using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace So_hot
{
    public class Video
    {
        private static string[] VideoExtension = { ".MKV", ".WMV", ".AVI", ".MP4", ".FLV" };
        static int m_Size = 100;
        public static string infofilepath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Sohot\\info";
        //Duyệt file để lấy thông tin file trong từng thư mục

        private static async Task<List<FileInfo>> getFileInfoList(List<string> folders, FileType type)
        {
            if (folders == null || !folders.Any())
                return null;
            List<FileInfo> lstFile = new List<FileInfo>();
            var tasks = new List<Task<List<FileInfo>>>();
            foreach (var folder in folders)
            {
                var getfilesInFolderTask = getFileInfoListInFolder(folder, type);
                tasks.Add(getfilesInFolderTask);
            }
            await Task.WhenAll(tasks);
            foreach (var task in tasks)
            {
                var lstFileInfo = task.Result;
                if (lstFileInfo != null)
                    lstFile.Concat(lstFileInfo);
            }
            return lstFile;
        }
        public static async Task<List<FileInfo>> getFileInfoListInFolder(string folderPath, FileType type)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                return null;
            List<FileInfo> lstFile = new List<FileInfo>();
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            if (!dir.Exists)
                return null;
            try
            {

                await Task.Run(() =>
                {
                    foreach (FileInfo finfo in dir.GetFiles())
                    {
                        if (type == FileType.Video && VideoExtension.Contains(finfo.Extension.ToUpper()))
                        {
                            lstFile.Add(finfo);

                        }
                        else if (type == FileType.Image && finfo.Extension == ".jpg")
                        {
                            lstFile.Add(finfo);
                        }
                    }
                });

            }
            catch
            {
                MessageBox.Show("Không thể truy cập đường dẫn", "Access is denied", MessageBoxButtons.OK);
            }
            return lstFile;
        }
        protected static async Task<string> GetBase64StringForImage(string imgPath)
        {
            string result = string.Empty;
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var bitmap = new Bitmap(imgPath))
                    {
                        await Task.Run(()=> {
                            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                            result = Convert.ToBase64String(ms.GetBuffer()); //Get Base64
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return "";
            }
            return result;
        }
        private static void getVideoThumbnail_v2(string video, string thumbnail)
        {
            var cmd = "ffmpeg  -itsoffset -1  -i " + '"' + video + '"' + " -vcodec mjpeg -vframes 1 -an -f rawvideo -s 320x240 " + '"' + thumbnail + '"';

            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = "/C " + cmd
            };

            var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();

        }
        //Tạo thư mục chứa hình xem trước của video.
        public static async Task<List<Movies>> createPreviewFolder(List<string> lstFolder)
        {
            List<Movies> lstBase64 = new List<Movies>();
            List<FileInfo> lstFileVideoInfo = new List<FileInfo>();
            lstFileVideoInfo = await getFileInfoList(lstFolder, FileType.Video);
            if (!Directory.Exists(UserManagement.UserSession.ImgFolder))
            {
                Directory.CreateDirectory(UserManagement.UserSession.ImgFolder);
            }
            foreach (FileInfo file in lstFileVideoInfo)
            {
                string fullPathFolderImage = $"{UserManagement.UserSession.ImgFolder}\\{file.Name}.jpg";
                var ffMpeg = new NReco.VideoConverter.FFMpegConverter();

                if (!File.Exists(fullPathFolderImage))
                {
                    ffMpeg.GetVideoThumbnail(file.FullName, fullPathFolderImage, 10);
                }
                try
                {
                    var img = await GetBase64StringForImage(fullPathFolderImage);
                    lstBase64.Add(new Movies
                    {
                        Name = file.Name,
                        FullPath = file.FullName,
                        Image = img
                    });
                }
                catch
                {

                }
            }
            return lstBase64;
        }
    }
}
