using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Edu.CommonNLogLibCore
{
    /// <summary>
    /// 对NLog的二次封装
    /// <para>author:wangyulong</para>
    /// </summary>
    public static class LogHelper
    {
        #region 属性

        private static string _baseDir;
        private static string _logDir;
        private static Logger Logger;

        #endregion

        #region 初始化

        public static void Init(string baseDir, string logDir)
        {
            _baseDir = baseDir;
            _logDir = logDir;

            DftInit();
        }

        static void DftInit()
        {
            try
            {
                string fileName = _baseDir + "/logs/${shortdate}.log";
                string tracefileName = _baseDir + "/logs/${shortdate}_Trace.log";
                string offfileName = _baseDir + "/logs/${shortdate}_Final.log";

                var logDir = _logDir;
                bool isNewLogDirSuccess = true;
                try
                {
                    if (!Directory.Exists(logDir))
                        Directory.CreateDirectory(logDir);
                }
                catch (Exception ex)
                {
                    isNewLogDirSuccess = false;
                }
                if (isNewLogDirSuccess)
                {
                    if (!string.IsNullOrEmpty(logDir))
                    {
                        fileName = logDir + "/logs/${shortdate}.log";
                        tracefileName = logDir + "/logs/${shortdate}_Trace.log";
                        offfileName = logDir + "/logs/${shortdate}_Final.log";
                    }
                }

                var config = new NLog.Config.LoggingConfiguration();

                // Targets where to log to: File and Console
                var logfile = new NLog.Targets.FileTarget("logfile")
                {
                    FileName = fileName,
                    Layout = "${longdate} ${uppercase:${level}} ${message}",
                    Header = "----------------------header--------------------------",
                    Footer = "----------------------footer--------------------------",
                    Encoding = Encoding.UTF8
                };
                // Targets where to log to: File and Console
                var traceLog = new NLog.Targets.FileTarget("logfileTrace")
                {
                    FileName = tracefileName,
                    Layout = "${longdate} ${uppercase:${level}} ${message}",
                    Header = "----------------------header--------------------------",
                    Footer = "----------------------footer--------------------------",
                    Encoding = Encoding.UTF8
                };

                // Targets where to log to: File and Console
                var fatalLog = new NLog.Targets.FileTarget("logfileTrace3")
                {
                    FileName = offfileName,
                    Layout = "${longdate} ${uppercase:${level}} ${message}",
                    Header = "----------------------header--------------------------",
                    Footer = "----------------------footer--------------------------",
                    Encoding = Encoding.UTF8
                };



                var logconsole = new NLog.Targets.ConsoleTarget("logconsole") { Layout = "${longdate} ${uppercase:${level}} ${message}" };

                // Rules for mapping loggers to targets            
                config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);
                config.AddRule(LogLevel.Info, LogLevel.Error, logfile);
                config.AddRule(LogLevel.Trace, LogLevel.Trace, traceLog);
                config.AddRule(LogLevel.Fatal, LogLevel.Fatal, fatalLog);

                // Apply config           
                LogManager.Configuration = config;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Logger = LogManager.GetCurrentClassLogger();
        }


        #endregion

        public static void LogException(this Exception exception, string message = null, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0, [CallerMemberName] string methodName = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                Logger.Log(LogLevel.Error, exception);
            }
            else
            {
                string msg = $"[{Path.GetFileName(path)}] [{line:d5}] [{methodName}]: {message}";
                Logger.Log(LogLevel.Error, exception, msg);
            }
        }

        public static void LogDebug(string message, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0, [CallerMemberName] string methodName = "")
        {
            string msg = $"[{Path.GetFileName(path)}] [{line:d5}] [{methodName}]: {message}";
            Logger.Log(LogLevel.Debug, msg);
        }

        public static void LogInfo(string message, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0, [CallerMemberName] string methodName = "")
        {
            string msg = $"[{Path.GetFileName(path)}] [{line:d5}] [{methodName}]: {message}";
            Logger.Log(LogLevel.Info, msg);
        }

        public static void LogError(string message, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0, [CallerMemberName] string methodName = "")
        {
            string msg = $"[{Path.GetFileName(path)}] [{line:d5}] [{methodName}]: {message}";
            Logger.Log(LogLevel.Error, msg);
        }

        public static void Error(string message, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0, [CallerMemberName] string methodName = "")
        {
            string msg = $"[{Path.GetFileName(path)}] [{line:d5}] [{methodName}]: {message}";
            Logger.Log(LogLevel.Error, msg);
        }


        public static void Error(string message, string className, string methodName, [CallerLineNumber] int line = 0)
        {
            LogError(message, className, line, methodName);
        }


        public static void Info(string message, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0, [CallerMemberName] string methodName = "")
        {
            string msg = $"[{Path.GetFileName(path)}] [{line:d5}] [{methodName}]: {message}";
            Logger.Log(LogLevel.Info, msg);
        }

        public static void Info(string message, string className, string methodName)
        {
            LogInfo(message);
        }

        /// <summary>
        /// 记录Traces级别日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="path"></param>
        /// <param name="line"></param>
        /// <param name="methodName"></param>
        public static void Traces(string message, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0, [CallerMemberName] string methodName = "")
        {
            string msg = $"[{Path.GetFileName(path)}] [{line:d5}] [{methodName}]: {message}";
            Logger.Log(LogLevel.Trace, msg);
        }


        /// <summary>
        /// 记录Off级别日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="path"></param>
        /// <param name="line"></param>
        /// <param name="methodName"></param>
        public static void LogFatal(string message, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0, [CallerMemberName] string methodName = "")
        {
            string msg = $"[{Path.GetFileName(path)}] [{line:d5}] [{methodName}]: {message}";
            Logger.Log(LogLevel.Fatal, msg);
        }

    }
}
