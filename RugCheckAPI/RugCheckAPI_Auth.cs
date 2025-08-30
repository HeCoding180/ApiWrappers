using Org.BouncyCastle.Crypto.Generators;
using Solnet;
using Solnet.Programs;
using Solnet.Rpc;
using Solnet.Wallet;
using Solnet.Wallet.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ApiWrappers.RugCheckAPI
{
    /// <summary>
    /// Authentication class for the RugCheck API.
    /// </summary>
    partial class RugCheckAPI
    {
        //   ---   Public Constants   ---

        public const string AUTH_REQUEST_URI = "auth/login/solana";

        //   ---   Public Methods   ---

        /// <summary>
        /// Method used to generate a JWT API key.
        /// </summary>
        /// <returns>Awaitable task resulting in new API authentication data.</returns>
        public async Task<HttpApiAuthData> GenerateJwtKey(Func<HttpRequestMessage, Task<HttpResponseMessage>> httpRequestMethod)
        {
            Models.dto.AuthResponse ApiKey;

            log.LogDebug("JWT key generation started");

            // Generate base message

            Models.dto.AuthMessage authMessage = new Models.dto.AuthMessage(SolanaWalletAccount.PublicKey.Key);

            string signatureMessage = authMessage.ToString();
            byte[] baseMessageBytes = Encoding.UTF8.GetBytes(signatureMessage);

            log.LogTrace("Signing base message: " + signatureMessage);

            // Calculate signature
            byte[] signatureBytes = SolanaWalletAccount.Sign(baseMessageBytes);

            // Generate key request payload
            string authRequestPayload = new Models.dto.AuthRequest(authMessage, new Models.SignatureData(signatureBytes)).ToString();

            HttpRequestMessage httpRequestMsg = new HttpRequestMessage(HttpMethod.Post, AUTH_REQUEST_URI);
            httpRequestMsg.Content = new StringContent(authRequestPayload, Encoding.UTF8, "application/json");

            log.LogTrace($"Sending authentication payload to \"{httpRequestMsg.Content.ToString()}\": " + authRequestPayload);

            Stopwatch stopwatch = Stopwatch.StartNew();

            HttpResponseMessage response = await httpRequestMethod(httpRequestMsg);

            stopwatch.Stop();

            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                if (response.IsSuccessStatusCode)
                {
                    ApiKey = JsonSerializer.Deserialize<Models.dto.AuthResponse>(responseStream);
                    log.LogDebug($"Response received after {stopwatch.ElapsedMilliseconds} ms with code {(int)response.StatusCode} ({response.StatusCode}). The new JWT API Key is \"{ApiKey.Key}\"");
                }
                else
                {
                    Models.dto.ErrorResponse responseError = JsonSerializer.Deserialize<Models.dto.ErrorResponse>(responseStream);
                    log.LogDebug($"Response received after {stopwatch.ElapsedMilliseconds} ms with code {(int)response.StatusCode} ({response.StatusCode}). The error from the API is \"{responseError.ErrorMessage}\"", DebugMessageType.Error);
                    throw new ApiHttpExeption(response.StatusCode);
                }
            }

            return HttpApiAuthData.GenerateBearerAuth(ApiKey.Key);
        }
    }
}
