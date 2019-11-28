using System;

namespace Gosocket.Dian.Functions.Utils
{
    public class NumberRangeResponse
    {
        public string AccountCode { get; set; }
        public int DocumentType { get; set; }
        public string Serie { get; set; }
        public Int64 FromNumber { get; set; }
        public Int64 ToNumber { get; set; }
        public string ResolutionNumber { get; set; }
        public string TechnicalKey { get; set; }
        public DateTime? ValidDateTimeFrom { get; set; }
        public DateTime? ValidDateTimeTo { get; set; }
        public DateTime? ResolutionDateTime { get; set; }
    }
}
