﻿using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Gosocket.Dian.Application
{
    #region Using

    using Gosocket.Dian.Application.Cosmos;
    using Gosocket.Dian.Common.Resources;
    using Gosocket.Dian.Domain.Common;
    using Gosocket.Dian.Domain.Cosmos;
    using Gosocket.Dian.Domain.Domain;
    using Gosocket.Dian.Infrastructure;
    using Gosocket.Dian.Interfaces.Services;
    using Gosocket.Dian.Services.Utils.Helpers;
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

        private readonly TableManager globalDocValidatorDocumentTableManager = new TableManager("GlobalDocValidatorDocument");
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
            StringBuilder template = new StringBuilder(_fileManager.GetText("radian-documents-templates", "RepresentacionGraficaNew1.html"));
            
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

            byte[] report = RadianPdfCreationService.GetPdfBytes(template.ToString(), "Representacion grafica");

            return report;
        }

        #endregion


        #region GetEventDataModel

        private async Task<Domain.Entity.EventDataModel> GetEventDataModel(string cude)
        {
            GlobalDocValidatorDocumentMeta eventItem = _queryAssociatedEventsService.DocumentValidation(cude);
            byte[] xmlBytes = RadianSupportDocument.GetXmlFromStorageAsync(cude);
            //var str = Encoding.Default.GetString(xmlBytes);

            Domain.Entity.EventDataModel model =
                new Domain.Entity.EventDataModel()
                {
                    Prefix = eventItem.Serie,
                    Number = eventItem.Number,
                    DateOfIssue = eventItem.SigningTimeStamp.ToString(),
                    EmissionDate = eventItem.EmissionDate.ToString(),
                    SenderCode = eventItem.SenderCode,
                    SenderName = eventItem.SenderName,
                    ReceiverCode = eventItem.ReceiverCode,
                    ReceiverName = eventItem.ReceiverName
                };

            model.EventStatus = (EventStatus)Enum.Parse(typeof(EventStatus), eventItem.EventCode);
            model.CUDE = cude;

            Dictionary<string, string> xpathRequest = new Dictionary<string, string>();
            xpathRequest = CreateGetXpathData(Convert.ToBase64String(xmlBytes), "RepresentacionGrafica");

            ResponseXpathDataValue fieldValues = ApiHelpers.ExecuteRequest<ResponseXpathDataValue>(ConfigurationManager.GetValue("GetXpathDataValuesUrl"), xpathRequest);
            
            model = MappingXpathValues(model, fieldValues);

            // Set Titles
            model.Title = _queryAssociatedEventsService.EventTitle(model.EventStatus, eventItem.CustomizationID, eventItem.EventCode);

            switch (model.EventStatus)
            {
                case EventStatus.Received:
                    model.EventTitle = EnumHelper.GetDescription(EventStatus.Received);
                    break;
                case EventStatus.Receipt:
                    model.EventTitle = EnumHelper.GetDescription(EventStatus.Receipt);
                    break;
                case EventStatus.AceptacionTacita:
                    model.EventTitle = EnumHelper.GetDescription(EventStatus.AceptacionTacita);
                    break;
                case EventStatus.Accepted:
                    model.EventTitle = EnumHelper.GetDescription(EventStatus.Accepted);
                    break;
                case EventStatus.SolicitudDisponibilizacion:
                    if (eventItem.CustomizationID.Equals("361") || eventItem.CustomizationID.Equals("362"))
                        model.EventTitle = "Registro Primera circulación de la FEV TV";
                    else
                        model.EventTitle = "Registro Circulación posterior  de la FEV TV";
                    break;
                case EventStatus.EndosoGarantia:
                    model.Title = model.EventStatus.GetDescription();
                    model.EventTitle = EnumHelper.GetDescription(EventStatus.EndosoGarantia);
                    model.RequestType = eventItem.EventCode;
                    break;
                case EventStatus.EndosoPropiedad:
                    model.Title = model.EventStatus.GetDescription();
                    model.EventTitle = EnumHelper.GetDescription(EventStatus.EndosoPropiedad);
                    model.RequestType = eventItem.EventCode;
                    break;
                case EventStatus.EndosoProcuracion:
                    model.Title = model.EventStatus.GetDescription();
                    model.EventTitle = EnumHelper.GetDescription(EventStatus.EndosoProcuracion);
                    model.RequestType = eventItem.EventCode;
                    break;
                case EventStatus.InvoiceOfferedForNegotiation:
                    model.Title = model.EventStatus.GetDescription();
                    model.EventTitle = EnumHelper.GetDescription(EventStatus.InvoiceOfferedForNegotiation);
                    model.RequestType = eventItem.EventCode;
                    break;
                case EventStatus.Avales:
                    model.EventTitle = EnumHelper.GetDescription(EventStatus.Avales);
                    break;
                case EventStatus.Mandato:
                    model.EventTitle = EnumHelper.GetDescription(EventStatus.Mandato);
                    break;
                case EventStatus.TerminacionMandato:
                    model.EventTitle = EnumHelper.GetDescription(EventStatus.TerminacionMandato);
                    break;
                case EventStatus.ValInfoPago:
                    model.EventTitle = EnumHelper.GetDescription(EventStatus.ValInfoPago);
                    break;
                case EventStatus.NotificacionPagoTotalParcial:
                    model.EventTitle = EnumHelper.GetDescription(EventStatus.NotificacionPagoTotalParcial);
                    break;
                case EventStatus.AnulacionLimitacionCirculacion:
                    model.EventTitle = EnumHelper.GetDescription(EventStatus.AnulacionLimitacionCirculacion);
                    break;
                case EventStatus.NegotiatedInvoice:
                    model.EventTitle = EnumHelper.GetDescription(EventStatus.NegotiatedInvoice);
                    break;
                default:
                    model.Title = _queryAssociatedEventsService.EventTitle(model.EventStatus, eventItem.CustomizationID, eventItem.EventCode);
                    model.ReceiverType = string.Empty;
                    //model.RequestType = model.Title;
                    break;
            }

            // SetReferences
            GlobalDocValidatorDocumentMeta referenceMeta = _queryAssociatedEventsService.DocumentValidation(eventItem.DocumentReferencedKey);
            if (referenceMeta != null)
            {
                string documentType = string.IsNullOrEmpty(referenceMeta.EventCode) ? TextResources.Event_DocumentType : EnumHelper.GetEnumDescription((Enum.Parse(typeof(EventStatus), referenceMeta.EventCode)));
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

            model.EntityName = referenceMeta.Serie;
            model.CertificateNumber = referenceMeta.SerieAndNumber;

            Domain.Entity.GlobalDocValidatorDocument eventVerification =
                    globalDocValidatorDocumentTableManager.Find<Domain.Entity.GlobalDocValidatorDocument>(referenceMeta?.Identifier, referenceMeta?.Identifier);

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
                    pks = new List<string> { $"co|{eventVerification.EmissionDateNumber.Substring(6, 2)}|{eventItem.DocumentReferencedKey.Substring(0, 2)}" };
                    (bool hasMoreResults, string continuation, List<GlobalDataDocument> globalDataDocuments) cosmosResponse =
                    (false, null, new List<GlobalDataDocument>());

                    cosmosResponse =
                        await _cosmosDBService
                        .ReadDocumentsAsyncOrderByReception(
                            null,
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
                        if (globalDocu.Events.Any() && globalDocu.Events.Any(a => a.Code.Equals($"0{(int)EventStatus.Accepted}")))
                        {
                            List<Event> eventosTituloValor = new List<Event>();
                            eventosTituloValor.Add(globalDocu.Events.FirstOrDefault(a => a.Code.Equals($"0{(int)EventStatus.Received}")));
                            eventosTituloValor.Add(globalDocu.Events.FirstOrDefault(a => a.Code.Equals($"0{(int)EventStatus.Receipt}")));
                            eventosTituloValor.Add(globalDocu.Events.FirstOrDefault(a => a.Code.Equals($"0{(int)EventStatus.Accepted}")));

                            globalDocu.Events = eventosTituloValor;

                            globalDocsValueTitle.Add(globalDocu);
                            model.ShowTitleValueSection = true;
                            model.ValueTitleEvents = globalDocsValueTitle;
                        }
                    }
                }
            }
            return model;
        }

        #endregion

        #region DataTemplateMapping

        private StringBuilder DataTemplateMapping(StringBuilder template, DateTime expeditionDate, Domain.Entity.EventDataModel model)
        {
            //string sectionHtml = "<div class='text-section padding-top20'> Sección {SectionNumber}</ div > ";
            bool isInEvent = false;

            if (model.EventCode == "035" || model.EventCode == "037" || model.EventCode == "038" || model.EventCode == "039" || model.EventCode == "041" || model.EventCode == "042" || model.EventCode == "045" || model.EventCode == "046")
            {
                isInEvent = true;
            }
            #region datos del evento

            string htmlEvent = "";
            htmlEvent += "<td>";
            htmlEvent += "<div id='EventNumber' class='text-subtitle text-gray'> Número del Evento: <a class='text-data'>{EventNumber}</a></div> ";

            if (model.EventCode == "037" || model.EventCode == "038" || model.EventCode == "039" || model.EventCode == "043" || model.EventCode == "045")
            {
                htmlEvent += "<div id='OperationDetails' class='text-subtitle text-gray'> Detalles del evento: <a class='text-data'>{OperationDetails}</a></div>";
            }
            if (isInEvent)
            {
                htmlEvent += "<div class='text-subtitle text-gray'> Valor total del evento: <a class='text-data'>";
                switch (model.EventCode)
                {
                    case "035":
                        htmlEvent += "{EventTotalValueAval}";
                        break;
                    case "037":
                        htmlEvent += "{EventTotalValueEndoso}";
                        break;
                    case "038":
                        htmlEvent += "{EventTotalValueEndoso}";
                        break;
                    case "039":
                        htmlEvent += "{EventTotalValueEndoso}";
                        break;
                    case "041":
                        htmlEvent += "{EventTotalValueLimitation}";
                        break;
                    case "042":
                        htmlEvent += "{EventTotalValueLimitation}";
                        break;
                    case "045":
                        htmlEvent += "{EventTotalValuePago}";
                        break;
                    case "046":
                        htmlEvent += "{EventTotalValuePago}";
                        break;
                }
                htmlEvent += "</a></div>";
            }

            htmlEvent += "</td>";
            htmlEvent += "<td>";
            htmlEvent += "<div id='EmissionDate' class='text-subtitle text-gray'>Fecha y Hora de Generación: <a class='text-data'>{EmissionDate}</a></div>";

            if (model.EventCode == "035" || model.EventCode == "043" || model.EventCode == "041")
            {
                htmlEvent += "<div id='EventStartDate' class='text-subtitle text-gray'>Fecha de Inicio: <a class='text-data'>{EventStartDate} </a></div>";
                htmlEvent += "<div id='EventFinishDate' class='text-subtitle text-gray'>Fecha de Terminación: <a class='text-data'>{EventFinishDate} </a></div>";
            }
           
            if (model.EventCode == "038" || model.EventCode == "039")
            {
                htmlEvent += "<div id='EventStartDate' class='text-subtitle text-gray'>Fecha de Inicio: <a class='text-data'>{EventStartDate} </a></div>";
            }
            htmlEvent += "</td>";
            template.Replace("{eventdata}", htmlEvent);

            #endregion

            #region datos de la factura

            string htmlInvoice = "";
            htmlInvoice += "<td>";
            htmlInvoice += "<div id='InvoiceNumber' class='text-subtitle text-gray'> Número de Factura: <a class='text-data'>{InvoiceNumber}</a></div> ";
            htmlInvoice += "<div id='CUFE' class='text-subtitle text-gray'> CUFE: <a class='text-data cufe'>{CUFE}</a></div>";
            htmlInvoice += "</td>";
            htmlInvoice += "<td>";

            //htmlInvoice += "<div id='TotalValue' class='text-subtitle text-gray'>Valor Total de la Factura: <a class='text-data'>{TotalValue}</a></div>";
            
            if (model.EventCode == "036" || model.EventCode == "037" || model.EventCode == "038" || model.EventCode == "045")
            {
                htmlInvoice += "<div id='ExpirationDate' class='text-subtitle text-gray'>Fecha de Vencimiento: <a class='text-data'>{ExpirationDate}</a></div>";
            }
            htmlInvoice += "</td>";
            template.Replace("{invoiceReference}", htmlInvoice);

            #endregion

            #region referencia del evento

            if (model.EventCode == "040" || model.EventCode == "042" || model.EventCode == "044") {
                string htmlReference = "";
                htmlReference += "<td>";
                htmlReference += "<div id='InvoiceNumber' class='text-subtitle text-gray'> Número del Evento: <a class='text-data'>{EventCode}</a></div>";
                htmlReference += "<div id='CUDE' class='text-subtitle text-gray'>CUDE: <a class='text-data cude'>{CUDE}</a></div>";
                htmlReference += "</td>";
                htmlReference += "<td>";
                htmlReference += "<div id='OperationDetails' class='text-subtitle text-gray'>Detalle del Evento: <a class='text-data'>{OperationDetails}</a></div>";
                htmlReference += "</td>";
                template.Replace("{eventReference}", htmlReference);
            }
            else
            {
                template = template.Replace("{eventReference}", "");
                template = template.Replace("{classEvents}", "noShow");

            }


            #endregion

            #region Mapping Event Data Section

            // Mapping Event Data Section
            template = template.Replace("{EventName}", model.Title);
            template = template.Replace("{EventNumber}", $"{model.Number}");
            template = template.Replace("{EventType}", model.EventTitle);
            template = template.Replace("{OperationType}", model.RequestType);
            template = template.Replace("{OperationDetails}", model.OperationDetails);
            template = template.Replace("{DiscountRate}", model.DiscountRate);
            template = template.Replace("{TotalEventAmount}", model.EndosoTotalAmount);
            template = template.Replace("{CUDE}", model.CUDE);
            template = template.Replace("{EmissionDate}", $"{model.EmissionDate:dd'/'MM'/'yyyy hh:mm:ss tt}");
            template = template.Replace("{RegistrationDate}", model.DateOfIssue);
            template = template.Replace("{EventStartDate}", model.EventStartDate);
            template = template.Replace("{EventFinishDate}", model.EventFinishDate);
            template = template.Replace("{Notes}", model.Note);
            template = template.Replace("{SignedBy}", model.SignedBy);
            template = template.Replace("{EventTotalValueAval}", model.EventTotalValueAval);
            template = template.Replace("{EventTotalValueEndoso}", model.EventTotalValueEndoso);
            template = template.Replace("{EventTotalValueLimitation}", model.EventTotalValueLimitation);
            template = template.Replace("{EventTotalValuePago}", model.EventTotalValuePago);

            if (!(model.EventCode == "036" || model.EventCode == "037" || model.EventCode == "038" || model.EventCode == "039" || model.EventCode == "040"))
            {
                template = template.Replace("{classNotes}", "noShow");
            }
           
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
            template = template.Replace("{IssueDate}", $"{model.References[0].DateOfIssue:dd'/'MM'/'yyyy}");
            template = template.Replace("{ExpirationDate}", model.EventFinishDate);
            template = template.Replace("{InvoiceOperationType}", string.Empty);

            // Mapping reference event data section

            template = template.Replace("{ReferenceEventData}", string.Empty);
            #endregion

            #region Mapping Sections data 

            // Mapping Sections data 
            // por el momento solo es posible mapear la sección 1(datos del adquiriente) y
            // la sección 2 datos del emisor.

            if (!string.IsNullOrEmpty(model.ReceiverName) && !string.IsNullOrEmpty(model.ReceiverCode))
            {
                StringBuilder templateSujeto = new StringBuilder(_fileManager.GetText("radian-documents-templates", "RepresentaciónGraficaSujetoNew.html"));

                StringBuilder subjects = new StringBuilder();


                // Section 1

                //subjects.Append(sectionHtml);
                subjects.Append(templateSujeto);

                subjects = SubjectTemplateMapping(subjects, "1", "Datos del emisor",
                    model.ReceiverName
                    , model.ReceiverType
                    , model.ReceiverDocumentType
                    , model.ReceiverCode
                    , string.Empty
                    , string.Empty
                    , model.ReceiverEmail
                    , model.ReceiverPhoneNumber
                    , "emisor");

                // Section 2

                //subjects.Append(sectionHtml);
                subjects.Append(templateSujeto);

                subjects = SubjectTemplateMapping(subjects, "2", "Datos del receptor",
                    model.SenderName
                    , model.ReceiverType
                    , model.SenderDocumentType
                    , model.SenderCode
                    , string.Empty
                    , string.Empty
                    , model.SenderEmail
                    , model.SenderPhoneNumber
                    , "receptor");

                template = template.Replace("{SectionsData}", subjects.ToString());
            }
            #endregion

            #region Mapping Title Value section

            // Mapping Title Value section

            if (model.ShowTitleValueSection)
            {
                StringBuilder templateTitleValue = new StringBuilder(_fileManager.GetText("radian-documents-templates", "RepresentaciónGraficaFacturaTituloValor.html"));

                for (int i = 0; i < model.ValueTitleEvents[0].Events.Count; i++)
                {
                    Event eventDoc = model.ValueTitleEvents[0].Events[i];
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
            string subjectPhoneNumber,
            string sujeto
            )
        {
            template = template.Replace("{sujeto}", sujeto);
            template = template.Replace("{SectionNumber}", sectionNumber);
            template = template.Replace("{titleNameParticipant}", subjectNumber);
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

        #region Xpaths

        #region CreateGetXpathData

        private static Dictionary<string, string> CreateGetXpathData(string xmlBase64, string fileName = null)
        {
            var requestObj = new Dictionary<string, string>
            {
                { "XmlBase64", xmlBase64},
                { "FileName", fileName},
                { "ReceiverEmail", "//*[local-name()='ApplicationResponse']/*[local-name()='ReceiverParty']/*[local-name()='Contact']/*[local-name()='ElectronicMail']" },
                { "SenderEmail", "//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='Contact']/*[local-name()='ElectronicMail']" },
                { "SenderPhoneNumber", "//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='Contact']/*[local-name()='Telephone']" },
                { "ReceiverPhoneNumber", "//*[local-name()='ApplicationResponse']/*[local-name()='ReceiverParty']/*[local-name()='Contact']/*[local-name()='Telephone']" },
                { "ReceiverDocumentType", "//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='DocumentReference']/*[local-name()='IssuerParty']/*[local-name()='PartyTaxScheme']/*[local-name()='CompanyID']/@schemeName"},
                { "EventTotalAmount", "//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='IssuerParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CorporateStockAmount']" },
                { "EventStartDate", "//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='DocumentReference']/*[local-name()='ValidityPeriod']/*[local-name()='StartDate']" },
                { "EventFinishDate","//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='DocumentReference']/*[local-name()='ValidityPeriod']/*[local-name()='EndDate']" },
                { "RequestType", "//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='Response']/*[local-name()='ResponseCode']" },
                { "OperationDetails", "//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='Response']/*[local-name()='ResponseCode']" },
                { "DiscountRate", "//*[local-name()='ApplicationResponse']/*[local-name()='UBLExtensions']/*[local-name()='UBLExtension']/*[local-name()='ExtensionContent']/*[local-name()='CustomTagGeneral']/*[local-name()='InformacionNegociacion']/*[local-name()='Value'][3]" },
                { "EndosoTotalAmount", "//*[local-name()='ApplicationResponse']/*[local-name()='UBLExtensions']/*[local-name()='UBLExtension']/*[local-name()='ExtensionContent']/*[local-name()='CustomTagGeneral']/*[local-name()='InformacionNegociacion']/*[local-name()='Value'][1]" },
                { "GenerationDate", "//*[local-name()='ApplicationResponse']/*[local-name()='IssueDate']" },
                { "GenerationTime", "//*[local-name()='ApplicationResponse']/*[local-name()='IssueTime']" },
                { "SenderNit", "//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyTaxScheme'][1]/*[local-name()='CompanyID']" },
                { "EventDescription","//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='Response']/*[local-name()='Description']" },
                { "SenderBusinessName","//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyTaxScheme'][1]/*[local-name()='RegistrationName']" },
                { "SenderDocumentType","//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyTaxScheme'][1]/*[local-name()='CompanyID']/@schemeName" },
                { "Note","//*[local-name()='ApplicationResponse']/*[local-name()='Note']" },
                { "SignedBy","//*[local-name()='ApplicationResponse']/*[local-name()='UBLExtensions']/*[local-name()='UBLExtension']/*[local-name()='ExtensionContent']/*[local-name()='Signature']/*[local-name()='Object']/*[local-name()='QualifyingProperties']/*[local-name()='SignedProperties']/*[local-name()='SignedSignatureProperties']/*[local-name()='SignerRole']/*[local-name()='ClaimedRoles']/*[local-name()='ClaimedRole']" },
                { "EventCode","//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='Response']/*[local-name()='ResponseCode']" },
                { "EventTotalValueAval","//*[local-name()='ApplicationResponse']/*[local-name()='UBLExtensions']/*[local-name()='UBLExtension']/*[local-name()='CustomTagGeneral']/*[local-name()='InformacionAvalar']/*[local-name()='Value']" },
                { "EventTotalValueEndoso","//*[local-name()='ApplicationResponse']/*[local-name()='UBLExtensions']/*[local-name()='UBLExtension']/*[local-name()='CustomTagGeneral']/*[local-name()='InformacionNegociacion']/*[local-name()='Value']" },
                { "EventTotalValueLimitation","//*[local-name()='ApplicationResponse']/*[local-name()='UBLExtensions']/*[local-name()='UBLExtension']/*[local-name()='CustomTagGeneral']/*[local-name()='InformacionMedidaCautelar']/*[local-name()='Value']" },
                { "EventTotalValuePago","//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CorporateStockAmount']" }
            };
            return requestObj;
        }

        #endregion

        #region MappingXpathValues

        private Domain.Entity.EventDataModel MappingXpathValues(Domain.Entity.EventDataModel model, ResponseXpathDataValue dataValues)
        {
            model.ReceiverEmail = dataValues.XpathsValues["ReceiverEmail"] != null ?
                    dataValues.XpathsValues["ReceiverEmail"] : string.Empty;
            model.SenderEmail = dataValues.XpathsValues["ReceiverEmail"] != null ?
                    dataValues.XpathsValues["ReceiverEmail"] : string.Empty;
            model.SenderPhoneNumber = dataValues.XpathsValues["SenderPhoneNumber"] != null ?
                    dataValues.XpathsValues["SenderPhoneNumber"] : string.Empty;
            model.ReceiverPhoneNumber = dataValues.XpathsValues["ReceiverPhoneNumber"] != null ?
                    dataValues.XpathsValues["ReceiverPhoneNumber"] : string.Empty;
            model.ReceiverDocumentType = dataValues.XpathsValues["ReceiverDocumentType"] != null ?
                    dataValues.XpathsValues["ReceiverDocumentType"] : string.Empty;
            model.EventTotalAmount = dataValues.XpathsValues["EventTotalAmount"] != null ?
                    dataValues.XpathsValues["EventTotalAmount"] : string.Empty;

            model.EventStartDate = dataValues.XpathsValues["EventStartDate"] != null ?
                    dataValues.XpathsValues["EventStartDate"] : string.Empty;

            model.EventFinishDate = dataValues.XpathsValues["EventFinishDate"] != null ?
                    dataValues.XpathsValues["EventFinishDate"] : string.Empty;

            model.RequestType = dataValues.XpathsValues["RequestType"] != null ?
                    dataValues.XpathsValues["RequestType"] : string.Empty;
            model.OperationDetails = dataValues.XpathsValues["OperationDetails"] != null ?
                    dataValues.XpathsValues["OperationDetails"] : string.Empty;
            model.DiscountRate = dataValues.XpathsValues["DiscountRate"] != null ?
                    dataValues.XpathsValues["DiscountRate"] : string.Empty;
            model.TotalAmount = dataValues.XpathsValues["EventTotalAmount"] != null ?
                    dataValues.XpathsValues["EventTotalAmount"] : string.Empty;
            model.EndosoTotalAmount = dataValues.XpathsValues["EndosoTotalAmount"] != null ?
                    dataValues.XpathsValues["EndosoTotalAmount"] : string.Empty;
            model.SenderNit = dataValues.XpathsValues["SenderNit"] != null ?
                    dataValues.XpathsValues["SenderNit"] : string.Empty;
            model.EventDescription = dataValues.XpathsValues["EventDescription"] != null ?
                    dataValues.XpathsValues["EventDescription"] : string.Empty;
            model.SenderBusinessName = dataValues.XpathsValues["SenderBusinessName"] != null ?
                    dataValues.XpathsValues["SenderBusinessName"] : string.Empty;
            model.SenderDocumentType = dataValues.XpathsValues["SenderDocumentType"] != null ?
                    dataValues.XpathsValues["SenderDocumentType"] : string.Empty;

            model.Note = dataValues.XpathsValues["Note"] != null ? dataValues.XpathsValues["Note"] : string.Empty;
            model.SignedBy = dataValues.XpathsValues["SignedBy"] != null ? dataValues.XpathsValues["SignedBy"] : string.Empty;
            model.EventCode = dataValues.XpathsValues["EventCode"] != null ? dataValues.XpathsValues["EventCode"] : string.Empty;


            model.EventTotalValueAval = dataValues.XpathsValues["EventTotalValueAval"] != null ? dataValues.XpathsValues["EventTotalValueAval"] : string.Empty;
            model.EventTotalValueEndoso = dataValues.XpathsValues["EventTotalValueEndoso"] != null ? dataValues.XpathsValues["EventTotalValueEndoso"] : string.Empty;
            model.EventTotalValueLimitation = dataValues.XpathsValues["EventTotalValueLimitation"] != null ? dataValues.XpathsValues["EventTotalValueLimitation"] : string.Empty;
            model.EventTotalValuePago = dataValues.XpathsValues["EventTotalValuePago"] != null ? dataValues.XpathsValues["EventTotalValuePago"] : string.Empty;

            
    
            
            

            if (dataValues.XpathsValues["GenerationDate"] != null)
            {
                model.EmissionDate = dataValues.XpathsValues["GenerationDate"];
            }
            if (dataValues.XpathsValues["GenerationTime"] != null)
            {
                model.EmissionDate = string.Format("{0} {1}", model.EmissionDate,
                dataValues.XpathsValues["GenerationTime"]);
            }
            return model;
        }

        #endregion

        #endregion

    }
}
