using System;
using System.Xml;

namespace Gosocket.Dian.Plugin.Functions.Cryptography.Helpers
{
    internal class XmlSigNodesHelper
    {
        private XmlSigNodesHelper() { }

        internal static XmlElement GetSignatureNode(XmlDocument document)
        {
            if (document == null) throw new Exception("Xml document cannot be null");
            if (document.DocumentElement == null) throw new Exception("Xml document must have root element");
            if (document.DocumentElement.LocalName.Equals("Signature")) return document.DocumentElement;
            var signatureNode = document.DocumentElement.GetElementsByTagName("ds:Signature");
            if (signatureNode.Count == 0) throw new Exception("'Signature' node not found in enveloped signature");
            return signatureNode[0] as XmlElement;
        }

        internal static string GetSignatureXadesDigestValue(XmlDocument document)
        {
            var sigPolicyHashNode = document.DocumentElement.GetElementsByTagName("xades:SigPolicyHash");
            if (sigPolicyHashNode.Count == 0) throw new Exception("Xml document must have SigPolicyHash element");
            return sigPolicyHashNode[0]?.LastChild?.InnerText;
        }
    }
}
