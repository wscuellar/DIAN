using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Web.Models
{

    public class QueryEventViewModel
    {
        public List<EventsViewModel> Events { get; set;}
    }

    public class EventsViewModel
    {
        public EventsViewModel()
        {
            
        }

        public string DocumentKey { get; set; }
        public string EventCode { get; internal set; }
        public string Description { get; internal set; }
        public DateTime EventDate { get; internal set; }
        public string SenderCode { get; internal set; }
        public string Sender { get; internal set; }
        public string ReceiverCode { get; internal set; }
        public string Receiver { get; internal set; }
        
    }
}