using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.Cryptography
{
#pragma warning disable 1591 // disable the xmlComments warning
    /// <summary>
    /// Enueration for the supported hash algorithm provider.
    /// </summary>
    public enum HashAlgorithmTypeEnum : int
    {
        None = 0,
        MD5HashAlgorithm = 1,
        SHA1HashAlgorithm = 2,
        SHA256HashAlgorithm = 3,
        SHA384HashAlgorithm = 4,
        SHA512HashAlgorithm = 5
    }
#pragma warning restore 1591 // restore the xmlComments warning

    /// <summary>
    /// This class wraps all the hashing crypto service provided by .NET cryptography library. It creates common
    /// functions which can be used to encrypt a plain string into cipher text using a given salt.
    /// </summary>
    public static class HashOperation
    {
        /// <summary>
        /// Creates a algorithm specific .NET implementation of the hash algorithm provider.
        /// </summary>
        public static HashAlgorithm CreateHashAlgorithmProvider(HashAlgorithmTypeEnum algType)
        {
            if (algType == HashAlgorithmTypeEnum.MD5HashAlgorithm)
                return new MD5CryptoServiceProvider();
            else if (algType == HashAlgorithmTypeEnum.SHA1HashAlgorithm)
                return new SHA1CryptoServiceProvider();
            else if (algType == HashAlgorithmTypeEnum.SHA256HashAlgorithm)
                return new SHA256Managed();
            else if (algType == HashAlgorithmTypeEnum.SHA384HashAlgorithm)
                return new SHA384Managed();
            else if (algType == HashAlgorithmTypeEnum.SHA512HashAlgorithm)
                return new SHA512Managed();
            else
                return new SHA1CryptoServiceProvider();
        }

        /// <summary>
        /// Create random salt for a given hash algorithm provider.
        /// </summary>
        public static string CreateRandomSalt(HashAlgorithmTypeEnum algType)
        {
            HashAlgorithm alg = CreateHashAlgorithmProvider(algType);
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] bufSalt = new byte[alg.HashSize / 10];
            rng.GetNonZeroBytes(bufSalt);
            return Convert.ToBase64String(bufSalt);
        }

        /// <summary>
        /// Create random salt for a salt size.
        /// </summary>
        public static string CreateRandomSalt(int size)
        {
            byte[] bufSalt = new byte[size];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetNonZeroBytes(bufSalt);
            return Convert.ToBase64String(bufSalt).Substring(0, size);
        }

        /// <summary>
        /// Create hash for a plainText using the given hash algorithm type and salt.
        /// </summary>
        public static string ComputeHash(HashAlgorithmTypeEnum algType, string plainText, string salt)
        {
            return ComputeHash(CreateHashAlgorithmProvider(algType), plainText, salt);
        }

        /// <summary>
        /// Create hash for a plainText using the given hash algorithm object and salt.
        /// </summary>
        public static string ComputeHash(HashAlgorithm alg, string plainText, string salt)
        {
            byte[] bufData = Encoding.ASCII.GetBytes(string.Format("{0}{1}", salt, plainText));
            return Convert.ToBase64String(alg.ComputeHash(bufData));
        }
    }
}
