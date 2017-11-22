using System;
using System.Collections.Generic;
using System.Xml;

namespace DownloadCenter
{
    class Setting
    {
        #region // ini settings
        private static string _logPath,_emailURL, _emailCCList, _emailSubjectContent,
            _apiFileListURL, _apiFileUpdateURL, _targetServerIP, _targetServerLogin, _targetServerPwd;
        private static XmlDocument _configDoc;
        public static void LoadConfigDoc()
        {
            _configDoc = new XmlDocument();
            _configDoc.Load("Setting.xml");
        }
        public static string GetSettingDocAttrValue(string nodePath, string attrKey)
        {
            string value = "";
            try
            {
                if (_configDoc == null)
                    LoadConfigDoc();
                XmlElement element = (XmlElement)_configDoc.SelectSingleNode(nodePath);
                value = element.GetAttribute(attrKey);
            }
            catch (Exception e)
            {
                //value = e.Message;
                //Console.WriteLine(e.Message);
                Log.WriteLog(e.Message, Log.Type.Exception);
            }
            return value;
        }
        public static void SetConfigureSettings()
        {
            SetTargetServerSettings();
            SetFileListApiSettings();
            SetUpdateSyncFileListApiSettings();
            SetEmailSettings();
            SetDownloadCenterLogSettings();
        }
        public static void SetTargetServerSettings() {
            _targetServerIP = GetSettingDocAttrValue("DownloadCenterSetting/TargetServer", "Ip");
            _targetServerLogin = GetSettingDocAttrValue("DownloadCenterSetting/TargetServer", "Login");
            _targetServerPwd = GetSettingDocAttrValue("DownloadCenterSetting/TargetServer", "Pwd");
        }
        public static void SetFileListApiSettings()
        {
            _apiFileListURL = GetSettingDocAttrValue("DownloadCenterSetting/FileListApi", "URL");
        }
        public static void SetUpdateSyncFileListApiSettings()
        {
            _apiFileUpdateURL = GetSettingDocAttrValue("DownloadCenterSetting/UpdateSyncFileListApi", "URL");
        }
        public static void SetEmailSettings()
        {
            _emailURL = GetSettingDocAttrValue("DownloadCenterSetting/Email", "URL");
            _emailCCList = GetSettingDocAttrValue("DownloadCenterSetting/Email/Option", "CC");
            _emailSubjectContent = GetSettingDocAttrValue("DownloadCenterSetting/Email/Subject", "Content");
        }
        public static void SetDownloadCenterLogSettings()
        {
            _logPath = GetSettingDocAttrValue("DownloadCenterSetting/DownloadCenterLog", "Path");
        }
        public struct Config
        {
            public static string TargetServerIP { get { return _targetServerIP; } }
            public static string TargetServerLogin { get { return _targetServerLogin; } }
            public static string TargetServerPwd { get { return _targetServerPwd; } }
            public static string LogFilePath { get { return _logPath; } }
            public static string ApiFileUpdateURL { get { return _apiFileUpdateURL; } }
            public static string ApiFileListURL { get { return _apiFileListURL; } }
            public static string EmailURL { get { return _emailURL; } }
            public static string EmailCCList { get { return _emailCCList; } }
            public static string EmailSubjectContent { get { return _emailSubjectContent; } }
        }
        #endregion

        #region // runtime var
        private static string _scheduleID, _scheduleStartTime, _scheduleFinishTime, _syncResultMessage, _syncAzureStorageRegion;
        private static int _syncFileTotalCount, _syncAzureStorageRegionCount;
        public static void SetScheduleID(string id)
        {
            _scheduleID = id;
        }
        public static void SetScheduleStartTime()
        {
            _scheduleStartTime = Time.GetNow(Time.TimeFormatType.YearSMonthSDayTimeChange);
        }
        public static void SetScheduleFinishTime()
        {
            _scheduleFinishTime = Time.GetNow(Time.TimeFormatType.YearSMonthSDayTimeChange);
        }
        public static void SetAzureStorageRegionCount(int regionCnt)
        {
            _syncAzureStorageRegionCount = regionCnt;
        }
        public static void SetAzureStorageRegion(string regionList)
        {
            _syncAzureStorageRegion = regionList;
        }
        public static void SetSyncFileTotalCount(int count)
        {
            _syncFileTotalCount = count;
        }
        public static void SetSyncResultMessage(string message)
        {
            _syncResultMessage = message;
        }
        public struct RuntimeSettings
        {
            public static string ScheduleID { get { return _scheduleID; } }
            public static string ScheduleStartTime { get { return _scheduleStartTime; } }
            public static string ScheduleFinishTime { get { return _scheduleFinishTime; } }
            public static string SyncResultMessage { get { return _syncResultMessage; } }
            public static string SyncAzureStorageRegion { get { return _syncAzureStorageRegion; } }
            public static int SyncAzureStorageRegionCount { get { return _syncAzureStorageRegionCount; } }
            public static int SyncFileTotalCount { get { return _syncFileTotalCount; } }
        }
        #endregion

        #region // 紀錄處理物件
        private static List<SyncResult> _SyncResultList;
        public static List<SyncResult> GetSyncResultList()
        {
            return _SyncResultList;
        }
        public static void InitSyncResultList()
        {
            _SyncResultList = null;
            _SyncResultList = new List<SyncResult>();
        }
        public static void AddSyncResultList(SyncResult obj)
        {
            if (_SyncResultList == null)
            {
                InitSyncResultList();
            }
            _SyncResultList.Add(obj);
        }
        public struct SyncResult
        {
            public string Id { get; set; }
            public string Size { get; set; }
            public string Status { get; set; }
            public string Message { get; set; }
            public string FinishTime { get; set; }
            public string SourcePath { get; set; }
            public string TargetPath { get; set; }
        }
        #endregion
    }
}
