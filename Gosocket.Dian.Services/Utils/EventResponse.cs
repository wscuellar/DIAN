using System.Runtime.Serialization;

namespace Gosocket.Dian.Services.Utils
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/EventResponse")]
    public class EventResponse
    {
        [DataMember]
        public string Code { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public string XmlBytesBase64 { get; set; }
    }
}
