namespace Gosocket.Dian.Web.Models
{
    public class ValidacionOtherElectronicDocumentsViewModel
    {
        public int UserCode { get; set; }

        public string Accion { get; set; }

        public int ElectronicDocument { get; set; }
        public string ComplementoTexto { get; set; }

        public Domain.Common.OtherDocElecContributorType ContributorIdType { get; set; }

        public Domain.Common.OtherDocElecOperationMode OperationModeId { get; set; }

    }
}