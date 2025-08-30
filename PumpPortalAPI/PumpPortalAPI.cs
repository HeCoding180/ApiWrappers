using Org.BouncyCastle.Asn1.Ocsp;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ApiWrappers.PumpPortalAPI
{
    /// <summary>
    /// Event handler delegate used for PumpPortal API events.
    /// </summary>
    /// <param name="responseData"><see cref="PumpPortal_DataMessage"/> struct containing the response data.</param>
    public delegate void PumpPortalApiEventHandler(PumpPortal_DataMessage responseData);

    public class PumpPortalAPI
    {
        //   ---   Private Constants   ---

        // language = regex
        private const string SUCCESS_MESSAGE_REGEX = "{\"message\":.*}";

        // language = regex
        private const string ERROR_MESSAGE_REGEX = "{\"errors\":.*}";

        // language = regex
        private const string DATA_MESSAGE_REGEX = "{\"signature\":.*}";

        //   ---   Public Constants   ---

        public const string API_BASE_ADDRESS = "wss://pumpportal.fun/api/data";

        //   ---   Private Properties   ---

        /// <summary>
        /// Cancellation token source used for the PumpPortal API wrapper.
        /// </summary>
        private CancellationTokenSource? _cts;

        /// <summary>
        /// Gets whether there is an active request.
        /// </summary>
        private bool RequestActive => (RequestSemaphore.CurrentCount == 0);

        /// <summary>
        /// Semaphore used to ensure only one request is active at a time.
        /// </summary>
        private readonly SemaphoreSlim RequestSemaphore;

        /// <summary>
        /// <see cref="BufferBlock{T}"/> instance used to communicate response messages
        /// </summary>
        private readonly BufferBlock<PumpPortalAPI_ActionResponse> ResponseBuffer;

        /// <summary>
        /// Wrapper object handling the low level operation of the WebSocket connection.
        /// </summary>
        private readonly WebsocketApiWrapper Wrapper;

        //   ---   Public Properties   ---

        /// <summary>
        /// Logger for the PumpPortal API class.
        /// </summary>
        public readonly ILogger log;

        /// <summary>
        /// Gets if there is an active subscription to "New Token" events.
        /// </summary>
        public bool NewTokenEventsSubscribed { private set; get; }

        /// <summary>
        /// Processing task, which continuously processes API requests.
        /// </summary>
        public Task? ProcessingTask { private set; get; }

        /// <summary>
        /// Gets if there is an active subscription to "New Token" events.
        /// </summary>
        public bool TokenMigrationEventsSubscribed { private set; get; }

        /// <summary>
        /// Gets if the WebSocket API wrapper is running and active.
        /// </summary>
        public bool IsRunningAndActive => Wrapper.ActiveConnectionState && Wrapper.TargetConnectionState;

        //   ---   Events   ---

        /// <summary>
        /// Occurs when a token created message is received from the server.
        /// </summary>
        public event PumpPortalApiEventHandler? NewToken;

        /// <summary>
        /// Occurs when the server unexpectedly closes its connection.
        /// </summary>
        public event EventHandler? ServerClosedConnection;

        /// <summary>
        /// Occurs when a token migrated message is received from the server.
        /// </summary>
        public event PumpPortalApiEventHandler? TokenMigrated;

        /// <summary>
        /// Occurs when any trade message is received from the server.
        /// </summary>
        public event PumpPortalApiEventHandler? Trade;

        //   ---   Constructors   ---

        /// <summary>
        /// Creates a new instance of the <see cref="PumpPortalAPI"/> class.
        /// </summary>
        /// <param name="logger">Logger for the PumpPortal API class.</param>
        public PumpPortalAPI(ILogger logger)
        {
            NewTokenEventsSubscribed = false;
            TokenMigrationEventsSubscribed = false;

            RequestSemaphore = new SemaphoreSlim(1);

            ResponseBuffer = new BufferBlock<PumpPortalAPI_ActionResponse>();

            Wrapper = new WebsocketApiWrapper(API_BASE_ADDRESS);

            log = logger;
        }

        //   ---   Private Methods   ---

        /// <summary>
        /// Method used to forward top level request.
        /// </summary>
        private void OnServerClosedConnection(object? sender, EventArgs e)
        {
            ServerClosedConnection?.Invoke(sender, e);
        }

        public async Task ProcessApiResponses()
        {
            if (!IsRunningAndActive)
                return;

            try
            {
                await foreach (string serverMessage in Wrapper.ReceiveDataAsync())
                {
                    if (Regex.IsMatch(serverMessage, SUCCESS_MESSAGE_REGEX))
                    {
                        PumpPortal_SuccessMessage successMessage;

                        try
                        {
                            successMessage = JsonSerializer.Deserialize<PumpPortal_SuccessMessage>(serverMessage);
                        }
                        catch (JsonException jex)
                        {
                            log.LogWarning($"PumpPortal API JSON parsing of the following response was unsuccessful: \"{serverMessage}\"\n\tException: {jex}");
                            continue;
                        }

                        if (!RequestActive)
                        {
                            log.LogWarning($"Received a server message, but there is no active request: {successMessage.Message}");
                            continue;
                        }

                        // Communicate success message
                        ResponseBuffer.Post(new PumpPortalAPI_ActionResponse(successMessage));

                    }
                    else if (Regex.IsMatch(serverMessage, ERROR_MESSAGE_REGEX))
                    {
                        PumpPortal_ErrorMessage errorMessage;

                        try
                        {
                            errorMessage = JsonSerializer.Deserialize<PumpPortal_ErrorMessage>(serverMessage);
                        }
                        catch (JsonException jex)
                        {
                            log.LogWarning($"PumpPortal API JSON parsing of the following response was unsuccessful: \"{serverMessage}\"\n\tException: {jex}");
                            continue;
                        }

                        if (!RequestActive)
                        {
                            log.LogWarning($"Received a server error message, but there is no active request: {errorMessage.ErrorMessage}");
                            continue;
                        }

                        // Communicate error message
                        ResponseBuffer.Post(new PumpPortalAPI_ActionResponse(errorMessage));
                    }
                    else if (Regex.IsMatch(serverMessage, DATA_MESSAGE_REGEX))
                    {
                        PumpPortal_DataMessage data;

                        try
                        {
                            data = JsonSerializer.Deserialize<PumpPortal_DataMessage>(serverMessage);
                        }
                        catch (JsonException jex)
                        {
                            log.LogWarning($"PumpPortal API JSON parsing of the following response was unsuccessful: \"{serverMessage}\"\n\tException: {jex}");
                            continue;
                        }

                        switch (data.TransactionType)
                        {
                            case "create":
                                log.LogDebug("Received new token event!");
                                OnNewToken(data);
                                break;
                            case "migrate":
                                log.LogDebug("Received migration event!");
                                OnTokenMigrated(data);
                                break;
                            case "buy":
                            case "sell":
                                log.LogDebug("Received trade event!");
                                OnTokenTrade(data);
                                break;
                            default:
                                log.LogWarning($"Received unknown transaction type: \"{data.TransactionType}\"!");
                                break;
                        }
                    }
                    else
                    {
                        log.LogWarning($"Received unknown message type from server: {serverMessage}");
                    }
                }
            }
            catch (OperationCanceledException) { }
        }

        private async Task<PumpPortalAPI_ActionResponse> SendRequest(string request)
        {
            if (_cts is null)
                throw new InvalidOperationException("Cannot send request, cancellation token source is null.");

            if (!IsRunningAndActive)
                throw new InvalidOperationException("Cannot send request, WebSocket isn't running and active!");

            // Ensure only one request is active
            await RequestSemaphore.WaitAsync();

            try
            {
                // Send request
                await Wrapper.SendMessageAsync(request);

                // Await and return response
                return await ResponseBuffer.ReceiveAsync().WaitAsync(TimeSpan.FromSeconds(ConfigManager.Config.PumpPortalApiRequestTimeout));
            }
            catch (TimeoutException ex)
            {
                throw new TimeoutException($"The following request timed out: \"{request}\"", ex);
            }
            finally
            {
                // Release semaphore to allow a new request
                RequestSemaphore.Release();
            }
        }

        //   ---   Public Methods (Connection)   ---

        /// <summary>
        /// Method used to asynchronously establish a connection to the API server.
        /// </summary>
        /// <returns>An awaitable task representing the asynchronous opertation.</returns>
        public async Task ConnectAsync()
        {
            // Make sure that there is no active connection
            await DisconnectAsync();

            _cts = new CancellationTokenSource();

            // Establish connection
            await Wrapper.ConnectAsync();

            // Start processing task
            ProcessingTask = ProcessApiResponses();
            DexDataMineSupervisor.RegisterTaskForExceptionHandling(ProcessingTask, "The message processing task for the PumpPortalAPI", log);
        }

        /// <summary>
        /// Method used to asynchronously close the connection to the API server.
        /// </summary>
        /// <returns>An awaitable task representing the asynchronous opertation.</returns>
        public async Task DisconnectAsync()
        {
            if (_cts is not null)
            {
                await _cts.CancelAsync();
                _cts.Dispose();
                _cts = null;
            }

            await Wrapper.DisconnectAsync();

            if (ProcessingTask is not null)
            {
                await ProcessingTask;
                ProcessingTask.Dispose();
                ProcessingTask = null;
            }
        }

        /// <summary>
        /// Method used to try and reconnect to the WebSocket server.
        /// </summary>
        /// <param name="retrialDelay">Delay until connection is retried if it is closed unexpectedly.</param>
        /// <returns></returns>
        public async Task TryReconnectAsync()
        {
            log.LogDebug("Trying to re-establish the WebSocket connection");

            if (_cts is not null)
            {
                await _cts.CancelAsync();
                _cts.Dispose();
                _cts = null;
            }

            _cts = new CancellationTokenSource();

            // Start base wrapper
            await Wrapper.TryReconnectAsync(ConfigManager.Config.PumpPortalApiReconnectDelay * 1000);

            // Start processing task
            ProcessingTask = ProcessApiResponses();
            DexDataMineSupervisor.RegisterTaskForExceptionHandling(ProcessingTask, "The message processing task for the PumpPortalAPI", log);
        }

        //   ---   Public Methods (Events)   ---

        /// <summary>
        /// Method used for event initialization
        /// </summary>
        public void Initialize()
        {
            Wrapper.ServerClosedConnection += OnServerClosedConnection;
        }

        /// <summary>
        /// Invokes the <see cref="NewToken"/> event.
        /// </summary>
        /// <param name="responseData"><see cref="PumpPortal_DataMessage"/> struct containing the received data.</param>
        public void OnNewToken(PumpPortal_DataMessage responseData)
        {
            DexDataMineSupervisor.RegisterTaskForExceptionHandling(Task.Run(() => { NewToken?.Invoke(responseData); }), "A NewToken event callback", log);
        }

        /// <summary>
        /// Invokes the <see cref="TokenMigrated"/> event.
        /// </summary>
        /// <param name="responseData"><see cref="PumpPortal_DataMessage"/> struct containing the received data.</param>
        public void OnTokenMigrated(PumpPortal_DataMessage responseData)
        {
            DexDataMineSupervisor.RegisterTaskForExceptionHandling(Task.Run(() => { TokenMigrated?.Invoke(responseData); }), "A TokenMigrated event callback", log);
        }

        /// <summary>
        /// Invokes the <see cref="Trade"/> event.
        /// </summary>
        /// <param name="responseData"><see cref="PumpPortal_DataMessage"/> struct containing the received data.</param>
        public void OnTokenTrade(PumpPortal_DataMessage responseData)
        {
            DexDataMineSupervisor.RegisterTaskForExceptionHandling(Task.Run(() => { Trade?.Invoke(responseData); }), "A Trade event callback", log);
        }

        //   ---   Public Methods (Subscription)   ---

        public async Task SubscribeNewTokens()
        {
            const string PAYLOAD = "{\"method\":\"subscribeNewToken\"}";

            PumpPortalAPI_ActionResponse response;

            try
            {
                response = await SendRequest(PAYLOAD);
            }
            catch (InvalidOperationException ex)
            {
                log.LogError("The request for subscribing to new token events was invalid: " + ex.Message);
                throw;
            }
            catch (TimeoutException)
            {
                log.LogError("The request for subscribing to new token events timed out!");
                throw;
            }

            if (response.ResponseType == PumpPortalAPI_ActionResponseType.Error)
            {
                SubscriptionRequestFailedException ex = new SubscriptionRequestFailedException(response.ErrorMessage.HasValue ? response.ErrorMessage.Value.ErrorMessage : null);

                log.LogError(ex.Message);
                throw ex;
            }

            log.LogDebug("Successfully subscribed to token creation events!");
        }

        public async Task UnsubscribeNewTokens()
        {
            const string PAYLOAD = "{\"method\":\"unsubscribeNewToken\"}";

            PumpPortalAPI_ActionResponse response;

            try
            {
                response = await SendRequest(PAYLOAD);
            }
            catch (InvalidOperationException ex)
            {
                log.LogError("The request for unsubscribing from new token events was invalid: " + ex.Message);
                throw;
            }
            catch (TimeoutException)
            {
                log.LogError("The request for unsubscribing from new token events timed out!");
                throw;
            }

            if (response.ResponseType == PumpPortalAPI_ActionResponseType.Error)
            {
                SubscriptionRequestFailedException ex = new SubscriptionRequestFailedException(response.ErrorMessage.HasValue ? response.ErrorMessage.Value.ErrorMessage : null);

                log.LogError(ex.Message);
                throw ex;
            }

            log.LogDebug("Successfully unsubscribed from token creation events!");
        }

        public async Task SubscribeTokenMigration()
        {
            const string PAYLOAD = "{\"method\":\"subscribeMigration\"}";

            PumpPortalAPI_ActionResponse response;
            
            try
            {
                response = await SendRequest(PAYLOAD);
            }
            catch (InvalidOperationException ex)
            {
                log.LogError("The request for subscribing to token migration events was invalid: " + ex.Message);
                throw;
            }
            catch (TimeoutException)
            {
                log.LogError("The request for subscribing to token migration events timed out!");
                throw;
            }

            if (response.ResponseType == PumpPortalAPI_ActionResponseType.Error)
            {
                SubscriptionRequestFailedException ex = new SubscriptionRequestFailedException(response.ErrorMessage.HasValue ? response.ErrorMessage.Value.ErrorMessage : null);

                log.LogError(ex.Message);
                throw ex;
            }

            log.LogDebug("Successfully subscribed to token migrations!");
        }

        public async Task SubscribeTokenTrade(List<string> tokens)
        {
            if (tokens.Count == 0)
                return;

            string payload = "{\"method\":\"subscribeTokenTrade\",\"keys\":[\"" + string.Join("\",\"", tokens) + "\"]}";

            PumpPortalAPI_ActionResponse response;

            try
            {
                response = await SendRequest(payload);
            }
            catch (InvalidOperationException ex)
            {
                log.LogError("The request for subscribing to token trade events was invalid: " + ex.Message);
                throw;
            }
            catch (TimeoutException)
            {
                log.LogError("The request for subscribing to token trade events timed out!");
                throw;
            }

            if (response.ResponseType == PumpPortalAPI_ActionResponseType.Error)
            {
                SubscriptionRequestFailedException ex = new SubscriptionRequestFailedException(response.ErrorMessage.HasValue ? response.ErrorMessage.Value.ErrorMessage : null);

                log.LogError(ex.Message);
                throw ex;
            }

            log.LogDebug("Successfully subscribed to token trades!");
        }

        public async Task UnsubscribeTokenTrade(List<string> tokens)
        {
            if (tokens.Count == 0)
                return;

            string payload = "{\"method\":\"unsubscribeTokenTrade\",\"keys\":[\"" + string.Join("\",\"", tokens) + "\"]}";

            PumpPortalAPI_ActionResponse response;

            try
            {
                response = await SendRequest(payload);
            }
            catch (InvalidOperationException ex)
            {
                log.LogError("The request for unsubscribing from token trade events was invalid: " + ex.Message);
                throw;
            }
            catch (TimeoutException)
            {
                log.LogError("The request for unsubscribing from token trade events timed out!");
                throw;
            }

            if (response.ResponseType == PumpPortalAPI_ActionResponseType.Error)
            {
                SubscriptionRequestFailedException ex = new SubscriptionRequestFailedException(response.ErrorMessage.HasValue ? response.ErrorMessage.Value.ErrorMessage : null);

                log.LogError(ex.Message);
                throw ex;
            }

            log.LogDebug("Successfully unsubscribed from token trades!");
        }

        public async Task SubscribeAccountTrade(List<string> accounts)
        {
            if (accounts.Count == 0)
                return;

            string payload = "{\"method\":\"subscribeAccountTrade\",\"keys\":[\"" + string.Join("\",\"", accounts) + "\"]}";

            PumpPortalAPI_ActionResponse response;

            try
            {
                response = await SendRequest(payload);
            }
            catch (InvalidOperationException ex)
            {
                log.LogError("The request for subscribing to account trade events was invalid: " + ex.Message);
                throw;
            }
            catch (TimeoutException)
            {
                log.LogError("The request for subscribing to account trade events timed out!");
                throw;
            }

            if (response.ResponseType == PumpPortalAPI_ActionResponseType.Error)
            {
                SubscriptionRequestFailedException ex = new SubscriptionRequestFailedException(response.ErrorMessage.HasValue ? response.ErrorMessage.Value.ErrorMessage : null);

                log.LogError(ex.Message);
                throw ex;
            }

            log.LogDebug("Successfully subscribed to account trades!");
        }

        public async Task UnsubscribeAccountTrade(List<string> accounts)
        {
            if (accounts.Count == 0)
                return;

            string payload = "{\"method\":\"unsubscribeAccountTrade\",\"keys\":[\"" + string.Join("\",\"", accounts) + "\"]}";

            PumpPortalAPI_ActionResponse response;

            try
            {
                response = await SendRequest(payload);
            }
            catch (InvalidOperationException ex)
            {
                log.LogError("The request for unsubscribing from account trade events was invalid: " + ex.Message);
                throw;
            }
            catch (TimeoutException)
            {
                log.LogError("The request for unsubscribing from account trade events timed out!");
                throw;
            }

            if (response.ResponseType == PumpPortalAPI_ActionResponseType.Error)
            {
                SubscriptionRequestFailedException ex = new SubscriptionRequestFailedException(response.ErrorMessage.HasValue ? response.ErrorMessage.Value.ErrorMessage : null);

                log.LogError(ex.Message);
                throw ex;
            }

            log.LogDebug("Successfully unsubscribed from account trades!");
        }
    }
}
