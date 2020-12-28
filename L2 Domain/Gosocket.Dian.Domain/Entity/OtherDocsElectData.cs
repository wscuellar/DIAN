using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string ContibutorType { get; set; }
        public int ContibutorTypeId { get; set; }
        public string Software { get; set; }
        public string SoftwareId { get; set; }
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
