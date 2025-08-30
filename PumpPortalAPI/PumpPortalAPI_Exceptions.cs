using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiWrappers.PumpPortalAPI
{
    public class SubscriptionRequestFailedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionRequestFailedException"/> class with a default error message.
        /// </summary>
        public SubscriptionRequestFailedException()
            : base("The subscription request failed!") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionRequestFailedException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the current exception.</param>
        public SubscriptionRequestFailedException(string? message)
            : base("The subscription request failed" + (string.IsNullOrWhiteSpace(message) ? "!" : $" with the following error message: {message}")) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionRequestFailedException"/> class with a specified error message and a reference to the inner exception that caused this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the current exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a <see langword="null"/> reference if no inner exception is specified.</param>
        public SubscriptionRequestFailedException(string? message, Exception? innerException)
            : base("The subscription request failed" + (string.IsNullOrWhiteSpace(message) ? "!" : $" with the following error message: {message}"), innerException) { }
    }
}
