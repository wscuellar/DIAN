using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Services;
using OpenHtmlToPdf;
using System.IO;
using System.Text;

namespace Gosocket.Dian.Application
{
    public class RadianPDFCreationService : IRadianPDFCreationService
    {
        private readonly IQueryAssociatedEventsService _queryAssociatedEventsService;

        public RadianPDFCreationService(IQueryAssociatedEventsService queryAssociatedEventsService)
        {
            _queryAssociatedEventsService = queryAssociatedEventsService;
        }

        public byte[] GetElectronicInvoicePdf(string eventItemIdentifier)
        {
            GlobalDocValidatorDocument input = _queryAssociatedEventsService.EventVerification(eventItemIdentifier);

            StringBuilder template = new StringBuilder(File.ReadAllText("../../Bin/Debug/Templates/RadianReport.html"));

            template = template.Replace("{cude}", input.PartitionKey);

            byte[] report = GetPdfBytes(template.ToString());

            return report;
        }

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
    }
}
