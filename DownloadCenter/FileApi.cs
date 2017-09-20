using CommonLibrary;
using DownloadCenterAzureStorage;
using DownloadCenterSetting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace DownloadCenterFileApi
{
    class FileApi
    {
        private int countRegion;
        private string apiFileList; 
        private string responseUpdateFileMessage;
        private Dictionary<string, string> responseFileIDApi;
        private HttpHelper http;

        public Tuple<string, string> GetFileList()
        {
            string errorMessage = null;
            try
            {
                http = new HttpHelper();
                apiFileList = http.Get(Setting.DownloadCenterXmlSetting.apiFileListURL);
            }
            catch(Exception e)
            {
                errorMessage = "[Download Center][ Exception ]Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + " " + e.Message;
                Console.WriteLine(e.Message); 
            }
            return new Tuple<string, string>(apiFileList, errorMessage);
        }

        public string SettingAzureStorageRegion(string getAzureStorageRegion)
        {
            string getAzureRegion = "";
            JObject fileList = (JObject)JsonConvert.DeserializeObject(getAzureStorageRegion);

            var storageSetting = fileList["storages"];

            foreach (var storageRegion in storageSetting)
            {
                if (storageRegion.Next == null)
                {
                    getAzureRegion = getAzureRegion + storageRegion["region"];
                }
                else
                {
                    getAzureRegion = getAzureRegion + storageRegion["region"] + "、";
                }

                if (storageRegion["region"] != null)
                {
                    countRegion++;
                }
            }
            return getAzureRegion;
        }

        public int GetAzureStorageTotalRegion()
        {
            return countRegion;
        }

        public string UpdateSyncFileList(List<BlobStorage> FileID)
        {
            try
            {
                Setting.GetApiFileUpdateURL();
                http = new HttpHelper();

                var getUpdateFileMessage = http.Post(Setting.DownloadCenterXmlSetting.apiFileUpdateURL, FileID, HttpHelper.ContnetTypeEnum.Json);

                if (getUpdateFileMessage == null)
                {
                    responseUpdateFileMessage = "[Download Center][  Error  ]Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + " Post Update File Api is Error";
                }
                else
                {
                    responseFileIDApi = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(getUpdateFileMessage);
                    responseUpdateFileMessage = "[Download Center][ Success ]Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + " " + responseFileIDApi["message"];
                }
            }
            catch (Exception e)
            {
                responseUpdateFileMessage = "[Download Center][Exception]Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + " " + e.Message;
            }
                return responseUpdateFileMessage;
        }
    }
}
