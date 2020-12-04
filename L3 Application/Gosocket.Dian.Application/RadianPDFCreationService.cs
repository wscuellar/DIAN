using Gosocket.Dian.Application.Cosmos;
using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Cosmos;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Services;
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
        private readonly FileManager _fileManager;


        #endregion

        #region Constructor

        public RadianPdfCreationService(IQueryAssociatedEventsService queryAssociatedEventsService, FileManager fileManager)
        {
            _queryAssociatedEventsService = queryAssociatedEventsService;
            _fileManager = fileManager;
        }

        #endregion

        #region GetElectronicInvoicePdf

        public async Task<byte[]> GetElectronicInvoicePdf(string eventItemIdentifier)
        {
            // Load Templates           

            StringBuilder templateFirstPage = new StringBuilder(_fileManager.GetText("radian-documents-templates", "CertificadoExistencia.html"));
            StringBuilder templateLastPage = new StringBuilder(_fileManager.GetText("radian-documents-templates", "CertificadoExistenciaFinal.html"));
            
            // Load Document Data
            GlobalDocValidatorDocumentMeta documentMeta = _queryAssociatedEventsService.DocumentValidation(eventItemIdentifier);
            EventStatus eventStatus = (EventStatus)Enum.Parse(typeof(EventStatus), documentMeta.EventCode);

            //Load documents
            List<GlobalDocReferenceAttorney> documents =
                _queryAssociatedEventsService.ReferenceAttorneys(
                    documentMeta.DocumentKey,
                    documentMeta.DocumentReferencedKey,
                    documentMeta.ReceiverCode,
                    documentMeta.SenderCode);

            //Load Events
            GlobalDataDocument cosmosDocument = await DocumentInfoFromCosmos(documentMeta);
            List<Event> events = cosmosDocument.Events;

            // Set Variables
            DateTime expeditionDate = DateTime.Now;
            Bitmap qrCode = GenerateQR(TextResources.RadianReportQRCode.Replace("{CUFE}", documentMeta.PartitionKey));
            int page = 1;

            string ImgDataURI = IronPdf.Util.ImageToDataUri(qrCode);
            string ImgHtml = String.Format("<img class='qr-content' src='{0}'>", ImgDataURI);


            // Mapping Labels common data

            templateFirstPage = CommonDataTemplateMapping(templateFirstPage, expeditionDate, page, documentMeta);

            // Mapping firts page
            templateFirstPage = templateFirstPage.Replace("{SenderBusinessName}", documentMeta.SenderName);
            templateFirstPage = templateFirstPage.Replace("{SenderNit}", documentMeta.SenderCode);
            templateFirstPage = templateFirstPage.Replace("{InvoiceValue}", Convert.ToString(documentMeta.TotalAmount));
            templateFirstPage = templateFirstPage.Replace("{Badge}", string.Empty);
            templateFirstPage = templateFirstPage.Replace("{Currency}", string.Empty);
            templateFirstPage = templateFirstPage.Replace("{PaymentMethod}", string.Empty);
            templateFirstPage = templateFirstPage.Replace("{ExpirationDate}", $"{documentMeta.EmissionDate:yyyy'-'MM'-'dd hh:mm:ss.000} UTC-5");
            templateFirstPage = templateFirstPage.Replace("{ReceiverBusinessName}", documentMeta.ReceiverName);
            templateFirstPage = templateFirstPage.Replace("{ReceiverNit}", documentMeta.ReceiverCode);

            // Mapping Events

            // se realiza el mapeo del primer evento
            if (events.Any())
            {
                templateFirstPage = EventTemplateMapping(templateFirstPage, events[0], string.Empty);

                // si tiene más eventos realiza el mapeo del siguiente template
                if (events.Count > 1)
                {
                    bool changePage = false;
                    StringBuilder middleTemplate = new StringBuilder();

                    for (int i = 1; i < events.Count; i++)
                    {
                        if (i % 2 == 1)
                        {
                            page++;
                            middleTemplate = new StringBuilder(_fileManager.GetText("radian-documents-templates", "CertificadoExistenciaInterna.html"));
                            middleTemplate = CommonDataTemplateMapping(middleTemplate, expeditionDate, page, documentMeta);

                        }
                        else
                        {
                            changePage = true;
                        }
                        middleTemplate = EventTemplateMapping(middleTemplate, events[i], changePage ? string.Empty : "1");
                        if (changePage)
                        {
                            templateFirstPage = templateFirstPage.Append(middleTemplate);
                            changePage = false;
                        }
                    }
                }
            }

            //Mapping last page

            // se aumenta el número de la pagina y se mapean los datos comunes de pagina
            page++;
            templateLastPage = CommonDataTemplateMapping(templateLastPage, expeditionDate, page, documentMeta);

            templateLastPage = templateLastPage.Replace("{DocumentsTotal}", documents.Count.ToString());
            templateLastPage = templateLastPage.Replace("{EventsTotal}", events.Count.ToString());
            templateLastPage = templateLastPage.Replace("{ExpeditionDate}", expeditionDate.ToShortDateString());
            templateLastPage = templateLastPage.Replace("{QRCode}", ImgHtml);

            byte[] report = GetPdfBytes(templateFirstPage.Append(templateLastPage.ToString()).ToString());

            return report;
        }

        #endregion

        #region GetPdfBytes

        private static byte[] GetPdfBytes(string htmlContent)
        {
            byte[] pdf = null;

            // Convert
            pdf = Pdf
                    .From(htmlContent)
                    .WithGlobalSetting("orientation", "Portrait")
                    .WithObjectSetting("web.defaultEncoding", "utf-8")
                    .OfSize(PaperSize.A4)
                    .Content();

            return pdf;
        }

        #endregion

        #region GenerateQR

        private Bitmap GenerateQR(string invoiceUrl)
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

        private StringBuilder CommonDataTemplateMapping(StringBuilder template, DateTime expeditionDate, int page, GlobalDocValidatorDocumentMeta documentMeta)
        {
            template = template.Replace("{PrintDate}", $"Impreso el {expeditionDate:d 'de' MM 'de' yyyy 'a las' hh:mm:ss tt}");
            template = template.Replace("{PrintTime}", expeditionDate.TimeOfDay.ToString());
            template = template.Replace("{PrintPage}", page.ToString());
            template = template.Replace("{InvoiceNumber}", documentMeta.SerieAndNumber);
            template = template.Replace("{CUFE}", documentMeta.PartitionKey);
            template = template.Replace("{EInvoiceGenerationDate}", $"{documentMeta.EmissionDate:yyyy'-'MM'-'dd hh:mm:ss.000} UTC-5");
            template = template.Replace("{Status}", $"Estyado: {(EventStatus)Enum.Parse(typeof(EventStatus), documentMeta.EventCode)}");
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

        #endregion

    }
}
