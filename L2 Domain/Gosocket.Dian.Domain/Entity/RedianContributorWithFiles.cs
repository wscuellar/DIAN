using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Domain.Entity
{

    public class AdminRadianFilter
    {

        public int Id { get; set; }

        public string Code { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public int Type { get; set; }

        public string RadianState { get; set; }

        public int Page { get; set; }

        public int Length { get; set; }


    }


    public class RadianAdmin
    {
        public RedianContributorWithTypes Contributor { get; set; }

        public RadianContributorType Type { get; set; }

        public RadianContributorFile File { get; set; }

        public List<RedianContributorWithTypes> Contributors { get; set; }

        public List<RadianContributorType> Types { get; set; }

        public List<RadianContributorFile> Files { get; set; }

        public List<RadianTestSetResult> Tests { get; set; }

        public List<string> LegalRepresentativeIds { get; set; }

        public int Step { get; set; }
        public int CurrentPage { get; set; }
        public int RowCount { get; set; }
        public List<RadianContributorFileType> FileTypes { get; set; }
    }

    public class RedianContributorWithTypes
    {

        public int Id { get; set; }

        public string Code { get; set; }

        public string TradeName { get; set; }

        public string BusinessName { get; set; }

        public string AcceptanceStatusName { get; set; }

        public string Email { get; set; }

        public int AcceptanceStatusId { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime Update { get; set; }

        public string RadianState { get; set; }
        public int RadianContributorTypeId { get; set; }
        public int Step { get; set; }
        public virtual ICollection<RadianContributor> RadiantContributors { get; set; }
        public int? ContributorTypeId { get; set; }

        public int RadianOperationModeId { get; set; }
        public int RadianContributorId { get; set; }
        public bool IsActive { get; set; }
    }
}
