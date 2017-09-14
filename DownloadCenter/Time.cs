using System;

namespace DownloadCenterTime
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

        public static string GetTimeFormatType(TimeFormatType timeType)
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
                dataTime = "yyyyMMddHmm";
            }
            else
            {
                dataTime = "yyyy";
            }

            return dataTime;
        }

        public static string GetTimeNow(TimeFormatType getDataTimeType = TimeFormatType.YearMonthDay)
        {
            string dataCreateTime = "", dataTimeType = "";

            DateTime dataTimeStart;

            dataTimeType = GetTimeFormatType(getDataTimeType);
            dataTimeStart = DateTime.Now;
            dataCreateTime = ToStringDataTime(dataTimeStart, dataTimeType);

            return dataCreateTime;
        }

        public static string ToStringDataTime(DateTime getTimeStart, string getTimeStartFormat)
        {
            string responseDataTime = "";

            responseDataTime = getTimeStart.ToString(getTimeStartFormat);

            return responseDataTime;
        }
    }
}
