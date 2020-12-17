﻿using System.Collections.Generic;

namespace Gosocket.Dian.Domain.Sql
{

    [System.ComponentModel.DataAnnotations.Schema.Table("OtherDocElecSoftwareStatus")]
    public class OtherDocElecSoftwareStatus
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<OtherDocElecSoftware> OtherDocElecSoftware { get; set; }
    }
}
