namespace Gosocket.Dian.Plugin.Functions.Models
{
    public class HolderExchangeModel
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public bool Active { get; set; }
        public string CorporateStockAmount { get; set; }
        public string CorporateStockAmountSender { get; set; }
        public string GlobalDocumentId { get; set; }
        public string PartyLegalEntity { get; set; }
        public string SenderCode { get; set; }
    }
}
