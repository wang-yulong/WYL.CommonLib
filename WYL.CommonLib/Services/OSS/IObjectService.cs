using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Edu.CommonLibCore.Services.OSS
{
    /// <summary>
    /// OSS操作接口定义
    /// </summary>
    public interface IObjectService
    {

        /// <summary>
        /// 初始化OSS对象
        /// </summary>
        /// <para>aliOss:endpoint,accessKeyId,accessKeySecret,bucketName</para>
        void InitObjectService(Dictionary<string, string> initDic);

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="key">oss的路径</param>
        /// <param name="fs">本地文件流</param>
        /// <returns></returns>
        void PutObject(string key, FileStream fs);

        /// <summary>
        /// 判断OSS上文件是否已经存在
        /// </summary>
        /// <param name="key">oss的路径</param>
        /// <param name="fs"></param>
        /// <returns></returns>
        bool ExistObject(string key);

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="fileName"></param>
        void DownLoadObject(string objectName, string fileName);

    }
}
