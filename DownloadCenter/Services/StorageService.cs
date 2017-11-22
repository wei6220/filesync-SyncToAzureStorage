using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace DownloadCenter
{
    class StorageService
    {
        private JToken _storages;

        public StorageService(JToken storages)
        {
            _storages = storages;
        }

        public async Task SyncFileToAzureBlob(string sourcePath, string targetPath)
        {
            var TaskList = new List<Task>();
            string region = "", container = "", connection = "";
            foreach (var storage in _storages)
            {
                connection = storage["connectstring"].ToString();
                region = storage["region"].ToString();
                container = storage["container"].ToString();
                var task = TransferLocalFileToAzureBlob(connection, region, container, sourcePath, targetPath);
                TaskList.Add(task);
            }
            await Task.WhenAll(TaskList.ToArray());
        }
        public async Task TransferLocalFileToAzureBlob(string connection, string region, string containerName, string sourcePath, string targetPath)
        {
            try
            {
                CloudStorageAccount account = CloudStorageAccount.Parse(connection);
                CloudBlobClient blobClient = account.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);
                CloudBlockBlob blob = container.GetBlockBlobReference(targetPath);

                TransferCheckpoint checkpoint = null;
                SingleTransferContext context = GetSingleTransferContext(checkpoint);

                Console.WriteLine("Transfer started..." + region + "");
                Stopwatch stopWatch = Stopwatch.StartNew();

                if (await blob.ExistsAsync())
                {
                    //Log.WriteLog(region + " blob is exist.");
                    await blob.DeleteIfExistsAsync();
                }
                await TransferManager.UploadAsync(sourcePath, blob);
                stopWatch.Stop();
                Log.WriteLog(region + " transfer operation completed in " + stopWatch.Elapsed.TotalSeconds + " seconds.");
            }
            catch (Exception e)
            {
                Log.WriteLog(e.Message, Log.Type.Exception);
                throw e;
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
    }

}