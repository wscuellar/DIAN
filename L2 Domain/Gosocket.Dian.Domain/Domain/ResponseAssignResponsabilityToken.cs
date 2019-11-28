using Newtonsoft.Json;

namespace Gosocket.Dian.Domain.Domain
{
    public class ResponseAssignResponsabilityToken
    {
        [JsonProperty(PropertyName = "accessToken")]
        public string AccessToken { get; set; }
        [JsonProperty(PropertyName = "idToken")]
        public string TokenId { get; set; }
        [JsonProperty(PropertyName = "tokenType")]
        public string TokenType { get; set; }
        [JsonProperty(PropertyName = "expireIn")]
        public int ExpireIn { get; set; }
        [JsonProperty(PropertyName = "refreshToken")]
        public string RefreshToken { get; set; }
        [JsonProperty(PropertyName = "clientId")]
        public string ClientId { get; set; }
    }
}
