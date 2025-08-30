using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiWrappers.PumpPortalAPI
{
    internal enum PumpPortalAPI_ActionResponseType
    {
        Success,
        Error
    }

    internal struct PumpPortalAPI_ActionResponse
    {
        /// <summary>
        /// Contains the type of the response.
        /// </summary>
        public PumpPortalAPI_ActionResponseType ResponseType { get; }

        /// <summary>
        /// Contains the success message if the response type is <see cref="PumpPortalAPI_ActionResponseType.Success"/>.
        /// </summary>
        public PumpPortal_SuccessMessage? SuccessMessage { get; }

        /// <summary>
        /// Contains the error message if the response type is <see cref="PumpPortalAPI_ActionResponseType.Error"/>.
        /// </summary>
        public PumpPortal_ErrorMessage? ErrorMessage { get; }

        //   ---   Constructors   ---

        /// <summary>
        /// Constructor for an successful response.
        /// </summary>
        /// <param name="successMessage"></param>
        public PumpPortalAPI_ActionResponse(PumpPortal_SuccessMessage successMessage)
        {
            ResponseType = PumpPortalAPI_ActionResponseType.Success;
            SuccessMessage = successMessage;
        }

        /// <summary>
        /// Constructor for a errorous response.
        /// </summary>
        /// <param name="errorMessage"></param>
        public PumpPortalAPI_ActionResponse(PumpPortal_ErrorMessage errorMessage)
        {
            ResponseType = PumpPortalAPI_ActionResponseType.Error;
            ErrorMessage = errorMessage;
        }
    }
}
