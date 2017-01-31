using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microservice.Configuration
{
	class Loader
	{
		/// <summary>
		/// Load Configuration Information From Zookeeper.
		/// Requires a "connectionstring" called "zookeeper' in the web.config.
		/// The ConnectionString should utilize the "ProviderName" attribute, which is the path to the configuration data stored in ZooKeeper.
		/// </summary>
		public void LoadFromZookeeper()
		{
			if (!String.IsNullOrEmpty(Configuration.Reader.getConnectionString("zookeeper")))
			{

				string sZKServer = Configuration.Reader.getConnectionString("zookeeper");
				string sConfigRoot = Configuration.Reader.getConnectionStringProvider("zookeeper");

				Logging.LogWriter.INFO("ZooKeeper Server:" + sZKServer);
				Logging.LogWriter.INFO("ZooKeeper Root:" + sConfigRoot);

				// zw = new ZooWatcher();
				Zoo zookeeper = new Zoo(sZKServer); // , zw);

				string config = zookeeper.getNodeDataString(sConfigRoot, false);
				Logging.LogWriter.INFO(config);

				foreach (string child in zookeeper.getNodeChildren(sConfigRoot, false))
				{
					Logging.LogWriter.INFO(child.ToString());
					string sConfigValue = zookeeper.getNodeDataString(sConfigRoot + "/" + child, false);
					Logging.LogWriter.INFO(sConfigValue);
					ConfigurationManager.AppSettings.Set(child, sConfigValue);
				}

				zookeeper.close();
				zookeeper = null;
			}
		}
	}
}
