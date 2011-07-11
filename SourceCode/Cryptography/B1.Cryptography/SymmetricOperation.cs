using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace B1.Cryptography
{
#pragma warning disable 1591 // disable the xmlComments warning
    /// <summary>
    /// Enueration for the supported symmetric algorithm provider.
    /// </summary>
    public enum SymmetricAlgorithmTypeEnum : int
    {
        None = 0,
        DES = 1,
        RC2 = 2,
        Rijndael = 3,
        TripleDES = 4
    }
#pragma warning restore 1591 // restore the xmlComments warning

    /// <summary>
    /// This class wraps all the symmetric algorithm provided by .NET cryptography library. It creates common
    /// functions which can be used to encrypt a plain string into cipher text using any desired symmetric
    /// algorithm.
    /// </summary>
    public static class SymmetricOperation
    {
        /// <summary>
        /// Creates a algorithm specific .NET implementation of the symmetric algorithm provider.
        /// </summary>
        public static SymmetricAlgorithm CreateSymmetricAlgorithmProvider(SymmetricAlgorithmTypeEnum provider)
        {
            switch (provider)
            {
                case SymmetricAlgorithmTypeEnum.DES:
                    return new DESCryptoServiceProvider();
                case SymmetricAlgorithmTypeEnum.RC2:
                    return new RC2CryptoServiceProvider();
                case SymmetricAlgorithmTypeEnum.Rijndael:
                    return new RijndaelManaged();
                case SymmetricAlgorithmTypeEnum.TripleDES:
                    return new TripleDESCryptoServiceProvider();
                default:
                    return new TripleDESCryptoServiceProvider();
            }
        }

        /// <summary>
        /// This function takes a key and turn it into the legal key size for a given algorithm. It pads the string
        /// with spaces if it is shorter or truncate if it is longer.
        /// </summary>
        public static string MakeKeyLegalSize(SymmetricAlgorithmTypeEnum provider, string secretKey)
        {
            SymmetricAlgorithm alg = CreateSymmetricAlgorithmProvider(provider);

            string ret;
            if (alg.LegalKeySizes.Length > 0)
            {
                int keySize = secretKey.Length * 8;
                if (keySize < alg.LegalKeySizes[0].MinSize)
                {
                    int pad = (alg.LegalKeySizes[0].MinSize - keySize) / 8;
                    ret = secretKey.PadRight(pad + secretKey.Length, ' ');
                }
                else if (keySize > alg.LegalKeySizes[0].MaxSize)
                {
                    int len = alg.LegalKeySizes[0].MaxSize / 8;
                    ret = secretKey.Substring(0, len);
                }
                else
                    ret = secretKey;
            }
            else
                ret = secretKey;

            return ret;
        }

        /// <summary>
        /// This function takes a IV and turn it into the legal block size for a given algorithm. It pads the string
        /// with spaces if it is shorter or truncate if it is longer.
        /// </summary>
        public static byte[] MakeLegalInitializationVector(SymmetricAlgorithmTypeEnum provider, string iv)
        {
            SymmetricAlgorithm alg = CreateSymmetricAlgorithmProvider(provider);

            string ret;
            if (alg.LegalBlockSizes.Length > 0)
            {
                int blockSize = iv.Length * 8;
                if (blockSize < alg.LegalBlockSizes[0].MinSize)
                {
                    int pad = (alg.LegalBlockSizes[0].MinSize - blockSize) / 8;
                    ret = iv.PadRight(pad + iv.Length, ' ');
                }
                else if (blockSize > alg.LegalBlockSizes[0].MaxSize)
                {
                    int len = alg.LegalBlockSizes[0].MaxSize / 8;
                    ret = iv.Substring(0, len);
                }
                else
                    ret = iv;
            }
            else
                ret = iv;

            return ASCIIEncoding.ASCII.GetBytes(ret);
        }

        /// <summary>
        /// Encrypts the data and encode using Base-64. Base-64 is useful when the string will be in transit through
        /// systems that are designed to deal with textual data.
        /// </summary>
        public static string EncryptToBase64(SymmetricAlgorithmTypeEnum provider, string plainText,
            string secretKey, byte[] bufInitVector, Encoding encoding)
        {
            byte[] bufEncryptedData = EncryptData(provider,
                encoding.GetBytes(plainText),
                encoding.GetBytes(secretKey), bufInitVector);
            return Convert.ToBase64String(bufEncryptedData);
        }

        /// <summary>
        /// Encrypts the data using the provider, key and IV.
        /// </summary>
        public static byte[] EncryptData(SymmetricAlgorithmTypeEnum provider, byte[] bufData, byte[] bufKey,
            byte[] bufInitVector)
        {
            SymmetricAlgorithm alg = CreateSymmetricAlgorithmProvider(provider);
            alg.Key = bufKey;
            alg.IV = bufInitVector;
            return EncryptData(alg, bufData);
        }

        /// <summary>
        /// Encrypts the data using the given symmetric algorithm provider, key and IV.
        /// </summary>
        public static byte[] EncryptData(SymmetricAlgorithm alg, byte[] bufData)
        {
            byte[] bufEncryptedData = null;
            ICryptoTransform encryptor = null;

            encryptor = alg.CreateEncryptor();
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    cs.Write(bufData, 0, bufData.Length);
                    cs.FlushFinalBlock();
                }
                bufEncryptedData = ms.ToArray();
            }

            return bufEncryptedData;
        }

        /// <summary>
        /// Decrypt the give Base 64 cypher string .
        /// </summary>
        public static string DecryptFromBase64(SymmetricAlgorithmTypeEnum provider, string cypherStringBase64,
            string secretKey, byte[] bufInitVector, Encoding encoding)
        {
            byte[] bufKey = encoding.GetBytes(secretKey);
            byte[] bufEncryptedData = Convert.FromBase64String(cypherStringBase64);
            byte[] bufData = DecryptData(provider, bufEncryptedData, bufKey, bufInitVector);
            string plainText = encoding.GetString(bufData);
            //?? return plainText.Substring(0, plainText.IndexOf('\0'));
            return plainText;
        }

        /// <summary>
        /// Decrypts the data using the provider, key and IV.
        /// </summary>
        public static byte[] DecryptData(SymmetricAlgorithmTypeEnum provider, byte[] bufEncryptedData, byte[] bufKey,
            byte[] bufInitVector)
        {
            SymmetricAlgorithm alg = CreateSymmetricAlgorithmProvider(provider);
            alg.Key = bufKey;
            alg.IV = bufInitVector;
            return DecryptData(alg, bufEncryptedData);
        }

        /// <summary>
        /// Decrypts the data using the symmetric algorithm provided, key and IV.
        /// </summary>
        public static byte[] DecryptData(SymmetricAlgorithm alg, byte[] bufEncryptedData)
        {
            // Size of the decrypted string which is read at a time
            int readSize = 1024;
            byte[] bufData = new byte[readSize];
            int bytesRead;

            ICryptoTransform decryptor = alg.CreateDecryptor();
            using (MemoryStream encryptedStream = new MemoryStream(bufEncryptedData))
            {
                using (CryptoStream cryptoStream = new CryptoStream(encryptedStream, decryptor, CryptoStreamMode.Read))
                {
                    using (MemoryStream outStream = new MemoryStream())
                    {
                        do
                        {
                            bytesRead = cryptoStream.Read(bufData, 0, readSize);
                            outStream.Write(bufData, 0, bytesRead);
                        } while (bytesRead > 0);

                        return outStream.ToArray();
                    }
                }
            }
        }

        public static SymmetricCipherResults EncryptData(string plainText, 
                SymmetricAlgorithmTypeEnum symmetricAlgorithm, 
                string key)
        {
            SymmetricAlgorithm algorithm = SymmetricOperation.CreateSymmetricAlgorithmProvider(symmetricAlgorithm);

            key = SymmetricOperation.MakeKeyLegalSize(symmetricAlgorithm, key);
            algorithm.GenerateIV();
            string iv = Convert.ToBase64String(algorithm.IV );

            string cipherText = SymmetricOperation.EncryptToBase64(symmetricAlgorithm, 
                    plainText, key, algorithm.IV, System.Text.Encoding.UTF8 );

            return new SymmetricCipherResults() { CipherText = cipherText, IV = iv };
        }

        public static string DecryptData(SymmetricCipherResults cipherResults, 
                SymmetricAlgorithmTypeEnum symmetricAlgorithm, 
                string key)
        {
            key = SymmetricOperation.MakeKeyLegalSize(symmetricAlgorithm, key);

            byte[] IV = Convert.FromBase64String( cipherResults.IV );

            string PlainText = SymmetricOperation.DecryptFromBase64(symmetricAlgorithm,
                    cipherResults.CipherText, key, IV, System.Text.Encoding.UTF8 );

            return PlainText;
        }                                                                                       
    }

    public class SymmetricCipherResults
    {
        public string IV { get; set; }
        public string CipherText { get; set; }
    }
}