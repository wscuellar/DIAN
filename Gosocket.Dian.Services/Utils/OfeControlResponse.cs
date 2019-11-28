using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Gosocket.Dian.Services.Utils
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/OfeControlResponse")]
    public class OfeControlResponse
    {
        [DataMember]
        [Display(Name = "NIT emisor")]
        public string SenderCode { get; set; }
        [DataMember]
        [Display(Name = "Nombre emisor")]
        public string SenderName { get; set; }
        [DataMember]
        [Display(Name = "Fecha inicio obligatoriedad")]
        public string StartDate { get; set; }
    }
}
