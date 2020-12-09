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
    using System.Globalization;
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

        #region GetGraphicRepresentationPdfReport

        public async Task<byte[]> GetPdfReport(string eventItemIdentifier)
        {
            // Load Templates            
            StringBuilder grTemplate = new StringBuilder(_fileManager.GetText("radian-documents-templates", "RepresentaciónGráfica.html"));

            // Load Document Data
            GlobalDocValidatorDocumentMeta documentMeta = _queryAssociatedEventsService.DocumentValidation(eventItemIdentifier);
            EventStatus eventStatus;

            var docs = await _cosmosDBService.ReadDocumentAsync(documentMeta.DocumentKey, documentMeta.PartitionKey, documentMeta.EmissionDate);

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
            Bitmap qrCode = RadianPdfCreationService.GenerateQR(TextResources.RadianReportQRCode.Replace("{CUFE}", documentMeta.PartitionKey));

            //string ImgDataURI = IronPdf.Util.ImageToDataUri(qrCode);
            //string ImgHtml = String.Format("<img class='qr-content' src='{0}'>", ImgDataURI);


            // Mapping Labels common data

            grTemplate = InitialDataTemplateMapping(grTemplate, expeditionDate, documentMeta);

            // Mapping firts page
            grTemplate = grTemplate.Replace("{SenderBusinessName}", documentMeta.SenderName);
            grTemplate = grTemplate.Replace("{SenderNit}", documentMeta.SenderCode);
            grTemplate = grTemplate.Replace("{InvoiceValue}", Convert.ToString(documentMeta.TotalAmount));
            grTemplate = grTemplate.Replace("{Badge}", string.Empty);
            grTemplate = grTemplate.Replace("{Currency}", string.Empty);
            grTemplate = grTemplate.Replace("{PaymentMethod}", string.Empty);
            grTemplate = grTemplate.Replace("{ExpirationDate}", $"{documentMeta.EmissionDate:yyyy'-'MM'-'dd hh:mm:ss.000} UTC-5");
            grTemplate = grTemplate.Replace("{ReceiverBusinessName}", documentMeta.ReceiverName);
            grTemplate = grTemplate.Replace("{ReceiverNit}", documentMeta.ReceiverCode);

            // Mapping Events

            byte[] report = RadianPdfCreationService.GetPdfBytes(grTemplate.ToString());

            return report;
        }

        #endregion

        #region DocumentInfoFromCosmos

        private async Task<GlobalDataDocument> DocumentInfoFromCosmos(GlobalDocValidatorDocumentMeta documentMeta)
        {
            string emissionDateNumber = documentMeta.EmissionDate.ToString("yyyyMMdd");
            string partitionKey = $"co|{emissionDateNumber.Substring(6, 2)}|{documentMeta.DocumentKey.Substring(0, 2)}";

            DateTime date = DateTime.ParseExact(emissionDateNumber, "yyyyMMdd", CultureInfo.InvariantCulture);

            GlobalDataDocument globalDataDocument = await CosmosDBService.Instance(date).ReadDocumentAsync(documentMeta.DocumentKey, partitionKey, date);

            return globalDataDocument;
        }

        #endregion

        #region InitialDataTemplateMapping

        private StringBuilder InitialDataTemplateMapping(StringBuilder template, DateTime expeditionDate, GlobalDocValidatorDocumentMeta documentMeta)
        {
            byte[] bytesLogo = _fileManager.GetBytes("radian-dian-logos", "Logo-DIAN-2020-color.jpg");
            byte[] bytesFooter = _fileManager.GetBytes("radian-dian-logos", "GroupFooter.png");
            string imgLogo = $"<img src='data:image/jpg;base64,{Convert.ToBase64String(bytesLogo)}'>";
            string imgFooter = $"<img src='data:image/jpg;base64,{Convert.ToBase64String(bytesFooter)}' class='img-footer'>";

            template = template.Replace("{Logo}", $"{imgLogo}");
            template = template.Replace("{ImgFooter}", $"{imgFooter}");
            template = template.Replace("{PrintDate}", $"Impreso el {expeditionDate:d 'de' MM 'de' yyyy 'a las' hh:mm:ss tt}");
            template = template.Replace("{PrintTime}", expeditionDate.TimeOfDay.ToString());
            template = template.Replace("{InvoiceNumber}", documentMeta.SerieAndNumber);
            template = template.Replace("{CUFE}", documentMeta.PartitionKey);
            template = template.Replace("{EInvoiceGenerationDate}", $"{documentMeta.EmissionDate:yyyy'-'MM'-'dd hh:mm:ss.000} UTC-5");
            template = template.Replace("{Status}", $": ");
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
