using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models
{
    public class OperationModeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ContributorOperationsTableViewModel
    {
        public ContributorOperationsTableViewModel()
        {
            Page = 0;
            Length = 10;
            ContributorOperations = new List<ContributorOperationsViewModel>();
        }

        public int ContributorId { get; set; }
        public ContributorViewModel Contributor { get; set; }
        public int Page { get; set; }
        public int Length { get; set; }

        public List<ContributorOperationsViewModel> ContributorOperations { get; set; }
    }

    public class ContributorOperationsViewModel
    {
        [Display(Name = "Identificador")]
        public int Id { get; set; }
        public bool Deleted { get; set; }
        public int StatusId { get; set; }
        public string Status { get; set; }

        public int ContributorId { get; set; }
        public ContributorViewModel Contributor { get; set; }

        public int OperationModeId { get; set; }
        public OperationModeViewModel OperationMode { get; set; }

        public int? ProviderId { get; set; }
        public ContributorViewModel Provider { get; set; }

        public Guid? SoftwareId { get; set; }
        public SoftwareViewModel Software { get; set; }
        public DateTime Timestamp { get; set; }

    }
}