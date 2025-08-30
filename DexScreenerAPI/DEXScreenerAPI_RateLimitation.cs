using ApiWrappers.RateLimitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiWrappers.DexScreenerAPI
{
    partial class DexScreenerAPI
    {
        //   ---   Private Properties (Static)   ---

        /// <summary>
        /// Gets whether the rate limiters are initialized.
        /// </summary>
        private static bool _rateLimitersInitialized = false;

        //   ---   Private Properties   ---

        /// <summary>
        /// Rate limiter for the "Get the latest token profiles" DEX Screener API request.
        /// </summary>
        private static ApiRateLimitter _latestTokenProfiles_rateLimitter { set; get; } = new ApiRateLimitter(60, ApiRateLimitPeriod.Minute);

        /// <summary>
        /// Rate limiter for the "Get the latest boosted tokens" DEX Screener API request.
        /// </summary>
        private static ApiRateLimitter _latestBoostedTokens_rateLimitter { set; get; } = new ApiRateLimitter(60, ApiRateLimitPeriod.Minute);

        /// <summary>
        /// Rate limiter for the "Get the tokens with most active boosts" DEX Screener API request.
        /// </summary>
        private static ApiRateLimitter _mostActiveBoostedTokens_rateLimitter { set; get; } = new ApiRateLimitter(60, ApiRateLimitPeriod.Minute);

        /// <summary>
        /// Rate limiter for the "Check orders paid for of token" DEX Screener API request.
        /// </summary>
        private static ApiRateLimitter _tokenOrdersPaid_rateLimitter { set; get; } = new ApiRateLimitter(60, ApiRateLimitPeriod.Minute);

        /// <summary>
        /// Rate limiter for the "Get one or multiple pairs by chain and pair address" DEX Screener API request.
        /// </summary>
        private static ApiRateLimitter _pairByPairAddress_rateLimitter { set; get; } = new ApiRateLimitter(300, ApiRateLimitPeriod.Minute);

        /// <summary>
        /// Rate limiter for the "Search for pairs matching query" DEX Screener API request.
        /// </summary>
        private static ApiRateLimitter _pairsMatchingQuery_rateLimitter { set; get; } = new ApiRateLimitter(300, ApiRateLimitPeriod.Minute);

        /// <summary>
        /// Rate limiter for the "Get the pools of a given token address" DEX Screener API request.
        /// </summary>
        private static ApiRateLimitter _poolsByTokenAddress_rateLimitter { set; get; } = new ApiRateLimitter(300, ApiRateLimitPeriod.Minute);

        /// <summary>
        /// Rate limiter for the "Get one or multiple pairs by token address" DEX Screener API request.
        /// </summary>
        private static ApiRateLimitter _pairsByTokenAddresses_rateLimitter { set; get; } = new ApiRateLimitter(300, ApiRateLimitPeriod.Minute);

        //   ---   Private Methods   ---

        /// <summary>
        /// Method used to initialize the rate limiters.
        /// </summary>
        private static void _initializeRateLimiters()
        {
            if (_rateLimitersInitialized)
                return;

            _latestTokenProfiles_rateLimitter.SubscribeEventCallbacks();
            _latestBoostedTokens_rateLimitter.SubscribeEventCallbacks();
            _mostActiveBoostedTokens_rateLimitter.SubscribeEventCallbacks();
            _tokenOrdersPaid_rateLimitter.SubscribeEventCallbacks();
            _pairByPairAddress_rateLimitter.SubscribeEventCallbacks();
            _pairsMatchingQuery_rateLimitter.SubscribeEventCallbacks();
            _poolsByTokenAddress_rateLimitter.SubscribeEventCallbacks();
            _pairsByTokenAddresses_rateLimitter.SubscribeEventCallbacks();
        }
    }
}