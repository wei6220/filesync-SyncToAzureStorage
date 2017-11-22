using System;
using System.IO;

namespace DownloadCenter
{
    class Log
    {
        public enum Type
        {
            Info,
            Failed,
            Exception,
        };

        public static void WriteLog(string log, Type type = Type.Info)
        {
            string prefix = "[Schedule:" + Setting.RuntimeSettings.ScheduleID + "]";
            switch (type)
            {
                case Type.Info:
                    prefix += "[Info]";
                    break;
                case Type.Failed:
                    prefix += "[Failed]";
                    break;
                case Type.Exception:
                    prefix += "[Exception]";
                    break;
            }
            Console.WriteLine(prefix + " " + log);
            CommonLibrary.LogHelper.Write(prefix + " " + log,
                Time.GetNow() + ".txt",
                Setting.Config.LogFilePath + Time.GetNow(Time.TimeFormatType.YearMonth) + "\\");
        }
    }
}
