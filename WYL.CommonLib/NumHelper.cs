using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Edu.CommonLibCore
{
    /// <summary>
    ///  值转换相关工具类
    /// </summary>
    public class NumHelper
    {
        private static int ser_int = 0;
        private static long ser_lng = 0L;

        /// <summary>
        /// 使用原子操作获取下一个int型全局序号（起始值为0）
        /// </summary>
        /// <returns></returns>
        public static int GetNextSerInt()
        {
            return Interlocked.Increment(ref ser_int);
        }

        /// <summary>
        /// 使用原子操作获取下一个long型全局序号（起始值为0）
        /// </summary>
        /// <returns></returns>
        public static long GetNextSerLng()
        {
            return Interlocked.Increment(ref ser_lng);
        }

        /// <summary>
        /// object 转换成 bool
        /// </summary>
        /// <param name="objVal"></param>
        /// <param name="dftVal"></param>
        /// <returns></returns>
        public static bool TryParseBool(object objVal, bool dftVal)
        {
            if (null == objVal) return dftVal;

            if (objVal is bool?)
            {
                return ((bool?)objVal).Value;
            }
            else if (objVal is int?)
            {
                int? int_val = (int?)objVal;
                return (int_val.Value > 0);
            }
            else if (objVal is string)
            {
                string str_val = (string)objVal;
                if (string.IsNullOrEmpty(str_val)) return dftVal;

                char ch0 = str_val[0];
                return IsTrueChar(ch0);
            }

            return dftVal;
        }


        /// <summary>
        /// 判断一个字符是否能表示true状态
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static bool IsTrueChar(char ch)
        {
            return ('y' == ch || 'Y' == ch || 't' == ch || 'T' == ch || '1' == ch || '√' == ch);
        }


        /// <summary>
        /// object 转换成 short
        /// </summary>
        /// <param name="objVal"></param>
        /// <param name="dftVal"></param>
        /// <returns></returns>
        public static short TryParseShort(object objVal, short dftVal)
        {
            if (null == objVal) return dftVal;

            if (objVal is short?)
            {
                short? val = (short?)objVal;
                return val.Value;
            }
            else if (objVal is string)
            {
                string str_val = (string)objVal;
                if (string.IsNullOrEmpty(str_val)) return dftVal;

                short val = 0;
                bool try_res = short.TryParse(str_val, out val);

                return (try_res ? val : dftVal);
            }

            return dftVal;
        }

        /// <summary>
        /// object 转换成 int
        /// </summary>
        /// <param name="objVal"></param>
        /// <param name="dftVal"></param>
        /// <returns></returns>
        public static int TryParseInt(object objVal, int dftVal)
        {
            if (null == objVal) return dftVal;

            if (objVal is int?)
            {
                int? val = (int?)objVal;
                return val.Value;
            }
            else if (objVal is string)
            {
                string str_val = (string)objVal;
                if (string.IsNullOrEmpty(str_val)) return dftVal;

                int val = 0;
                bool try_res = int.TryParse(str_val, out val);

                return (try_res ? val : dftVal);
            }
            else
            {
                int val = 0;
                if (int.TryParse(objVal.ToString(), out val))
                {
                    return val;
                }
            }
            return dftVal;
        }


        /// <summary>
        /// object 转换成 int
        /// </summary>
        /// <param name="objVal"></param>
        /// <param name="dftVal"></param>
        /// <returns></returns>
        public static byte TryParseByte(object objVal, byte dftVal)
        {
            if (null == objVal) return dftVal;

            if (objVal is byte?)
            {
                byte? val = (byte?)objVal;
                return val.Value;
            }
            else if (objVal is string)
            {
                string str_val = (string)objVal;
                if (string.IsNullOrEmpty(str_val)) return dftVal;

                byte val = 0;
                bool try_res = byte.TryParse(str_val, out val);

                return (try_res ? val : dftVal);
            }
            else
            {
                byte val = 0;
                if (byte.TryParse(objVal.ToString(), out val))
                {
                    return val;
                }
            }
            return dftVal;
        }

        /// <summary>
        /// object 转换成 long
        /// </summary>
        /// <param name="objVal"></param>
        /// <param name="dftVal"></param>
        /// <returns></returns>
        public static long TryParseLng(object objVal, long dftVal)
        {
            if (null == objVal) return dftVal;

            if (objVal is long?)
            {
                long? val = (long?)objVal;
                return val.Value;
            }
            else if (objVal is int?)
            {
                int? val = (int?)objVal;
                return val.Value;
            }
            else if (objVal is string)
            {
                string str_val = (string)objVal;
                if (string.IsNullOrEmpty(str_val)) return dftVal;

                long val = 0L;
                bool try_res = long.TryParse(str_val, out val);

                return (try_res ? val : dftVal);
            }

            return dftVal;
        }

        /// <summary>
        /// object 转换成 float
        /// </summary>
        /// <param name="objVal"></param>
        /// <param name="dftVal"></param>
        /// <returns></returns>
        public static float TryParseFloat(object objVal, float dftVal)
        {
            if (null == objVal) return dftVal;

            if (objVal is float?)
            {
                float? val = (float?)objVal;
                return val.Value;
            }
            else if (objVal is double?)
            {
                double? dbl_val = (double?)objVal;
                return (float)dbl_val.Value;
            }
            else if (objVal is string)
            {
                string str_val = (string)objVal;
                if (string.IsNullOrEmpty(str_val)) return dftVal;

                float val = 0;
                bool try_res = float.TryParse(str_val, out val);

                return (try_res ? val : dftVal);
            }

            return dftVal;
        }

        /// <summary>
        /// 百分比字符串 转换成 float
        /// </summary>
        /// <param name="strPct"></param>
        /// <param name="dftVal"></param>
        /// <returns></returns>
        public static float TryParsePct(string strPct, float dftVal)
        {
            if (string.IsNullOrEmpty(strPct)) return dftVal;

            int pos = strPct.IndexOf('%');
            if (pos <= 0) return dftVal;

            string str_val = strPct.Substring(0, pos);

            float val = 0;
            bool try_res = float.TryParse(str_val, out val);

            return (try_res ? val / 100 : dftVal);
        }

        /// <summary>
        /// object 转换成 double
        /// </summary>
        /// <param name="objVal"></param>
        /// <param name="dftVal"></param>
        /// <returns></returns>
        public static double TryParseDouble(object objVal, double dftVal)
        {
            if (null == objVal) return dftVal;

            if (objVal is double?)
            {
                double? val = (double?)objVal;
                return val.Value;
            }
            else if (objVal is float?)
            {
                float? flt_val = (float?)objVal;
                return flt_val.Value;
            }
            else if (objVal is string)
            {
                string str_val = (string)objVal;
                if (string.IsNullOrEmpty(str_val)) return dftVal;

                double val = 0;
                bool try_res = double.TryParse(str_val, out val);

                return (try_res ? val : dftVal);
            }

            return dftVal;
        }

        /// <summary>
        /// 对float按指定精度进行舍入
        /// </summary>
        /// <param name="value"></param>
        /// <param name="digits">保留小数位数, 需大于等于0</param>
        /// <param name="halfUp">末位是否使用四舍五入</param>
        /// <returns></returns>
        public static float Round(float value, int digits = 0, bool halfUp = true)
        {
            if (digits < 0) digits = 0;

            float ratio = 1f;
            while (digits-- > 0) ratio *= 10;

            int int_val = (int)(value * ratio + (halfUp ? 0.5f : 0));

            return int_val / ratio;
        }

        /// <summary>
        /// 判断2个float值在指定精度范围内是否相等
        /// </summary>
        /// <param name="flt01"></param>
        /// <param name="flt02"></param>
        /// <param name="accuracy">精度值</param>
        /// <returns></returns>
        public static bool IsEqual(float flt01, float flt02, float accuracy)
        {
            float det = flt01 - flt02;
            return (det >= -accuracy && det <= accuracy);
        }

        /// <summary>
        /// 判断2个float值在指定精度范围内是否相等
        /// </summary>
        /// <param name="dbl01"></param>
        /// <param name="dbl02"></param>
        /// <param name="accuracy">精度值</param>
        /// <returns></returns>
        public static bool IsEqual(double dbl01, double dbl02, double accuracy)
        {
            double det = dbl01 - dbl02;
            return (det >= -accuracy && det <= accuracy);
        }

        /// <summary>
		/// 将字符串转换成 int[] 数组
		/// </summary>
		/// <param name="str"></param>
        /// <param name="strSpt">分隔字符串</param>
		/// <returns></returns>
		public static int[] ParseArrInts(string str, string strSpt)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(strSpt))
            {
                return null;
            }

            string[] arr_str_int = str.Split(new string[] { strSpt }, System.StringSplitOptions.RemoveEmptyEntries);
            if (null == arr_str_int || 0 == arr_str_int.Length)
            {
                return null;
            }

            int len = arr_str_int.Length;
            int[] arr_int = new int[len];
            for (int ix = 0; ix < len; ix++)
            {
                int.TryParse(arr_str_int[ix], out arr_int[ix]);
            }

            return arr_int;
        }


        /// <summary>
        /// 获取指定数量不重复的随机数
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<int> GetRandomNums(int needCount, int minValue, int maxValue)
        {
            List<int> resultInt = new List<int>();

            while (resultInt.Count != needCount)
            {
                //随机数种子一样，产生的结果可能会死锁
                var Seed = Guid.NewGuid().GetHashCode();
                Random random = new Random(Seed);
                int currentInt = random.Next(minValue, maxValue + 1);
                if (!resultInt.Contains(currentInt))
                    resultInt.Add(currentInt);
                else if (resultInt.Count >= (maxValue - minValue + 1))
                {
                    resultInt.Add(currentInt);
                }
            }
            return resultInt;
        }

        /// <summary>
        ///  字符统计
        /// </summary>
        /// <param name="content">待统计字符串</param>
        /// <param name="charValidInfo">结果实体</param>
        public static void CharStatistic(string content, CharValidCountInfo charValidInfo)
        {
            Regex chinese = new Regex("[一-龢]");
            Regex space = new Regex("[ ]");
            Regex engilsh = new Regex("[a-zA-Z]");
            Regex number = new Regex("[0-9]");

            charValidInfo.AllCount += content.Length;
            charValidInfo.ChineseCount += chinese.Matches(content).Count;
            charValidInfo.SpaceCount += space.Matches(content).Count;
            charValidInfo.EngilshCount += engilsh.Matches(content).Count;
            charValidInfo.NumberCount += number.Matches(content).Count;
        }


        /// <summary>
        /// 尝试转换16进制的高低位字符串到10进制数字
        /// </summary>
        /// <param name="highStr"></param>
        /// <param name="lowerStr"></param>
        /// <returns></returns>
        public static int TryConvert16ToNumber(string highStr, string lowerStr)
        {
            //cmd由高低位组成，低位在前，高位在后
            int headLower = int.Parse(lowerStr, System.Globalization.NumberStyles.HexNumber);
            int headHigh = int.Parse(highStr + "", System.Globalization.NumberStyles.HexNumber);

            int total = headHigh * 256 + headLower;
            return total;

        }


        /// <summary>
        /// 逆序16进制的字符串，低位在前，高位在后
        /// </summary>
        /// <param name="hexStr"></param>
        /// <returns></returns>
        public static String ReverseHexStr(string hexStr)
        {
            StringBuilder strBulider = new StringBuilder();
            for (int i = (hexStr.Length - 1) / 2; i >= 0; i--)
            {
                strBulider.Append($"{hexStr[i * 2]}{hexStr[i * 2 + 1]}");
            }

            return strBulider.ToString();
        }


    }


    /// <summary>
    /// 字符数实体封装
    /// </summary>
    public class CharValidCountInfo
    {
        public int AllCount { get; set; } = 0;

        public int ChineseCount { get; set; } = 0;

        public int SpaceCount { get; set; } = 0;

        public int EngilshCount { get; set; } = 0;

        public int NumberCount { get; set; } = 0;

        public override string ToString()
        {
            return $"字符总数：{AllCount}, 汉字总数：{ChineseCount}，空格总数：{SpaceCount}，英文字母总数：{EngilshCount}，数字总数：{NumberCount}。";
        }
    }
}
