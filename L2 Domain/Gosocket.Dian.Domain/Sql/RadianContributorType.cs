using System.Collections.Generic;

namespace Gosocket.Dian.Domain
{

    [System.ComponentModel.DataAnnotations.Schema.Table("RadianContributorType")]
    public class RadianContributorType
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<RadianContributor> RadiantContributors { get; set; }
    }

}
