using System;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;

namespace DownloadCenter
{
    class ApiControlCenter
    {
        static void Main(string[] args)
        {
            try
            {
                Log.WriteLog("#######################################################################");
                Log.WriteLog("Schedule Start.");
                Setting.SetConfigureSettings();
                Setting.SetScheduleStartTime();
                Setting.SetScheduleID(Time.GetNow(Time.TimeFormatType.YearMonthDayHourMinute));
                SyncResultRecords.Init();

                var isScheduleReady = CheckScheduleReady("DownloadCenter");
                if (isScheduleReady)
                {
                    Thread.Sleep(60000);

                    var api = new FileApi();
                    var syncData = api.GetSyncData();
                    if (syncData.Item1 != null)
                    {
                        Setting.SetAzureStorageRegion(api.GetStorageRegionList());
                        Setting.SetAzureStorageRegionCount(api.GetStorageRegionCount());
                    }

                    if (!string.IsNullOrEmpty(Setting.RuntimeSettings.SyncAzureStorageRegion))
                    {
                        var loginCmd = new NetCommand();
                        var isLogin = loginCmd.ExeCommand();
                        if (isLogin)
                        {
                            
                            SyncData(syncData.Item1);
                            if (SyncResultRecords.All().Count > 0)
                            {
                                //UpdateStatus(SyncResultRecords.All());
                                Setting.SetSyncResultMessage(GenerateResultMessage());
                            }
                            else
                            {
                                Setting.SetSyncResultMessage(
                               "<font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">"
                               + "No file need to be sync </font>");
                                Log.WriteLog("No file need to be sync");
                            }
                        }
                        else
                        {
                            Setting.SetSyncResultMessage(
                                "<font color = \"#c61919\" size = \"2\" face = \"Verdana, sans-serif\">"
                                + "Not login Target Server " + Setting.Config.TargetServerIP + "</font>");
                            Log.WriteLog("Not login target server " + Setting.Config.TargetServerIP, Log.Type.Failed);
                        }
                    }
                    else
                    {
                        Setting.SetSyncResultMessage(
                            "<font color = \"#c61919\" size = \"2\" face = \"Verdana, sans-serif\">"
                            + "Not Get Azure Storage Setting</font>");
                        Log.WriteLog("Not get azure storage setting", Log.Type.Failed);
                    }
                }
                else
                {
                    Setting.SetSyncResultMessage(
                        "<font color = \"#c61919\" size = \"2\" face = \"Verdana, sans-serif\">"
                        + "Wait for anoher schedule is finish</font>");
                    Log.WriteLog("Wait for anoher schedule is finish", Log.Type.Failed);
                }

                Setting.SetScheduleFinishTime();
                var mail = new Mail();
                mail.sendMail();
                Log.WriteLog("Schedule Finish.");
            }
            catch (Exception e)
            {
                Log.WriteLog(e.Message, Log.Type.Exception);
            }
        }

        private static bool CheckScheduleReady(string processName)
        {
            bool isReady = true;
            try
            {
                Process currentProcess = Process.GetCurrentProcess();
                foreach (Process processRsyncSchedule in Process.GetProcessesByName(processName))
                {
                    if (currentProcess.Id != processRsyncSchedule.Id)
                        isReady = false;
                }

                if (!isReady)
                    Log.WriteLog("The another DownloadCenter.exe is execute, wait for anoher schedule finish");
            }
            catch (Exception e)
            {
                Log.WriteLog(e.Message, Log.Type.Exception);
            }
            return isReady;
        }

        private static void SyncData(JObject syncData)
        {
            var storages = syncData["storages"];
            var syncdata = syncData["syncdata"];

            var blob = new StorageService(storages);
            string id, size, sourcePath, targetPath, errorMessage;
            SyncResultRecords.SyncResult resultRecord;
            int SyncToAzureSuccessCount = 0;
            foreach (var syncInfo in syncdata)
            {
                errorMessage = "";
                size = "";
                id = syncInfo["id"].ToString();
                sourcePath = syncInfo["source"].ToString();
                targetPath = syncInfo["target"].ToString();
                sourcePath = ComposeFilePath(sourcePath, true);
                targetPath = ComposeFilePath(targetPath, false);
                Log.WriteLog("Start Sync to Azure Storage.(id:" + id + ")");
                Log.WriteLog(sourcePath + " -> " + targetPath);

                if (!File.Exists(sourcePath))
                {
                    Log.WriteLog("No such file or directory.", Log.Type.Failed);
                    resultRecord = new SyncResultRecords.SyncResult
                    {
                        Id = id,
                        Size = size,
                        FinishTime = Time.GetNow(Time.TimeFormatType.YearSMonthSDayTimeChange),
                        SourcePath = sourcePath,
                        TargetPath = targetPath,
                        Status = "Failed",
                        Message = "No such file or directory.",
                    };
                    errorMessage = UpdateStatus(new List<SyncResultRecords.SyncResult> { resultRecord });
                }
                else
                {
                    try
                    {
                        FileInfo file = new FileInfo(sourcePath);
                        size = file.Length.ToString();
                        blob.SyncFileToAzureBlob(sourcePath, targetPath).Wait();
                        SyncToAzureSuccessCount++;
                        resultRecord = new SyncResultRecords.SyncResult
                        {
                            Id = id,
                            Size = size,
                            FinishTime = Time.GetNow(Time.TimeFormatType.YearSMonthSDayTimeChange),
                            SourcePath = sourcePath,
                            TargetPath = targetPath,
                            Status = "Success",
                            Message = "Sync to azure storage is success.",
                        };
                        errorMessage = UpdateStatus(new List<SyncResultRecords.SyncResult> { resultRecord });
                    }
                    catch (Exception e)
                    {
                        Log.WriteLog(e.Message, Log.Type.Exception);
                        resultRecord = new SyncResultRecords.SyncResult
                        {
                            Id = id,
                            Size = size,
                            FinishTime = Time.GetNow(Time.TimeFormatType.YearSMonthSDayTimeChange),
                            SourcePath = sourcePath,
                            TargetPath = targetPath,
                            Status = "Exception",
                            Message = "Sync to azure storage is failed.",
                        };
                        errorMessage = UpdateStatus(new List<SyncResultRecords.SyncResult> { resultRecord });
                    }
                }

                if (errorMessage != "")
                {
                    resultRecord.Status = "Failed";
                    resultRecord.Message += (resultRecord.Message != "" ? " " : "") + errorMessage;
                }
                SyncResultRecords.Add(resultRecord);
                Log.WriteLog("Sync to szure storage is finish.");
            }
            Setting.SetSyncFileTotalCount(SyncToAzureSuccessCount);
        }

        private static string UpdateStatus(List<SyncResultRecords.SyncResult> syncResultList)
        {
            var errorMsg = "";
            var updateList = new List<FileApi.UpdateInfo>();
            syncResultList.ForEach(e =>
            {
                updateList.Add(new FileApi.UpdateInfo
                {
                    id = e.Id,
                    size = e.Size,
                    status = (e.Status.ToLower() == "failed" || e.Status.ToLower() == "exception") ? "error" : "success",
                    message = e.Message,
                });
            });

            if (updateList.Count > 0)
            {
                FileApi api = new FileApi();
                errorMsg = api.UpdateSyncStatus(updateList);
            }
            return errorMsg;
        }

        private static string GenerateResultMessage()
        {
            string message = "";
            SyncResultRecords.All().ForEach(e =>
            {
                if (e.Status.ToLower() == "failed" || e.Status.ToLower() == "exception")
                {
                    message += "<tr><td bgcolor = \"#f28c9b\" width = \"12%\"><font size = \"2\" face = \"Verdana, sans-serif\">" + e.FinishTime + "</font></td>"
                          + "<td width = \"55%\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">" + e.SourcePath.Replace("\\", "/")
                          + "</font></td><td width = \"30%\" ><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\"> " + e.Message + "</font></td></tr>";
                }
                else
                {
                    message += "<tr><td width = \"22%\" bgcolor = \"#EEF4FD\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">"
                    + e.FinishTime + "</font></td><td width = \"55%\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">"
                    + e.SourcePath.Replace("\\", "/") + "</font></td><td width = \"30%\" ><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">already Copy to "
                    + "Storage " + e.TargetPath.Replace("\\", "/") + "</font></td></tr>";
                }
            });
            return message;
        }

        private static string ComposeFilePath(string originPath, bool isSourceFile)
        {
            string[] pathChunks;
            pathChunks = (isSourceFile) ? originPath.Split(new string[] { "\\", "/" }, StringSplitOptions.None) :
                 originPath.Split(new char[] { '\\' });

            string newPath = "";
            bool isFirstRead = false;
            for (int index = 0; index < pathChunks.Length; index++)
            {
                if (string.IsNullOrEmpty(pathChunks[index]))
                    continue;

                if (isSourceFile && !isFirstRead)
                {
                    newPath += "\\\\" + Setting.Config.TargetServerIP;
                    isFirstRead = true;
                    continue;
                }

                if (newPath != "")
                    newPath += "\\";
                newPath += pathChunks[index];
            }

            return newPath;
        }

    }
}
