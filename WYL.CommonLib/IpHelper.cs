using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
#if NET45
using System.Management;
#endif

namespace Edu.CommonLibCore
{
    public class IpHelper
    {
        public static List<IPAddress> GetLocalIPV4(bool getAll = false)
        {
            var ret = new List<IPAddress>();
#if NET45
            try
            {
                ///排除了Microsoft 的 tunneling, miniport, WAN等类型的虚拟网卡
                ManagementObjectSearcher mos;
                var version = System.Environment.OSVersion.Version.Major + "." + System.Environment.OSVersion.Version.Minor;
                var DriverDescription = "DriverDescription";
                if (version == "6.1")//win7
                {
                    DriverDescription = "Description";
                    mos = new ManagementObjectSearcher(@"SELECT * FROM Win32_NetworkAdapter WHERE Manufacturer!='Microsoft' AND NOT PNPDeviceID LIKE 'ROOT\\%'");
                }
                else
                    mos = new ManagementObjectSearcher(@"\\.\ROOT\StandardCimv2", "SELECT * FROM MSFT_NetAdapter WHERE Virtual=False");
                ManagementObjectCollection moc = mos.Get();//获取真实网卡
                //获得网络接口，网卡，拨号器，适配器都会有一个网络接口 
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces().OrderBy(r => r.NetworkInterfaceType).ToArray();
                foreach (ManagementObject mo in moc)
                {
                    string driverDescription = mo[DriverDescription]?.ToString().ToUpper();

                    foreach (NetworkInterface network in networkInterfaces)
                    {
                        if (network.OperationalStatus == OperationalStatus.Up)//&& network.Description.ToUpper() == driverDescription
                        {
                            // 获得当前网络接口属性
                            IPInterfaceProperties properties = network.GetIPProperties();
                            // 每个网络接口可能会有多个IP地址 
                            foreach (IPAddressInformation address in properties.UnicastAddresses)
                            {
                                // 如果此IP不是ipv4，则进行下一次循环
                                if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                                    continue;
                                if (!ret.Contains(address.Address) && address.Address.ToString() != "127.0.0.1")
                                    ret.Add(address.Address);
                                if (!getAll) break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //MessageBox.Show("请检查本机是否是双网卡？且默认网段是否配置正确？", "错误");
            //CommonUtil.ExcuteKill("CastScreenClient");//强行关闭进程
#endif
            return ret;
        }

        /***
        * 获取本机正在使用的ipv4地址(访问互联网的IP)，有很多需要考虑的：
        * 1.一个电脑有多个网卡，有线的、无线的、还有vmare虚拟的两个网卡。
        * 2.就算只有一个网卡，但是该网卡配置了N个IP地址.其中还包括ipv6地址。
        * 通过查询本机路由表，获取访问默认网关时使用的网卡IP。
        * *****/
        /// <summary>
        /// 获取当前使用的IP
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIP()
        {
            string result = RunApp("route", "print", true);
            Match m = Regex.Match(result, @"0.0.0.0\s+0.0.0.0\s+(\d+.\d+.\d+.\d+)\s+(\d+.\d+.\d+.\d+)");
            if (m.Success)
            {
                //for (int i = 1; i <= m.Groups.Count; i++)
                //{
                //    LogHelper.Info("IP地址" + i + "：" + m.Groups[i - 1].Value);
                //}
                return m.Groups[2].Value;
            }
            else
            {
                try
                {
                    System.Net.Sockets.TcpClient c = new System.Net.Sockets.TcpClient();
                    c.Connect("www.baidu.com", 80);
                    string ip = ((System.Net.IPEndPoint)c.Client.LocalEndPoint).Address.ToString();
                    c.Close();
                    return ip;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 获取主机IP集合
        /// </summary>
        /// <returns></returns>
        public static List<string> GetIpAddressList()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            List<string> lst = new List<string>();
            foreach (IPAddress ip in ipHostInfo.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)//获得IPv4
                    if (!lst.Contains(ip.ToString().Trim()))
                        lst.Add(ip.ToString().Trim());
            }
            return lst;
        }

        /// <summary>
        /// 获取有效的本地IP
        /// </summary>
        /// <param name="validNetworkExt">多网络的话，需要传入网段地址</param>
        /// <returns></returns>
        public static string GetValidLoaclIp(string validNetworkExt)
        {
            if (IpHelper.GetIpAddressList().Count == 1)
            {
                if (IpHelper.GetLocalIP() == null) return "";
                return IPAddress.Parse(IpHelper.GetLocalIP()).ToString();
            }
            else
            {
                var ips = IpHelper.GetLocalIPV4(true);
                var ipLocalIP = ips.FirstOrDefault((ip) =>
                {
                    var sip = ip.ToString();
                    var sipSegment = sip.Substring(0, sip.LastIndexOf('.') + 1) + "*";
                    return sipSegment.ToString() == validNetworkExt;
                });
                if (ipLocalIP == null) ipLocalIP = ips.First();
                return ipLocalIP.ToString();
            }
        }

        #region 辅助处理

        static string RunApp(string filename, string arguments, bool recordLog)
        {
            try
            {
                if (recordLog)
                {
                    //Trace.WriteLine(filename + " " + arguments);
                }
                Process proc = new Process();
                proc.StartInfo.FileName = filename;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.Arguments = arguments;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.Start();

                using (System.IO.StreamReader sr = new System.IO.StreamReader(proc.StandardOutput.BaseStream, Encoding.Default))
                {
                    string txt = sr.ReadToEnd();
                    sr.Close();
                    if (recordLog)
                    {
                        //Trace.WriteLine(txt);
                    }
                    if (!proc.HasExited)
                    {
                        proc.Kill();
                    }
                    return txt;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return ex.Message;
            }
        }

        #endregion
    }
}
