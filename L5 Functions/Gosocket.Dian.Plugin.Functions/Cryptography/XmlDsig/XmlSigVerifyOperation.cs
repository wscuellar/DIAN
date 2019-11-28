using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Plugin.Functions.Cryptography.Common;
using Gosocket.Dian.Plugin.Functions.Cryptography.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Gosocket.Dian.Plugin.Functions.Cryptography.XmlDsig
{
    internal abstract class XmlSigVerifyOperation
    {

        internal static List<VerificationResult> VerifyAndGetResults(XmlDocument xmlDocument)
        {
            return PerformValidationFromXml(xmlDocument);
        }

        private static List<VerificationResult> PerformValidationFromXml(XmlDocument xmlDocument)
        {
            var results = new List<VerificationResult>();
            var newsignedXml = new ExtendedSignedXml(xmlDocument);
            if (xmlDocument.DocumentElement == null) throw new Exception("Document has no root element");

            var signatureNode = XmlSigNodesHelper.GetSignatureNode(xmlDocument);
            newsignedXml.LoadXml(signatureNode);

            var validationParameters = new VerificationParameters();
            var verificationCertificate = GetVerificationCertificate(newsignedXml, validationParameters);
            if (verificationCertificate == null) throw new Exception("Signer public key could not be found");

            var digestValue = XmlSigNodesHelper.GetSignatureXadesDigestValue(xmlDocument);
            var digest = ConfigurationManager.GetValue("SignatureDigestValue");
            digestValue = digestValue.Replace(Environment.NewLine, "");
            if (!digest.Split('@').Contains(digestValue))
                results.Add(new VerificationResult { IsValid = false, Message = "Política de firma inválida." });
            else
                results.Add(new VerificationResult { IsValid = true, Message = "Política de firma es válida." });

            if (!newsignedXml.CheckSignature(verificationCertificate, true)) results.Add(new VerificationResult { IsValid = false, Message = "Valor de la firma inválido." });
            else results.Add(new VerificationResult { IsValid = true, Message = "Valor de la firma es válida." });
            return results;
        }

        private static X509Certificate2 GetVerificationCertificate(SignedXml signedXml, VerificationParameters verificationParameters)
        {
            var validationCertificate = verificationParameters.VerificationCertificate;
            if (validationCertificate == null)
            {
                if (signedXml.KeyInfo != null)
                {
                    var certificates = signedXml.KeyInfo.GetEnumerator();
                    if (certificates.MoveNext())
                    {
                        var x509Data = (KeyInfoX509Data)certificates.Current;
                        if (x509Data != null)
                        {
                            if (x509Data.Certificates.Count > 0)
                                validationCertificate = (X509Certificate2)x509Data.Certificates[0];
                        }
                    }
                }
            }
            return validationCertificate;
        }
    }
}
