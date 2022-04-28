using System;

namespace Gosocket.Dian.Services.NotaAjustes
{
    /// <summary>
    /// Campos requeridos para el proceso de Validación de la nota de ajuste 
    /// de acuerdo al Anexo Técnico DSNO V1 1 del 24 - 11 - 2021
    /// </summary>
    public class NotaAjuste
    {
        /// <summary>
        /// /CreditNote/cac:LegalMonetaryTotal/cbc:PayableAmount 
        /// </summary>
        public string PayableAmount { get; set; }
    }
}
