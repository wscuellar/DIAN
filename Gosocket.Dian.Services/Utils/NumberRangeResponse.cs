using System;
using System.Runtime.Serialization;

namespace Gosocket.Dian.Services.Utils
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/NumberRangeResponse")]
    public class NumberRangeResponse
    {
        [DataMember(Order = 1)]
        public string ResolutionNumber { get; set; }
        [DataMember(Order = 2)]
        public string ResolutionDate { get; set; }
        [DataMember(Order = 3)]
        public string Prefix { get; set; }
        [DataMember(Order = 4)]
        public long FromNumber { get; set; }
        [DataMember(Order = 5)]
        public long ToNumber { get; set; }
        [DataMember(Order = 6)]
        public string ValidDateFrom { get; set; }
        [DataMember(Order = 7)]
        public string ValidDateTo { get; set; }
        [DataMember(Order = 8)]
        public string TechnicalKey { get; set; }
    }
}
