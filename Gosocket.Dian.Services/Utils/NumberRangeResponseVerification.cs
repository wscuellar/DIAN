using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Gosocket.Dian.Services.Utils
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/NumberRangeResponseList")]
    public class NumberRangeResponseList
    {
        [DataMember]
        public string OperationCode { get; set; }
        [DataMember]
        public string OperationDescription { get; set; }
        [DataMember]
        public List<NumberRangeResponse> ResponseList { get; set; }
    }
}
