
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
        }
    }
}
