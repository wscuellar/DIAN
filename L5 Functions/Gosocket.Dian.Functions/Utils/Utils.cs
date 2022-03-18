using Gosocket.Dian.Domain.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Utils.Helpers;
using Newtonsoft.Json;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Utils
{
    public class Utils
    {
        private static HttpClient client = new HttpClient();
        private static readonly TableManager TableManager = new TableManager("GlobalDocValidatorRuntime");

        public static HttpResponseMessage ConsumeApi(string url, dynamic requestObj)
        {
            
            var buffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestObj));
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return client.PostAsync(url, byteContent).Result;
            
        }

        public static async Task<HttpResponseMessage> ConsumeApiAsync<T>(string url, T requestObj)
        {
            
            var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestObj));
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return await client.PostAsync(url, byteContent);
            
        }

        public static ResponseDownloadXml DownloadXml<T>(T requestObj)
        {
            var response = ConsumeApi(ConfigurationManager.GetValue("DownloadXmlUrl"), requestObj);
            var result = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<ResponseDownloadXml>(result);
        }

        public static async Task <ResponseDownloadXml> DownloadXmlAsync<T>(T requestObj)
        {
            var response = await ConsumeApiAsync(ConfigurationManager.GetValue("DownloadXmlUrl"), requestObj);
            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ResponseDownloadXml>(result);
        }

        public static string ConvertImageToBase64String(Image image, System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();

                string base64String = Convert.ToBase64String(imageBytes);

                return base64String;
            }
        }

        public static Bitmap GetQRCode(string Content)
        {
            try
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(Content, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);

                //Bitmap qrCodeImage =  qrCode.GetGraphic(20);
                Bitmap qrCodeImage = new Bitmap(qrCode.GetGraphic(72), new Size(160, 160));

                return qrCodeImage;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static byte[] ConvertStreamToBytes(Stream stream)
        {
            byte[] buffer = new byte[16384];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        //public static List<GlobalDocValidatorTracking> GetValidationsByTrackId(string trackId)
        //{
        //    try
        //    {

        //        return ApiHelpers.ExecuteRequest<List<GlobalDocValidatorTracking>>(ConfigurationManager.GetValue("GetValidationsByTrackIdUrl"), new { trackId });

        //        //dynamic requestObj = new ExpandoObject();
        //        //requestObj.trackId = trackId;
        //        //var response = ConsumeApi(ConfigurationManager.GetValue("GetValidationsByTrackIdUrl"), requestObj);
        //        //var result = response.Content.ReadAsStringAsync().Result;
        //        //var validations = (List<GlobalDocValidatorTracking>)JsonConvert.DeserializeObject<List<GlobalDocValidatorTracking>>(result);
        //        //return validations;
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }
        //}

        public static async Task<byte[]> GetXmlFromStorageAsync(string trackId)
        {
            
            var documentStatusValidation = TableManager.Find<GlobalDocValidatorRuntime>(trackId, "UPLOAD");
            if (documentStatusValidation == null)
                return null;

            var fileManager = new FileManager();
            var container = $"global";
            var fileName = $"docvalidator/{documentStatusValidation.Category}/{documentStatusValidation.Timestamp.Date.Year}/{documentStatusValidation.Timestamp.Date.Month.ToString().PadLeft(2, '0')}/{trackId}.xml";
            var xmlBytes = await fileManager.GetBytesAsync(container, fileName);

            return xmlBytes;
        }
    }
}
