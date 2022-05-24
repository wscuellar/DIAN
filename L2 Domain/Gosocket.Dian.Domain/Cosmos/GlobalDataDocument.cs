using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Domain.Cosmos
{
    public class GlobalDataDocument : Resource
    {
        public GlobalDataDocument()
        {
            DocumentTags = new List<DocumentTag>();
            Events = new List<Event>();
            References = new List<Reference>();
        }

        public string PartitionKey { get; set; }
        public string Identifier { get; set; }
        public string DocumentKey { get; set; }
        public string GlobalDocumentId { get; set; }
        public Guid GlobalNumberRangeId { get; set; }
        public DateTime EmissionDate { get; set; }
        public int EmissionDateNumber { get; set; }
        public DateTime GenerationTimeStamp { get; set; }
        public DateTime ReceptionTimeStamp { get; set; }
        public DateTime? SigningTimeStamp { get; set; }
        public List<DocumentTag> DocumentTags { get; set; }
        public List<Event> Events { get; set; }
        public TaxesDetail TaxesDetail { get; set; }
        public TechProvider TechProviderInfo { get; set; }
        public ValidationResult ValidationResultInfo { get; set; }
        public List<Reference> References { get; set; }
        public string SenderCode { get; set; }
        public string SenderName { get; set; }
        public string ReceiverCode { get; set; }
        public string ReceiverName { get; set; }
        public double TotalAmount { get; set; }
        public double FreeAmount { get; set; }
        public double TaxAmountIva { get; set; }
        public double TaxAmountIca { get; set; }
        public double TaxAmountIpc { get; set; }
        public string Serie { get; set; }
        public string Number { get; set; }
        public string SerieAndNumber { get; set; }
        public string DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; }
        public ulong? GlobalNSUCode { get; set; }
        public string SoftwareId { get; set; }
        public string CustomizationID { get; set; }
        public string DocumentCurrencyCode { get; set; }


        public string id { get; set; }
        public string _rid { get; set; }
        public string _self { get; set; }
        public string _etag { get; set; }
        public string _attachments { get; set; }
        public int _ts { get; set; }
    }

    public class DocumentTag
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }
        public DateTime TimeStamp { get; set; }
    }
    public class DocumentTagMessage
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string DocumentKey { get; set; }
        public string Value { get; set; }
        public DateTime Timestamp { get; set; }
    }
    public class Event
    {
        public string prefijo;

        public string DocumentKey { get; set; }
        public DateTime Date { get; set; }
        public int DateNumber { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Code { get; set; }
        public string CustomizationID { get; set; }
        public string Description { get; set; }
        public string SenderCode { get; set; }
        public string SenderName { get; set; }
        public string ReceiverCode { get; set; }
        public string ReceiverName { get; set; }
        public string CancelElectronicEvent { get; set; }
        public string SendTestSet { get; set; }
    }
    public class Reference
    {
        public string DocumentTypeId { get; set; }
        public string DocumenTypeName { get; set; }
        public DateTime Date { get; set; }
        public int DateNumber { get; set; }
        public DateTime TimeStamp { get; set; }
        public string DocumentKey { get; set; }
        public string Description { get; set; }
        public string SenderCode { get; set; }
        public string SenderName { get; set; }
        public string ReceiverCode { get; set; }
        public string ReceiverName { get; set; }
    }
    public class TaxesDetail
    {
        public double? TaxAmountIva5Percent { get; set; }
        public double TaxAmountIva14Percent { get; set; }
        public double TaxAmountIva16Percent { get; set; }
        public double? TaxAmountIva19Percent { get; set; }
        public double? TaxAmountIva { get; set; }
        public double? TaxAmountIca { get; set; }
        public double? TaxAmountIpc { get; set; }
    }
    public class TechProvider
    {
        public string TechProviderCode { get; set; }
        //public string TechProviderName { get; set; }
        //public ulong? TechProviderNSUCode { get; set; }
    }
    public class ValidationResult
    {
        public int Status { get; set; }//1-Aceptado, 10-Aceptado con reparos, 2 Rechazado
        public string StatusName { get; set; }//1-Aceptado, 10-Aceptado con reparos, 2 Rechazado
        public string CategoryCode { get; set; }//Codigo de la categoria en el validador
        public int TotalCheckedRules { get; set; }
        public int MandatoryOk { get; set; }
        public int MandatoryFails { get; set; }
        public int NoMandatoryFails { get; set; }
        public int NoMandatoryOk { get; set; }
        public double ProcessTime { get; set; }
        public DateTime ValidationTimeStamp { get; set; }
    }
}