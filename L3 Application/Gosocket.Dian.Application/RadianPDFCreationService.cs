using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Application
{
    public class RadianPDFCreationService
    {
        public RadianPDFCreationService()
        {

        }

        //public byte[] GetElectronicInvoicePdf()
        //{
        //    StringBuilder template = new StringBuilder(File.ReadAllText("../../Bin/Debug/Templates/RadianReport.html"));

        //    /*  Espacio para mapeo de datos y etiquetas
        //    template = template.Replace("{prefijo}", input.Prefix);
        //    var report = PDFReport.PdfRender(template.ToString());
        //    */

        //    byte[] report = GetPdfBytes(template.ToString());

        //    return report;
        //}

        //public static byte[] GetPdfBytes(string htmlContent)
        //{
        //    byte[] pdf = null;
        //    // Convert
        //    pdf = OpenHtmlToPdf.Pdf
        //            .From(htmlContent)
        //            .WithGlobalSetting("orientation", "Portrait")
        //            .WithObjectSetting("web.defaultEncoding", "utf-8")
        //            .Content();
        //    return pdf;
        //}
    }
}
