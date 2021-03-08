using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalDocumentType : TableEntity // Tipos de documentos
    {
        public GlobalDocumentType() { }

        public GlobalDocumentType(string category, string code, string name, string description, DateTime date) : base($"DocumentType|{category}", $"{code}")
        {
            Category = category;
            DocumentTypeId = Guid.NewGuid();
            Code = code;
            Name = name;
            Description = description;
            Date = date;
        }

        public Guid DocumentTypeId { get; set; }
        public DateTime Date { get; set; }
        public string Category { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }

        // xpath configurations

        public string UBLVersionXPath { get; set; }
        public string EmissionDateXPath { get; set; }
        public string SerieXpath { get; set; }
        public string SerieAndNumberXpath { get; set; }
        public string SenderCodeXpath { get; set; }
        public string SenderNameXpath { get; set; }
        public string ReceiverCodeXpath { get; set; }
        public string ReceiverNameXpath { get; set; }
        public string TotalAmountXpath { get; set; }
        public string TotalIVAXpath { get; set; }
        public string TotalICAXpath { get; set; }
        public string TotalIPCXpath { get; set; }
        public string DocumentKeyXpath { get; set; }
        public string DocumentReferencedKeyXpath { get; set; }
        public string ReceiverTypeCodeXpath { get; set; }
        public string SenderTypeCodeXpath { get; set; }
        public string SenderSchemeCodeXpath { get; set; }
        public string InvoiceAuthorizationXpath { get; set; }
    }
}
