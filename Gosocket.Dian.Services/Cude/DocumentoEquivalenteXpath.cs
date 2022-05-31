namespace Gosocket.Dian.Services.Cude
{
    /// <summary>
    /// Constanstes Xpath de Acuerdo al Anexo Técnico DSNO V1 1  
    /// </summary>
    public static class DocumentoEquivalenteXpath
    {
        /// /Invoice/sts:SoftwareProvider/sts:SoftwareID
        public const string SoftwareId = "//*[local-name()='SoftwareProvider']/*[local-name()='SoftwareID']";

        /// <summary>
        /// /Invoice/cbc:UUID
        /// </summary>
        public const string Cude = "//*[local-name()='UUID']";

        /// <summary>
        /// /Invoice/cbc:InvoiceTypeCode=05
        /// </summary>
        public const string InvoiceTypeCode = "//*[local-name()='InvoiceTypeCode']";
        /// <summary>
        /// /Invoice/cbc:InvoiceTypeCode=05
        /// </summary>
        public const string CreditNoteTypeCode = "//*[local-name()='CreditNoteTypeCode']";
        /// <summary>
        /// Invoice/cbc:ID
        /// </summary>
        public const string NumFac = "//*[local-name()='Invoice']/*[local-name()='ID']";
        public const string NumFac1 = "//*[local-name()='ID']";
        //public const string NumFac = "//*[local-name()='ID']";
        /// <summary>
        /// Invoice/cbc:IssueDate
        /// </summary>
        public const string FecFac = "//*[local-name()='Invoice']/*[local-name()='IssueDate']";

        public const string FecFac1 = "//*[local-name()='IssueDate']";
        //public const string FecFac = "//*[local-name()='IssueDate']";
        /// <summary>
        /// "Invoice/cbc:IssueTime"
        /// </summary>
        public const string HorFac = "//*[local-name()='Invoice']/*[local-name()='IssueTime']";
        public const string HorFac1 = "//*[local-name()='IssueTime']";
        //public const string HorFac = "//*[local-name()='IssueTime']";
        /// <summary>
        /// Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount
        /// </summary>
        public const string ValFac = "//*[local-name()='LegalMonetaryTotal']/*[local-name()='LineExtensionAmount']";
        /// <summary>
        /// /Invoice/cac:TaxTotal/ cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID = 01
        /// </summary>
        public const string CodImp1 = "//*[local-name()='TaxTotal'][1]/*[local-name()='TaxSubtotal']/*[local-name()='TaxCategory']/*[local-name()='TaxScheme']/*[local-name()='ID']";
        /// <summary>
        /// "/Invoice/cac:TaxTotal/cbc:TaxAmount"
        /// </summary>
        public const string ValImp1 = "//*[local-name()='TaxTotal'][1]/*[local-name()='TaxAmount']";

        /// <summary>
        /// /Invoice/cac:TaxTotal[y]/ cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID = 04
        /// </summary>
        public const string CodImp2 = "//*[local-name()='TaxTotal'][2]/*[local-name()='TaxSubtotal']/*[local-name()='TaxCategory']/*[local-name()='TaxScheme']/*[local-name()='ID']";
        /// <summary>
        /// /Invoice/cac:TaxTotal[y]/cbc:TaxAmount
        /// </summary>
        public const string ValImp2 = "//*[local-name()='TaxTotal'][2]/*[local-name()='TaxAmount']";

        /// <summary>
        /// /Invoice/cac:TaxTotal[y]/ cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID = 03
        /// </summary>
        public const string CodImp3 = "//*[local-name()='TaxTotal'][3]/*[local-name()='TaxSubtotal']/*[local-name()='TaxCategory']/*[local-name()='TaxScheme']/*[local-name()='ID']";
        /// <summary>
        /// /Invoice/cac:TaxTotal[y]/cbc:TaxAmount
        /// </summary>
        public const string ValImp3 = "//*[local-name()='TaxTotal'][3]/*[local-name()='TaxAmount']";

        /// <summary>
        /// "/Invoice/cac:LegalMonetaryTotal/cbc:PayableAmount"
        /// </summary>
        public const string ValTol = "//*[local-name()='LegalMonetaryTotal']/*[local-name()='PayableAmount']";
        /// <summary>
        /// "/Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID"
        /// </summary>
        public const string NumOfe = "//*[local-name()='AccountingSupplierParty']/*[local-name()='Party']/*[local-name()='PartyTaxScheme']/*[local-name()='CompanyID']";
        /// <summary>
        /// "/Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID"
        /// </summary>
        public const string NumAdq = "//*[local-name()='AccountingCustomerParty']/*[local-name()='Party']/*[local-name()='PartyTaxScheme']/*[local-name()='CompanyID']";
        /// <summary>
        /// "/Invoice/cbc:ProfileExecutionID"
        /// </summary>
        public const string TipoAmb = "//*[local-name()='ProfileExecutionID']";
    }

}