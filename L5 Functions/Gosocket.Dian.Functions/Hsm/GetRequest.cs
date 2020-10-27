using Newtonsoft.Json;

namespace Gosocket.Dian.Functions.Hsm
{
    class GetRequest
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}
