using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Sql;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Gosocket.Dian.Domain
{
    [Table("Contributor")]
    public class Contributor
    {
        [Key]
        public int Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string BusinessName { get; set; }

        public string Email { get; set; }

        public string ExchangeEmail { get; set; }

        public int? PersonType { get; set; }

        public DateTime? StartDate { get; set; }
        public int? StartDateNumber { get; set; }

        public DateTime? EndDate { get; set; }
        public int? EndDateNumber { get; set; }

        public bool Status { get; set; }

        public bool Deleted { get; set; }

        public DateTime Timestamp { get; set; }

        public DateTime Updated { get; set; }

        public string CreatedBy { get; set; }

        public string PrincipalActivityCode { get; set; }

        
        public DateTime? ProductionDate { get; set; }

        public DateTime? HabilitationDate { get; set; }

        public int? StatusRut { get; set; }

        public int AcceptanceStatusId { get; set; }
        public virtual AcceptanceStatus AcceptanceStatus { get; set; }

        public int? ContributorTypeId { get; set; }
        public virtual ContributorType ContributorType { get; set; }

        public int? OperationModeId { get; set; }
        public virtual OperationMode OperationMode { get; set; }

        public virtual ICollection<Software> Softwares { get; set; }
        public IEnumerable<Software> SoftwaresInProduction() => Softwares.Where(t => t.AcceptanceStatusSoftwareId == (int)SoftwareStatus.Production && !t.Deleted);

        public int? ProviderId { get; set; }
        public virtual Contributor Provider { get; set; }
        public virtual ICollection<Contributor> Clients { get; set; }

        public virtual ICollection<ContributorFile> ContributorFiles { get; set; }

        public virtual List<ContributorOperations> ContributorOperations { get; set; }

        public virtual ICollection<RadianContributor> RadiantContributors { get; set; }
        public virtual ICollection<OtherDocElecContributor> OtherDocElecContributors { get; set; }
    }
}
