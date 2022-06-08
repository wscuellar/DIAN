using System;

namespace Gosocket.Dian.Services.Cuds 
{
    /// <summary>
    /// Campos requeridos para el proceso de Validación del Cuds 
    /// de acuerdo al Anexo Técnico DSNO V1 1 del 24 - 11 - 2021
    /// </summary>
    public class DocumentoSoporte
    {
        /// <summary>
        /// /Invoice/cbc:UUID
        /// </summary>
        public string Cuds { get; set; }

        /// <summary>
        /// «/Invoice/cbc:InvoiceTypeCode=05
        /// </summary>
        public string DocumentType { get; set; }

        /// <summary>
        ///  /Invoice/cbc:ID
        /// </summary>
        public string NumDs { get; set; }
        /// <summary>
        /// Invoice/cbc:IssueDate
        /// </summary>
        public string FecDs { get; set; }
        /// <summary>
        /// Invoice/cbc:IssueTime
        /// </summary>
        public string HorDs { get; set; }
        /// <summary>
        /// Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount
        /// </summary>
        public string ValDs { get; set; }
        /// <summary>
        /// Invoice/cac:TaxTotal/ cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID = 01
        /// </summary>
        public string CodImp { get; set; }
        /// <summary>
        /// Invoice/cac:TaxTotal/cbc:TaxAmount
        /// </summary>
        public string ValImp { get; set; }
        /// <summary>
        /// Invoice/cac:LegalMonetaryTotal/cbc:PayableAmount
        /// </summary>
        public string ValTol { get; set; }
        /// <summary>
        /// Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID
        /// </summary>
        public string NumSno { get; set; }
        /// <summary>
        /// Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID
        /// </summary>
        public string NitAbs { get; set; }
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
        /// Combinación de acuerdo al Anexo Técnico DSNO_V1_1
        /// NumFac + FecFac + HorFac + ValDS + CodImp + ValImp + ValTot + NitOFE + NumAdq + Software-PIN + TipoAmbie
        /// </summary>
        /// <param name="sep"></param>
        /// <returns></returns>
        public string ToCombinacionToCuds(string sep = "")
        {
            var valDsTruncado = TruncarValoresNumericos(ValDs);
            var valImpTruncado = TruncarValoresNumericos(ValImp);
            var valTot = TruncarValoresNumericos(ValTol);
            return $"{NumDs}{sep}{FecDs}{sep}{HorDs}{sep}{valDsTruncado}{sep}{CodImp}{sep}{valImpTruncado}{sep}{valTot}{sep}{NumSno}{sep}{NitAbs}{sep}{SoftwarePin}{sep}{TipoAmb}";
        }

        public static string TruncarValoresNumericos(string cadena)
        {
            var indexPuntoDecimal = cadena.IndexOf('.');
            if (indexPuntoDecimal == -1)
            {
                return cadena;
            }
            else
            {
                if (indexPuntoDecimal + 2 <= cadena.Length - 1)
                {
                    return cadena.Substring(0, indexPuntoDecimal + 2 + 1);
                }
                else
                {
                    return cadena.Substring(0, cadena.Length);
                }
            }
        }

        public bool IsAdjustmentNote() => DocumentType == "95";
    }

}
