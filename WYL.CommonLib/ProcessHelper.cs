using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Edu.CommonLibCore
{
    /// <summary>
    /// 进程操作工具类
    /// <para>author:wangyulong</para>
    /// </summary>
    public class ProcessHelper
    {
        static Dictionary<string, PerformanceCounter> processCpuntDic = new Dictionary<string, PerformanceCounter>();

        /// <summary>
        /// 进程是否存在
        /// </summary>
        /// <param name="exe"></param>
        /// <returns></returns>
        public static bool ProcessIsExist(string exe)
        {
            try
            {
                Process[] process = Process.GetProcessesByName(exe);
                if (process == null || process.Length == 0) return false;
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 尝试杀掉进程
        /// </summary>
        /// <param name="exe"></param>
        public static void ExcuteKill(string exe)
        {
            try
            {
                Process[] process = Process.GetProcessesByName(exe);
                foreach (Process iprocess in process)
                {
                    iprocess.Kill();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 尝试启动进程
        /// </summary>
        /// <param name="exePath"></param>
        public static void TryStartProcess(string exePath)
        {
            try
            {
                if (!File.Exists(exePath))
                {
                    throw new Exception("进程文件不存在") ;
                }
                //Process.Start(exePath);
                Process pro = new Process();
                FileInfo file = new FileInfo(exePath);
                pro.StartInfo.WorkingDirectory = file.Directory.FullName;
                pro.StartInfo.UseShellExecute = false;
                pro.StartInfo.FileName = exePath;
                //pro.StartInfo.CreateNoWindow = true;
                pro.StartInfo.Arguments = "";
                pro.Start();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 定时记录Cpu&内存使用情况
        /// </summary>
        public static void RecordCpuAndMemorys()
        {
            //PerformanceCounter curpcp = new PerformanceCounter("Process", "Working Set - Private", cur.ProcessName); 进程独占内存
            //PerformanceCounter curpc = new PerformanceCounter("Process", "Working Set", cur.ProcessName); 进程共享+独占内存
            //PerformanceCounter curtime = new PerformanceCounter("Process", "% Processor Time", cur.ProcessName); 进程Cpu使用率
            //PerformanceCounter totalcpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");  Cpu总的使用率
#if NET40

        Task.Factory.StartNew(()=>{
          try
                {
                    Process cur = Process.GetCurrentProcess();
                    PerformanceCounter curtime = new PerformanceCounter("Process", "% Processor Time", cur.ProcessName);

                    while (true)
                    {
                        try
                        {
                            string processCpuStr = (curtime.NextValue() / Environment.ProcessorCount).ToString("0.00") + "%";
                            string taotalMsg = GetProcessCpu(cur).Replace("[进程CPU%]", processCpuStr); ;
                            List<Process> chilidProcess = new List<Process>();
                            GetChilidProcess(cur, ref chilidProcess);
                            foreach (var itemProcess in chilidProcess)
                            {
                                if (!processCpuntDic.ContainsKey(itemProcess.ProcessName))
                                    processCpuntDic.Add(itemProcess.ProcessName, new PerformanceCounter("Process", "% Processor Time", itemProcess.ProcessName));

                                PerformanceCounter tmpProcessCount = processCpuntDic[itemProcess.ProcessName];
                                string tmpProcessCpu = (tmpProcessCount.NextValue() / Environment.ProcessorCount).ToString("0.00") + "%";
                                string tmpMsg = GetProcessCpu(itemProcess).Replace("[进程CPU%]", tmpProcessCpu);
                                
                            }
                            Thread.Sleep(2 * 1000);
                        }
                        catch (Exception ex)
                        {
                            
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
        
        });

#else


            Task.Run(() =>
            {
                try
                {
                    Process cur = Process.GetCurrentProcess();
                    PerformanceCounter curtime = new PerformanceCounter("Process", "% Processor Time", cur.ProcessName);

                    while (true)
                    {
                        try
                        {
                            string processCpuStr = (curtime.NextValue() / Environment.ProcessorCount).ToString("0.00") + "%";
                            string taotalMsg = GetProcessCpu(cur).Replace("[进程CPU%]", processCpuStr); ;
                            List<Process> chilidProcess = new List<Process>();
                            GetChilidProcess(cur, ref chilidProcess);
                            foreach (var itemProcess in chilidProcess)
                            {
                                if (!processCpuntDic.ContainsKey(itemProcess.ProcessName))
                                    processCpuntDic.Add(itemProcess.ProcessName, new PerformanceCounter("Process", "% Processor Time", itemProcess.ProcessName));

                                PerformanceCounter tmpProcessCount = processCpuntDic[itemProcess.ProcessName];
                                string tmpProcessCpu = (tmpProcessCount.NextValue() / Environment.ProcessorCount).ToString("0.00") + "%";
                                string tmpMsg = GetProcessCpu(itemProcess).Replace("[进程CPU%]", tmpProcessCpu);
                                
                            }
                            Thread.Sleep(2 * 1000);
                        }
                        catch (Exception ex)
                        {
                            
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            });

#endif

        }

        /// <summary>
        ///  获取当前进程的所有子进程
        /// </summary>
        /// <param name="currentProcess"></param>
        /// <param name="childProcessList"></param>
        public static void GetChilidProcess(Process currentProcess, ref List<Process> childProcessList)
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + currentProcess.Id);
                ManagementObjectCollection moc = searcher.Get();
                foreach (ManagementObject mo in moc)
                {
                    Process tmpProcess = Process.GetProcessById(Convert.ToInt32(mo["ProcessID"]));
                    childProcessList.Add(tmpProcess);
                    GetChilidProcess(tmpProcess, ref childProcessList);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

#region 检测某某程序是否已安装



#endregion

#region 辅助处理


        static string GetProcessCpu(Process pro)
        {
           /* string zhanYongNeiYun = (pro.PrivateMemorySize64 / 1024d / 1024d).ToString("0.00") + "MB";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher($"select * from Win32_Processor");
            string cupMsg = "";
            foreach (ManagementObject myobject in searcher.Get())
            {
                cupMsg = $"Cup占用率:{myobject["LoadPercentage"]}%";
            }

            Microsoft.VisualBasic.Devices.Computer myInfo = new Microsoft.VisualBasic.Devices.Computer();
            string total = (myInfo.Info.TotalPhysicalMemory / 1024d / 1024d).ToString("0.00");
            string avail = (myInfo.Info.AvailablePhysicalMemory / 1024d / 1024d).ToString("0.00");
            string percent = (((myInfo.Info.TotalPhysicalMemory - myInfo.Info.AvailablePhysicalMemory) / 1d / myInfo.Info.TotalPhysicalMemory) * 100).ToString("0.00");
            string taotalMsg = $"进程名:{pro.ProcessName},进程占用内存:{zhanYongNeiYun},进程占用Cpu:[进程CPU%] || 电脑总内存:{total}MB,可用内存:{avail}MB，内存占用率:{percent}%,{cupMsg}";
            return taotalMsg;*/

            return "";
        }

#endregion
    }
}
