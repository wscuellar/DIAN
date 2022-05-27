using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Gosocket.Dian.Services.Utils.Common
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/DianResponse")]
    public class DianResponseBulkDocumentDownload
    {
        [DataMember]
        public string StatusCode { get; set; }

        [DataMember]
        public string StatusMessage { get; set; }

        [DataMember]
        public bool IsValid { get; set; }

        [DataMember]
        public string StatusDescription { get; set; }

        [DataMember]
        public List<string> ErrorMessage { get; set; }
        
        [DataMember]
        public string UrlDescarga { get; set; }
    }
}
