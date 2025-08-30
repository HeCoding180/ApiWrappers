using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ApiWrappers.Wrapper.HTTP
{
    /// <summary>
    /// Authentication method to be used in an HTTP request.
    /// </summary>
    public enum HttpApiAuthMethod
    {
        /// <summary>
        /// No authentication.
        /// </summary>
        NoAuth,
        /// <summary>
        /// Basic username / password authentication.
        /// </summary>
        BasicAuth,
        /// <summary>
        /// Bearer token authentication.
        /// </summary>
        BearerAuth,
        /// <summary>
        /// Custom authentication string.
        /// </summary>
        CustomAuth
    }

    /// <summary>
    /// API authentication type enum used to define the type of its credentials.
    /// </summary>
    public enum HttpApiAuthType
    {
        /// <summary>
        /// Static authentication that is set once and stays the same.
        /// </summary>
        Static,
        /// <summary>
        /// Renewable authentication that periodically requires renewal of its credentials.
        /// </summary>
        Renewable
    }

    /// <summary>
    /// HTTP request authentication class used for describing and containing the content needed for authentication of a http request.
    /// </summary>
    public struct HttpApiAuthData
    {
        /// <summary>
        /// Authentication method.
        /// </summary>
        public HttpApiAuthMethod AuthMethod;

        /// <summary>
        /// Authentication header object that can be used for a http request.
        /// </summary>
        public AuthenticationHeaderValue? AuthHeader;

        /// <summary>
        /// Default HTTP authentication constructor for when no authentication is required.
        /// </summary>
        public HttpApiAuthData()
        {
            AuthMethod = HttpApiAuthMethod.NoAuth;
            AuthHeader = null;
        }

        /// <summary>
        /// HTTP authentication constructor.
        /// </summary>
        /// <param name="authMethod">Authentication method of the HTTP authentication struct.</param>
        /// <param name="authHeader">Authentication header value object.</param>
        public HttpApiAuthData(HttpApiAuthMethod authMethod, AuthenticationHeaderValue authHeader)
        {
            AuthMethod = authMethod;
            AuthHeader = authHeader;
        }

        /// <summary>
        /// Generate a basic authentication struct using basic authentication.
        /// </summary>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <returns>Returns complete HTTP authentication struct for basic authentication.</returns>
        public static HttpApiAuthData GenerateBasicAuth(string username, string password)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generate a bearer token HTTP authentication struct.
        /// </summary>
        /// <param name="bearerToken">Bearer token that is to be used for the HTTP authentication header.</param>
        /// <returns>Returns complete HTTP authentication struct for bearer token authentication.</returns>
        public static HttpApiAuthData GenerateBearerAuth(string bearerToken)
        {
            return new HttpApiAuthData(HttpApiAuthMethod.BearerAuth, new AuthenticationHeaderValue("Bearer", bearerToken));
        }

        /// <summary>
        /// Generate a custom HTTP authentication struct.
        /// </summary>
        /// <param name="authStr">Custom authentication string.</param>
        /// <returns></returns>
        public static HttpApiAuthData GenerateCustomAuth(string authStr)
        {
            return new HttpApiAuthData(HttpApiAuthMethod.CustomAuth, new AuthenticationHeaderValue(authStr));
        }
    }
}
