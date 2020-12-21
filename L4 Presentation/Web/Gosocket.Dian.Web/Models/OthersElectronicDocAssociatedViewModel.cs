using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Gosocket.Dian.Web.Models
{
    public class OthersElectronicDocAssociatedViewModel
    {
        public OthersElectronicDocAssociatedViewModel()
        {
            PageTable = 1;
        }

        public int Id { get; set; }
        public int PageTable { get; set; }
        public int Step { get; set; }

        public int ContributorId { get; set; }

        [Display(Name = "Tipo de participante")]
        public int ContributorTypeId { get; set; }

        [Display(Name = "NIT")]
        public string Nit { get; set; }

        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Display(Name = "Razón Social")]
        public string BusinessName { get; set; }

        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }
         
        [Display(Name = "Estado de aprobación")]
        public string State { get; set; }

        public Software Software { get; set; }

        // public TestSetResult TestSetResult { get; set; }
        //public ContributorOperationWithSoftware ContributorOperations { get; set; }
        public string OperationMode { get; set; }
        public int OperationModeId { get; set; }
        public string ElectronicDoc { get; set; }
        public int ElectronicDocId { get; set; }
        public string ContibutorType { get; set; }
        public int ContibutorTypeId { get; set; }

    }
}