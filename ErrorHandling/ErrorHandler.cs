using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.IO;

namespace Microservice.ErrorHandling
{
	/// <summary>
	/// Class which handles all errors. 500 errors are trapped and a clean response is sent to the client. 
	/// TODO: this class needs to be extended so that details of the 500 error can be written somewhere for later inspection
	/// </summary>
    public class ErrorHandler: IErrorHandler
    {
        public bool HandleError(Exception error)
        {
            return true;
        }

        public void ProvideFault(Exception error, System.ServiceModel.Channels.MessageVersion version, ref System.ServiceModel.Channels.Message fault)
        {
            // Tell WCF to use JSON encoding rather than default XML
            var webBodyFormatProperty = new WebBodyFormatMessageProperty(WebContentFormat.Json);
            var responseProperty = new HttpResponseMessageProperty();
            responseProperty.Headers.Add(HttpResponseHeader.ContentType, "application/json");

            if (error is FaultException)
            {
                // Extract the FaultContract object from the exception object.
                var detail = error.GetType().GetProperty("Detail").GetGetMethod().Invoke(error, null);

                // Return custom error http response.
                responseProperty.StatusCode = System.Net.HttpStatusCode.Unauthorized;

                // Create a fault message containing our FaultContract object
                fault = Message.CreateMessage(version, "", detail, new DataContractJsonSerializer(detail.GetType()));
                fault.Properties.Add(WebBodyFormatMessageProperty.Name, webBodyFormatProperty);
                fault.Properties.Add(HttpResponseMessageProperty.Name, responseProperty);
            }
            else
            {
                // Return custom error http response.
                responseProperty.StatusCode = System.Net.HttpStatusCode.InternalServerError;

                var detail = error.Message;

                fault = Message.CreateMessage(version, "", detail, new DataContractJsonSerializer(typeof(string)));
                fault.Properties.Add(WebBodyFormatMessageProperty.Name, webBodyFormatProperty);
                fault.Properties.Add(HttpResponseMessageProperty.Name, responseProperty);
            }
        }
    }
}
