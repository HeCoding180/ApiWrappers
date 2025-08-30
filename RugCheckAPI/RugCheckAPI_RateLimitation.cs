using ApiWrappers.RateLimitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiWrappers.RugCheckAPI
{
    partial class RugCheckAPI
    {
        //   ---   Private Properties (Static)   ---

        private static bool _rateLimitersInitialized = false;

        /// <summary>
        /// Rate limiter for the "Get registered domains" RugCheck API request.
        /// Rate-limit: 3 requests per second.
        /// </summary>
        private static ApiRateLimitter _getRegisteredDomains_rateLimitter { set; get; } = new ApiRateLimitter(3, ApiRateLimitPeriod.Second);

        /// <summary>
        /// Rate limiter for the "Get registered domains CSV" RugCheck API request.
        /// Rate-limit: 3 requests per second.
        /// </summary>
        private static ApiRateLimitter _getRegisteredDomainsCsv_rateLimitter { set; get; } = new ApiRateLimitter(3, ApiRateLimitPeriod.Second);

        /// <summary>
        /// Rate limiter for the "Lookup domain address by name" RugCheck API request.
        /// Rate-limit: 3 requests per second.
        /// </summary>
        private static ApiRateLimitter _lookupDomainAddressByName_rateLimitter { set; get; } = new ApiRateLimitter(3, ApiRateLimitPeriod.Second);

        /// <summary>
        /// Rate limiter for the "Get domain records by name" RugCheck API request.
        /// Rate-limit: 3 requests per second.
        /// </summary>
        private static ApiRateLimitter _getDomainRecordsByName_rateLimitter { set; get; } = new ApiRateLimitter(3, ApiRateLimitPeriod.Second);

        /// <summary>
        /// Rate limiter for the "Get leaderboard" RugCheck API request.
        /// Rate-limit: 3 requests per second.
        /// </summary>
        private static ApiRateLimitter _getLeaderboard_rateLimitter { set; get; } = new ApiRateLimitter(3, ApiRateLimitPeriod.Second);

        /// <summary>
        /// Rate limiter for the "Get maintenance status" RugCheck API request.
        /// Rate-limit: 3 requests per second.
        /// </summary>
        private static ApiRateLimitter _getMaintenanceStatus_rateLimitter { set; get; } = new ApiRateLimitter(3, ApiRateLimitPeriod.Second);

        /// <summary>
        /// Rate limiter for the "Ping service" RugCheck API request.
        /// Rate-limit: 3 requests per second.
        /// </summary>
        private static ApiRateLimitter _pingService_rateLimitter { set; get; } = new ApiRateLimitter(3, ApiRateLimitPeriod.Second);

        /// <summary>
        /// Rate limiter for the "Get RPC stats" RugCheck API request.
        /// Rate-limit: 3 requests per second.
        /// </summary>
        private static ApiRateLimitter _getRpcStats_rateLimitter { set; get; } = new ApiRateLimitter(3, ApiRateLimitPeriod.Second);

        /// <summary>
        /// Rate limiter for the "Get new tokens" RugCheck API request.
        /// Rate-limit: 3 requests per second.
        /// </summary>
        private static ApiRateLimitter _getNewTokens_rateLimitter { set; get; } = new ApiRateLimitter(3, ApiRateLimitPeriod.Second);

        /// <summary>
        /// Rate limiter for the "Get recent tokens" RugCheck API request.
        /// Rate-limit: 3 requests per second.
        /// </summary>
        private static ApiRateLimitter _getRecentTokens_rateLimitter { set; get; } = new ApiRateLimitter(3, ApiRateLimitPeriod.Second);

        /// <summary>
        /// Rate limiter for the "Get trending tokens" RugCheck API request.
        /// Rate-limit: 3 requests per second.
        /// </summary>
        private static ApiRateLimitter _getTrendingTokens_rateLimitter { set; get; } = new ApiRateLimitter(3, ApiRateLimitPeriod.Second);

        /// <summary>
        /// Rate limiter for the "Get verified tokens" RugCheck API request.
        /// Rate-limit: 3 requests per second.
        /// </summary>
        private static ApiRateLimitter _getRecentlyVerifiedTokens_rateLimitter { set; get; } = new ApiRateLimitter(3, ApiRateLimitPeriod.Second);

        /// <summary>
        /// Rate limiter for the "Submit token for verification" RugCheck API request.
        /// Rate-limit: 3 requests per second.
        /// </summary>
        private static ApiRateLimitter _verifyToken_rateLimiter { set; get; } = new ApiRateLimitter(3, ApiRateLimitPeriod.Second);

        /// <summary>
        /// Rate limiter for the "Check token eligibility" RugCheck API request.
        /// Rate-limit: 3 requests per second.
        /// </summary>
        private static ApiRateLimitter _checkTokenEligibility_rateLimitter { set; get; } = new ApiRateLimitter(3, ApiRateLimitPeriod.Second);

        /// <summary>
        /// Rate limiter for the "Submit token verification transaction" RugCheck API request.
        /// Rate-limit: 3 requests per second.
        /// </summary>
        private static ApiRateLimitter _getTokenVerificationTransaction_rateLimitter { set; get; } = new ApiRateLimitter(3, ApiRateLimitPeriod.Second);

        /// <summary>
        /// Rate limiter for the "Get token insiders graph" RugCheck API request.
        /// Rate-limit: 3 requests per second.
        /// </summary>
        private static ApiRateLimitter _getTokenInsidersGraph_rateLimitter { set; get; } = new ApiRateLimitter(3, ApiRateLimitPeriod.Second);

        /// <summary>
        /// Rate limiter for the "Get token report" RugCheck API request.
        /// Rate-limit: 3 requests per second.
        /// </summary>
        private static ApiRateLimitter _getTokenReport_rateLimitter { set; get; } = new ApiRateLimitter(3, ApiRateLimitPeriod.Second);

        /// <summary>
        /// Rate limiter for the "Submit token for report" RugCheck API request.
        /// Rate-limit: 3 requests per second.
        /// </summary>
        private static ApiRateLimitter _reportToken_rateLimitter { set; get; } = new ApiRateLimitter(3, ApiRateLimitPeriod.Second);

        /// <summary>
        /// Rate limiter for the "Get token check summary" RugCheck API request.
        /// Rate-limit: 3 requests per second.
        /// </summary>
        private static ApiRateLimitter _getTokenReportSummary_rateLimitter { set; get; } = new ApiRateLimitter(3, ApiRateLimitPeriod.Second);

        /// <summary>
        /// Rate limiter for the "GetTokenLpVaults" RugCheck API request.
        /// Rate-limit: 3 requests per second.
        /// </summary>
        private static ApiRateLimitter _getTokenLpVaults_rateLimitter { set; get; } = new ApiRateLimitter(3, ApiRateLimitPeriod.Second);

        /// <summary>
        /// Rate limiter for the "GetFluxTokenLpVaults" RugCheck API request.
        /// Rate-limit: 3 requests per second.
        /// </summary>
        private static ApiRateLimitter _getFluxTokenLpVaults_rateLimitter { set; get; } = new ApiRateLimitter(3, ApiRateLimitPeriod.Second);

        /// <summary>
        /// Rate limiter for the "Vote on token" RugCheck API request.
        /// Rate-limit: 3 requests per second.
        /// </summary>
        private static ApiRateLimitter _submitTokenVote_rateLimitter { set; get; } = new ApiRateLimitter(3, ApiRateLimitPeriod.Second);

        /// <summary>
        /// Rate limiter for the "Get token votes" RugCheck API request.
        /// Rate-limit: 3 requests per second.
        /// </summary>
        private static ApiRateLimitter _getTokenVotes_rateLimitter { set; get; } = new ApiRateLimitter(3, ApiRateLimitPeriod.Second);

        //   ---   Private Methods (Static)   ---

        private static void _initializeRateLimiters()
        {
            if (_rateLimitersInitialized)
                return;
            
            _getRegisteredDomains_rateLimitter.SubscribeEventCallbacks();
            _getRegisteredDomainsCsv_rateLimitter.SubscribeEventCallbacks();
            _lookupDomainAddressByName_rateLimitter.SubscribeEventCallbacks();
            _getDomainRecordsByName_rateLimitter.SubscribeEventCallbacks();
            _getLeaderboard_rateLimitter.SubscribeEventCallbacks();
            _getMaintenanceStatus_rateLimitter.SubscribeEventCallbacks();
            _pingService_rateLimitter.SubscribeEventCallbacks();
            _getRpcStats_rateLimitter.SubscribeEventCallbacks();
            _getNewTokens_rateLimitter.SubscribeEventCallbacks();
            _getRecentTokens_rateLimitter.SubscribeEventCallbacks();
            _getTrendingTokens_rateLimitter.SubscribeEventCallbacks();
            _getRecentlyVerifiedTokens_rateLimitter.SubscribeEventCallbacks();
            _verifyToken_rateLimiter.SubscribeEventCallbacks();
            _checkTokenEligibility_rateLimitter.SubscribeEventCallbacks();
            _getTokenVerificationTransaction_rateLimitter.SubscribeEventCallbacks();
            _getTokenInsidersGraph_rateLimitter.SubscribeEventCallbacks();
            _getTokenReport_rateLimitter.SubscribeEventCallbacks();
            _reportToken_rateLimitter.SubscribeEventCallbacks();
            _getTokenReportSummary_rateLimitter.SubscribeEventCallbacks();
            _getTokenLpVaults_rateLimitter.SubscribeEventCallbacks();
            _getFluxTokenLpVaults_rateLimitter.SubscribeEventCallbacks();
            _submitTokenVote_rateLimitter.SubscribeEventCallbacks();
            _getTokenVotes_rateLimitter.SubscribeEventCallbacks();
        }
    }
}