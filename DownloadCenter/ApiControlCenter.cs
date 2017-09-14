using DownloadCenterAzureStorage;
using DownloadCenterTime;
using DownloadCenterFileApi;
using DownloadCenterLog;
using DownloadCenterSetting;
using DownloadCenterNetCommand;
using DownloadCenterMail;
using DownloadCenterSchedule;
using System;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace DownloadCenter
{
    class ApiControlCenter
    {
        private string apiFileSource, apiFileTarget, targetRenameFile;
        private string syncAzureStoargeData = null;

        static void Main(string[] args)
        {
            Setting.GetConfigureSetting();
            Setting.ScheduleStartTimeSetting(Time.GetTimeNow(Time.TimeFormatType.YearSMonthSDayTimeChange));
            Setting.SettingScheduleID(Time.GetTimeNow(Time.TimeFormatType.YearMonthDayHourMinute));

            dynamic schedule = new Schedule();
            dynamic fileController = new FileApi();
            dynamic apiController = new ApiControlCenter();

            var logFileContent = Log.ReadLog();
            var getScheduleLength = schedule.GetScheduleStatus(logFileContent);
            WriteLogMessage("[Download Center][Success]Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + " Download Center Sync File To Azure Storage Schedule is Start",null);

            apiController.syncAzureStoargeData = fileController.GetFileList().Item1;
            WriteLogMessage(null, fileController.GetFileList().Item2);

            var getAllAzureStorageRegion = fileController.SettingAzureStorageRegion(apiController.syncAzureStoargeData);
            Setting.GetEachAzureStorageRegion(getAllAzureStorageRegion);
            Setting.GetTotalAzureStorageRegion(fileController.GetAzureStorageTotalRegion());

            if (getAllAzureStorageRegion != null)
            {
                var getScheduleProcess = scheduleProcess();

                if (getScheduleProcess)
                {
                    var login = new NetCommand();
                    var loginStatus = login.ExeCommand();

                    if (loginStatus)
                    {
                        GetSyncFileListApi(apiController,fileController);
                    }
                    else
                    {
                        Setting.EmailTemplateLogSetting("<font color = \"#c61919\" size = \"2\" face = \"Verdana, sans-serif\">Not login Target Server " + Setting.DownloadCenterXmlSetting.targetServerIP + "</font>", false);
                    }
                }
                else
                {
                    Setting.EmailTemplateLogSetting("<font color = \"#c61919\" size = \"2\" face = \"Verdana, sans-serif\">Wait for anoher schedule is finish</font>", false);
                }  
            }
            else
            {
                Setting.EmailTemplateLogSetting("<font color = \"#c61919\" size = \"2\" face = \"Verdana, sans-serif\">Not Get Azure Storage Setting</font>", false);
            }

            Setting.ScheduleFinishTimeSetting(Time.GetTimeNow(Time.TimeFormatType.YearSMonthSDayTimeChange));

            var mailSendLog = Mail.sendMail();
            WriteLogMessage(mailSendLog.Item1, mailSendLog.Item2);

            WriteLogMessage("[Download Center][Success]Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + " Download Center Sync File To Azure Storage Schedule Finish",null);
        }

        public static bool scheduleProcess()
        {
            bool exeScheduleStatus = true;
            string getScheduleLogMessage = null, getScheduleLogExceptionMessage = null;
            try
            {
                Process currentProcess = Process.GetCurrentProcess();

                foreach (Process processRsyncSchedule in Process.GetProcessesByName("DownloadCenter"))
                {

                    if (currentProcess.Id != processRsyncSchedule.Id)
                    {
                        exeScheduleStatus = false;
                    }
                }

                if(exeScheduleStatus)
                {
                    getScheduleLogMessage = "[Download Center][Success]Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + " The another DownloadCenter.exe is not execute";
                }
                else
                {
                    getScheduleLogMessage = "[Download Center][Success]Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + " The another DownloadCenter.exe is execute,wait for anoher schedule finish";
                }
            }
            catch(Exception e)
            {
                getScheduleLogExceptionMessage = "[Download Center][Exception]Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + " " + e.Message;
            }

            WriteLogMessage(getScheduleLogMessage, getScheduleLogExceptionMessage);
            return exeScheduleStatus;
        }

        private static void WriteLogMessage(string getResponseMessage, string getExceptionMessage)
        {
            if (getExceptionMessage != null)
            {
                Log.WriteLog(getExceptionMessage);
            }
            else
            {
                if (getResponseMessage != null)
                {
                    Log.WriteLog(getResponseMessage);
                }
            }
        }

        public static void GetSyncFileListApi(dynamic getApiController,dynamic getFileController)
        {
            try
            {
                var fileList = getFileController.GetFileList();
                if (fileList != null)
                {
                    var getFileApiLength = getApiController.ParseFilePath(fileList.Item1);
                    if (getFileApiLength == 0)
                    {
                        Setting.EmailTemplateLogSetting("<font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">No file sync to Azure Storage", false);
                    }
                }
                else
                {
                    Setting.EmailTemplateLogSetting("<font color = \"#c61919\" size = \"2\" face = \"Verdana, sans-serif\">Not Get File List Api", false);
                    WriteLogMessage("[Download Center][Error]Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + "Get File List Api is Null", null);
                }
            }
            catch(Exception e)
            {
                WriteLogMessage(null, "[Download Center][Error]Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + e.Message);
            }
        }
        
        private int ParseFilePath(string getFilesListProperty)
        {
            int fileIndex = 0;
            var blob = new BlobStorage();

            JObject fileList = (JObject)JsonConvert.DeserializeObject(getFilesListProperty);

            var storageSetting = fileList["storages"];
            var syncStorageData = fileList["syncdata"];
            blob.SettingAzureStorageCoonection(storageSetting);

            foreach (var getSyncStorageData in syncStorageData)
            {
                apiFileSource = getSyncStorageData["source"].ToString();
                apiFileTarget = getSyncStorageData["target"].ToString();
                Setting.UpdateFileIDSetting(getSyncStorageData["id"].ToString());

                ComposeSourceFilePathList(ref apiFileSource, true);
                ComposeSourceFilePathList(ref apiFileTarget, false);

                if (File.Exists(apiFileSource))
                {
                    FileInfo file = new FileInfo(apiFileSource);
                    Setting.UpdateFileSizeSetting(file.Length);
                }
                else
                {
                    Setting.UpdateFileSizeSetting(0);
                }

                blob.SyncTargetFileToAzureBlob(apiFileSource, apiFileTarget).Wait();
                Log.WriteLog("[Download Center][Success]Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + " Sync Azure Storage is Finish");
                fileIndex++;
            }
            return fileIndex;
        }

        private void ComposeSourceFilePathList(ref string getApiFileSource, bool sourceFile)
        {
            int targetFilePathHost = 0;
            string[] composeFilePath;

            if (sourceFile)
            {
                composeFilePath = getApiFileSource.Split(new string[] { "\\", "/" }, StringSplitOptions.None);
                getApiFileSource = "\\\\" + Setting.GetDownloadCenterConfigSetting("DownloadCenterSetting/TargetServer", "Ip") + "\\";
            }
            else
            {
                composeFilePath = getApiFileSource.Split(new char[] { '\\' });
                getApiFileSource = "";
            }

            for (int composeFilePathIndex = 0; composeFilePathIndex < composeFilePath.Length; composeFilePathIndex++)
            {
                if (composeFilePathIndex == composeFilePath.Length - 1)
                {
                    targetRenameFile = composeFilePath[composeFilePathIndex];
                    getApiFileSource = getApiFileSource + composeFilePath[composeFilePathIndex];
                }
                else
                {
                    if (composeFilePath[composeFilePathIndex] == "")
                    {
                        continue;
                    }
                    else
                    {
                        if (sourceFile && targetFilePathHost != 1)
                        {
                            targetFilePathHost++;
                        }
                        else
                        {
                            getApiFileSource = getApiFileSource + composeFilePath[composeFilePathIndex] + "\\";

                        }
                    }
                }
            }
        }
    }
}
