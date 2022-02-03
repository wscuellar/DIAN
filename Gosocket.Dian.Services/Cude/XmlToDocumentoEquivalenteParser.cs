using System.IO;
using System.Xml;
namespace Gosocket.Dian.Services.Cude
{
    public class XmlToDocumentoEquivalenteParser
    {
        private readonly XmlDocument xmlDocument;
        public XmlToDocumentoEquivalenteParser()
        {
            xmlDocument = new XmlDocument { PreserveWhitespace = true };
        }
        public DocumentoEquivalente Parser(byte[] xmlContent)
        {
            var invoiceDs = new DocumentoEquivalente();
            using (var ms = new MemoryStream(xmlContent))
            {
                using (var stream = new StreamReader(ms, System.Text.Encoding.UTF8))
                {
                    xmlDocument.Load(stream);
                    invoiceDs.SoftwareId = SelectSingleNode(DocumentoEquivalenteXpath.SoftwareId);
                    invoiceDs.Cude = SelectSingleNode(DocumentoEquivalenteXpath.Cude);
                    invoiceDs.DocumentType = SelectSingleNode(DocumentoEquivalenteXpath.InvoiceTypeCode);
                    invoiceDs.NumFac = SelectSingleNode(DocumentoEquivalenteXpath.NumFac);
                    invoiceDs.FecFac = SelectSingleNode(DocumentoEquivalenteXpath.FecFac);
                    invoiceDs.HorFac = SelectSingleNode(DocumentoEquivalenteXpath.HorFac);
                    invoiceDs.ValFac = SelectSingleNode(DocumentoEquivalenteXpath.ValFac);
                    invoiceDs.CodImp1 = SelectSingleNode(DocumentoEquivalenteXpath.CodImp1);
                    invoiceDs.ValImp1 = SelectSingleNode(DocumentoEquivalenteXpath.ValImp1);
                    invoiceDs.ValTol = SelectSingleNode(DocumentoEquivalenteXpath.ValTol);
                    invoiceDs.NumOfe = SelectSingleNode(DocumentoEquivalenteXpath.NumOfe);
                    invoiceDs.NitAdq = SelectSingleNode(DocumentoEquivalenteXpath.NumAdq);
                    invoiceDs.TipoAmb = SelectSingleNode(DocumentoEquivalenteXpath.TipoAmb);
                }
            } 
            return invoiceDs;
        }
        private string SelectSingleNode(string xpath)
        {
            return xmlDocument.SelectSingleNode(xpath)?.InnerText ?? "";
        }
     
    }

}
