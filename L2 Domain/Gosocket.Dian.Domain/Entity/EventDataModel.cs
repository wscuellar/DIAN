namespace Gosocket.Dian.Domain.Entity
{
    #region Using

    using Gosocket.Dian.Common.Resources;
    using Gosocket.Dian.Domain.Common;
    using Gosocket.Dian.Domain.Cosmos;
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


        public string Title { get; set; }
        public string CUDE { get; set; }
        public string CUFE { get; set; }
        public string Prefix { get; set; }
        public string Number { get; set; }
        public string DateOfIssue { get; set; }
        public string EmissionDate { get; set; }
        public string SenderCode { get; set; }
        public string ReceiverCode { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverType { get; set; }
        public string ReceiverEmail { get; set; }
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
        public string SenderPhoneNumber { get; set; }
        public string ReceiverPhoneNumber { get; set; }
        public string ReceiverDocumentType { get; set; }
        public string EventTotalAmount { get; set; }
        public string EventStartDate { get; set; }
        public string EndosoTotalAmount { get; set; }
        public string EventFinishDate { get; set; }
        public bool ShowTitleValueSection { get; set; }
        public string EntityName { get; set; }
        public string CertificateNumber { get; set; }
        public string ValidationMessage { get; set; }
        public string Note { get; set; }
        public string SignedBy { get; set; }
        public string EventCode { get; set; }

        public string EventTotalValueAval { get; set; }
        public string EventTotalValueEndoso { get; set; }
        public string EventTotalValueLimitation { get; set; }
        public string EventTotalValuePago { get; set; }
        public string InvoiceNumber { get; set; }
        public string CustomizationID { get; set; }
        public string ResponseCodeListID { get; set; }






        public EventStatus EventStatus { get; set; }
        public List<AssociatedValidationsModel> Validations { get; set; }

        public List<AssociatedReferenceModel> References
        {
            get; set;
        }

        public List<AssociatedEventsModel> AssociatedEvents { get; set; }
        public List<GlobalDataDocument> ValueTitleEvents { get; set; }
        public string EventTitle { get; set; }
        public string RequestType { get; set; }
        public string OperationDetails { get; set; }
        public string ValidationTitle { get; set; }
        public string DiscountRate { get; set; }
        public string TotalAmount { get; set; }
        public string ReferenceTitle { get; set; }
        public string SenderNit { get; set; }
        public string EventDescription { get; set; }
        public string SenderBusinessName { get; set; }
        public string SenderDocumentType { get; set; }
        public string CUDEReference { get; set; }
        public string EventCodeReference { get; set; }
        public string DescriptionReference { get; set; }
        public string SchemeID { get; set; }
        public string EventNumberReference { get; set; }
        public string ProviderIdNit { get; set; }
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
        public string SerieAndNumber { get; set; }
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
}
