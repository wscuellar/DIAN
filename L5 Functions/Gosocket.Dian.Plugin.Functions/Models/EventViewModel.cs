using System;

namespace Gosocket.Dian.Plugin.Functions.Models
{
    public class EventViewModel
    {
        public DateTime Date { get; set; }
        public int DateNumber { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string SenderCode { get; set; }
        public string SenderName { get; set; }
        public string ReceiverCode { get; set; }
        public string ReceiverName { get; set; }
    }
}
