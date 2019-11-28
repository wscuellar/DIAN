using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalDocValidatorRule : TableEntity
    {
        public GlobalDocValidatorRule() { }
        public GlobalDocValidatorRule(string pk, string rk) : base(pk, rk) { }

        public Guid RuleId { get; set; }
        public string Category { get; set; }
        public DateTime Created { get; set; } // Created
        public DateTime Updated { get; set; } // Updated
        public string CreatedBy { get; set; } // CreatedBy
        public string UpdatedBy { get; set; } // UpdatedBy
        public string Name { get; set; }
        public string Description { get; set; }
        public int Type { get; set; }
        public Guid? TypeListId { get; set; }
        public string DocumentTypeCode { get; set; }
        public string XPath { get; set; }
        public string XpathEvaluation { get; set; }
        public string Value { get; set; }
        public int Priority { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public bool BreakOut { get; set; }
        public bool Mandatory { get; set; }
    }
}
