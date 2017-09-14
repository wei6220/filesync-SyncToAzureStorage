using System;
using System.Xml;

namespace DownloadCenterSetting
{
    class Setting
    {
        private static string responseData = "";
        private static string __azureStorageAccount, __azureStorageKey, __azureStorageContainer, __azureStorageRegion;
        private static string __syncAzureStorageRegion;
        private static string __logPath, __logFile;
        private static string __scheduleID,__scheduleStartTime, __scheduleFinishTime;
        private static string __updateFileID, __updateFileIDList, __updateFileSize;
        private static string __emailURL, __emailCCList, __emailSubjectContent;
        private static string __mailSyncSuccessLog, __mailSyncErrorLog, __mailSyncRegion;
        private static string __apiFileListURL, __apiFileUpdateURL;
        private static string __targetServerIP, __targetServerLogin, __targetServerPassword;
        private static int __mailSyncTotalLog;
        private static int __syncAzureStorageTotalRegion;

        private static bool xmlConfigStatus;
        private static XmlDocument doc;

        private static void GetDownloadCenterConfig(string getXmlSetting, string getXmlValue)
        {
            try
            {
                doc = new XmlDocument();
                doc.Load("DownloadCenter.xml");
                
                XmlNode main = doc.SelectSingleNode(getXmlSetting);
                XmlElement element = (XmlElement)main;
                responseData = element.GetAttribute(getXmlValue);
            }
            catch (Exception e)
            {
                xmlConfigStatus = true;
                responseData = e.Message;
            }
        }

        public static void SettingScheduleID(string getScheduleID)
        {
            __scheduleID = getScheduleID;
        }

        public static void EmailSyncAzureStorageRegion(string getMailAzureStorageRegion)
        {
            __mailSyncRegion =  getMailAzureStorageRegion;
        }

        public static void GetTotalAzureStorageRegion(int totalRegion)
        {
            __syncAzureStorageTotalRegion = totalRegion;
        }

        public static void GetEachAzureStorageRegion(string getTotalStorageRegion)
        {
            __syncAzureStorageRegion = getTotalStorageRegion;
        }

        public static void GetConfigureSetting()
        {
            __targetServerIP = GetDownloadCenterConfigSetting("DownloadCenterSetting/TargetServer", "Ip");
            __targetServerLogin = GetDownloadCenterConfigSetting("DownloadCenterSetting/TargetServer", "Login");
            __targetServerPassword = GetDownloadCenterConfigSetting("DownloadCenterSetting/TargetServer", "Pwd");          
            __apiFileListURL = GetDownloadCenterConfigSetting("DownloadCenterSetting/FileListApi", "URL");
        }

        public static void GetApiFileUpdateURL()
        {
            __apiFileUpdateURL = GetDownloadCenterConfigSetting("DownloadCenterSetting/UpdateSyncFileListApi", "URL");
        }

        public static void GetEmailSetting()
        {
            __emailURL = GetDownloadCenterConfigSetting("DownloadCenterSetting/Email", "URL");
            __emailCCList = GetDownloadCenterConfigSetting("DownloadCenterSetting/Email/Option", "CC");
            __emailSubjectContent = GetDownloadCenterConfigSetting("DownloadCenterSetting/Email/Subject", "Content");
        }

        public static void GetSyncAzureStorageFile(int countSyncFile)
        {
            __mailSyncTotalLog = countSyncFile;
        }

        public static void EmailTemplateLogSetting(string logMesssage, bool logType)
        {
            if (logType)
            {
                __mailSyncSuccessLog = logMesssage;
            }
            else
            {
                __mailSyncErrorLog = logMesssage;
            }
        }

        public static void UpdateFileIDSetting(string getFileID)
        {
            __updateFileID = getFileID;
        }

        public static void UpdateFileIDListSetting(string getFileIDList)
        {
            __updateFileIDList = getFileIDList;
        }

        public static void UpdateFileSizeSetting(long getFileSize)
        {
            if(getFileSize == 0)
            {
                __updateFileSize = "";
            }
            else
            {
                __updateFileSize = getFileSize.ToString();
            }  
        }

        public static void ScheduleStartTimeSetting(string timeNow)
        {
            __scheduleStartTime = timeNow;
        }

        public static void ScheduleFinishTimeSetting(string timeNow)
        {
            __scheduleFinishTime = timeNow;
        }

        public static void GetDownloadCenterLogSetting()
        {
            __logPath = GetDownloadCenterConfigSetting("DownloadCenterSetting/DownloadCenterLog", "Path");
            __logFile = GetDownloadCenterConfigSetting("DownloadCenterSetting/DownloadCenterLog", "File");
        }

        public struct DownloadCenterXmlSetting
        {
            public static string targetServerIP { get { return __targetServerIP; } }
            public static string targetServerLogin { get { return __targetServerLogin; } }
            public static string targetServerPwd { get { return __targetServerPassword; } }
            public static string logFilePath { get { return __logPath; } }
            public static string logFileName { get { return __logFile; } }        
            public static string syncAzureStorageRegion { get { return __syncAzureStorageRegion; } }
            public static string scheduleStartTime { get { return __scheduleStartTime; } }
            public static string scheduleFinishTime { get { return __scheduleFinishTime; } }
            public static string scheduleID { get { return __scheduleID; } }
            public static string apiFileUpdateURL { get { return __apiFileUpdateURL; } }
            public static string apiFileListURL { get { return __apiFileListURL; } }
            public static string updateFileID { get { return __updateFileID; } }
            public static string updateFileIDList { get { return __updateFileIDList; } }
            public static string updateFileSize { get { return __updateFileSize; } }
            public static string emailURL { get { return __emailURL; } }
            public static string emailCCList { get { return __emailCCList; } }
            public static string emailSubjectContent { get { return __emailSubjectContent; } }
            public static string mailSyncSuccessLog { get { return __mailSyncSuccessLog; } }
            public static string mailSyncErrorLog { get { return __mailSyncErrorLog; } }
            public static string mailSyncRegion { get { return __mailSyncRegion; } }
            public static int mailSyncTotalLog { get { return __mailSyncTotalLog; } }
            public static int syncAzureStorageTotalRegion { get { return __syncAzureStorageTotalRegion; } }
        };

        public static string GetDownloadCenterConfigSetting(string xmlSetting, string xmlValue)
        {

            GetDownloadCenterConfig(xmlSetting, xmlValue);

            if (xmlConfigStatus)
            {
                //RsyncDateTime.WriteLog("[Download Center][Exception]" + xmlData);
            }

            return responseData;
        }
    }
}
