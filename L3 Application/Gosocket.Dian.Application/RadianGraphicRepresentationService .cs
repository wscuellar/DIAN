namespace Gosocket.Dian.Application
{
    #region Using

    using Gosocket.Dian.Application.Cosmos;
    using Gosocket.Dian.Common.Resources;
    using Gosocket.Dian.Domain.Common;
    using Gosocket.Dian.Domain.Cosmos;
    using Gosocket.Dian.Infrastructure;
    using Gosocket.Dian.Interfaces.Services;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;

    #endregion

    public class RadianGraphicRepresentationService : IRadianGraphicRepresentationService
    {
        #region Properties

        private readonly IQueryAssociatedEventsService _queryAssociatedEventsService;
        private readonly FileManager _fileManager;
        private readonly CosmosDBService _cosmosDBService;

        #endregion

        #region Constructor

        public RadianGraphicRepresentationService(IQueryAssociatedEventsService queryAssociatedEventsService, FileManager fileManager, CosmosDBService cosmosDBService)
        {
            _queryAssociatedEventsService = queryAssociatedEventsService;
            _fileManager = fileManager;
            _cosmosDBService = cosmosDBService;
        }

        #endregion

        #region GetGraphicRepresentationPdfReport

        public byte[] GetPdfReport(string cude)
        {
            // Load Templates            
            StringBuilder template = new StringBuilder(_fileManager.GetText("radian-documents-templates", "RepresentaciónGráfica.html"));

            // Load Document Data
            Domain.Entity.EventDataModel model = GetEventDataModel(cude);

            // Set Variables
            DateTime expeditionDate = DateTime.Now;
            Bitmap qrCode = RadianPdfCreationService.GenerateQR(TextResources.RadianReportQRCode.Replace("{CUFE}", model.CUDE));

            string ImgDataURI = IronPdf.Util.ImageToDataUri(qrCode);
            string ImgHtml = String.Format("<img class='qr-content' src='{0}'>", ImgDataURI);


            // Mapping Labels common data

            template = DataTemplateMapping(template, expeditionDate, model);

            // Replace QrLabel
            template = template.Replace("{QrCode}", ImgHtml);

            // Mapping Events

            byte[] report = RadianPdfCreationService.GetPdfBytes(template.ToString());

            return report;
        }

        #endregion

        #region GetEventDataModel

        private Domain.Entity.EventDataModel GetEventDataModel(string cude)
        {
            GlobalDocValidatorDocumentMeta eventItem = _queryAssociatedEventsService.DocumentValidation(cude);

            Domain.Entity.EventDataModel model =
                new Domain.Entity.EventDataModel()
                {
                    Prefix = eventItem.Serie,
                    Number = eventItem.Number,
                    DateOfIssue = eventItem.SigningTimeStamp.Date,
                    SenderCode = eventItem.SenderCode,
                    SenderName = eventItem.SenderName,
                    ReceiverCode = eventItem.ReceiverCode,
                    ReceiverName = eventItem.ReceiverName
                };

            model.EventStatus = (EventStatus)Enum.Parse(typeof(EventStatus), eventItem.EventCode);

            model.CUDE = cude;


            // Set Titles
            model.Title = _queryAssociatedEventsService.EventTitle(model.EventStatus, eventItem.CustomizationID, eventItem.EventCode);
            model.ValidationTitle = TextResources.Event_ValidationTitle;
            model.ReferenceTitle = TextResources.Event_ReferenceTitle;

            GlobalDocValidatorDocumentMeta invoice = _queryAssociatedEventsService.DocumentValidation(eventItem.PartitionKey);

            // Set Mandate

            if (model.EventStatus == EventStatus.Mandato)
            {
                model.Mandate = new Domain.Entity.ElectronicMandateModel()
                {
                    ReceiverCode = eventItem.ReceiverCode,
                    ReceiverName = eventItem.ReceiverName,
                    SenderCode = invoice.SenderCode,
                    SenderName = invoice.SenderName,
                    MandateType = TextResources.Event_MandateType
                };

                model.ReceiverName = model.Mandate.ReceiverName;
                model.ReceiverCode = model.Mandate.ReceiverCode;
                List<GlobalDocReferenceAttorney> referenceAttorneys = _queryAssociatedEventsService.ReferenceAttorneys(eventItem.DocumentKey, eventItem.DocumentReferencedKey, eventItem.ReceiverCode, eventItem.SenderCode);

                if (referenceAttorneys.Any())
                    model.Mandate.ContractDate = referenceAttorneys.FirstOrDefault().EffectiveDate;
            }


            // Set Endoso

            if (model.EventStatus == Gosocket.Dian.Domain.Common.EventStatus.EndosoGarantia || model.EventStatus == Gosocket.Dian.Domain.Common.EventStatus.EndosoProcuracion)
            {
                model.Endoso = new Domain.Entity.EndosoModel()
                {
                    ReceiverCode = eventItem.ReceiverCode,
                    ReceiverName = eventItem.ReceiverName,
                    SenderCode = invoice.SenderCode,
                    SenderName = invoice.SenderName,
                    EndosoType = EnumHelper.GetEnumDescription((Enum.Parse(typeof(EventStatus), eventItem.EventCode)))
                };
                model.ReceiverName = model.Endoso.ReceiverName;
                model.ReceiverCode = model.Endoso.ReceiverCode;
            }

            model.RequestType = TextResources.Event_RequestType;

            Domain.Entity.GlobalDocValidatorDocument eventVerification = _queryAssociatedEventsService.EventVerification(eventItem.Identifier);

            // Set Validations

            if (eventVerification.ValidationStatus == 1)
            {
                model.ValidationMessage = TextResources.Event_ValidationMessage;
            }
            else if (eventVerification.ValidationStatus == 10)
            {
                List<Domain.Entity.GlobalDocValidatorTracking> res = _queryAssociatedEventsService.ListTracking(eventItem.DocumentKey);

                model.Validations = res.Select(t => new Domain.Entity.AssociatedValidationsModel(t)).ToList();
            }

            // SetReferences
            GlobalDocValidatorDocumentMeta referenceMeta = _queryAssociatedEventsService.DocumentValidation(eventItem.DocumentReferencedKey);
            if (referenceMeta != null)
            {
                string documentType = string.IsNullOrEmpty(referenceMeta.EventCode) ? TextResources.Event_DocumentType : Domain.Common.EnumHelper.GetEnumDescription((Enum.Parse(typeof(EventStatus), referenceMeta.EventCode)));
                documentType = string.IsNullOrEmpty(documentType) ? TextResources.Event_DocumentType : documentType;
                model.References.Add(new Domain.Entity.AssociatedReferenceModel()
                {
                    Document = documentType,
                    DateOfIssue = referenceMeta.EmissionDate.Date,
                    Description = string.Empty,
                    SenderCode = referenceMeta.SenderCode,
                    SenderName = referenceMeta.SenderName,
                    ReceiverCode = referenceMeta.ReceiverCode,
                    ReceiverName = referenceMeta.ReceiverName,
                    Number = referenceMeta.Number,
                    CUFE = referenceMeta.DocumentKey,
                    TotalAmount = referenceMeta.TotalAmount
                });
            }

            // SetEventAssociated 
            EventStatus allowEvent = _queryAssociatedEventsService.IdentifyEvent(eventItem);

            if (allowEvent != EventStatus.None)
            {
                model.EventTitle = "Eventos de " + EnumHelper.GetEnumDescription(model.EventStatus);
                List<GlobalDocValidatorDocumentMeta> otherEvents = _queryAssociatedEventsService.OtherEvents(eventItem.DocumentKey, allowEvent);
                if (otherEvents.Any())
                {
                    foreach (GlobalDocValidatorDocumentMeta otherEvent in otherEvents)
                    {
                        if (_queryAssociatedEventsService.IsVerificated(otherEvent))
                            model.AssociatedEvents.Add(new Domain.Entity.AssociatedEventsModel()
                            {
                                EventCode = otherEvent.EventCode,
                                Document = EnumHelper.GetEnumDescription(Enum.Parse(typeof(EventStatus), otherEvent.EventCode)),
                                EventDate = otherEvent.SigningTimeStamp,
                                SenderCode = otherEvent.ReceiverCode,
                                Sender = otherEvent.SenderName,
                                ReceiverCode = otherEvent.ReceiverCode,
                                Receiver = otherEvent.ReceiverName
                            });
                    }
                }
            }

            // Set Particular Data
            switch (model.EventStatus)
            {
                case EventStatus.AceptacionTacita:
                    model.ReceiverType = "Emisor FEV";
                    model.ShowTitleValueSection = false;
                    break;
                case EventStatus.Received:
                    model.ReceiverType = "Adquiriente FEV";
                    model.ShowTitleValueSection = false;
                    break;
                case EventStatus.Receipt:
                    model.ReceiverType = "Adquiriente FEV";
                    model.ShowTitleValueSection = false;
                    break;
                case EventStatus.Accepted:
                    model.ReceiverType = "Adquiriente FEV";
                    model.ShowTitleValueSection = false;
                    break;
                case EventStatus.Rejected:
                    model.ReceiverType = "Adquiriente FEV";
                    break;
                case EventStatus.NegotiatedInvoice:
                    model.ReceiverType = "Autoridad Competente";
                    break;
                case EventStatus.AnulacionLimitacionCirculacion:
                    model.ReceiverType = "Autoridad Competente";
                    break;
                case EventStatus.EndosoGarantia:
                    model.ReceiverType = "Endosante";
                    break;
                case EventStatus.EndosoProcuracion:
                    model.ReceiverType = "Endosante";
                    break;
                case EventStatus.EndosoPropiedad:
                    model.ReceiverType = "Endosante";
                    break;
                case EventStatus.InvoiceOfferedForNegotiation:
                    model.ReceiverType = "Endosante";
                    break;
                case EventStatus.Avales:
                    model.ReceiverType = "Avalado";
                    break;
                case EventStatus.Mandato:
                    model.ReceiverType = "Mandante";
                    break;
                case EventStatus.TerminacionMandato:
                    model.ReceiverType = "Mandatario";
                    break;
                case EventStatus.NotificacionPagoTotalParcial:
                    model.ReceiverType = "Tenedor Legítimo";
                    break;
                default:
                    model.ReceiverType = "Emisor FEV";
                    break;
            }

            return model;
        }

        #endregion

        #region DataTemplateMapping

        private StringBuilder DataTemplateMapping(StringBuilder template, DateTime expeditionDate, Domain.Entity.EventDataModel model)
        {
            string sectionHtml = "<div class='text - section padding - top20'> Sección {SectionNumber}</ div > ";

            // Mapping Event Data Section
            template = template.Replace("{EventName}", model.Title);
            template = template.Replace("{EventNumber}", $"{model.Prefix} - {model.Number}");
            template = template.Replace("{EventType}", model.RequestType);
            template = template.Replace("{OperationType}", model.RequestType);
            template = template.Replace("{OperationDetails}", string.Empty);
            template = template.Replace("{DiscountRate}", string.Empty);
            template = template.Replace("{TotalEventValue}", string.Empty);
            template = template.Replace("{CUDE}", model.CUDE);
            template = template.Replace("{ExpeditionDate}", model.DateOfIssue.ToShortDateString());
            template = template.Replace("{RegistrationDate}", string.Empty);
            template = template.Replace("{StartDate}", string.Empty);
            template = template.Replace("{FinishDate}", string.Empty);

            // Mapping reference invoice data section

            template = template.Replace("{InvoiceNumber}", model.References[0].Number);
            template = template.Replace("{TotalValue}", $"{model.References[0].TotalAmount}");
            template = template.Replace("{PaymentWay}", string.Empty);
            template = template.Replace("{PaymentMethod}", string.Empty);
            template = template.Replace("{PaymentState}", string.Empty);
            template = template.Replace("{PaymentConditions}", string.Empty);
            template = template.Replace("{CUFE}", model.References[0].CUFE);
            template = template.Replace("{IssueDate}", model.References[0].DateOfIssue.ToShortDateString());
            template = template.Replace("{ExpirationDate}", string.Empty);
            template = template.Replace("{OperationType}", string.Empty);

            // Mapping reference event data section

            template = template.Replace("{ReferenceEventData}", string.Empty);

            // Mapping Sections data 

            if (!string.IsNullOrEmpty(model.ReceiverName) && !string.IsNullOrEmpty(model.ReceiverCode))
            {
                StringBuilder templateSujeto = new StringBuilder(_fileManager.GetText("radian-documents-templates", "RepresentaciónGraficaSujeto.html"));

                StringBuilder subjects = new StringBuilder();

                for (int i = 1; i <= 3; i++)
                {
                    subjects.Append(sectionHtml);
                    subjects.Append(templateSujeto);
                    subjects = subjects.Replace("{SectionNumber}", i.ToString());
                    subjects = subjects.Replace("{SubjectNumber}", i.ToString());
                    subjects = subjects.Replace("{SubjectBusinessName}", model.ReceiverName);
                    subjects = subjects.Replace("{SubjectType}", model.ReceiverType);
                    subjects = subjects.Replace("{SubjectDocumentType}", model.ReceiverCode);
                    subjects = subjects.Replace("{SubjectNit}", model.ReceiverCode);
                    subjects = subjects.Replace("{SubjectAddress}", string.Empty);
                    subjects = subjects.Replace("{SubjectCity}", string.Empty);
                    subjects = subjects.Replace("{SubjectEmail1}", string.Empty);
                    subjects = subjects.Replace("{SubjectPhoneNumber}", string.Empty);
                }

                template = template.Replace("{SectionsData}", subjects.ToString());
            }


            // Mapping Title Value section

            template = template.Replace("{TitleValue}", string.Empty);

            // Mapping Final Data Section

            template = template.Replace("{FinalData}", "Numero de Autrozación :20585563 Rango Autorizado: Desde 984567356346 Rango Autorizado: Hasta 2356745740585563 Vigencia 2030-01-04 ");

            return template;
        }

        #endregion

        #region SubjectTemplateMapping

        private StringBuilder SubjectTemplateMapping(StringBuilder template, Event eventObj, string subEvent)
        {
            template = template.Replace("{EventNumber" + subEvent + "}", eventObj.Code);
            template = template.Replace("{DocumentTypeName" + subEvent + "}", eventObj.Description);
            template = template.Replace("{CUDE" + subEvent + "}", eventObj.DocumentKey);
            template = template.Replace("{ValidationDate}", $"{eventObj.Date:yyyy'-'MM'-'dd hh:mm:ss.000} UTC-5");
            template = template.Replace("{SenderBusinessName" + subEvent + "}", eventObj.SenderName);
            template = template.Replace("{ReceiverBusinessName" + subEvent + "}", eventObj.ReceiverName);

            return template;
        }

        #endregion
    }
}
