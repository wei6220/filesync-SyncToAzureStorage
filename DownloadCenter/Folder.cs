using DownloadCenterSetting;
using System;
using System.IO;

namespace DownloadCenterFolder
{
    class Folder
    {
        string folderPath = "", folderCreateMessage = "", folderExceptionMessage = "";
        bool folderCreateSuccess, folderCreateError, folderCreateException;

        public string CreateFolder(string getFolderLocation)
        {
            folderPath = getFolderLocation;

            if (!Directory.Exists(getFolderLocation))
            {
                try
                {
                    Directory.CreateDirectory(getFolderLocation);
                    folderCreateSuccess = true;
                }
                catch (Exception e)
                {
                    folderCreateException = true;
                    folderExceptionMessage = e.Message;
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                folderCreateError = true;
            }

            GetFolderLog();

            return folderCreateMessage;
        }

        private void GetFolderLog()
        {
            if (folderCreateSuccess)
            {
                folderCreateMessage = "[Download Center][Success]Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + " Downlocad Center Log Directory " + folderPath + " was created.";
            }
            else if (folderCreateException)
            {
                folderCreateMessage = "[Download Center][Exception]Schedule ID:" + Setting.DownloadCenterXmlSetting.scheduleID + " " + folderExceptionMessage;
            }
            else
            {
                folderCreateMessage = "";
            }
        }
    }
}
