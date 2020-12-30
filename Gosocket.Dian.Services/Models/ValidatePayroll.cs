using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Services.Models
{
    class ValidatePayroll
    {
        public decimal AccruedTotal { get; set; }
        public decimal DeductionsTotal { get; set; }
        public decimal VoucherTotal { get; set; }
        public decimal OvertimeSurcharges { get; set; }
        public decimal Salary { get; set; }
        public decimal PercentageWorked { get; set; }
        public int AmountAdditionalHours { get; set; }

        


    }
}
