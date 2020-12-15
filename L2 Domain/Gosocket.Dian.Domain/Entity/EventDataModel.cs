namespace Gosocket.Dian.Domain.Entity
{
    #region Using

    using Gosocket.Dian.Common.Resources;
    using Gosocket.Dian.Domain.Common;
    using System;
    using System.Collections.Generic;

    #endregion

    public class EventDataModel
    {
        public EventDataModel()
        {
            Validations = new List<AssociatedValidationsModel>();
            References = new List<AssociatedReferenceModel>();
            AssociatedEvents = new List<AssociatedEventsModel>();
        }

        public EventDataModel(GlobalDocValidatorDocumentMeta eventItem)
        {
            Validations = new List<AssociatedValidationsModel>();
            References = new List<AssociatedReferenceModel>();
            AssociatedEvents = new List<AssociatedEventsModel>();


        }

        public string Title { get; set; }
        public string CUDE { get; set; }
        public string Prefix { get; set; }
        public string Number { get; set; }
        public DateTime DateOfIssue { get; set; }
        public string SenderCode { get; set; }
        public string ReceiverCode { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverType { get; set; }
        public string SenderName { get; set; }
        public bool ShowTitleValueSection { get; set; }
        public string ValidationMessage { get; set; }
        public EventStatus EventStatus { get; set; }
        public List<AssociatedValidationsModel> Validations { get; set; }

        public List<AssociatedReferenceModel> References
        {
            get; set;
        }

        public ElectronicMandateModel Mandate { get; set; }

        public List<AssociatedEventsModel> AssociatedEvents { get; set; }
        public EndosoModel Endoso { get; set; }
        public string EventTitle { get; set; }
        public string RequestType { get; set; }
        public string ValidationTitle { get; set; }
        public string ReferenceTitle { get; set; }
    }

    public class AssociatedValidationsModel
    {
        public AssociatedValidationsModel(GlobalDocValidatorTracking globalDocValidatorTracking)
        {
            RuleName = globalDocValidatorTracking.RuleName;
            Status = TextResources.Event_Status_01;
            Message = globalDocValidatorTracking.ErrorMessage;
        }

        public string RuleName { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }

    public class AssociatedReferenceModel
    {

        public AssociatedReferenceModel()
        {

        }

        public DateTime DateOfIssue { get; set; }
        public string Description { get; set; }
        public string SenderCode { get; set; }
        public string SenderName { get; set; }
        public string ReceiverCode { get; set; }
        public string ReceiverName { get; set; }
        public string Document { get; set; }
        public string CUFE { get; set; }
        public string Number { get; set; }
        public double TotalAmount { get; set; }
    }

    public class ElectronicMandateModel
    {
        public ElectronicMandateModel()
        {

        }

        public string ContractDate { get; set; }
        public string ReceiverCode { get; set; }
        public string ReceiverName { get; set; }
        public string SenderCode { get; set; }
        public string SenderName { get; set; }
        public string MandateType { get; set; }
    }

    public class AssociatedEventsModel
    {
        public AssociatedEventsModel()
        {

        }


        public string EventCode { get; set; }
        public string Document { get; set; }
        public DateTime EventDate { get; set; }
        public string SenderCode { get; set; }
        public string Sender { get; set; }
        public string ReceiverCode { get; set; }
        public string Receiver { get; set; }
    }

    public class EndosoModel
    {
        public EndosoModel()
        {

        }

        public string ReceiverCode { get; set; }
        public string ReceiverName { get; set; }
        public string SenderCode { get; set; }
        public string SenderName { get; set; }
        public string EndosoType { get; set; }
    }
}
