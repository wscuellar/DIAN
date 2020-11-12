namespace Gosocket.Dian.Web.Models
{
    public class RegistrationDataViewModel
    {
        public int ContributorId { get; set; }
        public Domain.Common.RadianContributorType RadianContributorType { get; set; }
        public Domain.Common.RadianOperationMode RadianOperationMode { get; set; }
    }
}