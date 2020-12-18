using Gosocket.Dian.Application;
using Gosocket.Dian.Application.Cosmos;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Cosmos;
using Gosocket.Dian.Domain.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Infrastructure.Utils;
using Gosocket.Dian.Interfaces.Services;
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
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Mvc;
using EnumHelper = Gosocket.Dian.Web.Models.EnumHelper;

namespace Gosocket.Dian.Web.Controllers
{
    [IPFilter]
    [Authorization]
    public class DocumentController : Controller
    {
        readonly GlobalDocumentService globalDocumentService = new GlobalDocumentService();
        private readonly TableManager documentMetaTableManager = new TableManager("GlobalDocValidatorDocumentMeta");
        private readonly TableManager globalDocValidatorDocumentTableManager = new TableManager("GlobalDocValidatorDocument");
        private readonly TableManager globalDocValidatorTrackingTableManager = new TableManager("GlobalDocValidatorTracking");
        private readonly TableManager globalTaskTableManager = new TableManager("GlobalTask");
        private readonly IRadianPdfCreationService _radianPdfCreationService;
        private readonly IRadianGraphicRepresentationService _radianGraphicRepresentationService;
        private readonly IQueryAssociatedEventsService _queryAssociatedEventsService;

        #region Constructor

        public DocumentController(IRadianPdfCreationService radianPdfCreationService,
                                  IRadianGraphicRepresentationService radianGraphicRepresentationService,
                                  IQueryAssociatedEventsService queryAssociatedEventsService)
        {
            _radianPdfCreationService = radianPdfCreationService;
            _radianGraphicRepresentationService = radianGraphicRepresentationService;
            _queryAssociatedEventsService = queryAssociatedEventsService;
        }

        #endregion

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
        public async Task<ActionResult> Index(SearchDocumentViewModel model) => await GetDocuments(model, 1);

        public async Task<ActionResult> Sent(SearchDocumentViewModel model) => await GetDocuments(model, 2);

        public async Task<ActionResult> Received(SearchDocumentViewModel model) => await GetDocuments(model, 3);

        [CustomRoleAuthorization(CustomRoles = "Proveedor")]
        public async Task<ActionResult> Provider(SearchDocumentViewModel model) => await GetDocuments(model, 4);

        public async Task<ActionResult> Details(string trackId)
        {
            DocValidatorModel model = await ReturnDocValidatorModelByCufe(trackId);
            model.IconsData = _queryAssociatedEventsService.IconType(null, trackId);

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
            GlobalDataDocument globalDataDocument = await CosmosDBService.Instance(date).ReadDocumentAsync(documentKey, partitionKey, date);

            if (globalDataDocument == null)
            {
                var searchViewModel = new SearchViewModel();
                ModelState.AddModelError("DocumentKey", "Documento no encontrado en los registros de la DIAN.");
                return View("Search", searchViewModel);
            }

            DocValidatorModel model = ReturnDocValidationModel(documentKey, globalDataDocument);

            return View(model);
        }

        [ExcludeFilter(typeof(Authorization))]
        public ActionResult Search()
        {
            return RedirectToAction(nameof(UserController.SearchDocument), "User");
        }

        [HttpPost]
        [ExcludeFilter(typeof(Authorization))]
        public ActionResult Search(SearchViewModel model)
        {
            return RedirectToAction(nameof(UserController.SearchDocument), "User");

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
            documentKey = documentKey.ToLower();
            var globalDocValidatorDocumentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(documentKey, documentKey);
            if (globalDocValidatorDocumentMeta == null) return RedirectToAction(nameof(SearchInvalidQR));

            var identifier = globalDocValidatorDocumentMeta.Identifier;
            var globalDocValidatorDocument = globalDocValidatorDocumentTableManager.Find<GlobalDocValidatorDocument>(identifier, identifier);
            if (globalDocValidatorDocument == null) return RedirectToAction(nameof(SearchInvalidQR));

            if (globalDocValidatorDocument.DocumentKey != documentKey) return RedirectToAction(nameof(SearchInvalidQR));

            var partitionKey = $"co|{globalDocValidatorDocument.EmissionDateNumber.Substring(6, 2)}|{globalDocValidatorDocument.DocumentKey.Substring(0, 2)}";

            return RedirectToAction(nameof(FindDocument), new { documentKey = globalDocValidatorDocument.DocumentKey, partitionKey, emissionDate = globalDocValidatorDocument.EmissionDateNumber });
        }

        [ExcludeFilter(typeof(Authorization))]
        public ActionResult SearchInvalidQR()
        {
            return View();
        }

        public async Task<JsonResult> PrintDocument(string cufe)
        {
            byte[] pdfDocument = await _radianPdfCreationService.GetElectronicInvoicePdf(cufe);
            String base64EncodedPdf = System.Convert.ToBase64String(pdfDocument);
            return Json(base64EncodedPdf, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> PrintGraphicRepresentation(string cufe)
        {
            byte[] pdfDocument = await _radianGraphicRepresentationService.GetPdfReport(cufe);
            String base64EncodedPdf = System.Convert.ToBase64String(pdfDocument);
            return Json(base64EncodedPdf, JsonRequestBehavior.AllowGet);
        }

        [ExcludeFilter(typeof(Authorization))]
        public async Task<ActionResult> ShowDocumentToPublic(string Id)
        {
            Tuple<GlobalDocValidatorDocument, List<GlobalDocValidatorDocumentMeta>, Dictionary<int, string>> invoiceAndNotes = _queryAssociatedEventsService.InvoiceAndNotes(Id);
            List<DocValidatorModel> listDocValidatorModels = new List<DocValidatorModel>();
            List<GlobalDocValidatorDocumentMeta> listGlobalValidatorDocumentMeta = invoiceAndNotes.Item2;

            DateTime date = DateNumberToDateTime(invoiceAndNotes.Item1.EmissionDateNumber);
            string partitionKey = ReturnPartitionKey(invoiceAndNotes.Item1.EmissionDateNumber, invoiceAndNotes.Item1.DocumentKey);
            GlobalDataDocument globalDataDocument = await CosmosDBService.Instance(date).ReadDocumentAsync(invoiceAndNotes.Item1.DocumentKey, partitionKey, date);

            DocValidatorModel docModel = await ReturnDocValidatorModelByCufe(invoiceAndNotes.Item1.DocumentKey, globalDataDocument);
            listDocValidatorModels.Add(docModel);

            foreach (var item in listGlobalValidatorDocumentMeta)
            {
                partitionKey = ReturnPartitionKey(invoiceAndNotes.Item1.EmissionDateNumber, item.DocumentKey);
                globalDataDocument = await CosmosDBService.Instance(date).ReadDocumentAsync(item.DocumentKey, partitionKey, date);

                docModel = await ReturnDocValidatorModelByCufe(item.DocumentKey, globalDataDocument);

                listDocValidatorModels.Add(docModel);
            }

            InvoiceNotesViewModel invoiceNotes = new InvoiceNotesViewModel(invoiceAndNotes.Item1, invoiceAndNotes.Item2, listDocValidatorModels, invoiceAndNotes.Item3);

            return View(invoiceNotes);
        }

        #region Private methods               

        private string ReturnPartitionKey(string emissionDateNumber, string documentKey)
        {
            return $"co|{emissionDateNumber.Substring(6, 2)}|{documentKey.Substring(0, 2)}";
        }

        private async Task<DocValidatorModel> ReturnDocValidatorModelByCufe(string trackId, GlobalDataDocument globalDataDocument = null)
        {
            List<DocValidatorTrackingModel> validations = GetValidatedRules(trackId);

            GlobalDocValidatorDocumentMeta globalDocValidatorDocumentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
            string emissionDateNumber = globalDocValidatorDocumentMeta.EmissionDate.ToString("yyyyMMdd");
            string partitionKey = $"co|{emissionDateNumber.Substring(6, 2)}|{globalDocValidatorDocumentMeta.DocumentKey.Substring(0, 2)}";

            DateTime date = DateNumberToDateTime(emissionDateNumber);

            if (globalDataDocument == null)
                globalDataDocument = await CosmosDBService.Instance(date).ReadDocumentAsync(globalDocValidatorDocumentMeta.DocumentKey, partitionKey, date);

            DocumentViewModel document = new DocumentViewModel
            {
                DocumentKey = globalDataDocument.DocumentKey,
                Amount = globalDataDocument.FreeAmount,
                DocumentTypeId = globalDataDocument.DocumentTypeId,
                DocumentTypeName = globalDataDocument.DocumentTypeName,
                GenerationDate = globalDataDocument.GenerationTimeStamp,
                Id = globalDataDocument.DocumentKey,
                EmissionDate = globalDataDocument.EmissionDate,
                Number = Services.Utils.StringUtil.TextAfter(globalDataDocument.SerieAndNumber, globalDataDocument.Serie),
                //TechProviderName = globalDataDocument?.TechProviderInfo?.TechProviderName,
                TechProviderCode = globalDataDocument?.TechProviderInfo?.TechProviderCode,
                ReceiverName = globalDataDocument.ReceiverName,
                ReceiverCode = globalDataDocument.ReceiverCode,
                ReceptionDate = globalDataDocument.ReceptionTimeStamp,
                Serie = globalDataDocument.Serie,
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
                DocumentKey = e.DocumentKey,
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


            model.Events = new List<EventsViewModel>();
            List<GlobalDocValidatorDocumentMeta> eventsByInvoice = documentMetaTableManager.FindDocumentReferenced_TypeId<GlobalDocValidatorDocumentMeta>(trackId, "96");
            if (eventsByInvoice.Any())
            {

                foreach (var eventItem in eventsByInvoice)
                {
                    if (!string.IsNullOrEmpty(eventItem.EventCode))
                    {
                        GlobalDocValidatorDocument eventVerification = globalDocValidatorDocumentTableManager.Find<GlobalDocValidatorDocument>(eventItem.Identifier, eventItem.Identifier);
                        if (eventVerification != null && (eventVerification.ValidationStatus == 1 || eventVerification.ValidationStatus == 10))
                        {
                            string eventcodetext = EnumHelper.GetEnumDescription((Enum.Parse(typeof(Domain.Common.EventStatus), eventItem.EventCode)));
                            model.Events.Add(new EventsViewModel()
                            {
                                DocumentKey = eventItem.DocumentKey,
                                EventCode = eventItem.EventCode,
                                Description = eventcodetext,
                                EventDate = eventItem.SigningTimeStamp,
                                SenderCode = eventItem.SenderCode,
                                Sender = eventItem.SenderName,
                                ReceiverCode = eventItem.ReceiverCode,
                                Receiver = eventItem.ReceiverName
                            });
                            model.Events = model.Events.OrderBy(t => t.EventCode).ToList();
                        }

                    }
                }
            }

            return model;
        }

        private DocValidatorModel ReturnDocValidationModel(string documentKey, GlobalDataDocument globalDataDocument)
        {
            var model = new DocValidatorModel();
            model.Validations.AddRange(GetValidatedRules(documentKey));

            model.Document = new DocumentViewModel
            {
                DocumentKey = globalDataDocument.DocumentKey,
                Amount = globalDataDocument.FreeAmount,
                DocumentTypeId = globalDataDocument.DocumentTypeId,
                DocumentTypeName = globalDataDocument.DocumentTypeName,
                GenerationDate = globalDataDocument.GenerationTimeStamp,
                Id = globalDataDocument.DocumentKey,
                EmissionDate = globalDataDocument.EmissionDate,
                Number = Services.Utils.StringUtil.TextAfter(globalDataDocument.SerieAndNumber, globalDataDocument.Serie),
                //TechProviderName = globalDataDocument?.TechProviderInfo?.TechProviderName,
                TechProviderCode = globalDataDocument?.TechProviderInfo?.TechProviderCode,
                ReceiverName = globalDataDocument.ReceiverName,
                ReceiverCode = globalDataDocument.ReceiverCode,
                ReceptionDate = globalDataDocument.ReceptionTimeStamp,
                Serie = globalDataDocument.Serie,
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

            return model;
        }

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

        private async Task<ActionResult> GetDocuments(SearchDocumentViewModel model, int filterType)
        {
            SetView(filterType);
            string continuationToken = (string)Session["Continuation_Token_" + model.Page];

            if (string.IsNullOrEmpty(continuationToken))
                continuationToken = "";

            List<string> pks = null;
            model.DocumentKey = model.DocumentKey?.ToLower();

            if (!string.IsNullOrEmpty(model.DocumentKey))
            {
                GlobalDocValidatorDocumentMeta documentMeta =
                    documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(model.DocumentKey, model.DocumentKey);
                GlobalDocValidatorDocument globalDocValidatorDocument =
                    globalDocValidatorDocumentTableManager.Find<GlobalDocValidatorDocument>(documentMeta?.Identifier, documentMeta?.Identifier);

                if (globalDocValidatorDocument == null)
                    return View("Index", model);

                if (globalDocValidatorDocument.DocumentKey != model.DocumentKey)
                    return View("Index", model);

                pks = new List<string> { $"co|{globalDocValidatorDocument.EmissionDateNumber.Substring(6, 2)}|{model.DocumentKey.Substring(0, 2)}" };
            }

            if (model.RadianStatus > 0 && model.RadianStatus < 6 && model.DocumentTypeId.Equals("00"))
                model.DocumentTypeId = "01";

            (bool hasMoreResults, string continuation, List<GlobalDataDocument> globalDataDocuments) cosmosResponse =
                (false, null, new List<GlobalDataDocument>());

            switch (filterType)
            {
                case 1:
                    cosmosResponse = await CosmosDBService.Instance(model.EndDate).ReadDocumentsAsyncOrderByReception(continuationToken,
                                                                                                                      model.StartDate,
                                                                                                                      model.EndDate,
                                                                                                                      model.Status,
                                                                                                                      model.DocumentTypeId,
                                                                                                                      model.SenderCode,
                                                                                                                      model.SerieAndNumber,
                                                                                                                      model.ReceiverCode,
                                                                                                                      null,
                                                                                                                      model.MaxItemCount,
                                                                                                                      model.DocumentKey,
                                                                                                                      model.ReferencesType,
                                                                                                                      pks,
                                                                                                                      model.RadianStatus);
                    break;
                case 2:
                    cosmosResponse = await CosmosDBService.Instance(model.EndDate).ReadDocumentsAsyncOrderByReception(continuationToken,
                                                                                                                      model.StartDate,
                                                                                                                      model.EndDate,
                                                                                                                      model.Status,
                                                                                                                      model.DocumentTypeId,
                                                                                                                      User.ContributorCode(),
                                                                                                                      model.SerieAndNumber,
                                                                                                                      model.ReceiverCode,
                                                                                                                      null,
                                                                                                                      model.MaxItemCount,
                                                                                                                      model.DocumentKey,
                                                                                                                      model.ReferencesType,
                                                                                                                      pks,
                                                                                                                      model.RadianStatus);
                    break;
                case 3:
                    cosmosResponse = await CosmosDBService.Instance(model.EndDate).ReadDocumentsAsyncOrderByReception(continuationToken,
                                                                                                                      model.StartDate,
                                                                                                                      model.EndDate,
                                                                                                                      model.Status,
                                                                                                                      model.DocumentTypeId,
                                                                                                                      model.SenderCode,
                                                                                                                      model.SerieAndNumber,
                                                                                                                      User.ContributorCode(),
                                                                                                                      null,
                                                                                                                      model.MaxItemCount,
                                                                                                                      model.DocumentKey,
                                                                                                                      model.ReferencesType,
                                                                                                                      pks,
                                                                                                                      model.RadianStatus);
                    break;
                case 4:
                    cosmosResponse = await CosmosDBService.Instance(model.EndDate).ReadDocumentsAsyncOrderByReception(continuationToken,
                                                                                                                      model.StartDate,
                                                                                                                      model.EndDate,
                                                                                                                      model.Status,
                                                                                                                      model.DocumentTypeId,
                                                                                                                      model.SenderCode,
                                                                                                                      model.SerieAndNumber,
                                                                                                                      model.ReceiverCode,
                                                                                                                      User.ContributorCode(),
                                                                                                                      model.MaxItemCount,
                                                                                                                      model.DocumentKey,
                                                                                                                      model.ReferencesType,
                                                                                                                      pks,
                                                                                                                      model.RadianStatus);
                    break;
            }

            if ((cosmosResponse.globalDataDocuments?.Count ?? 0) > 0)
            {
                model.Documents = cosmosResponse.globalDataDocuments.Select(d => new DocumentViewModel
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
                    Events = d.Events.Select(
                        e => new EventViewModel()
                        {
                            Code = e.Code,
                            Date = e.Date,
                            Description = e.Description
                        }).ToList()
                }).ToList();

                foreach (DocumentViewModel docView in model.Documents)
                    docView.RadianStatusName = DeterminateRadianStatus(docView.Events, model.DocumentTypeId);
            }

            if (model.RadianStatus == 7 && model.DocumentTypeId.Equals("00"))
                model.Documents.RemoveAll(d => d.DocumentTypeId.Equals("01"));

            model.IsNextPage = cosmosResponse.hasMoreResults;
            Session["Continuation_Token_" + (model.Page + 1)] = cosmosResponse.continuation;

            return View("Index", model);
        }

        private string DeterminateRadianStatus(List<EventViewModel> events, string documentTypeId)
        {
            if (events.Count() == 0)
                return RadianDocumentStatus.DontApply.GetDescription();

            int lastEventCode = int.Parse(events.OrderBy(t => t.Date).Last().Code);

            if (lastEventCode == ((int)EventStatus.NegotiatedInvoice)
                || lastEventCode == ((int)EventStatus.AnulacionLimitacionCirculacion))
                return RadianDocumentStatus.Limited.GetDescription();

            if (lastEventCode == ((int)EventStatus.NotificacionPagoTotalParcial))
                return RadianDocumentStatus.Paid.GetDescription();

            if (lastEventCode == ((int)EventStatus.EndosoPropiedad)
                || lastEventCode == ((int)EventStatus.EndosoGarantia)
                || lastEventCode == ((int)EventStatus.EndosoProcuracion)
                || lastEventCode == ((int)EventStatus.InvoiceOfferedForNegotiation))
                return RadianDocumentStatus.Endorsed.GetDescription();

            if (lastEventCode == ((int)EventStatus.SolicitudDisponibilizacion))
                return RadianDocumentStatus.Readiness.GetDescription();

            if (events.Any(e => int.Parse(e.Code) == ((int)EventStatus.Received))
                && events.Any(e => int.Parse(e.Code) == ((int)EventStatus.Receipt))
                && events.Any(e => int.Parse(e.Code) == ((int)EventStatus.Accepted)))
                return RadianDocumentStatus.SecurityTitle.GetDescription();

            if (documentTypeId == "01")
                return RadianDocumentStatus.ElectronicInvoice.GetDescription();

            return RadianDocumentStatus.DontApply.GetDescription();
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