using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models
{
    public class TestSetOthersDocumentsViewModel
    {
        public TestSetOthersDocumentsViewModel()
        {
            Date = DateTime.UtcNow;
        }

        public string TestSetId { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Documento Electrónico")]
        public int ElectronicDocumentId { get; set; }
        public string ElectronicDocumentName { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Modo de Operación")]
        public string OperationModeId { get; set; }
        public string OperationModeName { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Descripción")]
        public string Description { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Documentos")]
        public int TotalDocumentRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        //[Display(Name = "Otros Documentos")]
        public int OthersDocumentsRequired { get; set; }

        //[Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Nomina electrónica de Ajuste")]
        public int? ElectronicPayrollAjustmentRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Documentos")]
        public int TotalDocumentAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Otros Documentos")]
        public int OthersDocumentsAcceptedRequired { get; set; }

        //[Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Nomina electrónica de Ajuste")]
        public int? ElectronicPayrollAjustmentAcceptedRequired { get; set; }

        public DateTime Date { get; set; }

        public string CreatedBy { get; set; }
        public string UpdateBy { get; set; }
        public bool Active { get; set; }

        //public List<ElectronicDocumentViewModel> GetListElectronicDocuments()
        //{
        //    return new List<ElectronicDocumentViewModel>
        //    {
        //        new ElectronicDocumentViewModel{ Id = 1, Name = "Nomina Electronica y Nomina de Ajuste" },
        //        new ElectronicDocumentViewModel{ Id = 2, Name = "Documento de Importación" },
        //        new ElectronicDocumentViewModel{ Id = 3, Name = "Documento de Soporte" },
        //        new ElectronicDocumentViewModel{ Id = 4, Name = "Documento equivalente electrónico" },
        //        new ElectronicDocumentViewModel{ Id = 5, Name = "POS electrónico" }
        //    };
        //}

    }

    

}