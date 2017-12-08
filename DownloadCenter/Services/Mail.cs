using CommonLibrary;
using System;
using System.Collections.Generic;

namespace DownloadCenter
{
    class Mail
    {
        public Tuple<string, string> sendMail()
        {
            string resultMessage = "";
            string errorMessage = "";

            try
            {
                string emailCC = "";
                var emailGroup = Setting.Config.EmailCCList.Split(new char[] { ',' });
                for (int i = 0; i < emailGroup.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(emailGroup[i]))
                        continue;
                    if (!string.IsNullOrWhiteSpace(emailCC))
                        emailCC += ",";
                    emailCC += "\"" + emailGroup[i] + "\"";
                }

                bool isImportant = false;
                SyncResultRecords.All().ForEach(e =>
                {
                    if (e.Status.ToLower() == "failed" || e.Status.ToLower() == "exception")
                        isImportant = true;
                });

                var postContent = "{ 'subject' : '" + Setting.Config.EmailSubjectContent
                    + "','content' : '" + GetContent() + "', 'To':[" + emailCC + "],'Cc':'null', 'Bcc':'null'"
                    + (isImportant ? ", 'Priority':'high'" : "")
                    + "}";

                HttpHelper http = new HttpHelper();
                var result = http.Post(Setting.Config.EmailURL, postContent, HttpHelper.ContnetTypeEnum.Json);
                if (!string.IsNullOrEmpty(result))
                {
                    var ResultMessage = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, string>>(result);
                    resultMessage = ResultMessage["message"];
                }
            }
            catch (Exception e)
            {
                Log.WriteLog(e.Message, Log.Type.Exception);
                errorMessage = e.Message;
            }
            return Tuple.Create(resultMessage, errorMessage);
        }

        private string GetContent()
        {
            string content = GetMailTemplate().Replace('"', '\"');
            content = content.Replace("{count}", Setting.RuntimeSettings.SyncFileTotalCount.ToString());
            content = content.Replace("{ScheduleID}", Setting.RuntimeSettings.ScheduleID);
            content = content.Replace("{ScheduleStartTime}", Setting.RuntimeSettings.ScheduleStartTime);
            content = content.Replace("{ScheduleFinishTime}", Setting.RuntimeSettings.ScheduleFinishTime);
            content = content.Replace("{SourceFromHost}", "DFI_RD_SERVER");
            content = content.Replace("{SourceFromHostFolder}", Setting.Config.ApiFileListURL);
            content = content.Replace("{SyncToDestination}", "Azure Storage");
            content = content.Replace("{SyncToDestinationFolder}", Setting.RuntimeSettings.SyncAzureStorageRegion);
            content = content.Replace("{SyncLog}", Setting.RuntimeSettings.SyncResultMessage);
            return content;
        }

        private string GetMailTemplate()
        {
            string fileContent = "";
            try
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(System.IO.Path.GetFileName("EmailTemplate.html")))
                {
                    fileContent = file.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Log.WriteLog(e.Message);
            }
            return fileContent;
        }
    }
}
