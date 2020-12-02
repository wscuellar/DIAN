using Gosocket.Dian.Interfaces.Services;
using OpenHtmlToPdf;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Text;

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

        public byte[] GetElectronicInvoicePdf(string eventItemIdentifier)
        {
            // Load Templates
            StringBuilder templateFirstPage = new StringBuilder(File.ReadAllText("../../Templates/CertificadoExistencia.html"));
            StringBuilder eventTemplate = new StringBuilder(File.ReadAllText("../../Templates/CertificadoExistenciaInterna.html"));
            StringBuilder templateLastPage = new StringBuilder(File.ReadAllText("../../Templates/CertificadoExistenciaFinal.html"));


            // Load Document Data
            GlobalDocValidatorDocumentMeta documentMeta = _queryAssociatedEventsService.DocumentValidation(eventItemIdentifier);

            //Load documents
            List<GlobalDocReferenceAttorney> documents = _queryAssociatedEventsService.ReferenceAttorneys(documentMeta.DocumentKey, documentMeta.DocumentReferencedKey, documentMeta.ReceiverCode, documentMeta.SenderCode);

            //Load Events
            

            // Set Variables
            DateTime expeditionDate = DateTime.Now;
            Bitmap qrCode = GenerateQR(string.Format("{0}{1}", this.RadianReportQRCodeRadianReportQRCodeUrl, documentMeta.PartitionKey));
            int page = 1;
            int eventNumber = 1;

            // Mapping Labels common data
            templateFirstPage = templateFirstPage.Replace("{PrintDay}", expeditionDate.Day.ToString());
            templateFirstPage = templateFirstPage.Replace("{PrintMonth}", expeditionDate.Month.ToString());
            templateFirstPage = templateFirstPage.Replace("{PrintYear}", expeditionDate.Year.ToString());
            templateFirstPage = templateFirstPage.Replace("{PrintTime}", expeditionDate.TimeOfDay.ToString());
            templateFirstPage = templateFirstPage.Replace("{PrintPage}", page.ToString());
            templateFirstPage = templateFirstPage.Replace("{InvoiceNumber}", documentMeta.SerieAndNumber);
            templateFirstPage = templateFirstPage.Replace("{CUFE}", documentMeta.PartitionKey);
            templateFirstPage = templateFirstPage.Replace("{EInvoiceGenerationDate}", documentMeta.EmissionDate.ToString("yyyy-mm-dd HH:mm:ss.sss"));
            templateFirstPage = templateFirstPage.Replace("{Status}", string.Empty);

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

            //Mapping last page
            templateLastPage = templateLastPage.Replace("{PrintDay}", expeditionDate.Day.ToString());
            templateLastPage = templateLastPage.Replace("{PrintMonth}", expeditionDate.Month.ToString());
            templateLastPage = templateLastPage.Replace("{PrintYear}", expeditionDate.Year.ToString());
            templateLastPage = templateLastPage.Replace("{PrintTime}", expeditionDate.TimeOfDay.ToString());
            templateLastPage = templateLastPage.Replace("{PrintPage}", page.ToString());
            templateLastPage = templateLastPage.Replace("{InvoiceNumber}", documentMeta.SerieAndNumber);
            templateLastPage = templateLastPage.Replace("{CUFE}", documentMeta.PartitionKey);
            templateLastPage = templateLastPage.Replace("{EInvoiceGenerationDate}", documentMeta.EmissionDate.ToString("yyyy-mm-dd HH:mm:ss.sss"));
            templateLastPage = templateLastPage.Replace("{Status}", string.Empty);

            templateLastPage = templateLastPage.Replace("{DocumentsTotal}", $"{documents.Count}");
            templateLastPage = templateLastPage.Replace("{EventsTotal}", string.Empty);
            templateLastPage = templateLastPage.Replace("{ExpeditionDate}", expeditionDate.ToShortDateString());
            templateLastPage = templateLastPage.Replace("{QRCode}", qrCode.ToString());

            // Mapping Events
                        

            // Render Pdf
            byte[] report = GetPdfBytes(templateFirstPage.ToString());
            //

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
    }
}
