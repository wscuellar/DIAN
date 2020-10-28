using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace Gosocket.Dian.Domain
{
    [System.ComponentModel.DataAnnotations.Schema.Table("RadianContributor")]
    public class RadianContributor
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int Id { get; set; }
        public int ContributorId { get; set; }
        public Contributor Contributor { get; set; }
        public int ContributorTypeId { get; set; }
        public ContributorType ContributorType { get; set; }
        public int OperationModelId { get; set; }
        public OperationMode OperationMode { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime Update { get; set; }
        public string CreatedBy { get; set; }
    }

}
