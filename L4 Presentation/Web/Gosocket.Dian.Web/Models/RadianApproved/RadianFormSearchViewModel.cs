using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gosocket.Dian.Web.Utils;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models.RadianApproved
{
    public class RadianFormSearchViewModel
    {
        [Display(Name = "Nit Facturador")]
        public string Nit { get; set; }
        public RadianUtil.UserStates RadianState { get; set; }
    }
}