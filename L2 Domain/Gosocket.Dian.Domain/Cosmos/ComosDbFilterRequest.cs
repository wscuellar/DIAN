using System;

namespace Gosocket.Dian.Domain.Cosmos
{
    public class ComosDbFilterRequest
    {
        public ComosDbFilterRequest()
        {
            ReturnTotals = false;
        }

        public string ContinuationToken { get; set; }
        public string DocumentTypeId { get; set; }
        public int ResultMaxItemCount { get; set; }
        public DateTime EFrom { get; set; }
        public DateTime ETo { get; set; }
        public DateTime RFrom { get; set; }
        public DateTime RTo { get; set; }
        public string ReceiverCode { get; set; }
        public string SenderCode { get; set; }
        public int? Status { get; set; }
        public bool ReturnTotals{ get; set; }
    }
}
