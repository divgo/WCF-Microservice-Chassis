using System;
using System.IO;
using System.Net;
using System.Configuration;
using System.Text;

namespace Microservice.Configuration
{
	/// <summary>
	/// Static class used to gather information from the web.config
	/// </summary>
	public static class Setting
	{
		/// <summary>
		/// Returns the path configured for storing Log Files. If an AppSetting "log_directory" does not exist, Logs will be saved to the Executables directory
		/// </summary>
		/// <returns>String</returns>
		public static string logPath()
		{
			if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["log_directory"]))
			{
				return ConfigurationManager.AppSettings["log_directory"];
			}
			else
			{
				return AppDomain.CurrentDomain.BaseDirectory;
			}
		}
		/// <summary>
		/// Returns a String representation of the service's name. Uses AppSettings "environment" and "service_deployment_root" to derive the services name.
		/// This serviceName() value is used to create a LogDirectory (in logPath()) folder for the services Logs.
		/// This serviceName() value is used by the Load Balancer to direct traffic to hosts running this service.
		/// </summary>
		/// <returns></returns>
		public static string serviceName()
		{
			String service_name = "";

			if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["environment"]))
			{
				service_name = ConfigurationManager.AppSettings["environment"];
			}
			if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["service_deployment_root"]))
			{
				string root = ConfigurationManager.AppSettings["service_deployment_root"];
				if (!String.IsNullOrEmpty(root))
				{
					root = root.ToLower().Replace("/", "-");
					service_name = service_name + root;
				}
			}
			return service_name;
		}
	}
	/// <summary>
	/// A Static class used to safely read values from the services Web.config file
	/// </summary>
	public static class Reader
	{
		/// <summary>
		/// Static safe method of returning an item in the AppSettings collection
		/// </summary>
		/// <param name="key_name"></param>
		/// <returns></returns>
		public static string getAppSetting(string key_name)
		{
			if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings[key_name]))
			{
				return ConfigurationManager.AppSettings[key_name];
			}
			else
			{
				return null;
			}
		}
		/// <summary>
		/// Static safe method of returning an item in the ConnectionString collection
		/// </summary>
		/// <param name="key_name"></param>
		/// <returns></returns>
		public static string getConnectionString(string key_name)
		{
			if (ConfigurationManager.ConnectionStrings[key_name] != null)
			{
				return ConfigurationManager.ConnectionStrings[key_name].ConnectionString;
			}
			else
			{
				return null;
			}
		}
		/// <summary>
		/// Static safe method of returning the "ProviderName" from an item in the ConnectionString collection.
		/// This is mostly used by ZooKeeper implementations to get the path where config data is stored in ZooKeeper
		/// </summary>
		/// <param name="key_name">The name Connection String</param>
		/// <returns>String</returns>
		public static string getConnectionStringProvider(string key_name)
		{
			if (ConfigurationManager.ConnectionStrings[key_name] != null)
			{
				return ConfigurationManager.ConnectionStrings[key_name].ProviderName;
			}
			else
			{
				return null;
			}
		}
	}
	/// <summary>
	/// A Static class used to get useful information about the machine running the service. 
	/// </summary>
	public static class ServerInfo
	{
		/// <summary>
		/// A Static String representing the Name of the machine running the service
		/// </summary>
		public static string machine_name
		{
			get
			{
				return Environment.MachineName;
			}
		}
		/// <summary>
		/// A Static string representing the IP Address of the machine running the service.
		/// </summary>
		public static string ip_address
		{
			get
			{
				System.Net.IPHostEntry heserver = Dns.GetHostEntry(Dns.GetHostName());
				IPAddress curAdd = heserver.AddressList[0];
				return curAdd.ToString();
			}
		}
	}
}
