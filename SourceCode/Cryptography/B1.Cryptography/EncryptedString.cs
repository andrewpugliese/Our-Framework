using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.Cryptography
{
    /// <summary>
    /// This class provide functionality to encrypt a plain string with expiration date. This string can be passed on to
    /// the third-party web sites or other external components in the two way conversation. If this secure string is
    /// bookmarked they wont work after that timeframe is expired.
    /// </summary>
    public class EncryptedString
    {
        private string _secretKey;
        private byte[] _bufInitVector;
        private string _plainString;
        private string _salt;
        private TimeSpan _expirationTimeSpan;

        private DateTime lastEncryptTime = DateTime.MinValue;
        private string _secureStr;

        private const Int32 SaltLength = 8;
        private static SymmetricAlgorithmTypeEnum AlgorithmType = SymmetricAlgorithmTypeEnum.TripleDES;

        /// <summary>
        /// EncryptedString constructor.
        /// </summary>
        public EncryptedString(string secretKey, string plainStr, Int32 expirationIntervalMin)
        {
            _secretKey = SymmetricOperation.MakeKeyLegalSize(AlgorithmType, secretKey);
            _plainString = plainStr;
            _expirationTimeSpan = new TimeSpan(0, expirationIntervalMin, 0);
        }

        /// <summary>
        /// Get secure string.
        /// </summary>
        public string GetSecureString()
        {
            // Create new security string if the current one is expired
            if (DateTime.Now.Subtract(lastEncryptTime) > _expirationTimeSpan)
            {
                _secureStr = CreateSecureString();
            }

            return _secureStr;
        }

        /// <summary>
        /// Get initial vector.
        /// </summary>
        public byte[] InitVector
        {
            get
            {
                if (_bufInitVector == null)
                    _bufInitVector = SymmetricOperation.MakeLegalInitializationVector(AlgorithmType, "XYZA");
                return _bufInitVector;
            }
        }

        /// <summary>
        /// Create secure string.
        /// </summary>
        private string CreateSecureString()
        {
            //?? Create a new random salt for the specified length
            // Decrypt will ignore the first pre-fixed number of salt characters
            _salt = "AAAAABBB";

            // Put a new timestamp in the data to be encrypted
            lastEncryptTime = DateTime.Now;

            DateTime dtExpirationTim = lastEncryptTime + _expirationTimeSpan;

            String sPlainStr = string.Format("{0}{1}{2:yyyyMMddHHmmss}",
                _salt, _plainString, dtExpirationTim);

            return SymmetricOperation.EncryptToBase64(AlgorithmType, sPlainStr,
                _secretKey, InitVector, ASCIIEncoding.ASCII);
        }

        /// <summary>
        /// Decrypt data from the secure string.
        /// </summary>
        public static string GetDataFromSecureString(string secretKey, byte[] bufInitVector, string base64Data)
        {
            secretKey = SymmetricOperation.MakeKeyLegalSize(AlgorithmType, secretKey);

            // Decrypt the secret string and make sure that is not expired
            string plainStr = SymmetricOperation.DecryptFromBase64(AlgorithmType,
                base64Data, secretKey, bufInitVector, ASCIIEncoding.ASCII);

            //Parse data to remove the salt and the expiration timestamp
            plainStr = plainStr.Substring(SaltLength);
            string sExpirationTim = plainStr.Substring(plainStr.Length - 14);
            DateTime dtExpirationTim = DateTime.ParseExact(sExpirationTim,
                "yyyyMMddHHmmss",
                System.Globalization.DateTimeFormatInfo.InvariantInfo);

            // Check if the contetn should expire
            if (dtExpirationTim > DateTime.Now)
            {
                return plainStr.Substring(0, plainStr.Length - 14);
            }

            return string.Empty;
            //Bad format
        }
    }
}
