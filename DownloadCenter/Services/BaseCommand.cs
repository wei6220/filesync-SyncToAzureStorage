using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DownloadCenter
{
    abstract class BaseCommand
    {
        public string cmdError { get; set; }
        public string eventSuccess { get; set; }
        public string eventException { get; set; }

        static ConsoleEventDelegate handler;
        private delegate bool ConsoleEventDelegate(int eventType);

        [DllImport("kernel32")]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        protected void BaseCmd(string commandOption, string commandPath)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c " + commandOption;
                process.StartInfo.WorkingDirectory = commandPath;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.ErrorDialog = true;
                process.OutputDataReceived += new DataReceivedEventHandler(SortOutputHandler);

                handler = new ConsoleEventDelegate(ConsoleEventCallback);
                SetConsoleCtrlHandler(handler, true);

                process.Start();
                process.BeginOutputReadLine();
                cmdError = process.StandardError.ReadToEnd();

                if (cmdError != "")
                {
                    Console.WriteLine(cmdError);
                }

                process.WaitForExit();
                process.Close();
            }
            catch (Exception e)
            {
                ErrorExceptionHandle(e.Message);
            }
        }

        virtual public void ErrorExceptionHandle(string exceptionMessage)
        {
            if (!String.IsNullOrEmpty(exceptionMessage))
            {
                Console.WriteLine(exceptionMessage);
            }
        }

        public bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                try
                {
                    Console.WriteLine("Console window closing, death imminent");
                }
                catch (Exception e)
                {    
                    Console.WriteLine(e.Message);
                }
            }
            return true;
        }

        virtual public void ResponseCmdMessage(string cmdResponse)
        {
            Console.WriteLine(cmdResponse);
        }

        private void SortOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                ResponseCmdMessage(outLine.Data);
            }
        }
    }
}
