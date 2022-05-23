using Gosocket.Dian.Domain.Domain;
using Gosocket.Dian.Functions.Utils;
using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using OpenHtmlToPdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Pdf
{
    public static class GetPdf
    {
        private static readonly TableManager tableManagerGlobalDocValidatorDocumentMeta = new TableManager("GlobalDocValidatorDocumentMeta");

        [FunctionName("GetPdf")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                // Definir contenedor de parámetro
                string trackId = req.GetQueryNameValuePairs()
                    .FirstOrDefault(q => string.Compare(q.Key, "trackId", true) == 0)
                    .Value;

                // Obtener parámetros de consulta
                dynamic data = await req.Content.ReadAsAsync<object>();

                // Establecer nombre para consultar la cadena o los datos del cuerpo
                trackId = trackId ?? data?.trackId;

                if (trackId == null)
                    return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a trackId on the query string or in the request body");

                // Descargar Bytes de XML a partir de TrackId
                var requestObj = new { trackId };
                var response = await Utils.Utils.DownloadXmlAsync(requestObj);
                if (!response.Success)
                    throw new Exception(response.Message);
                var xmlBytes = Convert.FromBase64String(response.XmlBase64);

                //Consultar si existe ApplicationResponse
                GlobalDocValidatorDocumentMeta documentMetaEntity = null;
                documentMetaEntity = tableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
                var applicationResponse = await XmlUtil.GetApplicationResponseIfExist(documentMetaEntity);
                byte[] xmlBytesApplication = null;
                if (applicationResponse != null) xmlBytesApplication = applicationResponse;
                var responseApplication = new ResponseGetApplicationResponse { Success = true, Message = "OK" };
                if (xmlBytesApplication != null)
                {
                    responseApplication.Content = xmlBytesApplication;
                    responseApplication.ContentBase64 = Convert.ToBase64String(xmlBytesApplication);
                }

                string typeFormat = documentMetaEntity?.PrintFormatType;
                string[] sectionsToHide = (documentMetaEntity?.SectionsToHide ?? "").Split('|');

                // Diccionario para construir Pdf
                var dictionary = new Dictionary<string, string>
                {
                    {"documentTypeName", "Factura Electrónica"},
                    {"accountAvatar", null},
                    {"isMontoPeriodo", "0"},
                    {"showRefButton", "0"},
                };

                // Objeto que se Conecta al Storage 
                var fileManager = new FileManager();

                //var storageBiller = "DefaultEndpointsProtocol=https;AccountName=gtpabillerstoragetest;AccountKey=JAuJKjEWIsOU0e4VhpuY2VH5gk5CEfZyFp3xm3NffTZug7HzUNdJdX4DT+4/UIdE0qfrwG9asG730Zv6QBVFcA==";
                var fileManagerBiller = new FileManager("GlobalStorageBiller");


                // Transformar **XML** to **HTML**
                var htmlGDoc = new HtmlGDoc(xmlBytes, xmlBytesApplication);
                string Html_Content = htmlGDoc.GetHtmlGDoc(typeFormat, dictionary);

                //-------------------------------------------------------------------------------------------------------------------------
                Html_Content = ConfigurePrintTemplate(typeFormat, sectionsToHide, Html_Content);
                //-------------------------------------------------------------------------------------------------------------------------

                // Obtener en el Storage el Byte Array del **LOGO** a poner en el Documento - Convertir a Base 64 Image
                MemoryStream logoDianStream = new MemoryStream(fileManager.GetBytes("radian-dian-logos", "Logo-DIAN-2020-color.jpg"));
                string logoDianaStrBase64 = Convert.ToBase64String(logoDianStream.ToArray());
                var logoDianBase64 = $@"data:image/png;base64,{logoDianaStrBase64}";

                MemoryStream logoStream = new MemoryStream(fileManagerBiller.GetBytesBiller("logo", $"{documentMetaEntity.SenderCode}.jpg") ?? fileManager.GetBytes("radian-dian-logos", "Logo-DIAN-2020-color.jpg"));
                string logoStrBase64 = Convert.ToBase64String(logoStream.ToArray());
                var logoBase64 = $@"data:image/png;base64,{logoStrBase64}";


                // Obtener la Cadena para Construir el **CÓDIGO QR**
                var dataToEncode = htmlGDoc.GetQRNote();

                // Construir Objeto Bitmap para Código QR - Convertir a Base 64 Image
                var image = Utils.Utils.GetQRCode(dataToEncode);
                string qrStringBase64 = Utils.Utils.ConvertImageToBase64String(image, System.Drawing.Imaging.ImageFormat.Jpeg);
                var qrBase64 = $@"data:image/png;base64,{qrStringBase64}";

                //-------------------------------------------------------------------------------------------------------------------------

                // Sustuir en el HTML la ruta de LOGO y CÓDIGO QR para colocar imágenes
                Html_Content = Html_Content.Replace("#123logoDian", logoDianBase64);
                Html_Content = Html_Content.Replace("#123logo", logoBase64);
                Html_Content = Html_Content.Replace("#1qrcode", qrBase64);

                // Sustituir en el HTML la respuesta de la validación del documento y SigningTime
                var documentApplication = htmlGDoc.GetDocumentResponse();
                var documentSigningTime = htmlGDoc.GetSigningTime();
                //bug 14539


                if (documentApplication != null)
                {
                    documentApplication = DateNormalized(documentApplication, "Documento validado por la DIAN");
                    documentSigningTime = DateNormalized(documentSigningTime, "Documento generado el:", minusFiveHour: false);
                    Html_Content = Html_Content.Replace("#ApplicationResponse", documentApplication);
                    Html_Content = Html_Content.Replace("#SigningTime", documentSigningTime);
                }
                else
                {
                    Html_Content = Html_Content.Replace("#ApplicationResponse", "");
                    Html_Content = Html_Content.Replace("#SigningTime", "");
                }

                //-------------------------------------------------------------------------------------------------------------------------

                // Salvar HTML como fichero físico en PC
                // File.WriteAllText(@"D:\Users\wsuser41\Desktop\Dian\Documents\NUEVO.html", Html_Content);

                // Salvar PDF generado de HTML en el Storage
                var pdfBytes = await PdfCreator.Instance.PdfRenderAsync(Html_Content, trackId, getPaperSize(typeFormat));

                // Guardar en disco el pdf generado
                // File.WriteAllBytes(@"D:\Users\wsuser41\Desktop\Dian\Documents\NUEVO.pdf", pdfBytes);

                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new ByteArrayContent(pdfBytes);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                //result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/Binary");

                return result;
            }


            catch (System.Exception ex)
            {
                //return req.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.BadRequest);
                result.Content = new StringContent("No podemos generar el PDF en este momento debido al siguiente error: " + ex.Message);
                result.Content.Headers.ContentType =
                    new MediaTypeHeaderValue("text/plain");

                return result;
            }
        }

        static string DateNormalized(string currentDate, string dataReplace, bool minusFiveHour=true)
        {
            if (!currentDate.Contains(dataReplace))
                return currentDate;
            string[] dateparts = currentDate.Replace(dataReplace, "").Trim().Split('/');
            string[] hours = dateparts[2].Split(':');
            string[] years = hours[0].Split(' ');
            string[] rest = years[1].Split(':');
            int year = Convert.ToInt32(years[0]);
            int month = Convert.ToInt32(dateparts[1]);
            int day = Convert.ToInt32(dateparts[0]);
            int hour = Convert.ToInt32(rest[0]);
            int min = Convert.ToInt32(hours[1]);
            int sec = Convert.ToInt32(hours[2]);
            var newDate = new DateTime(year, month, day, hour, min, sec);
            if (minusFiveHour) { newDate = newDate.AddHours(-5); }
            return dataReplace + " " + newDate.ToString("yyyy-MM-dd HH:mm:ss");
            //return dataReplace + " " + (new DateTime(year, month, day, hour, min, sec).AddHours(-5)).ToString("dd/MM/yyyy HH:mm:ss");

        }

        private static string ConfigurePrintTemplate(string typeFormat, string[] sectionsToHide, string Html_Content)
        {
            if (typeFormat == "Formato tipo media carta")
            {
                Html_Content = AddStylesToTemplateForPrintHalfLetter(Html_Content);
            }
            if (typeFormat == "Formato tipo parametrizable")
            {
                Html_Content = ConfigureTemplateForVisualizeSelectedSections(sectionsToHide, Html_Content);
            }

            Html_Content = Html_Content.Replace("#stylesForHalfLetter#", "");
            Html_Content = Html_Content.Replace("#styleSectionDatosAdicionales#", "");
            Html_Content = Html_Content.Replace("#styleSectionDescuentosRecargosGlobales#", "");
            Html_Content = Html_Content.Replace("#styleSectionAnticipos#", "");
            Html_Content = Html_Content.Replace("#styleSectionReferencias#", "");
            Html_Content = Html_Content.Replace("#styleSectionInformacionComplementaria#", "");

            return Html_Content;
        }

        private static string ConfigureTemplateForVisualizeSelectedSections(string[] sectionsToHide, string Html_Content)
        {
            foreach (var section in sectionsToHide)
            {
                if (section.Equals("Datos adicionales"))
                {
                    Html_Content = Html_Content.Replace("#styleSectionDatosAdicionales#", "display:none");
                    continue;
                }
                if (section.Equals("Descuentos y recargos globales"))
                {
                    Html_Content = Html_Content.Replace("#styleSectionDescuentosRecargosGlobales#", "display:none");
                    continue;
                }
                if (section.Equals("Anticipos"))
                {
                    Html_Content = Html_Content.Replace("#styleSectionAnticipos#", "display:none");
                    continue;
                }
                if (section.Equals("Referencias"))
                {
                    Html_Content = Html_Content.Replace("#styleSectionReferencias#", "display:none");
                    continue;
                }
                if (section.Equals("Información complementaria"))
                {
                    Html_Content = Html_Content.Replace("#styleSectionInformacionComplementaria#", "display:none");
                }
            }

            return Html_Content;
        }

        private static string AddStylesToTemplateForPrintHalfLetter(string Html_Content)
        {
            string stylesMiddleLetter = @"
                .titleSecundario {
                    margin: 0 15px 0 !important;
                    font-size: 15px !important;
                }
                .divDetalle {
                    margin: 10px 30px 10px !important;
                }
                body {
                    font-size: 13px !important;
                }
                table thead tr:nth-child(1) td {
                    font-size: 18px !important;
                }
                table thead tr:nth-child(2) td {
                    font-size: 15px !important;
                }
                .divDetalleTotales table table:last-child td {
                    font-size: 11px !important;
                }
            ";
            Html_Content = Html_Content.Replace("#stylesForHalfLetter#", stylesMiddleLetter);
            return Html_Content;
        }

        private static PaperSize getPaperSize(string typeFormat)
        {
            switch (typeFormat)
            {
                case "Formato tipo carta":
                case "Formato tipo parametrizable":
                    return PaperSize.A4;
                case "Formato tipo media carta":
                    return new PaperSize(Length.Inches(5.5), Length.Inches(8.5));
                case "Formato tipo tirilla":
                    //return new PaperSize(Length.Millimeters(57), Length.Millimeters(110));
                default:
                    return PaperSize.A4;
            }
        }
    }
}
