using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security;
using System.Security.Claims;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security.Tokens;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microservice.Authentication
{
	/// <summary>
	/// Token Validation Code which is implemented as a MessageInspector. Uses a block of "AppSettings" config entries to drive token checking
	/// </summary>
    public class TokenValidator : IDispatchMessageInspector
    {
        #region Instance Variables
        // This value comes from the web.config of each respective microservice.
        private string token_key = ConfigurationManager.AppSettings["token_key"];
        // Token issuer trust checks
        private string token_issuer = ConfigurationManager.AppSettings["token_issuer"];
        private string token_secret_key = ConfigurationManager.AppSettings["token_secret_key"];

		private string host_info = "";
        #endregion

        #region Interface Methods
        public object AfterReceiveRequest(ref System.ServiceModel.Channels.Message request, System.ServiceModel.IClientChannel channel, InstanceContext instanceContext)
        {
			// If a token_key config entry exists in web.config, then this service requires authentication. 
			// If the config does not exist, the service can be used without a security token.

			var requestProperties = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];
			string endpoint = request.Headers.To.ToString();

			try
			{
				if (!String.IsNullOrEmpty(endpoint) && endpoint.IndexOf("/") > -1)
				{
					endpoint = endpoint.Substring(endpoint.LastIndexOf("/") + 1).ToLower();
				}
			} catch(Exception ex)
			{
				Logging.LogWriter.ERROR("endpoint parsing error" + ex.Message);
				Logging.LogWriter.ERROR(ex.InnerException.ToString());
			}


			if (!String.IsNullOrEmpty(token_key) && endpoint != "heartbeat")
            {

				//request.Headers.Action;


				IPHostEntry heserver = Dns.GetHostEntry(Dns.GetHostName());
				IPAddress curAdd = heserver.AddressList[0];
				host_info = curAdd.ToString();

                string authorization = requestProperties.Headers[HttpRequestHeader.Authorization];

                if(!String.IsNullOrEmpty(authorization))
                {
                    AuthenticationHeaderValue auth = new AuthenticationHeaderValue("Bearer", authorization.Substring("Bearer ".Length).Trim());
                    ClaimsPrincipal principal;

                    try
                    {
                        principal = AuthenticateRequest(auth.Parameter);
                        Thread.CurrentPrincipal = principal;
                    }
                    catch (Exception e)
                    {
                        throw new FaultException<UnauthorizedAccessException>(
                            new UnauthorizedAccessException("Error in request authentication: " + e.Message));
                    }

                    if (principal == null)
                        throw new FaultException<UnauthorizedAccessException>(new UnauthorizedAccessException());
                }
                else
                {
                    throw new FaultException<UnauthorizedAccessException>(new UnauthorizedAccessException("Request does not have authorization token"));
                }
            }
            else
            {
                // Service not configured to look for AuthTokens, allow traffic
            }

            return instanceContext;
        }

        public void BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState) {
			{
				try
				{
					var httpHeader = reply.Properties["httpResponse"] as HttpResponseMessageProperty;
					httpHeader.Headers.Add("token_valid", Thread.CurrentPrincipal.Identity.IsAuthenticated.ToString());
					httpHeader.Headers.Add("host_name", host_info);
				}
				catch (Exception ex)
				{

				}
			}
		}
        #endregion

        #region Helper Methods
        private ClaimsPrincipal AuthenticateRequest(string tokenString)
        {
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal();
            SecurityToken token;
            var tokenHandler = new JwtSecurityTokenHandler();

            TokenValidationParameters parameters = new TokenValidationParameters()
            {
                ValidIssuer = token_issuer,
                ValidAudiences = ValidAudiences(),
                IssuerSigningToken = new BinarySecretSecurityToken(GetTokenSigningKey())
            };

            claimsPrincipal = tokenHandler.ValidateToken(tokenString, parameters, out token);

            return claimsPrincipal;
        }

        private IEnumerable<String> ValidAudiences()
        {
            yield return ConfigurationManager.AppSettings["token_audience_main"];
            yield return ConfigurationManager.AppSettings["token_audience_services"];
        }

        private byte[] GetTokenSigningKey()
        {
            return Encoding.UTF8.GetBytes(token_secret_key);
        }
        #endregion
    }
}
