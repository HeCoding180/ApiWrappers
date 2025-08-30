using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiWrappers.RateLimitation
{
    /// <summary>
    /// Exception thrown when the API rate limiter exceeds the rate limit.
    /// </summary>
    public class ApiRateOverheadTimeoutException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ApiRateOverheadTimeoutException"/> with a message.
        /// </summary>
        /// <param name="message">Exception message string.</param>
        public ApiRateOverheadTimeoutException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ApiRateOverheadTimeoutException"/> with a message and an inner exception.
        /// </summary>
        /// <param name="message">Exception message string.</param>
        /// <param name="innerException">Inner exception object.</param>
        public ApiRateOverheadTimeoutException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
