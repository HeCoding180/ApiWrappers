using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiWrappers
{
    /// <summary>
    /// Exception used used to wrap any other exception to add API request information.
    /// </summary>
    public class ApiWrappedException : Exception
    {
        /// <summary>
        /// Gets the address used for the request.
        /// </summary>
        public string? Address { get; }

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> containing the addresses used for the request.
        /// </summary>
        public IEnumerable<string>? Addresses { get; }

        /// <summary>
        /// Gets the chain identifier used for the request.
        /// </summary>
        public string? ChainID { get; }

        /// <summary>
        /// Gets a generic identifier of the request.
        /// </summary>
        public string? GenericIdentifier { get; }

        /// <summary>
        /// Hide the <see cref="InnerException"/>, since the non-nullable <see cref="WrappedException"/> should be used.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Do not use the inner exception for the ApiWrappedException, use the WrappedException, as it is not nullable!")]
        public new Exception? InnerException { get; }

        /// <summary>
        /// Gets the exception which got wrapped by this exception.
        /// </summary>
        public Exception WrappedException { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="ApiWrappedException"/> with specification of the request chain and address, as well as the wrapped exception.
        /// </summary>
        /// <param name="chainID">Chain identifier used for the request.</param>
        /// <param name="address">Address used for the request.</param>
        /// <param name="wrappedException">Wrapped exception that caused this exception.</param>
        public ApiWrappedException(string chainID, string address, Exception wrappedException)
            : base($"An API request with the request address {address} on the {chainID} chain failed{(string.IsNullOrWhiteSpace(wrappedException.Message) ? "!" : $" with the error message {wrappedException.Message}")}", wrappedException)
        {
            Address = address;
            ChainID = chainID;
            WrappedException = wrappedException;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ApiWrappedException"/> with specification of the request chain and addresses, as well as the wrapped exception.
        /// </summary>
        /// <param name="chainID">Chain identifier used for the request.</param>
        /// <param name="addresses"><see cref="IEnumerable{T}"/> containing the addresses used for the request.</param>
        /// <param name="wrappedException">Wrapped exception that caused this exception.</param>
        public ApiWrappedException(string chainID, IEnumerable<string> addresses, Exception wrappedException)
            : base($"An API request with the request addresses {string.Join(", ", addresses)} on the {chainID} chain failed{(string.IsNullOrWhiteSpace(wrappedException.Message) ? "!" : $" with the error message {wrappedException.Message}")}", wrappedException)
        {
            Addresses = addresses;
            ChainID = chainID;
            WrappedException = wrappedException;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ApiWrappedException"/> with specification of a generic identifier and the wrapped exception.
        /// </summary>
        /// <param name="genericIdentifier">Generic identifier of the request.</param>
        /// <param name="wrappedException">Wrapped exception that caused this exception.</param>
        public ApiWrappedException(string genericIdentifier, Exception wrappedException)
            : base($"An API request with the generic request identifier {genericIdentifier} failed{(string.IsNullOrWhiteSpace(wrappedException.Message) ? "!" : $" with the error message {wrappedException.Message}")}", wrappedException)
        {
            GenericIdentifier = genericIdentifier;
            WrappedException = wrappedException;
        }
    }
}
