using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Models
{
   public class NotificationEntity
    {
        [JsonProperty("PartitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty("AccountId")]
        public string AccountId { get; set; }

        [JsonProperty("NotificationId")]
        public long NotificationId { get; set; }

        [JsonProperty("State")]
        public long State { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("Subject")]
        public string Subject { get; set; }

        [JsonProperty("NoticationType")]
        public long NoticationType { get; set; }

        [JsonProperty("CreationDateTime")]
        public DateTime CreationDateTime { get; set; }

        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("_rid")]
        public string Rid { get; set; }

        [JsonProperty("_self")]
        public string Self { get; set; }

        [JsonProperty("_etag")]
        public string Etag { get; set; }

        [JsonProperty("_attachments")]
        public string Attachments { get; set; }

        [JsonProperty("_ts")]
        public long Ts { get; set; }
    }
}
