using Org.BouncyCastle.Utilities.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ApiWrappers.Wrapper.HTTP
{
    public class ApiHttpExeption : Exception
    {
        /// <summary>
        /// Http Status Code that lead to the exception.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Message of the HTTP response.
        /// </summary>
        public string? ResponseMessage { get; }

        /// <summary>
        /// Base exception.
        /// </summary>
        public ApiHttpExeption() : base() { }

        /// <summary>
        /// Base exception with message content.
        /// </summary>
        /// <param name="message">Exception message string.</param>
        public ApiHttpExeption(string? message) : base(message) { }

        /// <summary>
        /// Base exception with message content and inner exception.
        /// </summary>
        /// <param name="message">Exception message string.</param>
        /// <param name="innerException">Inner exception object.</param>
        public ApiHttpExeption(string? message, Exception? innerException) : base(message, innerException) { }

        /// <summary>
        /// Creates a new instance of the <see cref="ApiHttpExeption"/> with an HTTP status code.
        /// </summary>
        /// <param name="statusCode">Status code of the HTTP response.</param>
        public ApiHttpExeption(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ApiHttpExeption"/> with an HTTP status code and a response message.
        /// </summary>
        /// <param name="statusCode">Status code of the HTTP response.</param>
        /// <param name="responseMessage">Message of the HTTP response.</param>
        public ApiHttpExeption(HttpStatusCode statusCode, string? responseMessage)
        {
            StatusCode = statusCode;
            ResponseMessage = responseMessage;
        }
    }
}
