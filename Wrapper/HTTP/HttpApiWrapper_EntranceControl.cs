using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace ApiWrappers.Wrapper.HTTP
{
    /// <summary>
    /// Delegate used for authentication relewal methods.
    /// </summary>
    /// <param name="httpRequestMethod">Unsafe HTTP request function that should be used to generate the authentication request. <b>USE OF NORMAL HTTP REQUEST METHODS WILL LEAD TO DEADLOCK!</b></param>
    /// <returns></returns>
    public delegate Task<HttpApiAuthData> AuthenticationRenewalDelegate(Func<HttpRequestMessage, Task<HttpResponseMessage>> httpRequestMethod);

    /// <summary>
    /// Enum used to specify the request type for the RugCheck API authenticator class.
    /// </summary>
    public enum AuthEntranceControlRequest
    {
        /// <summary>
        /// Request to enter an authenticated environment.
        /// </summary>
        Enter = 0,
        /// <summary>
        /// Request to leave an authenticated environment.
        /// </summary>
        Leave,
        /// <summary>
        /// Request to renew authentication.
        /// </summary>
        RenewAuth
    }

    public partial class HttpApiWrapper
    {
        //   ---   Private Properties   ---

        /// <summary>
        /// Cancellation token source for enter requests.
        /// </summary>
        private CancellationTokenSource? _cts;

        /// <summary>
        /// Semaphore used to control entrance into authenticated environment.
        /// </summary>
        private readonly SemaphoreSlim _authEnterSemaphore = new SemaphoreSlim(0);

        /// <summary>
        /// Unsigned integer containing the amount of active authentication environments.
        /// </summary>
        private uint _activeAuthEnvironments = 0;

        /// <summary>
        /// Unsigned integer containing the amount of authentication environments that are waiting to enter.
        /// </summary>
        private uint _waitingAuthEnvironments = 0;

        /// <summary>
        /// Boolean containing the information wheter credential renewal is required.
        /// </summary>
        private bool _credRenewalRequired = true;

        /// <summary>
        /// API credential renewal function.
        /// </summary>
        private readonly AuthenticationRenewalDelegate? _credentialsRenewalFunc;

        //   ---   Public Properties   ---

        /// <summary>
        /// Processing manager for RugCheck authentication requests.
        /// </summary>
        public DataProcessingEngine<AuthEntranceControlRequest> EntranceControlManager { private set; get; }

        //   ---   Private Methods   ---

        /// <summary>
        /// Authentication request handling function used by the entrance control manager.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="InvalidEnumArgumentException"></exception>
        private async Task HandleAuthRequest(AuthEntranceControlRequest request)
        {
            if (request == AuthEntranceControlRequest.Enter)
            {
                if (_credRenewalRequired)
                {
                    _waitingAuthEnvironments++;
                }
                else
                {
                    _activeAuthEnvironments++;
                    _authEnterSemaphore.Release();
                }
            }
            else if (request == AuthEntranceControlRequest.Leave)
            {
                if (_activeAuthEnvironments > 0)
                    _activeAuthEnvironments--;
            }
            else if (request == AuthEntranceControlRequest.RenewAuth)
            {
                _credRenewalRequired = true;
            }
            else
            {
                throw new InvalidEnumArgumentException($"Invalid RugCheck authentication request enumerator value: {(int)request}", (int)request, typeof(AuthEntranceControlRequest));
            }

            if ((_activeAuthEnvironments == 0) && _credRenewalRequired)
            {
                // Call credential renewal function and store the new header in the auth data property
                AuthData = await _credentialsRenewalFunc!(request => _requestAsync(request));

                _credRenewalRequired = false;

                // Release auth environment semaphore
                if (_waitingAuthEnvironments > 0)
                {
                    _authEnterSemaphore.Release((int)_waitingAuthEnvironments);
                    _waitingAuthEnvironments = 0;
                }
            }
        }

        /// <summary>
        /// Method used to request entrance into an authenticated environment. Is used to block new entrances if the 
        /// </summary>
        /// <returns>Returns an awaitable task for entrance into an authenticated environment.</returns>
        private async Task Enter()
        {
            // Check validity of function call

            if (AuthType == HttpApiAuthType.Static)
            {
                throw new InvalidOperationException("Auth environment enter was requested for static authentication.");
            }

            if ((_cts is null) || (_credentialsRenewalFunc is null))
            {
                throw new InvalidOperationException("Auth environment enter was requested, but cancellation token source and/or credentials renewal function is not initialized/null.");
            }

            EntranceControlManager.EnqueueData(AuthEntranceControlRequest.Enter);

            try
            {
                await _authEnterSemaphore.WaitAsync(_cts.Token);
            }
            catch (OperationCanceledException ex)
            {
                throw new OperationCanceledException("Auth environment entrance was canceled.", ex);
            }
        }

        /// <summary>
        /// Method used to leave the authentication environment.
        /// </summary>
        private void Leave()
        {
            // Check validity of function call

            if (AuthType == HttpApiAuthType.Static)
            {
                throw new InvalidOperationException("Auth environment leave was requested for static authentication.");
            }

            EntranceControlManager.EnqueueData(AuthEntranceControlRequest.Leave);
        }

        //   ---   Public Methods   ---

        /// <summary>
        /// Method used to asynchronously start the entrance control manager.
        /// </summary>
        /// <param name="forceCredentialRenewal">Defines if the credentials should be immediately renewed.</param>
        public async Task StartAuthenticatorAsync(bool forceCredentialRenewal)
        {
            // Check validity of function call

            if (AuthType == HttpApiAuthType.Static)
            {
                // Auth is static, entrance control manager does not need to be started.
                return;
            }

            if (_credentialsRenewalFunc is null)
            {
                throw new InvalidOperationException("Start was requested, but credentials renewal function is not initialized/null.");
            }

            _cts = new CancellationTokenSource();

            EntranceControlManager.Start(true);

            // Set credential renewal flag if requested.
            if (forceCredentialRenewal)
            {
                _credRenewalRequired = true;
            }

            // Generate new credentials if requested
            if (_credRenewalRequired)
            {
                // Request enter and immediately leave again to trigger token renewal
                await Enter();
                Leave();
            }
        }

        /// <summary>
        /// Method used to start the entrance control manager.
        /// </summary>
        /// <param name="forceCredentialRenewal">Defines if the credentials should be immediately renewed.</param>
        public void StartAuthenticator(bool forceCredentialRenewal)
        {
            // Synchronously wait for start task.
            StartAuthenticatorAsync(forceCredentialRenewal).Wait();
        }

        /// <summary>
        /// Method used to asynchronously stop the entrance control manager.
        /// </summary>
        public async Task StopAuthenticatorAsync()
        {
            // Check validity of function call

            if (AuthType == HttpApiAuthType.Static)
            {
                // Auth is static, entrance control manager does not need to be stopped.
                return;
            }

            if (_cts is null)
            {
                throw new InvalidOperationException("Stop was requested, but cancellation token source is not initialized/null.");
            }

            // Cancel cancellation token source to cancel any active awaits to enter an authenticated environment
            await _cts.CancelAsync();

            // Stop entrance control manager
            await EntranceControlManager.StopAsync();
        }

        /// <summary>
        /// Method used to cancel active enter requests and stop the entrance control manager.
        /// </summary>
        public void StopAuthenticator()
        {
            // Synchronously wait for asynchronous stop task
            StopAuthenticatorAsync().Wait();
        }

        /// <summary>
        /// Method used to request the renewal of the API wrapper's credentials.
        /// </summary>
        public void RequestCredentialRenewal()
        {
            // Check validity of function call

            if (AuthType == HttpApiAuthType.Static)
            {
                throw new InvalidOperationException("Renewal was requested for static authentication.");
            }

            if ((_cts is null) || (_credentialsRenewalFunc is null))
            {
                throw new InvalidOperationException("Renewal was requested, but cancellation token source and/or renewal function is not initialized/null.");
            }

            EntranceControlManager.EnqueueData(AuthEntranceControlRequest.RenewAuth);
        }
    }
}
