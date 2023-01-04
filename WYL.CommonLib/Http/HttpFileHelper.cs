using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Edu.CommonLibCore.Http
{
    /// <summary>
    /// c#原生的文件上传工具类
    /// <para>author:wangyulong</para>
    /// </summary>
    public class HttpFileUtil
    {

        public delegate void DownLoadPercentHandler(long totalSize, long finishedSize, double percent, object param);

        /// <summary>
        /// Download file to success, tell download progress or throw.
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <param name="savePath"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public static async Task DownloadFileAsyc(string fileUrl, string savePath, object param, CancellationTokenSource cancelToken, DownLoadPercentHandler downLoadPercentEvent)
        {
            await Task.Run(() =>
            {
                try
                {
                    Int64 totalSize = 0; Int64 finishedSize = 0;
                    int pendingChunkSize = 1024 * 2; int chunkSize;

                    Byte[] readBytes = new Byte[pendingChunkSize];
                    Stopwatch speedTimer = new Stopwatch();

                    var isChunkedTransfer = false; //Support simple chunked transfer.

                    using (var writer = new FileStream(savePath, FileMode.Create))
                    {
                        cancelToken.Token.ThrowIfCancellationRequested();
                        var webReq = (HttpWebRequest)WebRequest.Create(fileUrl);
                        webReq.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                        cancelToken.Token.ThrowIfCancellationRequested();
                        using (var webResp = (HttpWebResponse)webReq.GetResponse())
                        {
                            cancelToken.Token.ThrowIfCancellationRequested();
                            totalSize = webResp.ContentLength;

                            if (webResp.Headers.AllKeys.Contains("Transfer-Encoding") &&
                                 webResp.Headers["Transfer-Encoding"].Contains("chunked"))
                            {
                                isChunkedTransfer = true;
                            }

                            while ((finishedSize < totalSize || isChunkedTransfer == true) && !cancelToken.IsCancellationRequested)
                            {
                                speedTimer.Start();

                                chunkSize = webResp.GetResponseStream().Read(readBytes, 0, pendingChunkSize);
                                //if (true/*IsInDesignMode*/) Thread.Sleep(20);

                                if (chunkSize > 0)
                                {
                                    finishedSize += chunkSize;
                                    cancelToken.Token.ThrowIfCancellationRequested();
                                    writer.Write(readBytes, 0, chunkSize);

                                    var speed = (int)(chunkSize * 1000 / (speedTimer.ElapsedMilliseconds + 1));

                                    double percent = Math.Round(finishedSize * 1.0 / (totalSize * 1.0) * 100, 1);
                                    downLoadPercentEvent?.Invoke(totalSize, finishedSize, percent, param);
                                    speedTimer.Reset();
                                }
                                else isChunkedTransfer = false;
                            }
                            cancelToken.Token.ThrowIfCancellationRequested();
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (File.Exists(savePath))
                        File.Delete(savePath);
                    throw ex;
                }
            });
        }


        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="files"></param>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static Task<string> UploadFile(string url, FileWrap_Upload file, Dictionary<string, object> data, Encoding encoding, object param = null, DownLoadPercentHandler handler = null)
        {
            return Task.Run<string>(() =>
            {
                try
                {
                    string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                    byte[] boundarybytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
                    byte[] endbytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

                    //1.HttpWebRequest
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.ContentType = "multipart/form-data; boundary=" + boundary;
                    request.Method = "POST";
                    request.Timeout = 5 * 1000 * 60;
                    request.KeepAlive = false;
                    //request.Credentials = CredentialCache.DefaultCredentials;

                    using (Stream stream = request.GetRequestStream())
                    {
                        //1.1 key/value
                        string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                        if (data != null)
                        {
                            foreach (string key in data.Keys)
                            {
                                stream.Write(boundarybytes, 0, boundarybytes.Length);
                                string formitem = string.Format(formdataTemplate, key, data[key]);
                                byte[] formitembytes = encoding.GetBytes(formitem);
                                stream.Write(formitembytes, 0, formitembytes.Length);
                            }
                        }

                        //1.2 file
                        string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n";
                        byte[] buffer = new byte[4096];
                        int bytesRead = 0;

                        stream.Write(boundarybytes, 0, boundarybytes.Length);
                        string header = string.Format(headerTemplate, "file", file.UploadFileName);
                        byte[] headerbytes = encoding.GetBytes(header);
                        stream.Write(headerbytes, 0, headerbytes.Length);
                        FileInfo fileInfo = new FileInfo(file.FilePath);
                        long totalSize = fileInfo.Length;
                        long finishedSize = 0;
                        using (FileStream fileStream = new FileStream(file.FilePath, FileMode.Open, FileAccess.Read))
                        {
                            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                stream.Write(buffer, 0, bytesRead);
                                finishedSize += bytesRead;
                                double percent = Math.Round(finishedSize * 1.0 / (totalSize * 1.0) * 100, 1);
                                handler?.Invoke(totalSize, finishedSize, percent, param);

                            }
                        }

                        //1.3 form end
                        stream.Write(endbytes, 0, endbytes.Length);
                    }

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                    {
                        return stream.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return "";
            });
        }
    }

    public class FileWrap_Upload
    {
        /// <summary>
        /// 文件真实路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 上传的文件名
        /// </summary>
        public string UploadFileName { get; set; }
    }
}
