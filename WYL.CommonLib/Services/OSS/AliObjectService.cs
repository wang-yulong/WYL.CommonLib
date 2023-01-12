using Aliyun.OSS;
using Aliyun.OSS.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Edu.CommonLibCore.Services.OSS
{
    /// <summary>
    /// 阿里OSS上传下载实现
    /// </summary>
    public class AliObjectService
    {
        //endpoint,accessKeyId,accessKeySecret,bucketName
        private string _bucketName; //存贮空间
        private string _accessKeyId; //oss key
        private string _accessKeySecret;
        //OSS访问地址  https://help.aliyun.com/document_detail/31837.html?spm=a2c4g.11186623.2.9.66d361bcmtkn7W#concept-zt4-cvy-5db
        private string _endpoint;

        public DateTime? GetLastModifyName(string key)
        {
            var client = new OssClient(_endpoint, _accessKeyId, _accessKeySecret);
            try
            {
                ObjectMetadata objMetatdata = client.GetObjectMetadata(_bucketName, key);
                //服务器拉取的时候跟本地的差8小时
                return objMetatdata.LastModified.AddHours(8);
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public ObjectMetadata GetObjectMetadata(string key)
        {
            var client = new OssClient(_endpoint, _accessKeyId, _accessKeySecret);
            try
            {
                ObjectMetadata objMetatdata = client.GetObjectMetadata(_bucketName, key);
                return objMetatdata;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        public bool ExistObject(string key)
        {
            // 创建OssClient实例。
            var client = new OssClient(_endpoint, _accessKeyId, _accessKeySecret);
            try
            {
                var exist = client.DoesObjectExist(_bucketName, key);
                Console.WriteLine("exist ? " + exist);
                return exist;
            }
            catch (OssException ex)
            {
                Console.WriteLine("Failed with error code: {0}; Error info: {1}. \nRequestID:{2}\tHostID:{3}",
                    ex.ErrorCode, ex.Message, ex.RequestId, ex.HostId);
                throw ex;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed with error info: {0}", ex.Message);
                throw ex;
            }
            return false;
        }


        public bool DeleteObject(string key)
        {
            // 创建OssClient实例。
            var client = new OssClient(_endpoint, _accessKeyId, _accessKeySecret);
            try
            {
                DeleteObjectResult result = client.DeleteObject(_bucketName, key);
                return result.DeleteMarker;
            }
            catch (OssException ex)
            {
                Console.WriteLine("Failed with error code: {0}; Error info: {1}. \nRequestID:{2}\tHostID:{3}",
                    ex.ErrorCode, ex.Message, ex.RequestId, ex.HostId);
                throw ex;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed with error info: {0}", ex.Message);
                throw ex;
            }
            return false;
        }

        public void InitObjectService(Dictionary<string, string> initDic)
        {
            this._bucketName = initDic["bucketName"];
            this._accessKeyId = initDic["accessKeyId"];
            this._accessKeySecret = initDic["accessKeySecret"];
            this._endpoint = initDic["endpoint"];
        }

        public void PutObject(string key, Stream fs)
        {
            // 创建OssClient实例。
            var client = new OssClient(_endpoint, _accessKeyId, _accessKeySecret);
            try
            {
                // 上传文件。
                client.PutObject(_bucketName, key, fs);
                Console.WriteLine("Put object succeeded");

            }
            catch (Exception ex)
            {
                Console.WriteLine("Put object failed, {0}", ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="objectName">// objectName 表示您在下载文件时需要指定的文件名称，如abc/efg/123.jpg。</param>
        /// <param name="downloadFilename">文件名</param>
        public bool DownLoadObject(string objectName, string downloadFilename)
        {

            // 创建OssClient实例。
            var client = new OssClient(_endpoint, _accessKeyId, _accessKeySecret);
            try
            {
                // 下载文件到流。OssObject 包含了文件的各种信息，如文件所在的存储空间、文件名、元信息以及一个输入流。
                var obj = client.GetObject(_bucketName, objectName);
                using (var requestStream = obj.Content)
                {
                    byte[] buf = new byte[1024];
                    var fs = File.Open(downloadFilename, FileMode.OpenOrCreate);
                    var len = 0;
                    // 通过输入流将文件的内容读取到文件或者内存中。
                    while ((len = requestStream.Read(buf, 0, 1024)) != 0)
                    {
                        fs.Write(buf, 0, len);
                    }
                    fs.Close();
                }
                Console.WriteLine("Get object succeeded");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Get object failed. {0}", ex.Message);
                throw ex;
            }
            return false;
        }


        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="objectName">// objectName 表示您在下载文件时需要指定的文件名称，如abc/efg/123.jpg。</param>
        /// <param name="downloadFilename">文件名</param>
        public Stream DownLoadObject(string objectName)
        {

            Stream fs = new MemoryStream();
            // 创建OssClient实例。
            var client = new OssClient(_endpoint, _accessKeyId, _accessKeySecret);
            try
            {
                // 下载文件到流。OssObject 包含了文件的各种信息，如文件所在的存储空间、文件名、元信息以及一个输入流。
                var obj = client.GetObject(_bucketName, objectName);
                using (var requestStream = obj.Content)
                {
                    byte[] buf = new byte[1024];
                    var len = 0;
                    // 通过输入流将文件的内容读取到文件或者内存中。
                    while ((len = requestStream.Read(buf, 0, 1024)) != 0)
                    {
                        fs.Write(buf, 0, len);
                    }
                }
                Console.WriteLine("Get object succeeded");
                return fs;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Get object failed. {0}", ex.Message);
                throw ex;
            }
        }


        /// <summary>
        /// 获取当前目录下的所有文件名，并按照包名分组
        /// </summary>
        /// <param name="inputDir"></param>
        /// <param name="directoryName"></param>
        /// <param name="validFileExts">有效文件扩展名</param>
        /// <returns></returns>
        public Dictionary<string, List<string>> SimpleListObjectGrps(string inputDir, string directoryName, List<string> validFileExts)
        {
            var client = new OssClient(_endpoint, _accessKeyId, _accessKeySecret);
            Dictionary<string, List<string>> resultDic = new Dictionary<string, List<string>>();
            try
            {
                var result = client.ListObjects(this._bucketName, directoryName);

                Console.WriteLine("List objects of bucket:{0} succeeded ", this._bucketName);
                foreach (var summary in result.ObjectSummaries)
                {
                    FileInfo itemInfo = new FileInfo(summary.Key);
                    if (!validFileExts.Contains(itemInfo.Extension.ToLower())) continue;
                    string[] items = summary.Key.Replace(inputDir, "").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    string packageName = items[0];
                    if (!resultDic.ContainsKey(packageName)) resultDic.Add(packageName, new List<string>() { summary.Key });
                    else resultDic[packageName].Add(summary.Key);

                    Console.WriteLine(summary.Key);
                }

                Console.WriteLine("List objects of bucket:{0} succeeded, is list all objects ? {1}", this._bucketName, !result.IsTruncated);
            }
            catch (OssException ex)
            {
                Console.WriteLine("Failed with error code: {0}; Error info: {1}. \nRequestID:{2}\tHostID:{3}",
                    ex.ErrorCode, ex.Message, ex.RequestId, ex.HostId);
                throw ex;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed with error info: {0}", ex.Message);
                throw ex;
            }
            return resultDic;
        }


        /// <summary>
        /// 获取当前目录下的所有文件名
        /// </summary>
        /// <param name="directoryName"></param>
        public List<string> SimpleListObjects(string objectName)
        {
            var client = new OssClient(_endpoint, _accessKeyId, _accessKeySecret);
            List<string> resultList = new List<string>();
            try
            {
                var result = client.ListObjects(this._bucketName, objectName);

                Console.WriteLine("List objects of bucket:{0} succeeded ", this._bucketName);
                foreach (var summary in result.ObjectSummaries)
                {
                    resultList.Add(summary.Key);
                    Console.WriteLine(summary.Key);
                }

                Console.WriteLine("List objects of bucket:{0} succeeded, is list all objects ? {1}", this._bucketName, !result.IsTruncated);
            }
            catch (OssException ex)
            {
                Console.WriteLine("Failed with error code: {0}; Error info: {1}. \nRequestID:{2}\tHostID:{3}",
                    ex.ErrorCode, ex.Message, ex.RequestId, ex.HostId);
                throw ex;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed with error info: {0}", ex.Message);
                throw ex;
            }
            return resultList;
        }



        public List<string> SimpleListObjects_All(string objectName)
        {
            var client = new OssClient(_endpoint, _accessKeyId, _accessKeySecret);
            var flag = true;
            string maker = string.Empty;
            List<string> resultList = new List<string>();

            var dayTime = 24 * 60 * 60 * 1000;
            int k = 0;
            do
            {
                var listObjectsRequest = new ListObjectsRequest(this._bucketName);
                listObjectsRequest.Prefix = objectName + "/"; //指定下一级文件
                listObjectsRequest.Marker = maker; //获取下一页的起始点，它的下一项
                listObjectsRequest.MaxKeys = 1000;//设置分页的页容量
                listObjectsRequest.Delimiter = "/";

                var result = client.ListObjects(listObjectsRequest);

                Console.WriteLine("List objects succeeded");

                foreach (var summary in result.ObjectSummaries)
                {
                    //if(new FileInfo(summary.Key).Directory.Name.Equals(new DirectoryInfo(objectName).Name))
                    resultList.Add(summary.Key);
                    Console.WriteLine($"{k}File name:{summary.Key}\r\n\r\n");
                    TimeSpan timepsan = DateTime.Now - summary.LastModified;//最后修改时间
                    /*     //时间超过一天就删除
                         if (timepsan.TotalMilliseconds > dayTime)
                         {
                             //删除oss文件
                             client.DeleteObject(this._bucketName, summary.Key);
                             Console.WriteLine($"删除{summary.Key}");
                         }*/
                    k++;
                }
                maker = result.NextMarker;
                flag = result.IsTruncated;//全部执行完后，为false
            } while (flag);

            return resultList;
        }


    }
}
