using Gosocket.Dian.Application.Cosmos;
using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Cosmos;
using Gosocket.Dian.Domain.Domain;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Services.Utils.Helpers;
using OpenHtmlToPdf;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Application
{
    public class RadianPdfCreationService : IRadianPdfCreationService
    {
        #region Properties

        private readonly IQueryAssociatedEventsService _queryAssociatedEventsService;
        private readonly IGlobalDocValidationDocumentMetaService _globalDocValidationDocumentMetaService;
        private readonly FileManager _fileManager;
        private readonly CosmosDBService _cosmosDBService;

        #endregion

        #region Constructor

        public RadianPdfCreationService(IQueryAssociatedEventsService queryAssociatedEventsService, FileManager fileManager, CosmosDBService cosmosDBService, IGlobalDocValidationDocumentMetaService globalDocValidationDocumentMetaService)
        {
            _queryAssociatedEventsService = queryAssociatedEventsService;
            _fileManager = fileManager;
            _cosmosDBService = cosmosDBService;
            _globalDocValidationDocumentMetaService = globalDocValidationDocumentMetaService;
        }

        #endregion

        #region GetElectronicInvoicePdf

        public async Task<byte[]> GetElectronicInvoicePdf(string eventItemIdentifier, string webPath)
        {

                // Load Templates            

                StringBuilder templateFirstPage = new StringBuilder(_fileManager.GetText("radian-documents-templates", "CertificadoExistencia.html"));
                StringBuilder templateLastPage = new StringBuilder(_fileManager.GetText("radian-documents-templates", "CertificadoExistenciaFinal.html"));
                StringBuilder footerTemplate = new StringBuilder(_fileManager.GetText("radian-documents-templates", "CertificadoExistenciaFooter.html"));
                StringBuilder firstEvent = new StringBuilder(_fileManager.GetText("radian-documents-templates", "CertificadoExistenciaFirstEvent.html"));


                // load xml
                byte[] xmlBytes = RadianSupportDocument.GetXmlFromStorageAsync(eventItemIdentifier);
                //var str = Encoding.Default.GetString(xmlBytes);

                Dictionary<string, string> xpathRequest = new Dictionary<string, string>();
                xpathRequest = CreateGetXpathData(Convert.ToBase64String(xmlBytes), "RepresentacionGrafica");
                ResponseXpathDataValue fieldValues = ApiHelpers.ExecuteRequest<ResponseXpathDataValue>(ConfigurationManager.GetValue("GetXpathDataValuesUrl"), xpathRequest);
                //ResponseXpathDataValue fieldValues = ApiHelpers.ExecuteRequest<ResponseXpathDataValue>("https://global-function-docvalidator-sbx.azurewebsites.net/api/GetXpathDataValues?code=tyW3skewKS1q4GuwaOj0PPj3mRHa5OiTum60LfOaHfEMQuLbvms73Q==", xpathRequest);

                // Load Document Data
                GlobalDocValidatorDocumentMeta documentMeta = _queryAssociatedEventsService.DocumentValidation(eventItemIdentifier);
                EventStatus eventStatus;
                _ = await _cosmosDBService.ReadDocumentAsync(documentMeta.DocumentKey, documentMeta.PartitionKey, documentMeta.EmissionDate);

                //Load documents
                List<GlobalDocReferenceAttorney> documents =
                    _queryAssociatedEventsService.ReferenceAttorneys(
                        documentMeta.DocumentKey,
                        documentMeta.DocumentReferencedKey,
                        documentMeta.ReceiverCode,
                        documentMeta.SenderCode);

                //Load Events
                GlobalDataDocument cosmosDocument = await DocumentInfoFromCosmos(documentMeta);
                List<Event> eventsCosmos = cosmosDocument.Events;
                List<GlobalDocValidatorDocumentMeta> storageEvents = _globalDocValidationDocumentMetaService.FindDocumentByReference(documentMeta.DocumentKey);
                List<Event> events = ListEvents(eventsCosmos, storageEvents);
                xmlBytes = RadianSupportDocument.GetXmlDianFromStorage(eventItemIdentifier, documentMeta);
                Dictionary<string, string> newXpathRequest = CreateGetXpathValidation(Convert.ToBase64String(xmlBytes), "InvoiceValidation");
                ResponseXpathDataValue newFieldValues = ApiHelpers.ExecuteRequest<ResponseXpathDataValue>("https://global-function-docvalidator-sbx.azurewebsites.net/api/GetXpathDataValues?code=tyW3skewKS1q4GuwaOj0PPj3mRHa5OiTum60LfOaHfEMQuLbvms73Q==", newXpathRequest);

                events.Insert(0, new Event()
                {

                    Description = newFieldValues.XpathsValues["Description"],
                    DocumentKey = newFieldValues.XpathsValues["CUDE"],
                    Date = Convert.ToDateTime(newFieldValues.XpathsValues["SigningTime"]),
                    SenderName = newFieldValues.XpathsValues["Sender"],
                    ReceiverName = newFieldValues.XpathsValues["Receiver"]

                });

                Dictionary<int, string> dicStatus = _queryAssociatedEventsService.IconType(null, documentMeta.DocumentKey);
                string invoiceStatus = dicStatus.OrderBy(t => t.Key).Last().Value;

                // Set Variables
                DateTime expeditionDate = DateTime.UtcNow.AddHours(-5);
                Bitmap qrCode = GenerateQR($"{webPath}?documentkey={documentMeta.PartitionKey}");
                int page = 1;

                string ImgDataURI = IronPdf.Util.ImageToDataUri(qrCode);
                string ImgHtml = String.Format("<img class='qr-content' src='{0}'>", ImgDataURI);

                firstEvent = EventTemplateMapping(firstEvent, events[0], string.Empty);
                templateFirstPage = templateFirstPage.Append(firstEvent);
                templateFirstPage = templateFirstPage.Append(footerTemplate);

                // Mapping Labels common data

                templateFirstPage = CommonDataTemplateMapping(templateFirstPage, expeditionDate, page, documentMeta, invoiceStatus);

                // Mapping firts page
                templateFirstPage = templateFirstPage.Replace("{SenderBusinessName}", documentMeta.SenderName);
                templateFirstPage = templateFirstPage.Replace("{SenderNit}", documentMeta.SenderCode);
                templateFirstPage = templateFirstPage.Replace("{InvoiceValue}", Convert.ToString(documentMeta.TotalAmount));
                templateFirstPage = templateFirstPage.Replace("{Badge}", string.Empty);

                templateFirstPage = templateFirstPage.Replace("{Currency}",
                    fieldValues.XpathsValues["InvoiceCurrencyId"] != null ? fieldValues.XpathsValues["InvoiceCurrencyId"] : string.Empty);


                if (fieldValues.XpathsValues["InvoicePaymentId"] != null)
                {
                    templateFirstPage = templateFirstPage.Replace("{PaymentMethod}", fieldValues.XpathsValues["InvoicePaymentId"].Equals("1") ? "CONTADO" : "A CRÉDITO");
                }
                else
                {
                    templateFirstPage = templateFirstPage.Replace("{PaymentMethod}", string.Empty);
                }

                templateFirstPage = templateFirstPage.Replace("{ExpirationDate}", $"{documentMeta.EmissionDate:yyyy'-'MM'-'dd hh:mm:ss.000} UTC-5");
                templateFirstPage = templateFirstPage.Replace("{ReceiverBusinessName}", documentMeta.ReceiverName);
                templateFirstPage = templateFirstPage.Replace("{ReceiverNit}", documentMeta.ReceiverCode);



                // Mapping Events

                // se realiza el mapeo del primer evento
                // si tiene más eventos realiza el mapeo del siguiente template
                StringBuilder middleTemplate = new StringBuilder();
                StringBuilder eventTemplate = new StringBuilder();
                StringBuilder headerTemplate = new StringBuilder();

                if (events.Any())
                {
                    for (int i = 1; i < events.Count; i++)
                    {

                        if (i % 2 == 1)
                        {
                            page++;
                            middleTemplate = new StringBuilder(_fileManager.GetText("radian-documents-templates", "CertificadoExistenciaHeader.html"));
                            eventTemplate = new StringBuilder(_fileManager.GetText("radian-documents-templates", "CertificadoExistenciaInterna.html"));
                            eventTemplate = EventTemplateMapping(eventTemplate, events[i], string.Empty);
                            middleTemplate = middleTemplate.Append(eventTemplate);
                            middleTemplate = CommonDataTemplateMapping(middleTemplate, expeditionDate, page, documentMeta, invoiceStatus);
                            templateFirstPage = templateFirstPage.Append(middleTemplate);
                        }
                        if (i % 2 == 0)
                        {
                            eventTemplate = new StringBuilder(_fileManager.GetText("radian-documents-templates", "CertificadoExistenciaInterna.html"));
                            eventTemplate = EventTemplateMapping(eventTemplate, events[i], string.Empty);
                            templateFirstPage = templateFirstPage.Append(eventTemplate);
                            footerTemplate = CommonDataTemplateMapping(footerTemplate, expeditionDate, page, documentMeta, invoiceStatus);
                            templateFirstPage = templateFirstPage.Append(footerTemplate);
                        }
                    }
                }

                //Mapping last page
                // se aumenta el número de la pagina y se mapean los datos comunes de pagina
                if (events.Count % 2 == 0)
                {
                    templateLastPage = templateLastPage.Replace("{DocumentsTotal}", documents.Count.ToString());
                    templateLastPage = templateLastPage.Replace("{EventsTotal}", events.Count.ToString());
                    templateLastPage = templateLastPage.Replace("{ExpeditionDate}", expeditionDate.ToShortDateString());
                    templateLastPage = templateLastPage.Replace("{QRCode}", ImgHtml);
                    templateLastPage = templateLastPage.Append(footerTemplate);
                    templateLastPage = CommonDataTemplateMapping(templateLastPage, expeditionDate, page, documentMeta, invoiceStatus);
                }
                else
                {
                    page++;
                    headerTemplate = new StringBuilder(_fileManager.GetText("radian-documents-templates", "CertificadoExistenciaHeader.html"));
                    templateLastPage = templateLastPage.Replace("{DocumentsTotal}", documents.Count.ToString());
                    templateLastPage = templateLastPage.Replace("{EventsTotal}", events.Count.ToString());
                    templateLastPage = templateLastPage.Replace("{ExpeditionDate}", expeditionDate.ToShortDateString());
                    templateLastPage = templateLastPage.Replace("{QRCode}", ImgHtml);
                    templateLastPage = templateLastPage.Append(footerTemplate);
                    headerTemplate = headerTemplate.Append(templateLastPage);
                    templateLastPage = CommonDataTemplateMapping(headerTemplate, expeditionDate, page, documentMeta, invoiceStatus);
                }

                byte[] report = GetPdfBytes(templateFirstPage.Append(templateLastPage.ToString()).ToString(), "Factura electronica");

                return report;          
        }

        #endregion

        #region GetPdfBytes

        public static byte[] GetPdfBytes(string htmlContent, string documentName = null)
        {
            byte[] pdf = null;

            // Convert
            pdf = Pdf
                .From(htmlContent)
                .WithTitle(documentName)
                .WithGlobalSetting("orientation", "Portrait")
                .WithObjectSetting("web.defaultEncoding", "utf-8")
                .OfSize(PaperSize.A4)
                .WithTitle(documentName)
                .WithMargins(1.0.Centimeters())
                .Content();

            return pdf;
        }

        #endregion

        #region GenerateQR

        public static Bitmap GenerateQR(string invoiceUrl)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(invoiceUrl, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            return qrCodeImage;
        }

        #endregion

        #region DocumentInfoFromCosmos

        private async Task<GlobalDataDocument> DocumentInfoFromCosmos(GlobalDocValidatorDocumentMeta documentMeta)
        {
            string emissionDateNumber = documentMeta.EmissionDate.ToString("yyyyMMdd");
            string partitionKey = $"co|{emissionDateNumber.Substring(6, 2)}|{documentMeta.DocumentKey.Substring(0, 2)}";

            DateTime date = DateNumberToDateTime(emissionDateNumber);

            GlobalDataDocument globalDataDocument = await CosmosDBService.Instance(date).ReadDocumentAsync(documentMeta.DocumentKey, partitionKey, date);

            return globalDataDocument;
        }

        #endregion

        #region DateNumberToDateTime

        private DateTime DateNumberToDateTime(string date)
        {
            return DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture);
        }

        #endregion

        #region CommonDataTemplateMapping

        private StringBuilder CommonDataTemplateMapping(StringBuilder template, DateTime expeditionDate, int page, GlobalDocValidatorDocumentMeta documentMeta, string invoiceStatus)
        {
            byte[] bytesLogo = _fileManager.GetBytes("radian-dian-logos", "Logo-DIAN-2020-color.jpg");
            byte[] bytesFooter = _fileManager.GetBytes("radian-dian-logos", "GroupFooter.png");
            string imgLogo = $"<img src='data:image/jpg;base64,{Convert.ToBase64String(bytesLogo)}'>";
            string imgFooter = $"<img src='data:image/jpg;base64,{Convert.ToBase64String(bytesFooter)}' class='img-footer'>";
            
            template = template.Replace("{Logo}", $"{imgLogo}");
            template = template.Replace("{ImgFooter}", $"{imgFooter}");
            template = template.Replace("{PrintDate}", $"Impreso el {expeditionDate:d 'de' MM 'de' yyyy 'a las' hh:mm:ss tt}");
            template = template.Replace("{PrintTime}", expeditionDate.TimeOfDay.ToString());
            template = template.Replace("{PrintPage}", page.ToString());
            template = template.Replace("{InvoiceNumber}", documentMeta.SerieAndNumber);
            template = template.Replace("{CUFE}", documentMeta.PartitionKey);
            template = template.Replace("{EInvoiceGenerationDate}", $"{documentMeta.EmissionDate:yyyy'-'MM'-'dd hh:mm:ss.000} UTC-5");
            template = template.Replace("{Status}", invoiceStatus.ToUpper());
            return template;
        }

        #endregion

        #region EventTemplateMapping

        private StringBuilder EventTemplateMapping(StringBuilder template, Event eventObj, string subEvent)
        {
            template = template.Replace("{EventNumber" + subEvent + "}", eventObj.Code);
            template = template.Replace("{DocumentTypeName" + subEvent + "}", eventObj.Description);
            template = template.Replace("{CUDE" + subEvent + "}", eventObj.DocumentKey);
            template = template.Replace("{ValidationDate}", $"{eventObj.Date:yyyy'-'MM'-'dd hh:mm:ss.000} UTC-5");
            template = template.Replace("{SenderBusinessName" + subEvent + "}", eventObj.SenderName);
            template = template.Replace("{ReceiverBusinessName" + subEvent + "}", eventObj.ReceiverName);

            return template;
        }

        private List<Event> ListEvents(List<Event> events, List<GlobalDocValidatorDocumentMeta> documents)
        {
            List<Event> finalEvents = new List<Event>();

            foreach (var item in documents)
            {
                GlobalDocValidatorDocumentMeta document = documents.LastOrDefault(d => d.DocumentKey == item.DocumentKey);

                if (!string.IsNullOrEmpty(document.EventCode))
                {
                    Event newEvent = events.LastOrDefault(e => e.DocumentKey == item.DocumentKey);
                    if (newEvent != null)
                    {
                        newEvent.Description = EnumHelper.GetEnumDescription((EventStatus)int.Parse(document.EventCode));
                        finalEvents.Add(newEvent);
                    }    
                }
            }

            finalEvents = finalEvents.Where(e=>e.Code!=null).OrderBy(t => t.Date).ToList();

            return finalEvents;
        }

        #endregion

        #region CreateGetXpathData

        private static Dictionary<string, string> CreateGetXpathData(string xmlBase64, string fileName = null)
        {
            var requestObj = new Dictionary<string, string>
            {
                { "XmlBase64", xmlBase64},
                { "FileName", fileName},
                { "InvoicePaymentId", "//*[local-name()='PaymentMeans']/*[local-name()='ID']" },
                { "InvoiceCurrencyId", "//*[local-name()='Invoice']/*[local-name()='DocumentCurrencyCode']" },
                ////{ "","" },
            };
            return requestObj;
        }

        #endregion

        private static Dictionary<string, string> CreateGetXpathValidation(string xmlBase64, string fileName = null)

        {
            var requestObj = new Dictionary<string, string>
            {
                { "XmlBase64", xmlBase64},
                { "FileName", fileName},
                { "SigningTime", "//*[local-name()='SigningTime']" },
                { "CUDE", "//*[local-name()='UUID'][@schemeName='CUDE-SHA384']" },
                { "Description", "//*[local-name()='DocumentResponse']/*[local-name()='Response']/*[local-name()='Description']" },
                { "Sender", "//*[local-name()='SenderParty']/*[local-name()='PartyTaxScheme']/*[local-name()='RegistrationName']" },
                { "Receiver", "//*[local-name()='ReceiverParty']/*[local-name()='PartyTaxScheme']/*[local-name()='RegistrationName']" }
            };
            return requestObj;

        }

    }
}
