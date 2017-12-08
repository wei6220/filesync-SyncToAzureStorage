using System;

namespace DownloadCenter
{
    class NetCommand : BaseCommand
    {
        private string _cmdResponse = "";

        public bool ExeLoginCmd()
        {
            string netCmd = "", netPath = "";
            netCmd = "net " + @"use \\" + Setting.Config.TargetServerIP + " " + Setting.Config.TargetServerPwd + " /user:misd\\" + Setting.Config.TargetServerLogin;
            netPath = @"C:\Windows\System32";
            Log.WriteLog("Cmd: " + netCmd);

            BaseCmd(netCmd, netPath);

            return GetLoginTargetServerStatus();
        }

        private bool GetLoginTargetServerStatus()
        {
            bool isLogin;
            if (_cmdResponse.Contains("成功") || _cmdResponse.Contains("successfully"))
            {
                isLogin = true;
                Log.WriteLog(Setting.Config.TargetServerLogin
                    + " Success Login TargetServer(" + Setting.Config.TargetServerIP + ")" );
            }
            //else if (cmdError.Contains("錯誤") || cmdError.Contains("error"))
            //{
            //    logMessage = "Fail Login TargetServer(" + Setting.Config.TargetServerIP + ")";
            //    isLogin = false;
            //}
            else
            {
                isLogin = false;
                Log.WriteLog(Setting.Config.TargetServerLogin
                    + " Fail Login TargetServer(" + Setting.Config.TargetServerIP + ")", Log.Type.Failed);

            }
            return isLogin;
        }

        public override void ResponseCmdMessage(string cmdResponse)
        {
            if (!string.IsNullOrEmpty(cmdResponse))
            {
                _cmdResponse = cmdResponse;
                Log.WriteLog("Cmd response:" + cmdResponse);
            }
        }

        public override void ErrorExceptionHandle(string exceptionMessage)
        {
            if (!string.IsNullOrEmpty(exceptionMessage))
            {
                Log.WriteLog(exceptionMessage, Log.Type.Exception);
            }
        }
    }
}
