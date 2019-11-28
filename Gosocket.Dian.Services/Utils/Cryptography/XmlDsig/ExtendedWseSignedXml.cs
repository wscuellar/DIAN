

using System.Security.Cryptography.Xml;
using System.Xml;

namespace Gosocket.Dian.Services.Utils.Cryptography.XmlDsig
{
    internal class ExtendedWseSignedXml : SignedXml
    {
        public ExtendedWseSignedXml()
        {
        }

        public ExtendedWseSignedXml(XmlDocument document) : base(document)
        {
        }

        public override XmlElement GetIdElement(XmlDocument document, string idValue)
        {
            if (document == null)
                return (null);

            var elem = document.GetElementById(idValue);
            if (elem != null)
                return elem;
            elem = document.SelectSingleNode("//*[@Id=\"" + idValue + "\"]") as XmlElement;
            if (elem != null)
                return elem;
            elem = document.SelectSingleNode("//*[@id=\"" + idValue + "\"]") as XmlElement;
            if (elem != null)
                return elem;
            elem = document.SelectSingleNode("//*[@ID=\"" + idValue + "\"]") as XmlElement;

            return elem;
        }
    }
}
