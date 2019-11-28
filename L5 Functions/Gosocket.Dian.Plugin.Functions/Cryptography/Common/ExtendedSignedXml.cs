using System.Security.Cryptography.Xml;
using System.Xml;

namespace Gosocket.Dian.Plugin.Functions.Cryptography.Common
{
    class ExtendedSignedXml : SignedXml
    {
        public ExtendedSignedXml(XmlDocument document) : base(document) { }
    }
}
