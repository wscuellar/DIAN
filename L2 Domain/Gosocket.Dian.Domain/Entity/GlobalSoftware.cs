using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalSoftware : TableEntity
    {
        public GlobalSoftware() { }
        public GlobalSoftware(string pk, string rk) : base(pk, rk)
        { }

        public Guid Id { get; set; }
        public bool Deleted { get; set; }
        public string Pin { get; set; }
        public int StatusId { get; set; }
    }
}
