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
            Registered,
            [Display(Name = "En pruebas")]
            Test,
            [Display(Name = "Habilitado")]
            Enabled,
            [Display(Name = "Cancelado")]
            Canceled
        }

        public enum UserApprovalStates
        {
            [Display(Name = "En pruebas")]
            Test,
            [Display(Name = "Cancelado")]
            Canceled
        }
    }
}