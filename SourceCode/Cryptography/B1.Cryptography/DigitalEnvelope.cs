using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace B1.Cryptography
{
#pragma warning disable 1591
    /// <summary>
    /// Enueration for the supported cryptographic service provider (CSP).
    /// RSA: can be used both for digital enveloping and signature.
    /// DSA: used for digital signature.
    /// </summary>
    public enum AsymmetricAlgorithmTypeEnum : int
    {
        None = 0,
        RSA = 1,
        DSA = 2
    }
#pragma warning restore 1591 // disable the xmlComments warning

    /// <summary>
    /// Digital Enveloping is done in two phases.
    /// Phase 1: Exchange secret-key (session key) using asymmetric encryption.
    /// Phase 2: Exchange actual messages using symmetric key encryption (with previously exchanged secret-key).
    /// 
    /// This class is assuming that RijndaelManaged is being used for the encryption. //?? add support for all
    /// 
    /// This class is used on the sender side.
    /// </summary>
    public class DigitalEnvelopeSender : IDisposable
    {
        // Hardcoded to use AES Symmetric Algorithm //?? Add multiple support
        RijndaelManaged _aes = null;

        // Dummy object which is used for synchronizing code execution
        private object _dummyObject = new object();

        /// <summary>
        /// This function uses asymmetric RSA algorithm to encypt a session key. Session key is created using the
        /// symmetric AES algorithm generated key and IV.
        /// </summary>
        public byte[] EncodeEnvelope(string keyContainerName, byte[] bufKey)
        {
            RSACryptoServiceProvider rsa = AsymmetricOperation.GetRSACryptoServiceProvider(keyContainerName);
            _aes = new RijndaelManaged();
            _aes.Mode = CipherMode.CBC;
            _aes.Padding = PaddingMode.PKCS7;
            _aes.Key = bufKey;
            // Send the session initialiazaion vector in the envelope
            //string sessionKey = encoding.GetString(_aes.Key, 0, _aes.Key.Length)  // 256 bits - encoded to 16 bytes
            //    + encoding.GetString(_aes.IV, 0, _aes.IV.Length);                 // 128 bits - encoded to 8 bytes
            return rsa.Encrypt(_aes.IV, false);
        }

        /// <summary>
        /// This function uses the symmetric AES algorithm to encrypt data (the key and IV are the one which is already
        /// sent to the receiver in the first phase as session key.
        /// </summary>
        public byte[] EncodeMessage(byte[] bufData)
        {
            return SymmetricOperation.EncryptData(_aes, bufData);
        }

        /// <summary>
        /// Release the resources held by RijndaelManaged object.
        /// </summary>
        public void Dispose()
        {
            if (_aes != null)
            {
                lock (_dummyObject)
                {
                    if (_aes != null) _aes.Clear();
                }
            }
        }
    }

    /// <summary>
    /// This class is used on the receiving end. Receives the envelope containing the secret session key.
    /// That is used to create the AES algorithm provider.
    /// </summary>
    public class DigitalEnvelopeReceiver : IDisposable
    {
        // Hardcoded to use AES Symmetric Algorithm //?? Add multiple support
        RijndaelManaged _aes = null;

        // Dummy object which is used for synchronizing code execution
        private object _dummyObject = new object();

        /// <summary>
        /// This function decodes the envelope and initialize the AES provider.
        /// </summary>
        public void DecodeEnvelope(byte[] envelope, string keyContainerName, byte[] bufKey)
        {
            // Use RSA to decrypt the envelope
            RSACryptoServiceProvider rsa = AsymmetricOperation.GetRSACryptoServiceProvider(keyContainerName);
            byte[] ivBuf = rsa.Decrypt(envelope, false);

            ////// Get the secret key and split into the key and IV
            ////string sessionKey = encoding.GetString(secretkey, 0, secretkey.Length);
            ////string key = sessionKey.Substring(0, 16);
            ////string iv = sessionKey.Substring(16, 8);

            _aes = new RijndaelManaged();
            _aes.Mode = CipherMode.CBC;
            _aes.Padding = PaddingMode.PKCS7;
            _aes.Key = bufKey; //?? encoding.GetBytes(key);
            _aes.IV = ivBuf; //?? encoding.GetBytes(iv);
        }

        /// <summary>
        /// This functon decodes the messages using the AES provider initialized when envelope was decoded.
        /// </summary>
        public byte[] DecodeMessage(byte[] cypherText)
        {
            return SymmetricOperation.DecryptData(_aes, cypherText);
        }

        /// <summary>
        /// Release the resources held by RijndaelManaged object.
        /// </summary>
        public void Dispose()
        {
            if (_aes != null)
            {
                lock (_dummyObject)
                {
                    if (_aes != null) _aes.Clear();
                }
            }
        }
    }
}
