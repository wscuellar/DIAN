using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Gosocket.Dian.Services.Utils
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/UploadDocumentResponse")]
    public class UploadDocumentResponse
    {
        [DataMember]
        public string ZipKey { get; set; }
        [DataMember]
        public List<XmlParamsResponseTrackId> ErrorMessageList { get; set; }

    }
}
