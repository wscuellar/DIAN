using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Pdf
{
    public class PDFUtiles
    {

        public static byte[] GetPdfBytes(string htmlContent)
        {
            byte[] pdf = null;
            // Convert
            pdf = OpenHtmlToPdf.Pdf
                    .From(htmlContent)
                    .WithGlobalSetting("orientation", "Portrait")
                    .WithObjectSetting("web.defaultEncoding", "utf-8")
                    .Content();
            return pdf;
        }
    }
}
