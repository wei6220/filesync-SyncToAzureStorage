using DownloadCenterLog;
using DownloadCenterRsyncBaseCmd;
using DownloadCenterSetting;
using System;

namespace DownloadCenterNetCommand
{
    class NetCommand : BaseCommand
    {
        private string successOutput = "";
        private string logStatus = "", logMessage = "";
        private bool netCmdLoginStatus;

        private void LoginTaegetServer()
        {
            if (cmdError.Contains("錯誤") || cmdError.Contains("error"))
            {
                logStatus = "[Error]";
                logMessage = "Fail Login";
                netCmdLoginStatus = false;

            }
            else if (successOutput.Contains("成功") || successOutput.Contains("successfully"))
            {
                logStatus = "[Success]";
                logMessage = "Success Login Target";
                netCmdLoginStatus = true;
            }
            else
            {
                logStatus = "[Unknown]";
                logMessage = "Fail Login Target";
                netCmdLoginStatus = false;
            }

            Console.WriteLine(Setting.DownloadCenterXmlSetting.targetServerLogin + " " + logMessage + " " + Setting.DownloadCenterXmlSetting.targetServerIP);
            Log.WriteLog("[Download Center]" + logStatus + "Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + " " + Setting.DownloadCenterXmlSetting.targetServerLogin + " " + logMessage + " " + Setting.DownloadCenterXmlSetting.targetServerIP);
        }

        public bool ExeCommand()
        {
            string netCmd = "", netPath = "";

            netCmd = "net " + @"use \\" + Setting.DownloadCenterXmlSetting.targetServerIP + " " + Setting.DownloadCenterXmlSetting.targetServerPwd + " /user:misd\\" + Setting.DownloadCenterXmlSetting.targetServerLogin;
            netPath = @"C:\Windows\System32";
            BaseCmd(netCmd, netPath);

            LoginTaegetServer();

            return netCmdLoginStatus;
        }

        public override void ResponseCmdMessage(string cmdResponse)
        {
            if (!String.IsNullOrEmpty(cmdResponse))
            {
                successOutput = cmdResponse;
            }
        }

        public override void ErrorExceptionHandle(string exceptionMessage)
        {
            if (exceptionMessage != "")
            {
                Log.WriteLog("[Download Center][Exception]Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + " " + exceptionMessage);
            }
        }
    }
}
