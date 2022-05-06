using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Gosocket.Dian.Services.Utils.Common
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/DianResponse")]
    public class DianResponse
    {
        [DataMember]
        public byte[] XmlBytes { get; set; }

        [DataMember]
        public byte[] XmlBase64Bytes { get; set; }

        //[DataMember]
        //public byte[] ZipBase64Bytes { get; set; }

        [DataMember]
        public string StatusCode { get; set; }

        [DataMember]
        public string StatusMessage { get; set; }

        [DataMember]
        public bool IsValid { get; set; }

        [DataMember]
        public string StatusDescription { get; set; }

        [DataMember]
        public string XmlFileName { get; set; }

        [DataMember]
        public string XmlDocumentKey { get; set; }

        [DataMember]
        public List<string> ErrorMessage { get; set; }
        
        //[DataMember]
        //public string UrlDescarga { get; set; }
    }
}
