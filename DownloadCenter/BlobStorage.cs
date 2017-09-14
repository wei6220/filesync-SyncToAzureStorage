using DownloadCenterFileApi;
using DownloadCenterLog;
using DownloadCenterSetting;
using DownloadCenterTime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace DownloadCenterAzureStorage
{
    class BlobStorage
    {
        private string storageRegion = "", storageContainer = "", storageConnection = "";
        private string syncSourcePath, syncRenameFilePath;
        private string syncSuccessStartTime, syncErrorStartTime;
        private string mailSyncSuccess, mailSyncError;
        private int countSyncAzureStorageError, countSyncAzureStorageSuccess, countSyncAzureStorageLog, countSyncAzureStorageFile, countSyncAzureStorageExist;
        private bool sourcePathError;
        private List<BlobStorage> responseFileList;
        private JToken azuerStorageConnection;
        dynamic fileUpdate;
        
        public string id { get; set; }
        public string size { get; set; }

        public void  SettingAzureStorageCoonection(JToken getAzureStorageConnection)
        {
            azuerStorageConnection = getAzureStorageConnection;
        }

        public CloudStorageAccount AzureStorageConnection()
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(storageConnection);
            return account;
        }

        public async Task SyncTargetFileToAzureBlob(string getSourceFilePath, string getTargetRenameFilePath)
        {

            syncSourcePath = getSourceFilePath;
            syncRenameFilePath = getTargetRenameFilePath;

            var TaskList = new List<Task>();
            var getSyncStorageData = azuerStorageConnection;
            Log.WriteLog("[Download Center][Success]Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + " Sync Azure Storage is Start");
            Log.WriteLog("[Download Center][Success]Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + " " + syncSourcePath + "  --------------> " + "FileID:" + Setting.DownloadCenterXmlSetting.updateFileID + "," + "FileSize:" + Setting.DownloadCenterXmlSetting.updateFileSize);


            foreach (var storageSetting in getSyncStorageData)
            {
                storageConnection = storageSetting["connectstring"].ToString();
                storageRegion = storageSetting["region"].ToString();
                storageContainer = storageSetting["container"].ToString();

                var task = TransferLocalFileToAzureBlob(AzureStorageConnection());
                TaskList.Add(task);
                SingleSyncAzureStorageEmailLog();
            }
            await Task.WhenAll(TaskList.ToArray());
            sourcePathError = true;
        }

        private void SingleSyncAzureStorageEmailLog()
        {
            if (countSyncAzureStorageError == Setting.DownloadCenterXmlSetting.syncAzureStorageTotalRegion)
            {
                mailSyncError = mailSyncError + "<tr><td bgcolor = \"#f28c9b\" width = \"12%\"><font size = \"2\" face = \"Verdana, sans-serif\">" + syncErrorStartTime + "</font></td>"
                                              + "<td width = \"55%\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">" + syncSourcePath.Replace("\\", "/") + "</font></td><td width = \"30%\" ><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\"> No Such File or Directory" + "</font></td></tr>";
                Setting.EmailTemplateLogSetting(mailSyncError, false);
                
            }

            if (countSyncAzureStorageError == Setting.DownloadCenterXmlSetting.syncAzureStorageTotalRegion || countSyncAzureStorageSuccess == Setting.DownloadCenterXmlSetting.syncAzureStorageTotalRegion || countSyncAzureStorageExist == Setting.DownloadCenterXmlSetting.syncAzureStorageTotalRegion)
            {
                countSyncAzureStorageError = 0;
                countSyncAzureStorageSuccess = 0;
                countSyncAzureStorageLog = 0;
                countSyncAzureStorageExist = 0;
            }
        }

        private void ConfirmUpdateFileIDList()
        {
            string fileID,fileSize;

            if (countSyncAzureStorageSuccess == Setting.DownloadCenterXmlSetting.syncAzureStorageTotalRegion || countSyncAzureStorageExist == Setting.DownloadCenterXmlSetting.syncAzureStorageTotalRegion || (countSyncAzureStorageSuccess+ countSyncAzureStorageExist) == Setting.DownloadCenterXmlSetting.syncAzureStorageTotalRegion)
            {
                fileID = Setting.DownloadCenterXmlSetting.updateFileID;
                fileSize = Setting.DownloadCenterXmlSetting.updateFileSize;
                Log.WriteLog("[Download Center][Success]Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + " send {" + "id:" + fileID + "," + "size:" + fileSize + "} for Web Api");

                responseFileList = new List<BlobStorage>();
                responseFileList.Add(new BlobStorage { id = fileID, size = fileSize });

                fileUpdate = new FileApi();
                var getUpdateFileMessage = fileUpdate.UpdateSyncFileList(responseFileList);
                Log.WriteLog(getUpdateFileMessage);
            }
        }

        private void GetSyncAzureStorageLog()
        {
            countSyncAzureStorageLog++;
            if (countSyncAzureStorageLog == Setting.DownloadCenterXmlSetting.syncAzureStorageTotalRegion)
            {
                countSyncAzureStorageFile++;
                Setting.GetSyncAzureStorageFile(countSyncAzureStorageFile);
                mailSyncSuccess = mailSyncSuccess +  "<tr><td width = \"22%\" bgcolor = \"#EEF4FD\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">" + syncSuccessStartTime + "</font></td><td width = \"55%\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">" + syncSourcePath.Replace("\\","/") + "</font></td><td width = \"30%\" ><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">already Copy to " + "Storage " + syncRenameFilePath.Replace("\\", "/") + "</font></td></tr>";
            }   
        }

        public static SingleTransferContext GetSingleTransferContext(TransferCheckpoint checkpoint)
        {
            SingleTransferContext context = new SingleTransferContext(checkpoint);

            context.ProgressHandler = new Progress<TransferStatus>((progress) =>
            {
                Console.Write("\rBytes transferred: {0}", progress.BytesTransferred);
            });

            return context;
        }

        public async Task TransferLocalFileToAzureBlob(CloudStorageAccount account)
        {
            try
            {
                string getStorageRegion = "";
                getStorageRegion = storageRegion;
                CloudBlobClient blobClient = account.CreateCloudBlobClient();
                TransferCheckpoint checkpoint = null;
                SingleTransferContext context = GetSingleTransferContext(checkpoint);
                Console.WriteLine("Transfer started..." + getStorageRegion + "");
                Stopwatch stopWatch = Stopwatch.StartNew();
                
                CloudBlobContainer container = blobClient.GetContainerReference(storageContainer);
                CloudBlockBlob blob = container.GetBlockBlobReference(syncRenameFilePath);

                if (!blob.Exists())
                {
                    if (File.Exists(syncSourcePath))
                    { 
                        await TransferManager.UploadAsync(syncSourcePath, blob);
                        stopWatch.Stop();
                        Console.WriteLine("\n" + getStorageRegion + " Transfer operation completed in " + stopWatch.Elapsed.TotalSeconds + " seconds.");   
                    }
                    else
                    {
                        countSyncAzureStorageError++;
                        syncErrorStartTime = Time.GetTimeNow(Time.TimeFormatType.YearSMonthSDayTimeChange);
                        if (sourcePathError)
                        {
                            sourcePathError = false;
                            Log.WriteLog("[Download Center][Error]Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + " Source File " + syncSourcePath + " not exists");
                        }
                    }

                    if (blob.Exists())
                    { 
                        countSyncAzureStorageSuccess++;
                        syncSuccessStartTime = Time.GetTimeNow(Time.TimeFormatType.YearSMonthSDayTimeChange);
                        GetSyncAzureStorageLog(); 
                        Setting.EmailTemplateLogSetting(mailSyncSuccess, true);
                        Log.WriteLog("[Download Center][Success]Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + " Soruce File " + syncSourcePath + " already copy to " + getStorageRegion + " "+ blob.StorageUri.PrimaryUri.AbsoluteUri);
                        Log.WriteLog("[Download Center][Success]Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + " " + getStorageRegion + " Transfer operation completed in " + stopWatch.Elapsed.TotalSeconds + " seconds.");
                        ConfirmUpdateFileIDList();
                    }
                }
                else
                {
                    countSyncAzureStorageExist++;
                    ConfirmUpdateFileIDList();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Log.WriteLog("[Download Center][Exception]Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + " " + e.Message);
            }
        }
    }
}