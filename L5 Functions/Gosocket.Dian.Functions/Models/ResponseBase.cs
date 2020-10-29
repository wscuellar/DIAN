using Newtonsoft.Json;

namespace Gosocket.Dian.Functions.Models
{
    class ResponseBase
    {
        [JsonProperty(PropertyName = "ok")]
        public bool Ok { get; set; }
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}
