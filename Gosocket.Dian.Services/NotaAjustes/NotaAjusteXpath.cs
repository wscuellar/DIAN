namespace Gosocket.Dian.Services.NotaAjustes
{
    /// <summary>
    /// Constanstes Xpath de Acuerdo al Anexo Técnico DSNO V1 1  
    /// </summary>
    public static class NotaAjusteXpath
    {
        /// <summary>
        /// /Invoice/cac:TaxTotal/ cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID = 01
        /// </summary>
        public const string PayableAmount = "//*[local-name()='LegalMonetaryTotal']/*[local-name()='PayableAmount']";
    }

}