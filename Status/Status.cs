using System;
using System.ServiceModel;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Configuration;

namespace Microservice.Status
{
	/// <summary>
	/// Factory called when a WCF service is Starting up. 
	/// Loads configuration data from external store if configured.
	/// Registers callbacks for Opened and Closing service events which perform service registration with the LoadBalancer
	/// </summary>
	class StartupHost : ServiceHostFactory
	{
		protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
		{
			Logging.LogWriter.INFO(new String('-', 200));
			Logging.LogWriter.INFO("CreateServiceHost Begin Call");
			if (!String.IsNullOrEmpty(Configuration.Reader.getConnectionString("zookeeper")))
			{
				Microservice.Configuration.Loader cl = new Configuration.Loader();
				cl.LoadFromZookeeper();
			}
			Logging.LogWriter.INFO("CreateServiceHost Complete Call");

			Logging.LogWriter.INFO("Service Host: " + Environment.MachineName);
			Logging.LogWriter.INFO("Service Name: " + serviceType.FullName);
			Logging.LogWriter.INFO("Service Path: " + ConfigurationManager.AppSettings["service_deployment_root"].ToString());
			Logging.LogWriter.INFO("Service Hosts:");
			foreach (Uri addr in baseAddresses)
			{
				Logging.LogWriter.INFO(addr.ToString());
			}

			// define the ServiceHost and bind the event handlers we are interested in
			WebServiceHost result = new WebServiceHost(serviceType, baseAddresses);
			result.Closing += result_Closing;
			result.Opened += result_Opened;

			return result;
		}

		void result_Opened(object sender, EventArgs e)
		{
			Logging.LogWriter.INFO("Service is open for business");
			Registration.registerWithNetscalar("bind");
		}

		void result_Closing(object sender, EventArgs e)
		{
			Logging.LogWriter.INFO("Service is going down");
			Registration.registerWithNetscalar("unbind");
		}
	}
}
