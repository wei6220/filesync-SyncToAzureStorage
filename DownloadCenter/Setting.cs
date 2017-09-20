using System;
using System.Xml;

namespace DownloadCenterSetting
{
    class Setting
    {
        private static string _responseData = "";
        private static string _syncAzureStorageRegion;
        private static string _logPath, _logFile;
        private static string _scheduleID,_scheduleStartTime, _scheduleFinishTime;
        private static string _updateFileID, _updateFileIDList, _updateFileSize;
        private static string _emailURL, _emailCCList, _emailSubjectContent;
        private static string _mailSyncSuccessLog, _mailSyncErrorLog, _mailSyncRegion;
        private static string _apiFileListURL, _apiFileUpdateURL;
        private static string _targetServerIP, _targetServerLogin, _targetServerPassword;
        private static int _mailSyncTotalLog;
        private static int _syncAzureStorageTotalRegion;

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
                _responseData = element.GetAttribute(getXmlValue);
            }
            catch (Exception e)
            {
                xmlConfigStatus = true;
                _responseData = e.Message;
            }
        }

        public static void SettingScheduleID(string getScheduleID)
        {
            _scheduleID = getScheduleID;
        }

        public static void EmailSyncAzureStorageRegion(string getMailAzureStorageRegion)
        {
            _mailSyncRegion =  getMailAzureStorageRegion;
        }

        public static void GetTotalAzureStorageRegion(int totalRegion)
        {
            _syncAzureStorageTotalRegion = totalRegion;
        }

        public static void GetEachAzureStorageRegion(string getTotalStorageRegion)
        {
            _syncAzureStorageRegion = getTotalStorageRegion;
        }

        public static void GetConfigureSetting()
        {
            _targetServerIP = GetDownloadCenterConfigSetting("DownloadCenterSetting/TargetServer", "Ip");
            _targetServerLogin = GetDownloadCenterConfigSetting("DownloadCenterSetting/TargetServer", "Login");
            _targetServerPassword = GetDownloadCenterConfigSetting("DownloadCenterSetting/TargetServer", "Pwd");          
            _apiFileListURL = GetDownloadCenterConfigSetting("DownloadCenterSetting/FileListApi", "URL");
        }

        public static void GetApiFileUpdateURL()
        {
            _apiFileUpdateURL = GetDownloadCenterConfigSetting("DownloadCenterSetting/UpdateSyncFileListApi", "URL");
        }

        public static void GetEmailSetting()
        {
            _emailURL = GetDownloadCenterConfigSetting("DownloadCenterSetting/Email", "URL");
            _emailCCList = GetDownloadCenterConfigSetting("DownloadCenterSetting/Email/Option", "CC");
            _emailSubjectContent = GetDownloadCenterConfigSetting("DownloadCenterSetting/Email/Subject", "Content");
        }

        public static void GetSyncAzureStorageFile(int countSyncFile)
        {
            _mailSyncTotalLog = countSyncFile;
        }

        public static void EmailTemplateLogSetting(string logMesssage, bool logType)
        {
            if (logType)
            {
                _mailSyncSuccessLog = logMesssage;
            }
            else
            {
                _mailSyncErrorLog = logMesssage;
            }
        }

        public static void UpdateFileIDSetting(string getFileID)
        {
            _updateFileID = getFileID;
        }

        public static void UpdateFileIDListSetting(string getFileIDList)
        {
            _updateFileIDList = getFileIDList;
        }

        public static void UpdateFileSizeSetting(long getFileSize)
        {
            if(getFileSize == 0)
            {
                _updateFileSize = "";
            }
            else
            {
                _updateFileSize = getFileSize.ToString();
            }  
        }

        public static void ScheduleStartTimeSetting(string timeNow)
        {
            _scheduleStartTime = timeNow;
        }

        public static void ScheduleFinishTimeSetting(string timeNow)
        {
            _scheduleFinishTime = timeNow;
        }

        public static void GetDownloadCenterLogSetting()
        {
            _logPath = GetDownloadCenterConfigSetting("DownloadCenterSetting/DownloadCenterLog", "Path");
            _logFile = GetDownloadCenterConfigSetting("DownloadCenterSetting/DownloadCenterLog", "File");
        }

        public struct DownloadCenterXmlSetting
        {
            public static string targetServerIP { get { return _targetServerIP; } }
            public static string targetServerLogin { get { return _targetServerLogin; } }
            public static string targetServerPwd { get { return _targetServerPassword; } }
            public static string logFilePath { get { return _logPath; } }
            public static string logFileName { get { return _logFile; } }        
            public static string syncAzureStorageRegion { get { return _syncAzureStorageRegion; } }
            public static string scheduleStartTime { get { return _scheduleStartTime; } }
            public static string scheduleFinishTime { get { return _scheduleFinishTime; } }
            public static string scheduleID { get { return _scheduleID; } }
            public static string apiFileUpdateURL { get { return _apiFileUpdateURL; } }
            public static string apiFileListURL { get { return _apiFileListURL; } }
            public static string updateFileID { get { return _updateFileID; } }
            public static string updateFileIDList { get { return _updateFileIDList; } }
            public static string updateFileSize { get { return _updateFileSize; } }
            public static string emailURL { get { return _emailURL; } }
            public static string emailCCList { get { return _emailCCList; } }
            public static string emailSubjectContent { get { return _emailSubjectContent; } }
            public static string mailSyncSuccessLog { get { return _mailSyncSuccessLog; } }
            public static string mailSyncErrorLog { get { return _mailSyncErrorLog; } }
            public static string mailSyncRegion { get { return _mailSyncRegion; } }
            public static int mailSyncTotalLog { get { return _mailSyncTotalLog; } }
            public static int syncAzureStorageTotalRegion { get { return _syncAzureStorageTotalRegion; } }
        };

        public static string GetDownloadCenterConfigSetting(string xmlSetting, string xmlValue)
        {

            GetDownloadCenterConfig(xmlSetting, xmlValue);

            if (xmlConfigStatus)
            {
                
            }

            return _responseData;
        }
    }
}
