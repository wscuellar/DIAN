using System.IO;
using System.Xml;
namespace Gosocket.Dian.Services.NotaAjustes
{
    public class XmlToNotaAjusteParser
    {
        private readonly XmlDocument xmlDocument;
        public XmlToNotaAjusteParser()
        {
            xmlDocument = new XmlDocument { PreserveWhitespace = true };
        }
        public NotaAjuste Parser(byte[] xmlContent)
        {
            var notaAjuste = new NotaAjuste();
            using (var ms = new MemoryStream(xmlContent))
            {
                using (var stream = new StreamReader(ms, System.Text.Encoding.UTF8))
                {
                    xmlDocument.Load(stream);
                    notaAjuste.PayableAmount = SelectSingleNode(NotaAjusteXpath.PayableAmount);
                }
            } 
            return notaAjuste;
        }

        private string SelectSingleNode(string xpath)
        {
            return xmlDocument.SelectSingleNode(xpath)?.InnerText ?? "";
        }
     
    }

}
