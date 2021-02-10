using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gosocket.Dian.Web.Utils;
using System.ComponentModel.DataAnnotations;
using Gosocket.Dian.Domain;

namespace Gosocket.Dian.Web.Models.RadianApproved
{
    public class RadianGetSetTestViewModel
    {
        public int RadianContributorId { get; set; }
        public string Nit { get; set; }
        public string RadianState { get; set; }
        public int RadianContributorTypeId { get; set; }
        public int ContributorId { get; set; }
        public string SoftwareId { get; set; }
        public int SoftwareType { get; set; }
        public int OperationMode { get; set; }
    }

}