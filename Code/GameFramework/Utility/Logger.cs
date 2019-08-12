using System;
using System.Text;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System.Net;
using System.Diagnostics;

namespace Game.Utility
{
    public class LogHelper
    {
        private enum E_LOG_TYPE
        {
            FATAL,
            ERROR,
            INFO,
            DEBUG
        }

        private const string m_ConversionPattern = "%d{yyyy-MM-dd HH:mm:ss} %-5level %message%newline";

        private static ILog m_logger;

        private static int m_FrameLevelCount = 8;

        private static bool m_IsFileAppender = false;

        public static int FrameLevelCount
        {
            get
            {
                return LogHelper.m_FrameLevelCount;
            }
            set
            {
                LogHelper.FrameLevelCount = value;
            }
        }

        static LogHelper() {
            CreateConsoleAppender();
        }

        public static void CreateConsoleAppender()
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Name = "Console";
            ConsoleAppender consoleAppender = new ConsoleAppender();
            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = m_ConversionPattern;
            patternLayout.ActivateOptions();
            consoleAppender.Layout = patternLayout;
            hierarchy.Root.AddAppender(consoleAppender);
            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;
            LogHelper.m_logger = LogManager.GetLogger("Console");
        }

        public static void CreateUdpAppender(string ipAddress, int nPort)
        {
            LogHelper.m_IsFileAppender = false;
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Name = "IggUdpRunTime";
            UdpAppender udpAppender = new UdpAppender();
            udpAppender.RemotePort = nPort;
            IPAddress remoteAddress = IPAddress.Parse(ipAddress);
            udpAppender.RemoteAddress = remoteAddress;
            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%d{yyyy-MM-dd HH:mm:ss} %-5level %message%newline";
            patternLayout.ActivateOptions();
            udpAppender.Layout = patternLayout;
            udpAppender.Encoding = Encoding.UTF8;
            udpAppender.ActivateOptions();
            hierarchy.Root.AddAppender(udpAppender);
            hierarchy.Root.Level = Level.Fatal;
            hierarchy.Configured = true;
            LogHelper.m_logger = LogManager.GetLogger("IggUdpRunTime");
        }

        public static void CreateFileAppender(string WorkDir)
        {
            LogHelper.m_IsFileAppender = true;
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Name = "IggRunTime";
            RollingFileAppender rollingFileAppender = new RollingFileAppender();
            rollingFileAppender.AppendToFile = true;
            rollingFileAppender.DatePattern = "yyyy-MM-dd HH";
            rollingFileAppender.File = WorkDir + "\\Runtime.log";
            rollingFileAppender.RollingStyle = RollingFileAppender.RollingMode.Size;
            rollingFileAppender.MaximumFileSize = "10M";
            rollingFileAppender.MaxSizeRollBackups = 10;
            rollingFileAppender.StaticLogFileName = true;
            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%d{yyyy-MM-dd HH:mm:ss} %-5level %message%newline";
            patternLayout.ActivateOptions();
            rollingFileAppender.Layout = patternLayout;
            rollingFileAppender.Encoding = Encoding.UTF8;
            rollingFileAppender.ActivateOptions();
            hierarchy.Root.AddAppender(rollingFileAppender);
            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;
            LogHelper.m_logger = LogManager.GetLogger("IggRunTime");
        }

        public static void LogFatalErr(string format, params object[] args)
        {
            LogHelper.WriteLog(LogHelper.E_LOG_TYPE.FATAL, format, args);
        }

        public static void LogFatalErr(string message)
        {
            LogHelper.WriteLog(LogHelper.E_LOG_TYPE.FATAL, message);
        }

        public static void LogErr(string format, params object[] args)
        {
            LogHelper.WriteLog(LogHelper.E_LOG_TYPE.ERROR, format, args);
        }

        public static void LogErr(string message)
        {
            LogHelper.WriteLog(LogHelper.E_LOG_TYPE.ERROR, message);
        }

        public static void LogInfo(string format, params object[] args)
        {
            LogHelper.WriteLog(LogHelper.E_LOG_TYPE.INFO, format, args);
        }

        public static void LogInfo(string message)
        {
            LogHelper.WriteLog(LogHelper.E_LOG_TYPE.INFO, message);
        }

        public static void LogDebug(string format, params object[] args)
        {
            LogHelper.WriteLog(LogHelper.E_LOG_TYPE.DEBUG, format, args);
        }

        public static void LogDebug(string message)
        {
            LogHelper.WriteLog(LogHelper.E_LOG_TYPE.DEBUG, message);
        }

        private static string BuildStack()
        {
            StackTrace stackTrace = new StackTrace(true);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("[");
            for (int i = 3; i < LogHelper.FrameLevelCount; i++)
            {
                StackFrame frame = stackTrace.GetFrame(i);
                if (frame != null && frame.GetFileLineNumber() != 0)
                {
                    stringBuilder.Append(frame.GetFileName());
                    stringBuilder.AppendFormat(" {0} ", frame.GetFileLineNumber());
                }
            }
            stringBuilder.Append("]");
            return stringBuilder.ToString();
        }

        private static void WriteLog(LogHelper.E_LOG_TYPE eLogType, string format, params object[] args)
        {
            if (!string.IsNullOrEmpty(format) && args != null)
            {
                if (LogHelper.m_logger != null)
                {
                    if (LogHelper.E_LOG_TYPE.FATAL == eLogType)
                    {
                        string text = LogHelper.BuildStack();
                        text += format;
                        LogHelper.m_logger.FatalFormat(text, args);
                    }
                    else if (LogHelper.E_LOG_TYPE.ERROR == eLogType)
                    {
                        if (LogHelper.m_IsFileAppender)
                        {
                            string text = LogHelper.BuildStack();
                            text += format;
                            LogHelper.m_logger.ErrorFormat(text, args);
                        }
                        else
                        {
                            LogHelper.m_logger.ErrorFormat(format, args);
                        }
                    }
                    else if (LogHelper.E_LOG_TYPE.DEBUG == eLogType)
                    {
                        LogHelper.m_logger.DebugFormat(format, args);
                    }
                    else if (LogHelper.E_LOG_TYPE.INFO == eLogType)
                    {
                        LogHelper.m_logger.InfoFormat(format, args);
                    }
                }
            }
        }

        private static void WriteLog(LogHelper.E_LOG_TYPE eLogType, string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                if (LogHelper.m_logger != null)
                {
                    if (LogHelper.E_LOG_TYPE.FATAL == eLogType)
                    {
                        string text = LogHelper.BuildStack();
                        text += message;
                        LogHelper.m_logger.Fatal(text);
                    }
                    else if (LogHelper.E_LOG_TYPE.ERROR == eLogType)
                    {
                        if (LogHelper.m_IsFileAppender)
                        {
                            string text = LogHelper.BuildStack();
                            text += message;
                            LogHelper.m_logger.Error(text);
                        }
                        else
                        {
                            LogHelper.m_logger.Error(message);
                        }
                    }
                    else if (LogHelper.E_LOG_TYPE.DEBUG == eLogType)
                    {
                        LogHelper.m_logger.Debug(message);
                    }
                    else if (LogHelper.E_LOG_TYPE.INFO == eLogType)
                    {
                        LogHelper.m_logger.Info(message);
                    }
                }
            }
        }
    }
}
