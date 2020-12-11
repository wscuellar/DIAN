using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalDocValidatorDocumentMeta : TableEntity
    {
        public GlobalDocValidatorDocumentMeta() { }

        public GlobalDocValidatorDocumentMeta(string pk, string rk) : base(pk, rk)
        {

        }

        public string UblVersion { get; set; }
        public DateTime EmissionDate { get; set; }
        public int DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; }
        public string SenderCode { get; set; }
        public string SenderName { get; set; }
        public string ReceiverCode { get; set; }
        public string ReceiverName { get; set; }
        public string Number { get; set; }
        public string Serie { get; set; }
        public string SerieAndNumber { get; set; }
        public string DocumentKey { get; set; }
        public double TotalAmount { get; set; }
        public double FreeAmount { get; set; }
        public double TaxAmountIva { get; set; }
        public double TaxAmountIca { get; set; }
        public double TaxAmountIpc { get; set; }
        public string FileName { get; set; }
        public string EventCode { get; set; }
        public DateTime SigningTimeStamp { get; set; }
    }
}
