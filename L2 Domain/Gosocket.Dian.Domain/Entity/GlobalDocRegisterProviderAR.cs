using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalDocRegisterProviderAR : TableEntity
    {
        public GlobalDocRegisterProviderAR() { }

        public GlobalDocRegisterProviderAR(string pk, string rk) : base(pk, rk)
        {

        }

        public string trackId { get; set; }
        public string providerCode { get; set; }
        public string SerieAndNumber { get; set; }
        public string SenderCode { get; set; }
        public string DocumentTypeId { get; set; }
        public string GlobalCufeId { get; set; }
        public string EventCode { get; set; }
        public string CustomizationID { get; set; }
    }
}
