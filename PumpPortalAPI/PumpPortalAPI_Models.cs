using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ApiWrappers.PumpPortalAPI
{
    public struct PumpPortal_SuccessMessage
    {
        [JsonPropertyName("message")]
        public string Message { set; get; }
    }

    public struct PumpPortal_ErrorMessage
    {
        [JsonPropertyName("errors")]
        public string ErrorMessage { set; get; }
    }

    public struct PumpPortal_DataMessage
    {
        [JsonPropertyName("signature")]
        public string Signature { set; get; }

        [JsonPropertyName("mint")]
        public string Mint { set; get; }

        [JsonPropertyName("traderPublicKey")]
        public string TraderPublicKey { set; get; }

        [JsonPropertyName("txType")]
        public string TransactionType { set; get; }

        [JsonPropertyName("initialBuy")]
        public double InitialBuy { set; get; }

        [JsonPropertyName("solAmount")]
        public double SolAmount { set; get; }

        [JsonPropertyName("bondingCurveKey")]
        public string BondingCurveKey { set; get; }

        [JsonPropertyName("vTokensInBondingCurve")]
        public double TokensInBondingCurve { set; get; }

        [JsonPropertyName("vSolInBondingCurve")]
        public double SolInBondingCurve { set; get; }

        [JsonPropertyName("marketCapSol")]
        public double SolMarketCap { set; get; }

        [JsonPropertyName("name")]
        public string Name { set; get; }

        [JsonPropertyName("symbol")]
        public string Symbol { set; get; }

        [JsonPropertyName("uri")]
        public string URI { set; get; }

        [JsonPropertyName("pool")]
        public string Pool { set; get; }
    }
}
