using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Domain.Sql
{
    [System.ComponentModel.DataAnnotations.Schema.Table("OtherDocElecPayroll")]
    public class OtherDocElecPayroll
    {
        public OtherDocElecPayroll()
        {

        }

        [Key]
        public Guid Id { get; set; }
    }
}
