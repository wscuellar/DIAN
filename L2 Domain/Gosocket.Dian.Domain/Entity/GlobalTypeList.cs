using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalTypeList : TableEntity
    {
        public GlobalTypeList() { }

        public Guid TypeListId { get; set; }
        public string Category { get; set; }
        public int Code { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Value2 { get; set; }
        public bool Locked { get; set; }
        public string Author { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
    }
}
