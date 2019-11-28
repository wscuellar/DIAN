using Gosocket.Dian.Plugin.Functions.Cryptography.Common;
using Gosocket.Dian.Plugin.Functions.Cryptography.XmlDsig;
using System.Collections.Generic;
using System.Xml;

namespace Gosocket.Dian.Plugin.Functions.Cryptography.Operations
{
    class XadesVerifyOperation
    {
        public static List<VerificationResult> VerifyAndGetResult(XmlDocument xmlDocument)
        {
            return XmlSigVerifyOperation.VerifyAndGetResults(xmlDocument);
        }
    }
}
