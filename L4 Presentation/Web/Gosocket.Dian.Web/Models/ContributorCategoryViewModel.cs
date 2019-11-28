using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Gosocket.Dian.Web.Models
{
    public class ContributorCategoryTableViewModel
    {
        public ContributorCategoryTableViewModel()
        {
            Page = 0;
            Length = 15;
            SearchFinished = false;
            ContributorCategories = new List<ContributorCategoryViewModel>();
        }

        public int Page { get; set; }
        public int Length { get; set; }

        public bool SearchFinished { get; set; }
        public List<ContributorCategoryViewModel> ContributorCategories { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "NIT empresa")]
        [Required(ErrorMessage = "NIT empresa es requerido.")]
        public string Code { get; set; }
    }
    public class ContributorCategoryViewModel
    {
        public string CategoryId { get; set; }
        public string ActivityCodes { get; set; }
        public DateTime MaximumEmissionDate { get; set; }
        public DateTime MaximumRegistrationDate { get; set; }
        public int ResolutionNumber { get; set; }
        public DateTime ResolutionDate { get; set; }
    }
}