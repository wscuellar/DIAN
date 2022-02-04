namespace Gosocket.Dian.Services.Cude 
{
    /// <summary>
    /// Campos requeridos para el proceso de Validación del Cuds 
    /// de acuerdo al Anexo Técnico DSNO V1 1 del 24 - 11 - 2021
    /// </summary>
    public class DocumentoEquivalente
    {
        /// <summary>
        /// /Invoice/cbc:UUID
        /// </summary>
        public string Cude { get; set; }

        /// <summary>
        /// «/Invoice/cbc:InvoiceTypeCode=05
        /// </summary>
        public string DocumentType { get; set; }

        /// <summary>
        ///  /Invoice/cbc:ID
        /// </summary>
        public string NumFac { get; set; }
        /// <summary>
        /// Invoice/cbc:IssueDate
        /// </summary>
        public string FecFac { get; set; }
        /// <summary>
        /// Invoice/cbc:IssueTime
        /// </summary>
        public string HorFac { get; set; }
        /// <summary>
        /// Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount
        /// </summary>
        public string ValFac { get; set; }
        /// <summary>
        /// Invoice/cac:TaxTotal/ cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID = 01
        /// </summary>
        public string CodImp1 { get; set; }
        /// <summary>
        /// Invoice/cac:TaxTotal/cbc:TaxAmount
        /// </summary>
        public string ValImp1 { get; set; }
        /// <summary>
        /// Invoice/cac:TaxTotal/ cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID = 04
        /// </summary>
        public string CodImp2 { get; set; }
        /// <summary>
        /// Invoice/cac:TaxTotal/cbc:TaxAmount
        /// </summary>
        public string ValImp2 { get; set; }
        /// <summary>
        /// Invoice/cac:TaxTotal/ cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID = 03
        /// </summary>
        public string CodImp3 { get; set; }
        /// <summary>
        /// Invoice/cac:TaxTotal/cbc:TaxAmount
        /// </summary>
        public string ValImp3 { get; set; }
        /// <summary>
        /// Invoice/cac:LegalMonetaryTotal/cbc:PayableAmount
        /// </summary>
        public string ValTol { get; set; }
        /// <summary>
        /// Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID
        /// </summary>
        public string NumOfe { get; set; }
        /// <summary>
        /// Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID
        /// </summary>
        public string NitAdq { get; set; }
        ///No esta en el documento xml
        public string SoftwarePin { get; set; }
        /// <summary>
        /// Invoice/cbc:ProfileExecutionID
        /// </summary>
        public string TipoAmb { get; set; }
        /// <summary>
        /// Invoice/sts:SoftwareProvider/sts:SoftwareID
        /// </summary>
        public string SoftwareId { get; set; }

        /// <summary>
        /// Combinación de acuerdo al Anexo Técnico DOCUMENTO EQUIVALENTE POS-FINAL 2 V1.0
        /// NumFac + FecFac + HorFac + ValFac + CodImp1 + ValImp1+ CodImp2 + ValImp2+ CodImp3 + ValImp3 + ValTot + NitOFE + NumAdq + Software-PIN + TipoAmb
        /// </summary>
        /// <param name="sep"></param>
        /// <returns></returns>
        public string ToCombinacionToCude(string sep = "")
        {
            return $"{NumFac}{sep}{FecFac}{sep}{HorFac}{sep}{ValFac}{sep}{CodImp1}{sep}{ValImp1}{sep}{CodImp2}{sep}{ValImp2}{sep}{CodImp3}{sep}{ValImp3}{sep}{ValTol}{sep}{NumOfe}{sep}{NitAdq}{sep}{SoftwarePin}{sep}{TipoAmb}";

        }


    }

}
