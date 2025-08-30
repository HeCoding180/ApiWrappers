using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace ApiWrappers.Wrapper.HTTP
{
    public partial class HttpApiWrapper
    {
        //   ---   Private Properties   ---

        /// <summary>
        /// Logging source for the API wrapper class.
        /// </summary>
        private readonly ILogger log;

        //   ---   Public Properties   ---

        /// <summary>
        /// Base address of the API wrapper.
        /// </summary>
        public string BaseAddress { private set; get; }

        /// <summary>
        /// URI of the base address of the API wrapper.
        /// </summary>
        public Uri BaseAddressUri { private set; get; }

        /// <summary>
        /// HTTP client object.
        /// </summary>
        public HttpClient HttpClient { private set; get; }

        /// <summary>
        /// Default HTTP authentication information that is to be used for HTTP requests.
        /// </summary>
        public HttpApiAuthData AuthData { private set; get; }

        /// <summary>
        /// Type of authentication that is to be used for this class's instance.
        /// </summary>
        public readonly HttpApiAuthType AuthType;

        //   ---   Public Constructors   ---

        /// <summary>
        /// Constructor for the API wrapper class with no authentication.
        /// </summary>
        /// <param name="baseAddress">Base API address for HTTP API requests.</param>
        public HttpApiWrapper(string baseAddress, ILogger logger)
        {
            // Store base address and URI
            BaseAddress = baseAddress;
            BaseAddressUri = new Uri(baseAddress);

            // Configure authentication settings
            AuthData = new HttpApiAuthData();
            AuthType = HttpApiAuthType.Static;

            // Set up entrance control manager so it doesn't have to be nullable
            EntranceControlManager = new DataProcessingEngine<AuthEntranceControlRequest>(HandleAuthRequest);

            // Configure HTTP client
            HttpClient = new HttpClient();
            HttpClient.BaseAddress = BaseAddressUri;

            // Store logger
            log = logger;
        }

        /// <summary>
        /// Constructor for the API wrapper class with static authentication.
        /// </summary>
        /// <param name="baseAddress">Base API address for HTTP API requests.</param>
        /// <param name="authData">Static authentication data that is to be used.</param>
        public HttpApiWrapper(string baseAddress, HttpApiAuthData authData, ILogger logger)
        {
            // Store base address and URI
            BaseAddress = baseAddress;
            BaseAddressUri = new Uri(baseAddress);

            AuthData = authData;
            AuthType = HttpApiAuthType.Static;

            // Set up entrance control manager so it doesn't have to be nullable
            EntranceControlManager = new DataProcessingEngine<AuthEntranceControlRequest>(HandleAuthRequest);

            // Configure HTTP client
            HttpClient = new HttpClient();
            HttpClient.BaseAddress = BaseAddressUri;

            // Store logger
            log = logger;
        }

        /// <summary>
        /// Constructor for the API wrapper class with renewable authentication.
        /// </summary>
        /// <param name="baseAddress">Base API address for HTTP API requests.</param>
        /// <param name="credentialRenewalDelegate"></param>
        public HttpApiWrapper(string baseAddress, AuthenticationRenewalDelegate credentialRenewalDelegate, ILogger logger)
        {
            // Store base address and URI
            BaseAddress = baseAddress;
            BaseAddressUri = new Uri(baseAddress);

            // Configure authentication settings
            AuthData = new HttpApiAuthData();
            AuthType = HttpApiAuthType.Renewable;

            // Set up entrance control manager
            EntranceControlManager = new DataProcessingEngine<AuthEntranceControlRequest>(HandleAuthRequest);
            _credentialsRenewalFunc = credentialRenewalDelegate;

            // Configure HTTP client
            HttpClient = new HttpClient();
            HttpClient.BaseAddress = BaseAddressUri;

            // Store logger
            log = logger;
        }

        //   ---   Private Methods   ---

        private LogLevel _getLogLevel(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                return LogLevel.Warning;
            }
            else
            {
                return LogLevel.Information;
            }
        }

        /// <summary>
        /// Unsafe base request method for HTTP requests.
        /// </summary>
        /// <param name="request">Request that is to be sent.</param>
        /// <returns>HTTP response message of the HTTP request.</returns>
        private async Task<HttpResponseMessage> _requestAsync(HttpRequestMessage request, bool useAuthentication = false)
        {
            log.LogDebug($"Sending {(useAuthentication ? "authenticated " : "")}{request.Method} request to {HttpClient.BaseAddress}{request.RequestUri}");

            // Start log time measurement stopwatch
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Add authentication header if set by parameter
            if (useAuthentication)
            {
                request.Headers.Authorization = AuthData.AuthHeader;
            }

            HttpResponseMessage response = await HttpClient.SendAsync(request);

            stopwatch.Stop();
            log.Log(_getLogLevel(response), $"Response received from {request.RequestUri} after {stopwatch.ElapsedMilliseconds} ms with code {(int)response.StatusCode} ({response.StatusCode})!");

            return response;
        }

        //   ---   Public Methods   ---

        /// <summary>
        /// Base method used for HTTP requests.
        /// </summary>
        /// <param name="request">HTTP request message object of the request</param>
        /// <returns>Awaitable task returning the HTTP response message of the HTTP request.</returns>
        public async Task<HttpResponseMessage> RequestAsync(HttpRequestMessage request, bool useAuthentication = false)
        {
            // If static authentication is used, to entrance control is required.
            if (AuthType == HttpApiAuthType.Static || !useAuthentication)
            {
                return await _requestAsync(request);
            }

            // Renewable environment is used. Encapsulate request in enter and leave functions.
            await Enter();

            try
            {
                return await _requestAsync(request, useAuthentication);
            }
            finally
            {
                Leave();
            }
        }

        /// <summary>
        /// Method used to asynchronously request and parse a JSON object using the HTTP GET method.
        /// </summary>
        /// <typeparam name="T">Type reference for the JSON object that is to be parsed.</typeparam>
        /// <param name="requestUri">Request URI.</param>
        /// <returns>Tast containing the parsed JSON object.</returns>
        /// <exception cref="ApiHttpExeption">Thrown if the request was not successful.</exception>
        public async Task<T?> GetJsonObjectAsync<T>(string requestUri, bool useAuthentication = false)
        {
            // Generate request message object
            HttpRequestMessage requestMsgObj = new HttpRequestMessage(HttpMethod.Get, requestUri);
            requestMsgObj.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Get the HTTP response
            HttpResponseMessage response = await RequestAsync(requestMsgObj, useAuthentication);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiHttpExeption(response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                T? parsedObject = await JsonSerializer.DeserializeAsync<T>(responseStream);

                log.LogDebug($"Completed JSON deserialization of the response from {requestMsgObj.RequestUri}");

                return parsedObject;
            }
        }

        /// <summary>
        /// Method used to request and parse a JSON object using the HTTP GET method.
        /// </summary>
        /// <typeparam name="T">Type reference for the JSON object that is to be parsed.</typeparam>
        /// <param name="requestUri">Request URI.</param>
        /// <returns>Parsed JSON object.</returns>
        /// <exception cref="ApiHttpExeption">Thrown if the request was not successful.</exception>
        public T? GetJsonObject<T>(string requestUri, bool useAuthentication = false)
        {
            Task<T?> requestTask = GetJsonObjectAsync<T>(requestUri, useAuthentication);
            requestTask.Wait();

            return requestTask.Result;
        }

        /// <summary>
        /// Method used to asynchronously request and parse a JSON object using the HTTP POST method.
        /// </summary>
        /// <typeparam name="T">Type reference for the JSON object that is to be parsed.</typeparam>
        /// <param name="requestUri">Request URI.</param>
        /// <param name="content">Content for the HTTP POST request.</param>
        /// <returns>Tast containing the parsed JSON object.</returns>
        /// <exception cref="ApiHttpExeption">Thrown if the request was not successful.</exception>
        public async Task<T?> PostJsonObjectRequestAsync<T>(string requestUri, string? content, bool useAuthentication = false)
        {
            // Generate request message object
            HttpRequestMessage requestMsgObj = new HttpRequestMessage(HttpMethod.Post, requestUri);
            if (content is not null)
                requestMsgObj.Content = new StringContent(content, Encoding.UTF8, "application/json");
            requestMsgObj.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Post the HTTP request
            HttpResponseMessage response = await RequestAsync(requestMsgObj, useAuthentication);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiHttpExeption(response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                return await JsonSerializer.DeserializeAsync<T>(responseStream);
            }
        }

        /// <summary>
        /// Method used to request and parse a JSON object using the HTTP POST method.
        /// </summary>
        /// <typeparam name="T">Type reference for the JSON object that is to be parsed.</typeparam>
        /// <param name="requestUri">Request URI.</param>
        /// <param name="content">Content of the HTTP POST request.</param>
        /// <returns>Parsed JSON object.</returns>
        /// <exception cref="ApiHttpExeption">Thrown if the request was not successful.</exception>
        public T? PostJsonObjectRequest<T>(string requestUri, string? content, bool useAuthentication = false)
        {
            Task<T?> requestTask = PostJsonObjectRequestAsync<T>(requestUri, content);
            requestTask.Wait();

            return requestTask.Result;
        }

        /// <summary>
        /// Method used to asynchronously request a string using the HTTP GET method.
        /// </summary>
        /// <param name="requestUri">Request URI.</param>
        /// <returns><returns>Tast containing a string containing the response's contents.</returns></returns>
        /// <exception cref="ApiHttpExeption">Thrown if the request was not successful.</exception>
        public async Task<string> GetStringAsync(string requestUri, bool useAuthentication = false)
        {
            // Generate request message object
            HttpRequestMessage requestMsgObj = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Get the HTTP response
            HttpResponseMessage response = await RequestAsync(requestMsgObj, useAuthentication);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiHttpExeption(response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Method used to request a string using the HTTP GET method.
        /// </summary>
        /// <param name="requestUri">Request URI.</param>
        /// <returns>String containing the response's contents.</returns>
        /// <exception cref="ApiHttpExeption">Thrown if the request was not successful.</exception>
        public string GetString(string requestUri, bool useAuthentication = false)
        {
            Task<string> requestTask = GetStringAsync(requestUri, useAuthentication);
            requestTask.Wait();

            return requestTask.Result;
        }

        /// <summary>
        /// Method used to asynchronously request a string using the HTTP POST method.
        /// </summary>
        /// <param name="requestUri">Request URI.</param>
        /// <param name="content">Content for the HTTP POST request.</param>
        /// <param name="contentType">Content type of the HTTP POST request.</param>
        /// <returns>Tast containing a string containing the response's contents.</returns>
        /// <exception cref="ApiHttpExeption">Thrown if the request was not successful.</exception>
        public async Task<string> PostStringRequestAsync(string requestUri, string? content, string? contentType, bool useAuthentication = false)
        {
            // Generate request message object
            HttpRequestMessage requestMsgObj = new HttpRequestMessage(HttpMethod.Post, requestUri);
            if (content is not null)
                requestMsgObj.Content = new StringContent(content, Encoding.UTF8, contentType);

            // Post the HTTP request
            HttpResponseMessage response = await RequestAsync(requestMsgObj, useAuthentication);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiHttpExeption(response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Method used to request a string using the HTTP POST method.
        /// </summary>
        /// <param name="requestUri">Request URI.</param>
        /// <param name="content">Content of the HTTP POST request.</param>
        /// <param name="contentType">Content type of the HTTP POST request.</param>
        /// <returns>String containing the response's contents.</returns>
        /// <exception cref="ApiHttpExeption">Thrown if the request was not successful.</exception>
        public string PostStringRequest(string requestUri, string? content, string? contentType, bool useAuthentication = false)
        {
            Task<string> requestTask = PostStringRequestAsync(requestUri, content, contentType, useAuthentication);
            requestTask.Wait();

            return requestTask.Result;
        }
    }
}
