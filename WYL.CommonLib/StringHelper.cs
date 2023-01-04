using Edu.CommonLibCore.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace BaseLib
{
    /// <summary>
    /// 字符串辅助功能集
    /// </summary>
    public class StringHelper
    {

        //16进制字符串转换为字节数组
        public static byte[] HexStrTobyte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2).Trim(), 16);
            return returnBytes;
        }

        //16进制字节数组转换为字符串
        public static string GetByteStr(byte[] bytes)
        {
            StringBuilder strBuider = new StringBuilder();
            for (int index = 0; index < bytes.Length; index++)
            {
                strBuider.Append(((int)bytes[index]).ToString("X2"));
            }
            return strBuider.ToString();
        }


        /// <summary>
        /// 获取一个数组的前多少项
        /// </summary>
        /// <param name="items">数组</param>
        /// <param name="queShaoInx">后面的多少项不需要</param>
        /// <returns></returns>
        public static List<string> GetArrayPreItems(string []items,int queShaoInx)
        {
            List<string> validList = new List<string>();
            for(int i=0;i< items.Length- queShaoInx;i++)
            {
                validList.Add(items[i]);
            }
            return validList;
        }

        public static List<string> GetArrayPreItems_Before(string[] items, int queShaoInx)
        {
            List<string> validList = new List<string>();
            for (int i = queShaoInx; i < items.Length; i++)
            {
                validList.Add(items[i]);
            }
            return validList;
        }


        /// <summary>
        /// 判断两个字符串相等（含null情况）
        /// </summary>
        /// <param name="str01"></param>
        /// <param name="str02"></param>
        /// <returns></returns>
        public static bool IsEqual(string str01, string str02) {
            if (null == str01 && null == str02) return true;
            if (null == str01 || null == str02) return false;

            return str01.Equals(str02);
        }

        /// <summary>
        /// 判断一个字符是否能表示true状态
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static bool IsTrueChar(char ch) {
            return ('y' == ch || 'Y' == ch || 't' == ch || 'T' == ch || '1'==ch || '√' == ch);
        }

        /// <summary>
        /// 判断一个字符是否是括号（含中英文）
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static bool IsBracketChar(char ch) {
            if ((ch == '(' || ch == ')')
                || (ch == '[' || ch == ']')
                || (ch == '{' || ch == '}')
                || (ch == '<' || ch == '>'))
                return true;

            if ((ch == '（' || ch == '）')
                || (ch == '【' || ch == '】')
                || (ch == '《' || ch == '》'))
                return true;

            return false;
        }

        /// <summary>
        /// 将一个字符转换成机读卡支持的字符
        /// <para>小写转大写；默认为'-'</para>
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static char ToScantronChar(char ch) {
            if (ch > 127) return '-';
            if (ch >= 'a' && ch <= 'z') return (char)('A' + ch - 'a');

            return ch;
        }
        
        /// <summary>
        /// 将全角字符转换为半角字符（原本就是半角则返回自身）
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static char ToHalfWidthChar(char ch) {
            char half_char = ch;

            int input_chint = ch;
            if (input_chint == 12288) { // 空格
                half_char = (char)32;
            }
            else if (input_chint >= 65281 && input_chint <= 65374) { // 其它全角字符，对应[33, 126]范围内的半角字符
                half_char = (char)(input_chint - 65248);
            }

            return half_char;
        }

        /// <summary>
        /// 判断字符元素内容是否在ASCII范围内
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static bool IsAsciiChar(char ch) {
            return (ch > 0 && ch < 127);
        }

        /// <summary>
        /// 判断文本中是否全为ASCII字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsAllAsciiChars(string str) {
            if (string.IsNullOrEmpty(str)) return true;

            bool res = true;

            char[] arr_ch = str.ToCharArray();
            foreach (char ch in arr_ch) {
                if(!IsAsciiChar(ch)) {
                    res = false;
                    break;
                }
            }

            return res;
        }

        /// <summary>
        /// 生成md5字符串 (使用'-'连接字节)
        /// </summary>
        /// <param name="strSrc"></param>
        /// <returns></returns>
        public static string GetStrMD5(string strSrc) {
            if (string.IsNullOrEmpty(strSrc)) return "";

            byte[] src_bytes = System.Text.Encoding.UTF8.GetBytes(strSrc);
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] out_bytes = md5.ComputeHash(src_bytes);

            string str_md5 = System.BitConverter.ToString(out_bytes);
            return str_md5;
        }

        /// <summary>
        /// 获取流对象
        /// </summary>
        /// <param name="strStr"></param>
        /// <returns></returns>
        public static MemoryStream GetMemoryStream_UTF8NoBom(string strStr)
        {
            UTF8Encoding utf8 = new UTF8Encoding(false);
            MemoryStream stream = new MemoryStream(utf8.GetBytes(strStr));
            return stream;
        }

        /// <summary>
        /// 生成一个GuID字符串
        /// </summary>
        /// <returns></returns>
        public static string CrateNewGUID()
        {
           return Guid.NewGuid().ToString("D");
        }

        /// <summary>
        /// 转换为集合到字符串
        /// </summary>
        /// <param name="inputList">输入集合</param>
        /// <param name="spliteChar">分隔符</param>
        /// <returns></returns>
        public static string ConvertListToStr(List<object> inputList, string spliteChar)
        {
            StringBuilder resultStr = new StringBuilder();
            for (int i = 0; i < inputList.Count; i++)
            {
                resultStr.Append(inputList[i].ToString());
                if (i == inputList.Count - 1) continue;
                resultStr.Append(spliteChar);
            }

            return resultStr.ToString();
        }

        /// <summary>
        /// 转换字典到集合
        /// </summary>
        /// <typeparam name="T">类型1</typeparam>
        /// <typeparam name="T1">类型2</typeparam>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static List<string> ConvertDicToList<T,T1>(Dictionary<T, T1> dic,string spliteChar = ",")
        {
            List<string> resultList = new List<string>();
            foreach(var item in dic)
            {
                resultList.Add(item.Key+ spliteChar + item.Value);
            }
            return resultList;
        }

        /// <summary>
        /// 转换为集合到字符串
        /// </summary>
        /// <param name="inputList">输入集合</param>
        /// <param name="spliteChar">分隔符</param>
        /// <returns></returns>
        public static string ConvertListToStr(List<string> inputList, string spliteChar)
        {
            StringBuilder resultStr = new StringBuilder();
            for (int i = 0; i < inputList.Count; i++)
            {
                resultStr.Append(inputList[i]);
                if (i == inputList.Count - 1) continue;
                resultStr.Append(spliteChar);
            }

            return resultStr.ToString();
        }


        public static string ConvertMemoryStreamToStrByUtf8NoBom(MemoryStream stream,bool isCloseStream = false)
        {
            UTF8Encoding utf8 = new UTF8Encoding(false);
            string resultStr = utf8.GetString(stream.ToArray());
            if (isCloseStream) stream.Close();
            return resultStr;
        }


       /* /// <summary>
        /// 获取属性值，文件扩展名为ini
        /// </summary>
        /// <param name="baseCaption">标签头类似：【Base】</param>
        /// <param name="keyName">查看值的Key</param>
        /// <param name="iniFilePath">文件路径</param>
        /// <param name="dftVal">默认值</param>
        /// <returns></returns>
        public static string GetValueByFile_ini(string baseCaption,string keyName,string iniFilePath,string dftVal)
        {
            StringBuilder tempString = new StringBuilder();
            GetPrivateProfileString(baseCaption, keyName, dftVal, tempString, 1024, iniFilePath);
            return tempString.ToString();
        }*/


        /// <summary>
        /// 读取Ini文件到字典
        /// </summary>
        /// <param name="iniFilePath"></param>
        /// <returns></returns>
        public static Dictionary<string,string> ReadIniToDic(string iniFilePath)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (!File.Exists(iniFilePath)) return dic;

            List<String> fileRows = FileHelper.ReadFileByLines(iniFilePath);
            if (fileRows == null || fileRows.Count == 0) return dic;

            foreach(var itemRow in fileRows)
            {
                if (itemRow.StartsWith(";")) continue;
                string []items = itemRow.Split('=');
                if (items == null || items.Length <2 ) continue;
                string key = items[0];
                //-1 减去一个等号
                string val = itemRow.Substring(key.Length+1, itemRow.Length- key.Length-1).Trim();
                if (!dic.ContainsKey(key)) dic.Add(key,val);
            }
            return dic;


        }


        public static string TryGetValInDic(Dictionary<string, string> srcDic, string key,string dftVal)
        {
            if (srcDic == null || srcDic.Count == 0) return dftVal;
            if (!srcDic.ContainsKey(key)) return dftVal;
            return srcDic[key];
        }

        /// <summary>
        ///尝试截取字符串 截取失败，返回默认值
        /// </summary>
        /// <param name="src"></param>
        /// <param name="startInx"></param>
        /// <param name="length"></param>
        /// <param name="dftVal"></param>
        /// <returns></returns>
        public static string TrySubString(string src,int startInx,int length,string dftVal)
        {
            try
            {
                return src.Substring(startInx,length);
            }
            catch(Exception ex)
            {
                return dftVal;
            }
        }
        

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
    }
}
