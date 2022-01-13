using System.IO;
using System.Xml;
namespace Gosocket.Dian.Services.Cuds
{
    public class XmlToInvoiceParser
    {
        private readonly XmlDocument xmlDocument;
        public XmlToInvoiceParser()
        {
            xmlDocument = new XmlDocument { PreserveWhitespace = true };
        }
        public InvoiceDs Parser(byte[] xmlContent)
        {
            var invoiceDs = new InvoiceDs();
            using (var ms = new MemoryStream(xmlContent))
            {
                using (var stream = new StreamReader(ms, System.Text.Encoding.UTF8))
                {
                    xmlDocument.Load(stream);
                    invoiceDs.InvoiceTypeCode = SelectSingleNode(InvoiceXpath.InvoiceTypeCode);
                    invoiceDs.NumDs = SelectSingleNode(InvoiceXpath.NumDs);
                    invoiceDs.FecDs = SelectSingleNode(InvoiceXpath.FecDs);
                    invoiceDs.HorDs = SelectSingleNode(InvoiceXpath.HorDs);
                    invoiceDs.ValDs = SelectSingleNode(InvoiceXpath.ValDs);
                    invoiceDs.CodImp = SelectSingleNode(InvoiceXpath.CodImp);
                    invoiceDs.ValImp = SelectSingleNode(InvoiceXpath.ValImp);
                    invoiceDs.ValTol = SelectSingleNode(InvoiceXpath.ValTol);
                    invoiceDs.NumSno = SelectSingleNode(InvoiceXpath.NumSno);
                    invoiceDs.NitAbs = SelectSingleNode(InvoiceXpath.NitAbs);
                    invoiceDs.TipoAmb = SelectSingleNode(InvoiceXpath.TipoAmb);
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
