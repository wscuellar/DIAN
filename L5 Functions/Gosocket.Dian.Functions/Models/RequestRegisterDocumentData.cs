using Newtonsoft.Json;

namespace Gosocket.Dian.Functions.Models
{
    public class RequestRegisterDocumentData : RequestBase
    {
        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }
        [JsonProperty(PropertyName = "xmlBase64")]
        public string XmlBase64 { get; set; }
        [JsonProperty(PropertyName = "fileName")]
        public string FileName { get; set; }
        [JsonProperty(PropertyName = "documentTypeId")]
        public string DocumentTypeId { get; set; }
        [JsonProperty(PropertyName = "zipKey")]
        public string ZipKey { get; set; }
        [JsonProperty(PropertyName = "softwareId")]
        public string SoftwareId { get; set; }
        [JsonProperty(PropertyName = "testSetId")]
        public string TestSetId { get; set; }
    }
}
