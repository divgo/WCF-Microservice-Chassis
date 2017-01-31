using System;
using System.IO;
using System.Net;
using System.Configuration;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microservice
{
	class Registration
	{

		private static bool Netscalar_ServiceGroupChecking()
		{
			return true;
		}

		private static string Netscalar_SendCommand(string url, string payload, string http_method, string content_type)
		{
			string output = "";
			string nitro_url = Configuration.Reader.getAppSetting("ns_url");
			string nitro_un = Configuration.Reader.getAppSetting("ns_username");
			string nitro_pw = Configuration.Reader.getAppSetting("ns_password");

			try
			{
				if (!String.IsNullOrEmpty(nitro_url) && !String.IsNullOrEmpty(nitro_un) && !String.IsNullOrEmpty(nitro_pw))
				{

					nitro_url = nitro_url + url;
					Microservice.Logging.LogWriter.INFO("nitro url: " + nitro_url);
					Microservice.Logging.LogWriter.INFO("nitro payload: " + payload);

					HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.CreateHttp(nitro_url);
					webRequest.Method = http_method;
					webRequest.ContentType = content_type;
					webRequest.Headers.Set("X-NITRO-USER", nitro_un);
					webRequest.Headers.Set("X-NITRO-PASS", nitro_pw);

					System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
					byte[] bytes = encoding.GetBytes(payload);

					using (Stream requestStream = webRequest.GetRequestStream())
					{
						requestStream.Write(bytes, 0, bytes.Length);
					}

					using (WebResponse resp = webRequest.GetResponse())
					{
						using (var streamReader = new StreamReader(resp.GetResponseStream()))
						{
							output = streamReader.ReadToEnd();
						}
					}

					Microservice.Logging.LogWriter.INFO(output);
					webRequest = null;
				}
			}
			catch (Exception ex)
			{
				Microservice.Logging.LogWriter.ERROR("nitro API Error");
				Microservice.Logging.LogWriter.ERROR(ex.Message);
				output = ex.Message;
			}

			return output;
		}
		public static void registerWithNetscalar(string status = "bind")
		{
			// this is where we will put the WebService call to the Netscalar which will inform the netscalar that this service exists
			Microservice.Logging.LogWriter.INFO("registerWithNetscalar Called");
			string service_name = Configuration.Setting.serviceName();
			string service_port = Configuration.Reader.getAppSetting("service_port");
			service_port = String.IsNullOrEmpty(service_port) ? "80" : service_port; // Most services will run on Port 80, but we should allow services to run on alt-ports

			if (!String.IsNullOrEmpty(service_name))
			{
				Microservice.Logging.LogWriter.INFO("registerWithNetscalar Beginning");

				string ns_payload = "{\"servicegroup_servicegroupmember_binding\":{\"servicegroupname\":\"" + service_name + "\",\"servername\":\"" + Configuration.ServerInfo.machine_name + "\",\"port\":" + service_port + "}}";
				string url = "config/servicegroup_servicegroupmember_binding"; // /" + service_name + "?action=" + status;				
				string output = Netscalar_SendCommand(url, ns_payload, "PUT", "application/json"); // vnd.com.citrix.netscaler.servicegroup_servicegroupmember_binding+json");

				Microservice.Logging.LogWriter.INFO("registerWithNetscalar Result");
				Microservice.Logging.LogWriter.INFO(output);
			};
		}

		private void registerWithAppManager(string serviceName)
		{
			// the app manager will be a database where we register; service_name, server_name, status, start_datetime
		}

	}
}
