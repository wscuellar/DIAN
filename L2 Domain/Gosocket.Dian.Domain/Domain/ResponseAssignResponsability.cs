using Newtonsoft.Json;

namespace Gosocket.Dian.Domain.Domain
{
    public class ResponseAssignResponsability
    {
        [JsonProperty(PropertyName = "codigoRespuesta")]
        public string Code { get; set; }
        [JsonProperty(PropertyName = "severidad")]
        public string Severity { get; set; }
        [JsonProperty(PropertyName = "descripcionEstado")]
        public string StatusDescription { get; set; }
        [JsonProperty(PropertyName = "idTransaccionOrigen")]
        public string OriginTransactionId { get; set; }
        [JsonProperty(PropertyName = "fechaRespuesta")]
        public string ResponseDate { get; set; }
    }
}
