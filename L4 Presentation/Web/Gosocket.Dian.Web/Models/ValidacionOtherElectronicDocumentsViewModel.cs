namespace Gosocket.Dian.Web.Models
{
    public class ValidacionOtherDocsElecViewModel
    {
        public  int ContributorId { get; set; }
        public int UserCode { get; set; }

        public string Accion { get; set; }

        public int ElectronicDocumentId { get; set; }
        public string ComplementoTexto { get; set; }

        public string Message { get; set; }

        public Domain.Common.OtherDocElecContributorType ContributorIdType { get; set; }

        public Domain.Common.OtherDocElecOperationMode OperationModeId { get; set; }

    }
}