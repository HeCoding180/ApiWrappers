using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ApiWrappers.DexScreenerAPI
{
    /// <summary>
    /// Link struct. Used in the following responses:<br>
    /// TokenProfile_Latest<br>
    /// TokenProfile_Boosted
    /// </summary>
    public struct Link
    {
        public string type { set; get; }
        public string label { set; get; }
        public string url { set; get; }
    }

    /// <summary>
    /// Struct that holds basic information about number of boosts.
    /// </summary>
    public struct Boosts
    {
        public int active { set; get; }
    }

    /// <summary>
    /// Struct used for time series values
    /// </summary>
    public struct TimeSeries<T>
    {
        /// <summary>
        /// Value in a 5 minute interval.
        /// </summary>
        [JsonPropertyName("m5")]
        public T FiveMinute { set ; get; }

        /// <summary>
        /// Value in a 1 hour interval.
        /// </summary>
        [JsonPropertyName("h1")]
        public T OneHour { set; get; }

        /// <summary>
        /// Value in a 6 hour interval.
        /// </summary>
        [JsonPropertyName("h6")]
        public T SixHour { set; get; }

        /// <summary>
        /// Value in a 24 hour interval.
        /// </summary>
        [JsonPropertyName("h24")]
        public T FullDay { set; get; }
    }

    /// <summary>
    /// Struct that contains information about number of transactions split in buy and sell transactions.
    /// </summary>
    public struct Transactions
    {
        /// <summary>
        /// Number of buy transactions.
        /// </summary>
        public int buys { set; get; }

        /// <summary>
        /// Nubmer of sell transactions.
        /// </summary>
        public int sells { set; get; }
    }

    /// <summary>
    /// Struct that contains information about a token's liquidity.
    /// </summary>
    public struct Liquidity
    {
        /// <summary>
        /// Liquidity in USD.
        /// </summary>
        public float usd { set; get; }
        [JsonPropertyName("base")]
        public float baseValue { set; get; }
        public float quote { set; get; }
    }

    /// <summary>
    /// Struct that contains information about a linked social media account.
    /// </summary>
    public struct SocialLinkage
    {
        /// <summary>
        /// Name of the social media plaform.
        /// </summary>
        [JsonPropertyName("type")]
        public string platform { set; get; }

        /// <summary>
        /// Link to the social media account.
        /// </summary>
        public string url { set; get; }
    }

    /// <summary>
    /// Struct that holds basic token information.
    /// </summary>
    public struct Token
    {
        public string address { set; get; }
        public string name { set; get; }
        public string symbol { set; get; }
    }

    /// <summary>
    /// Token links to website, socials, et cetera.
    /// </summary>
    public struct TokenLinks
    {
        /// <summary>
        /// URL to the token's icon.
        /// </summary>
        [JsonPropertyName("imageUrl")]
        public string iconUrl { set; get; }

        /// <summary>
        /// URL to the token's header image.
        /// </summary>
        [JsonPropertyName("header")]
        public string headerUrl { set; get; }

        /// <summary>
        /// URL to the token's open graph image (detailed header image with market cap, liquidity, etc.)
        /// </summary>
        [JsonPropertyName("openGraph")]
        public string openGraphUrl { set; get; }

        /// <summary>
        /// List of linked websites.
        /// </summary>
        public List<Link> websites { set; get; }

        /// <summary>
        /// List of linked social media accounts.
        /// </summary>
        public List<Link> socials { set; get; }
    }
}
