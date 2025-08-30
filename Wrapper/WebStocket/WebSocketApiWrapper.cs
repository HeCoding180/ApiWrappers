using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace ApiWrappers.Wrapper.WebSocket
{
    public class WebsocketApiWrapper : IDisposable
    {
        //   ---   Private Properties (const)   ---

        /// <summary>
        /// Private receive buffer size field. Default size: 8192
        /// </summary>
        private const int RECEIVE_BUFFER_SIZE = 256;

        //   ---   Private Properties   ---

        /// <summary>
        /// Logger for this <see cref="WebsocketApiWrapper"/> instance.
        /// </summary>
        private readonly ILogger log;

        /// <summary>
        /// Field containing the value of the <see cref="ActiveConnectionState"/> property.
        /// </summary>
        private bool _activeConnectionState = false;

        /// <summary>
        /// Lock for the <see cref="ActiveConnectionState"/> property.
        /// </summary>
        private readonly Lock _activeConnectionStateLock = new Lock();

        /// <summary>
        /// Cancellation token source instance for an active Websocket connection.
        /// </summary>
        private CancellationTokenSource? _cts;

        /// <summary>
        /// Field containing the value of the <see cref="TargetConnectionState"/> property.
        /// </summary>
        private bool _targetConnectionState = false;

        /// <summary>
        /// Lock for the <see cref="TargetConnectionState"/> property.
        /// </summary>
        private readonly Lock _targetConnectionStateLock = new Lock();

        /// <summary>
        /// Lock object used for the connect and disconnect methods, so only one of them can happen at a time.
        /// </summary>
        private readonly SemaphoreSlim ConnectionStateChangeSemaphore = new SemaphoreSlim(1);

        /// <summary>
        /// Websocket client used for the WebSocket connection.
        /// </summary>
        private ClientWebSocket? WsClient;

        //   ---   Public Properties   ---

        /// <summary>
        /// Gets the active state of the WebSocket connection. <see langword="true"/> if the connection is open, <see langword="false"/> if the connection is closed.
        /// </summary>
        public bool ActiveConnectionState
        {
            private set
            {
                lock (_activeConnectionStateLock)
                {
                    _activeConnectionState = value;
                }
            }
            get
            {
                lock (_activeConnectionStateLock)
                {
                    return _activeConnectionState;
                }
            }
        }

        /// <summary>
        /// Base address of the websocket connection
        /// </summary>
        public string BaseAddress { get; }

        /// <summary>
        /// Gets invoked when the server unexpectedly closes the connection.
        /// </summary>
        public event EventHandler? ServerClosedConnection;

        /// <summary>
        /// Gets the target connection state of the WebSocket connection. <see langword="true"/> if the connection should be open, <see langword="false"/> if the connection should be closed.
        /// </summary>
        public bool TargetConnectionState
        {
            private set
            {
                lock (_targetConnectionStateLock)
                {
                    _targetConnectionState = value;
                }
            }
            get
            {
                lock (_targetConnectionStateLock)
                {
                    return _targetConnectionState;
                }
            }
        }

        /// <summary>
        /// Gets the current state of this instance's underlying websocket client.
        /// </summary>
        public WebSocketState WebSocketClientState => (WsClient is null) ? WebSocketState.None : WsClient.State;

        //   ---   Constructors   ---

        public WebsocketApiWrapper(string baseAddress, ILogger logger)
        {
            BaseAddress = baseAddress;
            log = logger;
        }

        //   ---   Private Methods   ---

        private void OnServerClosedConnection()
        {
            log.LogDebug("ServerClosedConnection event invoked");
            ServerClosedConnection?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Method used to handle an exception thrown during receival of data.
        /// </summary>
        /// <param name="ex">Exception that was thrown.</param>
        private void HandleReceiveException(Exception ex)
        {
            if (ex is WebSocketException webEx)
            {
                log.LogError($"WebSocket data recieval resulted in a WebSocket exception. Error code: {(int)webEx.WebSocketErrorCode} ({webEx.WebSocketErrorCode})");

                // Re-throw error if the target state is "closed"
                if (!TargetConnectionState)
                {
                    ActiveConnectionState = false;
                    throw ex;
                }

                // Update conenction states
                if (WebSocketClientState != WebSocketState.Open)
                {
                    TargetConnectionState = false;
                    ActiveConnectionState = false;
                }

                // Check if the error is a closed prematurely error
                else if (webEx.WebSocketErrorCode != WebSocketError.ConnectionClosedPrematurely)
                {
                    throw ex;
                }

                OnServerClosedConnection();
            }
            else
            {
                log.LogError("WebSocket data recieval resulted in an exception: " + ex.StackTrace);

                // Update conenction states
                if (WebSocketClientState != WebSocketState.Open)
                {
                    TargetConnectionState = false;
                    ActiveConnectionState = false;
                }

                throw ex;
            }
        }

        //   ---   Public Methods   ---

        public async Task ConnectAsync()
        {
            await ConnectionStateChangeSemaphore.WaitAsync();

            try
            {
                // Return if running or currently connecting
                if (ActiveConnectionState || TargetConnectionState)
                    return;

                // Check if already connected
                if (WebSocketClientState == WebSocketState.Open)
                {
                    log.LogDebug("Websocket client is already connected.");
                    return;
                }

                TargetConnectionState = true;

                WsClient?.Abort();
                WsClient?.Dispose();
                WsClient = new ClientWebSocket();

                // Dispose current cancellation token source if it has not been disposed already
                _cts?.Dispose();

                // Generate new cancellation token source
                _cts = new CancellationTokenSource();

                // Connect asynchronously
                await WsClient.ConnectAsync(new Uri(BaseAddress), _cts.Token);

                log.LogDebug($"Connection to \"{BaseAddress}\" established!");

                ActiveConnectionState = true;
            }
            catch (Exception)
            {
                TargetConnectionState = false;
                ActiveConnectionState = false;

                throw;
            }
            finally
            {
                ConnectionStateChangeSemaphore.Release();
            }
        }

        public async Task DisconnectAsync()
        {
            await ConnectionStateChangeSemaphore.WaitAsync();

            try
            {
                // Return if not running or currently disconnecting
                if (!ActiveConnectionState || !TargetConnectionState)
                    return;

                // Check if websocket client is disposed
                ObjectDisposedException.ThrowIf(WsClient is null, WsClient);

                // Check if connection is not opening or opened
                if ((WebSocketClientState != WebSocketState.Connecting) && (WebSocketClientState != WebSocketState.Open))
                {
                    ActiveConnectionState = false;
                    TargetConnectionState = false;

                    return;
                }

                if (_cts is null)
                    throw new InvalidProgramException("Invalid program state reached: Websocket connection is still open, but cancellation token source is already disposed!");

                TargetConnectionState = false;

                // Close output and connection
                await WsClient.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                await WsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);

                // Cancel cancellation token
                await _cts.CancelAsync();

                _cts.Dispose();
                _cts = null;

                WsClient.Abort();
                WsClient.Dispose();
                WsClient = null;

                log.LogDebug($"Disconnected from \"{BaseAddress}\"!");

                ActiveConnectionState = false;
            }
            finally
            {
                bool resultingConnectionState = WebSocketClientState == WebSocketState.Open;

                TargetConnectionState = resultingConnectionState;
                ActiveConnectionState = resultingConnectionState;

                ConnectionStateChangeSemaphore.Release();
            }
        }

        /// <summary>
        /// Method used to try and reconnect to the WebSocket server after an exception occured.
        /// </summary>
        /// <param name="retrialDelay">Delay until connection is retried if it is closed unexpectedly.</param>
        /// <returns></returns>
        public async Task TryReconnectAsync(int retrialDelay = 1000)
        {
            if (_cts is not null)
            {
                // Cancel cancellation token
                await _cts.CancelAsync();

                _cts.Dispose();
                _cts = null;
            }

            WsClient?.Abort();
            WsClient?.Dispose();
            WsClient = null;

            ActiveConnectionState = false;
            TargetConnectionState = false;

            if (retrialDelay > 0)
            {
                log.LogDebug($"Delaying reconnect by {retrialDelay}ms");
                await Task.Delay(retrialDelay);
            }

            log.LogDebug("Trying to reconnect to the WebSocket server");
            await ConnectAsync();
        }

        /// <summary>
        /// Method used to send a string message.
        /// </summary>
        /// <param name="message">Message string that is to be sent.</param>
        /// <returns>An awaitable task representing the asynchronous opertation.</returns>
        public async Task SendMessageAsync(string message)
        {
            if (!TargetConnectionState || !ActiveConnectionState)
            {
                throw new InvalidOperationException("There is no active connection, cannot send message!");
            }
            if (WsClient is null)
            {
                throw new NullReferenceException("The ClientWebSocket instance is null even though it should be running!");
            }

            log.LogDebug($"Sending message to \"{BaseAddress}\": {message}");

            // Send request
            await WsClient.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        /// <summary>
        /// Method used to continuously receive responses.
        /// </summary>
        /// <returns>A string containing the complete response.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="InvalidProgramException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="WebSocketConnectionClosedUnexpectedlyException"></exception>
        public async IAsyncEnumerable<string> ReceiveDataAsync()
        {
            if (WebSocketClientState != WebSocketState.Open)
                throw new InvalidOperationException("Cannot receive messages, as websocket connection is not open.");

            if (_cts is null)
                throw new InvalidOperationException("The cancellation token source used for cancling asynchronous data receiving is null!");

            if (WsClient is null)
                throw new InvalidOperationException("The ClientWebSocket instance is null!");

            while (!_cts.Token.IsCancellationRequested)
            {
                MemoryStream receiveStream = new MemoryStream(RECEIVE_BUFFER_SIZE);
                WebSocketReceiveResult receiveResult;

                try
                {
                    do
                    {
                        // Asynchronously await the receival of data
                        byte[] receiveBuffer = new byte[RECEIVE_BUFFER_SIZE];
                        receiveResult = await WsClient.ReceiveAsync(receiveBuffer, _cts.Token);

                        if (receiveResult.MessageType != WebSocketMessageType.Close)
                        {
                            await receiveStream.WriteAsync(receiveBuffer, 0, receiveResult.Count);
                        }
                    } while (!receiveResult.EndOfMessage);
                }
                catch (Exception ex)
                {
                    // Ensure that the memory stream is disposed if there is an exception
                    receiveStream?.Dispose();

                    HandleReceiveException(ex);

                    break;
                }

                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    receiveStream?.Dispose();

                    // Check if target state is false -> if so, the closure was caused by the client side
                    if (!TargetConnectionState)
                    {
                        break;
                    }
                    else
                    {
                        if (receiveResult.CloseStatus.HasValue)
                        {
                            throw new WebSocketConnectionClosedUnexpectedlyException(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription);
                        }
                        else
                        {
                            throw new WebSocketConnectionClosedUnexpectedlyException();
                        }
                    }
                }

                // Check if MemoryStream is null (should not be possible, thus the InvalidProgramException)
                if (receiveStream is null)
                    throw new InvalidProgramException("Invalid program state: Reached memory stream yield point, but memory stream is null!");

                // Ignore empty streams just to be safe
                if (receiveStream.Length == 0)
                    continue;

                // Reset position and yield MemoryStream object received from the websocket connection
                receiveStream.Position = 0;

                using (receiveStream)
                using (StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    string responseString = await reader.ReadToEndAsync();

                    if (log.IsEnabled(LogLevel.Trace))
                    {
                        log.LogTrace($"Data received from \"{BaseAddress}\": \"{responseString}\"");
                    }
                    else
                    {
                        log.LogDebug($"Data received from \"{BaseAddress}\"");
                    }

                    yield return responseString;
                }
            }
        }

        /// <summary>
        /// Disposes of the current <see cref="WebsocketApiWrapper"/> instance.
        /// </summary>
        public void Dispose()
        {
            DisconnectAsync().Wait();

            WsClient?.Abort();
            WsClient?.Dispose();
        }
    }
}
