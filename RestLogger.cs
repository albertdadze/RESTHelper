using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualBasic;

namespace REST
{ 
    public sealed class RestLogger
    {
        private const string c_EventSource = "PiSys";
        private const string c_LogName = "Application";

        public static void Write(string errorMessage)
        {
            try
            {
                // the event source should be created during the installation process
                if (EventLog.SourceExists(c_EventSource))
                {
                    // write the message as an error
                    EventLog msg = new EventLog(c_LogName);
                    msg.Source = c_EventSource;
                    msg.WriteEntry(errorMessage, EventLogEntryType.Error);
                }
                else
                    // try to create the event source for the next error (this requires admin rights)
                    EventLog.CreateEventSource(c_EventSource, c_LogName);
            }
            catch
            {
            }
        }

        public static void WriteToLog(string message)
        {
            string filename = AppDomain.CurrentDomain.BaseDirectory + @"\\App_Data\RESTLogger_" + System.Convert.ToString(DateTime.Now.Year) + "-" + Strings.Right("0" + System.Convert.ToString(DateTime.Now.Month), 2) + "-" + Strings.Right("0" + System.Convert.ToString(DateTime.Now.Day), 2) + ".log";

            try
            {
                if (!File.Exists(filename))
                    File.Create(filename).Close();

                StreamWriter sw = new StreamWriter(filename, true);
                sw.WriteLine(DateTime.Now.ToString() + ": " + message);
                sw.Flush();
                sw.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }

}
