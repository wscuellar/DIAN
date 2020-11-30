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
            StringBuilder template = new StringBuilder(File.ReadAllText("../../Templates/ElectronicInvoiceExistenceCertificateReport.html"));
            StringBuilder eventTemplate = new StringBuilder(File.ReadAllText("../../Templates/RadianEventsTemplate.html"));

            // Load Document Data
            GlobalDocValidatorDocumentMeta documentMeta = _queryAssociatedEventsService.DocumentValidation(eventItemIdentifier);

            //Load documents
            List<GlobalDocReferenceAttorney> documents = _queryAssociatedEventsService.ReferenceAttorneys(documentMeta.DocumentKey, documentMeta.DocumentReferencedKey, documentMeta.ReceiverCode, documentMeta.SenderCode);

            //Load Events
            

            // Set Variables
            DateTime expeditionDate = DateTime.Now;
            var qrCode = GenerateQR(string.Format("{0}{1}", this.RadianReportQRCodeRadianReportQRCodeUrl, documentMeta.PartitionKey));

            // Mapping Labels
            template = template.Replace("{PrintDay}", expeditionDate.Day.ToString());
            template = template.Replace("{PrintMonth}", expeditionDate.Month.ToString());
            template = template.Replace("{PrintYear}", expeditionDate.Year.ToString());
            template = template.Replace("{PrintTime}", expeditionDate.TimeOfDay.ToString());
            template = template.Replace("{InvoiceNumber}", documentMeta.SerieAndNumber);
            template = template.Replace("{CUFE}", documentMeta.PartitionKey);
            template = template.Replace("{EInvoiceGenerationDate}", documentMeta.EmissionDate.ToString("yyyy-mm-dd HH:mm:ss.sss"));
            template = template.Replace("{SenderBusinessName}", documentMeta.SenderName);
            template = template.Replace("{EInvoiceNit}", documentMeta.SenderCode);

            template = template.Replace("{InvoiceValue}", Convert.ToString(documentMeta.TotalAmount));
            template = template.Replace("{Badge}", string.Empty);
            template = template.Replace("{PaymentMethod}", string.Empty);
            template = template.Replace("{Expiration}", string.Empty);
            template = template.Replace("{Acquirer}", documentMeta.ReceiverName);
            template = template.Replace("{AcquirerNit}", documentMeta.ReceiverCode);
            template = template.Replace("{DocumentsTotal}", $"{documents.Count}");
            template = template.Replace("{EventsTotal}", string.Empty);
            template = template.Replace("{ExpeditionDate}", expeditionDate.ToShortDateString());
            template = template.Replace("{QRCode}", qrCode.ToString());

            // Mapping Events

            

            // Render Pdf
            byte[] report = GetPdfBytes(template.ToString());

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
