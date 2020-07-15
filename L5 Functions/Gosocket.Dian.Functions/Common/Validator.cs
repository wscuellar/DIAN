using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Cosmos;
using Gosocket.Dian.Services.Utils;
using System;
using System.Linq;

namespace Gosocket.Dian.Functions.Common
{
    public class Validator
    {
        public static Tuple<bool, EventResponse> ValidateReceptionDate(GlobalDataDocument globalDataDocument)
        {
            DateTime authorizedDate = globalDataDocument.ReceptionTimeStamp.AddDays(30);

            if (authorizedDate < DateTime.UtcNow)
            {
                var response = new EventResponse { Code = ((int)EventValidationMessage.OutOffDate).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.OutOffDate) };
                return Tuple.Create(false, response);
            }

            return Tuple.Create(true, new EventResponse { });
        }

        public static Tuple<bool, EventResponse> ValidateEvent(GlobalDataDocument globalDataDocument, string responseCode)
        {
            if (globalDataDocument.Events.Count > 0)
            {
                //var acceptedCodes = new string[] { ((int)EventStatus.Accepted).ToString(), ((int)EventStatus.Accepted).ToString().PadLeft(3, '0') };
                //var rejectedCodes = new string[] { ((int)EventStatus.Rejected).ToString(), ((int)EventStatus.Rejected).ToString().PadLeft(3, '0') };
                //var receiptCodes = new string[] { ((int)EventStatus.Receipt).ToString(), ((int)EventStatus.Receipt).ToString().PadLeft(3, '0') };
                //var receivedCodes = new string[] { ((int)EventStatus.Received).ToString(), ((int)EventStatus.Received).ToString().PadLeft(3, '0') };
                switch (int.Parse(responseCode))
                {
                    case (int)EventStatus.Accepted:
                        if (globalDataDocument.Events.Any(e => e.Code == ((int)EventStatus.Accepted).ToString().PadLeft(3, '0')))
                        {
                            var response = new EventResponse { Code = ((int)EventValidationMessage.PreviouslyRegistered).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.PreviouslyRegistered) };
                            return Tuple.Create(false, response);
                        }
                        if (globalDataDocument.Events.Any(e => e.Code == ((int)EventStatus.Rejected).ToString().PadLeft(3, '0')))
                        {
                            var response = new EventResponse { Code = ((int)EventValidationMessage.PreviouslyRejected).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.PreviouslyRejected) };
                            return Tuple.Create(false, response);
                        }
                        break;
                    case (int)EventStatus.Rejected:
                        if (globalDataDocument.Events.Any(e => e.Code == ((int)EventStatus.Rejected).ToString().PadLeft(3, '0')))
                        {
                            var response = new EventResponse { Code = ((int)EventValidationMessage.PreviouslyRegistered).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.PreviouslyRegistered) };
                            return Tuple.Create(false, response);
                        }
                        if (globalDataDocument.Events.Any(e => e.Code == ((int)EventStatus.Accepted).ToString().PadLeft(3, '0')))
                        {
                            var response = new EventResponse { Code = ((int)EventValidationMessage.PreviouslyAccepted).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.PreviouslyAccepted) };
                            return Tuple.Create(false, response);
                        }
                        break;
                    case (int)EventStatus.Receipt:
                        if (globalDataDocument.Events.Any(e => e.Code == ((int)EventStatus.Receipt).ToString().PadLeft(3, '0')))
                        {
                            var response = new EventResponse { Code = ((int)EventValidationMessage.PreviouslyRegistered).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.PreviouslyRegistered) };
                            return Tuple.Create(false, response);
                        }
                        if (globalDataDocument.Events.Any(e => e.Code == ((int)EventStatus.Rejected).ToString().PadLeft(3, '0')))
                        {
                            var response = new EventResponse { Code = ((int)EventValidationMessage.ReceiptPreviouslyRejected).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.ReceiptPreviouslyRejected) };
                            return Tuple.Create(false, response);
                        }
                        break;
                    case (int)EventStatus.Received:
                        if (globalDataDocument.Events.Any(e => e.Code == ((int)EventStatus.Received).ToString().PadLeft(3, '0')))
                        {
                            var response = new EventResponse { Code = ((int)EventValidationMessage.PreviouslyRegistered).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.PreviouslyRegistered) };
                            return Tuple.Create(false, response);
                        }
                        break;
                    case (int)EventStatus.InvoiceOfferedForNegotiation:
                        if (globalDataDocument.Events.Any(e => e.Code == ((int)EventStatus.Rejected).ToString().PadLeft(3, '0')))
                        {
                            var response = new EventResponse { Code = ((int)EventValidationMessage.InvoiceOfferedForNegotiationPreviouslyRejected).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.InvoiceOfferedForNegotiationPreviouslyRejected) };
                            return Tuple.Create(false, response);
                        }

                        //Validar fecha... por definir
                        break;
                    case (int)EventStatus.NegotiatedInvoice:
                        if (globalDataDocument.Events.Any(e => e.Code == ((int)EventStatus.Rejected).ToString().PadLeft(3, '0')))
                        {
                            var response = new EventResponse { Code = ((int)EventValidationMessage.NegotiatedInvoicePreviouslyRejected).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.NegotiatedInvoicePreviouslyRejected) };
                            return Tuple.Create(false, response);
                        }

                        //Validar fecha... por definir
                        break;
                    default:
                        break;
                }
            }

            return Tuple.Create(true, new EventResponse { });
        }
    }
}
