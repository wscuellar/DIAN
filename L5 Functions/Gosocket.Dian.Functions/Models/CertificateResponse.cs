using Newtonsoft.Json;

namespace Gosocket.Dian.Functions.Models
{
    class CertificateResponse : ResponseBase
    {
        [JsonProperty(PropertyName = "content")]
        public string Content { get; set; }
    }
}
