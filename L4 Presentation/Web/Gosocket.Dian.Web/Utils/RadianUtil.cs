using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Utils
{
    public class RadianUtil
    {
        public enum UserStates
        {
            [Display(Name = "Registrado")]
            Registrado,
            [Display(Name = "En pruebas")]
            Test,
            [Display(Name = "Habilitado")]
            Habilitado,
            [Display(Name = "Cancelado")]
            Cancelado
        }

        public enum UserApprovalStates
        {
            [Display(Name = "En pruebas")]
            Test,
            [Display(Name = "Cancelado")]
            Canceled
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