using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Shell;
using System.Drawing.Imaging;

namespace So_hot
{
    public class Video
    {
        private static string[] VideoExtension = {    ".MKV", ".WMV", ".AVI", ".MP4", ".FLV" };
        static int m_Size = 100;
        static List<FileInfo> lstFileVideoInfo = new List<FileInfo>();
        static List<FileInfo> lstFilePreviewInfo = new List<FileInfo>();
        public static string infofilepath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Sohot\\info";
        //Duyệt file để lấy thông tin file trong từng thư mục
        public static List<FileInfo> getFileInfo(string folderPath, string type)
        {

            try
            {
                DirectoryInfo dir = new DirectoryInfo(folderPath);
                lstFilePreviewInfo = new List<FileInfo>();
                if (dir.Exists)
                    foreach (FileInfo i in dir.GetFiles())
                    {
                        if (type == "video")
                        {
                            if (VideoExtension.Contains(i.Extension.ToUpperInvariant()))
                            {
                                lstFileVideoInfo.Add(i);
                            }
                        }
                        if (type == "image")
                        {
                            if (i.Extension == ".jpg")
                            {
                                lstFilePreviewInfo.Add(i);
                            }
                        }
                    }
            }
            catch
            {
                MessageBox.Show("Không thể truy cập đường dẫn", "Access is denied", MessageBoxButtons.OK);
            }
            if (type == "video")
                return lstFileVideoInfo;
            else
                return lstFilePreviewInfo;
        }
        protected static string GetBase64StringForImage(string imgPath)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var bitmap = new Bitmap(imgPath))
                    {
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                       return Convert.ToBase64String(ms.GetBuffer()); //Get Base64
                    }
                }
            }
            catch(Exception ex)
            {
                return "";
            }
            //return imageBytes;
        }
        private static void getThumbnail(string video, string thumbnail)
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
        public static void GetVideoThumbnail(string inputVideoFile, string output)
        {
            var video = new MediaFile
            {
                Filename = inputVideoFile
            };
            var image = new MediaFile { Filename = output };
            using (var engine = new Engine())
            {
                engine.GetMetadata(video);

                // Saves the frame located on the 15th second of the video.
                var options = new ConversionOptions { Seek = TimeSpan.FromSeconds(15) };
                engine.GetThumbnail(video, image, options);
            }
        }
        //Tạo thư mục chứa hình xem trước của video.
        public static List<Movies> createPreviewFolder(List<string> lstFolder)
        {
            string folderimg = string.Empty;
            List<Movies> lstBase64 = new List<Movies>();
            lstFileVideoInfo = new List<FileInfo>();
            using (StreamReader sr = new StreamReader(infofilepath))
            {
                folderimg = sr.ReadLine();
                sr.Close();
            }
            foreach(string folder in lstFolder)
            {
                lstFileVideoInfo = getFileInfo(folder,"video");
            }
            foreach(FileInfo file in lstFileVideoInfo)
            {
                if(VideoExtension.Contains(file.Extension.ToUpperInvariant()))
                {
                    var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
                    string[] temp = file.FullName.Split('\\');
                    string fullPathFolderImage = $"{folderimg}\\{file.Name}.jpg";
                    if(!Directory.Exists(folderimg))
                    {
                        Directory.CreateDirectory(folderimg);
                    }
                    if (!File.Exists(fullPathFolderImage))
                    {
                        try
                        {
                            ffMpeg.GetVideoThumbnail(file.FullName, fullPathFolderImage, 10);
                            var img = GetBase64StringForImage(fullPathFolderImage);
                            lstBase64.Add(new Movies
                            {
                                Name = file.Name,
                                FullPath = file.FullName,
                                Image = img
                            });
                        }
                       catch(Exception e)
                        {
                            var x = e.ToString();
                        }
                    }
                    else
                    {
                        var img = GetBase64StringForImage(fullPathFolderImage);
                        lstBase64.Add(new Movies
                        {
                            Name = file.Name,
                            FullPath = file.FullName,
                            Image = img
                        });
                    }
                        
                }            
            }
            return lstBase64;
        }
        //Tạo thư mục chứa hình xem trước của video.
        public static void createPreviewFolder(string folder)
        {
            string folderimg = string.Empty;
            using (StreamReader sr = new StreamReader(infofilepath))
            {
                folderimg = sr.ReadLine();
                sr.Close();
            }
            lstFileVideoInfo = getFileInfo(folder, "video");
            foreach (FileInfo file in lstFileVideoInfo)
            {
                if (VideoExtension.Contains(file.Extension.ToUpperInvariant()))
                {
                    var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
                    string[] temp = file.FullName.Split('\\');
                    string drive = temp[0].Substring(0, 1);
                    ffMpeg.GetVideoThumbnail(file.FullName, $"{folderimg}\\{file.Name}.jpg", 10);
                }
            }
        }
        //Xây dựng Imagelist.
        public static ImageList AddThumbnailList(List<FileInfo> listfile)
        {
            ImageList imglist = new ImageList();
            imglist.ImageSize = new Size(m_Size, m_Size);
            imglist.ColorDepth = ColorDepth.Depth24Bit;
            try
            {
                for (int i = 0; i < listfile.Count; i++)
                {
                    using (Image x = Image.FromFile(listfile[i].FullName.ToString()))
                    {
                        Image thumbnail = x.GetThumbnailImage(100, 100, null, new IntPtr());
                        imglist.Images.Add(listfile[i].Name, thumbnail);

                    }
                }
            }
            catch (OutOfMemoryException ome)
            {
                MessageBox.Show(ome.Message, "Lỗi", MessageBoxButtons.OK);
            }
            return imglist;
        }

    }
}
