using Gosocket.Dian.Domain.Domain;
using Gosocket.Dian.Functions.Utils;
using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
                var response = Utils.Utils.DownloadXml(requestObj);
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

                // Diccionario para construir Pdf
                var dictionary = new Dictionary<string, string>
                {
                    {"documentTypeName", "Factura Electrónica"},
                    {"accountAvatar", null},
                    {"isMontoPeriodo", "0"},
                    {"showRefButton", "0"},
                };

                // Objeto que se Conecta al Storage 
                var fileManager = new FileManager(ConfigurationManager.GetValue("GlobalStorage"));


                // Transformar **XML** to **HTML**
                var htmlGDoc = new HtmlGDoc(xmlBytes, xmlBytesApplication);
                string Html_Content = htmlGDoc.GetHtmlGDoc(dictionary);

                //-------------------------------------------------------------------------------------------------------------------------

                // Obtener en el Storage el Byte Array del **LOGO** a poner en el Documento - Convertir a Base 64 Image
                MemoryStream logoStream = new MemoryStream(fileManager.GetBytes("dian", "images/logoTest.png"));
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
                Html_Content = Html_Content.Replace("#123logo", logoBase64);
                Html_Content = Html_Content.Replace("#1qrcode", qrBase64);

                // Sustituir en el HTML la respuesta de la validación del documento y SigningTime
                var documentApplication = htmlGDoc.GetDocumentResponse();
                var documentSigningTime = htmlGDoc.GetSigningTime();
                if (documentApplication != null)
                {
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
                var pdfBytes = PdfCreator.Instance.PdfRender(Html_Content, trackId);

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
    }
}
