using DexDataMine.TokenInfo;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace ApiWrappers.DexScreenerAPI
{
    /// <summary>
    /// DEX Screener API class.
    /// </summary>
    public partial class DexScreenerAPI
    {
        //   ---   Public Constants   ---

        /// <summary>
        /// Base address for DEXScreener API requests.
        /// </summary>
        public const string API_BASE_ADDRESS = "https://api.dexscreener.com";

        /// <summary>
        /// Maximum API request size for requests containing multiple addresses.
        /// </summary>
        public const int MAX_API_REQUEST_ADDRESSES = 30;

        //   ---   Public Properties   ---

        /// <summary>
        /// Logger for the DEXScreenerAPI class.
        /// </summary>
        public readonly ILogger log;

        /// <summary>
        /// API wrapper object for the DexScreener API.
        /// </summary>
        public HttpApiWrapper Wrapper { get; }

        //   ---   Constructors   ---

        /// <summary>
        /// Constructor for the DEXScreenerAPI class.
        /// </summary>
        /// <param name="logger">Logger instance for the API class.</param>
        public DexScreenerAPI(ILogger logger)
        {
            log = logger ?? throw new ArgumentNullException(nameof(logger));
            Wrapper = new HttpApiWrapper(API_BASE_ADDRESS, logger);
        }

        //   ---   Private Methods   ---

        /// <summary>
        /// Method used to check a list of pairs from a response.
        /// </summary>
        /// <param name="data">Data that is to be checked.</param>
        /// <returns></returns>
        private List<Pair>? CheckPairList(List<Pair?>? data)
        {
            List<Pair?>? uncheckedPairList = data;
            List<Pair>? checkedPairList;

            if (uncheckedPairList is null)
            {
                log.LogWarning("Pair list response is null, returning null");
                return null;
            }

            checkedPairList = new List<Pair>();

            foreach (Pair? responsePair in uncheckedPairList)
            {
                if (!responsePair.HasValue)
                {
                    log.LogWarning("Null pair found in response, skipping pair!");
                    continue;
                }

                if (!responsePair.Value.IsDataValid)
                {
                    log.LogWarning("Invalid pair struct found in response, skipping pair!");
                    continue;
                }

                checkedPairList.Add(responsePair.Value);
            }

            return checkedPairList;
        }

        //   ---   Public Methods   ---

        /// <summary>
        /// <para>
        /// Asynchronous "Get the latest token profiles" DEX Screener API request.<br/>
        /// Rate-limit: 60 requests per minute.
        /// </para>
        /// For more information see: <see href="https://docs.dexscreener.com/api/reference#token-profiles-latest-v1">API Reference</see>
        /// </summary>
        /// <returns>An awaitable task resulting in a list of <see cref="TokenProfile_Latest"/> structs containing the response data.</returns>
        public async Task<List<TokenProfile_Latest?>?> GetLatestTokenProfiles()
        {
            const string REQUEST = "token-profiles/latest/v1";

            return await _latestTokenProfiles_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetJsonObjectAsync<List<TokenProfile_Latest?>>(REQUEST));
        }

        /// <summary>
        /// <para>
        /// Asynchronous "Get the latest boosted tokens" DEX Screener API request.<br/>
        /// Rate-limit: 60 requests per minute.
        /// </para>
        /// For more information see: <see href="https://docs.dexscreener.com/api/reference#token-boosts-latest-v1">API Reference</see>
        /// </summary>
        /// <returns>An awaitable task resulting in a list of <see cref="TokenProfile_Boosted"/> structs containing the response data.</returns>
        public async Task<List<TokenProfile_Boosted?>?> GetLatestBoostedTokens()
        {
            const string REQUEST = "token-boosts/latest/v1";

            return await _latestBoostedTokens_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetJsonObjectAsync<List<TokenProfile_Boosted?>>(REQUEST));
        }

        /// <summary>
        /// <para>
        /// Asynchronous "Get the tokens with most active boosts" DEX Screener API request.<br/>
        /// Rate-limit: 60 requests per minute.
        /// </para>
        /// For more information see: <see href="https://docs.dexscreener.com/api/reference#token-boosts-top-v1">API Reference</see>
        /// </summary>
        /// <returns>An awaitable task resulting in a list of <see cref="TokenProfile_Boosted"/> structs containing the response data.</returns>
        public async Task<List<TokenProfile_Boosted?>?> GetMostActiveBoostedTokens()
        {
            const string REQUEST = "token-boosts/top/v1";

            return await _mostActiveBoostedTokens_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetJsonObjectAsync<List<TokenProfile_Boosted?>>(REQUEST));
        }

        /// <summary>
        /// <para>
        /// Asynchronous "Check orders paid for of token" DEX Screener API request.<br/>
        /// Rate-limit: 60 requests per minute.
        /// </para>
        /// For more information see: <see href="https://docs.dexscreener.com/api/reference#orders-v1-chainid-tokenaddress">API Reference</see>
        /// </summary>
        /// <param name="chainId">Chain id for which the request should be made.</param>
        /// <param name="tokenAddress">Token address for which the request should be made.</param>
        /// <returns>An awaitable task resulting in a list of <see cref="Token_OrdersPaid"/> structs containing the response data with request identification information.</returns>
        public async Task<AddressIdentifyableData<List<Token_OrdersPaid>>> GetTokenOrdersPaid(string chainId, string tokenAddress)
        {
            string request = $"orders/v1/{chainId}/{tokenAddress}";

            try
            {
                // Run the request through the rate limiter
                List<Token_OrdersPaid>? response = await _tokenOrdersPaid_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetJsonObjectAsync<List<Token_OrdersPaid>>(request));

                return new AddressIdentifyableData<List<Token_OrdersPaid>>(chainId, tokenAddress, response);
            }
            catch (Exception ex)
            {
                throw new ApiWrappedException(chainId, tokenAddress, ex);
            }
        }

        /// <summary>
        /// <para>
        /// Asynchronous "Get one or multiple pairs by chain and pair address" DEX Screener API request.<br/>
        /// Rate-limit: 300 requests per minute.
        /// </para>
        /// For more information see: <see href="https://docs.dexscreener.com/api/reference#latest-dex-pairs-chainid-pairid">API Reference</see>
        /// </summary>
        /// <param name="chainId">Chain id for which the request should be made.</param>
        /// <param name="pairAddress">Pair address for which the request should be made.</param>
        /// <returns>An awaitable task resulting in a <see cref="IdentifyableData{VersionedPairs}"/> struct containing the response data together with request identification information.</returns>
        public async Task<AddressIdentifyableData<VersionedPairs>> GetPairByPairAddress(string chainId, string pairAddress)
        {
            string request = $"latest/dex/pairs/{chainId}/{pairAddress}";

            try
            {
                // Run the request through the rate limiter
                VersionedPairs response = await _pairByPairAddress_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetJsonObjectAsync<VersionedPairs>(request));

                return new AddressIdentifyableData<VersionedPairs>(chainId, pairAddress, response);
            }
            catch (Exception ex)
            {
                throw new ApiWrappedException(chainId, pairAddress, ex);
            }
        }

        /// <summary>
        /// <para>
        /// Asynchronous "Search for pairs matching query" DEX Screener API request.<br/>
        /// Rate-limit: 300 requests per minute.
        /// </para>
        /// For more information see: <see href="https://docs.dexscreener.com/api/reference#latest-dex-search">API Reference</see>
        /// </summary>
        /// <param name="responseMethod">Delegate that is executed once the reponse from the DEX Screener API is received and deserialized.</param>
        /// <param name="query">Search query for the token pairs.</param>
        /// <returns>An awaitable task resulting in a <see cref="IdentifyableData{VersionedPairs}"/> struct containing the response data together with request identification information.</returns>
        public async Task<IdentifyableData<VersionedPairs>> GetPairsMatchingQuery(string query)
        {
            string request = $"latest/dex/search?q={HttpUtility.UrlEncode(query)}";

            try
            {
                // Run the request through the rate limiter
                VersionedPairs response = await _pairsMatchingQuery_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetJsonObjectAsync<VersionedPairs>(request));

                return new IdentifyableData<VersionedPairs>(query, response);
            }
            catch (Exception ex)
            {
                throw new ApiWrappedException(query, ex);
            }
        }

        /// <summary>
        /// <para>
        /// Asynchronous "Get the pools of a given token address" DEX Screener API request.<br/>
        /// Rate-limit: 300 requests per minute.
        /// </para>
        /// For more information see: <see href="https://docs.dexscreener.com/api/reference#token-pairs-v1-chainid-tokenaddress">API Reference</see>
        /// </summary>
        /// <param name="chainId">Chain id for which the request should be made.</param>
        /// <param name="tokenAddress">Token address for which the request should be made.</param>
        /// <returns>An awaitable task resulting in a list of <see cref="Pair"/> structs containing the response data together with request identification information.</returns>
        public async Task<AddressIdentifyableData<List<Pair>>> GetPoolsByTokenAddress(string chainId, string tokenAddress)
        {
            string request = $"token-pairs/v1/{chainId}/{tokenAddress}";

            try
            {
                // Run the request through the rate limiter
                List<Pair?>? response = await _poolsByTokenAddress_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetJsonObjectAsync<List<Pair?>>(request));

                // Check the response for invalid pairs
                List<Pair>? checkedPairList = CheckPairList(response);

                return new AddressIdentifyableData<List<Pair>>(chainId, tokenAddress, checkedPairList);
            }
            catch (Exception ex)
            {
                throw new ApiWrappedException(chainId, tokenAddress, ex);
            }
        }

        /// <summary>
        /// <para>
        /// Asynchronous "Get one or multiple pairs by token address" DEX Screener API request.<br/>
        /// Rate-limit: 300 requests per minute.
        /// </para>
        /// For more information see: <see href="https://docs.dexscreener.com/api/reference#tokens-v1-chainid-tokenaddresses">API Reference</see>
        /// </summary>
        /// <param name="chainId">Chain id for which the request should be made.</param>
        /// <param name="tokenAddresses">List of token addresses for which the request should be made.</param>
        /// <returns>An awaitable task resulting in a <see cref="IdentifyableDataArray{T}"/> struct containing the response data (list of <see cref="Pair"/> structs) together with request identification information.</returns>
        public async Task<AddressIdentifyableDataArray<Pair>> GetPairsByTokenAddresses(string chainId, IEnumerable<string> tokenAddresses)
        {
            try
            {
                int addressCount = tokenAddresses.Count();

                // Check if list of token addresses is below the request limit of 30 token addresses.
                if (addressCount <= 30)
                {
                    string request = $"tokens/v1/{chainId}/";
                    bool isAppendedToken = false;

                    foreach (string tokenAddress in tokenAddresses)
                    {
                        if (isAppendedToken) request += ",";
                        else isAppendedToken = true;

                        request += tokenAddress;
                    }

                    // Run the request through the rate limiter
                    List<Pair?>? response = await _pairsByTokenAddresses_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetJsonObjectAsync<List<Pair?>>(request));

                    // Check the response for invalid pairs
                    List<Pair>? checkedPairList = CheckPairList(response);

                    return new AddressIdentifyableDataArray<Pair>(chainId, [.. tokenAddresses], checkedPairList);
                }
                // Too many token addresses, return null
                else throw new ArgumentOutOfRangeException($"Received too many token addresses: {addressCount} (maximum is 30).");
            }
            catch (Exception ex)
            {
                throw new ApiWrappedException(chainId, tokenAddresses, ex);
            }
        }
    }
}
