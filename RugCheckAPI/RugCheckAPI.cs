using ApiWrappers.RateLimitation;
using ApiWrappers.Wrapper.HTTP;
using Org.BouncyCastle.Asn1.Ocsp;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ApiWrappers.RugCheckAPI
{
    public partial class RugCheckAPI
    {
        //   ---   Public Constants   ---

        public const string API_BASE_ADDRESS = "https://api.rugcheck.xyz";

        //   ---   Public Properties   ---

        /// <summary>
        /// Logger for the RugCheck API class.
        /// </summary>
        public readonly ILogger log;

        /// <summary>
        /// Solana account used for 
        /// </summary>
        public Account SolanaWalletAccount { private set; get; }

        /// <summary>
        /// API wrapper object for the RugCheck API class.
        /// </summary>
        public HttpApiWrapper Wrapper { get; }

        //   ---   Constructors   ---

        /// <summary>
        /// Constructor for the RugCheck API class.
        /// </summary>
        /// <param name="solanaPrivateKey">Base58-encoded private key of a Solana account used for authentication.</param>
        /// <param name="logger">Logger instance for the API class.</param>
        public RugCheckAPI(string solanaPrivateKey, ILogger logger)
        {
            log = logger ?? throw new ArgumentNullException(nameof(logger));
            
            SolanaWalletAccount = SolnetUtil.FromSecretKey(solanaPrivateKey);
            Wrapper = new HttpApiWrapper(API_BASE_ADDRESS, logger);

            _initializeRateLimiters();
        }

        //   ---   Public Methods   ---

        /// <summary>
        /// Method used to get registered .token domains.<br/>
        /// Rate-limit: 3 requests per second.<br/>
        /// For more info see: 
        /// <see href="https://api.rugcheck.xyz/swagger/index.html#/Domains/get_domains">API Reference</see>
        /// </summary>
        /// <param name="page">Page number for results.</param>
        /// <param name="limit">Max number of results that should be returned</param>
        /// <param name="verified">Filter to only domains with verification set. Use null if results should be unfiltered.</param>
        /// <returns>Awaitable task containing a List of <see cref="Models.dto.DomainResponse"/> structs.</returns>
        public async Task<List<Models.dto.DomainResponse>?> GetRegisteredDomains(int page, int limit, bool? verified)
        {
            string requestUri = $"v1/domains?page={page}&limit={limit}";

            if (verified is not null)
            {
                requestUri += $"&verified={verified}";
            }

            return await _getRegisteredDomains_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetJsonObjectAsync<List<Models.dto.DomainResponse>>(requestUri));
        }

        /// <summary>
        /// Method used to get registered .token domains.<br/>
        /// Rate-limit: 3 requests per second.<br/>
        /// For more info see: 
        /// <see href="https://api.rugcheck.xyz/swagger/index.html#/Domains/get_domains_data_csv">API Reference</see>
        /// </summary>
        /// <param name="verified">Filter to only domains with verification set. Use null if results should be unfiltered.</param>
        /// <returns>Awaitable task containing a list of domain strings split by newline characters.</returns>
        public async Task<string> GetRegisteredDomainsCsv(bool? verified)
        {
            string requestUri = "v1/domains/data.csv";

            if (verified is not null)
            {
                requestUri += $"?verified={verified}";
            }

            return await _getRegisteredDomainsCsv_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetStringAsync(requestUri));
        }

        /// <summary>
        /// Method used to lookup a domain address by its name.<br/>
        /// Rate-limit: 3 requests per second.<br/>
        /// For more info see: 
        /// <see href="https://api.rugcheck.xyz/swagger/index.html#/Domains/get_domains_lookup__id_">API Reference</see>
        /// </summary>
        /// <param name="name">Name of the domain. Format: name.tld</param>
        /// <returns>Awaitable task containing the domain address.</returns>
        public async Task<string> LookupDomainAddressByName(string name)
        {
            string requestUri = $"v1/domains/lookup/{name}";

            return await _lookupDomainAddressByName_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetStringAsync(requestUri));
        }

        /// <summary>
        /// Method used to get related domain records of a domain by its name.<br/>
        /// Rate-limit: 3 requests per second.<br/>
        /// For more info see: 
        /// <see href="https://api.rugcheck.xyz/swagger/index.html#/Domains/get_domains_records__id_">API Reference</see>
        /// </summary>
        /// <param name="name">Name of the domain. Format: name.tld</param>
        /// <returns><b>Unknown</b>; Bare response contents as a <see langword="string"/>.</returns>
        public async Task<string> GetDomainRecordsByName(string name)
        {
            string requestUri = $"v1/domains/lookup/{name}";

            return await _getDomainRecordsByName_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetStringAsync(requestUri));
        }

        /// <summary>
        /// Method used to get user leaderboard rankings.<br/>
        /// Rate-limit: 3 requests per second.<br/>
        /// For more info see: 
        /// <see href="https://api.rugcheck.xyz/swagger/index.html#/General/get_leaderboard">API Reference</see>
        /// </summary>
        /// <param name="page">Page number for results.</param>
        /// <param name="limit">Max number of results that should be returned</param>
        /// <returns>Awaitable task containing <see cref="Models.rugcheck_api.User"/> structs of the top leaderboard users.</returns>
        public async Task<List<Models.rugcheck_api.User>?> GetLeaderboard(int page, int limit)
        {
            string requestUri = $"v1/leaderboard?page={page}&limit={limit}";

            return await _getLeaderboard_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetJsonObjectAsync<List<Models.rugcheck_api.User>>(requestUri));
        }

        /// <summary>
        /// Method used to get the maintenance status of the server.<br/>
        /// Rate-limit: 3 requests per second.<br/>
        /// For more info see: 
        /// <see href="https://api.rugcheck.xyz/swagger/index.html#/General/get_maintenance">API Reference</see>
        /// </summary>
        /// <returns>Awaitable task containing the response.</returns>
        public async Task<string> GetMaintenanceStatus()
        {
            const string REQUEST_URI = "v1/maintenance";

            return await _getMaintenanceStatus_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetStringAsync(REQUEST_URI));
        }

        /// <summary>
        /// <b>Does not seem to be implemented!</b><br/>
        /// Method used to ping the service.<br/>
        /// For more info see: 
        /// <see href="https://api.rugcheck.xyz/swagger/index.html#/General/get_ping">API Reference</see>
        /// </summary>
        /// <returns>Awaitable task containing the response as a <see langword="string"/>.</returns>
        [Obsolete]
        public async Task<string> PingService()
        {
            const string REQUEST_URI = "v1/ping";

            return await _pingService_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetStringAsync(REQUEST_URI));
        }

        /// <summary>
        /// <b>Does not seem to be implemented!</b><br/>
        /// Method used to get statistics about RPC node usage.<br/>
        /// For more info see: 
        /// <see href="https://api.rugcheck.xyz/swagger/index.html#/General/get_rpc_stats">API Reference</see>
        /// </summary>
        /// <returns>Awaitable task containing the response as a <see langword="string"/>.</returns>
        [Obsolete]
        public async Task<string> GetRpcStats()
        {
            const string REQUEST_URI = "v1/rpc/stats";

            return await _getRpcStats_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetStringAsync(REQUEST_URI));
        }

        /// <summary>
        /// Method used to get recently detected tokens.<br/>
        /// Rate-limit: 3 requests per second.<br/>
        /// For more info see: 
        /// <see href="https://api.rugcheck.xyz/swagger/index.html#/Stats/get_stats_new_tokens">API Reference</see>
        /// </summary>
        /// <returns>Awaitable task returning a list of <see cref="Models.rugcheck_api.Token"/> structs of newly discovered tokens.</returns>
        public async Task<List<Models.rugcheck_api.Token>?> GetNewTokens()
        {
            const string REQUEST_URI = "v1/stats/new_tokens";

            return await _getNewTokens_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetJsonObjectAsync<List<Models.rugcheck_api.Token>>(REQUEST_URI));
        }

        /// <summary>
        /// Method used to get the most viewed tokens from the last 24 hours.<br/>
        /// Rate-limit: 3 requests per second.<br/>
        /// For more info see: 
        /// <see href="https://api.rugcheck.xyz/swagger/index.html#/Stats/get_stats_recent">API Reference</see>
        /// </summary>
        /// <returns>Awaitable task returning a <see cref="Models.dto.TokenInfoAgg"/> struct</returns>
        public async Task<List<Models.dto.TokenInfoAgg>?> GetRecentTokens()
        {
            const string REQUEST_URI = "v1/stats/recent";

            return await _getRecentTokens_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetJsonObjectAsync<List<Models.dto.TokenInfoAgg>>(REQUEST_URI));
        }

        /// <summary>
        /// Metod used to get most voted for tokens from the past 24 hours.<br/>
        /// For more info see: 
        /// <see href="https://api.rugcheck.xyz/swagger/index.html#/Stats/get_stats_trending">API Reference</see>
        /// </summary>
        /// <returns>Awaitable task returning a list of <see cref="Models.services.TrendingToken"/> structs.</returns>
        public async Task<List<Models.services.TrendingToken>?> GetTredingTokens()
        {
            const string REQUEST_URI = "v1/stats/trending";

            return await _getTrendingTokens_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetJsonObjectAsync<List<Models.services.TrendingToken>>(REQUEST_URI));
        }

        /// <summary>
        /// Method used to get tokens that were recently verified.<br/>
        /// For more info see: 
        /// <see href="https://api.rugcheck.xyz/swagger/index.html#/Stats/get_stats_verified">API Reference</see>
        /// </summary>
        /// <returns>Awaitable task returning a list of <see cref="Models.rugcheck_api.VerifiedToken"/> structs.</returns>
        public async Task<List<Models.rugcheck_api.VerifiedToken>?> GetRecentlyVerifiedTokens()
        {
            const string REQUEST_URI = "v1/stats/verified";

            return await _getRecentlyVerifiedTokens_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetJsonObjectAsync<List<Models.rugcheck_api.VerifiedToken>>(REQUEST_URI));
        }

        /// <summary>
        /// Method used to submit a token for verification.<br/>
        /// For more info see: 
        /// <see href="https://api.rugcheck.xyz/swagger/index.html#/Tokens/post_tokens_verify">API Reference</see>
        /// </summary>
        /// <param name="requestBody">Request body data for the token verification request.</param>
        /// <returns>Awaitable task returning a boolean representing the success state of the verification request. <see langword="true"/>: Token verification submission was successful. Otherwise <see langword="false"/>.</returns>
        public async Task<bool> VerifyToken(Models.dto.TokenVerificationRequest requestBody)
        {
            const string REQUEST_URI = "v1/tokens/verify";

            string requestBodyStr = JsonSerializer.Serialize(requestBody);

            Models.dto.SuccessResponse? response;

            try
            {
                response = await _verifyToken_rateLimiter.EncapsulateRequestAsync(() => Wrapper.PostJsonObjectRequestAsync<Models.dto.SuccessResponse>(REQUEST_URI, requestBodyStr, true));
                return response.HasValue ? response.Value.Ok : false;
            }
            catch (ApiHttpExeption ex)
            {
                // Re-throw all exceptions but Unauthorized (401)
                if (ex.StatusCode != System.Net.HttpStatusCode.Unauthorized)
                {
                    throw;
                }
            }

            // Invoke API credential renewal, as it might be out of date
            log.LogWarning("Http request was unauthorized, requesting credential renewal.");
            Wrapper.RequestCredentialRenewal();

            // Try the same request again
            response = await _verifyToken_rateLimiter.EncapsulateRequestAsync(() => Wrapper.PostJsonObjectRequestAsync<Models.dto.SuccessResponse>(REQUEST_URI, requestBodyStr, true));
            return response.HasValue ? response.Value.Ok : false;
        }

        /// <summary>
        /// Method used to check if a token is eligible for verification.<br/>
        /// For more info see: 
        /// <see href="https://api.rugcheck.xyz/swagger/index.html#/Tokens/post_tokens_verify_eligible">API Reference</see>
        /// </summary>
        /// <param name="mint">Mint of the token, whose eligibility should be checked.</param>
        /// <returns>Awaitable task returning a <see cref="Models.dto.TokenEligibilityResponse"/> struct containing the token's eligibility status.</returns>
        public async Task<Models.dto.TokenEligibilityResponse?> CheckTokenEligibility(string mint)
        {
            const string REQUEST_URI = "v1/tokens/verify/eligible";

            Models.dto.TokenEligibilityRequest requestBody = new Models.dto.TokenEligibilityRequest();
            requestBody.Mint = mint;
            string requestBodyStr = JsonSerializer.Serialize(requestBody);

            try
            {
                return await _checkTokenEligibility_rateLimitter.EncapsulateRequestAsync(() => Wrapper.PostJsonObjectRequestAsync<Models.dto.TokenEligibilityResponse>(REQUEST_URI, requestBodyStr, true));
            }
            catch (ApiHttpExeption ex)
            {
                // Re-throw all exceptions but Unauthorized (401)
                if (ex.StatusCode != System.Net.HttpStatusCode.Unauthorized)
                {
                    throw;
                }
            }

            // Invoke API credential renewal, as it might be out of date
            log.LogWarning("Http request was unauthorized, requesting credential renewal.");
            Wrapper.RequestCredentialRenewal();

            // Try the same request again
            return await _checkTokenEligibility_rateLimitter.EncapsulateRequestAsync(() => Wrapper.PostJsonObjectRequestAsync<Models.dto.TokenEligibilityResponse>(REQUEST_URI, requestBodyStr, true));
        }

        /// <summary>
        /// Method used to get the transaction for a token verification.<br/>
        /// For more info see: 
        /// <see href="https://api.rugcheck.xyz/swagger/index.html#/Tokens/post_tokens_verify_transaction">API Reference</see>
        /// </summary>
        /// <param name="transactionRequest">Mint of the token, for which the insider graph should be generated.</param>
        /// <returns>Awaitable task returning a <see langword="string"/> containing the <see cref="Models.dto.TokenVerificationTransactionRequest"/> struct's "Transaction" member's value
        /// or <see langword="null"/> if the response was <see langword="null"/>.</returns>
        public async Task<string?> GetTokenVerificationTransaction(Models.dto.TokenVerificationTransactionRequest transactionRequest)
        {
            const string REQUEST_URI = "v1/tokens/verify/transaction";

            string requestBody = JsonSerializer.Serialize(transactionRequest);

            Models.dto.TokenVerificationTransactionResponse? response;

            try
            {
                response = await _getTokenVerificationTransaction_rateLimitter.EncapsulateRequestAsync(() => Wrapper.PostJsonObjectRequestAsync<Models.dto.TokenVerificationTransactionResponse>(REQUEST_URI, requestBody, true));
                return response.HasValue ? response.Value.Transaction : null;
            }
            catch (ApiHttpExeption ex)
            {
                // Re-throw all exceptions but Unauthorized (401)
                if (ex.StatusCode != System.Net.HttpStatusCode.Unauthorized)
                {
                    throw;
                }
            }

            // Invoke API credential renewal, as it might be out of date
            log.LogWarning("Http request was unauthorized, requesting credential renewal.");
            Wrapper.RequestCredentialRenewal();

            // Try the same request again
            response = await _getTokenVerificationTransaction_rateLimitter.EncapsulateRequestAsync(() => Wrapper.PostJsonObjectRequestAsync<Models.dto.TokenVerificationTransactionResponse>(REQUEST_URI, requestBody, true));
            return response.HasValue ? response.Value.Transaction : null;
        }

        /// <summary>
        /// Method used to get an insider graph for a given token mint.<br/>
        /// Rate-limit: 3 requests per second.<br/>
        /// For more info see: 
        /// <see href="https://api.rugcheck.xyz/swagger/index.html#/Tokens/get_tokens__id__insiders_graph">API Reference</see>
        /// </summary>
        /// <param name="mint">Mint of the token, for which the insider graph should be generated.</param>
        /// <returns>Awaitable task returning a <see langword="string"/> containing the bare response ( <b>Format is unknown</b> )</returns>
        public async Task<string?> GetTokenInsiderGraph(string mint)
        {
            string requestUri = $"v1/tokens/{mint}/insiders/graph";

            return await _getTokenInsidersGraph_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetStringAsync(requestUri));
        }

        /// <summary>
        /// Method used to get the full token report of a token by its mint.<br/>
        /// Rate-limit: 3 requests per second.<br/>
        /// For more info see: 
        /// <see href="https://api.rugcheck.xyz/swagger/index.html#/Tokens/get_tokens__id__report">API Reference</see>
        /// </summary>
        /// <param name="mint">Mint of the token, for which the full token report should be fetched.</param>
        /// <returns>Awaitable task returning a <see cref="Models.rugcheck_api.TokenCheck"/> struct containing the token report information.</returns>
        public async Task<Models.rugcheck_api.TokenCheck?> GetTokenReport(string mint)
        {
            string request = $"v1/tokens/{mint}/report";

            return await _getTokenReport_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetJsonObjectAsync<Models.rugcheck_api.TokenCheck>(request));
        }

        /// <summary>
        /// Method used to report a suspicious token.<br/>
        /// For more info see: 
        /// <see href="https://api.rugcheck.xyz/swagger/index.html#/Tokens/post_tokens__id__report">API Reference</see>
        /// </summary>
        /// <param name="mint">Mint of the token, which should be reported.</param>
        /// <returns>Awaitable task returning a boolean. <see langword="true"/>: Token Report was successful. Otherwise <see langword="false"/>.</returns>
        public async Task<bool> ReportToken(string mint)
        {
            string requestUri = $"v1/tokens/{mint}/report";

            Models.dto.SuccessResponse? response;

            try
            {
                response = await _reportToken_rateLimitter.EncapsulateRequestAsync(() => Wrapper.PostJsonObjectRequestAsync<Models.dto.SuccessResponse>(requestUri, null, true));
                return response.HasValue ? response.Value.Ok : false;
            }
            catch (ApiHttpExeption ex)
            {
                // Re-throw all exceptions but Unauthorized (401)
                if (ex.StatusCode != System.Net.HttpStatusCode.Unauthorized)
                {
                    throw;
                }
            }

            // Invoke API credential renewal, as it might be out of date
            log.LogWarning("Http request was unauthorized, requesting credential renewal.");
            Wrapper.RequestCredentialRenewal();

            // Try the same request again
            response = await _reportToken_rateLimitter.EncapsulateRequestAsync(() => Wrapper.PostJsonObjectRequestAsync<Models.dto.SuccessResponse>(requestUri, null, true));
            return response.HasValue ? response.Value.Ok : false;
        }

        /// <summary>
        /// Method used to get the token report summary of a token by its mint.<br/>
        /// For more info see: 
        /// <see href="https://api.rugcheck.xyz/swagger/index.html#/Tokens/get_tokens__id__report_summary">API Reference</see>
        /// </summary>
        /// <param name="mint">Mint of the token, for which the token report summary should be fetched.</param>
        /// <param name="cacheOnly">Only return cached reports.</param>
        /// <returns>Awaitable task returning a <see cref="Models.dto.TokenCheckSummary"/> struct containing the token report summary.</returns>
        public async Task<Models.dto.TokenCheckSummary?> GetTokenReportSummary(string mint, string? cacheOnly)
        {
            string requestUri = $"v1/tokens/{mint}/report/summary";

            if (cacheOnly is not null)
            {
                requestUri += $"?cacheOnly={cacheOnly}";
            }

            return await _getTokenReportSummary_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetJsonObjectAsync<Models.dto.TokenCheckSummary>(requestUri));
        }

        /// <summary>
        /// Method used to get the LP vaults of a token by its mint address.<br/>
        /// For more info see: 
        /// <see href="https://api.rugcheck.xyz/swagger/index.html#/Vaults/get_tokens__id__lockers">API Reference</see>
        /// </summary>
        /// <param name="mint">Mint of the token, for which the token LP vaults should be fetched.</param>
        /// <returns>Awaitable task returning a <see cref="Models.dto.VaultResponse"/> struct.</returns>
        public async Task<Models.dto.VaultResponse?> GetTokenLpVaults(string mint)
        {
            string requestUri = $"v1/tokens/{mint}/lockers";

            try
            {
                return await _getTokenLpVaults_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetJsonObjectAsync<Models.dto.VaultResponse>(requestUri, true));
            }
            catch (ApiHttpExeption ex)
            {
                // Re-throw all exceptions but Unauthorized (401)
                if (ex.StatusCode != System.Net.HttpStatusCode.Unauthorized)
                {
                    throw;
                }
            }

            // Invoke API credential renewal, as it might be out of date
            log.LogWarning("Http request was unauthorized, requesting credential renewal.");
            Wrapper.RequestCredentialRenewal();

            // Try the same request again
            return await _getTokenLpVaults_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetJsonObjectAsync<Models.dto.VaultResponse>(requestUri, true));
        }

        /// <summary>
        /// Method used to get the LP vaults of a token by its mint address from flux locker.<br/>
        /// For more info see: 
        /// <see href="https://api.rugcheck.xyz/swagger/index.html#/Vaults/get_tokens__id__lockers_flux">API Reference</see>
        /// </summary>
        /// <param name="mint">Mint of the token, for which the token LP vaults should be fetched.</param>
        /// <returns>Awaitable task returning a <see cref="Models.dto.VaultResponse"/> struct.</returns>
        public async Task<Models.dto.VaultResponse?> GetFluxTokenLpVaults(string mint)
        {
            string requestUri = $"v1/tokens/{mint}/lockers/flux";

            try
            {
                return await _getFluxTokenLpVaults_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetJsonObjectAsync<Models.dto.VaultResponse>(requestUri, true));
            }
            catch (ApiHttpExeption ex)
            {
                // Re-throw all exceptions but Unauthorized (401)
                if (ex.StatusCode != System.Net.HttpStatusCode.Unauthorized)
                {
                    throw;
                }
            }

            // Invoke API credential renewal, as it might be out of date
            log.LogWarning("Http request was unauthorized, requesting credential renewal.");
            Wrapper.RequestCredentialRenewal();

            // Try the same request again
            return await _getFluxTokenLpVaults_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetJsonObjectAsync<Models.dto.VaultResponse>(requestUri, true));
        }

        /// <summary>
        /// Method used to submit a vote on a token mint.<br/>
        /// For more info see: 
        /// <see href="https://api.rugcheck.xyz/swagger/index.html#/Votes/post_tokens__id__vote">API Reference</see>
        /// </summary>
        /// <returns>Awaitable task returning a <see cref="Models.dto.VoteResponse"/> struct containing the updated voting statistics of the mint.</returns>
        public async Task<Models.dto.VoteResponse?> SubmitTokenVote(string mint, bool voteSide)
        {
            string requestUri = $"v1/tokens/{mint}/vote";

            Models.dto.VoteRequest requestBody = new Models.dto.VoteRequest();
            requestBody.Mint = mint;
            requestBody.Side = voteSide;

            return await _submitTokenVote_rateLimitter.EncapsulateRequestAsync(() => Wrapper.PostJsonObjectRequestAsync<Models.dto.VoteResponse>(requestUri, JsonSerializer.Serialize(requestBody), true));
        }

        /// <summary>
        /// Method used to get voting statistics for a token mint.<br/>
        /// For more info see: 
        /// <see href="https://api.rugcheck.xyz/swagger/index.html#/Votes/get_tokens__id__votes">API Reference</see>
        /// </summary>
        /// <returns>Awaitable task returning a <see cref="Models.dto.VoteResponse"/> struct containing the voting statistics of the mint.</returns>
        public async Task<Models.dto.VoteResponse?> GetTokenVotes(string mint)
        {
            string requestUri = $"v1/tokens/{mint}/votes";

            return await _getTokenVotes_rateLimitter.EncapsulateRequestAsync(() => Wrapper.GetJsonObjectAsync<Models.dto.VoteResponse>(requestUri));
        }
    }
}
