using System;
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
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor debe ser numérico")]
        public int TotalDocumentRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Range(0, int.MaxValue, ErrorMessage = "El valor debe ser numérico")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor debe ser numérico")]
        //[Display(Name = "Otros Documentos")]
        public int OthersDocumentsRequired { get; set; }

        //[Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Nomina electrónica de Ajuste")]
        [Range(0, int.MaxValue, ErrorMessage = "El valor debe ser numérico")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor debe ser numérico")]
        //[RequiredIf("ElectronicDocumentId == 1", ErrorMessage = "El campo es requerido")]
        public int? ElectronicPayrollAjustmentRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Documentos")]
        [Range(0, int.MaxValue, ErrorMessage = "El valor debe ser numérico")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor debe ser numérico")]
        public int TotalDocumentAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Otros Documentos")]
        [Range(0, int.MaxValue, ErrorMessage = "El valor debe ser numérico")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor debe ser numérico")]
        public int OthersDocumentsAcceptedRequired { get; set; }

        //[Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Nomina electrónica de Ajuste")]

        [Range(0, int.MaxValue, ErrorMessage = "El valor debe ser numérico")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor debe ser numérico")]
        //[RequiredIf("ElectronicDocumentId == 1", ErrorMessage = "El campo es requerido")]
        public int? ElectronicPayrollAjustmentAcceptedRequired { get; set; }

        public DateTime Date { get; set; }

        public string CreatedBy { get; set; }
        public string UpdateBy { get; set; }
        public bool Active { get; set; }
    }
}