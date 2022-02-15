using System.Collections.Generic;

namespace Gosocket.Dian.Domain
{

    [System.ComponentModel.DataAnnotations.Schema.Table("RadianOperationMode")]
    public class RadianOperationMode
    {
        public RadianOperationMode() { }

        public RadianOperationMode(int id, string name)
        {
            Id = id;
            Name = name;
        }

        [System.ComponentModel.DataAnnotations.Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<RadianContributor> RadiantContributors { get; set; }
    }

}
