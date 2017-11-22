using CommonLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace DownloadCenter
{
    class FileApi
    {
        private int _regionCnt = 0;
        private string _regionList = "";
        private HttpHelper _http;

        public Tuple<JObject, string> GetSyncData()
        {
            string errorMessage = "";
            JObject jResult = null;
            try
            {
                _http = new HttpHelper();
                var result = _http.Get(Setting.Config.ApiFileListURL);

                if (result == "")
                {
                    errorMessage = "Get File Api Result is Null";
                    Log.WriteLog(errorMessage, Log.Type.Failed);
                }
                else
                {
                    jResult = (JObject)JsonConvert.DeserializeObject(result);

                    var storages = jResult["storages"];
                    foreach (var storage in storages)
                    {
                        if (storage["region"] == null)
                            continue;

                        if (!string.IsNullOrWhiteSpace(_regionList))
                            _regionList += "、";
                        _regionList += storage["region"];
                        _regionCnt++;
                    }
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                Log.WriteLog(e.Message, Log.Type.Exception);
            }
            return Tuple.Create(jResult, errorMessage);
        }
        public string GetStorageRegionList()
        {
            return _regionList;
        }
        public int GetStorageRegionCount()
        {
            return _regionCnt;
        }

        public string UpdateSyncStatus(List<UpdateInfo> FileID)
        {
            string resultMessage;
            try
            {
                _http = new HttpHelper();
                var response = _http.Post(Setting.Config.ApiFileUpdateURL, FileID, HttpHelper.ContnetTypeEnum.Json);
                if (string.IsNullOrEmpty(response))
                {
                    resultMessage = "Post Update File Status Api is Failed";
                    Log.WriteLog(resultMessage, Log.Type.Failed);
                }
                else
                {
                    var result = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(response);
                    resultMessage = result["message"];
                    //Log.WriteLog(resultMessage);
                }
            }
            catch (Exception e)
            {
                resultMessage = e.Message;
                Log.WriteLog(resultMessage,Log.Type.Exception);
            }
            return resultMessage;
        }

        public struct UpdateInfo
        {
            public string id { get; set; }
            public string size { get; set; }
            public string status { get; set; }
            public string message { get; set; }
        }
    }



}
