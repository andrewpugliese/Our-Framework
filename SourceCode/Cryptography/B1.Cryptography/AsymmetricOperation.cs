using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace B1.Cryptography
{
    /// <summary>
    /// This class provides functions to create and use RSA and DSA algorithms for assymetric encryption
    /// and description.
    /// </summary>
    public class AsymmetricOperation
    {
        /// <summary>
        /// This function creates the crypto service provider for a given csp type.
        /// </summary>
        public static CspParameters GetCryptoServiceProvider(string keyContainerName, AsymmetricAlgorithmTypeEnum cspType)
        {
            CspParameters csp = new CspParameters();
            csp.KeyContainerName = keyContainerName; // Key container that has the _rsa key pair
            csp.Flags = CspProviderFlags.UseMachineKeyStore; // Use machine key store
            if (cspType == AsymmetricAlgorithmTypeEnum.DSA)
            {
                csp.ProviderType = 13;      // CSP provider type PROV_DSS_DH
                csp.ProviderName = "Microsoft Enhanced DSS and Diffie-Hellman Cryptographic Provider";
            }
            else
            {
                csp.ProviderType = 1;       // CSP provider type PROV_RSA_FULL
                csp.ProviderName = "Microsoft Enhanced Cryptographic Provider v1.0";
            }
            return csp;
        }

        /// <summary>
        /// This function creates the RSA crypto service provider.
        /// </summary>
        public static RSACryptoServiceProvider GetRSACryptoServiceProvider(string keyContainerName)
        {
            return new RSACryptoServiceProvider(
                AsymmetricOperation.GetCryptoServiceProvider(keyContainerName, AsymmetricAlgorithmTypeEnum.RSA));
        }

        /// <summary>
        /// This function creates the DSA crypto service provider.
        /// </summary>
        public static DSACryptoServiceProvider GetDSACryptoServiceProvider(string keyContainerName)
        {
            return new DSACryptoServiceProvider(
                AsymmetricOperation.GetCryptoServiceProvider(keyContainerName, AsymmetricAlgorithmTypeEnum.DSA));
        }
    }
}
