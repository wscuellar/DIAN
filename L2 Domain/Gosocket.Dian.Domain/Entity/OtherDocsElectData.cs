using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Domain.Entity
{
    public class OtherDocsElectData
    {
        public OtherDocsElectData() { }

        public int Id { get; set; }
        public int ContributorId { get; set; }
        public string OperationMode { get; set; }
        public int OperationModeId { get; set; }
        public string ElectronicDoc { get; set; }
        public int ElectronicDocId { get; set; }
        public string ContributorType { get; set; }
        public int ContributorTypeId { get; set; }
        public string Software { get; set; }
        public string SoftwareId { get; set; }
        public Guid SoftwareIdBase { get; set; }
        public int ProviderId { get; set; }
        public string PinSW { get; set; }
        public string StateSoftware { get; set; }

        public List<string> LegalRepresentativeIds { get; set; }
        public string StateContributor { get; set; }
        public string Url { get; set; }
        public DateTime CreatedDate { get; set; }

        public int Step { get; set; }
        public string State { get; set; }
        public int Length { get; set; }
    }
}
