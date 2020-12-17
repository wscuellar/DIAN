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
    using System.Threading.Tasks;

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

        #region GetPdfReport

        public async Task<byte[]> GetPdfReport(string cude)
        {
            // Load Templates            
            StringBuilder template = new StringBuilder(_fileManager.GetText("radian-documents-templates", "RepresentaciónGráfica.html"));

            // Load Document Data
            Domain.Entity.EventDataModel model = await GetEventDataModel(cude);

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

        private  async Task<Domain.Entity.EventDataModel> GetEventDataModel(string cude)
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
            model.ReceiverType = string.Empty;

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

            model.EntityName = referenceMeta.Serie;
            model.CertificateNumber = referenceMeta.SerieAndNumber;

            // Set title value data Particular Data
            // valida la regla de negocio para mostrar la sección de titulos valor

            if (model.EventStatus == EventStatus.AceptacionTacita
                || model.EventStatus == EventStatus.Received
                || model.EventStatus == EventStatus.Receipt
                || model.EventStatus == EventStatus.Accepted)
            {
                model.ShowTitleValueSection = false;
            }
            else
            {
                // GetDocuments

                List<GlobalDataDocument> globalDocsValueTitle = new List<GlobalDataDocument>();

                if (!string.IsNullOrEmpty(eventItem.DocumentKey))
                {
                    List<string> pks = null;
                    List<string> radianStatusFilter = new List<string>() {
                            $"0{(int)EventStatus.Received}", $"0{(int)EventStatus.Receipt}", $"0{(int)EventStatus.Accepted}"
                        };
                    pks = new List<string> { $"co|{eventVerification.EmissionDateNumber.Substring(6, 2)}|{eventItem.DocumentReferencedKey.Substring(0, 2)}" };


                    (bool hasMoreResults, string continuation, List<GlobalDataDocument> globalDataDocuments) cosmosResponse =
                    (false, null, new List<GlobalDataDocument>());

                    cosmosResponse = await _cosmosDBService.ReadDocumentsAsyncOrderByReception(null,
                                                                                                DateTime.Now,
                                                                                                DateTime.Now,
                                                                                                0,
                                                                                                null,
                                                                                                null,
                                                                                                null,
                                                                                                null,
                                                                                                null,
                                                                                                40,
                                                                                                eventItem.DocumentReferencedKey,
                                                                                                null,
                                                                                                pks
                                                                                                );

                   

                    foreach (GlobalDataDocument globalDocu in cosmosResponse.globalDataDocuments)
                    {
                        if (!globalDocu.Events.Any())
                        {
                            break;
                        }
                        bool correctStatus = true;
                        for (int i = 0; i < radianStatusFilter.Count; i++)
                        {
                            if (!radianStatusFilter.Contains(globalDocu.Events.OrderByDescending(e => e.Date).ElementAt(i).Code))
                            {
                                correctStatus = false;
                                break;
                            }
                        }

                        if (correctStatus)
                            globalDocsValueTitle.Add(globalDocu);
                    }

                    if (globalDocsValueTitle.Any())
                    {
                        model.ShowTitleValueSection = true;
                        model.ValueTitleDocuments = globalDocsValueTitle;
                    }
                }
            }
            return model;
        }

        #endregion

        #region DataTemplateMapping

        private StringBuilder DataTemplateMapping(StringBuilder template, DateTime expeditionDate, Domain.Entity.EventDataModel model)
        {
            string sectionHtml = "<div class='text-section padding-top20'> Sección {SectionNumber}</ div > ";

            #region Mapping Event Data Section
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
            #endregion

            #region Mapping reference invoice data section

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
            #endregion

            #region Mapping Sections data 

            // Mapping Sections data 
            // por el momento solo es posible mapear la sección 1(datos del adquiriente) y
            // la sección 2 datos del emisor.

            if (!string.IsNullOrEmpty(model.ReceiverName) && !string.IsNullOrEmpty(model.ReceiverCode))
            {
                StringBuilder templateSujeto = new StringBuilder(_fileManager.GetText("radian-documents-templates", "RepresentaciónGraficaSujeto.html"));

                StringBuilder subjects = new StringBuilder();


                // Section 1

                subjects.Append(sectionHtml);
                subjects.Append(templateSujeto);

                subjects = SubjectTemplateMapping(subjects, "1", "1",
                    model.ReceiverName
                    , model.ReceiverType
                    , model.ReceiverCode
                    , model.ReceiverCode
                    , string.Empty
                    , string.Empty
                    , string.Empty
                    , string.Empty);

                // Section 2

                subjects.Append(sectionHtml);
                subjects.Append(templateSujeto);

                subjects = SubjectTemplateMapping(subjects, "2", "1",
                    model.SenderName
                    , model.ReceiverType
                    , model.SenderCode
                    , model.SenderCode
                    , string.Empty
                    , string.Empty
                    , string.Empty
                    , string.Empty);

                template = template.Replace("{SectionsData}", subjects.ToString());
            }
            #endregion

            #region Mapping Title Value section

            // Mapping Title Value section

            if (model.ShowTitleValueSection)
            {
                StringBuilder templateTitleValue = new StringBuilder(_fileManager.GetText("radian-documents-templates", "RepresentaciónGraficaFacturaTituloValor.html"));

                for (int i = 0; i < model.ValueTitleDocuments[0].Events.Count; i++)
                {
                    Event eventDoc = model.ValueTitleDocuments[0].Events[i];
                    templateTitleValue = DocumentTemplateMapping(templateTitleValue, eventDoc, (i + 1).ToString());
                }
                template = template.Replace("{TitleValue}", templateTitleValue.ToString());
            }
            else
            {
                template = template.Replace("{TitleValue}", string.Empty);
            }

            #endregion


            // Mapping Final Data Section

            template = template.Replace("{FinalData}", $"Nombre de la Entidad de Certificación Digital: {model.EntityName}  Número del certificado digital: {model.CertificateNumber} ");

            return template;
        }

        #endregion

        #region SubjectTemplateMapping

        private StringBuilder SubjectTemplateMapping(StringBuilder template,
            string sectionNumber,
            string subjectNumber,
            string subjectBusinessName,
            string subjectType,
            string subjectDocumentType,
            string subjectNit,
            string subjectAddress,
            string subjectCity,
            string subjectEmail,
            string subjectPhoneNumber
            )
        {

            template = template.Replace("{SectionNumber}", sectionNumber);
            template = template.Replace("{SubjectNumber}", subjectNumber);
            template = template.Replace("{SubjectBusinessName}", subjectBusinessName);
            template = template.Replace("{SubjectType}", subjectType);
            template = template.Replace("{SubjectDocumentType}", subjectDocumentType);
            template = template.Replace("{SubjectNit}", subjectNit);
            template = template.Replace("{SubjectAddress}", subjectAddress);
            template = template.Replace("{SubjectCity}", subjectCity);
            template = template.Replace("{SubjectEmail}", subjectEmail);
            template = template.Replace("{SubjectPhoneNumber}", subjectPhoneNumber);

            return template;
        }

        #endregion

        #region DocumentTemplateMapping

        private StringBuilder DocumentTemplateMapping(StringBuilder template, Event eventDoc, string number)
        {
            template = template.Replace("{Number" + number + "}", number);
            template = template.Replace("{EventNumber" + number + "}", eventDoc.Code);
            template = template.Replace("{Description" + number + "}", eventDoc.Description);
            template = template.Replace("{GenerationDate" + number + "}", eventDoc.Date.ToShortDateString());
            template = template.Replace("{Registrationdate" + number + "}", eventDoc.Date.ToShortDateString());

            return template;
        }

        #endregion
    }
}
