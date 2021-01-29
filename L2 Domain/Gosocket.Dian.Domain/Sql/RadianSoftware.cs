using Gosocket.Dian.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Domain
{

    [System.ComponentModel.DataAnnotations.Schema.Table("RadianSoftware")]
    public class RadianSoftware
    {

        public RadianSoftware()
        {

        }


        public RadianSoftware(Software software, int contributorId, string createdBy)
        {
            Id = software.Id;
            Name = software.Name;
            Pin = software.Pin;
            ContributorId = contributorId;
            SoftwareDate = software.SoftwareDate;
            SoftwarePassword = software.SoftwarePassword;
            SoftwareUser = software.SoftwareUser;
            CreatedBy = createdBy;
            Deleted = false;
            Status = true;
            RadianSoftwareStatusId = (int)RadianSoftwareStatus.InProcess;
            Url = software.Url;
            Timestamp = software.Timestamp;
            Updated = software.Updated;
        }

        [Key]
        public Guid Id { get; set; }

        public int ContributorId { get; set; }

        public string Name { get; set; }

        public string Pin { get; set; }

        public DateTime? SoftwareDate { get; set; }

        public string SoftwareUser { get; set; }

        public string SoftwarePassword { get; set; }

        public string Url { get; set; }

        public bool Status { get; set; }

        public int RadianSoftwareStatusId { get; set;}

        public bool Deleted { get; set; }

        public DateTime Timestamp { get; set; }

        public DateTime Updated { get; set; }

        public string CreatedBy { get; set; }

        public virtual ICollection<RadianContributorOperation> RadianContributorOperations { get; set; }

    }

}
