using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gosocket.Dian.Web.Utils;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models.RadianApproved
{
    public class RadianGetSetTestViewModel
    {
        public int RadianContributorId { get; set; }
        public string Nit { get; set; }
        public string RadianState { get; set; }
        public int RadianContributorTypeId { get; set; }
        public string ContributorId { get; set; }
    }

}