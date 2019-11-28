using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gosocket.Dian.Domain
{
    [Table("UserContributors")]
    public class UserContributors
    {
        [Key, Column(Order = 0)]
        public string UserId { get; set; }

        [Key, Column(Order = 1)]
        public int ContributorId { get; set; }

        public DateTime Timestamp { get; set; }

        public string CreatedBy { get; set; }

        [NotMapped]
        public bool Deleted { get; set; }
    }
}
