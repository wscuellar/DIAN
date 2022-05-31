using System.Collections.Generic;
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
            Dictionary<string, string> impuestos= new Dictionary<string, string>();
            var invoiceDs = new DocumentoEquivalente();
            using (var ms = new MemoryStream(xmlContent))
            {
                using (var stream = new StreamReader(ms, System.Text.Encoding.UTF8))
                {
                    xmlDocument.Load(stream);
                    invoiceDs.SoftwareId = SelectSingleNode(DocumentoEquivalenteXpath.SoftwareId);
                    invoiceDs.Cude = SelectSingleNode(DocumentoEquivalenteXpath.Cude);
                    invoiceDs.DocumentType = SelectSingleNode(DocumentoEquivalenteXpath.InvoiceTypeCode) != "" ? SelectSingleNode(DocumentoEquivalenteXpath.InvoiceTypeCode) : SelectSingleNode(DocumentoEquivalenteXpath.CreditNoteTypeCode);
                    invoiceDs.NumFac = SelectSingleNode(DocumentoEquivalenteXpath.NumFac) != "" ? SelectSingleNode(DocumentoEquivalenteXpath.NumFac) : SelectSingleNode(DocumentoEquivalenteXpath.NumFac1);
                    invoiceDs.FecFac = SelectSingleNode(DocumentoEquivalenteXpath.FecFac) != "" ? SelectSingleNode(DocumentoEquivalenteXpath.FecFac): SelectSingleNode(DocumentoEquivalenteXpath.FecFac1);
                    invoiceDs.HorFac = SelectSingleNode(DocumentoEquivalenteXpath.HorFac) != "" ? SelectSingleNode(DocumentoEquivalenteXpath.HorFac) : SelectSingleNode(DocumentoEquivalenteXpath.HorFac1);
                    invoiceDs.ValFac = SelectSingleNode(DocumentoEquivalenteXpath.ValFac);
                    invoiceDs.CodImp1 = SelectSingleNode(DocumentoEquivalenteXpath.CodImp1);
                    invoiceDs.ValImp1 = SelectSingleNode(DocumentoEquivalenteXpath.ValImp1);
                    invoiceDs.CodImp2 = SelectSingleNode(DocumentoEquivalenteXpath.CodImp2);
                    invoiceDs.ValImp2 = SelectSingleNode(DocumentoEquivalenteXpath.ValImp2);
                    invoiceDs.CodImp3 = SelectSingleNode(DocumentoEquivalenteXpath.CodImp3);
                    invoiceDs.ValImp3 = SelectSingleNode(DocumentoEquivalenteXpath.ValImp3);
                    
                    var codImp1 = SelectSingleNode(DocumentoEquivalenteXpath.CodImp1);
                    var valImp1 = SelectSingleNode(DocumentoEquivalenteXpath.ValImp1);
                    impuestos.Add(codImp1, valImp1);
                    var codImp2 = SelectSingleNode(DocumentoEquivalenteXpath.CodImp2);
                    var valImp2 = SelectSingleNode(DocumentoEquivalenteXpath.ValImp2);
                    if (!string.IsNullOrWhiteSpace(codImp2))
                    {
                        if (!impuestos.ContainsKey(codImp2))
                            impuestos.Add(codImp2, valImp2);
                    }
                    var codImp3 = SelectSingleNode(DocumentoEquivalenteXpath.CodImp3);
                    var valImp3 = SelectSingleNode(DocumentoEquivalenteXpath.ValImp3);
                    if (!string.IsNullOrWhiteSpace(codImp3))
                    {
                        if (!impuestos.ContainsKey(codImp3))
                            impuestos.Add(codImp3, valImp3);
                    }


                    invoiceDs.ValTol = SelectSingleNode(DocumentoEquivalenteXpath.ValTol);
                    invoiceDs.NumOfe = SelectSingleNode(DocumentoEquivalenteXpath.NumOfe);
                    invoiceDs.NitAdq = SelectSingleNode(DocumentoEquivalenteXpath.NumAdq);
                    invoiceDs.TipoAmb = SelectSingleNode(DocumentoEquivalenteXpath.TipoAmb);

                    invoiceDs.CodImp1 = "01";
                    invoiceDs.ValImp1 = impuestos.ContainsKey("01") ? impuestos["01"] : "0.00";
                    invoiceDs.CodImp2 = "04";
                    invoiceDs.ValImp2 = impuestos.ContainsKey("04") ? impuestos["04"] : "0.00";
                    invoiceDs.CodImp3 = "03";
                    invoiceDs.ValImp3 = impuestos.ContainsKey("03") ? impuestos["03"] : "0.00";
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
