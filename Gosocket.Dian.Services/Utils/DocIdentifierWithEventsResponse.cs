using System.Runtime.Serialization;

namespace Gosocket.Dian.Services.Utils
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/DocIdentifierWithEventsResponse")]
    public class DocIdentifierWithEventsResponse
    {
        [DataMember]
        public string StatusCode { get; set; }
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public byte[] CsvBase64Bytes { get; set; }
    }
}