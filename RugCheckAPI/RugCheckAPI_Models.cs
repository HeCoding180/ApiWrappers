using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ApiWrappers.RugCheckAPI
{
    namespace Models
    {
        namespace dto
        {
            /// <summary>
            /// dto.AuthMessage model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct AuthMessage(string publicKey)
            {
                [JsonPropertyName("message")]
                public string Message { set; get; } = "Sign-in to Rugcheck.xyz";

                [JsonPropertyName("timestamp")]
                public long Timestamp { set; get; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                [JsonPropertyName("publicKey")]
                public string PublicKey { set; get; } = publicKey;

                //   ---   Method Overrides   ---

                /// <summary>
                /// Method used to convert this object to a JSON string.
                /// </summary>
                /// <returns>JSON string of this object.</returns>
                public override readonly string ToString()
                {
                    return JsonSerializer.Serialize(this);
                }
            }

            /// <summary>
            /// dto.AuthRequest model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct AuthRequest(dto.AuthMessage baseMessage, Models.SignatureData signatureData)
            {
                [JsonPropertyName("message")]
                public dto.AuthMessage Message { set; get; } = baseMessage;

                [JsonPropertyName("signature")]
                public SignatureData Signature { set; get; } = signatureData;

                [JsonPropertyName("wallet")]
                public string WalletPublicKey { set; get; } = baseMessage.PublicKey;

                //   ---   Method Overrides   ---

                /// <summary>
                /// Method used to convert this object to a JSON string.
                /// </summary>
                /// <returns>JSON string of this object.</returns>
                public override readonly string ToString()
                {
                    return JsonSerializer.Serialize(this);
                }
            }

            /// <summary>
            /// dto.AuthResponse model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct AuthResponse
            {
                /// <summary>
                /// String containing the JWT API key.
                /// </summary>
                [JsonPropertyName("token")]
                public string Key { set; get; }
            }

            /// <summary>
            /// dto.DomainResponse model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct DomainResponse
            {
                [JsonPropertyName("tokens")]
                public rugcheck_api.VerifiedTokenSimple Tokens { set; get; }
            }

            /// <summary>
            /// dto.ErrorResponse model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct ErrorResponse
            {
                [JsonPropertyName("error")]
                public string ErrorMessage { set; get; }
            }

            /// <summary>
            /// dto.Pong model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct Pong
            {
                [JsonPropertyName("message")]
                public string Message { set; get; }
            }

            /// <summary>
            /// dto.SuccessResponse model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct SuccessResponse
            {
                [JsonPropertyName("ok")]
                public bool Ok { set; get; }
            }

            /// <summary>
            /// dto.TokenCheckSummary model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct TokenCheckSummary
            {
                [JsonPropertyName("risks")]
                public List<rugcheck_api.Risk> Risks { set; get; }

                [JsonPropertyName("score")]
                public int Score { set; get; }

                [JsonPropertyName("score_normalised")]
                public int NormalisedScore { set; get; }

                [JsonPropertyName("tokenProgram")]
                public string TokenProgram { set; get; }

                [JsonPropertyName("tokenType")]
                public string TokenType { set; get; }
            }

            /// <summary>
            /// dto.TokenEligibilityRequest model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct TokenEligibilityRequest
            {
                [JsonPropertyName("mint")]
                public string Mint { set; get; }
            }

            /// <summary>
            /// dto.TokenEligibilityResponse model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct TokenEligibilityResponse
            {
                [JsonPropertyName("criteria")]
                public rugcheck_api.EligibilityResponse Criteria { set; get; }

                [JsonPropertyName("eligible")]
                public bool Eligible { set; get; }

                [JsonPropertyName("mint")]
                public string Mint { set; get; }
            }

            /// <summary>
            /// dto.TokenInfoAgg model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct TokenInfoAgg
            {
                [JsonPropertyName("metadata")]
                public rugcheck_api.TokenMetadata Metadata { set; get; }

                [JsonPropertyName("mint")]
                public string Mint { set; get; }

                [JsonPropertyName("score")]
                public int Score { set; get; }

                [JsonPropertyName("user_visits")]
                public int UserVisits { set; get; }

                [JsonPropertyName("visits")]
                public int Visits { set; get; }
            }

            /// <summary>
            /// dto.TokenVerificationData model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct TokenVerificationData
            {
                [JsonPropertyName("dataIntegrityAccepted")]
                public bool DataIntegrityAccepted { set; get; }

                [JsonPropertyName("description")]
                public string Description { set; get; }

                [JsonPropertyName("links")]
                public Dictionary<string, string> Links { set; get; }

                [JsonPropertyName("solDomain")]
                public string SolDomain { set; get; }

                [JsonPropertyName("termsAccepted")]
                public bool TermsAccepted { set; get; }
            }

            /// <summary>
            /// dto.TokenVerificationRequest model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct TokenVerificationRequest
            {
                [JsonPropertyName("data")]
                public dto.TokenVerificationData Data { set; get; }

                [JsonPropertyName("mint")]
                public string Mint { set; get; }

                [JsonPropertyName("payer")]
                public string Payer { set; get; }

                [JsonPropertyName("signature")]
                public string Signature { set; get; }
            }

            /// <summary>
            /// dto.TokenVerificationTransactionRequest model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct TokenVerificationTransactionRequest
            {
                [JsonPropertyName("data")]
                public dto.TokenVerificationData Data { set; get; }

                [JsonPropertyName("mint")]
                public string Mint { set; get; }

                [JsonPropertyName("payer")]
                public string Payer { set; get; }

                [JsonPropertyName("priority_fee")]
                public int PriorityFee { set; get; }
            }

            /// <summary>
            /// dto.TokenVerificationTransactionResponse model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct TokenVerificationTransactionResponse
            {
                [JsonPropertyName("transaction")]
                public string Transaction { set; get; }
            }

            /// <summary>
            /// dto.VaultResponse model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct VaultResponse
            {
                [JsonPropertyName("lockers")]
                public Dictionary<string, rugcheck_api.Locker> Lockers { set; get; }

                [JsonPropertyName("total")]
                public dto.VaultResponseSummary Total { set; get; }
            }

            /// <summary>
            /// dto.VaultResponseSummary model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct VaultResponseSummary
            {
                [JsonPropertyName("pct")]
                public double PCT { set; get; }

                [JsonPropertyName("totalUSDC")]
                public double TotalUSDC { set; get; }
            }

            /// <summary>
            /// dto.VoteRequest model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            /// <param name="mint">Mint of the token this vote request is submitted for.</param>
            /// <param name="side">Vote side.</param>
            public struct VoteRequest(string mint, bool side)
            {
                [JsonPropertyName("mint")]
                public string Mint { set; get; } = mint;

                [JsonPropertyName("side")]
                public bool Side { set; get; } = side;
            }

            /// <summary>
            /// dto.VoteResponse model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct VoteResponse
            {
                [JsonPropertyName("down")]
                public int Down { set; get; }

                [JsonPropertyName("up")]
                public int Up { set; get; }

                [JsonPropertyName("userVoted")]
                public bool UserVoted { set; get; }
            }
        }

        namespace rugcheck_api
        {
            /// <summary>
            /// rugcheck_api.CreatorToken model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct CreatorToken
            {
                [JsonPropertyName("createdAt")]
                public string CreatedAt { set; get; }

                [JsonPropertyName("marketCap")]
                public double MC { set; get; }

                [JsonPropertyName("mint")]
                public string Mint { set; get; }
            }

            /// <summary>
            /// rugcheck_api.EligibilityResponse model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct EligibilityResponse
            {
                [JsonPropertyName("created_recently")]
                public bool CreatedRecently { set; get; }

                [JsonPropertyName("duplicate")]
                public bool IsDuplicate { set; get; }

                [JsonPropertyName("exists")]
                public bool Exists { set; get; }

                [JsonPropertyName("freeze_authority_set")]
                public bool IsFreezeAuthoritySet { set; get; }

                [JsonPropertyName("liquidity_unlocked")]
                public bool IsLiquidityUnlocked { set; get; }

                [JsonPropertyName("metadata_missing")]
                public bool IsMetadataMissing { set; get; }

                [JsonPropertyName("mint_authority_set")]
                public bool IsMintAuthoritySet { set; get; }

                [JsonPropertyName("risk_score")]
                public int RiskScore { set; get; }
            }

            /// <summary>
            /// rugcheck_api.FileMetadata model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct FileMetadata
            {
                [JsonPropertyName("description")]
                public string Description { set; get; }

                [JsonPropertyName("image")]
                public string Image { set; get; }

                [JsonPropertyName("name")]
                public string Name { set; get; }

                [JsonPropertyName("symbol")]
                public string Symbol { set; get; }
            }

            /// <summary>
            /// rugcheck_api.InsiderNetwork model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct InsiderNetwork
            {
                [JsonPropertyName("activeAccounts")]
                public int ActiveAccounts { set; get; }

                [JsonPropertyName("id")]
                public string ID { set; get; }

                [JsonPropertyName("size")]
                public int Size { set; get; }

                [JsonPropertyName("tokenAmount")]
                public ulong TokenAmount { set; get; }

                [JsonPropertyName("type")]
                public string Type { set; get; }
            }

            /// <summary>
            /// rugcheck_api.KnownAccount model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct KnownAccount
            {
                [JsonPropertyName("name")]
                public string Name { set; get; }

                [JsonPropertyName("type")]
                public string Type { set; get; }
            }

            /// <summary>
            /// rugcheck_api.Locker model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct Locker
            {
                [JsonPropertyName("owner")]
                public string Owner { set; get; }

                [JsonPropertyName("programID")]
                public string ProgramID { set; get; }

                [JsonPropertyName("tokenAccount")]
                public string TokenAccount { set; get; }

                [JsonPropertyName("type")]
                public string Type { set; get; }

                [JsonPropertyName("unlockDate")]
                public int UnlockDate { set; get; }

                [JsonPropertyName("uri")]
                public string URI { set; get; }

                [JsonPropertyName("usdcLocked")]
                public double USDC_Locked { set; get; }
            }

            /// <summary>
            /// rugcheck_api.Market model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct Market
            {
                [JsonPropertyName("liquidityA")]
                public string LiquidityA { set; get; }

                [JsonPropertyName("liquidityAAccount")]
                public others.LiquidityAccount LiquidityAAccount { set; get; }

                [JsonPropertyName("liquidityB")]
                public string LiquidityB { set; get; }

                [JsonPropertyName("liquidityBAccount")]
                public others.LiquidityAccount LiquidityBAccount { set; get; }

                [JsonPropertyName("lp")]
                public rugcheck_api.MarketLP LP { set; get; }

                [JsonPropertyName("marketType")]
                public string MarketType { set; get; }

                [JsonPropertyName("mintA")]
                public string MintA { set; get; }

                [JsonPropertyName("mintAAccount")]
                public others.MintAccount MintAAccount { set; get; }

                [JsonPropertyName("mintB")]
                public string MintB { set; get; }

                [JsonPropertyName("mintBAccount")]
                public others.MintAccount MintBAccount { set; get; }

                [JsonPropertyName("mintLP")]
                public string MintLp { set; get; }

                [JsonPropertyName("mintLPAccount")]
                public others.MintAccount MintLpAccount { set; get; }

                [JsonPropertyName("pubkey")]
                public string PublicKey { set; get; }
            }

            /// <summary>
            /// rugcheck_api.MarketLP model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct MarketLP
            {
                [JsonPropertyName("base")]
                public double Base { set; get; }

                [JsonPropertyName("baseMint")]
                public string BaseMint { set; get; }

                [JsonPropertyName("basePrice")]
                public double BasePrice { set; get; }

                [JsonPropertyName("baseUSD")]
                public double BaseUSD { set; get; }

                [JsonPropertyName("currentSupply")]
                public long CurrentSupply { set; get; }

                [JsonPropertyName("holders")]
                public List<rugcheck_api.TokenHolder> Holders { set; get; }

                [JsonPropertyName("lpCurrentSupply")]
                public long LpCurrentSupply { set; get; }

                [JsonPropertyName("lpLocked")]
                public long LpLocked { set; get; }

                [JsonPropertyName("lpLockedPct")]
                public double LpLockedPct { set; get; }

                [JsonPropertyName("lpLockedUSD")]
                public double LpLockedUSD { set; get; }

                [JsonPropertyName("lpMaxSupply")]
                public long LpMaxSupply { set; get; }

                [JsonPropertyName("lpMint")]
                public string LpMint { set; get; }

                [JsonPropertyName("lpTotalSupply")]
                public long LpTotalSupply { set; get; }

                [JsonPropertyName("lpUnlocked")]
                public long LpUnlocked { set; get; }

                [JsonPropertyName("pctReserve")]
                public double PctReserve { set; get; }

                [JsonPropertyName("pctSupply")]
                public double PctSupply { set; get; }

                [JsonPropertyName("quote")]
                public double Quote { set; get; }

                [JsonPropertyName("quoteMint")]
                public string QuoteMint { set; get; }

                [JsonPropertyName("quotePrice")]
                public double QuotePrice { set; get; }

                [JsonPropertyName("quoteUSD")]
                public double QuoteUSD { set; get; }

                [JsonPropertyName("reserveSupply")]
                public long ReserveSupply { set; get; }

                [JsonPropertyName("tokenSupply")]
                public long TokenSupply { set; get; }

                [JsonPropertyName("totalTokensUnlocked")]
                public long TotalTokensUnlocked { set; get; }
            }

            /// <summary>
            /// rugcheck_api.Risk model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct Risk
            {
                [JsonPropertyName("description")]
                public string Description { set; get; }

                [JsonPropertyName("level")]
                public string Level { set; get; }

                [JsonPropertyName("name")]
                public string Name { set; get; }

                [JsonPropertyName("score")]
                public int Score { set; get; }

                [JsonPropertyName("value")]
                public string Value { set; get; }
            }

            /// <summary>
            /// rugcheck_api.Token model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct Token
            {
                [JsonPropertyName("createAt")]
                public string CreateAt { set; get; }

                [JsonPropertyName("creator")]
                public string Creator { set; get; }

                [JsonPropertyName("decimals")]
                public int Decimals { set; get; }

                [JsonPropertyName("events")]
                public List<rugcheck_api.TokenEvent> Events { set; get; }

                [JsonPropertyName("freezeAuthority")]
                public string FreezeAuthority { set; get; }

                [JsonPropertyName("mint")]
                public string Mint { set; get; }

                [JsonPropertyName("mintAuthority")]
                public string MintAuthority { set; get; }

                [JsonPropertyName("program")]
                public string Program { set; get; }

                [JsonPropertyName("symbol")]
                public string Symbol { set; get; }

                [JsonPropertyName("updatedAt")]
                public string UpdatedAt { set; get; }
            }

            /// <summary>
            /// rugcheck_api.TokenCheck model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct TokenCheck
            {
                [JsonPropertyName("creator")]
                public string Creator { set; get; }

                [JsonPropertyName("creatorTokens")]
                public List<rugcheck_api.CreatorToken>? CreatorTokens { set; get; }

                [JsonPropertyName("detectedAt")]
                public string DetectedAt { set; get; }

                [JsonPropertyName("events")]
                public List<rugcheck_api.TokenEvent>? Events { set; get; }

                [JsonPropertyName("fileMeta")]
                public rugcheck_api.FileMetadata? FileMetadata { set; get; }

                [JsonPropertyName("freezeAuthority")]
                public string FreezeAuthority { set; get; }

                [JsonPropertyName("graphInsidersDetected")]
                public int GraphInsidersDetected { set; get; }

                [JsonPropertyName("insiderNetworks")]
                public List<rugcheck_api.InsiderNetwork>? InsiderNetworks { set; get; }

                [JsonPropertyName("knownAccounts")]
                public Dictionary<string, rugcheck_api.KnownAccount>? KnownAccounts { set; get; }

                [JsonPropertyName("lockerOwners")]
                public Dictionary<string, bool>? LockerOwners { set; get; }

                [JsonPropertyName("lockers")]
                public Dictionary<string, rugcheck_api.Locker>? Lockers { set; get; }

                [JsonPropertyName("markets")]
                public List<rugcheck_api.Market>? Markets { set; get; }

                [JsonPropertyName("mint")]
                public string Mint { set; get; }

                [JsonPropertyName("mintAuthority")]
                public string MintAuthority { set; get; }

                [JsonPropertyName("price")]
                public double Price { set; get; }

                [JsonPropertyName("risks")]
                public List<rugcheck_api.Risk>? Risks { set; get; }

                [JsonPropertyName("rugged")]
                public bool Rugged { set; get; }

                [JsonPropertyName("score")]
                public int Score { set; get; }

                [JsonPropertyName("score_normalised")]
                public int NormalisedScore { set; get; }

                [JsonPropertyName("token")]
                public others.MintAccount? Token { set; get; }

                [JsonPropertyName("tokenMeta")]
                public rugcheck_api.TokenMetadata? TokenMetadata { set; get; }

                [JsonPropertyName("tokenProgram")]
                public string TokenProgram { set; get; }

                [JsonPropertyName("tokenType")]
                public string TokenType { set; get; }

                [JsonPropertyName("token_extensions")]
                public string TokenExtensions { set; get; }

                [JsonPropertyName("topHolders")]
                public List<rugcheck_api.TokenHolder>? TopHolders { set; get; }

                [JsonPropertyName("totalHolders")]
                public int TotalHolders { set; get; }

                [JsonPropertyName("totalLPProviders")]
                public int TotalLpProviders { set; get; }

                [JsonPropertyName("totalMarketLiquidity")]
                public double TotalMarketLiquidity { set; get; }

                [JsonPropertyName("transferFee")]
                public TransferFeeData? TransferFee { set; get; }

                [JsonPropertyName("verification")]
                public rugcheck_api.VerifiedToken? Verification { set; get; }
            }

            /// <summary>
            /// rugcheck_api.TokenEvent model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct TokenEvent
            {
                [JsonPropertyName("createdAt")]
                public string CreatedAt { set; get; }

                [JsonPropertyName("event")]
                public int EventID { set; get; }

                [JsonPropertyName("newValue")]
                public string NewValue { set; get; }

                [JsonPropertyName("oldValue")]
                public string OldValue { set; get; }
            }

            /// <summary>
            /// rugcheck_api.TokenHolder model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct TokenHolder
            {
                [JsonPropertyName("address")]
                public string Address { set; get; }

                [JsonPropertyName("amount")]
                public long Amount { set; get; }

                [JsonPropertyName("decimals")]
                public int Decimals { set; get; }

                [JsonPropertyName("insider")]
                public bool IsInsider { set; get; }

                [JsonPropertyName("owner")]
                public string Owner { set; get; }

                [JsonPropertyName("pct")]
                public double Pct { set; get; }

                [JsonPropertyName("uiAmount")]
                public double UiAmount { set; get; }

                [JsonPropertyName("uiAmountString")]
                public string UiAmountStr { set; get; }
            }

            /// <summary>
            /// rugcheck_api.TokenMetadata model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct TokenMetadata
            {
                [JsonPropertyName("mutable")]
                public bool IsMutable { set; get; }

                [JsonPropertyName("name")]
                public string Name { set; get; }

                [JsonPropertyName("symbol")]
                public string Symbol { set; get; }

                [JsonPropertyName("updateAuthority")]
                public string UpdateAuthority { set; get; }

                [JsonPropertyName("uri")]
                public string URI { set; get; }
            }

            /// <summary>
            /// rugcheck_api.User model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct User
            {
                [JsonPropertyName("username")]
                public string Username { set; get; }

                [JsonPropertyName("votes")]
                public int Votes { set; get; }

                [JsonPropertyName("weight")]
                public int Weight { set; get; }

                [JsonPropertyName("wins")]
                public int Wins { set; get; }
            }

            /// <summary>
            /// rugcheck_api.VerifiedToken model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct VerifiedToken
            {
                [JsonPropertyName("description")]
                public string Description { set; get; }

                [JsonPropertyName("jup_strict")]
                public bool IsJupStrict { set; get; }

                [JsonPropertyName("jup_verified")]
                public bool IsJupVerified { set; get; }

                [JsonPropertyName("links")]
                public List<rugcheck_api.VerifiedTokenLinks> Links { set; get; }

                [JsonPropertyName("mint")]
                public string Mint { set; get; }

                [JsonPropertyName("name")]
                public string Name { set; get; }

                [JsonPropertyName("payer")]
                public string Payer { set; get; }

                [JsonPropertyName("symbol")]
                public string Symbol { set; get; }
            }

            /// <summary>
            /// rugcheck_api.VerifiedTokenLinks model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct VerifiedTokenLinks
            {
                [JsonPropertyName("provider")]
                public string Provider { set; get; }

                [JsonPropertyName("value")]
                public string Value { set; get; }
            }

            /// <summary>
            /// rugcheck_api.VerifiedTokenSimple model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct VerifiedTokenSimple
            {
                [JsonPropertyName("createdAt")]
                public string CreatedAt { set; get; }

                [JsonPropertyName("domain")]
                public string Domain { set; get; }

                [JsonPropertyName("mint")]
                public string Mint { set; get; }

                [JsonPropertyName("name")]
                public string Name { set; get; }

                [JsonPropertyName("symbol")]
                public string Symbol { set; get; }
            }
        }

        namespace services
        {
            /// <summary>
            /// services.TrendingToken model according to the <see href="https://api.rugcheck.xyz/swagger/index.html#/">API Reference</see>.
            /// </summary>
            public struct TrendingToken
            {
                [JsonPropertyName("mint")]
                public string Mint { set; get; }

                [JsonPropertyName("up_count")]
                public int UpCount { set; get; }

                [JsonPropertyName("vote_count")]
                public int VoteCount { set; get; }
            }
        }

        namespace others
        {
            public struct LiquidityAccount
            {
                [JsonPropertyName("mint")]
                public string Mint { set; get; }

                [JsonPropertyName("owner")]
                public string Owner { set; get; }

                [JsonPropertyName("amount")]
                public long Amount { set; get; }

                [JsonPropertyName("delegate")]
                public string? Delegate { set; get; }

                [JsonPropertyName("state")]
                public int State { set; get; }

                [JsonPropertyName("delegatedAmount")]
                public long DelegatedAmount { set; get; }

                [JsonPropertyName("closeAuthority")]
                public string CloseAuthority { set; get; }
            }

            public struct MintAccount
            {
                [JsonPropertyName("mintAuthority")]
                public string MintAuthority { set; get; }

                [JsonPropertyName("supply")]
                public long Supply { set; get; }

                [JsonPropertyName("decimals")]
                public long Decimals { set; get; }

                [JsonPropertyName("isInitialized")]
                public bool IsInitialized { set; get; }

                [JsonPropertyName("freezeAuthority")]
                public string FreezeAuthority { set; get; }
            }
        }

        /// <summary>
        /// Class used to build the signature data RugCheck API authentication requests.
        /// </summary>
        /// <param name="signature"></param>
        public struct SignatureData(byte[] signature)
        {
            [JsonPropertyName("data")]
            public List<byte> Signature { set; get; } = signature.ToList();

            [JsonPropertyName("type")]
            public string Type { set; get; } = "ed25519";
        }

        /// <summary>
        /// Transfer fee data struct used for the rugcheck_api.TokenCheck model.
        /// </summary>
        public struct TransferFeeData
        {
            [JsonPropertyName("authority")]
            public string Authority { set; get; }

            [JsonPropertyName("maxAmount")]
            public double MaxAmount { set; get; }

            [JsonPropertyName("pct")]
            public double Pct { set; get; }
        }
    }
}
