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

            byte[] report = RadianPdfCreationService.GetPdfBytes(template.ToString(), "Representacion grafica");

            return report;
        }

        #endregion

        #region GetEventDataModel

        private async Task<Domain.Entity.EventDataModel> GetEventDataModel(string cude)
        {
            GlobalDocValidatorDocumentMeta eventItem = _queryAssociatedEventsService.DocumentValidation(cude);
            byte[] xmlBytes = RadianSupportDocument.GetXmlFromStorageAsync(cude);
            var str = Encoding.Default.GetString(xmlBytes);

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
            //ResponseXpathDataValue fieldValues = ApiHelpers.ExecuteRequest<ResponseXpathDataValue>("https://global-function-docvalidator-sbx.azurewebsites.net/api/GetXpathDataValues?code=tyW3skewKS1q4GuwaOj0PPj3mRHa5OiTum60LfOaHfEMQuLbvms73Q==", xpathRequest);
            model = MappingXpathValues(model, fieldValues);

            // Set Titles
            model.Title = _queryAssociatedEventsService.EventTitle(model.EventStatus, eventItem.CustomizationID, eventItem.EventCode);

            switch (model.EventStatus)
            {
                case EventStatus.Received:
                    model.EventTitle = "Acuse de Recibo de la FEV";
                    break;
                case EventStatus.Receipt:
                    model.EventTitle = "Recibo del bien o servicio";
                    break;
                case EventStatus.AceptacionTacita:
                    model.EventTitle = "Aceptación Tácita de FEV";
                    break;
                case EventStatus.Accepted:
                    model.EventTitle = "Aceptación Expresa de FEV";
                    break;
                case EventStatus.EndosoGarantia:
                    model.Title = model.EventStatus.GetDescription();
                    model.EventTitle = "Endoso En garantía de la FEV TV";
                    model.RequestType = eventItem.EventCode;
                    model.DiscountRate = eventItem.TaxAmountIva.ToString();
                    model.TotalAmount = eventItem.TotalAmount.ToString();
                    break;
                case EventStatus.EndosoPropiedad:
                    model.Title = model.EventStatus.GetDescription();
                    model.EventTitle = "Endoso En propiedad de la FEV TV";
                    model.RequestType = eventItem.EventCode;
                    model.DiscountRate = eventItem.TaxAmountIva.ToString();
                    model.TotalAmount = eventItem.TotalAmount.ToString();
                    break;
                case EventStatus.EndosoProcuracion:
                    model.Title = model.EventStatus.GetDescription();
                    model.EventTitle = "Endoso En procuración de la FEV TV";
                    model.RequestType = eventItem.EventCode;
                    model.DiscountRate = eventItem.TaxAmountIva.ToString();
                    model.TotalAmount = eventItem.TotalAmount.ToString();
                    break;
                case EventStatus.InvoiceOfferedForNegotiation:
                    model.Title = model.EventStatus.GetDescription();
                    model.EventTitle = "Cancelación del endoso electrónico de la FEV";
                    model.RequestType = eventItem.EventCode;
                    model.DiscountRate = eventItem.TaxAmountIva.ToString();
                    model.TotalAmount = eventItem.TotalAmount.ToString();
                    break;
                case EventStatus.Avales:
                    model.EventTitle = "Aval de la FEV";
                    break;
                case EventStatus.Mandato:
                    model.EventTitle = "Mandato de la FEV";
                    break;
                case EventStatus.TerminacionMandato:
                    model.EventTitle = "Terminación Mandato de la FEV TV";
                    break;
                case EventStatus.ValInfoPago:
                    model.EventTitle = "Informe para el pago de la FEV TV";
                    break;
                case EventStatus.NotificacionPagoTotalParcial:
                    model.EventTitle = "Pago de la FEV TV";
                    break;
                case EventStatus.AnulacionLimitacionCirculacion:
                    model.EventTitle = "Terminación Limitación  para circulación de la FEV TV";
                    break;
                case EventStatus.NegotiatedInvoice:
                    model.EventTitle = "Limitación  para circulación de la FEV TV";
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
            string sectionHtml = "<div class='text-section padding-top20'> Sección {SectionNumber}</ div > ";

            #region Mapping Event Data Section
            // Mapping Event Data Section
            template = template.Replace("{EventName}", model.Title);
            template = template.Replace("{EventNumber}", $"{model.Prefix} - {model.Number}");
            template = template.Replace("{EventType}", model.EventTitle);
            template = template.Replace("{OperationType}", model.RequestType);
            template = template.Replace("{OperationDetails}", model.OperationDetails);
            template = template.Replace("{DiscountRate}", model.DiscountRate);
            template = template.Replace("{TotalEventAmount}", model.EventTotalAmount);
            template = template.Replace("{CUDE}", model.CUDE);
            template = template.Replace("{EmissionDate}", $"{model.EmissionDate:dd'/'MM'/'yyyy hh:mm:ss tt}");
            template = template.Replace("{RegistrationDate}", model.DateOfIssue);
            template = template.Replace("{StartDate}", model.EventStartDate);
            template = template.Replace("{FinishDate}", model.EventFinishDate);
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
                    , model.ReceiverDocumentType
                    , model.ReceiverCode
                    , string.Empty
                    , string.Empty
                    , model.ReceiverEmail
                    , model.ReceiverPhoneNumber);

                // Section 2

                subjects.Append(sectionHtml);
                subjects.Append(templateSujeto);

                subjects = SubjectTemplateMapping(subjects, "2", "1",
                    model.SenderName
                    , model.ReceiverType
                    , model.SenderDocumentType
                    , model.SenderCode
                    , string.Empty
                    , string.Empty
                    , model.SenderEmail
                    , model.SenderPhoneNumber);

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

        #region Xpaths

        #region CreateGetXpathData

        private static Dictionary<string, string> CreateGetXpathData(string xmlBase64, string fileName = null)
        {
            var requestObj = new Dictionary<string, string>
            {
                { "XmlBase64", xmlBase64},
                { "FileName", fileName},
                { "ReceiverEmail", "ApplicationResponse/cac:ReceiverParty/cac:Contact/cbc:ElectronicMail" },
                { "SenderEmail", "ApplicationResponse/cac:SenderParty/cac:Contact/cbc:ElectronicMail" },
                { "SenderPhoneNumber", "ApplicationResponse/cac:SenderParty/cac:Contact/cbc:Telephone" },
                { "ReceiverPhoneNumber", "ApplicationResponse/cac:ReceiverParty/cac:Contact/cbc:Telephone" },
                { "ReceiverDocumentType", "ApplicationResponse[1]/cac:DocumentResponse[1] / cac:IssuerParty[1] / cac:PartyLegalEntity[1] / cbc:CompanyID[1] / @schemeName" },
                { "EventTotalAmount", "ApplicationResponse[1]/cac:DocumentResponse[1]/cac:IssuerParty[1]/cac:PartyLegalEntity[X]/cbc:CorporateStockAmount[1]" },
                { "EventStartDate", "ApplicationResponse[1]/cac:DocumentResponse[1]/cac:DocumentReference[1]/cac:ValidityPeriod[1]/cbc:StartDate[1]" },
                { "EventFinishDate","ApplicationResponse[1]/cac:DocumentResponse[1]/cac:DocumentReference[1]/cac:ValidityPeriod[1]/cbc:EndDate[1]" },

                { "RequestType", "ApplicationResponse[1]/cac:DocumentResponse[1]/cac:Response[1]/cbc:ResponseCode[1]" },
                { "OperationDetails", "ApplicationResponse[1]/cac:DocumentResponse[1]/cac:Response[1]/cbc:ResponseCode[1]/@listID" },
                { "DiscountRate", "ApplicationResponse[1]/ext:UBLExtensions[1]/ext:UBLExtension[2]/ext:ExtensionContent[1]/CustomTagGeneral[1]/InformacionNegociacion[1]/Value[3]" },
                { "EndosoTotalAmount", "ApplicationResponse[1]/ext:UBLExtensions[1]/ext:UBLExtension[2]/ext:ExtensionContent[1]/CustomTagGeneral[1]/InformacionNegociacion[1]/Value[1]" },
                { "GenerationDate", "ApplicationResponse[1]/cbc:IssueDate[1]" },
                { "GenerationTime", "ApplicationResponse[1]/cbc:IssueTime[1]" },
                { "SenderNit", "ApplicationResponse[1]/cac:SenderParty[1]/cac:PartyTaxScheme[1]/cbc:CompanyID[1]" },
                { "EventDescription","ApplicationResponse[1]/cac:DocumentResponse[1]/cac:Response[1]/cbc:Description[1]" },
                { "SenderBusinessName","ApplicationResponse[1]/cac:SenderParty[1]/cac:PartyTaxScheme[1]/cbc:RegistrationName[1]" },
                { "SenderDocumentType","ApplicationResponse[1]/cac:SenderParty[1]/cac:PartyTaxScheme[1]/cbc:CompanyID[1]" },
                //{ "","" },
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
            //model.DateOfIssue = dataValues.XpathsValues["GenerationDate"] != null ?
            //        dataValues.XpathsValues["GenerationDate"] : string.Empty;
            model.SenderNit = dataValues.XpathsValues["SenderNit"] != null ?
                    dataValues.XpathsValues["SenderNit"] : string.Empty;
            model.EventDescription = dataValues.XpathsValues["EventDescription"] != null ?
                    dataValues.XpathsValues["EventDescription"] : string.Empty;
            model.SenderBusinessName = dataValues.XpathsValues["SenderBusinessName"] != null ?
                    dataValues.XpathsValues["SenderBusinessName"] : string.Empty;
            model.SenderDocumentType = dataValues.XpathsValues["SenderDocumentType"] != null ?
                    dataValues.XpathsValues["SenderDocumentType"] : string.Empty;

            return model;
        }

        #endregion

        #endregion
    }
}
