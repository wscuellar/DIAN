using System.IO;
using System.Xml;
namespace Gosocket.Dian.Services.Cuds
{
    public class XmlToDocumentoSoporteParser
    {
        private readonly XmlDocument xmlDocument;
        public XmlToDocumentoSoporteParser()
        {
            xmlDocument = new XmlDocument { PreserveWhitespace = true };
        }
        public DocumentoSoporte Parser(byte[] xmlContent)
        {
            var invoiceDs = new DocumentoSoporte();
            using (var ms = new MemoryStream(xmlContent))
            {
                using (var stream = new StreamReader(ms, System.Text.Encoding.UTF8))
                {
                    xmlDocument.Load(stream);
                    invoiceDs.SoftwareId = SelectSingleNode(DocumentoSoporteXpath.SoftwareId);
                    invoiceDs.Cuds = SelectSingleNode(DocumentoSoporteXpath.Cuds);
                    invoiceDs.DocumentType = SelectSingleNode(DocumentoSoporteXpath.InvoiceTypeCode);
                    invoiceDs.NumDs = SelectSingleNode(DocumentoSoporteXpath.NumDs);
                    invoiceDs.FecDs = SelectSingleNode(DocumentoSoporteXpath.FecDs);
                    invoiceDs.HorDs = SelectSingleNode(DocumentoSoporteXpath.HorDs);
                    invoiceDs.ValDs = SelectSingleNode(DocumentoSoporteXpath.ValDs);
                    invoiceDs.CodImp = SelectSingleNode(DocumentoSoporteXpath.CodImp);
                    invoiceDs.ValImp = SelectSingleNode(DocumentoSoporteXpath.ValImp);
                    invoiceDs.ValTol = SelectSingleNode(DocumentoSoporteXpath.ValTol);
                    invoiceDs.NumSno = SelectSingleNode(DocumentoSoporteXpath.NumSno);
                    invoiceDs.NitAbs = SelectSingleNode(DocumentoSoporteXpath.NitAbs);
                    invoiceDs.TipoAmb = SelectSingleNode(DocumentoSoporteXpath.TipoAmb);

                    if (string.IsNullOrWhiteSpace(invoiceDs.CodImp))
                    {
                        invoiceDs.CodImp = "01";
                    }
                    if (string.IsNullOrWhiteSpace(invoiceDs.ValImp))
                    {
                        invoiceDs.ValImp = "0.00";
                    }

                    if (string.IsNullOrWhiteSpace(invoiceDs.DocumentType))
                    {
                        invoiceDs.DocumentType = SelectSingleNode(DocumentoSoporteXpath.AdjustmentNoteTypeCode);
                    }
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
