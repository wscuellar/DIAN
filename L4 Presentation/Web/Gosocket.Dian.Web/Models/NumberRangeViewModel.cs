using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models
{
    public class NumberRangeTableViewModel
    {
        public NumberRangeTableViewModel()
        {
            Page = 0;
            Length = 15;
            SearchFinished = false;
            NumberRanges = new List<NumberRangeViewModel>();
        }

        public int Page { get; set; }
        public int Length { get; set; }

        public bool SearchFinished { get; set; }
        public List<NumberRangeViewModel> NumberRanges { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "NIT empresa")]
        [Required(ErrorMessage = "NIT empresa es requerido.")]
        public string Code { get; set; }
    }
    public class NumberRangeViewModel
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Active { get; set; }
        public string Serie { get; set; }
        public string ResolutionNumber { get; set; }
        public string AssociationDate { get; set; }
        public string ExpirationDate { get; set; }
        public DateTime ResolutionDate { get; set; }
        public string ValidDateNumberFrom { get; set; }
        public string ValidDateNumberTo { get; set; }
        public string TechnicalKey { get; set; }
        public long FromNumber { get; set; }
        public long ToNumber { get; set; }

        public string SoftwareId { get; set; }
        public string SoftwareOwner { get; set; }
        public string SoftwareName { get; set; }
    }
}