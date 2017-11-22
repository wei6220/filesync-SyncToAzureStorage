using System;

namespace DownloadCenter
{
    class Time
    {
        public enum TimeFormatType
        {
            YearMonthDayTime,
            YearMonthDay,
            YearSMonthSDay,
            YearMonth,
            YearSMonthSDayTimeChange,
            YearMonthDayHourMinute
        };

        public static string GetFormatType(TimeFormatType timeType)
        {
            string dataTime = "";

            if (timeType == TimeFormatType.YearMonthDayTime)
            {
                dataTime = "yyyy/MM/dd H:mm:ss";
            }
            else if(timeType == TimeFormatType.YearSMonthSDayTimeChange)
            {
                dataTime = "yyyy/MM/dd tt H:mm:ss";
            }
            else if (timeType == TimeFormatType.YearMonthDay)
            {
                dataTime = "yyyyMMdd";
            }
            else if (timeType == TimeFormatType.YearSMonthSDay)
            {
                dataTime = "yyyy/MM/dd";
            }
            else if(timeType == TimeFormatType.YearMonth)
            {
                dataTime = "yyyyMM";
            }
            else if (timeType == TimeFormatType.YearMonthDayHourMinute)
            {
                dataTime = "yyyyMMddHHmm";
            }
            else
            {
                dataTime = "yyyy";
            }

            return dataTime;
        }

        public static string GetNow(TimeFormatType getDataTimeType = TimeFormatType.YearMonthDay)
        {
            return DateTime.Now.ToString(GetFormatType(getDataTimeType));
        }
    
    }
}
