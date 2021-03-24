using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain.Entity;
using System;
using System.Collections.Generic;

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

        public SummaryEventsViewModel(GlobalDocValidatorDocumentMeta eventItem)
        {
            Validations = new List<AssociatedValidationsViewModel>();
            References = new List<AssociatedReferenceViewModel>();
            AssociatedEvents = new List<AssociatedEventsViewModel>();

            Prefix = eventItem.Serie;
            Number = eventItem.Number;
            DateOfIssue = eventItem.SigningTimeStamp.Date;
            SenderCode = eventItem.SenderCode;
            SenderName = eventItem.SenderName;
            ReceiverCode = eventItem.ReceiverCode;
            ReceiverName = eventItem.ReceiverName;
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
            get; set;
        }

        public ElectronicMandateViewModel Mandate { get; set; }

        public List<AssociatedEventsViewModel> AssociatedEvents { get; set; }
        public EndosoViewModel Endoso { get; set; }
        public string EventTitle { get; internal set; }
        public string RequestType { get; internal set; }
        public string ValidationTitle { get; internal set; }
        public string ReferenceTitle { get; internal set; }
    }

    public class AssociatedValidationsViewModel
    {
        public AssociatedValidationsViewModel(GlobalDocValidatorTracking globalDocValidatorTracking)
        {
            RuleName = globalDocValidatorTracking.RuleName;
            Status = TextResources.Event_Status_01;
            Message = globalDocValidatorTracking.ErrorMessage;
        }

        public string RuleName { get; internal set; }
        public string Status { get; internal set; }
        public string Message { get; internal set; }
    }

    public class AssociatedReferenceViewModel
    {

        public AssociatedReferenceViewModel(GlobalDocValidatorDocumentMeta globalDocValidatorDocumentMeta, string documentType, string description)
        {
            Document = documentType;
            DateOfIssue = globalDocValidatorDocumentMeta.EmissionDate.Date;
            Description = description;
            SenderCode = globalDocValidatorDocumentMeta.SenderCode;
            SenderName = globalDocValidatorDocumentMeta.SenderName;
            ReceiverCode = globalDocValidatorDocumentMeta.ReceiverCode;
            ReceiverName = globalDocValidatorDocumentMeta.ReceiverName;
        }

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
        public ElectronicMandateViewModel(GlobalDocValidatorDocumentMeta eventItem, GlobalDocValidatorDocumentMeta invoice)
        {
            TechProviderCode = eventItem.TechProviderCode;
            TechProviderName = eventItem.ReceiverName;
            SenderCode = invoice.SenderCode;
            SenderName = invoice.SenderName;
        }

        public string ContractDate { get; internal set; }
        public string TechProviderCode { get; internal set; }
        public string TechProviderName { get; internal set; }
        public string SenderCode { get; internal set; }
        public string SenderName { get; internal set; }
        public string MandateType { get; internal set; }
        public string SchemeID { get; internal set; }
    }

    public class AssociatedEventsViewModel
    {
        public AssociatedEventsViewModel(GlobalDocValidatorDocumentMeta globalDocValidatorDocumentMeta)
        {
            EventCode = globalDocValidatorDocumentMeta.EventCode;
            Document = Domain.Common.EnumHelper.GetEnumDescription(Enum.Parse(typeof(Domain.Common.EventStatus), globalDocValidatorDocumentMeta.EventCode));
            EventDate = globalDocValidatorDocumentMeta.SigningTimeStamp;
            SenderCode = globalDocValidatorDocumentMeta.ReceiverCode;
            Sender = globalDocValidatorDocumentMeta.SenderName;
            ReceiverCode = globalDocValidatorDocumentMeta.ReceiverCode;
            Receiver = globalDocValidatorDocumentMeta.ReceiverName;
        }


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
        public EndosoViewModel(GlobalDocValidatorDocumentMeta eventItem, GlobalDocValidatorDocumentMeta invoice)
        {
            ReceiverCode = eventItem.ReceiverCode;
            ReceiverName = eventItem.ReceiverName;
            if(eventItem.SenderCode.Length > 0)
            {
                SenderCode = invoice.SenderCode;
                SenderName = invoice.SenderName;
            }
            EndosoType = Domain.Common.EnumHelper.GetEnumDescription((Enum.Parse(typeof(Gosocket.Dian.Domain.Common.EventStatus), eventItem.EventCode)));
        }

        public string ReceiverCode { get; internal set; }
        public string ReceiverName { get; internal set; }
        public string SenderCode { get; internal set; }
        public string SenderName { get; internal set; }
        public string EndosoType { get; internal set; }
    }
}