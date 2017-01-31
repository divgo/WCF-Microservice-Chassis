using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
// using Microservice.Authentication;

namespace Microservice.CORS
{
	/// <summary>
	/// Implements a MessageInspector to inject CORS headers into the Response stream
	/// Also contains the ChannelDispatcher which injects a global ErrorHandler. ErrorHandler is an instance of Microservice.ErrorHandling.ErrorHandler
	/// TODO:  This should be broken out a bit more, but it works for now
	/// </summary>
	public class CORSEnablingBehavior : BehaviorExtensionElement, IEndpointBehavior
	{
		public void AddBindingParameters( ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }

		public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime) { }

		public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new CORSHeaderInjectingMessageInspector());
			endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new Microservice.Authentication.TokenValidator());

            endpointDispatcher.ChannelDispatcher.ErrorHandlers.Clear();
            endpointDispatcher.ChannelDispatcher.ErrorHandlers.Add(new Microservice.ErrorHandling.ErrorHandler());
		}

		public void Validate(ServiceEndpoint endpoint) { }

		public override Type BehaviorType { get { return typeof(CORSEnablingBehavior); } }

		protected override object CreateBehavior() { return new CORSEnablingBehavior(); }

		private class CORSHeaderInjectingMessageInspector : IDispatchMessageInspector
		{

			private static IDictionary<string, string> _headersToInject = new Dictionary<string, string>
			{
				{ "Access-Control-Allow-Origin", "*" },
				{ "Access-Control-Request-Method", "POST,GET,PUT,DELETE,OPTIONS" },
				{ "Access-Control-Allow-Headers", "X-Requested-With,Content-Type" }
			};
			
			public object AfterReceiveRequest( ref Message request, IClientChannel channel, InstanceContext instanceContext)
			{
				return null;
			}

			public void BeforeSendReply(ref Message reply, object correlationState)
			{
				var httpHeader = reply.Properties["httpResponse"] as HttpResponseMessageProperty;
				foreach (var item in _headersToInject)
					httpHeader.Headers.Add(item.Key, item.Value);
			}
		}
	}
}