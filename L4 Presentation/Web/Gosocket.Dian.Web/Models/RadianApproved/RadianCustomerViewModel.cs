using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gosocket.Dian.Web.Models.RadianApproved
{
    public class RadianCustomerViewModel
    {
        public RadianCustomerViewModel()
        {
            Lenght = 10;
            Page = 1;
        }
        public int Lenght { get; set; }
        public int Page { get; set; }
        public int Nit { get; set; }
        public int RadianApproveState { get; set; }
        public int BussinessName { get; set; }
        public string RadianState { get; set; }
    }
}