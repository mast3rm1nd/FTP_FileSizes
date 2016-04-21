using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO;
using System.Net;
using System.Threading;

namespace FTP_FileSizes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //var test = GetFileExtension("qwdqwdqwd.txt");
            //var test2 = GetFileExtension("wdwdwdww");
        }


        //public static IEnumerable<string> GetFilesInFtpDirectory(string url, string username, string password)
        //{
        //    // Get the object used to communicate with the server.
        //    var request = (FtpWebRequest)WebRequest.Create(url);
        //    request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
        //    request.Credentials = new NetworkCredential(username, password);

        //    using (var response = (FtpWebResponse)request.GetResponse())
        //    {
        //        using (var responseStream = response.GetResponseStream())
        //        {
        //            var reader = new StreamReader(responseStream);
        //            while (!reader.EndOfStream)
        //            {
        //                var line = reader.ReadLine();
        //                if (string.IsNullOrWhiteSpace(line) == false)
        //                {
        //                    yield return line.Split(new[] { ' ', '\t' }).Last();
        //                }
        //            }
        //        }
        //    }
        //}

        string address = "";
        string login = "";
        string password = "";

        Thread DoAllWorkThread;
        private void DoWork_Button_Click(object sender, RoutedEventArgs e)
        {
            DoAllWorkThread = new Thread(new ThreadStart(DoAllWork));
            DoAllWorkThread.IsBackground = true;
            DoAllWorkThread.Start();

            address = FPT_Server_Address_TextBox.Text;
            login = FPT_Server_Login_TextBox.Text;
            password = FPT_Server_Password_PasswordBox.Password;

            //SetUI(false);

            //var address = FPT_Server_Address_TextBox.Text;

            //if (!address.StartsWith("ftp://"))
            //    address = "ftp://" + address;

            //if (!address.EndsWith("/"))
            //    address = address + "/";

            //var login = FPT_Server_Login_TextBox.Text;
            //var password = FPT_Server_Password_PasswordBox.Password;

            ////var allFiles = GetFilesInFtpDirectory(address, login, password);
            //var allFiles = FTPListTree(address, login, password);

            //var filesWithSize = new List<FtpFile>();
            //foreach(var filepath in allFiles)
            //{
            //    var size = GetFtpFileSize(filepath, login, password);

            //    filesWithSize.Add(new FtpFile { Path = filepath, Size = size });
            //}

            //var extensionsWithSize = new List<ExtensionSize>();
            //long totalSize = 0;
            //foreach(var fileInfo in filesWithSize)
            //{
            //    totalSize += fileInfo.Size;

            //    var currFileExtension = GetFileExtension(fileInfo.Path);

            //    var indexOfExtension = extensionsWithSize.FindIndex(x => x.Extension == currFileExtension);

            //    //if (extensionsWithSize.Exists(x => x.Extension == currFileExtension))
            //    if(indexOfExtension == -1) // если не найдена стата по текущему расширению
            //    {
            //        extensionsWithSize.Add(new ExtensionSize { Extension = currFileExtension, Size = fileInfo.Size });
            //    }
            //    else
            //    {
            //        extensionsWithSize[indexOfExtension].Size += fileInfo.Size;
            //    }
            //}

            //extensionsWithSize.OrderByDescending(x => x.Size);

            //var results = String.Format("{0}{1,18}", "Расширение", "Размер [Байт]") + Environment.NewLine;

            //foreach(var extInfo in extensionsWithSize)
            //{
            //    results += String.Format("{0}{1}", extInfo.Extension.PadRight(18, ' '), extInfo.Size) + Environment.NewLine; ;
            //}

            //results += Environment.NewLine + Environment.NewLine+ String.Format("Всего: {0} Байт", totalSize);

            //Results_TextBox.Text = results;

            //SetUI(true);
        }


        public static String[] FTPListTree(String FtpUri, String User, String Pass)
        {

            List<String> files = new List<String>();
            Queue<String> folders = new Queue<String>();
            folders.Enqueue(FtpUri);

            while (folders.Count > 0)
            {
                String fld = folders.Dequeue();
                List<String> newFiles = new List<String>();

                FtpWebRequest ftp = (FtpWebRequest)FtpWebRequest.Create(fld);
                ftp.Credentials = new NetworkCredential(User, Pass);
                ftp.UsePassive = false;
                ftp.Method = WebRequestMethods.Ftp.ListDirectory;
                using (StreamReader resp = new StreamReader(ftp.GetResponse().GetResponseStream()))
                {
                    String line = resp.ReadLine();
                    while (line != null)
                    {
                        var file = line.Trim();

                        //if (file != "." && file != "..")
                            newFiles.Add(file);

                        line = resp.ReadLine();
                    }
                }

                ftp = (FtpWebRequest)FtpWebRequest.Create(fld);
                ftp.Credentials = new NetworkCredential(User, Pass);
                ftp.UsePassive = false;
                ftp.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                using (StreamReader resp = new StreamReader(ftp.GetResponse().GetResponseStream()))
                {
                    String line = resp.ReadLine();
                    while (line != null)
                    {
                        if (line.Trim().ToLower().StartsWith("d") || line.Contains(" <DIR> "))
                        {
                            String dir = newFiles.First(x => line.EndsWith(x));

                            if (dir == ".." || dir == ".")
                            {
                                newFiles.Remove(dir);
                                line = resp.ReadLine();
                                continue;
                            }
                                

                            newFiles.Remove(dir);
                            folders.Enqueue(fld + dir + "/");
                        }
                        line = resp.ReadLine();
                    }
                }
                files.AddRange(from f in newFiles select fld + f);
            }
            return files.ToArray();
        }

        void SetUI(bool enabled)
        {
            Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    FPT_Server_Address_TextBox.IsEnabled = enabled;
                    FPT_Server_Login_TextBox.IsEnabled = enabled;
                    FPT_Server_Password_PasswordBox.IsEnabled = enabled;

                    DoWork_Button.IsEnabled = enabled;
                }));
        }


        long GetFtpFileSize(String FileUri, String User, String Pass)
        {
            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(FileUri);
            request.Proxy = null;
            request.Credentials = new NetworkCredential(User, Pass);
            request.Method = WebRequestMethods.Ftp.GetFileSize;

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            long size = response.ContentLength;
            response.Close();

            return size;
        }


        class FtpFile
        {
            public string Path { get; set; }
            public long Size { get; set; }
        }


        class ExtensionSize
        {
            public string Extension { get; set; }
            public long Size { get; set; }
        }


        string GetFileExtension(string filename)
        {
            for(int index = filename.Length - 1; index >= 0; index--)
            {
                if (filename[index] == '.')
                    return filename.Substring(index + 1, filename.Length - index - 1).ToLower();

                if (filename[index] == '/' || filename[index] == '\\')
                    return "";
            }

            return "";
        }


        const string _DO_WORK_BUTTON_DEFAULT_TEXT = "Получить статистику";
        const string _DO_WORK_BUTTON_IN_PROCESS_TEXT = "Получение статистики...";
        void DoAllWork()
        {
            SetUI(false);

            
            //address = FPT_Server_Address_TextBox.Text;

            Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    DoWork_Button.Content = _DO_WORK_BUTTON_IN_PROCESS_TEXT;
                }));

            if (!address.StartsWith("ftp://"))
                address = "ftp://" + address;

            if (!address.EndsWith("/"))
                address = address + "/";

            //login = FPT_Server_Login_TextBox.Text;
            //password = FPT_Server_Password_PasswordBox.Password;
                

            //var allFiles = GetFilesInFtpDirectory(address, login, password);
            var allFiles = FTPListTree(address, login, password);

            var filesWithSize = new List<FtpFile>();
            foreach (var filepath in allFiles)
            {
                var size = GetFtpFileSize(filepath, login, password);

                filesWithSize.Add(new FtpFile { Path = filepath, Size = size });
            }

            var extensionsWithSize = new List<ExtensionSize>();
            long totalSize = 0;
            foreach (var fileInfo in filesWithSize)
            {
                totalSize += fileInfo.Size;

                var currFileExtension = GetFileExtension(fileInfo.Path);

                var indexOfExtension = extensionsWithSize.FindIndex(x => x.Extension == currFileExtension);

                //if (extensionsWithSize.Exists(x => x.Extension == currFileExtension))
                if (indexOfExtension == -1) // если не найдена стата по текущему расширению
                {
                    extensionsWithSize.Add(new ExtensionSize { Extension = currFileExtension, Size = fileInfo.Size });
                }
                else
                {
                    extensionsWithSize[indexOfExtension].Size += fileInfo.Size;
                }
            }

            //extensionsWithSize.OrderByDescending(x => x.Size);

            var extensionsWithSize_Sorted = extensionsWithSize.OrderByDescending(x => x.Size).ToArray();


            var results = String.Format("{0}{1,18}", "Расширение", "Размер [Байт]") + Environment.NewLine;

            foreach (var extInfo in extensionsWithSize_Sorted)
            {
                results += String.Format("{0}{1}", extInfo.Extension.PadRight(18, ' '), extInfo.Size) + Environment.NewLine; ;
            }

            results += Environment.NewLine + Environment.NewLine + String.Format("Всего: {0} Байт", totalSize);

            Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    Results_TextBox.Text = results;

                    DoWork_Button.Content = _DO_WORK_BUTTON_DEFAULT_TEXT;
                }));

            SetUI(true);
        }
    }
}
