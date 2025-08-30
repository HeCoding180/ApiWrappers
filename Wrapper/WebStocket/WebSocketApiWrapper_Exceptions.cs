using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace ApiWrappers.Wrapper.WebSocket
{
    public class WebSocketConnectionClosedUnexpectedlyException : Exception
    {
        //   ---   Public Properties   ---

        /// <summary>
        /// Gets the reason for the remote closure of the handshake.
        /// </summary>
        public WebSocketCloseStatus? CloseStatus { get; }

        /// <summary>
        /// Gets the description for the remote closure of the handshake.
        /// </summary>
        public string? CloseStatusDescription { get; }

        //   ---   Constructors   ---

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketConnectionClosedUnexpectedlyException"/> class with a default error message.
        /// </summary>
        public WebSocketConnectionClosedUnexpectedlyException()
            : base("The WebSocket connection was closed unexpectedly!") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketConnectionClosedUnexpectedlyException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the current exception.</param>
        public WebSocketConnectionClosedUnexpectedlyException(string? message)
            : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketConnectionClosedUnexpectedlyException"/> class with a specified error message and a reference to the inner exception that caused this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the current exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a <see langword="null"/> reference if no inner exception is specified.</param>
        public WebSocketConnectionClosedUnexpectedlyException(string? message, Exception? innerException)
            : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketConnectionClosedUnexpectedlyException"/> class with a specified <see cref="WebSocketCloseStatus"/> and close status description. Exception message is generated automatically.
        /// </summary>
        /// <param name="closeStatus">Reason for the remote closure of the handshake.</param>
        /// <param name="closeStatusDescription">Description for the remote closure of the handshake.</param>
        public WebSocketConnectionClosedUnexpectedlyException(WebSocketCloseStatus closeStatus, string? closeStatusDescription)
            : base($"The WebSocket connection was closed unexpectedly with the close status \"{closeStatus}\" ({(int)closeStatus})"
                  + (string.IsNullOrWhiteSpace(closeStatusDescription) ? "!" : $" and the following description: {closeStatusDescription}"))
        {
            CloseStatus = closeStatus;
            CloseStatusDescription = closeStatusDescription;
        }
    }
}
