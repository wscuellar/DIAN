using Gosocket.Dian.Plugin.Functions.Cryptography.Common;
using Gosocket.Dian.Plugin.Functions.Cryptography.Operations;
using System.Collections.Generic;
using System.Xml;

namespace Gosocket.Dian.Plugin.Functions.Cryptography.Verify
{
    public class XadesVerify
    {
        public List<VerificationResult> PerformAndGetResults(XmlDocument xmlDocument)
        {
            return XadesVerifyOperation.VerifyAndGetResult(xmlDocument);
        }
    }
}
