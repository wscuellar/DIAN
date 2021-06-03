
namespace Gosocket.Dian.Services.Utils.Common
{
    public class DocumentParsedNomina
    {
        public string DocumentTypeId { get; set; }
        public string CUNE { get; set; }
        public string CUNEPred { get; set; }
        public string CodigoTrabajador { get; set; }
        public string ProveedorNIT { get; set; }
        public string ProveedorDV { get; set; }
        public string ProveedorSoftwareID { get; set; }
        public string ProveedorSoftwareSC { get; set; }
        public string SerieAndNumber { get; set; }
        public string EmpleadorDV { get; set; }
        public string EmpleadorNIT { get; set; }
        public string NumeroDocumento { get; set; }
        public string TipoNota { get; set; }


        public static void SetValues (ref DocumentParsedNomina documentParsedNomina)
        {
            documentParsedNomina.DocumentTypeId = documentParsedNomina?.DocumentTypeId;
            documentParsedNomina.CUNE = documentParsedNomina?.CUNE?.ToString()?.ToLower();
            documentParsedNomina.CUNEPred = documentParsedNomina?.CUNEPred?.ToString()?.ToLower();
            documentParsedNomina.CodigoTrabajador = documentParsedNomina?.CodigoTrabajador;
            documentParsedNomina.ProveedorNIT = documentParsedNomina?.ProveedorNIT;
            documentParsedNomina.ProveedorDV = documentParsedNomina?.ProveedorDV;
            documentParsedNomina.ProveedorSoftwareID = documentParsedNomina?.ProveedorSoftwareID;
            documentParsedNomina.ProveedorSoftwareSC = documentParsedNomina?.ProveedorSoftwareSC.ToString()?.ToLower();
            documentParsedNomina.SerieAndNumber = documentParsedNomina?.SerieAndNumber.ToString()?.ToUpper();
            documentParsedNomina.TipoNota = documentParsedNomina?.TipoNota;
        }
    }
}
