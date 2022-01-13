using System.IO;
using System.Xml;
namespace Gosocket.Dian.Services.Cuds
{
    public class XmlToCudsParser
    {
        private readonly XmlDocument xmlDocument;
        public XmlToCudsParser()
        {
            xmlDocument = new XmlDocument { PreserveWhitespace = true };
        }
        public InvoiceCuds Parser(byte[] xmlContent)
        {
            var invoiceDs = new InvoiceCuds();
            using (var ms = new MemoryStream(xmlContent))
            {
                using (var stream = new StreamReader(ms, System.Text.Encoding.UTF8))
                {
                    xmlDocument.Load(stream);
                    invoiceDs.SoftwareId = SelectSingleNode(InvoiceXpath.SoftwareId);
                    invoiceDs.Cuds = SelectSingleNode(InvoiceXpath.Cuds);
                    invoiceDs.DocumentType = SelectSingleNode(InvoiceXpath.InvoiceTypeCode);
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
