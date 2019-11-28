using System.Runtime.Serialization;

namespace Gosocket.Dian.Services.Utils
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/XmlParamsResponseTrackId")]
    public class XmlParamsResponseTrackId
    {
        [DataMember]
        public bool Success{ get; set; }
        [DataMember]
        public string DocumentKey { get; set; }
        [DataMember]
        public string SenderCode { get; set; }
        [DataMember]
        public string XmlFileName { get; set; }
        [DataMember]
        public string ProcessedMessage { get; set; }
    }
}
