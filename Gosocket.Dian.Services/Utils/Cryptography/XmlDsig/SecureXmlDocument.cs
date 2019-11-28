using System;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text.RegularExpressions;
using Gosocket.Dian.Services.Utils.Common;

namespace Gosocket.Dian.Services.Utils.Cryptography.XmlDsig
{
    public class SecureXmlDocument : ValidXmlDocument
    {
        public bool FormatBase64Text { get; set; } = true;

        public bool ApplyEnvelopedTransformation { get; set; } = false;

        public bool ApplyC14NTransformation { get; set; } = true;

        public bool IncludeKeyInfo { get; set; } = true;

        public bool SignStandAlone { get; set; } = true;

        public void SignWithRsaSha1(string xmlElementToSign, KeyManager key)
        {
            SignWithRsaSha1(xmlElementToSign, 0, key.CspParams, key.RsaParams, key.X509Certificate, key.KeyInFile);
        }

        public void SignWithRsaSha1(string xmlElementToSign, int elementIndex, KeyManager key)
        {
            SignWithRsaSha1(xmlElementToSign, elementIndex, key.CspParams, key.RsaParams, key.X509Certificate, key.KeyInFile);
        }

        public void SignWithRsaSha1(string xmlElementToSign, CspParameters cspparams,
            RSAParameters rsaparams, X509Certificate certX509, bool keyInFile)
        {
            SignWithRsaSha1(xmlElementToSign, 0, cspparams, rsaparams, certX509, keyInFile);
        }

        public void SignWithRsaSha1(string xmlElementToSign, int elementIndex,
            CspParameters cspparams, RSAParameters rsaparams,
            X509Certificate certX509, bool keyInFile)
        {
            try
            {
                //get the element to be signed
                var nodeList = GetElementsByTagName(xmlElementToSign);
                var nodeToSign = nodeList[elementIndex];

                //check if this element has a signature then remove it
                var nsmgr = new XmlNamespaceManager(NameTable);
                nsmgr.AddNamespace("xmldsg", "http://www.w3.org/2000/09/xmldsig#");

                var signature = nodeToSign.ParentNode?.SelectSingleNode("xmldsg:Signature", nsmgr);
                if (signature != null)
                    nodeToSign.ParentNode.RemoveChild(signature);

                ExtendedSignedXml xmlsgn;

                if (SignStandAlone)
                {
                    //put the element along to calculate the signature
                    var xmldoc = new XmlDocument { PreserveWhitespace = true };
                    xmldoc.LoadXml(RemoveNameSpaces(nodeToSign.ParentNode));
                    xmlsgn = new ExtendedSignedXml(xmldoc);
                }
                else
                    xmlsgn = new ExtendedSignedXml(this);

                //obtener la llave privada a partir de los parámetros csp
                RSACryptoServiceProvider key;
                if (keyInFile)
                {
                    var csp = new CspParameters { Flags = CspProviderFlags.UseMachineKeyStore };
                    key = new RSACryptoServiceProvider(csp);
                    key.ImportParameters(rsaparams);
                }
                else
                    key = new RSACryptoServiceProvider(cspparams);

                xmlsgn.SigningKey = key;

                //construir el elemento referencia
                var elementId = string.Empty;
                if (nodeToSign.Attributes?["ID"] != null)
                    elementId = nodeToSign.Attributes["ID"].InnerText;
                else if (nodeToSign.Attributes?["Id"] != null)
                    elementId = nodeToSign.Attributes["Id"].InnerText;
                else if (nodeToSign.Attributes?["id"] != null)
                    elementId = nodeToSign.Attributes["id"].InnerText;

                var attachSignTo = (XmlElement)nodeToSign.ParentNode;
                var refElement = new Reference("#" + elementId);

                if (ApplyEnvelopedTransformation)
                    refElement.AddTransform(new XmlDsigEnvelopedSignatureTransform());

                if (ApplyC14NTransformation)
                    refElement.AddTransform(new XmlDsigC14NTransform());

                xmlsgn.AddReference(refElement);

                //agregar la información de la llave
                var keyinf = new KeyInfo();
                if (IncludeKeyInfo)
                    keyinf.AddClause(new RSAKeyValue(key));

                xmlsgn.KeyInfo = keyinf;

                //agregar la información del certificado
                var certkey = new KeyInfoX509Data(certX509);
                xmlsgn.KeyInfo.AddClause(certkey);

                //calcular la firma
                xmlsgn.ComputeSignature();
                var xmlelem = xmlsgn.GetXml();
                if (FormatBase64Text)
                {
                    var nodeToFormat = xmlelem.GetElementsByTagName("X509Certificate")[0];
                    nodeToFormat.InnerText = Formatter.StandardBase64(nodeToFormat.InnerText);

                    if (IncludeKeyInfo)
                    {
                        nodeToFormat = xmlelem.GetElementsByTagName("Modulus")[0];
                        nodeToFormat.InnerText = Formatter.StandardBase64(nodeToFormat.InnerText);
                        nodeToFormat = xmlelem.GetElementsByTagName("SignatureValue")[0];
                        nodeToFormat.InnerText = Formatter.StandardBase64(nodeToFormat.InnerText);
                    }
                }

                var signatureElement = ImportNode(xmlelem, true);

                //AttachSignTo.AppendChild(SignatureElement);
                attachSignTo?.InsertAfter(signatureElement, nodeToSign);
            }
            catch (Exception e)
            {
                throw new CryptographicException("The enveloping signature could not be applied to the element " + xmlElementToSign + "\r\n" + e.Message, e);
            }
        }

        public void SignEnveloped(KeyManager key)
        {
            SignEnveloped(key.CspParams, key.RsaParams, key.X509Certificate, key.KeyInFile);
        }

        public void SignEnveloped(KeyManager key, bool issuerSerial)
        {
            SignEnveloped(key.CspParams, key.RsaParams, key.X509Certificate, key.KeyInFile, issuerSerial, null);
        }

        public void SignEnveloped(KeyManager key, bool issuerSerial, XmlNode signedNode)
        {
            SignEnveloped(key.CspParams, key.RsaParams, key.X509Certificate, key.KeyInFile, issuerSerial, signedNode);
        }

        public void SignEnveloped(CspParameters cspparams, RSAParameters rsaparams,
            X509Certificate certX509, bool keyInFile)
        {
            try
            {
                var xmlsgn = new SignedXml(this);

                //getting key value
                RSACryptoServiceProvider key;
                if (keyInFile)
                {
                    var csp = new CspParameters { Flags = CspProviderFlags.UseMachineKeyStore };
                    key = new RSACryptoServiceProvider(csp);
                    key.ImportParameters(rsaparams);
                }
                else
                    key = new RSACryptoServiceProvider(cspparams);

                //xmlsgn.SigningKey.FromXmlString(key.ToXmlString(true));
                xmlsgn.SigningKey = key;

                //adding key info
                var keyinf = new KeyInfo();
                if (IncludeKeyInfo)
                    keyinf.AddClause(new RSAKeyValue(key));
                xmlsgn.KeyInfo = keyinf;

                //building reference
                var refElement = new Reference { Uri = string.Empty };
                var env = new XmlDsigEnvelopedSignatureTransform();
                refElement.AddTransform(env);
                if (ApplyC14NTransformation)
                    refElement.AddTransform(new XmlDsigC14NTransform());
                xmlsgn.AddReference(refElement);

                //adding X509Certificate.
                var certkey = new KeyInfoX509Data(certX509);
                xmlsgn.KeyInfo.AddClause(certkey);
                xmlsgn.ComputeSignature();
                var xmlDigitalSignature = xmlsgn.GetXml();

                // Append the element to the XML document.				
                DocumentElement?.AppendChild(ImportNode(xmlDigitalSignature, true));
                if (!FormatBase64Text)
                    return;

                //include line breaks in base64 string
                var nodeToFormat = GetElementsByTagName("X509Certificate")[0];
                nodeToFormat.InnerText = Formatter.StandardBase64(nodeToFormat.InnerText);
                if (!IncludeKeyInfo)
                    return;

                nodeToFormat = GetElementsByTagName("Modulus")[0];
                nodeToFormat.InnerText = Formatter.StandardBase64(nodeToFormat.InnerText);
                nodeToFormat = GetElementsByTagName("SignatureValue")[0];
                nodeToFormat.InnerText = Formatter.StandardBase64(nodeToFormat.InnerText);
            }
            catch (Exception ex)
            {
                throw new CryptographicException("The enveloped signature could not be applied to the document \r\n" + ex.Message, ex);
            }
        }

        public void SignEnveloped(CspParameters cspparams, RSAParameters rsaparams,
            X509Certificate certX509, bool keyInFile, bool issuerSerial, XmlNode signedNode)
        {
            try
            {
                var certificate = new X509Certificate2(certX509);
                var signedXml = new SignedXml(this);

                RSACryptoServiceProvider key;
                if (keyInFile)
                {
                    var csp = new CspParameters { Flags = CspProviderFlags.UseMachineKeyStore };
                    key = new RSACryptoServiceProvider(csp);
                    key.ImportParameters(rsaparams);
                }
                else
                    key = new RSACryptoServiceProvider(cspparams);

                signedXml.SigningKey = key;

                var refElement = new Reference { Uri = "" };
                var env = new XmlDsigEnvelopedSignatureTransform();

                refElement.AddTransform(env);
                if (ApplyC14NTransformation)
                    refElement.AddTransform(new XmlDsigC14NTransform());

                signedXml.AddReference(refElement);

                var certkey = new KeyInfoX509Data(certificate);

                if (issuerSerial)
                {
                    var xserial = new X509IssuerSerial
                    {
                        IssuerName = certificate.IssuerName.Name,
                        SerialNumber = certificate.SerialNumber
                    };
                    certkey.AddIssuerSerial(xserial.IssuerName, xserial.SerialNumber);
                }

                certkey.AddSubjectName(certificate.SubjectName.Name);

                var keyinfo = new KeyInfo();
                if (IncludeKeyInfo)
                    keyinfo.AddClause(new RSAKeyValue(key));
                keyinfo.AddClause(certkey);

                signedXml.KeyInfo = keyinfo;
                signedXml.ComputeSignature();

                var xmlDigitalSignature = signedXml.GetXml();

                var xmlNodeList = xmlDigitalSignature.SelectNodes(
                    "descendant-or-self::*[namespace-uri()='http://www.w3.org/2000/09/xmldsig#']");
                if (xmlNodeList != null)
                    foreach (XmlNode node in xmlNodeList)
                        node.Prefix = "ds";

                if (signedNode == null)
                    DocumentElement?.AppendChild(ImportNode(xmlDigitalSignature, true));
                else
                    signedNode.AppendChild(ImportNode(xmlDigitalSignature, true));
            }
            catch (Exception ex)
            {
                throw new CryptographicException("The enveloped signature could not be applied to the document \r\n" + ex.Message, ex);
            }
        }

        public virtual bool CheckSignature(string signedXmlElement)
        {
            var nodes = GetElementsByTagName(signedXmlElement);
            for (var i = 0; i < nodes.Count; i++)
                if (!CheckSignature(signedXmlElement, i))
                    return false;
            return true;
        }

        public virtual bool CheckSignature(string signedXmlElement, int elementIndex)
        {
            var result = false;

            var nodes = GetElementsByTagName(signedXmlElement);
            if (elementIndex > nodes.Count)
                throw new IndexOutOfRangeException();

            var nsmgr = new XmlNamespaceManager(NameTable);
            nsmgr.AddNamespace("xmldsg", "http://www.w3.org/2000/09/xmldsig#");

            var xmlNode = nodes[elementIndex].ParentNode;
            if (xmlNode == null)
                return false;

            var signature = xmlNode.SelectSingleNode("xmldsg:Signature", nsmgr);

            var xmlset = new XmlDocument { PreserveWhitespace = true };
            xmlset.LoadXml(OuterXml);

            var xmldoc = new XmlDocument { PreserveWhitespace = true };
            RSA rsakey = null;

            //check if the public key is in the signature
            if (signature != null)
            {
                var keyval = signature.SelectSingleNode("xmldsg:KeyInfo/xmldsg:KeyValue", nsmgr);
                if (keyval == null)
                {
                    var ndcert = signature.SelectSingleNode("xmldsg:KeyInfo/xmldsg:X509Data/xmldsg:X509Certificate", nsmgr);
                    if (ndcert != null)
                    {
                        //Org.Mentalis.Security.Certificates.Certificate cert = Org.Mentalis.Security.Certificates.Certificate.CreateFromBase64String(ndcert.InnerText);
                        var cert = new X509Certificate2(Convert.FromBase64String(ndcert.InnerText));
                        rsakey = (RSA)cert.PublicKey.Key;

                        //Org.Mentalis.Security.Certificates.Certificate cert = Org.Mentalis.Security.Certificates.Certificate.CreateFromBase64String(ndcert.InnerText);
                        //rsakey = cert.PublicKey;
                    }
                    else
                        throw new CryptographicException("The public key could not be found.");
                }
            }

            //try to check signature with the element in whole document
            string xmlout;
            try
            {
                var signedXml = new ExtendedSignedXml(xmlset);
                if (signature != null)
                {
                    signedXml.LoadXml((XmlElement)signature);
                    result = rsakey == null ? signedXml.CheckSignature() : signedXml.CheckSignature(rsakey);

                    //try to check signature with element out of document (inside the parent)
                    if (!result)
                    {
                        xmldoc.LoadXml(xmlNode.OuterXml);
                        signedXml = new ExtendedSignedXml(xmldoc);
                        signedXml.LoadXml((XmlElement)signature);
                        result = rsakey == null ? signedXml.CheckSignature() : signedXml.CheckSignature(rsakey);

                        //try to check signature with element out of document (only the element). Removing all namespaces
                        if (!result)
                        {
                            xmlout = RemoveNameSpaces(nodes[elementIndex]);
                            xmldoc.LoadXml(xmlout);

                            signedXml = new ExtendedSignedXml(xmldoc);
                            signedXml.LoadXml((XmlElement)signature);
                            result = rsakey == null ? signedXml.CheckSignature() : signedXml.CheckSignature(rsakey);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            if (result)
                return true;

            //try to check with web services extensions
            var wseSignedXml = new ExtendedWseSignedXml(xmlset);
            if (signature == null)
                return false;

            wseSignedXml.LoadXml((XmlElement)signature);
            result = rsakey == null
                ? wseSignedXml.CheckSignature()
                : wseSignedXml.CheckSignature(rsakey);

            //try to check signature with element out of document (inside the parent)
            if (result)
                return true;

            xmldoc.LoadXml(xmlNode.OuterXml);
            wseSignedXml = new ExtendedWseSignedXml(xmldoc);
            wseSignedXml.LoadXml((XmlElement)signature);
            result = rsakey == null
                ? wseSignedXml.CheckSignature()
                : wseSignedXml.CheckSignature(rsakey);

            //try to check signature with element out of document (only the element). Removing all namespaces
            if (result)
                return true;

            xmlout = RemoveNameSpaces(nodes[elementIndex]);
            xmldoc.LoadXml(xmlout);

            wseSignedXml = new ExtendedWseSignedXml(xmldoc);
            wseSignedXml.LoadXml((XmlElement)signature);
            result = rsakey == null
                ? wseSignedXml.CheckSignature()
                : wseSignedXml.CheckSignature(rsakey);

            return result;
        }

        public virtual bool CheckSignature()
        {
            var result = false;
            var nsmgr = new XmlNamespaceManager(NameTable);
            nsmgr.AddNamespace("xmldsg", "http://www.w3.org/2000/09/xmldsig#");

            var signature = DocumentElement?.SelectSingleNode("xmldsg:Signature", nsmgr);

            var xmlset = new XmlDocument { PreserveWhitespace = true };
            xmlset.LoadXml(OuterXml);

            var xmldoc = new XmlDocument { PreserveWhitespace = true };
            RSA rsakey = null;

            //check if the public key is in the signature
            if (signature != null)
            {
                var keyval = signature.SelectSingleNode("xmldsg:KeyInfo/xmldsg:KeyValue", nsmgr);
                if (keyval == null)
                {
                    var ndcert = signature.SelectSingleNode("xmldsg:KeyInfo/xmldsg:X509Data/xmldsg:X509Certificate", nsmgr);
                    if (ndcert != null)
                    {
                        //Org.Mentalis.Security.Certificates.Certificate cert = Org.Mentalis.Security.Certificates.Certificate.CreateFromBase64String(ndcert.InnerText);
                        var cert = new X509Certificate2(Convert.FromBase64String(ndcert.InnerText));
                        rsakey = (RSA)cert.PublicKey.Key;
                    }
                    else
                        throw new CryptographicException("The public key could not be found.");
                }
            }

            //try to check signature with the element in whole document
            string xmlout;
            try
            {
                var signedXml = new ExtendedSignedXml(xmlset);
                if (signature != null)
                {
                    signedXml.LoadXml((XmlElement)signature);
                    result = rsakey == null
                        ? signedXml.CheckSignature()
                        : signedXml.CheckSignature(rsakey);

                    //try to check signature with element out of document (inside the parent)
                    if (!result)
                    {
                        xmldoc.LoadXml(DocumentElement.OuterXml);
                        signedXml = new ExtendedSignedXml(xmldoc);
                        signedXml.LoadXml((XmlElement)signature);
                        result = rsakey == null
                            ? signedXml.CheckSignature()
                            : signedXml.CheckSignature(rsakey);

                        //try to check signature with element out of document (only the element). Removing all namespaces
                        if (!result)
                        {
                            xmlout = RemoveNameSpaces(DocumentElement);
                            xmldoc.LoadXml(xmlout);
                            signedXml = new ExtendedSignedXml(xmldoc);
                            signedXml.LoadXml((XmlElement)signature);
                            result = rsakey == null
                                ? signedXml.CheckSignature()
                                : signedXml.CheckSignature(rsakey);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            if (result)
                return true;

            //try to check with web services extensions
            var wseSignedXml = new ExtendedWseSignedXml(xmlset);
            if (signature == null)
                return false;

            wseSignedXml.LoadXml((XmlElement)signature);
            result = rsakey == null
                ? wseSignedXml.CheckSignature()
                : wseSignedXml.CheckSignature(rsakey);

            //try to check signature with element out of document (inside the parent)
            if (result)
                return true;

            xmldoc.LoadXml(DocumentElement.OuterXml);

            wseSignedXml = new ExtendedWseSignedXml(xmldoc);
            wseSignedXml.LoadXml((XmlElement)signature);

            result = rsakey == null
                ? wseSignedXml.CheckSignature()
                : wseSignedXml.CheckSignature(rsakey);

            //try to check signature with element out of document (only the element). Removing all namespaces
            if (result)
                return true;

            xmlout = RemoveNameSpaces(DocumentElement);
            xmldoc.LoadXml(xmlout);

            wseSignedXml = new ExtendedWseSignedXml(xmldoc);
            wseSignedXml.LoadXml((XmlElement)signature);

            result = rsakey == null
                ? wseSignedXml.CheckSignature()
                : wseSignedXml.CheckSignature(rsakey);

            return result;
        }

        internal string RemoveNameSpaces(XmlNode node)
        {
            return (Regex.Replace(node.OuterXml,
                "( xmlns.*=[\",'](http|https):\\/\\/[\\w\\-_]+(\\.[\\w\\-_]+)+([\\w\\-\\.,@?^=%&:/~\\+#]*[\\w\\-\\@?^=%&/~\\+#])[\",'])",
                ReplaceNameSpaces));
        }

        private static string ReplaceNameSpaces(Match m)
        {
            return m.Groups[0].Value.IndexOf("www.w3.org", StringComparison.Ordinal) > -1
                ? m.Groups[0].Value
                : string.Empty;
        }

        public void RemoveSignature(string signedXmlElement)
        {
            RemoveSignature(signedXmlElement, 0);
        }

        public void RemoveSignature(string signedXmlElement, int elementIndex)
        {
            //get the element signed
            var nodeList = GetElementsByTagName(signedXmlElement);
            var nodeSigned = nodeList[elementIndex];

            //check if this element has a signature then remove it
            var nsmgr = new XmlNamespaceManager(NameTable);
            nsmgr.AddNamespace("xmldsg", "http://www.w3.org/2000/09/xmldsig#");

            var signature = nodeSigned.ParentNode?.SelectSingleNode("xmldsg:Signature", nsmgr);
            if (signature != null)
                nodeSigned.ParentNode.RemoveChild(signature);
        }
    }
}
