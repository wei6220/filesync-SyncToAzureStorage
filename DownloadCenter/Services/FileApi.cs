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
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                Log.WriteLog(e.Message, Log.Type.Exception);
            }
            return Tuple.Create(jResult, errorMessage);
        }

        public string UpdateSyncStatus(List<UpdateInfo> info)
        {
            string resultMessage = "";
            try
            {
                _http = new HttpHelper();
                var response = _http.Post(Setting.Config.ApiFileUpdateURL, info, HttpHelper.ContnetTypeEnum.Json);
                if (string.IsNullOrEmpty(response))
                {
                    resultMessage = "Post UpdateStatus Api is failed.";
                    Log.WriteLog(resultMessage, Log.Type.Failed);
                }
                else
                {
                    var result = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(response);
                    if (result["status"] != null && result["status"].ToLower() == "error")
                    {
                        resultMessage = result["message"];
                        Log.WriteLog(resultMessage, Log.Type.Failed);
                    }
                }
            }
            catch (Exception e)
            {
                resultMessage = e.Message;
                Log.WriteLog(resultMessage, Log.Type.Exception);
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
