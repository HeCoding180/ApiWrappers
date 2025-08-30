using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Transactions;

namespace ApiWrappers.DexScreenerAPI
{
    public struct TokenProfile_Latest
    {
        /// <summary>
        /// URL of the token's DEX Screener page.
        /// </summary>
        public string url { set; get; }

        /// <summary>
        /// Token's chain ID.
        /// </summary>
        public string chainId { set; get; }

        /// <summary>
        /// Token's address.
        /// </summary>
        public string tokenAddress { set; get; }

        /// <summary>
        /// URL to the token's icon.
        /// </summary>
        [JsonPropertyName("icon")]
        public string iconUrl { set; get; }

        /// <summary>
        /// URL to the token's header image.
        /// </summary>
        [JsonPropertyName("header")]
        public string headerUrl { set; get; }

        /// <summary>
        /// Token description.
        /// </summary>
        public string description;
        public List<Link> links { set; get; }
    }

    public struct TokenProfile_Boosted
    {
        /// <summary>
        /// URL of the token's DEX Screener page.
        /// </summary>
        public string url { set; get; }

        /// <summary>
        /// Token's chain ID.
        /// </summary>
        public string chainId { set; get; }

        /// <summary>
        /// Token's address.
        /// </summary>
        public string tokenAddress { set; get; }
        public int amount { set; get; }
        public int totalAmount { set; get; }

        /// <summary>
        /// URL to the token's icon.
        /// </summary>
        [JsonPropertyName("icon")]
        public string iconUrl { set; get; }

        /// <summary>
        /// URL to the token's header image.
        /// </summary>
        [JsonPropertyName("header")]
        public string headerUrl { set; get; }

        /// <summary>
        /// Token description.
        /// </summary>
        public string description { set; get; }
        public List<Link> links { set; get; }
    }

    public struct Token_OrdersPaid
    {
        /// <summary>
        /// Order type.
        /// </summary>
        public string type { set; get; }

        /// <summary>
        /// Order status.
        /// </summary>
        public string status { set; get; }

        /// <summary>
        /// Order's payment timestamp.
        /// </summary>
        public int paymentTimestamp { set; get; }
    }

    /// <summary>
    /// Versioned pair response. Contains version and list of pairs.
    /// </summary>
    public struct VersionedPairs
    {
        public string schemaVersion { set; get; }
        public List<Pair> pairs { set; get; }
    }

    /// <summary>
    /// Simple pair response.
    /// </summary>
    public struct Pair
    {
        /// <summary>
        /// Chain ID of the token.
        /// </summary>
        public string chainId { set; get; }

        /// <summary>
        /// DEX ID of the token.
        /// </summary>
        public string dexId { set; get; }

        /// <summary>
        /// URL of the token's DEX Screener page.
        /// </summary>
        public string url { set; get; }

        /// <summary>
        /// Pair address of the token.
        /// </summary>
        public string pairAddress { set; get; }
        public List<string> labels { set; get; }
        public Token baseToken { set; get; }
        public Token quoteToken { set; get; }
        public string priceNative { set; get; }

        /// <summary>
        /// Current price in USD.
        /// </summary>
        public string priceUsd { set; get; }

        /// <summary>
        /// Number of transactions (values) over different time intervals (keys).
        /// </summary>
        [JsonPropertyName("txns")]
        public TimeSeries<Transactions> transactions { set; get; }

        /// <summary>
        /// Token order volume (values) over different time intervals (keys).
        /// </summary>
        public TimeSeries<float> volume { set; get; }

        /// <summary>
        /// Token price change (values) over different time intervals (keys).
        /// </summary>
        public TimeSeries<float> priceChange { set; get; }

        /// <summary>
        /// Current token liquidity.
        /// </summary>
        public Liquidity liquidity { set; get; }

        /// <summary>
        /// Current FDV (fully diluted valuation) of the token.
        /// </summary>
        public float fdv { set; get; }

        /// <summary>
        /// Current market cap of the token.
        /// </summary>
        public float marketCap { set; get; }

        /// <summary>
        /// Timestamp (milliseconds) of when the pair was created at.
        /// </summary>
        public long pairCreatedAt { set; get; }

        /// <summary>
        /// Token links.
        /// </summary>
        public TokenLinks info { set; get; }

        /// <summary>
        /// Token boosts information.
        /// </summary>
        public Boosts boosts { set; get; }

        /// <summary>
        /// Public property to check if the data is valid.
        /// </summary>
        [JsonIgnore]
        public bool IsDataValid
        {
            get
            {
                bool isValid = true;

                isValid &= priceUsd is not null;

                return isValid;
            }
        }
    }
}
