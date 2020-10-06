using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Plugin.Functions.Models
{
    public class DocumentViewModel
    {
        public DocumentViewModel()
        {
            //DocumentTags = new List<DocumentTagViewModel>();
            Events = new List<EventViewModel>();
            //References = new List<ReferenceViewModel>();
            //TaxesDetail = new TaxesDetailViewModel();
        }
        public string Id { get; set; }

        public string PartitionKey { get; set; }
        //public List<DocumentTagViewModel> DocumentTags { get; set; }
        public List<EventViewModel> Events { get; set; }
        // public List<ReferenceViewModel> References { get; set; }

        //public TaxesDetailViewModel TaxesDetail { get; set; }

        public DateTime EmissionDate { get; set; }
        public string DocumentKey { get; set; }
        public string DocumentTypeId { get; set; }

        public string DocumentTypeName { get; set; }

        public string Number { get; set; }
        public string Serie { get; set; }
        public string SerieAndNumber { get; set; }


        public string SenderCode { get; set; }
        public string SenderName { get; set; }
        public string ReceiverCode { get; set; }
        public string ReceiverName { get; set; }
        public string TechProviderCode { get; set; }
        public string TechProviderName { get; set; }
        public DateTime GenerationDate { get; set; }
        public DateTime ReceptionDate { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public double Amount { get; set; }
        public double TaxAmountIva5Percent { get; set; }
        public double TaxAmountIva19Percent { get; set; }
        public double TaxAmountIva { get; set; }
        public double TaxAmountIca { get; set; }
        public double TaxAmountIpc { get; set; }
        public double TotalAmount { get; set; }
    }
}
