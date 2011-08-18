using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace B1.Cryptography
{
    /// <summary>
    /// This class provides functions to hash and sign plain text using RSA and DSA.
    /// </summary>
    public class DigitalSignature
    {
        /// <summary>
        /// Hash the given data and sign it using RSA.
        /// </summary>
        public static byte[] HashAndSignUsingRSA(byte[] dataToSign, string keyContainerName,
            HashAlgorithmTypeEnum hashType)
        {
            RSACryptoServiceProvider rsa = AsymmetricOperation.GetRSACryptoServiceProvider(keyContainerName);
            return rsa.SignData(dataToSign, HashOperation.CreateHashAlgorithmProvider(hashType));
        }

        /// <summary>
        /// Verify the given data and signature using RSA.
        /// </summary>
        public static bool VerifySignedHashUsingRSA(byte[] dataToVerify, byte[] signedData,
            string keyContainerName, HashAlgorithmTypeEnum hashType)
        {
            RSACryptoServiceProvider rsa = AsymmetricOperation.GetRSACryptoServiceProvider(keyContainerName);
            return rsa.VerifyData(dataToVerify, HashOperation.CreateHashAlgorithmProvider(hashType), signedData);
        }

        /// <summary>
        /// Hash the given data and sign it using DSA.
        /// </summary>
        public static byte[] HashAndSignUsingDSA(byte[] dataToSign, string keyContainerName, HashAlgorithmTypeEnum hashType)
        {
            DSACryptoServiceProvider dsa = AsymmetricOperation.GetDSACryptoServiceProvider(keyContainerName);
            byte[] hashedData = HashOperation.CreateHashAlgorithmProvider(hashType).ComputeHash(dataToSign);
            return dsa.CreateSignature(hashedData);
        }

        /// <summary>
        /// Verify the given data and signature using RSA.
        /// </summary>
        public static bool VerifySignedHashUsingDSA(byte[] dataToVerify, byte[] signedData,
            string keyContainerName, HashAlgorithmTypeEnum hashType)
        {
            DSACryptoServiceProvider dsa = AsymmetricOperation.GetDSACryptoServiceProvider(keyContainerName);
            byte[] hashedData = HashOperation.CreateHashAlgorithmProvider(hashType).ComputeHash(dataToVerify);
            return dsa.VerifySignature(hashedData, signedData);
        }
    }
}
