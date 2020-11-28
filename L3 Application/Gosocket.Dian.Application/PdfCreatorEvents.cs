using System;

namespace Gosocket.Dian.Application
{
    public class PdfCreatorEvent
    {
        public int EventNumber { get; set; }
        public DateTime ValidationDate { get; set; }        
        public string CUFE { get; set; }
        public string DIANValidationResponse { get; set; }
        public string ValidatiorName { get; set; }
        public string ReceiverName { get; set; }
    }
}