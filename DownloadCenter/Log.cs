using DownloadCenterFolder;
using DownloadCenterSetting;
using DownloadCenterTime;
using System;
using System.IO;

namespace DownloadCenterLog
{
    class Log
    {
        private static string DownloadCenterWriteLogPath, DownloadCenterWriteLogFile, DownloadCenterLogFolder, DownloadCenterFileLogTimeNow, DownloadCenterWriteLogTimeNow;
        private static object _lockWrite = new object();

        private static void GetDownloadCenterLog()
        {
            Setting.GetDownloadCenterLogSetting();
            DownloadCenterWriteLogPath = Setting.DownloadCenterXmlSetting.logFilePath;
            DownloadCenterWriteLogFile = Setting.DownloadCenterXmlSetting.logFileName;
        }

        private static void GetDownloadCenterLogTime()
        {

            DownloadCenterWriteLogTimeNow = Time.GetTimeNow();
            DownloadCenterFileLogTimeNow = Time.GetTimeNow(Time.TimeFormatType.YearMonthDay);
            DownloadCenterLogFolder = Time.GetTimeNow(Time.TimeFormatType.YearMonth);
        }

        public static void WriteLog(string getLogMessage)
        {
            GetDownloadCenterLog();    
            GetDownloadCenterLogTime();

            ConfirmExistLogFolder();
            CommonLibrary.LogHelper.Write(getLogMessage, DownloadCenterWriteLogTimeNow + "_" + DownloadCenterWriteLogFile, DownloadCenterWriteLogPath + DownloadCenterLogFolder + "_DownloadCenter\\");
        }

        private static void ConfirmExistLogFolder()
        {
            dynamic folder = new Folder();
            folder.CreateFolder(DownloadCenterWriteLogPath + DownloadCenterLogFolder + "_DownloadCenter");
        }

        public static string[] ReadLog()
        {
            string logPath;
            string[] logFile = null;
            try
            {
                lock (_lockWrite)
                {
                    GetDownloadCenterLog();
                    GetDownloadCenterLogTime();
                    logPath = DownloadCenterWriteLogPath + DownloadCenterLogFolder + "_DownloadCenter\\" + DownloadCenterWriteLogTimeNow + "_" + DownloadCenterWriteLogFile;
                    if (File.Exists(logPath))
                    {
                        using (FileStream file = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            logFile = File.ReadAllLines(logPath);
                            file.Close();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return logFile;
        }
    }
}
