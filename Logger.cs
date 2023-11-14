using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace BinderJetMotionControllerVer._1
{
    internal class Logger
    {
        static Queue<string> logQueue = new Queue<string>();
        static Queue<string> buttonLogQueue = new Queue<string>();
        static Queue<string> tempLogQueue = new Queue<string>();
        private static readonly object writeLock = new object();

        internal async static void WriteLog(string str, bool write, string filePath)
        {
            //Debug.WriteLine(str);
            logQueue.Enqueue(str);
            if (write)
            {
                lock (writeLock)
                {
                    using (StreamWriter sw = new StreamWriter(filePath, append: true))
                    {
                        while (logQueue.Count > 0)
                        {
                            string log = logQueue.Dequeue();
                            sw.Write(log);
                            
                        }
                    }
                    using (StreamWriter sw = new StreamWriter(filePath, append: true))
                    {
                        sw.WriteLine();
                    }
                }
            }
        }

        internal async static void WriteButtonLog(string str, string filePath)
        {
            string date = DateTime.Now.ToString("yyyy-MM-ddtthhmmss");
            string sendData = date + " " + str;
            Debug.WriteLine(sendData);
            buttonLogQueue.Enqueue(sendData);

            using(StreamWriter sw = new StreamWriter(filePath, append: true))
            {
                while (buttonLogQueue.Count > 0) 
                {
                    string log = buttonLogQueue.Dequeue();
                    sw.WriteLine(log);
                }
            }
        }
        internal async static void WrtieTempLog(string str, string filePath)
        {
            string sendData = str;
            
            tempLogQueue.Enqueue(sendData);
            using (StreamWriter sw = new StreamWriter(filePath, append: true))
            {
                while (tempLogQueue.Count > 0)
                {
                    string log = tempLogQueue.Dequeue();
                    sw.WriteLine(log);
                }
            }
        }
    }
}
