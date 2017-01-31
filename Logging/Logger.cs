using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace Microservice.Logging
{
	/// <summary>
	/// Static class containing 2 static methods used to write information to Log Files. 
	/// Log Path comes from Web.Config file. Uses "log_directory" to determine where to write folders. 
	/// </summary>
	public static class LogWriter
	{
		/// <summary>
		/// Method used to write generic information to a Log File. Info is written to a file named "yyyy-MM-dd-INFO.txt"
		/// </summary>
		/// <param name="Message">the string that you want to write to the INFO log</param>
		public static void INFO(string Message)
		{
			string PID = System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
			String log_path = Configuration.Setting.logPath() + Configuration.Setting.serviceName();
			//String log_name = String.Format("{0}-INFO-{1}.txt", DateTime.Now.ToString("yyyy-MM-dd"), PID); // roll log files over each day
			String log_name = String.Format("{0}-INFO.txt", DateTime.Now.ToString("yyyy-MM-dd")); // roll log files over each day
			String MessagePrefix = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss\t"); // datetime string to prepend to output messages

			if (!log_path.EndsWith("\\"))
			{
				log_path += "\\";
			}
			// create the log folder if it doesnt exit
			if (!System.IO.Directory.Exists(log_path))
			{
				System.IO.Directory.CreateDirectory(log_path);
			};

			System.Text.StringBuilder sb = new StringBuilder();
			sb.Append(MessagePrefix + Message + Environment.NewLine);

			File.AppendAllText(log_path + log_name, sb.ToString());
			sb.Clear();
			sb = null;
		}
		/// <summary>
		/// Method used to write Error Details to a Log File. ERROR is written to a file named "yyyy-MM-dd-ERROR.txt"
		/// </summary>
		/// <param name="Message">the string that you want to write to the ERROR log</param>
		public static void ERROR(string Message)
		{
			String log_path = Configuration.Setting.logPath() + Configuration.Setting.serviceName();
			String log_name = DateTime.Now.ToString("yyyy-MM-dd") + "-ERROR.txt"; // roll log files over each day
			String MessagePrefix = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss\t"); // datetime string to prepend to output messages

			if (!log_path.EndsWith("\\"))
			{
				log_path += "\\";
			}
			// create the log folder if it doesnt exit
			if (!System.IO.Directory.Exists(log_path))
			{
				System.IO.Directory.CreateDirectory(log_path);
			};

			System.Text.StringBuilder sb = new StringBuilder();
			sb.Append(MessagePrefix + Message + Environment.NewLine);

			File.AppendAllText(log_path + log_name, sb.ToString());
			sb.Clear();
			sb = null;
		}
	}
}