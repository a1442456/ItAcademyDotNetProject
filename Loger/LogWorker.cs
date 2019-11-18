using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;

namespace Loger
{
    public class LogWorker
    {
        const long MaxFileSize = 30000;
        const char indexDevider = '_';
        string _filePath;
        string _fileMask;
        public LogWorker()
        {   
                DateTime date = DateTime.Now;
                _fileMask = $"log {date.Year}{ConfigureMask(date.Month)}{ConfigureMask(date.Day)}";
                _filePath = ConfigurationManager.AppSettings["LogPath"];

                if (!Directory.Exists(_filePath))
                    CreateDirectoriesToFile();

                if (IsNeedCreateNewFile())
                    CreateLogFile();
        }

        private string ConfigureMask(int number)
        {
            StringBuilder sb = new StringBuilder(number.ToString());
            if (number < 10)
            {
                sb = new StringBuilder();
                sb.Append('0');
                sb.Append(number);
            }

            return sb.ToString();
        }

        public void TypeInLogFile(string text, StackTrace trace)
        {
            if (IsNeedCreateNewFile())
                CreateLogFile();
            string[] fileNames = Directory.GetFiles(_filePath, $"{_fileMask}*");
            string lastFile = $"{_filePath}{_fileMask}{indexDevider}{GetLastIndexFile()}.log";
            
            string traceStr = GetStringOfFrames(trace.GetFrames());
            using (System.IO.StreamWriter sw = File.AppendText(lastFile))
            {
                DateTime date = DateTime.Now;
                sw.WriteLine($"{date.TimeOfDay.ToString()} {LogStatus.Info}: {traceStr} {text}");
            }
        }

        public void TypeInLogFile(string text, StackTrace trace, LogStatus status)
        {
            if (IsNeedCreateNewFile())
                CreateLogFile();
            string[] fileNames = Directory.GetFiles(_filePath, $"{_fileMask}*");
            string lastFile = $"{_filePath}{_fileMask}{indexDevider}{GetLastIndexFile()}.log";

            string traceStr = GetStringOfFrames(trace.GetFrames());
            using (System.IO.StreamWriter sw = File.AppendText(lastFile))
            {
                DateTime date = DateTime.Now;
                sw.WriteLine($"{date.TimeOfDay.ToString()} {status}: {traceStr} {text}");
            }
        }

        public void TypeInLogFile(string text, LogStatus status)
        {
            if (IsNeedCreateNewFile())
                CreateLogFile();
            string[] fileNames = Directory.GetFiles(_filePath, $"{_fileMask}*");
            string lastFile = $"{_filePath}{_fileMask}{indexDevider}{GetLastIndexFile()}.log";
            using (System.IO.StreamWriter sw = File.AppendText(lastFile))
            {
                DateTime date = DateTime.Now;
                sw.WriteLine($"{date.TimeOfDay.ToString()} {status}: {text}");
            }
        }

        public void TypeInLogFile(string text)
        {
            if (IsNeedCreateNewFile())
                CreateLogFile();
            string[] fileNames = Directory.GetFiles(_filePath, $"{_fileMask}*");
            string lastFile = $"{_filePath}{_fileMask}{indexDevider}{GetLastIndexFile()}.log";
            using (System.IO.StreamWriter sw = File.AppendText(lastFile))
            {
                DateTime date = DateTime.Now;
                sw.WriteLine($"{date.TimeOfDay.ToString()} {LogStatus.Info}: {text}");
            }
        }

        private string GetStringOfFrames(StackFrame[] frames)
        {
            StringBuilder sb = new StringBuilder(string.Empty);
            sb.Append("[");
            sb.Append("Called class:");
            for (int frameIndex = 0; frameIndex < frames.Length; frameIndex++)
            {
                if (frameIndex == 0)
                {
                    MethodBase mb = frames[frameIndex].GetMethod();
                    sb.Append(mb.DeclaringType.FullName);
                    sb.Append(" Frames:");
                }
                sb.Append($" {frames[frameIndex].GetMethod().Name}");
                if (frameIndex < frames.Length - 1)
                {
                    sb.Append(';');
                }
            }
            sb.Append("]");
            return sb.ToString();
        }
        
        private void CreateDirectoriesToFile()
        {
            System.IO.Directory.CreateDirectory(_filePath);
        }

        private void CreateLogFile()
        {
            int fileIndex = GetLastIndexFile() + 1;
            DateTime date = DateTime.Now;
            string fileName = $"{_filePath}{_fileMask}";
            fileName = fileName.Replace(".", "");
            fileName = $"{fileName}{indexDevider}{fileIndex}.log";
            var myFile = File.Create(fileName);
            myFile.Close();
        }

        private int GetLastIndexFile()
        {
            int lastIndex = 0;
            int logIndex = 0;
            string[] fileNames = Directory.GetFiles(_filePath, $"{_fileMask}*");

            for (int fileNameIndex = 0; fileNameIndex < fileNames.Length; fileNameIndex++)
            {
                logIndex = GetLogIndexFromName(fileNames[fileNameIndex]);
                if (lastIndex < logIndex)
                    lastIndex = logIndex;
            }
            return lastIndex;
        }

        private int GetLogIndexFromName(string fileName)
        {
            string[] directories = fileName.Split('\\');
            string file = directories[directories.Length - 1];
            string indexStr = file.Substring(_fileMask.Length + 1);

            string index = string.Empty;
            for (int i = 0; i < indexStr.Length; i++)
            {
                if (indexStr[i] == '.')
                {
                    break;
                }
                index += indexStr[i];
            }
            return int.Parse(index);
        }

        private bool IsNeedCreateNewFile()
        {
            bool isNeed = new bool();
            string[] fileNames = Directory.GetFiles(_filePath,$"{_fileMask}*");
            Array.Sort(fileNames);
           
            if (fileNames.Length > 0 )
            {
                string lastFile = $"{_filePath}{_fileMask}{indexDevider}{GetLastIndexFile()}.log";
                FileInfo fi = new FileInfo(lastFile);
                long lastFileSize = fi.Length;

                if (lastFileSize > MaxFileSize)
                    isNeed = true;
            }
            else if(fileNames.Length <= 0)
                isNeed = true;
            return isNeed;
        }
    }

    public enum LogStatus
    {
        Error,
        Debug,
        Info
    }
}
