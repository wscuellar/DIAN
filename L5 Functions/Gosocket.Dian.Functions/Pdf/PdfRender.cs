namespace Gosocket.Dian.Functions.Pdf
{
    public static class PdfRender
    {
        //[FunctionName("PdfRender")]
        //public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        //{
        //    try
        //    {
        //        // Definir contenedor de parámetro
        //        string trackId = req.GetQueryNameValuePairs()
        //            .FirstOrDefault(q => string.Compare(q.Key, "trackId", true) == 0)
        //            .Value;

        //        // Obtener parámetros de consulta
        //        dynamic data = await req.Content.ReadAsAsync<object>();

        //        // Establecer nombre para consultar la cadena o los datos del cuerpo
        //        trackId = trackId ?? data?.trackId;

        //        // Descargar Bytes de XML a partir de TrackId
        //        dynamic requestObj = new ExpandoObject();
        //        requestObj.trackId = trackId;

        //        var responseXmlUrl = Utils.Utils.ConsumeApi(ConfigurationManager.GetValue("DownloadXmlUrl"), requestObj);
        //        var xmlBase64 = responseXmlUrl.Content.ReadAsStringAsync().Result.Replace("\"", string.Empty);
        //        var xmlBytes = Convert.FromBase64String(xmlBase64);


        //        // Diccionario para construir Pdf
        //        var dictionary = new Dictionary<string, string>
        //        {
        //            {"documentTypeName", "Factura Electrónica"},
        //            {"accountAvatar", null},
        //            {"isMontoPeriodo", "0"},
        //            {"showRefButton", "0"},
        //        };

        //        // Objeto que se Conecta al Storage 
        //        var fileManager = new FileManager(ConfigurationManager.GetValue("DianStorage"));


        //        // Transformar **XML** to **HTML**
        //        var htmlGDoc = new HtmlGDoc(xmlBytes);
        //        string Html_Content = htmlGDoc.GetHtmlGDoc(dictionary);

        //        //-------------------------------------------------------------------------------------------------------------------------

        //        // Obtener en el Storage el Byte Array del **LOGO** a poner en el Documento - Convertir a Base 64 Image
        //        MemoryStream logoStream = new MemoryStream(fileManager.GetBytes("render-xslt-pdfs", "logoTest.png"));
        //        string logoStrBase64 = Convert.ToBase64String(logoStream.ToArray());
        //        var logoBase64 = $@"data:image/png;base64,{logoStrBase64}";


        //        // Obtener la Cadena para Construir el **CÓDIGO QR**
        //        var dataToEncode = htmlGDoc.GetQRNote();

        //        // Construir Objeto Bitmap para Código QR - Convertir a Base 64 Image
        //        var image = Utils.Utils.GetQRCode(dataToEncode);
        //        string qrStringBase64 = Utils.Utils.ConvertImageToBase64String(image, System.Drawing.Imaging.ImageFormat.Jpeg);
        //        var qrBase64 = $@"data:image/png;base64,{qrStringBase64}";

        //        //-------------------------------------------------------------------------------------------------------------------------

        //        // Sustuir en el HTML la ruta de LOGO y CÓDIGO QR para colocar imágenes
        //        Html_Content = Html_Content.Replace("#123logo", logoBase64);
        //        Html_Content = Html_Content.Replace("#1qrcode", qrBase64);

        //        //-------------------------------------------------------------------------------------------------------------------------

        //        // Salvar HTML como fichero físico en PC
        //        // File.WriteAllText(@"D:\Users\wsuser41\Desktop\Dian\Documents\NUEVO.html", Html_Content);

        //        // Salvar PDF generado de HTML en el Storage
        //        var pdfBytes = PdfCreator.Instance.PdfRender(Html_Content, trackId);
        //        fileManager.Upload("render-xslt-pdfs", $"{trackId}.pdf", pdfBytes);

        //        // Respuesta de Function
        //        return req.CreateResponse(HttpStatusCode.OK, $"Pdf {trackId}.pdf se ha guardado exitosamente en Container 'render-xslt-pdfs'.");


        //    }
        //    catch (Exception ex)
        //    {
        //        //return req.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
        //        HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.NoContent);
        //        result.Content = new StringContent("No podemos generar el PDF en este momento debido al siguiente error: " + ex.Message);
        //        result.Content.Headers.ContentType =
        //            new MediaTypeHeaderValue("text/plain");

        //        return result;
        //    }
        //}
    }
}
