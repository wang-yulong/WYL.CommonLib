using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Edu.CommonLibCore.Http
{
    /// <summary>
    /// Http请求调用工具类，使用c#原生结构
    /// </summary>
    public class HttpHelper
    {

        #region 属性

        private Dictionary<RequestAddressType, string> _baseUrlDic = new Dictionary<RequestAddressType, string>();

        public static HttpHelper Instacne = new HttpHelper();

        /// <summary>
        /// 请求类型
        /// </summary>
        public enum RequestType
        {
            Post,
            Get
        }

        /// <summary>
        /// 请求方式
        /// </summary>
        public enum ContentType
        {
            FormUrl,//application/x-www-form-urlencoded
            Json,//application/json
            Image_Jpeg//image/jpeg
        }

        /// <summary>
        /// 请求地址类型
        /// </summary>
        public enum RequestAddressType
        {
            CvShareScrren
        }

        #endregion

        #region 辅助管理

        string GetContentTypeStr(ContentType contentType)
        {
            string contentTypeStr = "";
            switch (contentType)
            {
                case ContentType.FormUrl:
                    contentTypeStr = "application/x-www-form-urlencoded";
                    break;
                case ContentType.Json:
                    contentTypeStr = "application/json";
                    break;
                case ContentType.Image_Jpeg:
                    contentTypeStr = "image/jpeg";
                    break;

            }
            return contentTypeStr;
        }

        HttpWebRequest HttpReqest_Base(string url, IDictionary<string, object> args,
            RequestType requestType, RequestAddressType addreessType, ContentType contentType)
        {
            string getUrl = BulidSendUrl(addreessType, url);
            HttpWebRequest httpWebRequest = BulidHttpWebRequest(requestType, getUrl, contentType);
            SendHttpWebReponse(args, contentType, httpWebRequest);
            return httpWebRequest;
        }

        HttpWebResponse GetHttpResponse(HttpWebRequest httpWebRequest, out bool isTrue, out string expMsg, RequestAddressType addreessType, string url)
        {
            isTrue = true;
            expMsg = "";
            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.Timeout)
                {
                    isTrue = false;
                    expMsg = $"[WebException]:[{addreessType.ToString()}]:[{url.Split('?')[0]}]:{ex.Message}";
                }
                else
                {
                    httpWebResponse = (HttpWebResponse)ex.Response;
                    if (httpWebResponse == null)
                    {
                        isTrue = false;

                    }
                    else
                    {
                        if (httpWebResponse.StatusCode != HttpStatusCode.OK
                            && !"api-m/user/wisdom/login-courier".Equals(url)
                            && !url.StartsWith("api-m/courier/temporary-save"))
                        {
                            isTrue = false;
                            int m = int.Parse(Enum.Format(typeof(HttpStatusCode), httpWebResponse.StatusCode, "d"));
                            expMsg = $"[WebException]:[{addreessType.ToString()}]:[{url.Split('?')[0]}]:{m}:{httpWebResponse.StatusCode}";
                        }

                    }
                }
            }
            return httpWebResponse;
        }

        void SendHttpWebReponse(IDictionary<string, object> args, ContentType contentType, HttpWebRequest httpWebRequest)
        {
            string str = "";
            if (args != null)
            {
                switch (contentType)
                {
                    case ContentType.FormUrl:
                        foreach (string key in args.Keys)
                        {
                            if (string.IsNullOrEmpty(key)) continue;
                            str += "&";
                            str += String.Format("{0}{1}{2}", key, "=", args[key]);
                        }
                        byte[] btBodys = Encoding.UTF8.GetBytes(str);
                        httpWebRequest.ContentLength = btBodys.Length;
                        httpWebRequest.GetRequestStream().Write(btBodys, 0, btBodys.Length);
                        break;
                    case ContentType.Json:
                        string contentJson = JsonHelper.SerializeObject(args, false, false);
                        btBodys = Encoding.UTF8.GetBytes(contentJson);
                        httpWebRequest.GetRequestStream().Write(btBodys, 0, btBodys.Length);
                        break;
                }
            }

        }

        HttpWebRequest BulidHttpWebRequest(RequestType requestType, string getUrl, ContentType contentType)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(getUrl);
            httpWebRequest.ContentType = GetContentTypeStr(contentType);
            httpWebRequest.Method = requestType.ToString();
            httpWebRequest.Timeout = 10 * 1000;
            httpWebRequest.KeepAlive = false;
            httpWebRequest.ProtocolVersion = HttpVersion.Version10;
            return httpWebRequest;
        }

        //构建请求的url
        string BulidSendUrl(RequestAddressType addressType, string url)
        {
            string tmpUrl = "";
            if (_baseUrlDic.ContainsKey(addressType))
                tmpUrl = _baseUrlDic[addressType];

            string getUrl = "";
            if (tmpUrl.StartsWith("http"))
                getUrl = $"{tmpUrl}/{url}";
            else getUrl = $"http://{tmpUrl}/{url}";
            return getUrl;
        }

        #endregion

        #region 对外接口


        /// <summary>
        /// 请求数据接口
        /// </summary>
        /// <param name="url">接口url</param>
        /// <param name="args">参数</param>
        /// <param name="requestType">请求类型</param>
        /// <param name="isUserInterface">是否用户中心相关接口</param>
        /// <returns>返回是否请求成功</returns>
        public KeyValuePair<bool, string> HttpReqest(string url, IDictionary<string, object> args,
            RequestType requestType, RequestAddressType addreessType, ContentType contentType)
        {
            bool isTrue = true;
            string responseContent = "";
            try
            {
                HttpWebRequest httpWebRequest = HttpReqest_Base(url, args, requestType, addreessType, contentType);
                HttpWebResponse httpWebResponse = GetHttpResponse(httpWebRequest, out isTrue, out responseContent, addreessType, url);

                if (isTrue && httpWebResponse != null)
                {
                    StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
                    responseContent = streamReader.ReadToEnd();
                    streamReader.Close();
                    httpWebResponse.Close();
                    httpWebRequest.Abort();
                }
            }
            catch (Exception ex)
            {
                isTrue = false;
                throw ex;
                responseContent = $"[WebException]:[{addreessType.ToString()}]:[{url.Split('?')[0]}]:{ex.Message}";
            }
            if (string.IsNullOrEmpty(responseContent))
            {
                isTrue = false;
                responseContent = $"[WebException]:返回值为空";
            }
            return new KeyValuePair<bool, string>(isTrue, responseContent);

        }


        /// <summary>
        /// 请求数据接口
        /// </summary>
        /// <param name="url">接口url</param>
        /// <param name="args">参数</param>
        /// <param name="requestType">请求类型</param>
        /// <param name="isUserInterface">是否用户中心相关接口</param>
        /// <returns>返回是否请求成功</returns>
        public KeyValuePair<bool, byte[]> HttpReqest_GetBytes(string url, IDictionary<string, object> args,
            RequestType requestType, RequestAddressType addreessType, ContentType contentType)
        {
            bool isTrue = true;
            byte[] responseBytes = null;
            try
            {
                HttpWebRequest httpWebRequest = HttpReqest_Base(url, args, requestType, addreessType, contentType);
                string expMsg = "";
                HttpWebResponse httpWebResponse = GetHttpResponse(httpWebRequest, out isTrue, out expMsg, addreessType, url);
                if (httpWebResponse != null)
                {
                    var stream = httpWebResponse.GetResponseStream();
                    Byte[] buffer = new Byte[httpWebResponse.ContentLength];
                    int offset = 0, actuallyRead = 0;
                    do
                    {
                        actuallyRead = stream.Read(buffer, offset, buffer.Length - offset);
                        offset += actuallyRead;
                    }
                    while (actuallyRead > 0);
                    responseBytes = buffer;
                    httpWebResponse.Close();
                    httpWebRequest.Abort();
                }
            }
            catch (Exception ex)
            {
                isTrue = false;
                throw ex;
            }
            return new KeyValuePair<bool, byte[]>(isTrue, responseBytes);

        }


        /// <summary>
        /// HttpUploadFile
        /// </summary>
        /// <param name="url"></param>
        /// <param name="files"></param>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public string HttpUploadFile(string url, string[] files, Dictionary<string, object> data, Encoding encoding)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            byte[] endbytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");

            //1.HttpWebRequest
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            request.KeepAlive = true;
            request.Credentials = CredentialCache.DefaultCredentials;

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
                for (int i = 0; i < files.Length; i++)
                {
                    stream.Write(boundarybytes, 0, boundarybytes.Length);
                    string header = string.Format(headerTemplate, "file" + i, Path.GetFileName(files[i]));
                    byte[] headerbytes = encoding.GetBytes(header);
                    stream.Write(headerbytes, 0, headerbytes.Length);
                    using (FileStream fileStream = new FileStream(files[i], FileMode.Open, FileAccess.Read))
                    {
                        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            stream.Write(buffer, 0, bytesRead);
                        }
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


        public void SetBaseUrl(RequestAddressType requetAddressType, string url)
        {
            if (!_baseUrlDic.ContainsKey(requetAddressType))
                _baseUrlDic.Add(requetAddressType, url);
            else
            {
                _baseUrlDic[requetAddressType] = url;
            }

        }

        #endregion

    }
}
