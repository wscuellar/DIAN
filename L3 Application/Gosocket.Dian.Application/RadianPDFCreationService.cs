using Gosocket.Dian.Application.Cosmos;
using Gosocket.Dian.Domain.Cosmos;
using Gosocket.Dian.Interfaces.Services;
using OpenHtmlToPdf;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Application
{
    public class RadianPDFCreationService : IRadianPDFCreationService
    {
        #region Properties

        private readonly IQueryAssociatedEventsService _queryAssociatedEventsService;


        #region RadianReportQRCodeRadianReportQRCodeUrl

        private string RadianReportQRCodeRadianReportQRCodeUrl
        {
            get
            {
                if (ConfigurationManager.AppSettings["RadianReportQRCodeUrl"] != null)
                {
                    return ConfigurationManager.AppSettings["RadianReportQRCodeUrl"].ToString();
                }
                return string.Empty;
            }
        }

        #endregion

        #endregion

        #region Constructor

        public RadianPDFCreationService(IQueryAssociatedEventsService queryAssociatedEventsService)
        {
            _queryAssociatedEventsService = queryAssociatedEventsService;
        }

        #endregion

        #region GetElectronicInvoicePdf

        public async Task<byte[]> GetElectronicInvoicePdf(string eventItemIdentifier)
        {
            // Load Templates
            StringBuilder templateFirstPage = new StringBuilder(File.ReadAllText(@"\Templates\CertificadoExistencia.html"));
            // StringBuilder eventTemplate = new StringBuilder(File.ReadAllText(@"G:\Workspace\CertificadoExistenciaInterna.html"));
            StringBuilder templateLastPage = new StringBuilder(File.ReadAllText(@"\Templates\CertificadoExistenciaFinal.html"));


            // Load Document Data
            GlobalDocValidatorDocumentMeta documentMeta = _queryAssociatedEventsService.DocumentValidation(eventItemIdentifier);

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
            Bitmap qrCode = GenerateQR(this.RadianReportQRCodeRadianReportQRCodeUrl.Replace("{CUFE}", documentMeta.PartitionKey));
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
            templateFirstPage = templateFirstPage.Replace("{Expiration}", string.Empty);
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
                            middleTemplate = new StringBuilder(File.ReadAllText(@"\Templates\CertificadoExistenciaInterna.html"));
                            middleTemplate = CommonDataTemplateMapping(middleTemplate, expeditionDate, page, documentMeta);
                            
                        }else
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

            // se aumenta el número de la pagina
            page++;
            templateLastPage = CommonDataTemplateMapping(templateLastPage, expeditionDate, page, documentMeta);

            templateLastPage = templateLastPage.Replace("{DocumentsTotal}", documents.Count.ToString());
            templateLastPage = templateLastPage.Replace("{EventsTotal}", events.Count.ToString());
            templateLastPage = templateLastPage.Replace("{ExpeditionDate}", expeditionDate.ToShortDateString());
            templateLastPage = templateLastPage.Replace("{QRCode}", ImgHtml);

            byte[] report = GetPdfBytes(templateFirstPage.Append(templateLastPage.ToString()).ToString());
            File.WriteAllBytes(@"G:\Workspace\Test.pdf", report);

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

        public Bitmap GenerateQR(string invoiceUrl)
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

        StringBuilder CommonDataTemplateMapping(StringBuilder template, DateTime expeditionDate, int page, GlobalDocValidatorDocumentMeta documentMeta)
        {
            template = template.Replace("{PrintDay}", expeditionDate.Day.ToString());
            template = template.Replace("{PrintMonth}", expeditionDate.Month.ToString());
            template = template.Replace("{PrintYear}", expeditionDate.Year.ToString());
            template = template.Replace("{PrintTime}", expeditionDate.TimeOfDay.ToString());
            template = template.Replace("{PrintPage}", page.ToString());
            template = template.Replace("{InvoiceNumber}", documentMeta.SerieAndNumber);
            template = template.Replace("{CUFE}", documentMeta.PartitionKey);
            template = template.Replace("{EInvoiceGenerationDate}", documentMeta.EmissionDate.ToString("yyyy-mm-dd HH:mm:ss.sss"));
            template = template.Replace("{Status}", string.Empty);
            return template;
        }

        #endregion

        #region EventTemplateMapping

        StringBuilder EventTemplateMapping(StringBuilder template, Event eventObj, string subEvent)
        {
            template = template.Replace("{EventNumber" + subEvent + "}", eventObj.DateNumber.ToString());
            template = template.Replace("{DocumentTypeName" + subEvent + "}", eventObj.Description);
            template = template.Replace("{CUDE"+subEvent+"}", eventObj.Code);
            template = template.Replace("{ValidationDate" + subEvent + "}", eventObj.Date.ToString("yyyy-mm-dd HH:mm:ss.sss"));
            template = template.Replace("{SenderBusinessName" + subEvent + "}", eventObj.SenderName);
            template = template.Replace("{ReceiverBusinessName" + subEvent + "}", eventObj.ReceiverName);
            
            return template;
        }

        #endregion

    }
}
