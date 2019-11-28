using Newtonsoft.Json;

namespace Gosocket.Dian.Functions.Models
{
    public class RequestBase
    {
        [JsonProperty(PropertyName = "trackId")]
        public string TrackId { get; set; }
    }
}
