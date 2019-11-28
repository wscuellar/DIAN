using Gosocket.Dian.Application;
using Gosocket.Dian.Application.Cosmos;
using Gosocket.Dian.Domain.Cosmos;
using Gosocket.Dian.Domain.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Infrastructure.Utils;
using Gosocket.Dian.Services.Utils.Helpers;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Web.Filters;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using Microsoft.Azure.EventGrid.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    [IPFilter]
    [Authorization]
    public class DocumentController : Controller
    {
        readonly GlobalDocumentService globalDocumentService = new GlobalDocumentService();
        private readonly TableManager documentMetaTableManager = new TableManager("GlobalDocValidatorDocumentMeta");
        private readonly TableManager globalDocValidatorDocumentTableManager = new TableManager("GlobalDocValidatorDocument");
        private readonly TableManager globalTaskTableManager = new TableManager("GlobalTask");

        private List<DocValidatorTrackingModel> GetValidatedRules(string trackId)
        {
            var requestObj = new { trackId };
            var validations = GetValidations(requestObj);

            return validations.Select(d => new DocValidatorTrackingModel
            {
                ErrorMessage = d.ErrorMessage,
                IsValid = d.IsValid,
                IsNotification = d.IsNotification,
                Mandatory = d.Mandatory,
                Name = d.RuleName,
                Priority = d.Priority,
                Status = d.Status
            }).Where(d => d.IsNotification).OrderBy(d => d.Status).ToList();
        }

        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public async Task<ActionResult> Index(SearchDocumentViewModel model)
        {
            return await GetDocuments(model, 1);
        }

        public async Task<ActionResult> Sent(SearchDocumentViewModel model)
        {
            return await GetDocuments(model, 2);
        }

        public async Task<ActionResult> Received(SearchDocumentViewModel model)
        {
            return await GetDocuments(model, 3);
        }

        [CustomRoleAuthorization(CustomRoles = "Proveedor")]
        public async Task<ActionResult> Provider(SearchDocumentViewModel model)
        {
            return await GetDocuments(model, 4);
        }

        public async Task<ActionResult> Details(string trackId)
        {
            var validations = GetValidatedRules(trackId);

            var globalDocValidatorDocumentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
            var emissionDateNumber = globalDocValidatorDocumentMeta.EmissionDate.ToString("yyyyMMdd");
            var partitionKey = $"co|{emissionDateNumber.Substring(6, 2)}|{globalDocValidatorDocumentMeta.DocumentKey.Substring(0, 2)}";

            var date = DateNumberToDateTime(emissionDateNumber);

            var globalDataDocument = await CosmosDBService.Instance(date).ReadDocumentAsync(globalDocValidatorDocumentMeta.DocumentKey, partitionKey, date);

            var document = new DocumentViewModel
            {
                Amount = globalDataDocument.FreeAmount,
                DocumentTypeId = globalDataDocument.DocumentTypeId,
                DocumentTypeName = globalDataDocument.DocumentTypeName,
                GenerationDate = globalDataDocument.GenerationTimeStamp,
                Id = globalDataDocument.DocumentKey,
                EmissionDate = globalDataDocument.EmissionDate,
                Number = globalDataDocument.Number,
                TechProviderName = globalDataDocument?.TechProviderInfo?.TechProviderName,
                TechProviderCode = globalDataDocument?.TechProviderInfo?.TechProviderCode,
                ReceiverName = globalDataDocument.ReceiverName,
                ReceiverCode = globalDataDocument.ReceiverCode,
                ReceptionDate = globalDataDocument.ReceptionTimeStamp,
                SenderName = globalDataDocument.SenderName,
                SenderCode = globalDataDocument.SenderCode,
                Status = globalDataDocument.ValidationResultInfo.Status,
                StatusName = globalDataDocument.ValidationResultInfo.StatusName,
                TaxAmountIva = globalDataDocument.TaxAmountIva,
                TotalAmount = globalDataDocument.TotalAmount
            };

            document.TaxesDetail.TaxAmountIva5Percent = globalDataDocument.TaxesDetail?.TaxAmountIva5Percent ?? 0;
            document.TaxesDetail.TaxAmountIva14Percent = globalDataDocument.TaxesDetail?.TaxAmountIva14Percent ?? 0;
            document.TaxesDetail.TaxAmountIva16Percent = globalDataDocument.TaxesDetail?.TaxAmountIva16Percent ?? 0;
            document.TaxesDetail.TaxAmountIva19Percent = globalDataDocument.TaxesDetail?.TaxAmountIva19Percent ?? 0;
            document.TaxesDetail.TaxAmountIva = globalDataDocument.TaxesDetail?.TaxAmountIva ?? 0;
            document.TaxesDetail.TaxAmountIca = globalDataDocument.TaxesDetail?.TaxAmountIca ?? 0;
            document.TaxesDetail.TaxAmountIpc = globalDataDocument.TaxesDetail?.TaxAmountIpc ?? 0;

            document.DocumentTags = globalDataDocument.DocumentTags.Select(t => new DocumentTagViewModel()
            {
                Code = t.Value,
                Description = t.Description,
                Value = t.Value,
                TimeStamp = t.TimeStamp
            }).ToList();

            document.Events = globalDataDocument.Events.Select(e => new EventViewModel()
            {
                Code = e.Code,
                Date = e.Date,
                DateNumber = e.DateNumber,
                Description = e.Description,
                ReceiverCode = e.ReceiverCode,
                ReceiverName = e.ReceiverName,
                SenderCode = e.SenderCode,
                SenderName = e.SenderName,
                TimeStamp = e.TimeStamp
            }).ToList();

            document.References = globalDataDocument.References.Select(r => new ReferenceViewModel()
            {
                DocumentKey = r.DocumentKey,
                DocumentTypeId = r.DocumentTypeId,
                DocumenTypeName = r.DocumenTypeName,
                Date = r.Date,
                DateNumber = r.DateNumber,
                Description = r.Description,
                ReceiverCode = r.ReceiverCode,
                ReceiverName = r.ReceiverName,
                SenderCode = r.SenderCode,
                SenderName = r.SenderName,
                TimeStamp = r.TimeStamp,
                ShowAsReference = true
            }).ToList();

            var model = new DocValidatorModel
            {
                Document = document,
                Validations = validations
            };


            ViewBag.CurrentPage = Navigation.NavigationEnum.DocumentDetails;
            return View(model);
        }

        public ActionResult Viewer(Navigation.NavigationEnum nav)
        {
            ViewBag.CurrentPage = nav;
            ViewBag.HasActions = true;
            return View();
        }

        public ActionResult DownloadAttachments(Guid? documentId)
        {
            try
            {
                string file = HostingEnvironment.MapPath("~/Content/resources/Adjuntos_980005140.zip");

                return File(file, "application/zip",
                "Adjuntos_980005140.zip");
            }
            catch (Exception)
            {
            }

            return RedirectToAction(nameof(Viewer));

        }

        public ActionResult DownloadExportedZipFile(string pk, string rk)
        {
            try
            {
                var bytes = DownloadExportedFile(pk, rk);
                var zipFile = ZipExtensions.CreateZip(bytes, rk, "xlsx");
                return File(zipFile, "application/zip", $"{rk}.zip");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return File(new byte[1], "application/zip", $"error");
            }
        }

        public ActionResult DownloadZipFiles(string trackId)
        {
            try
            {
                string url = ConfigurationManager.GetValue("GetPdfUrl");
                var requestObj = new { trackId };
                HttpResponseMessage responseMessage = ConsumeApi(url, requestObj);

                var pdfbytes = responseMessage.Content.ReadAsByteArrayAsync().Result;
                var xmlBytes = DownloadXml(trackId);

                var zipFile = ZipExtensions.CreateMultipleZip(new List<Tuple<string, byte[]>>
                {
                    new Tuple<string, byte[]>(trackId + ".pdf", pdfbytes),
                    xmlBytes != null ? new Tuple<string, byte[]>(trackId + ".xml", xmlBytes) : null
                }, trackId);

                return File(zipFile, "application/zip", $"{trackId}.zip");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return File(new byte[1], "application/zip", $"error");
            }
        }

        [ExcludeFilter(typeof(Authorization))]
        public ActionResult DownloadPDF(string trackId, string recaptchaToken)
        {
            try
            {
                IsValidCaptcha(recaptchaToken);
                string url = ConfigurationManager.GetValue("GetPdfUrl");

                var requestObj = new { trackId };
                HttpResponseMessage responseMessage = ConsumeApi(url, requestObj);

                var pdfbytes = responseMessage.Content.ReadAsByteArrayAsync().Result;

                return File(pdfbytes, "application/pdf", $"{trackId}.pdf");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return File(new byte[1], "application/zip", $"error");
            }


        }

        public ActionResult Export()
        {
            var model = new ExportDocumentTableViewModel();

            GetExportDocumentTasks(ref model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Export(ExportDocumentTableViewModel model)
        {
            await CreateGlobalTask(model);

            return RedirectToAction(nameof(Export));
        }

        [ExcludeFilter(typeof(Authorization))]
        public async Task<ActionResult> FindDocument(string documentKey, string partitionKey, string emissionDate)
        {
            var date = DateNumberToDateTime(emissionDate);
            var globalDataDocument = await CosmosDBService.Instance(date).ReadDocumentAsync(documentKey, partitionKey, date);

            if (globalDataDocument == null)
            {
                var searchViewModel = new SearchViewModel();
                ModelState.AddModelError("DocumentKey", "Documento no encontrado en los registros de la DIAN.");
                return View("Search", searchViewModel);
            }

            var model = new DocValidatorModel();
            model.Validations.AddRange(GetValidatedRules(documentKey));

            model.Document = new DocumentViewModel
            {
                Amount = globalDataDocument.FreeAmount,
                DocumentTypeId = globalDataDocument.DocumentTypeId,
                DocumentTypeName = globalDataDocument.DocumentTypeName,
                GenerationDate = globalDataDocument.GenerationTimeStamp,
                Id = globalDataDocument.DocumentKey,
                EmissionDate = globalDataDocument.EmissionDate,
                Number = globalDataDocument.Number,
                TechProviderName = globalDataDocument?.TechProviderInfo?.TechProviderName,
                TechProviderCode = globalDataDocument?.TechProviderInfo?.TechProviderCode,
                ReceiverName = globalDataDocument.ReceiverName,
                ReceiverCode = globalDataDocument.ReceiverCode,
                ReceptionDate = globalDataDocument.ReceptionTimeStamp,
                SenderName = globalDataDocument.SenderName,
                SenderCode = globalDataDocument.SenderCode,
                Status = globalDataDocument.ValidationResultInfo.Status,
                StatusName = globalDataDocument.ValidationResultInfo.StatusName,
                TaxAmountIva = globalDataDocument.TaxAmountIva,
                TotalAmount = globalDataDocument.TotalAmount
            };

            model.Document.Events = globalDataDocument.Events.Select(e => new EventViewModel
            {
                Code = e.Code,
                Date = e.Date,
                DateNumber = e.DateNumber,
                Description = e.Description,
                ReceiverCode = e.ReceiverCode,
                ReceiverName = e.ReceiverName,
                SenderCode = e.SenderCode,
                SenderName = e.SenderName,
                TimeStamp = e.TimeStamp
            }).ToList();

            model.Document.References = globalDataDocument.References.Select(r => new ReferenceViewModel
            {
                DocumentKey = r.DocumentKey,
                DocumentTypeId = r.DocumentTypeId,
                DocumenTypeName = r.DocumenTypeName,
                Date = r.Date,
                DateNumber = r.DateNumber,
                Description = r.Description,
                ReceiverCode = r.ReceiverCode,
                ReceiverName = r.ReceiverName,
                SenderCode = r.SenderCode,
                SenderName = r.SenderName,
                TimeStamp = r.TimeStamp,
                ShowAsReference = true
            }).ToList();

            //Se debe evaluar sustituir la lista de referencia a la factura por campos de nota de credito y debito
            TableManager tableManagerGlobalDocReference = new TableManager("GlobalDocReference");
            if (model.Document.DocumentTypeId == "1" || model.Document.DocumentTypeId == "01")
            {
                List<GlobalDocReference> globalDocReferences = tableManagerGlobalDocReference.FindByPartition<GlobalDocReference>(model.Document.Id).Where(x => x.RowKey != "INVOICE").ToList();

                model.Document.References.AddRange(globalDocReferences.Select(r => new ReferenceViewModel
                {
                    DocumentKey = r.DocumentKey,
                    DocumenTypeName = r.DocumentTypeName,
                    DateNumber = r.DateNumber,
                    TimeStamp = r.Timestamp.Date,
                    ShowAsReference = false
                }).ToList());
            }

            return View(model);
        }

        [ExcludeFilter(typeof(Authorization))]
        public ActionResult Search()
        {
            return View();
        }

        [HttpPost]
        [ExcludeFilter(typeof(Authorization))]
        public ActionResult Search(SearchViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var globalDocValidatorDocumentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(model.DocumentKey, model.DocumentKey);
            if (globalDocValidatorDocumentMeta == null)
            {
                ModelState.AddModelError("DocumentKey", "Documento no encontrado en los registros de la DIAN.");
                return View(model);
            }

            var identifier = $"{globalDocValidatorDocumentMeta.SenderCode}{globalDocValidatorDocumentMeta.DocumentTypeId}{globalDocValidatorDocumentMeta.SerieAndNumber}".EncryptSHA256();
            var globalDocValidatorDocument = globalDocValidatorDocumentTableManager.Find<GlobalDocValidatorDocument>(identifier, identifier);
            if (globalDocValidatorDocument == null)
            {
                ModelState.AddModelError("DocumentKey", "Documento no encontrado en los registros de la DIAN.");
                return View(model);
            }

            var partitionKey = $"co|{globalDocValidatorDocument.EmissionDateNumber.Substring(6, 2)}|{globalDocValidatorDocument.DocumentKey.Substring(0, 2)}";

            return RedirectToAction(nameof(FindDocument), new { documentKey = globalDocValidatorDocument.DocumentKey, partitionKey, emissionDate = globalDocValidatorDocument.EmissionDateNumber });
        }

        [ExcludeFilter(typeof(Authorization))]
        public ActionResult SearchQR(string documentKey)
        {
            var globalDocValidatorDocumentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(documentKey, documentKey);
            if (globalDocValidatorDocumentMeta == null)
                return RedirectToAction(nameof(SearchInvalidQR));

            var identifier = $"{globalDocValidatorDocumentMeta.SenderCode}{globalDocValidatorDocumentMeta.DocumentTypeId}{globalDocValidatorDocumentMeta.SerieAndNumber}".EncryptSHA256();
            var globalDocValidatorDocument = globalDocValidatorDocumentTableManager.Find<GlobalDocValidatorDocument>(identifier, identifier);
            if (globalDocValidatorDocument == null)
                return RedirectToAction(nameof(SearchInvalidQR));

            var partitionKey = $"co|{globalDocValidatorDocument.EmissionDateNumber.Substring(6, 2)}|{globalDocValidatorDocument.DocumentKey.Substring(0, 2)}";

            return RedirectToAction(nameof(FindDocument), new { documentKey = globalDocValidatorDocument.DocumentKey, partitionKey, emissionDate = globalDocValidatorDocument.EmissionDateNumber });
        }

        [ExcludeFilter(typeof(Authorization))]
        public ActionResult SearchInvalidQR()
        {
            return View();
        }

        #region Private methods

        private bool IsValidCaptcha(string token)
        {

            var secret = ConfigurationManager.GetValue("RecaptchaServer");
            var req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(ConfigurationManager.GetValue("RecaptchaUrl") + "?secret=" + secret + "&response=" + token);

            using (var wResponse = req.GetResponse())
            {

                using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                {
                    string responseFromServer = readStream.ReadToEnd();
                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseFromServer);
                    if (jsonResponse.success.ToObject<bool>() && jsonResponse.score.ToObject<float>() > 0.4)
                        return true;
                    else if (jsonResponse["error-codes"].ToObject<List<string>>().Contains("timeout-or-duplicate"))
                        return false;
                    else
                        throw new Exception(jsonResponse.ToString());
                }
            }


        }

        private DateTime DateNumberToDateTime(string date)
        {
            return DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture);
        }

        private string DateNumberToString(string date)
        {
            return DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("dd-MM-yyyy");
        }

        private byte[] DownloadExportedFile(string pk, string rk)
        {
            FileManager fileManager = new FileManager();
            return fileManager.GetBytes("global", $"export/{pk}/{rk}.xlsx");
        }

        private byte[] DownloadXml(string trackId)
        {
            string url = ConfigurationManager.GetValue("DownloadXmlUrl");
            dynamic requestObj = new { trackId };
            var response = DownloadXml(requestObj);
            if (response.Success)
            {
                byte[] xmlBytes = Convert.FromBase64String(response.XmlBase64);
                return xmlBytes;
            }
            throw new Exception(response.Message);
        }

        public static ResponseDownloadXml DownloadXml<T>(T requestObj)
        {
            return ApiHelpers.ExecuteRequest<ResponseDownloadXml>(ConfigurationManager.GetValue("DownloadXmlUrl"), requestObj);
        }

        public static List<GlobalDocValidatorTracking> GetValidations<T>(T requestObj)
        {
            return ApiHelpers.ExecuteRequest<List<GlobalDocValidatorTracking>>(ConfigurationManager.GetValue("GetValidationsByTrackIdUrl"), requestObj);
        }

        private static HttpResponseMessage ConsumeApi(string url, dynamic requestObj)
        {
            using (var client = new HttpClient())
            {
                var buffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestObj));
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return client.PostAsync(url, byteContent).Result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="filterType"></param>
        /// <returns></returns>
        private async Task<ActionResult> GetDocuments(SearchDocumentViewModel model, int filterType)
        {
            string continuationToken = (string)Session["Continuation_Token_" + model.Page];

            SetView(filterType);

            List<string> pks = null;
            if (!string.IsNullOrEmpty(model.DocumentKey))
            {
                var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(model.DocumentKey, model.DocumentKey);
                var globalDocValidatorDocument = globalDocValidatorDocumentTableManager.Find<GlobalDocValidatorDocument>(documentMeta?.Identifier, documentMeta?.Identifier);
                if (globalDocValidatorDocument == null)
                    return View("Index", model);

                pks = new List<string> { $"co|{globalDocValidatorDocument.EmissionDateNumber.Substring(6, 2)}|{model.DocumentKey.Substring(0, 2)}" };
            }

            Tuple<bool, string, List<GlobalDataDocument>> result = new Tuple<bool, string, List<GlobalDataDocument>>(false, null, null);

            switch (filterType)
            {
                case 1:
                    result = await CosmosDBService.Instance(model.EndDate).ReadDocumentsAsync(continuationToken, model.StartDate, model.EndDate, model.Status, model.DocumentTypeId, model.SenderCode, model.ReceiverCode, null, model.MaxItemCount, model.DocumentKey, model.ReferencesType, pks);
                    break;
                case 2:
                    result = await CosmosDBService.Instance(model.EndDate).ReadDocumentsAsync(continuationToken, model.StartDate, model.EndDate, model.Status, model.DocumentTypeId, User.ContributorCode(), model.ReceiverCode, null, model.MaxItemCount, model.DocumentKey, model.ReferencesType, pks);
                    break;
                case 3:
                    result = await CosmosDBService.Instance(model.EndDate).ReadDocumentsAsync(continuationToken, model.StartDate, model.EndDate, model.Status, model.DocumentTypeId, model.SenderCode, User.ContributorCode(), null, model.MaxItemCount, model.DocumentKey, model.ReferencesType, pks);
                    break;
                case 4:
                    result = await CosmosDBService.Instance(model.EndDate).ReadDocumentsAsync(continuationToken, model.StartDate, model.EndDate, model.Status, model.DocumentTypeId, model.SenderCode, model.ReceiverCode, User.ContributorCode(), model.MaxItemCount, model.DocumentKey, model.ReferencesType, pks);
                    break;
                default:
                    break;
            }

            if (result.Item3 != null && result.Item3.Count > 0)
            {
                model.Documents = result.Item3.Select(d => new DocumentViewModel
                {
                    PartitionKey = d.PartitionKey,
                    Amount = d.FreeAmount,
                    DocumentTypeId = d.DocumentTypeId,
                    DocumentTypeName = d.DocumentTypeName,
                    GenerationDate = d.GenerationTimeStamp,
                    Id = d.DocumentKey,
                    EmissionDate = d.EmissionDate,
                    Number = d.Number,
                    Serie = d.Serie,
                    SerieAndNumber = d.SerieAndNumber,
                    TechProviderName = d?.TechProviderInfo?.TechProviderName,
                    TechProviderCode = d?.TechProviderInfo?.TechProviderCode,
                    ReceiverName = d.ReceiverName,
                    ReceiverCode = d.ReceiverCode,
                    ReceptionDate = d.ReceptionTimeStamp,
                    SenderName = d.SenderName,
                    SenderCode = d.SenderCode,
                    Status = d.ValidationResultInfo.Status,
                    StatusName = d.ValidationResultInfo.StatusName,
                    TaxAmountIva = d.TaxAmountIva,
                    TotalAmount = d.TotalAmount,
                }).ToList();
            }

            model.IsNextPage = result.Item1;
            string test = "Continuation_Token_" + (model.Page + 1);
            this.Session["Continuation_Token_" + (model.Page + 1)] = result.Item2;

            return View("Index", model);
        }

        private void SetView(int filterType)
        {
            switch (filterType)
            {
                case 1:
                    ViewBag.CurrentPage = Navigation.NavigationEnum.DocumentList;
                    ViewBag.ViewType = "Index";
                    break;
                case 2:
                    ViewBag.CurrentPage = Navigation.NavigationEnum.DocumentSent;
                    ViewBag.ViewType = "Sent";
                    break;
                case 3:
                    ViewBag.CurrentPage = Navigation.NavigationEnum.DocumentReceived;
                    ViewBag.ViewType = "Received";
                    break;
                case 4:
                    ViewBag.CurrentPage = Navigation.NavigationEnum.DocumentProvider;
                    ViewBag.ViewType = "Provider";
                    break;
                default:
                    break;
            }
        }
        private void GetExportDocumentTasks(ref ExportDocumentTableViewModel model)
        {
            string pk = "ADMIN";
            if (!User.IsInAnyRole("Administrador", "Super")) pk = User.ContributorCode();

            var tasks = globalTaskTableManager.FindByPartition<GlobalTask>(pk);

            model.Tasks = tasks.Select(t => new ExportDocumentViewModel
            {
                PartitionKey = t.PartitionKey,
                RowKey = t.RowKey,
                Date = t.Date,
                User = t.User,
                Type = t.Type,
                TypeDescription = EnumHelper.GetEnumDescription((Domain.Common.ExportType)t.Type),
                Status = t.Status,
                StatusDescription = EnumHelper.GetEnumDescription((Domain.Common.ExportStatus)t.Status),
                FilterDate = t.FilterDate,
                FilterGroup = t.FilterGroup,
                TotalResult = t.TotalResult
            }).ToList();
        }
        private async Task CreateGlobalTask(ExportDocumentTableViewModel model)
        {
            string pk = "ADMIN";
            if (!User.IsInAnyRole("Administrador", "Super"))
            {
                model.SenderCode = User.ContributorCode();
                pk = model.SenderCode;
            };

            var globalTask = new GlobalTask(pk, Guid.NewGuid().ToString())
            {
                Date = DateTime.UtcNow,
                User = User.Identity.Name,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                SenderCode = model.SenderCode,
                ReceiverCode = model.ReceiverCode,
                Status = (int)Domain.Common.ExportStatus.InProcess,
                Type = model.Type,
                FilterGroupCode = model.GroupCode,
                FilterDate = $"Desde {model.StartDate.ToString("dd-MM-yyyy")} Hasta {model.EndDate.ToString("dd-MM-yyyy")}"
            };

            switch (model.GroupCode)
            {
                case "0":
                    globalTask.FilterGroup = "Emitidos y Recibidos";
                    break;
                case "1":
                    globalTask.FilterGroup = "Emitidos";
                    break;
                case "2":
                    globalTask.FilterGroup = "Recibidos";
                    break;
                default:
                    break;
            }

            globalTaskTableManager.InsertOrUpdate(globalTask);
            await SentTask(globalTask);
        }
        private async Task SentTask(GlobalTask task)
        {
            string subject = "ADMIN";
            if (!User.IsInAnyRole("Administrador", "Super")) subject = "CONTRIBUTOR";
            List<EventGridEvent> eventsList = new List<EventGridEvent>
            {
                new EventGridEvent()
                {
                    Id = Guid.NewGuid().ToString(),
                    EventType = "Export.Event",
                    Data = JsonConvert.SerializeObject(task),
                    EventTime = DateTime.UtcNow,
                    Subject = $"|{subject}|",
                    DataVersion = "2.0"
                }
            };
            await EventGridManager.Instance("EventGridKey", "EventGridTopicEndpoint").SendMessagesToEventGridAsync(eventsList);
        }
        #endregion
    }
}