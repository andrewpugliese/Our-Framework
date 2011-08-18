using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace B1.Cryptography
{
    /// <summary>
    /// This class have functions to sign and verify XML documents
    /// </summary>
    public class SignedXmlOperation
    {
        /// <summary>
        /// Sign XML string using the RSA key.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="rsaKey"></param>
        /// <returns></returns>
        public static string SignXmlString(string xml, RSA rsaKey)
        {
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = false;
            doc.LoadXml(xml);
            return SignXml(doc, rsaKey).OuterXml;
        }

        /// <summary>
        /// Sign a given XML file path and RSA key, sign the xml and write it to the out xml path
        /// </summary>
        /// <param name="xmlFilePath">XML file path</param>
        /// <param name="xmlOutPath">XML Out file Path</param>
        /// <param name="rsaKey">RSA key</param>
        public static void SignXmlFile(string xmlFilePath, string xmlOutPath, RSA rsaKey)
        {
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = false;
            doc.Load(xmlFilePath);
            doc = SignXml(doc, rsaKey);
            using(XmlTextWriter wtr = new XmlTextWriter(xmlOutPath, new UTF8Encoding(false)))
            {
                doc.WriteTo(wtr);
            }
        }

        /// <summary>
        /// Sign an XML document using the given RSA key. The key information is added to the document so
        /// that the verifying code can have the key.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="rsaKey"></param>
        /// <returns></returns>
        public static XmlDocument SignXml(XmlDocument doc, RSA rsaKey)
        {
            // Create a SignedXml object.
            SignedXml signedXml = new SignedXml(doc);
            signedXml.SigningKey = rsaKey;

            // Create a reference to be signed and add an enveloped transformation to this reference
            Reference reference = new Reference();
            reference.Uri = "";
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            signedXml.AddReference(reference);

            // Add an RSAKeyValue KeyInfo (helps recipient find key to validate).
            KeyInfo keyInfo = new KeyInfo();
            keyInfo.AddClause(new RSAKeyValue((RSA)rsaKey));
            signedXml.KeyInfo = keyInfo;

            // Compute the signature, get xml representation of signature and append it to the xml document
            signedXml.ComputeSignature();
            XmlElement xmlDigitalSignature = signedXml.GetXml();
            doc.DocumentElement.AppendChild(doc.ImportNode(xmlDigitalSignature, true));

            if (doc.FirstChild is XmlDeclaration)
            {
                doc.RemoveChild(doc.FirstChild);
            }

            return doc;
        }

        /// <summary>
        /// Verifies signature contained in the XML string.
        /// </summary>
        /// <param name="signedXml">Signed XML string</param>
        /// <returns>True if the signature is verified</returns>
        public static bool VerifySignedXmlString(string signedXml)
        {
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = false;
            doc.LoadXml(signedXml);
            return VerifySignedXml(doc);
        }

        /// <summary>
        /// Verifies signature contained in the XML file.
        /// </summary>
        /// <param name="signedXmlPath">File Path to the signed XML</param>
        /// <returns>True if the signature is verified</returns>
        public static bool VerifySignedXmlFile(string signedXmlPath)
        {
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = false;
            doc.Load(signedXmlPath);
            return VerifySignedXml(doc);
        }

        /// <summary>
        /// This function verifies the XML signature.
        /// </summary>
        /// <param name="doc">XmlDocument which contains the signature node</param>
        /// <returns>True if the signature is verified</returns>
        public static bool VerifySignedXml(XmlDocument doc)
        {
            // Create a new SignedXml object and pass it the XML document class.
            SignedXml signedXml = new SignedXml(doc);

            // Find the "Signature" node and create a new XmlNodeList object.
            XmlNodeList nodeList = doc.GetElementsByTagName("Signature");

            // Load the signature node.
            signedXml.LoadXml((XmlElement)nodeList[0]);

            // Check the signature and return the result.
            return signedXml.CheckSignature();
        }
    }
}
