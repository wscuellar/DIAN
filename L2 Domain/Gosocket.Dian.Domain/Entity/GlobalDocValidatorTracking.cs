using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalDocValidatorTracking : TableEntity
    {
        public Guid RuleId { get; set; }
        public string Category { get; set; }
        public string DocumentTypeCode { get; set; }
        public bool IsValid { get; set; }
        public bool IsNotification { get; set; }
        public bool NotApply { get; set; }
        public bool Mandatory { get; set; }
        public int Priority { get; set; }
        public string ErrorCode { get; set; }
        public string RuleName { get; set; }
        public string ErrorMessage { get; set; }
        public string Status { get; set; }
        public bool BreakOut { get; set; }
        public double? ExecutionTime { get; set; }

        public GlobalDocValidatorTracking() { }

        public GlobalDocValidatorTracking(string partitionKey, string rowKey, Guid ruleId, string category, string docTypeCode, bool isValid, bool mandatory, int priority, string errorCode, string ruleName, string errorMessage, string status)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;

            RuleId = ruleId;
            RuleName = ruleName;
            Category = category;
            DocumentTypeCode = docTypeCode;
            IsValid = isValid;
            Priority = priority;
            Mandatory = mandatory;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            Status = status;
        }
    }
}
