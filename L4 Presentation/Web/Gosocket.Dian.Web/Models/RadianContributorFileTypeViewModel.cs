using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Models
{
    public class RadianContributorFileTypeTableViewModel
    {
        public RadianContributorFileTypeTableViewModel()
        {
            Page = 0;
            Length = 10;
            RadianContributorFileTypes = new List<RadianContributorFileTypeViewModel>();
        }
        public int Page { get; set; }
        public int Length { get; set; }

        public bool SearchFinished { get; set; }

        [Display(Name = "Tipo de Participante")]
        public string SelectedRadianContributorTypeId { get; set; }
        public SelectList RadianContributorTypes { get; set; }

        public List<RadianContributorFileTypeViewModel> RadianContributorFileTypes { get; set; }
        public RadianContributorFileTypeViewModel RadianContributorFileTypeViewModel { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Nombre")]
        public string Name { get; set; }
    }

    public class RadianContributorFileTypeViewModel
    {
        public int Id { get; set; }

        public bool HideDelete { get; set; }

        [Required (ErrorMessage = "El campo nombre es requerido")]
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        public DateTime Timestamp { get; set; }
        public DateTime Updated { get; set; }

        [Display(Name = "Obligatorio")]
        public bool Mandatory { get; set; }

        [Display(Name = "Tipo de Participante")]
        public RadianContributorType RadianContributorType { get; set; }

        [Display(Name = "Tipo de Participante")]
        public string SelectedRadianContributorTypeId { get; set; }
        public SelectList RadianContributorTypes { get; set; }
    }
}