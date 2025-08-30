using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiWrappers.DexScreenerAPI
{
    /// <summary>
    /// Struct used to track the origin of some typed data. Mainly used for API requests to track the request's origin.
    /// </summary>
    /// <typeparam name="Tdata">Type of the data.</typeparam>
    public struct AddressIdentifyableData<Tdata>
    {
        /// <summary>
        /// Gets the address used for the request.
        /// </summary>
        public string Address { private set; get; }

        /// <summary>
        /// Gets the chain identifier of the data.
        /// </summary>
        public string ChainID { private set; get; }

        /// <summary>
        /// Gets the underying data.
        /// </summary>
        public Tdata? Data { private set; get; }

        /// <summary>
        /// Constructor for the <see cref="AddressIdentifyableData{Tdata}"/> struct.
        /// </summary>
        /// <param name="chainID">Chain identifier of the data.</param>
        /// <param name="address">Address used for the request.</param>
        /// <param name="data">Underlying data.</param>
        public AddressIdentifyableData(string chainID, string address, Tdata? data)
        {
            Address = address;
            ChainID = chainID;
            Data = data;
        }
    }

    /// <summary>
    /// Struct used to track the origin of a list of some typed data. Mainly used for API requests to track the request's origin.
    /// </summary>
    /// <typeparam name="Tdata">Type of the data.</typeparam>
    public struct AddressIdentifyableDataArray<Tdata>
    {
        /// <summary>
        /// Gets the list of addresses that were used for the request.
        /// </summary>
        public List<string> Addresses { private set; get; }

        /// <summary>
        /// Gets the chain identifier of the data.
        /// </summary>
        public string ChainID { private set; get; }

        /// <summary>
        /// Gets the <see cref="List{Tdata}"/> containing the underlying data.
        /// </summary>
        public List<Tdata>? Data { private set; get; }

        /// <summary>
        /// Constructor for the <see cref="AddressIdentifyableDataArray{Tdata}"/> struct.
        /// </summary>
        /// <param name="chainID">Chain identifier of the data.</param>
        /// <param name="addresses">List of addresses that were used for the request.</param>
        /// <param name="data"><see cref="List{Tdata}"/> containing the underlying data.</param>
        public AddressIdentifyableDataArray(string chainID, List<string> addresses, List<Tdata>? data)
        {
            Addresses = addresses;
            ChainID = chainID;
            Data = data;
        }
    }

    /// <summary>
    /// Struct used to track the origin of some typed data. Mainly used for API requests to track the request's origin.
    /// </summary>
    /// <typeparam name="Tdata">Type of the data.</typeparam>
    public struct IdentifyableData<Tdata>
    {
        /// <summary>
        /// Gets a generic identifier used for this request.
        /// </summary>
        public string Identifier { private set; get; }

        /// <summary>
        /// Gets the underying data.
        /// </summary>
        public Tdata? Data { private set; get; }

        /// <summary>
        /// Constructor for the <see cref="IdentifyableData{Tdata}"/> struct.
        /// </summary>
        /// <param name="identifier">Chain identifier of the data.</param>
        /// <param name="data">Underlying data.</param>
        public IdentifyableData(string identifier, Tdata? data)
        {
            Identifier = identifier;
            Data = data;
        }
    }
}
