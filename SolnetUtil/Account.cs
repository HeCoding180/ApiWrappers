using Solnet.Wallet.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solnet.Wallet
{
    /// <summary>
    /// This class is an extension of the Solnet.Wallet package with functions which have not been released yet
    /// </summary>
    public static class SolnetUtil
    {
        /// <summary>
        /// Initialize an account with the passed secret key
        /// </summary>
        /// <param name="secretKey">The private key.</param>
        public static Account FromSecretKey(string secretKey)
        {
            var B58 = new Base58Encoder();
            byte[] skeyBytes = B58.DecodeData(secretKey);
            if (skeyBytes.Length != 64)
            {
                throw new ArgumentException("Not a secret key");
            }

            Account acc = new Account(skeyBytes, skeyBytes.AsSpan(32, 32).ToArray());

            return acc;
        }
    }
}
