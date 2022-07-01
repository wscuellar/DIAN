using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.ServicesGroup;
using Gosocket.Dian.Services.Utils;
using Gosocket.Dian.Services.Utils.Common;
using Gosocket.Dian.Web.Services.Validator;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Gosocket.Dian.Logger.Logger;

namespace Gosocket.Dian.Web.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "WcfDianCustomerServices" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select WcfDianCustomerServices.svc or WcfDianCustomerServices.svc.cs at the Solution Explorer and start debugging.
    [ServiceBehavior(Namespace = "http://wcf.dian.colombia", InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class WcfDianCustomerServices : IWcfDianCustomerServices
    {
        private static readonly TableManager tableManagerGlobalLogger = new TableManager("GlobalLogger");
        private static readonly FileManager fileManager = new FileManager();
        private static readonly string blobContainer = "global";
        private static readonly string blobContainerFolder = "syncValidator";
        private static readonly string zipMimeType = "application/x-zip-compressed";

        /// <summary>
        /// Get contributors exchange email
        /// </summary>
        /// <returns></returns>
        public async Task<ExchangeEmailResponse> GetExchangeEmails()
        {
            string authCode = "";
            string email = "";
            try
            {
                authCode = GetAuthCode();
                email = GetAuthEmail();

                DianPAServices customerDianPa = new DianPAServices();
                {
                    var start = DateTime.UtcNow;
                    var result = await customerDianPa.GetExchangeEmails(authCode);

                    customerDianPa = null;
                    var end = DateTime.UtcNow.Subtract(start).TotalSeconds;
                    Log($"{authCode} {email}", (int)InsightsLogType.Info, "GetExchangeEmails " + end);

                    return result;
                    //return new ExchangeEmailResponse { StatusCode = "0", Success = true, Message = "OK", Bytes = result };
                }
            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>{
                    { "authCode",authCode },
                    { "email",email },
                    { "method", MethodBase.GetCurrentMethod().Name}
                    
                };
                LogException(ex, properties);
                var exception = new GlobalLogger($"GetExchangeEmails-{DateTime.UtcNow.ToString("yyyyMMdd")}", Guid.NewGuid().ToString()) { Action = $"GetExchangeEmails", Message = ex.Message, StackTrace = ex.StackTrace };
                tableManagerGlobalLogger.InsertOrUpdate(exception);
                return new ExchangeEmailResponse { StatusCode = "500", Success = false, Message = "Ha ocurrido un error. Por favor inténtentelo de nuevo.", CsvBase64Bytes = null };
            }
        }

        /// <summary>
        /// Get status of single document
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns></returns>
        public async Task<DianResponse> GetStatus(string trackId)
        {
            string authCode = "";
            string email = "";
            try
            {
                authCode = GetAuthCode();
                email = GetAuthEmail();

                var check = CheckTrackIdFormat(trackId, authCode, email);
                if (check.Any()) return check.FirstOrDefault();

                DianPAServices customerDianPa = new DianPAServices();
                {
                    var start = DateTime.UtcNow;

                    var exist = fileManager.Exists(blobContainer, $"{blobContainerFolder}/applicationResponses/{trackId.ToUpper()}.json");
                    if (exist)
                    {
                        var previusResult = await fileManager.GetTextAsync(blobContainer, $"{blobContainerFolder}/applicationResponses/{trackId.ToUpper()}.json");

                        if (!string.IsNullOrEmpty(previusResult))
                        {
                            DianResponse response = JsonConvert.DeserializeObject<DianResponse>(previusResult);
                            if (!response.IsValid || response.XmlBase64Bytes == null)
                                response = await customerDianPa.GetStatus(trackId.ToLower());
                            Log($"{authCode} {email}", (int)InsightsLogType.Info, "GetStatus(p) " + DateTime.UtcNow.Subtract(start).TotalSeconds);
                            customerDianPa = null;
                            return response;
                        }
                    }

                    var result = await customerDianPa.GetStatus(trackId.ToLower());
                    customerDianPa = null;

                    if (result.StatusCode == "66")
                    {
                        Log($"{authCode} {email} {DateTime.UtcNow.Subtract(start).TotalSeconds}", (int)InsightsLogType.Info, "GetStatus(ne) " + trackId);
                    }
                    else
                    {
                        Log($"{authCode} {email} {DateTime.UtcNow.Subtract(start).TotalSeconds}", (int)InsightsLogType.Info, "GetStatus(n) " + trackId);
                    }

                    return result;
                }

            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>{
                    { "authCode",authCode },
                    { "email",email },
                    { "method", MethodBase.GetCurrentMethod().Name},
                    { "trackId", trackId}
                };
                LogException(ex, properties);
                var exception = new GlobalLogger($"GetStatusException-{DateTime.UtcNow.ToString("yyyyMMdd")}", Guid.NewGuid().ToString()) { Action = $"GetStatus, trackId: {trackId}", Message = ex.Message, StackTrace = ex.StackTrace };
                tableManagerGlobalLogger.InsertOrUpdate(exception);
                return new DianResponse { StatusCode = "500", StatusDescription = $"Ha ocurrido un error. Por favor inténtentelo de nuevo.", IsValid = false };
            }
        }

        /// <summary>
        /// Get status of multiple documents
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns></returns>
        public async Task<List<DianResponse>> GetStatusZip(string trackId)
        {
            string authCode = "";
            string email = "";
            try
            {
                authCode = GetAuthCode();
                email = GetAuthEmail();

                var response = CheckTrackIdFormat(trackId, authCode, email, true);
                if (response.Any()) return response;

                DianPAServices customerDianPa = new DianPAServices();
                {
                    var start = DateTime.UtcNow;
                    var result = await customerDianPa.GetBatchStatus(trackId.ToLower());
                    customerDianPa = null;

                    Log($"{authCode} {email}", (int)InsightsLogType.Info, "GetStatusZip " + DateTime.UtcNow.Subtract(start).TotalSeconds);
                    return result;
                }
            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>{
                    { "authCode",authCode },
                    { "email",email },
                    { "method", MethodBase.GetCurrentMethod().Name},
                    { "trackId", trackId}
                };
                LogException(ex, properties);
                var exception = new GlobalLogger($"GetStatusZipException-{DateTime.UtcNow.ToString("yyyyMMdd")}", Guid.NewGuid().ToString()) { Action = $"GetStatusZip, trackId: {trackId}", Message = ex.Message, StackTrace = ex.StackTrace };
                tableManagerGlobalLogger.InsertOrUpdate(exception);
                var result = new DianResponse { StatusCode = "500", StatusDescription = $"Ha ocurrido un error. Por favor inténtentelo de nuevo.", IsValid = false };
                var response = new List<DianResponse> { result };
                return response;
            }

        }

        /// <summary>
        /// Get events of single document
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns></returns>
        public DianResponse GetStatusEvent(string trackId)
        {
            string email = "";
            string authCode = "";
            try
            {
                authCode = GetAuthCode();
                email = GetAuthEmail();

                var check = CheckTrackIdFormat(trackId, authCode, email);
                if (check.Any()) return check.FirstOrDefault();

                DianPAServices customerDianPa = new DianPAServices();
                {
                    var start = DateTime.UtcNow;
                    var result = customerDianPa.GetStatusEvent(trackId.ToLower());
                    customerDianPa = null;

                    if (result.StatusCode == "66")
                    {
                        Log($"{authCode} {email} {DateTime.UtcNow.Subtract(start).TotalSeconds}", (int)InsightsLogType.Info, "GetStatusEvent(ne) " + trackId);
                    }
                    else
                    {
                        Log($"{authCode} {email} {DateTime.UtcNow.Subtract(start).TotalSeconds}", (int)InsightsLogType.Info, "GetStatusEvent(n) " + trackId);
                    }

                    return result;
                }

            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>{
                    { "authCode",authCode },
                    { "email",email },
                    { "method", MethodBase.GetCurrentMethod().Name},
                    { "trackId", trackId}
                };
                LogException(ex, properties);
                var exception = new GlobalLogger($"GetStatusEventException-{DateTime.UtcNow.ToString("yyyyMMdd")}", Guid.NewGuid().ToString()) { Action = $"GetStatusEvent, trackId: {trackId}", Message = ex.Message, StackTrace = ex.StackTrace };
                tableManagerGlobalLogger.InsertOrUpdate(exception);
                return new DianResponse { StatusCode = "500", StatusDescription = $"Ha ocurrido un error. Por favor inténtentelo de nuevo.", IsValid = false };
            }
        }

        /// <summary>
        /// Process multiple docuements
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="contentFile"></param>
        /// <returns></returns>
        public async Task<UploadDocumentResponse> SendBillAsync(string fileName, byte[] contentFile)
        {
            string email = "";
            string authCode = "";

            try
            {
                authCode = GetAuthCode();
                email = GetAuthEmail();

                if (string.IsNullOrEmpty(authCode))
                {
                    var response = new XmlParamsResponseTrackId { XmlFileName = fileName, ProcessedMessage = "NIT de la empresa no informado en el certificado." };
                    return new UploadDocumentResponse { ErrorMessageList = new List<XmlParamsResponseTrackId>() { response } };
                }

                if (contentFile == null)
                {
                    Log($"{authCode} {email} SendBillAsync", (int)InsightsLogType.Error, "Archivo no enviado.");
                    var response = new XmlParamsResponseTrackId { XmlFileName = fileName, ProcessedMessage = "Archivo no enviado." };
                    return new UploadDocumentResponse { ErrorMessageList = new List<XmlParamsResponseTrackId>() { response } };
                }

                var mimeType = GetMimeFromBytes(contentFile);
                if (mimeType != zipMimeType)
                {
                    Log($"{authCode} {email} SendBillAsync", (int)InsightsLogType.Error, $"MIMEType del archivo inválido ({mimeType}).");
                    var response = new XmlParamsResponseTrackId { XmlFileName = fileName, ProcessedMessage = $"MIMEType del archivo inválido ({mimeType})." };
                    return new UploadDocumentResponse { ErrorMessageList = new List<XmlParamsResponseTrackId>() { response } };
                }

                if (!contentFile.ZipContainsXmlFiles())
                {
                    Log($"{authCode} {email} SendBillAsync", (int)InsightsLogType.Error, $"Archivo ZIP no contiene XML's.");
                    var response = new XmlParamsResponseTrackId { XmlFileName = fileName, ProcessedMessage = $"Error descomprimiendo el archivo ZIP: No fue encontrado ningun documento XML valido." };
                    return new UploadDocumentResponse { ErrorMessageList = new List<XmlParamsResponseTrackId>() { response } };
                }

                DianPAServices customerDianPa = new DianPAServices();
                {
                    Log($"{authCode} {email}", (int)InsightsLogType.Info, "SendBillAsync");
                    var start = DateTime.UtcNow;
                    var result = await customerDianPa.ProcessBatchZipFile(fileName, contentFile, authCode);
                    customerDianPa = null;
                    Log($"{authCode} {email}", (int)InsightsLogType.Info, "SendBillAsync " + DateTime.UtcNow.Subtract(start).TotalSeconds);
                    return result;
                }
            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>{
                    { "authCode",authCode },
                    { "email",email },
                    { "method", MethodBase.GetCurrentMethod().Name},
                    { "fileName", fileName }
                };
                LogException(ex, properties);
                var exception = new GlobalLogger($"SendBillAsyncException-{DateTime.UtcNow.ToString("yyyyMMdd")}", Guid.NewGuid().ToString()) { Action = $"SendBillAsync, fileName: {fileName}", Message = ex.Message, StackTrace = ex.StackTrace };
                tableManagerGlobalLogger.InsertOrUpdate(exception);
                var response = new XmlParamsResponseTrackId { XmlFileName = fileName, ProcessedMessage = $"Ha ocurrido un error. Por favor inténtentelo nuevamente.", Success = false };
                return new UploadDocumentResponse { ErrorMessageList = new List<XmlParamsResponseTrackId>() { response } };
            }
        }

        /// <summary>
        /// Process multiple documents for test set
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="contentFile"></param>
        /// <param name="testSetId"></param>
        /// <returns></returns>
        public async Task<UploadDocumentResponse> SendTestSetAsync(string fileName, byte[] contentFile, string testSetId)
        {
            string email = "";
            string authCode = "";

            try
            {
                if (ConfigurationManager.GetValue("Environment") == "Prod")
                {
                    var response = new XmlParamsResponseTrackId { XmlFileName = fileName, ProcessedMessage = "Acción no disponible en producción." };
                    return new UploadDocumentResponse { ErrorMessageList = new List<XmlParamsResponseTrackId>() { response } };
                }

                if (string.IsNullOrEmpty(testSetId) || string.IsNullOrWhiteSpace(testSetId))
                {
                    var response = new XmlParamsResponseTrackId { XmlFileName = fileName, ProcessedMessage = "Identificador del set de pruebas es obligatorio." };
                    return new UploadDocumentResponse { ErrorMessageList = new List<XmlParamsResponseTrackId>() { response } };
                }
                
                testSetId = testSetId.Trim();
                email = GetAuthEmail();
                authCode = GetAuthCode();

                if (string.IsNullOrEmpty(authCode))
                {
                    var response = new XmlParamsResponseTrackId { XmlFileName = fileName, ProcessedMessage = "NIT de la empresa no informado en el certificado." };
                    return new UploadDocumentResponse { ErrorMessageList = new List<XmlParamsResponseTrackId>() { response } };
                }

                if (contentFile == null)
                {
                    Log($"{authCode} {email} SendTestSetAsync", (int)InsightsLogType.Error, "Archivo no enviado.");
                    var response = new XmlParamsResponseTrackId { XmlFileName = fileName, ProcessedMessage = "Archivo no enviado." };
                    return new UploadDocumentResponse { ErrorMessageList = new List<XmlParamsResponseTrackId>() { response } };
                }

                var mimeType = GetMimeFromBytes(contentFile);
                if (mimeType != zipMimeType)
                {
                    Log($"{authCode} {email} SendTestSetAsync", (int)InsightsLogType.Error, $"MIMEType del archivo inválido ({mimeType}).");
                    var response = new XmlParamsResponseTrackId { XmlFileName = fileName, ProcessedMessage = $"MIMEType del archivo inválido ({mimeType})." };
                    return new UploadDocumentResponse { ErrorMessageList = new List<XmlParamsResponseTrackId>() { response } };
                }

                if (!contentFile.ZipContainsXmlFiles())
                {
                    Log($"{authCode} {email} SendTestSetAsync", (int)InsightsLogType.Error, $"Archivo ZIP no contiene XML's.");
                    var response = new XmlParamsResponseTrackId { XmlFileName = fileName, ProcessedMessage = $"Error descomprimiendo el archivo ZIP: No fue encontrado ningun documento XML valido." };
                    return new UploadDocumentResponse { ErrorMessageList = new List<XmlParamsResponseTrackId>() { response } };
                }

                DianPAServices customerDianPa = new DianPAServices();
                {
                    var start = DateTime.UtcNow;
                    //var result = customerDianPa.UploadMultipleDocumentAsync(fileName, contentFile, authCode, testSetId);
                    var result = await customerDianPa.ProcessBatchZipFile(fileName, contentFile, authCode, testSetId);
                    customerDianPa = null;
                    Log($"{authCode} {email}", (int)InsightsLogType.Info, "SendTestSetAsync " + DateTime.UtcNow.Subtract(start).TotalSeconds);
                    return result;
                }

            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>{
                    { "authCode",authCode },
                    { "email",email },
                    { "method", MethodBase.GetCurrentMethod().Name},
                    { "fileName", fileName }
                };
                LogException(ex, properties);
                var exception = new GlobalLogger($"SendTestSetAsyncException-{DateTime.UtcNow.ToString("yyyyMMdd")}", Guid.NewGuid().ToString()) { Action = $"SendTestSetAsync, fileName: {fileName}", Message = ex.Message, StackTrace = ex.StackTrace };
                tableManagerGlobalLogger.InsertOrUpdate(exception);
                var response = new XmlParamsResponseTrackId { XmlFileName = fileName, ProcessedMessage = $"Ha ocurrido un error. Por favor inténtentelo nuevamente.", Success = false };
                return new UploadDocumentResponse { ErrorMessageList = new List<XmlParamsResponseTrackId>() { response } };
            }
        }

        /// <summary>
        /// Document reception
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="contentFile"></param>
        /// <returns></returns>
        public async Task<DianResponse> SendBillSync(string fileName, byte[] contentFile)
        {
            string authCode = "";
            string email = "";
            try
            {
                authCode = GetAuthCode();
                email = GetAuthEmail();

                if (string.IsNullOrEmpty(authCode))
                    return new DianResponse { StatusCode = "89", StatusDescription = "NIT de la empresa no informado en el certificado." };

                if (contentFile == null)
                {
                    Log($"{authCode} {email} SendBillSync", (int)InsightsLogType.Error, "Archivo no enviado.");
                    return new DianResponse { StatusCode = "89", StatusDescription = "Archivo no enviado." };
                }

                var mimeType = GetMimeFromBytes(contentFile);
                if (mimeType != zipMimeType)
                {
                    Log($"{authCode} {email} SendBillSync", (int)InsightsLogType.Error, $"MIMEType del archivo inválido ({mimeType}).");
                    return new DianResponse { StatusCode = "89", StatusDescription = $"MIMEType del archivo inválido ({mimeType})." };
                }

                if (!contentFile.ZipContainsXmlFiles())
                {
                    Log($"{authCode} {email} SendBillSync", (int)InsightsLogType.Error, $"Archivo ZIP no contiene XML's.");
                    return new DianResponse { StatusCode = "89", StatusDescription = $"Error descomprimiendo el archivo ZIP: No fue encontrado ningun documento XML válido." };
                }

                DianPAServices customerDianPa = new DianPAServices();
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    var result = await customerDianPa.UploadDocumentSync(fileName, contentFile, authCode);

                    var exist = fileManager.Exists(blobContainer, $"{blobContainerFolder}/applicationResponses/{result?.XmlDocumentKey?.ToUpper()}.json");
                    if (!exist && result.IsValid && result.XmlBase64Bytes != null)
                        await fileManager.UploadAsync(blobContainer, $"{blobContainerFolder}/applicationResponses/{result.XmlDocumentKey.ToUpper()}.json", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result)));

                    customerDianPa = null;

                    stopwatch.Stop();
                    double ms = stopwatch.ElapsedMilliseconds;
                    double seconds = ms / 1000;
                    stopwatch = null;
                    Log($"{authCode} {email}", (int)InsightsLogType.Info, "SendBillSync " + seconds);
                    if (seconds >= 10)
                    {
                        var logger = new GlobalLogger($"MORETHAN10SECONDS-{DateTime.UtcNow.ToString("yyyyMMdd")}", result.XmlDocumentKey) { Message = seconds.ToString(), Action = "SendBillSync" };
                        tableManagerGlobalLogger.InsertOrUpdate(logger);
                    }

                    //Logged if response do not have AR
                    if (result?.XmlBase64Bytes == null)
                    {
                        var logger = new GlobalLogger($"RESPONSEWITHOUTAR-{DateTime.UtcNow.ToString("yyyyMMdd")}", result.XmlDocumentKey) { Message = "Response without AR", Action = "SendBillSync" };
                        tableManagerGlobalLogger.InsertOrUpdate(logger);
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>{
                    { "authCode",authCode },
                    { "email",email },
                    { "method", MethodBase.GetCurrentMethod().Name},
                    { "fileName", fileName }
                };
                LogException(ex, properties);
                var exception = new GlobalLogger($"SendBillSyncException-{DateTime.UtcNow.ToString("yyyyMMdd")}", Guid.NewGuid().ToString()) { Action = $"SendBillSync, fileName: {fileName}", Message = ex.Message, StackTrace = ex.StackTrace };
                tableManagerGlobalLogger.InsertOrUpdate(exception);
                return new DianResponse { StatusCode = "500", StatusDescription = $"Ha ocurrido un error. Por favor inténtentelo de nuevo.", XmlFileName = fileName, IsValid = false };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="contentFile"></param>
        /// <returns></returns>
        public UploadDocumentResponse SendBillAttachmentAsync(string fileName, byte[] contentFile)
        {
            try
            {
                var response = new XmlParamsResponseTrackId { XmlFileName = fileName, ProcessedMessage = "Método no disponible." };
                return new UploadDocumentResponse { ErrorMessageList = new List<XmlParamsResponseTrackId>() { response } };

                //var authCode = GetAuthCode();
                //var email = GetAuthEmail();

                //if (string.IsNullOrEmpty(authCode))
                //{
                //    var response = new XmlParamsResponseTrackId { XmlFileName = fileName, ProcessedMessage = "NIT de la empresa no informado en el certificado." };
                //    return new UploadDocumentResponse { ErrorMessageList = new List<XmlParamsResponseTrackId>() { response } };
                //}

                //if (contentFile == null)
                //{
                //    Log(fileName, (int)InsightsLogType.Error, "Archivo no enviado.");
                //    var response = new XmlParamsResponseTrackId { XmlFileName = fileName, ProcessedMessage = "Archivo no enviado." };
                //    return new UploadDocumentResponse { ErrorMessageList = new List<XmlParamsResponseTrackId>() { response } };
                //}

                //var mimeType = GetMimeFromBytes(contentFile);
                //if (mimeType != zipMimeType)
                //{
                //    Log($"{authCode} {email} SendBillAttachmentAsync", (int)InsightsLogType.Error, $"MIMEType del archivo inválido ({mimeType}).");
                //    var response = new XmlParamsResponseTrackId { XmlFileName = fileName, ProcessedMessage = $"MIMEType del archivo inválido ({mimeType})." };
                //    return new UploadDocumentResponse { ErrorMessageList = new List<XmlParamsResponseTrackId>() { response } };
                //}

                //if (!contentFile.ZipContainsXmlFiles())
                //{
                //    Log($"{authCode} {email} SendBillAttachmentAsync", (int)InsightsLogType.Error, $"Archivo ZIP no contiene XML's.");
                //    var response = new XmlParamsResponseTrackId { XmlFileName = fileName, ProcessedMessage = $"Error descomprimiendo el archivo ZIP: No fue encontrado ningun documento XML valido." };
                //    return new UploadDocumentResponse { ErrorMessageList = new List<XmlParamsResponseTrackId>() { response } };
                //}

                //DianPAServices customerDianPa = new DianPAServices();
                //{
                //    Log($"{authCode} {email}", (int)InsightsLogType.Info, "SendBillAttachmentAsync");
                //    var result = customerDianPa.UploadDocumentAttachmentAsync(fileName, contentFile, authCode);
                //    customerDianPa = null;
                //    return result;
                //}
            }
            catch (Exception ex)
            {
                Log(fileName, (int)InsightsLogType.Error, ex.Message);
                var exception = new GlobalLogger("SendBillAttachmentAsyncException", Guid.NewGuid().ToString()) { Action = $"SendBillAttachmentAsync, fileName: {fileName}", Message = ex.Message, StackTrace = ex.StackTrace };
                tableManagerGlobalLogger.InsertOrUpdate(exception);
                var response = new XmlParamsResponseTrackId { XmlFileName = fileName, ProcessedMessage = $"Ha ocurrido un error. Por favor inténtentelo nuevamente.", Success = false };
                return new UploadDocumentResponse { ErrorMessageList = new List<XmlParamsResponseTrackId>() { response } };
            }
        }

        /// <summary>
        /// Process document event
        /// </summary>
        /// <param name="contentFile"></param>
        /// <returns></returns>
        public async Task<DianResponse> SendEventUpdateStatus(byte[] contentFile)
        {
            string email = "";
            string authCode = "";
            try
            {
                authCode = GetAuthCode();
                email = GetAuthEmail();

                if (contentFile == null)
                {
                    Log($"{authCode} {email} SendEventUpdateStatus", (int)InsightsLogType.Error, "Archivo no enviado.");
                    return new DianResponse { StatusCode = "89", StatusDescription = "Archivo no enviado." };
                }

                var mimeType = GetMimeFromBytes(contentFile);
                if (mimeType != zipMimeType)
                {
                    Log($"{authCode} {email} SendEventUpdateStatus", (int)InsightsLogType.Error, $"MIMEType del archivo inválido ({mimeType}).");
                    return new DianResponse { StatusCode = "89", StatusDescription = $"MIMEType del archivo inválido ({mimeType})." };
                }

                if (!contentFile.ZipContainsXmlFiles())
                {
                    Log($"{authCode} {email} SendEventUpdateStatus", (int)InsightsLogType.Error, $"Archivo ZIP no contiene XML's.");
                    return new DianResponse { StatusCode = "89", StatusDescription = $"Error descomprimiendo el archivo ZIP: No fue encontrado ningun documento XML válido." };
                }

                DianPAServices customerDianPa = new DianPAServices();
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    var result = await customerDianPa.SendEventUpdateStatus(contentFile, authCode);

                    var exist = fileManager.Exists(blobContainer, $"{blobContainerFolder}/applicationResponses/{result?.XmlDocumentKey?.ToUpper()}.json");
                    if (!exist && result.IsValid && result.XmlBase64Bytes != null)
                        await fileManager.UploadAsync(blobContainer, $"{blobContainerFolder}/applicationResponses/{result.XmlDocumentKey.ToUpper()}.json", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result)));

                    customerDianPa = null;

                    stopwatch.Stop();
                    double ms = stopwatch.ElapsedMilliseconds;
                    double seconds = ms / 1000;
                    stopwatch = null;
                    Log($"{authCode} {email}", (int)InsightsLogType.Info, "SendEventUpdateStatus " + seconds);
                    if (seconds >= 10)
                    {
                        var logger = new GlobalLogger($"MORETHAN10SECONDS-{DateTime.UtcNow.ToString("yyyyMMdd")}", result.XmlDocumentKey) { Message = seconds.ToString(), Action = "SendEventUpdateStatus" };
                        tableManagerGlobalLogger.InsertOrUpdate(logger);
                    }

                    //Logged if response do not have AR
                    if (result?.XmlBase64Bytes == null)
                    {
                        var logger = new GlobalLogger($"RESPONSEWITHOUTAR-{DateTime.UtcNow.ToString("yyyyMMdd")}", result.XmlDocumentKey) { Message = "Response without AR", Action = "SendEventUpdateStatus" };
                        tableManagerGlobalLogger.InsertOrUpdate(logger);
                    }

                    return result;

                }
            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>{
                    { "authCode",authCode },
                    { "email",email },
                    { "method", MethodBase.GetCurrentMethod().Name}
                };
                LogException(ex, properties);
                var exception = new GlobalLogger($"SendEventUpdateStatusException-{DateTime.UtcNow.ToString("yyyyMMdd")}", Guid.NewGuid().ToString())
                { Action = $"SendEventUpdateStatus", Message = ex.Message, StackTrace = ex.StackTrace };
                tableManagerGlobalLogger.InsertOrUpdate(exception);

                return new DianResponse
                {
                    StatusCode = "500",
                    StatusDescription = $"Ha ocurrido un error en la estrucutra del documento XML.",
                    XmlFileName = "SendEventUpdateStatus",
                    IsValid = false,
                    StatusMessage = "Documento XML ApplicationResponse: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Process document event
        /// </summary>
        /// <param name="contentFile"></param>
        /// <returns></returns>
        public async Task<DianResponse> SendNominaSync(byte[] contentFile)
        {
            string email = "";
            string authCode = "";
            try
            {
                authCode = GetAuthCode();
                email = GetAuthEmail();

                if (contentFile == null)
                {
                    Log($"{authCode} {email} SendEventUpdateStatus", (int)InsightsLogType.Error, "Archivo no enviado.");
                    return new DianResponse { StatusCode = "89", StatusDescription = "Archivo no enviado." };
                }

                var mimeType = GetMimeFromBytes(contentFile);
                if (mimeType != zipMimeType)
                {
                    Log($"{authCode} {email} SendEventUpdateStatus", (int)InsightsLogType.Error, $"MIMEType del archivo inválido ({mimeType}).");
                    return new DianResponse { StatusCode = "89", StatusDescription = $"MIMEType del archivo inválido ({mimeType})." };
                }

                if (!contentFile.ZipContainsXmlFiles())
                {
                    Log($"{authCode} {email} SendEventUpdateStatus", (int)InsightsLogType.Error, $"Archivo ZIP no contiene XML's.");
                    return new DianResponse { StatusCode = "89", StatusDescription = $"Error descomprimiendo el archivo ZIP: No fue encontrado ningun documento XML válido." };
                }

                LogicalNMService customerNomina = new LogicalNMService();
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    var result = await customerNomina.SendNominaUpdateStatusAsync(contentFile, authCode);

                    var exist = fileManager.Exists(blobContainer, $"{blobContainerFolder}/applicationResponses/{result?.XmlDocumentKey?.ToUpper()}.json");
                    if (!exist && result.IsValid && result.XmlBase64Bytes != null)
                        await fileManager.UploadAsync(blobContainer, $"{blobContainerFolder}/applicationResponses/{result.XmlDocumentKey.ToUpper()}.json", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result)));

                    customerNomina = null;

                    stopwatch.Stop();
                    double ms = stopwatch.ElapsedMilliseconds;
                    double seconds = ms / 1000;
                    stopwatch = null;
                    Log($"{authCode} {email}", (int)InsightsLogType.Info, "SendEventUpdateStatus " + seconds);
                    if (seconds >= 10)
                    {
                        var logger = new GlobalLogger($"MORETHAN10SECONDS-{DateTime.UtcNow.ToString("yyyyMMdd")}", result?.XmlDocumentKey) { Message = seconds.ToString(), Action = "SendEventUpdateStatus" };
                        tableManagerGlobalLogger.InsertOrUpdate(logger);
                    }

                    //Logged if response do not have AR
                    if (result?.XmlBase64Bytes == null)
                    {
                        var logger = new GlobalLogger($"RESPONSEWITHOUTAR-{DateTime.UtcNow.ToString("yyyyMMdd")}", result?.XmlDocumentKey) { Message = "Response without AR", Action = "SendEventUpdateStatus" };
                        tableManagerGlobalLogger.InsertOrUpdate(logger);
                    }

                    return result;

                }
            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>{
                    { "authCode",authCode },
                    { "email",email },
                    { "method", MethodBase.GetCurrentMethod().Name},
                };
                LogException(ex, properties);
                var exception = new GlobalLogger($"SendNominaSyncStatusException-{DateTime.UtcNow.ToString("yyyyMMdd")}", Guid.NewGuid().ToString())
                { Action = $"SendNominaSync", Message = ex.Message, StackTrace = ex.StackTrace };
                tableManagerGlobalLogger.InsertOrUpdate(exception);

                return new DianResponse
                {
                    StatusCode = "500",
                    StatusDescription = $"Ha ocurrido un error. Por favor inténtentelo de nuevo.",
                    XmlFileName = "SendEventNominaStatus",
                    IsValid = false,
                    StatusMessage = "Documento XML ApplicationResponse"
                };
            }
        }
        /// <summary>
        /// Get number ranges
        /// </summary>
        /// <param name="accountCode"></param>
        /// <param name="accountCodeT"></param>
        /// <param name="softwareCode"></param>
        /// <returns></returns>
        public NumberRangeResponseList GetNumberingRange(string accountCode, string accountCodeT, string softwareCode)
        {
            string email = "";
            string authCode = "";
            try
            {
                authCode = GetAuthCode();
                if (string.IsNullOrEmpty(authCode))
                    return new NumberRangeResponseList { OperationCode = "89", OperationDescription = "NIT de la empresa no informado en el certificado." };

                DianPAServices customerDianPa = new DianPAServices();
                {
                    email = GetAuthEmail();
                    Log($"{authCode} {email}", (int)InsightsLogType.Info, "GetNumberingRange");
                    var result = customerDianPa.GetNumberingRange(accountCode, accountCodeT, softwareCode, authCode);
                    customerDianPa = null;
                    return result;
                }
            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>{
                    { "authCode",authCode },
                    { "email",email },
                    { "method", MethodBase.GetCurrentMethod().Name},
                };
                LogException(ex, properties);
                
                var exception = new GlobalLogger($"GetNumberingRangeException-{DateTime.UtcNow.ToString("yyyyMMdd")}", Guid.NewGuid().ToString()) { Action = $"GetNumberingRange, accountCode: {accountCode}, accountCodeT: {accountCodeT}, softwareCode: {softwareCode}", Message = ex.Message, StackTrace = ex.StackTrace };
                tableManagerGlobalLogger.InsertOrUpdate(exception);
                throw new FaultException(ex.Message, new FaultCode("Client"));
            }
        }

        /// <summary>
        /// Get xml by trackId (CUFE)
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns></returns>
        public async Task<EventResponse> GetXmlByDocumentKey(string trackId)
        {
            string email = "";
            string authCode = "";
            try
            {
                authCode = GetAuthCode();
                email = GetAuthEmail();

                var check = CheckTrackIdFormat(trackId, authCode, email);
                if (check.Any()) return new EventResponse { Code = check.FirstOrDefault().StatusCode, Message = check.FirstOrDefault().StatusDescription };

                DianPAServices customerDianPa = new DianPAServices();
                {
                    Log($"{authCode} {email}", (int)InsightsLogType.Info, "GetXmlByDocumentKey");
                    var result = await customerDianPa.GetXmlByDocumentKey(trackId, GetAuthCode());
                    customerDianPa = null;
                    return result;
                }
            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>{
                    { "authCode",authCode },
                    { "email",email },
                    { "method", MethodBase.GetCurrentMethod().Name},
                    { "trackId", trackId}
                };
                LogException(ex, properties);
                var exception = new GlobalLogger($"GetXmlByDocumentKeyException-{DateTime.UtcNow.ToString("yyyyMMdd")}", Guid.NewGuid().ToString()) { Action = $"GetXmlByDocumentKey", Message = ex.Message, StackTrace = ex.StackTrace };
                tableManagerGlobalLogger.InsertOrUpdate(exception);
                return new EventResponse { Code = "500", Message = $"Ha ocurrido un error. Por favor inténtentelo de nuevo." };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contributorCode"></param>
        /// <param name="dateNumber"></param>
        /// <returns></returns>
        public DocIdentifierWithEventsResponse GetDocIdentifierWithEvents(string contributorCode, string dateNumber)
        {
            string authCode = "";
            string email = "";
            try
            {
                authCode = GetAuthCode();
                email = GetAuthEmail();

                if(string.IsNullOrWhiteSpace(contributorCode))
                {
                    Log($"{authCode} {email} GetDocIdentifierWithEvents", (int)InsightsLogType.Error, "ContributorCode no enviado.");
                    return new DocIdentifierWithEventsResponse { StatusCode = "89", Success = false, Message = "ContributorCode no enviado.", CsvBase64Bytes = null };
                }

                DianPAServices customerDianPa = new DianPAServices();
                {
                    var start = DateTime.UtcNow;
                    var result = customerDianPa.GetDocIdentifierWithEvents(authCode, contributorCode, dateNumber);

                    customerDianPa = null;
                    var end = DateTime.UtcNow.Subtract(start).TotalSeconds;
                    Log($"{authCode} {email}", (int)InsightsLogType.Info, "GetDocIdentifierWithEvents " + end);

                    return result;
                }
            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>{
                    { "authCode",authCode },
                    { "email",email },
                    { "method", MethodBase.GetCurrentMethod().Name}
                };
                LogException(ex,properties);
                var exception = new GlobalLogger($"GetDocIdentifierWithEvents-{DateTime.UtcNow.ToString("yyyyMMdd")}", Guid.NewGuid().ToString()) { Action = $"GetDocIdentifierWithEvents", Message = ex.Message, StackTrace = ex.StackTrace };
                tableManagerGlobalLogger.InsertOrUpdate(exception);
                return new DocIdentifierWithEventsResponse { StatusCode = "500", Success = false, Message = "Ha ocurrido un error. Por favor inténtentelo de nuevo.", CsvBase64Bytes = null };
            }
        }

        /// <summary>
        /// Check trackId format
        /// </summary>
        /// <param name="trackId"></param>
        /// <param name="authCode"></param>
        /// <param name="email"></param>
        /// <param name="isGuid"></param>
        /// <returns></returns>
        private List<DianResponse> CheckTrackIdFormat(string trackId, string authCode, string email, bool isGuid = false)
        {
            var responses = new List<DianResponse>();
            if (isGuid)
            {
                try
                {
                    Guid.Parse(trackId);
                }
                catch
                {
                    Log($"{authCode} {email}, trackId: '{trackId}' GetStatusZip", (int)InsightsLogType.Error, "Formato trackId inválido.");
                    responses.Add(new DianResponse { StatusCode = "89", StatusDescription = $"Formato trackId inválido." });
                }
            }

            if (string.IsNullOrEmpty(trackId))
            {
                Log($"{authCode} {email} GetStatus", (int)InsightsLogType.Error, $"TrackId es nulo o vacío.");
                responses.Add(new DianResponse { StatusCode = "89", StatusDescription = "TrackId es nulo o vacío." });
            }
            else if (trackId.Length < 20)
            {
                Log($"{authCode} {email}, trackId: '{trackId}'", (int)InsightsLogType.Error, $"TrackId inválido (Length).");
                responses.Add(new DianResponse { StatusCode = "89", StatusDescription = "TrackId inválido (Length)." });
            }

            return responses;
        }

        /// <summary>
        /// Get nit from claims
        /// </summary>
        /// <returns></returns>
        private string GetAuthCode()
        {
            var principal = ClaimsPrincipal.Current;
            var authCode = principal.AuthCode();
            return authCode;
        }

        /// <summary>
        /// Get email from claims
        /// </summary>
        /// <returns></returns>
        private string GetAuthEmail()
        {
            var principal = ClaimsPrincipal.Current;
            var authEmail = principal.AuthEmail();
            return authEmail;
        }

        /// <summary>
        /// Get mime type from data
        /// </summary>
        /// <param name="pBC"></param>
        /// <param name="pwzUrl"></param>
        /// <param name="pBuffer"></param>
        /// <param name="cbSize"></param>
        /// <param name="pwzMimeProposed"></param>
        /// <param name="dwMimeFlags"></param>
        /// <param name="ppwzMimeOut"></param>
        /// <param name="dwReserved"></param>
        /// <returns></returns>
        [DllImport("urlmon.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
        static extern int FindMimeFromData(IntPtr pBC,
            [MarshalAs(UnmanagedType.LPWStr)] string pwzUrl,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.I1, SizeParamIndex=3)]
            byte[] pBuffer,
            int cbSize,
            [MarshalAs(UnmanagedType.LPWStr)] string pwzMimeProposed,
            int dwMimeFlags,
            out IntPtr ppwzMimeOut,
            int dwReserved);

        /// <summary>
        /// Get bytes mime type
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string GetMimeFromBytes(byte[] data)
        {
            try
            {
                string mime = string.Empty;
                IntPtr suggestPtr = IntPtr.Zero, filePtr = IntPtr.Zero, outPtr = IntPtr.Zero;
                int ret = FindMimeFromData(IntPtr.Zero, null, data, data.Length, null, 0, out outPtr, 0);
                if (ret == 0 && outPtr != IntPtr.Zero)
                {
                    //todo: this leaks memory outPtr must be freed
                    return Marshal.PtrToStringUni(outPtr);
                }
                return mime;
            }
            catch
            {
                return "";
            }
        }


        /// <summary>
        /// Generate report of Send or Received Documents
        /// </summary>
        /// <param name="nit" type="string"></param>
        /// <param name="startDate" type="DateTime"></param>
        /// <param name="endDate" type="DateTime"></param>
        /// <param name="documentGroup" type="string"></param>
        /// <returns></returns>
        public DianResponse BulkDocumentDownloadAsync(string nit, string startDate, string endDate, string documentGroup)
        {
            string[] allowedDocumentGroups = new string[] { "Todos", "Emitido", "Recibido" };
            
            try
            {    
                DateTime dateStart, dateEnd;

                var authCode = GetAuthCode();
                var email = GetAuthEmail();

                if (string.IsNullOrEmpty(authCode))
                    return new DianResponse { StatusCode = "89", StatusDescription = "NIT de la empresa no informado en el certificado." };

                DateTime.TryParse(startDate, out dateStart);
                DateTime.TryParse(endDate, out dateEnd);

                /*validación de todos los campos deben ser obligatorios*/
                if(dateStart.Date <= DateTime.MinValue.Date && 
                    dateEnd.Date <= DateTime.MinValue.Date && 
                    string.IsNullOrWhiteSpace(documentGroup) &&
                    string.IsNullOrWhiteSpace(nit))
                {
                    Log($"{authCode} {email} BulkDocumentDownloadAsync", (int)InsightsLogType.Error, "No se recibieron parámetros de consulta.");
                    return new DianResponse { StatusCode = "89", StatusDescription = "No se recibieron parámetros de consulta." };
                }

                /*2022-05-31 Agregar el campo nit como obligatorio*/
                if (string.IsNullOrWhiteSpace(nit))
                {
                    Log($"{authCode} {email} BulkDocumentDownloadAsync", (int)InsightsLogType.Error, "Para realizar la solicitud de descarga masiva de documentos, el nit es obligatorio.");
                    return new DianResponse { StatusCode = "89", StatusDescription = "Para realizar la solicitud de descarga masiva de documentos, el nit es obligatorio." };
                }

                #region Validaciones de tipo de dato y/o formato
                if (!Regex.IsMatch(startDate, @"^[0-9/-]+$")) /*fecha inicial solo contener numeros con guion o slash*/
                {
                    Log($"{authCode} {email} BulkDocumentDownloadAsync", (int)InsightsLogType.Error, "El parámetro de consulta 'fecha inicial' NO cumple con el tipo de dato y/o formato.");
                    return new DianResponse { StatusCode = "89", StatusDescription = "El parámetro de consulta 'fecha inicial' NO cumple con el tipo de dato y/o formato." };
                }

                if (!Regex.IsMatch(endDate, @"^[0-9/-]+$"))
                {
                    Log($"{authCode} {email} BulkDocumentDownloadAsync", (int)InsightsLogType.Error, "El parámetro de consulta 'fecha final' NO cumple con el tipo de dato y/o formato.");
                    return new DianResponse { StatusCode = "89", StatusDescription = "El parámetro de consulta 'fecha final' NO cumple con el tipo de dato y/o formato." };
                }

                if (string.IsNullOrWhiteSpace(documentGroup))
                {
                    documentGroup = "Todos";
                }

                if (!allowedDocumentGroups.Any(t => t.ToLower() == documentGroup.ToLower()))
                {
                    Log($"{authCode} {email} BulkDocumentDownloadAsync", (int)InsightsLogType.Error, $"El grupo de documento solicitado ({documentGroup}) es inválido.");
                    return new DianResponse { StatusCode = "89", StatusDescription = $"El grupo de documento solicitado ({ documentGroup }) es inválido." };
                }
                #endregion

                if (dateStart.Date <= DateTime.MinValue.Date)
                {
                    Log($"{authCode} {email} BulkDocumentDownloadAsync", (int)InsightsLogType.Error, "La fecha inicio es obligatoria.");
                    return new DianResponse { StatusCode = "89", StatusDescription = "La fecha inicio es obligatoria." };
                }

                if (dateEnd.Date <= DateTime.MinValue.Date)
                {
                    Log($"{authCode} {email} BulkDocumentDownloadAsync", (int)InsightsLogType.Error, "La fecha final es obligatoria.");
                    return new DianResponse { StatusCode = "89", StatusDescription = "La fecha final es obligatoria." };
                }

                if (dateEnd.Date < dateStart.Date)
                {
                    Log($"{authCode} {email} BulkDocumentDownloadAsync", (int)InsightsLogType.Error, "La fecha final debe ser mayor o igual a la fecha inicial.");
                    return new DianResponse { StatusCode = "89", StatusDescription = "La fecha final debe ser mayor o igual a la fecha inicial." };
                }

                int maxQuantyDays = Convert.ToInt32(ConfigurationManager.GetValue("BulkDocumentsDownload_QuantyMaxDaysFoDateRange"));
                if ((dateEnd.Date - dateStart.Date).TotalDays > maxQuantyDays)
                {
                    Log($"{authCode} {email} BulkDocumentDownloadAsync", (int)InsightsLogType.Error, "El rango de fechas especificado para esta solicitud, supera el rango máximo de fechas permitido (3 meses).");
                    return new DianResponse { StatusCode = "89", StatusDescription = "El rango de fechas especificado para esta solicitud, supera el rango máximo de fechas permitido (3 meses)." };
                }

                /*agregar validacion de que la fecha inicial no debe ser mayor a 3 meses a partir de hoy*/
                if ((DateTime.Now.Date - dateStart.Date).TotalDays > maxQuantyDays)
                {
                    Log($"{authCode} {email} BulkDocumentDownloadAsync", (int)InsightsLogType.Error, "la fecha de inicio supera el rango máximo de fechas permitido (3 meses).");
                    return new DianResponse { StatusCode = "89", StatusDescription = "la fecha de inicio supera el rango máximo de fechas permitido (3 meses)." };
                }

                DianPAServices customerDianPa = new DianPAServices();

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                var result = customerDianPa.SendRequestBulkDocumentsDownload(authCode, email, nit, dateStart, dateEnd, documentGroup);

                stopwatch.Stop();
                double ms = stopwatch.ElapsedMilliseconds;
                double seconds = ms / 1000;

                stopwatch.Reset();
                Log($"{authCode} {email}", (int)InsightsLogType.Info, "BulkDocumentDownloadAsync " + seconds);
                if (seconds >= 10)
                {
                    var logger = new GlobalLogger($"MORETHAN10SECONDS-{DateTime.UtcNow.ToString("yyyyMMdd")}", result.XmlDocumentKey) { Message = seconds.ToString(), Action = "BulkDocumentDownloadAsync" };
                    tableManagerGlobalLogger.InsertOrUpdate(logger);
                }

                return result;
            }
            catch (Exception ex)
            {
                Log($"{nit} BulkDocumentDownloadAsync", (int)InsightsLogType.Error, ex.Message);
                var exception = new GlobalLogger($"BulkDocumentDownloadAsyncException-{DateTime.UtcNow.ToString("yyyyMMdd")}", Guid.NewGuid().ToString()) { Action = $"BulkDocumentDownloadAsync, Nit: {nit}, StartDate: {startDate:dd/MM/yyyy}, EndDate: {endDate:dd/MM/yyyy}, DocumentGroup: {documentGroup}", Message = ex.Message, StackTrace = ex.StackTrace };
                tableManagerGlobalLogger.InsertOrUpdate(exception);
                return new DianResponse { StatusCode = "500", StatusDescription = $"Ha ocurrido un error. Por favor inténtentelo de nuevo.", IsValid = false };
            }
        }

        /// <summary>
        /// Consult the status of report of Send or Received Documents
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns></returns>
        public DianResponseBulkDocumentDownload GetStatusBulkDocumentDownload(string trackId)
        {
            try
            {
                var authCode = GetAuthCode();
                var email = GetAuthEmail();

                if (string.IsNullOrEmpty(authCode))
                    return new DianResponseBulkDocumentDownload { StatusCode = "89", StatusDescription = "NIT de la empresa no informado en el certificado." };

                if (string.IsNullOrWhiteSpace(trackId))
                {
                    Log($"{authCode} {email} GetStatusBulkDocumentDownload", (int)InsightsLogType.Error, "El trackId de la solciitud es obligatorio.");
                    return new DianResponseBulkDocumentDownload { StatusCode = "89", StatusDescription = "El trackId de la solciitud es obligatorio." };
                }

                DianPAServices customerDianPa = new DianPAServices();

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                var result = customerDianPa.GetStatusBulkDocumentsDownload(trackId);

                stopwatch.Stop();
                double ms = stopwatch.ElapsedMilliseconds;
                double seconds = ms / 1000;

                stopwatch.Reset();
                Log($"{authCode} {email}", (int)InsightsLogType.Info, "GetStatusBulkDocumentDownload " + seconds);
                if (seconds >= 10)
                {
                    var logger = new GlobalLogger($"MORETHAN10SECONDS-{DateTime.UtcNow.ToString("yyyyMMdd")}", result.UrlDescarga) { Message = seconds.ToString(), Action = "BulkDocumentDownloadAsync" };
                    tableManagerGlobalLogger.InsertOrUpdate(logger);
                }

                return result;
            }
            catch (Exception ex)
            {
                Log($"{trackId} GetStatusBulkDocumentDownload", (int)InsightsLogType.Error, ex.Message);
                var exception = new GlobalLogger($"GetStatusBulkDocumentDownloadException-{DateTime.UtcNow.ToString("yyyyMMdd")}", Guid.NewGuid().ToString()) { Action = $"GetStatusBulkDocumentDownload, trackId: {trackId}", Message = ex.Message, StackTrace = ex.StackTrace };
                tableManagerGlobalLogger.InsertOrUpdate(exception);
                return new DianResponseBulkDocumentDownload { StatusCode = "500", StatusDescription = $"Ha ocurrido un error. Por favor inténtentelo de nuevo.", IsValid = false };
            }
        }
    }
}