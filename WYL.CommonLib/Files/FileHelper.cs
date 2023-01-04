using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace Edu.CommonLibCore.Files
{
    /// <summary>
    /// 文件操作工具类
    /// <para>author:wangyulong</para>
    /// </summary>
    public class FileHelper
    {
        #region 目录文件获取


        /// <summary>
        /// 获取存在有效文件的目录
        /// </summary>
        /// <param name="inputDir"></param>
        /// <param name="exts"></param>
        /// <returns></returns>
        public static List<DirInfo> GetDirInfos(string inputDir, string[] exts)
        {
            FileInfo[] infos = FileHelper.GetFileInfos(inputDir, exts);
            if (infos == null) return null;


            List<string> tmpList = new List<string>();
            List<DirInfo> resultInfos = new List<DirInfo>();
            foreach (var itemInfo in infos)
            {
                if (tmpList.Contains(itemInfo.Directory.FullName)) continue;
                tmpList.Add(itemInfo.Directory.FullName);
                resultInfos.Add(new DirInfo(itemInfo.Directory.Name, itemInfo.Directory.FullName));
            }

            return resultInfos;
        }

        /// <summary>
        /// 获取Excel文件（过滤~$对于的临时文件）
        /// </summary>
        /// <param name="directName"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static FileInfo[] GetExcelInfos(string directName, SearchOption searchOption = SearchOption.AllDirectories)
        {
            FileInfo[] itemInfos = GetFileInfos(directName, new string[] { ".xls", ".xlsx" });

            List<FileInfo> resultInfoList = new List<FileInfo>();
            foreach (var temInfo in itemInfos)
            {
                if (temInfo.Name.StartsWith("~$")) continue;
                resultInfoList.Add(temInfo);

            }
            return resultInfoList.ToArray();
        }


        /// <summary>
        /// 获取当前目录下的文件列表
        /// </summary>
        /// <param name="directName">目录文件</param>
        /// <param name="filterStrs">扩展名限制【.txt,.doc】</param>
        /// <param name="relativeDir">相对目录名，路径返回自动过滤相对目录，如D:\\aa\\test.txt,可返回test.txt</param>
        /// <param name="searchOption">当前目录，还是递归查询子目录</param>
        /// <returns></returns>
        public static List<string> GetFileInfoList(string directName, string[] filterStrs, string relativeDir = "", SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (!Directory.Exists(directName))
            {
                return new List<string>();
            }
            if (filterStrs == null)
            {
                filterStrs = new string[0];
            }
            new Stopwatch().Start();
            FileInfo[] fileInfos = new DirectoryInfo(directName).GetFiles("*.*", searchOption);
            List<string> resultFileInfos = new List<string>();
            if (fileInfos != null && fileInfos.Length != 0)
            {
                object lockObj = new object();
                List<string> extList = new List<string>();
                string[] array = filterStrs;
                foreach (string itemExt in array)
                {
                    extList.Add(itemExt.ToUpper());
                }
                Parallel.ForEach(fileInfos, delegate (FileInfo item, ParallelLoopState loopstate)
                {
                    if ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden && (extList.Count == 0 || extList.Contains(Path.GetExtension(item.Name).ToUpper())))
                    {
                        lock (lockObj)
                        {
                            resultFileInfos.Add(item.FullName.Replace(relativeDir, ""));
                        }
                    }
                });
            }
            return resultFileInfos;
        }


        /// <summary>
        ///  获取该文件目录下的所有文件名
        /// </summary>
        /// <param name="directName">目录名称</param>
        /// <param name="filterStr"> 过滤器字符串 </param>
        /// <returns></returns>
        public static List<string> GetDirectAllFileNames(string directName, string[] filterStrs)
        {
            if (!Directory.Exists(directName)) return null;

            var files = Directory.GetFiles(directName, "*.*", SearchOption.AllDirectories);
            List<string> resultList = new List<string>();
            if (filterStrs == null)
            {
                foreach (var item in files)
                {
                    resultList.Add(item);
                }
                return resultList;
            }

            if (files != null && files.Length > 0)
            {
                foreach (var item in files)
                {
                    if (filterStrs != null)
                    {
                        foreach (var filterName in filterStrs)
                        {
                            if (item.EndsWith(filterName))
                            {
                                resultList.Add(item);
                                break;
                            }
                        }
                    }
                }
            }
            return resultList.ToList();
        }

        /// <summary>
        ///  获取该文件目录下的所有文件名
        /// </summary>
        /// <param name="directName">目录名称</param>
        /// <param name="filterStr"> 过滤器字符串 </param>
        /// <returns></returns>
        public static List<string> GetDirectFileNames(string directName, string filterStr)
        {
            if (!Directory.Exists(directName)) return null;

            DirectoryInfo folder = new DirectoryInfo(directName);

            List<string> fileNameList = new List<string>();
            foreach (FileInfo file in folder.GetFiles(filterStr))
            {
                fileNameList.Add(file.Name);
            }
            return fileNameList;
        }

        /// <summary>
        ///  获取该文件目录下的所有文件
        /// </summary>
        /// <param name="directName">目录名称</param>
        /// <param name="filterStr"> 过滤器字符串 </param>
        /// <returns></returns>
        public static FileInfo[] GetFileInfos_SortByFileName(string directName, string[] filterStrs, SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (!Directory.Exists(directName)) return null;

            DirectoryInfo folder = new DirectoryInfo(directName);
            FileInfo[] fileInfos = folder.GetFiles("*.*", searchOption);
            if (filterStrs == null || filterStrs.Length == 0) return fileInfos;

            List<FileInfo> resultFileInfos = new List<FileInfo>();
            if (fileInfos != null && fileInfos.Length > 0)
            {
                foreach (var item in fileInfos)
                {
                    if ((item.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) continue;
                    if (filterStrs != null)
                    {
                        foreach (var filterName in filterStrs)
                        {
                            if (item.Extension.ToUpper().EndsWith(filterName.ToUpper()))
                            {
                                resultFileInfos.Add(item);
                                break;
                            }
                        }
                    }
                }
            }
            resultFileInfos.Sort(new FileComparer());
            return resultFileInfos.ToArray();
        }

        /// <summary>
        ///  获取该文件目录下的所有文件
        /// </summary>
        /// <param name="directName">目录名称</param>
        /// <param name="filterStr"> 过滤器字符串 </param>
        /// <returns></returns>
        public static FileInfo[] GetFileInfos(string directName, string[] filterStrs, SearchOption searchOption = SearchOption.AllDirectories)
        {
            //       FileSystemInfo[] Fsi = di.GetFileSystemInfos(); 直接获取磁盘目录文件不报错
            if (!Directory.Exists(directName)) return new FileInfo[] { };

            Stopwatch watch = new Stopwatch();
            watch.Start();

            DirectoryInfo folder = new DirectoryInfo(directName);
            FileInfo[] fileInfos = folder.GetFiles("*.*", searchOption);

            List<FileInfo> resultFileInfos = new List<FileInfo>();

            if (fileInfos != null && fileInfos.Length > 0)
            {
                if (null == filterStrs)
                {
                    resultFileInfos.AddRange(fileInfos);
                }
                else
                {
                    object lockObj = new object();
                    List<string> extList = new List<string>();
                    foreach (var itemExt in filterStrs)
                    {
                        extList.Add(itemExt.ToUpper());
                    }
                    Parallel.ForEach(fileInfos, (item, loopstate) =>
                    {
                        if ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                        {
                            if (extList.Contains(Path.GetExtension(item.Name).ToUpper()))
                            {
                                lock (lockObj)
                                    resultFileInfos.Add(item);
                            }
                        }

                    });
                }
            }
            resultFileInfos.Sort(new FileCompare_ByDirectory());
            // Console.WriteLine($"加载文件数{fileInfos.Length}个，耗时{watch.ElapsedMilliseconds/1000f}s");
            return resultFileInfos.ToArray();
        }

        #endregion

        #region 获取文件大小

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileSize(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Length < 1024)
            {
                return fileInfo.Length + "KB";
            }
            if (fileInfo.Length >= 1024 && fileInfo.Length < 1048576)
            {
                return GetMB(fileInfo.Length);
            }
            if (fileInfo.Length >= 1048576 && fileInfo.Length < 1073741824)
            {
                return GetGB(fileInfo.Length);
            }
            if (fileInfo.Length >= 1073741824 && fileInfo.Length < 1099511627776L)
            {
                return GetTB(fileInfo.Length);
            }
            return fileInfo.Length + "KB";
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="fileSize"></param>
        /// <returns></returns>
        public static string GetFileSize(long fileSize)
        {
            if (fileSize < 1024)
            {
                return fileSize + "B";
            }
            if (fileSize >= 1024 && fileSize < 1048576)
            {
                return GetKB(fileSize);
            }
            if (fileSize >= 1048576 && fileSize < 1073741824)
            {
                return GetMB(fileSize);
            }
            if (fileSize >= 1073741824 && fileSize < 1099511627776L)
            {
                return GetGB(fileSize);
            }
            if (fileSize >= 1099511627776L && fileSize < 1125899906842624L)
            {
                return GetTB(fileSize);
            }
            return fileSize + "B";
        }


        private static string GetTB(long b)
        {
            double dd = b;
            for (int i = 0; i < 4; i++)
            {
                dd /= 1024.0;
            }
            return dd.ToString("0.000000") + "TB";
        }

        private static string GetGB(long b)
        {
            double dd = b;
            for (int i = 0; i < 3; i++)
            {
                dd /= 1024.0;
            }
            return dd.ToString("0.000000") + "GB";
        }

        private static string GetMB(long b)
        {
            double dd = b;
            for (int i = 0; i < 2; i++)
            {
                dd /= 1024.0;
            }
            return dd.ToString("0.000000") + "MB";
        }

        private static string GetKB(long b)
        {
            double dd = b;
            for (int i = 0; i < 1; i++)
            {
                dd /= 1024.0;
            }
            return dd.ToString("0.000000") + "KB";
        }

        #endregion

        /// <summary>
        /// 强制替换文件名中的非法字符
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ReplaceUnValidFilePathChars(string filePath)
        {
            char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
            foreach (char rInvalidChar in invalidFileNameChars)
            {
                filePath = filePath.Replace(rInvalidChar.ToString(), string.Empty);
            }
            return filePath;
        }

        #region 文件框选择


        /// <summary>
        ///  打开选择目录对话框
        /// </summary>
        /// <returns></returns>
        public static string OpenSelectDirectDialog()
        {
            FolderBrowserDialog browserDialog = new FolderBrowserDialog();
            DialogResult result = browserDialog.ShowDialog();
            if (result == DialogResult.OK) return browserDialog.SelectedPath;
            else return "";
        }

        /// <summary>
        /// 弹出文件选择对话框
        /// </summary>
        /// <param name="initDir">初始显示目录</param>
        /// <param name="filterStr">过滤字符串</param>
        /// <returns></returns>
        public static string OpenSelectFileDialog(string initDir = "", string filterStr = "")
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (!string.IsNullOrEmpty(initDir)) openFileDialog.InitialDirectory = initDir;
            if (!string.IsNullOrEmpty(filterStr)) openFileDialog.Filter = filterStr;
            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                return openFileDialog.FileName;
            }
            else return null;
        }

        /// <summary>
        /// 弹出文件选择对话框
        /// </summary>
        /// <param name="initDir">初始显示目录</param>
        /// <param name="filterStr">过滤字符串</param>
        /// <returns></returns>
        public static string[] OpenSelectFileDialog(bool isMul)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = isMul;

            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                return openFileDialog.FileNames;
            }
            else return null;
        }

        #endregion

        #region 读文件

        /// <summary>
        /// 读取文件到二进制数组
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static byte[] ReadFile(string path)
        {
            if (!File.Exists(path)) return null;

            var tmpStream = new FileStream(path, FileMode.Open);
            //获取文件大小
            long size = tmpStream.Length;
            byte[] array = new byte[size];
            //将文件读到byte数组中
            tmpStream.Read(array, 0, array.Length);

            tmpStream.Close();

            return array;
        }

        /// <summary>
        /// 读取文件到字符串中
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReadFileToStr(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return "";

            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader streamReader = new StreamReader(fileStream, GetType(path));
            fileStream.Seek(0, SeekOrigin.Begin);
            string content = streamReader.ReadToEnd();
            fileStream.Close();
            return content;

        }

        /// <summary>
        /// 读取文件到字符串中
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReadFileToStr(Stream stream)
        {
            StreamReader streamReader = new StreamReader(stream, GetType(stream, false));
            stream.Seek(0, SeekOrigin.Begin);
            string content = streamReader.ReadToEnd();
            return content;

        }

        /// <summary>
        /// 按行读取文件
        /// </summary>
        /// <returns></returns>
        public static List<string> ReadFileByLines(string path)
        {
            List<string> rowList = new List<string>();
            if (!File.Exists(path)) return rowList;
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                StreamReader streamReader = new StreamReader(fileStream, GetType(path));
                fileStream.Seek(0, SeekOrigin.Begin);
                string content = streamReader.ReadLine();
                while (content != null)
                {
                    rowList.Add(content);
                    content = streamReader.ReadLine();
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (fileStream != null) fileStream.Close();
            }

            return rowList;
        }

        #endregion

        #region 写文件

        /// <summary>
        /// 写二进制数组到文件中去
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileBytes"></param>
        /// <returns></returns>
        public static void WriteFile(string path, byte[] fileBytes)
        {
            var tmpStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            tmpStream.Write(fileBytes, 0, fileBytes.Length);
            tmpStream.Close();
        }

        /// <summary>
        /// 写二进制数组到文件中去
        /// </summary>
        /// <param name="directoryPath">需创建的文件夹名称</param>
        /// <param name="fileName">需创建的文件名</param>
        /// <param name="fileBytes">文件存贮的二进制数组</param>
        /// <returns></returns>
        public static void WriteFile(string directoryPath, string fileName, byte[] fileBytes)
        {
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

            string path = directoryPath + "/" + fileName;

            var tmpStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            tmpStream.Write(fileBytes, 0, fileBytes.Length);
            tmpStream.Close();
        }

        /// <summary>
        /// 使用特定的编码写文件到本地
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        /// <param name="encoding"></param>
        /// <param name="fileMode"></param>
        public static void WriteFile(string filePath, string content, Encoding encoding, FileMode fileMode = FileMode.Create)
        {
            FileInfo info = new FileInfo(filePath);
            if (!Directory.Exists(info.Directory.FullName))
                Directory.CreateDirectory(info.Directory.FullName);
            if (fileMode == FileMode.Create && File.Exists(filePath)) File.Delete(filePath);

            FileStream stream = new FileStream(filePath, fileMode);
            StreamWriter writer = new StreamWriter(stream, encoding);
            writer.Write(content);
            writer.Flush();
            writer.Close();
            stream.Close();
        }

        /// <summary>
        /// 使用UTF8的方式 写文件到本地
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        /// <param name="fileMode"></param>
        public static void WriteFile_UTF8NOBoming(string filePath, string content, FileMode fileMode = FileMode.Create)
        {
            UTF8Encoding uTF8 = new UTF8Encoding(false);
            WriteFile(filePath, content, uTF8, fileMode);
        }

        #endregion

        #region 获取文件编码

        /// <summary> 
        /// 给定文件的路径，读取文件的二进制数据，判断文件的编码类型 
        /// </summary> 
        /// <param name=“FILE_NAME“>文件路径</param> 
        /// <returns>文件的编码类型</returns> 
        public static System.Text.Encoding GetType(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            Encoding r = GetType(fs);
            fs.Close();
            return r;
        }

        /// <summary> 
        /// 通过给定的文件流，判断文件的编码类型 
        /// </summary> 
        /// <param name=“fs“>文件流</param> 
        /// <returns>文件的编码类型</returns> 
        public static Encoding GetType(Stream fs, bool isClose = true)
        {
            byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM 
            Encoding reVal = Encoding.Default;

            BinaryReader r = new BinaryReader(fs, System.Text.Encoding.Default);
            int i;
            int.TryParse(fs.Length.ToString(), out i);
            byte[] ss = r.ReadBytes(i);
            if (IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF))
            {
                reVal = Encoding.UTF8;
            }
            else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
            {
                reVal = Encoding.BigEndianUnicode;
            }
            else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
            {
                reVal = Encoding.Unicode;
            }
            if (isClose) r.Close();
            return reVal;

        }

        /// <summary> 
        /// 判断是否是不带 BOM 的 UTF8 格式 
        /// </summary> 
        /// <param name=“data“></param> 
        /// <returns></returns> 
        private static bool IsUTF8Bytes(byte[] data)
        {
            int charByteCounter = 1; //计算当前正分析的字符应还有的字节数 
            byte curByte; //当前分析的字节. 
            for (int i = 0; i < data.Length; i++)
            {
                curByte = data[i];
                if (charByteCounter == 1)
                {
                    if (curByte >= 0x80)
                    {
                        //判断当前 
                        while (((curByte <<= 1) & 0x80) != 0)
                        {
                            charByteCounter++;
                        }
                        //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X 
                        if (charByteCounter == 1 || charByteCounter > 6)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    //若是UTF-8 此时第一位必须为1 
                    if ((curByte & 0xC0) != 0x80)
                    {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if (charByteCounter > 1)
            {
                throw new Exception("非预期的byte格式");
            }
            return true;
        }

        #endregion

        #region 文件Copy&Move

        /// <summary>
        /// 尝试创建文件目录
        /// </summary>
        /// <param name="filePath"></param>
        public static void TryCreateDir(string filePath)
        {
            FileInfo info = new FileInfo(filePath);
            if (!Directory.Exists(info.Directory.FullName))
                Directory.CreateDirectory(info.Directory.FullName);
        }

        /// <summary>
        /// 尝试移动文件
        /// </summary>
        /// <param name="srcFile"></param>
        /// <param name="newFile"></param>
        public static void TryMoveFile(string srcFile, string newFile)
        {
            TryCreateDir(newFile);

            try
            {
                File.Copy(srcFile, newFile, true);
                File.Delete(srcFile);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 尝试移动文件
        /// </summary>
        /// <param name="srcFile"></param>
        /// <param name="newFile"></param>
        public static void TryCopyFile(string srcFile, string newFile)
        {
            TryCreateDir(newFile);

            try
            {
                File.Copy(srcFile, newFile, true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 直接调用调用的剪切方法
        /// </summary>
        /// <param name="srcFile"></param>
        /// <param name="newFile"></param>
        public static void TryMoveFileExt(string srcFile, string newFile)
        {
            if (!File.Exists(srcFile)) return;
            if (File.Exists(newFile)) return;
            if (srcFile.Equals(newFile)) return;
            TryCreateDir(newFile);
            File.Move(srcFile, newFile);
        }

        #endregion

        #region 文件Md5 & Sha256计算


        /// <summary>
        /// 获取文件的MD5值
        /// </summary>
        /// <param name="filePath">文件地址</param>
        /// <returns></returns>
        public static string GetFileMD5String(string filePath)
        {
            string md5Result = string.Empty;
            if (File.Exists(filePath))
            {
                using (var md5 = new MD5CryptoServiceProvider())
                {
                    byte[] buffer = md5.ComputeHash(File.ReadAllBytes(filePath));
                    md5Result = BitConverter.ToString(buffer);
                }
            }
            return md5Result;
        }

        /// <summary>
        /// 对文件进行MD5加密
        /// </summary>
        /// <param name="filePath"></param>
        public static string GetMd5File_BigFile(string filePath, long maxBuffer = -1)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            int bufferSize = 1048576; // 缓冲区大小，1MB
            byte[] buff = new byte[bufferSize];

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            md5.Initialize();

            long offset = 0;
            while (offset < fs.Length)
            {
                long readSize = bufferSize;
                if (offset + readSize > fs.Length)
                {
                    readSize = fs.Length - offset;
                }

                fs.Read(buff, 0, Convert.ToInt32(readSize)); // 读取一段数据到缓冲区

                // 最后一块或者超过预设的最大值（部分大视频计算整个文件的Md5超级慢）
                if (offset + readSize >= fs.Length || (maxBuffer != -1 && offset + readSize > maxBuffer))
                {
                    md5.TransformFinalBlock(buff, 0, Convert.ToInt32(readSize));

                }
                else// 不是最后一块
                {
                    md5.TransformBlock(buff, 0, Convert.ToInt32(readSize), buff, 0);
                }

                offset += bufferSize;
                if (maxBuffer != -1 && offset > maxBuffer)
                    break;
            }

            fs.Close();
            byte[] result = md5.Hash;
            md5.Clear();

            StringBuilder sb = new StringBuilder(32);
            for (int i = 0; i < result.Length; i++)
            {
                sb.Append(result[i].ToString("X2"));
            }

            Console.WriteLine("计算Md5耗时：" + stopwatch.ElapsedMilliseconds + "ms");

            return sb.ToString();
        }

        /// <summary>
        /// 获取文件的sha256
        /// </summary>
        public static string GetFileSha256(string filePath)
        {
            if (!File.Exists(filePath)) return "";
            using (FileStream inputStream = new FileStream(filePath, FileMode.Open))
            {
                return BitConverter.ToString(new SHA256Managed().ComputeHash(inputStream)).Replace("-", "").ToLower();
            }
        }

        #endregion

        #region Stream&Byte互相转换

        /// <summary> 
        /// 将 Stream 转成 byte[] 
        /// 此种方式不支持FtpStream
        /// </summary> 
        public static byte[] StreamToBytes(Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            // 设置当前流的位置为流的开始 
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }

        /// <summary> 
        /// 将 byte[] 转成 Stream 
        /// </summary> 
        public static Stream BytesToStream(byte[] bytes)
        {
            Stream stream = new MemoryStream(bytes);
            return stream;
        }

        #endregion

        /// <summary>
        /// 获取唯一的文件名称
        /// 如文件存在，自动重命名
        /// </summary>
        /// <param name="srcPath"></param>
        /// <returns></returns>
        public static string GetUniqueName(string srcPath)
        {
            if (!File.Exists(srcPath)) return srcPath;
            FileInfo fileinfo = new FileInfo(srcPath);
            for (int i = 0; i < int.MaxValue; i++)
            {
                string newName = Path.Combine(fileinfo.Directory.FullName, $"{Path.GetFileNameWithoutExtension(fileinfo.Name)}_({i + 1}){fileinfo.Extension}");
                if (!File.Exists(newName)) return newName;
            }

            return srcPath;
        }

        /// <summary>
        /// 获取文件版本
        /// </summary>
        /// <returns></returns>
        public static string GetFileVersion(string filePath)
        {
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(filePath);
            return fvi.FileVersion;
        }

        /// <summary>
        /// 获取所有注册的程序
        /// </summary>
        public static void GetAllInstallsPrograms()
        {
            int i = 0;
            string registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (Microsoft.Win32.RegistryKey key = Registry.LocalMachine.OpenSubKey(registry_key))
            {
                foreach (string subkey_name in key.GetSubKeyNames())
                {
                    using (RegistryKey subkey = key.OpenSubKey(subkey_name))
                    {
                        Console.WriteLine($"{++i}:{subkey.GetValue("DisplayName")}");
                    }
                }
            }
        }

        #region 获取默认打开程序

        /// <summary>
        /// 获取默认打开程序
        /// </summary>
        /// <param name="association"></param>
        /// <param name="extension">扩展名[.txt]</param>
        /// <returns></returns>
        public static string GetDefeaultProgram(string extension, AssocStr association = AssocStr.Command)
        {
            const int S_OK = 0;
            const int S_FALSE = 1;

            uint length = 0;
            uint ret = AssocQueryString(AssocF.None, association, extension, null, null, ref length);
            if (ret != S_FALSE)
            {
                throw new InvalidOperationException("Could not determine associated string");
            }

            var sb = new StringBuilder((int)length); // (length-1) will probably work too as the marshaller adds null termination
            ret = AssocQueryString(AssocF.None, association, extension, null, sb, ref length);
            if (ret != S_OK)
            {
                throw new InvalidOperationException("Could not determine associated string");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="str"></param>
        /// <param name="pszAssoc"></param>
        /// <param name="pszExtra"></param>
        /// <param name="pszOut"></param>
        /// <param name="pcchOut"></param>
        /// <returns></returns>
        [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern uint AssocQueryString(
            AssocF flags,
            AssocStr str,
            string pszAssoc,
            string pszExtra,
            [Out] StringBuilder pszOut,
            ref uint pcchOut
        );

      
        [Flags]
        public enum AssocF
        {
            None = 0,
            Init_NoRemapCLSID = 0x1,
            Init_ByExeName = 0x2,
            Open_ByExeName = 0x2,
            Init_DefaultToStar = 0x4,
            Init_DefaultToFolder = 0x8,
            NoUserSettings = 0x10,
            NoTruncate = 0x20,
            Verify = 0x40,
            RemapRunDll = 0x80,
            NoFixUps = 0x100,
            IgnoreBaseClass = 0x200,
            Init_IgnoreUnknown = 0x400,
            Init_Fixed_ProgId = 0x800,
            Is_Protocol = 0x1000,
            Init_For_File = 0x2000
        }

        public enum AssocStr
        {
            Command = 1,
            Executable,
            FriendlyDocName,
            FriendlyAppName,
            NoOpen,
            ShellNewValue,
            DDECommand,
            DDEIfExec,
            DDEApplication,
            DDETopic,
            InfoTip,
            QuickTip,
            TileInfo,
            ContentType,
            DefaultIcon,
            ShellExtension,
            DropTarget,
            DelegateExecute,
            Supported_Uri_Protocols,
            ProgID,
            AppID,
            AppPublisher,
            AppIconReference,
            Max
        }

        #endregion

        #region 内部类

        /// <summary>
        /// 按照磁盘顺序对文件进行排序
        /// </summary>
        public class FileComparer : IComparer<FileInfo>
        {
            [System.Runtime.InteropServices.DllImport("Shlwapi.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
            public static extern int StrCmpLogicalW(string psz1, string psz2);
            public int Compare(FileInfo psz1, FileInfo psz2)
            {
                return StrCmpLogicalW(psz1.Name, psz2.Name);
            }
        }

        /// <summary>
        /// 按照磁盘顺序对文件进行排序
        /// </summary>
        public class FileCompare_ByDirectory : IComparer<FileInfo>
        {
            [System.Runtime.InteropServices.DllImport("Shlwapi.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
            public static extern int StrCmpLogicalW(string psz1, string psz2);
            public int Compare(FileInfo psz1, FileInfo psz2)
            {
                return StrCmpLogicalW(psz1.FullName, psz2.FullName);
            }
        }

        [System.Runtime.InteropServices.DllImport("Shlwapi.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);

        #endregion

    }
}
