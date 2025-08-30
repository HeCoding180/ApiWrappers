using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiWrappers.RateLimitation
{
    public enum ApiRateLimitPeriod
    {
        /// <summary>
        /// Rate limit will be applied per second.
        /// </summary>
        Second,
        /// <summary>
        /// Rate limit will be applied per minute.
        /// </summary>
        Minute,
        /// <summary>
        /// Rate limit will be applied per hour.
        /// </summary>
        Hour,
        /// <summary>
        /// Rate limit will be applied per day.
        /// </summary>
        Day
    }

    public class ApiRateLimitter : IDisposable
    {
        //   ---   Private Properties   ---

        /// <summary>
        /// Semaphore used to limit the number of requests.
        /// </summary>
        private SemaphoreSlim? _requestSemaphore;

        /// <summary>
        /// Data processing engine used to control the release of the request semaphore.
        /// </summary>
        private readonly DataProcessingEngine<DateTime> _releaseTimeProcessor;

        //   ---   Public Properties (Static)   ---

        /// <summary>
        /// Timeout delay for API rate limiter in milliseconds.
        /// </summary>
        public static virtual int RateLimitOverheadTimeout => 20000;

        /// <summary>
        /// Gets whether to register API usage before request.
        /// </summary>
        public static virtual bool RegisterUsageBeforeRequest => false;

        //   ---   Public Properties   ---

        /// <summary>
        /// Logger for the API rate limiter class.
        /// </summary>
        public static ILogger log;

        /// <summary>
        /// Gets the delay time span based on the rate limit period.
        /// </summary>
        public TimeSpan DelayTimeSpan => RateLimitPeriod switch
        {
            ApiRateLimitPeriod.Second => TimeSpan.FromSeconds(1),
            ApiRateLimitPeriod.Minute => TimeSpan.FromMinutes(1),
            ApiRateLimitPeriod.Hour => TimeSpan.FromHours(1),
            ApiRateLimitPeriod.Day => TimeSpan.FromDays(1),
            _ => throw new ArgumentOutOfRangeException($"Invalid API rate limit period: {(int)RateLimitPeriod}")
        };

        public bool IsRunning => _releaseTimeProcessor.IsRunning;

        /// <summary>
        /// Maximum number of requests that can be made in the specified period.
        /// </summary>
        public int RateLimit { get; }

        /// <summary>
        /// Period of time in which the request limit is applied.
        /// </summary>
        public ApiRateLimitPeriod RateLimitPeriod { get; }

        /// <summary>
        /// Number of requests that can be made in the current period.
        /// </summary>
        public int RemainingRequests => _requestSemaphore?.CurrentCount ?? 0;

        //   ---   Constructors   ---

        /// <summary>
        /// Creates a new instance of the <see cref="ApiRateLimitter"/> class.
        /// </summary>
        /// <param name="rateLimit">Maximum number of requests that can be made in the specified period.</param>
        /// <param name="rateLimitPeriod">Period of time in which the request limit is applied.</param>
        public ApiRateLimitter(int rateLimit, ApiRateLimitPeriod rateLimitPeriod)
        {
            RateLimit = rateLimit;
            RateLimitPeriod = rateLimitPeriod;

            _releaseTimeProcessor = new DataProcessingEngine<DateTime>(ProcessReleaseTime);
        }

        /// <summary>
        /// Finalizer to ensure proper disposal.
        /// </summary>
        ~ApiRateLimitter()
        {
            Dispose(false);
        }

        //   ---   Private Methods   ---

        /// <summary>
        /// Method used to enter the rate limited environment.
        /// </summary>
        /// <returns>An awaitable task for entering the rate limited environment.</returns>
        /// <exception cref="ApiRateOverheadTimeoutException">Thrown if the rate limit overhead timeout is reached.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the rate limiter is not running or the request semaphore is not initialized.</exception>
        private async Task EnterRateLimitedEnvironment()
        {
            if (!IsRunning || _requestSemaphore is null)
            {
                throw new InvalidOperationException("Cannot enter rate limited environment, rate limitation is not available!");
            }

            if (!await _requestSemaphore.WaitAsync(RateLimitOverheadTimeout, _releaseTimeProcessor.CancellationToken))
            {
                log.EnterWarningMessage("API rate overhead timeout reached!");

                throw new ApiRateOverheadTimeoutException("API rate overhead timeout reached!");
            }
        }

        /// <summary>
        /// Processes a release time by waiting for the remaining time and then releasing the request semaphore.
        /// </summary>
        /// <param name="releaseTime">The time when the semaphore should be released.</param>
        private async Task ProcessReleaseTime(DateTime releaseTime)
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan remainingTime = releaseTime - now;

            if (remainingTime > TimeSpan.Zero)
            {
                await Task.Delay(remainingTime, _releaseTimeProcessor.CancellationToken);
            }

            _requestSemaphore?.Release();
        }

        /// <summary>
        /// Calculates and enqueues the next release time for the semaphore.
        /// </summary>
        private void RegisterRateLimitUsage() => _releaseTimeProcessor.EnqueueData(DateTime.UtcNow + DelayTimeSpan);

        /// <summary>
        /// Executes the request task with rate limit usage registered before execution.
        /// </summary>
        private async Task ExecuteRequestWithRateLimitBefore(Func<Task> requestTask)
        {
            if (!_releaseTimeProcessor.CancellationToken.IsCancellationRequested)
            {
                RegisterRateLimitUsage();
            }
            await requestTask();
        }

        /// <summary>
        /// Executes the request task with rate limit usage registered after execution.
        /// </summary>
        private async Task ExecuteRequestWithRateLimitAfter(Func<Task> requestTask)
        {
            try
            {
                await requestTask();
            }
            finally
            {
                if (!_releaseTimeProcessor.CancellationToken.IsCancellationRequested)
                {
                    RegisterRateLimitUsage();
                }
            }
        }

        /// <summary>
        /// Executes the request task with rate limit usage registered before execution.
        /// </summary>
        private async Task<Tresponse> ExecuteRequestWithRateLimitBefore<Tresponse>(Func<Task<Tresponse>> requestTask)
        {
            if (!_releaseTimeProcessor.CancellationToken.IsCancellationRequested)
            {
                RegisterRateLimitUsage();
            }
            return await requestTask();
        }

        /// <summary>
        /// Executes the request task with rate limit usage registered after execution.
        /// </summary>
        private async Task<Tresponse> ExecuteRequestWithRateLimitAfter<Tresponse>(Func<Task<Tresponse>> requestTask)
        {
            try
            {
                return await requestTask();
            }
            finally
            {
                if (!_releaseTimeProcessor.CancellationToken.IsCancellationRequested)
                {
                    RegisterRateLimitUsage();
                }
            }
        }

        //   ---   Public Methods   ---

                /// <summary>
        /// Method used to initialize the request semaphore. This method is called when the rate limiter supervisor is started.
        /// </summary>
        public void Start()
        {
            // Ensure that the old request semaphore is disposed properly
            _requestSemaphore?.Dispose();

            _requestSemaphore = new SemaphoreSlim(RateLimit);

            // Start the release time processor
            _releaseTimeProcessor.Start(true);
        }

        /// <summary>
        /// Method used to stop the rate limiter.
        /// </summary>
        public async Task StopAsync()
        {
            await _releaseTimeProcessor.StopAsync();
        }

        /// <summary>
        /// Method used to subscribe to the event callbacks of the rate limiter supervisor.
        /// </summary>
        public void SubscribeEventCallbacks()
        {
            ApiRateLimitterSupervisor.Started += Start;
            ApiRateLimitterSupervisor.Stopped += StopAsync;
        }

        /// <summary>
        /// Method used to encapsulate request tasks to enforce the set rate limit.
        /// </summary>
        /// <param name="requestTask">Rate-limited task to be executed.</param>
        /// <returns>An awaitable task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the rate limiter is not running or the request semaphore is not available.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the rate limiter is stopped.</exception>
        public async Task EncapsulateRequestAsync(Func<Task> requestTask)
        {
            if (RemainingRequests <= 0)
            {
                log.EnterDebugMessage("Rate limit reached, API request will be delayed!");
            }

            await EnterRateLimitedEnvironment();

            if (RegisterUsageBeforeRequest)
            {
                await ExecuteRequestWithRateLimitBefore(requestTask);
            }
            else
            {
                await ExecuteRequestWithRateLimitAfter(requestTask);
            }
        }

        /// <summary>
        /// Method used to encapsulate request actions to enforce the set rate limit.
        /// </summary>
        /// <param name="requestAction">Rate-limited action to be executed.</param>
        /// <returns>An awaitable task representing the asynchronous operation.</returns>
        public async Task EncapsulateRequestAsync(Action requestAction)
        {
            await EncapsulateRequestAsync(() =>
            {
                requestAction();
                return Task.CompletedTask;
            });
        }

        /// <summary>
        /// Method used to encapsulate request tasks to enforce the set rate limit.
        /// </summary>
        /// <typeparam name="Tresponse">Type of the response to be returned.</typeparam>
        /// <param name="requestTask">Rate-limited task to be executed.</param>
        /// <returns>An awaitable task resulting in the response of the request task.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the rate limiter is not running or the request semaphore is not available.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the rate limiter is stopped.</exception>
        public async Task<Tresponse> EncapsulateRequestAsync<Tresponse>(Func<Task<Tresponse>> requestTask)
        {
            if (RemainingRequests <= 0)
            {
                log.LogDebug("Rate limit reached, API request will be delayed!");
            }

            await EnterRateLimitedEnvironment();

            if (RegisterUsageBeforeRequest)
            {
                return await ExecuteRequestWithRateLimitBefore(requestTask);
            }
            else
            {
                return await ExecuteRequestWithRateLimitAfter(requestTask);
            }
        }

        /// <summary>
        /// Disposes the rate limiter and stops the release time processor.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected dispose method for cleanup.
        /// </summary>
        /// <param name="disposing">True if called from Dispose, false if called from finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _releaseTimeProcessor?.Stop();
                _requestSemaphore?.Dispose();
            }
        }
    }
}
