using System;

namespace DownloadCenterSchedule
{
    class Schedule
    {
        bool scheduleFinishStatus, scheduleStartStatus;
        int scheduleLength;

        public int GetScheduleStatus(string[] getLogFile)
        {
            int getLogFileLength;
            string logLine;

            if (getLogFile == null)
            {
                scheduleLength = 1;
            }
            else
            {
                getLogFileLength = getLogFile.Length -1;
                while (getLogFileLength >= 0)
                {
                    logLine = getLogFile[getLogFileLength];
                    GetScheduleStatus(logLine);

                    if(scheduleStartStatus && scheduleFinishStatus)
                    {
                        scheduleLength = 1;
                        break;
                    }
                    else if(scheduleStartStatus && getLogFileLength !=0)
                    {
                        scheduleLength = 0;
                        break;
                    }
                    getLogFileLength--;
                }
            }
            return scheduleLength;
        }

        private void GetScheduleStatus(string getLogLine)
        {
            if(SearchString(ref getLogLine, "Download Center Sync File To Azure Storage Schedule is Finish"))
            {
                scheduleFinishStatus = true;
            }
            else if(SearchString(ref getLogLine, "Download Center Sync File To Azure Storage Schedule is Start"))
            {
                scheduleStartStatus = true;
            }
        }

        private bool SearchString(ref string getString,string getSubString)
        {
            return 0 <= getString.IndexOf(getSubString, StringComparison.OrdinalIgnoreCase);
        }
    }
}
