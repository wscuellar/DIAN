using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gosocket.Dian.Web.Models
{
    public class SummaryEventsViewModel
    {
        public SummaryEventsViewModel()
        {
            Validations = new List<AssociatedValidationsViewModel>();
            References = new List<AssociatedReferenceViewModel>();
            AssociatedEvents = new List<AssociatedEventsViewModel>();
        }

        public string Title { get; internal set; }
        public string CUDE { get; internal set; }
        public string Prefix { get; internal set; }
        public string Number { get; internal set; }
        public DateTime DateOfIssue { get; internal set; }
        public string SenderCode { get; internal set; }
        public string ReceiverCode { get; internal set; }
        public string ReceiverName { get; internal set; }
        public string SenderName { get; internal set; }
        public string ValidationMessage { get; internal set; }
        public Domain.Common.EventStatus EventStatus { get; internal set; }
        public List<AssociatedValidationsViewModel> Validations { get; internal set; }

        public List<AssociatedReferenceViewModel> References
        {
            get;set;
        }

        public ElectronicMandateViewModel Mandate { get; set; }

        public List<AssociatedEventsViewModel> AssociatedEvents { get; set; }
        public EndosoViewModel Endoso { get; set; }
        public string EventTitle { get; internal set; }
        public string RequestType { get; internal set; }
    }

    public class AssociatedValidationsViewModel
    {
        public string RuleName { get; internal set; }
        public string Status { get; internal set; }
        public string Message { get; internal set; }
    }

    public class AssociatedReferenceViewModel
    {
        public DateTime DateOfIssue { get; internal set; }
        public string Description { get; internal set; }
        public string SenderCode { get; internal set; }
        public string SenderName { get; internal set; }
        public string ReceiverCode { get; internal set; }
        public string ReceiverName { get; internal set; }
        public string Document { get; internal set; }
    }

    public class ElectronicMandateViewModel
    {
        public string ContractDate { get; internal set; }
        public string ReceiverCode { get; internal set; }
        public string ReceiverName { get; internal set; }
        public string SenderCode { get; internal set; }
        public string SenderName { get; internal set; }
        public string MandateType { get; internal set; }
    }

    public class AssociatedEventsViewModel
    {
        public string EventCode { get; internal set; }
        public string Document { get; internal set; }
        public DateTime EventDate { get; internal set; }
        public string SenderCode { get; internal set; }
        public string Sender { get; internal set; }
        public string ReceiverCode { get; internal set; }
        public string Receiver { get; internal set; }
    }

    public class EndosoViewModel
    {
        public string ReceiverCode { get; internal set; }
        public string ReceiverName { get; internal set; }
        public string SenderCode { get; internal set; }
        public string SenderName { get; internal set; }
        public string EndosoType { get; internal set; }
    }
}