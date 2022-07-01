using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Utils
{
    public class RadianUtil
    {
        public enum UserStates
        {
            [Display(Name = "Registrado")]
            Registrado = 1,
            [Display(Name = "En pruebas")]
            Test = 2,
            [Display(Name = "Habilitado")]
            Habilitado = 3,
            [Display(Name = "Cancelado")]
            Cancelado = 4
        }

        public enum UserApprovalStates
        {
            [Display(Name = "En pruebas")]
            Test,
            [Display(Name = "Cancelado")]
            Canceled,
            [Display(Name = "Actualiza estado requisitos")]
            MissingRequirement
        }

        public enum DocumentStates
        {
            [Display(Name = "Pendiente")]
            Pendiente,
            [Display(Name = "Cargado y en revisión ")]
            Cargado,
            [Display(Name = "Aprobado")]
            Aprobado,
            [Display(Name = "Rechazado")]
            Rechazado,
            [Display(Name = "Observaciones")]
            Observaciones
        }
    }
}