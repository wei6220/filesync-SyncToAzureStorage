using CommonLibrary;
using DownloadCenterSetting;
using System;
using System.Collections.Generic;

namespace DownloadCenterMail
{
    class Mail
    {
        private static string fileContent;
        private static string getHtmlMailTemplate;

        public static string MailTemplate()
        {
            try
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(System.IO.Path.GetFileName("EmailTemplate.html")))
                {
                    fileContent = file.ReadToEnd();
                }

            }
            catch (Exception e)
            {
                fileContent = e.Message;
                Console.WriteLine(fileContent);
            }
            return fileContent;
        }

        private static void SettingHtmlMailTemplate()
        {
            getHtmlMailTemplate = MailTemplate().Replace('"', '\"');
            getHtmlMailTemplate = getHtmlMailTemplate.Replace("{count}", Setting.DownloadCenterXmlSetting.mailSyncTotalLog.ToString());
            getHtmlMailTemplate = getHtmlMailTemplate.Replace("{ScheduleID}", Setting.DownloadCenterXmlSetting.scheduleID);
            getHtmlMailTemplate = getHtmlMailTemplate.Replace("{ScheduleStartTime}", Setting.DownloadCenterXmlSetting.scheduleStartTime);
            getHtmlMailTemplate = getHtmlMailTemplate.Replace("{ScheduleFinishTime}", Setting.DownloadCenterXmlSetting.scheduleFinishTime);
            getHtmlMailTemplate = getHtmlMailTemplate.Replace("{SourceFromHost}", "DFI_RD_SERVER");
            getHtmlMailTemplate = getHtmlMailTemplate.Replace("{SourceFromHostFolder}", Setting.DownloadCenterXmlSetting.apiFileListURL);
            getHtmlMailTemplate = getHtmlMailTemplate.Replace("{SyncToDestination}", "Azure Storage");
            getHtmlMailTemplate = getHtmlMailTemplate.Replace("{SyncToDestinationFolder}", Setting.DownloadCenterXmlSetting.syncAzureStorageRegion);
            getHtmlMailTemplate = getHtmlMailTemplate.Replace("{SyncLog}", Setting.DownloadCenterXmlSetting.mailSyncRegion + Setting.DownloadCenterXmlSetting.mailSyncSuccessLog + Setting.DownloadCenterXmlSetting.mailSyncErrorLog);
        }

        public static Tuple<string, string> sendMail()
        {
            string[] emailGroup;
            string getEmailCC, sendEmailCC, emailCC, htmlLogMessage, responseSendEmail;
            Dictionary<string, string> EmailMessageLog;
            string sendEmailLog = null, sendEmailLogException = null;

            try
            {
                Setting.GetEmailSetting();
                SettingHtmlMailTemplate();

                getEmailCC = Setting.DownloadCenterXmlSetting.emailCCList;
                emailGroup = getEmailCC.Split(new char[] { ',' });

                emailCC = "\"" + emailGroup[0] + "\"";

                for (int i = 1; i < emailGroup.Length; i++)
                {
                    sendEmailCC = "\"" + emailGroup[i] + "\"";
                    emailCC = emailCC + "," + sendEmailCC;
                }

                htmlLogMessage = "{ 'subject' : '" + Setting.DownloadCenterXmlSetting.emailSubjectContent + "','content' : '" + getHtmlMailTemplate + "', 'To':[" + emailCC + "],'Cc':'null', 'Bcc':'null'}";
                HttpHelper http = new HttpHelper();

                responseSendEmail = http.Post(Setting.DownloadCenterXmlSetting.emailURL, htmlLogMessage, HttpHelper.ContnetTypeEnum.Json);

                if (responseSendEmail == null)
                {
                    sendEmailLog = "[Download Center][  Error  ]Email Send fail";
                }
                else
                {
                    EmailMessageLog = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, string>>(responseSendEmail);
                    sendEmailLog = "[Download Center][ Success ]Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + " Email " + EmailMessageLog["message"];
                }
            }
            catch(Exception e)
            {
                sendEmailLogException = "[Download Center][ Success ]Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + " " + e.Message;
            }
            return new Tuple<string, string>(sendEmailLog, sendEmailLogException);
        }
    }
}
