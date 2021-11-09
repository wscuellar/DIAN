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
        private readonly TableManager globalDocReferenceAttorneyTableManager = new TableManager("GlobalDocReferenceAttorney");
        private readonly TableManager globalDocValidatorTrackingTableManager = new TableManager("GlobalDocValidatorTracking");
        private readonly TableManager globalTaskTableManager = new TableManager("GlobalTask");
        private readonly TableManager payrollTableManager = new TableManager("GlobalDocPayRoll");
        private readonly TableManager municipalitiesTableManager = new TableManager("Municipalities");
        private readonly TableManager globalDocPayrollEmployeesTableManager = new TableManager("GlobalDocPayrollEmployees");
        private readonly IRadianPdfCreationService _radianPdfCreationService;
        private readonly IAssociateDocuments _associateDocuments;
        private readonly IRadianGraphicRepresentationService _radianGraphicRepresentationService;
        private readonly IRadianSupportDocument _radianSupportDocument;
        private readonly IQueryAssociatedEventsService _queryAssociatedEventsService;
        private readonly IRadianPayrollGraphicRepresentationService _radianPayrollGraphicRepresentationService;
        private static readonly OtherDocElecPayrollService otherDocElecPayrollService = new OtherDocElecPayrollService();
        const string TITULOVALORCODES = "030, 032, 033, 034";
        const string DISPONIBILIZACIONCODES = "036";
        const string PAGADACODES = "045";
        const string ENDOSOCODES = "037,038,039";
        const string LIMITACIONCODES = "041";
        const string ANULACIONENDOSOCODES = "040";
        const string ANULACIONLIMITACIONCODES = "042";
        const string MANDATOCODES = "043";

        public List<GlobalDocPayroll> PayrollList
        {
            get
            {
                if (Session[$"PayrollList"] == null) Session[$"PayrollList"] = new List<GlobalDocPayroll>();
                return Session[$"PayrollList"] as List<GlobalDocPayroll>;
            }
            set
            {
                Session[$"PayrollList"] = value;
            }
        }

        #region Properties


        private readonly FileManager _fileManager;

        #endregion


        #region Constructor

        public DocumentController(IRadianPdfCreationService radianPdfCreationService,
                                  IRadianGraphicRepresentationService radianGraphicRepresentationService,
                                  IQueryAssociatedEventsService queryAssociatedEventsService,
                                  IRadianSupportDocument radianSupportDocument, FileManager fileManager,
                                  IRadianPayrollGraphicRepresentationService radianPayrollGraphicRepresentationService,
                                  IAssociateDocuments associateDocuments)
        {
            _radianSupportDocument = radianSupportDocument;
            _radianPdfCreationService = radianPdfCreationService;
            _radianPdfCreationService = radianPdfCreationService;
            _radianGraphicRepresentationService = radianGraphicRepresentationService;
            _queryAssociatedEventsService = queryAssociatedEventsService;
            _fileManager = fileManager;
            _radianPayrollGraphicRepresentationService = radianPayrollGraphicRepresentationService;
            _associateDocuments = associateDocuments;
        }

        #endregion

        private static HttpClient client = new HttpClient();

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

        [ExcludeFilter(typeof(Authorization))]
        public async Task<ActionResult> Details(string trackId)
        {
            DocValidatorModel model = await ReturnDocValidatorModelByCufe(trackId);
            GetDataLegitemateOwner(model);
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
                //IsValidCaptcha(recaptchaToken);
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
        
        [CustomRoleAuthorization(CustomRoles = "Administrador")]
        public async Task<ActionResult> Payroll()
        {
            var model = new PayrollViewModel();

            LoadData(ref model);
            ViewBag.CurrentPage = Navigation.NavigationEnum.Payroll;

            // Por defecto se listarán los primeros 20 registros que cumplan con las siguientes condiciones:
            model.TipoDocumento = "00";
            model.Ciudad = "00";
            model.MesValidacion = DateTime.Now.Month.ToString().PadLeft(2, char.Parse("0")); // (el mes actual)
            model.RangoSalarial = "01"; // ($0- $1.000.000)

            this.SetViewBag_FirstSurnameData();

            await this.GetPayrollData(20, model);

            model.Payrolls = GetPayrollsList(model);

            return View(model);
        }
      
        [CustomRoleAuthorization(CustomRoles = "Administrador")]
        [HttpPost]
        public async Task<ActionResult> Payroll(PayrollViewModel model)
        {
            ViewBag.CurrentPage = Navigation.NavigationEnum.Payroll;            

            if (string.IsNullOrWhiteSpace(model.CUNE) && string.IsNullOrWhiteSpace(model.NumeroDocumento))
            {
                if (!model.RangoNumeracionMenor.HasValue || !model.RangoNumeracionMayor.HasValue)
                {
                    model.Mensaje = "Debe seleccionar un Rango de Numeración.";
                    LoadData(ref model);
                    model.Payrolls = new List<DocumentViewPayroll>();
                    return View(model);
                }
                // Se inicia el contador en 1, porque el rango ya debe estar seleccionado
                int totalFiltersSelected = 1;

                // Mínimo se requieren dos filtros más...
                if (model.MesValidacion != "00") totalFiltersSelected++;

                if (model.TipoDocumento != "00") totalFiltersSelected++;

                if (!string.IsNullOrWhiteSpace(model.NumeroDocumento)) totalFiltersSelected++;

                if (!string.IsNullOrWhiteSpace(model.LetraPrimerApellido)) totalFiltersSelected++;

                if (model.RangoSalarial != "00") totalFiltersSelected++;

                if (model.Ciudad != "00") totalFiltersSelected++;

                if (totalFiltersSelected < 3)
                {
                    model.Mensaje = "Debe seleccionar al menos 3 filtros o consultar por el CUNE.";
                    LoadData(ref model);
                    model.Payrolls = new List<DocumentViewPayroll>();
                    return View(model);
                }
            }

            await this.GetPayrollData(50, model);

            model.Payrolls = GetPayrollsList(model);

            LoadData(ref model);
            this.SetViewBag_FirstSurnameData();

            return View(model);
        }

        private List<DocumentViewPayroll> GetPayrollsList(PayrollViewModel model)
        {
            model.TotalItems = this.PayrollList.Count;
            model.HasMoreData = false;
            var resultPayroll = new List<GlobalDocPayroll>();
            if (this.PayrollList != null && this.PayrollList.Count > 0)
            {
                // la paginación se hace de 20 registros...
                var maxItems = model.MaxItemCount;
                var index = (model.Page * maxItems);
                var nextIndex = (index + maxItems);

                var totalItemsList = this.PayrollList.Count;
                if (nextIndex >= totalItemsList)
                {
                    maxItems = maxItems - (nextIndex - totalItemsList);
                    model.HasMoreData = false;
                }
                else
                    model.HasMoreData = true;

                resultPayroll = this.PayrollList.GetRange(index, maxItems);
            }

            List<DocumentViewPayroll> result = new List<DocumentViewPayroll>();

            foreach (var payroll in resultPayroll)
            {
                var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(payroll.CUNE, payroll.CUNE);
                var document = globalDocValidatorDocumentTableManager.Find<GlobalDocValidatorDocument>(documentMeta.Identifier, documentMeta.Identifier);
                var numAdjustment = string.Empty;

                if (int.Parse(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayroll && !string.IsNullOrWhiteSpace(documentMeta.DocumentReferencedKey)) // Nómina Individual con Ajuste...
                {
                    var adjustmentDocumentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(documentMeta.DocumentReferencedKey, documentMeta.DocumentReferencedKey);
                    if (adjustmentDocumentMeta != null) numAdjustment = adjustmentDocumentMeta.SerieAndNumber;
                }

                result.Add(new DocumentViewPayroll
                {
                    PartitionKey = payroll.PartitionKey,
                    RowKey = payroll.RowKey,
                    link = Url.Action("DownloadPayrollPDF", new { id = payroll.PartitionKey }),
                    NumeroNomina = payroll.Numero,
                    ApellidosNombre = $"{payroll.PrimerApellido} {payroll.SegundoApellido} {payroll.PrimerNombre}",
                    TipoDocumento = findTypeDocument(payroll.TipoDocumento),
                    NoDocumento = payroll.NumeroDocumento,
                    Salario = payroll.Sueldo,
                    Devengado = payroll.DevengadosTotal,
                    Deducido = payroll.DeduccionesTotal,
                    ValorTotal = payroll.ComprobanteTotal,
                    MesValidacion = (payroll.FechaPagoInicio.HasValue) ? payroll.FechaPagoInicio.Value.Month.ToString() : string.Empty,
                    Novedad = payroll.Novedad,
                    NumeroAjuste = numAdjustment,
                    Resultado = document.ValidationStatusName,
                    Ciudad = payroll.MunicipioCiudad
                });
            }

            if (model.Ordenar != "00")
            {
                switch (model.Ordenar)
                {
                    case "01":
                        result = result.OrderBy(t => t.NoDocumento).ToList();
                        break;
                    case "02":
                        result = result.OrderByDescending(t => t.NoDocumento).ToList();
                        break;
                    case "03":
                        result = result.OrderBy(t => t.ApellidosNombre).ToList();
                        break;
                    case "04":
                        result = result.OrderByDescending(t => t.ApellidosNombre).ToList();
                        break;
                }
            }

           return result;
        }

        [ExcludeFilter(typeof(Authorization))]
        public FileResult DownloadPayrollPDF(string id)
        {
            var documentName = "NóminaIndividualElectrónica";
            var pdfbytes = this._radianPayrollGraphicRepresentationService.GetPdfReport(id, ref documentName);
            return File(pdfbytes, "application/pdf", $"{documentName}.pdf");
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

            return RedirectToAction(nameof(ShowDocumentToPublic), new { id = documentKey });
        }

        [ExcludeFilter(typeof(Authorization))]
        public ActionResult SearchInvalidQR()
        {
            return View();
        }

        [ExcludeFilter(typeof(Authorization))]
        public async Task<JsonResult> PrintDocument(string cufe)
        {
            string webPath = Url.Action("searchqr", "Document", null, Request.Url.Scheme);
            byte[] pdfDocument = await _radianPdfCreationService.GetElectronicInvoicePdf(cufe, webPath);
            String base64EncodedPdf = Convert.ToBase64String(pdfDocument);
            var json = Json(base64EncodedPdf, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = 500000000;
            return json;
        }

        [ExcludeFilter(typeof(Authorization))]
        public async Task<JsonResult> PrintSupportDocument(string cufe)
        {
            string webPath = Url.Action("searchqr", "Document", null, Request.Url.Scheme);
            byte[] pdfDocument = await _radianSupportDocument.GetGraphicRepresentation(cufe, webPath);
            String base64EncodedPdf = Convert.ToBase64String(pdfDocument);
            var json = Json(base64EncodedPdf, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = 500000000;
            return json;
        }

        [ExcludeFilter(typeof(Authorization))]
        public async Task<JsonResult> PrintGraphicRepresentation(string cufe)
        {
            string urlBase = Request.Url.OriginalString.Replace(Request.Url.AbsolutePath, string.Empty);
            byte[] pdfDocument = await _radianGraphicRepresentationService.GetPdfReport(cufe, urlBase);
            String base64EncodedPdf = Convert.ToBase64String(pdfDocument);
            return Json(base64EncodedPdf, JsonRequestBehavior.AllowGet);
        }

        [ExcludeFilter(typeof(Authorization))]
        public async Task<ActionResult> ShowDocumentToPublic(string Id)
        {            
            List<DocValidatorModel> listDocValidatorModels = new List<DocValidatorModel>();
            List<GlobalDocValidatorDocumentMeta> listGlobalValidatorDocumentMeta = new List<GlobalDocValidatorDocumentMeta>();

            var globalDocValidatorDocumentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(Id, Id);

            var identifier = globalDocValidatorDocumentMeta.Identifier;

            GlobalDocValidatorDocument globalDocValidatorDocument = globalDocValidatorDocumentTableManager.Find<GlobalDocValidatorDocument>(identifier, identifier);

            DateTime date = DateNumberToDateTime(globalDocValidatorDocument.EmissionDateNumber);
            string partitionKey = ReturnPartitionKey(globalDocValidatorDocument.EmissionDateNumber, globalDocValidatorDocument.DocumentKey);
            GlobalDataDocument globalDataDocument = await CosmosDBService.Instance(date).ReadDocumentAsync(globalDocValidatorDocument.DocumentKey, partitionKey, date);
            if (globalDataDocument.DocumentTypeId == "96")
                return RedirectToAction("SearchDocument", "User");

            Tuple<List<GlobalDocValidatorDocumentMeta>, Dictionary<int, string>> invoiceAndNotes = _queryAssociatedEventsService.InvoiceAndNotes(globalDataDocument.DocumentTags, Id, globalDocValidatorDocument.DocumentTypeId);

            listGlobalValidatorDocumentMeta = invoiceAndNotes.Item1;

            DocValidatorModel docModel = await ReturnDocValidatorModelByCufe(globalDocValidatorDocument.DocumentKey, globalDataDocument);
            GetDataLegitemateOwner(docModel);
            listDocValidatorModels.Add(docModel);

            foreach (var item in listGlobalValidatorDocumentMeta)
            {
                partitionKey = ReturnPartitionKey(globalDocValidatorDocument.EmissionDateNumber, item.DocumentKey);
                globalDataDocument = await CosmosDBService.Instance(date).ReadDocumentAsync(item.DocumentKey, partitionKey, date);

                docModel = await ReturnDocValidatorModelByCufe(item.DocumentKey, globalDataDocument);
                GetDataLegitemateOwner(docModel);
                listDocValidatorModels.Add(docModel);
            }

            InvoiceNotesViewModel invoiceNotes = new InvoiceNotesViewModel(globalDocValidatorDocument, invoiceAndNotes.Item1, listDocValidatorModels, invoiceAndNotes.Item2);

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
            GlobalDocValidatorDocumentMeta globalDocValidatorDocumentMeta;

            List<InvoiceWrapper> invoiceData = _associateDocuments.GetEventsByTrackId(trackId);
            if (invoiceData.Any())
                globalDocValidatorDocumentMeta = invoiceData.First().Invoice;
            else
                globalDocValidatorDocumentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);

            string emissionDateNumber = globalDocValidatorDocumentMeta.EmissionDate.ToString("yyyyMMdd");
            string partitionKey = $"co|{emissionDateNumber.Substring(6, 2)}|{globalDocValidatorDocumentMeta.DocumentKey.Substring(0, 2)}";

            DateTime date = DateNumberToDateTime(emissionDateNumber);

            if (globalDataDocument == null)
                globalDataDocument = await CosmosDBService.Instance(date).ReadDocumentAsync(globalDocValidatorDocumentMeta.DocumentKey, partitionKey, date);

            DocumentViewModel document = new DocumentViewModel();

            #region --- Mapear globalDataDocument ---

            if (globalDataDocument != null)
            {
                document = new DocumentViewModel
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
            }

            #endregion

            var model = new DocValidatorModel
            {
                Document = document,
                Validations = validations
            };

            model.Events = new List<EventsViewModel>();
            GlobalDocReferenceAttorney attorney;
            string eventcodetext = string.Empty;
            if (invoiceData.Any())
            {
                var invoice = invoiceData.First();

                foreach (var eventItem in invoice.Documents)
                {
                    EventStatus eventStatus = (EventStatus)Enum.Parse(typeof(Domain.Common.EventStatus), eventItem.DocumentMeta.EventCode.ToString());
                    eventcodetext = _queryAssociatedEventsService.EventTitle(eventStatus, eventItem.DocumentMeta.CustomizationID, eventItem.DocumentMeta.EventCode,
                       eventItem.DocumentMeta.EventCode == "043" ? eventItem.Attorney?.SchemeID : string.Empty);

                    model.Events.Add(new EventsViewModel()
                    {
                        DocumentKey = eventItem.DocumentMeta.DocumentKey,
                        EventCode = eventItem.DocumentMeta.EventCode,
                        Description = eventcodetext,
                        EventDate = eventItem.DocumentMeta.SigningTimeStamp,
                        SenderCode = eventItem.DocumentMeta.SenderCode,
                        Sender = eventItem.DocumentMeta.SenderName,
                        ReceiverCode = eventItem.DocumentMeta.ReceiverCode,
                        Receiver = eventItem.DocumentMeta.ReceiverName
                    });
                }

                model.Events = model.Events.Distinct().OrderBy(t => t.EventDate).ToList();
                return model;
            }

            if (invoiceData.Any())
            {
                List<GlobalDocValidatorDocumentMeta> eventsByInvoice = (invoiceData[0].Documents.Any()) ? invoiceData[0].Documents.Select(x => x.DocumentMeta).ToList() : null;
                if (eventsByInvoice.Any())
                    eventsByInvoice = eventsByInvoice.Where(t => int.Parse(t.DocumentTypeId) == (int)DocumentType.ApplicationResponse).ToList();

                if (eventsByInvoice.Any())
                {
                    foreach (var eventItem in eventsByInvoice)
                    {
                        if (!string.IsNullOrEmpty(eventItem.EventCode))
                        {
                            GlobalDocValidatorDocument eventVerification = globalDocValidatorDocumentTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(eventItem.Identifier, eventItem.Identifier, eventItem.PartitionKey);

                            if (eventVerification != null && (eventVerification.ValidationStatus == 0 || eventVerification.ValidationStatus == 1 || eventVerification.ValidationStatus == 10))
                            {
                                attorney = new GlobalDocReferenceAttorney();
                                eventcodetext = string.Empty;
                                if (eventItem.EventCode == "043")
                                    attorney = globalDocReferenceAttorneyTableManager.FindDocumentReferenceAttorney<GlobalDocReferenceAttorney>(eventItem.DocumentKey);

                                eventcodetext = _queryAssociatedEventsService.EventTitle((EventStatus)Enum.Parse(typeof(Domain.Common.EventStatus), eventItem.EventCode.ToString()),
                                eventItem.CustomizationID, eventItem.EventCode, eventItem.EventCode == "043" ? attorney?.SchemeID : string.Empty);

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
                                //Adiciono el evento de finalizacion de mandato.
                                if (eventItem.EventCode == "043")
                                {
                                    EventsViewModel _event = FillEventsAttorney(attorney);
                                    if (_event != null)
                                        model.Events.Add(_event);
                                }

                            }

                        }
                    }
                    model.Events = model.Events.OrderBy(t => t.EventDate).ToList();
                }
            }


            return model;
        }

        private static void GetDataLegitemateOwner(DocValidatorModel model)
        {
            if (model.Events.Count > 0)
            {
                var eventLegitimateOwner = model.Events.LastOrDefault(t => t.EventCode != null && Convert.ToInt32(t.EventCode) == (int)EventStatus.EndosoPropiedad);
                model.LegitimateOwner = eventLegitimateOwner != null ? eventLegitimateOwner.Receiver : model.Document.SenderName;
                var eventDateInscription = model.Events.LastOrDefault(t => t.EventCode != null && Convert.ToInt32(t.EventCode) == (int)EventStatus.SolicitudDisponibilizacion);
                if (eventDateInscription != null)
                {
                    model.DateInscription = eventDateInscription.EventDate;
                }
            }
            else
                model.LegitimateOwner = model.Document.SenderName;
        }

        private EventsViewModel FillEventsAttorney(GlobalDocReferenceAttorney Attorney)
        {
            EventsViewModel _event = null;
            //Se busca en la GlobalDocReferenceAttorney 
            //En el campo DocReferencedEndAttorney si tiene valor 

            if (Attorney != null && !string.IsNullOrEmpty(Attorney.DocReferencedEndAthorney))
            {
                //Se busca en la GlobalDocValidatorDocumentMeta y se saca el evento de terminacion.
                GlobalDocValidatorDocumentMeta eventEndMandate = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(Attorney.DocReferencedEndAthorney, Attorney.DocReferencedEndAthorney);
                if (eventEndMandate != null)
                {
                    var eventcodetext = _queryAssociatedEventsService.EventTitle((EventStatus)Enum.Parse(typeof(Domain.Common.EventStatus), eventEndMandate.EventCode.ToString()),
                          eventEndMandate.CustomizationID, eventEndMandate.EventCode, Attorney.SchemeID);
                    _event = new EventsViewModel()
                    {
                        DocumentKey = eventEndMandate.DocumentKey,
                        EventCode = eventEndMandate.EventCode,
                        Description = eventcodetext,
                        EventDate = eventEndMandate.SigningTimeStamp,
                        SenderCode = eventEndMandate.SenderCode,
                        Sender = eventEndMandate.SenderName,
                        ReceiverCode = eventEndMandate.ReceiverCode,
                        Receiver = eventEndMandate.ReceiverName
                    };
                }
            }
            return _event;
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
            
            var buffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestObj));
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return client.PostAsync(url, byteContent).Result;
            
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

            if (model.RadianStatus > 0 && model.RadianStatus < 7 && model.DocumentTypeId.Equals("00"))
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
                {
                    try
                    {
                        docView.RadianStatusName = DeterminateRadianStatus(docView.Events, docView.DocumentTypeId);
                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }
                }
            }

            if (model.RadianStatus == 7 && model.DocumentTypeId.Equals("00"))
                model.Documents.RemoveAll(d => d.DocumentTypeId.Equals("01"));

            model.IsNextPage = cosmosResponse.hasMoreResults;
            Session["Continuation_Token_" + (model.Page + 1)] = cosmosResponse.continuation;

            return View("Index", model);
        }

        private List<EventViewModel> eventListByTimestamp(List<EventViewModel> originalList)
        {
            List<EventViewModel> resultList = new List<EventViewModel>();

            foreach (var item in originalList)
            {
                if (!string.IsNullOrEmpty(item.Code))
                {
                    resultList.Add(item);
                }
            }

            return resultList.Where(e => TITULOVALORCODES.Contains(e.Code.Trim()) || DISPONIBILIZACIONCODES.Contains(e.Code.Trim()) || PAGADACODES.Contains(e.Code.Trim()) || ENDOSOCODES.Contains(e.Code.Trim()) || DISPONIBILIZACIONCODES.Contains(e.Code.Trim()) || ANULACIONENDOSOCODES.Contains(e.Code.Trim()) || LIMITACIONCODES.Contains(e.Code.Trim()) || ANULACIONLIMITACIONCODES.Contains(e.Code.Trim())).ToList();
        }


        private string DeterminateRadianStatus(List<EventViewModel> events, string documentTypeId)
        {

            if (events.Count == 0)
            {
                if (documentTypeId == "01")
                    return RadianDocumentStatus.ElectronicInvoice.GetDescription();
                else
                    return RadianDocumentStatus.DontApply.GetDescription();
            }

            Dictionary<int, string> statusValue = new Dictionary<int, string>();
            int securityTitleCounter = 0;
            int index = 3;

            if (documentTypeId == "01")
                statusValue.Add(1, $"{RadianDocumentStatus.ElectronicInvoice.GetDescription()}");

            events = events.Where(t => t.Code != null).OrderBy(t => t.TimeStamp).ToList();
            events = eventListByTimestamp(events).OrderBy(t => t.TimeStamp).ThenBy(t => t.Code).ToList();

            var eveOrder = events.OrderBy(z => z.Date).ToList();

            eveOrder = removeEvents(eveOrder, EventStatus.InvoiceOfferedForNegotiation, new List<string>() { $"0{(int)EventStatus.EndosoProcuracion}", $"0{ (int)EventStatus.EndosoGarantia}" });
            eveOrder = removeEvents(eveOrder, EventStatus.AnulacionLimitacionCirculacion, new List<string>() { $"0{(int)EventStatus.NegotiatedInvoice}" });


            foreach (var documentMeta in eveOrder)
            {
                if (TITULOVALORCODES.Contains(documentMeta.Code.Trim()))
                    securityTitleCounter++;

                if (!statusValue.Values.Contains(RadianDocumentStatus.SecurityTitle.GetDescription()) && securityTitleCounter >= 3)
                    statusValue.Add(2, $"{RadianDocumentStatus.SecurityTitle.GetDescription()}");//5

                if (DISPONIBILIZACIONCODES.Contains(documentMeta.Code.Trim()))
                {
                    statusValue.Add(index, $"{RadianDocumentStatus.Readiness.GetDescription()}");
                    index++;
                }

                if (ENDOSOCODES.Contains(documentMeta.Code.Trim()))
                {
                    statusValue.Add(index, $"{RadianDocumentStatus.Endorsed.GetDescription()}");
                    index++;
                }

                if (PAGADACODES.Contains(documentMeta.Code.Trim()))
                {
                    statusValue.Add(index, $"{RadianDocumentStatus.Paid.GetDescription()}");
                    index++;
                }

                if (LIMITACIONCODES.Contains(documentMeta.Code.Trim()))
                {
                    statusValue.Add(index, $"{RadianDocumentStatus.Limited.GetDescription()}");
                    index++;
                }
            }

            Dictionary<int, string> cleanDictionary = statusValue.GroupBy(pair => pair.Value)
                         .Select(group => group.Last())
                         .ToDictionary(pair => pair.Key, pair => pair.Value);

            if (cleanDictionary.ContainsValue(RadianDocumentStatus.Readiness.GetDescription()) || cleanDictionary.ContainsValue(RadianDocumentStatus.Limited.GetDescription()))
                cleanDictionary.Remove(1);

            if (cleanDictionary.Count == 0)
                return RadianDocumentStatus.DontApply.GetDescription();

            return cleanDictionary.OrderBy(t => t.Key).Last().Value;

        }


        private List<EventViewModel> removeEvents(List<EventViewModel> events, EventStatus conditionalStatus, List<string> removeData)
        {
            if (events.Count > 0 && events.Last().Code == $"0{(int)conditionalStatus }")
            {
                events.Remove(events.Last());
                foreach (var item in events.OrderByDescending(x => x.Date))
                {
                    if (removeData.Contains(item.Code.Trim()))
                    {
                        events.Remove(item);
                        break;
                    }
                }
                return removeEvents(events, conditionalStatus, removeData);
            }
            return events;
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

        // Payroll
        void LoadData(ref PayrollViewModel model)
        {
            model.LetrasPrimerApellido = LetraModel.List();
            model.TiposDocumento = TipoDocumentoModel.List();
            model.RangosSalarial = RangoSalarialModel.List();
            model.MesesValidacion = MesModel.List();
            model.Ordenadores = OrdenarModel.List();
            model.Ciudades = GetCiudadModelLists();
        }
        private async Task GetPayrollData(int toTake, PayrollViewModel model)
        {
            this.PayrollList = null;

            if (!String.IsNullOrEmpty(model.CUNE))
            {
                var payrollByCUNE = payrollTableManager.FindGlobalPayrollByCUNE<GlobalDocPayroll>(model.CUNE);
                if (payrollByCUNE != null) this.PayrollList.Add(payrollByCUNE);
            }
            else if (!String.IsNullOrEmpty(model.NumeroDocumento))
            {
                DateTime date = DateTime.Now;
                List<GlobalDataDocument> listGlobalDataDocument = await CosmosDBService.Instance(date).ReadDocumentByReceiverCodeAsync(model.NumeroDocumento, date);

                foreach (GlobalDataDocument item in listGlobalDataDocument)
                {
                    GlobalDocPayroll payroll = payrollTableManager.Find<GlobalDocPayroll>(item.DocumentKey, item.SenderCode);
                    this.PayrollList.Add(payroll);
                }
            }
            else
            {
                DateTime? monthStart = null, monthEnd = null;
                double? employeeSalaryStart = null, employeeSalaryEnd = null;

                if (model.MesValidacion != "00")
                {
                    monthStart = new DateTime(DateTime.Now.Year, int.Parse(model.MesValidacion), 1, 0, 0, 0);
                    monthEnd = new DateTime(monthStart.Value.Year, int.Parse(model.MesValidacion), DateTime.DaysInMonth(monthStart.Value.Year, monthStart.Value.Month), 0, 0, 0);
                }

                if (model.RangoSalarial != "00")
                {
                    switch (model.RangoSalarial)
                    {
                        case "01":
                            employeeSalaryStart = 0;
                            employeeSalaryEnd = 1000000;
                            break;
                        case "02":
                            employeeSalaryStart = 1000000;
                            employeeSalaryEnd = 2000000;
                            break;
                        case "03":
                            employeeSalaryStart = 2000000;
                            employeeSalaryEnd = 3000000;
                            break;
                        case "04":
                            employeeSalaryStart = 3000000;
                            employeeSalaryEnd = 5000000;
                            break;
                        case "05":
                            employeeSalaryStart = 5000000;
                            employeeSalaryEnd = 10000000;
                            break;
                        case "06":
                            employeeSalaryStart = 10000000;
                            employeeSalaryEnd = 20000000;
                            break;
                        case "07":
                            employeeSalaryStart = 20000000;
                            employeeSalaryEnd = 1000000000;
                            break;
                    }
                }

                if (model.TipoDocumento == "00") model.TipoDocumento = null;

                if (model.NumeroDocumento == "00") model.NumeroDocumento = null;

                if (model.Ciudad == "00") model.Ciudad = null;
                #region Temporal para poblar tabla SQL
                var all = otherDocElecPayrollService.GetAll();

                if (all.Count == 0)
                {
                    var PayrollListAll = payrollTableManager.FindAll<GlobalDocPayroll>();

                    all = (from sql in PayrollListAll
                            select new Domain.Sql.OtherDocElecPayroll()
                            {
                                HighPensionRisk = sql.AltoRiesgoPension,
                                Environment = string.IsNullOrEmpty(sql.Ambiente) ? 0 : int.Parse(sql.Ambiente),
                                AuxTransport = sql.AuxTransporte,
                                Bank = sql.Banco,
                                BonusNS = sql.BonificacionNS,
                                BonusS = sql.BonificacionS,
                                Quantity = sql.Cantidad,
                                Ces_Paymet = sql.Ces_Pago,
                                Ces_InterestPayment = sql.Ces_PagoIntereses,
                                Ces_Percentage = sql.Ces_Porcentaje,
                                WorkerCode = sql.CodigoTrabajador,
                                Commissions = sql.Comisiones,
                                CompensationE = sql.CompensacionE,
                                CompensationO = sql.CompensacionO,
                                TotalVoucher = sql.ComprobanteTotal.ToString(),
                                Consecutive = sql.Consecutivo.ToString(),
                                CUNE = sql.CUNE,
                                CUNENov = sql.CUNENov,
                                CUNEPred = sql.CUNEPred,
                                DeductionsTotal = sql.DeduccionesTotal.ToString(),
                                StateDepartment = sql.DepartamentoEstado,
                                AccruedTotal = sql.DevengadosTotal.ToString(),
                                WorkedDays = sql.DiasTrabajados,
                                DV = string.IsNullOrEmpty(sql.DV) ? short.Parse("0") : short.Parse(sql.DV),
                                CompanyStateDepartment = sql.Emp_DepartamentoEstado,
                                CompanyAddress = sql.Emp_Direccion,
                                CompanyDV = short.Parse(sql.Emp_DV),
                                CompanyCityMunicipality = sql.Emp_MunicipioCiudad,
                                CompanyNIT = string.IsNullOrEmpty(sql.Emp_NIT) ? 0 : long.Parse(sql.Emp_NIT),
                                CompanyOtherNames = sql.Emp_OtrosNombres,
                                CompanyCountry = sql.Emp_Pais,
                                CompanySurname = sql.Emp_PrimerApellido,
                                CompanyFirstName = sql.Emp_PrimerNombre,
                                CompanyBusinessName = sql.Emp_RazonSocial,
                                CompanySecondSurname = sql.Emp_SegundoApellido,
                                EncripCUNE = sql.EncripCUNE,
                                EndDate = sql.FechaFin,
                                GenDate = sql.FechaGen,
                                GenPredDate = sql.FechaGenPred,
                                AdmissionDate = sql.FechaIngreso,
                                StartDate = sql.FechaInicio,
                                SettlementDate = sql.FechaLiquidacion,
                                EndPaymentDate = sql.FechaPagoFin,
                                StartPaymentDate = sql.FechaPagoInicio,
                                WithdrawalDate = sql.FechaRetiro,
                                PaymentDate = sql.FechasPagos,
                                Shape = sql.Forma,
                                FP_Deduction = sql.FP_Deduccion,
                                FP_Percentage = sql.FP_Porcentaje,
                                FSP_Deduction = sql.FSP_Deduccion,
                                FSP_DeductionSub = sql.FSP_DeduccionSub,
                                FSP_Percentage = sql.FSP_Porcentaje,
                                FSP_PercentageSub = sql.FSP_PorcentajeSub,
                                HED = sql.HED,
                                HEDDF = sql.HEDDF,
                                HEN = sql.HEN,
                                HENDF = sql.HENDF,
                                GenTime = sql.HoraGen,
                                HRDDF = sql.HRDDF,
                                HRN = sql.HRN,
                                HRNDF = sql.HRNDF,
                                Idiom = sql.Idioma,
                                Inc_Quantity = sql.Inc_Cantidad,
                                Inc_Payment = sql.Inc_Pago,
                                Info_DateGen = sql.Info_FechaGen,
                                WorkplaceStateDepartment = sql.LugarTrabajoDepartamentoEstado,
                                WorkplaceAddress = sql.LugarTrabajoDireccion,
                                WorkplaceMunicipalityCity = sql.LugarTrabajoMunicipioCiudad,
                                PlaceWorkCountry = sql.LugarTrabajoPais,
                                Method = sql.Metodo,
                                CityMunicipality = sql.MunicipioCiudad,
                                NIT = string.IsNullOrEmpty(sql.NIT) ? 0 : long.Parse(sql.NIT),
                                Notes = sql.Notas,
                                Novelty = sql.Novedad,
                                SerialNumber = sql.Numero,
                                AccountNumber = sql.NumeroCuenta,
                                DocumentNumber = sql.NumeroDocumento,
                                NumberPred = sql.NumeroPred,
                                OtherNames = sql.OtrosNombres,
                                Payment = sql.Pago,
                                Country = sql.Pais,
                                PayrollPeriod = sql.PeriodoNomina,
                                SerialPrefix = sql.Prefijo,
                                Surname = sql.PrimerApellido,
                                FirstName = sql.PrimerNombre,
                                Pri_Quantity = sql.Pri_Cantidad,
                                Pri_Payment = sql.Pri_Pago,
                                Pri_PaymentNS = sql.Pri_PagoNS,
                                ProvOtherNames = sql.Prov_OtrosNombres,
                                ProvSurname = sql.Prov_PrimerApellido,
                                ProvFirstName = sql.Prov_PrimerNombre,
                                Prov_CompanyName = sql.Prov_RazonSocial,
                                ProvSecondSurname = sql.Prov_SegundoApellido,
                                RetentionSource = sql.RetencionFuente,
                                ComprehensiveSalary = sql.SalarioIntegral,
                                SalaryWorked = sql.SalarioTrabajado,
                                SecondSurname = sql.SegundoApellido,
                                SoftwareID = sql.SoftwareID,
                                SoftwareSC = sql.SoftwareSC,
                                WorkerSubType = sql.SubTipoTrabajador,
                                Salary = sql.Sueldo.ToString(),
                                S_Deduction = sql.s_Deduccion,
                                S_Percentage = sql.s_Porcentaje,
                                TimeWorked = sql.TiempoLaborado,
                                CreateDate = DateTime.Parse(sql.Timestamp.ToString()),
                                ContractType = sql.TipoContrato,
                                AccountType = sql.TipoCuenta,
                                DocumentType = sql.TipoDocumento,
                                CurrencyType = sql.TipoMoneda,
                                TypeNote = sql.TipoNota == null ? short.Parse("0") : short.Parse(sql.TipoNota.ToString()),
                                WorkerType = sql.TipoTrabajador,
                                XMLType = sql.TipoXML,
                                JobCodeWorker = sql.Trab_CodigoTrabajador,
                                TRM = sql.TRM,
                                Version = sql.Version,
                                ViaticoManuAlojNS = sql.ViaticoManuAlojNS,
                                ViaticoManuAlojS = sql.ViaticoManuAlojS,
                            }).ToList();

                    var insertAll = otherDocElecPayrollService.InsertAll(all);
                }
                #endregion
                this.PayrollList = payrollTableManager.FindGlobalPayrollByMonth_EnumerationRange_EmployeeDocType_EmployeeDocNumber_FirstSurname_EmployeeSalaryRange_EmployerCity<GlobalDocPayroll>(
                    toTake, monthStart, monthEnd, model.RangoNumeracionMenor, model.RangoNumeracionMayor, model.TipoDocumento,
                    model.NumeroDocumento, model.LetraPrimerApellido, employeeSalaryStart, employeeSalaryEnd, model.Ciudad);

                var otherDocElecPayrolls = otherDocElecPayrollService.Find_ByMonth_EnumerationRange_EmployeeDocType_EmployeeDocNumber_FirstSurname_EmployeeSalaryRange_EmployerCity(toTake, monthStart, monthEnd, model.RangoNumeracionMenor, model.RangoNumeracionMayor, model.TipoDocumento,
                    model.NumeroDocumento, model.LetraPrimerApellido, employeeSalaryStart, employeeSalaryEnd, model.Ciudad);

                this.PayrollList = (from sql in otherDocElecPayrolls
                                    select new GlobalDocPayroll()
                                    {
                                        AltoRiesgoPension = sql.HighPensionRisk,
                                        Ambiente = sql.Environment.ToString(),
                                        AuxTransporte = sql.AuxTransport,
                                        Banco = sql.Bank,
                                        BonificacionNS = sql.BonusNS,
                                        BonificacionS = sql.BonusS,
                                        Cantidad = sql.Quantity,
                                        Ces_Pago = sql.Ces_Paymet,
                                        Ces_PagoIntereses = sql.Ces_InterestPayment,
                                        Ces_Porcentaje = sql.Ces_Percentage,
                                        CodigoTrabajador = sql.WorkerCode,
                                        Comisiones = sql.Commissions,
                                        CompensacionE = sql.CompensationE,
                                        CompensacionO = sql.CompensationO,
                                        ComprobanteTotal = double.Parse(sql.TotalVoucher),
                                        Consecutivo = double.Parse(sql.Consecutive),
                                        CUNE = sql.CUNE,
                                        CUNENov = sql.CUNENov,
                                        CUNEPred = sql.CUNEPred,
                                        DeduccionesTotal = double.Parse(sql.DeductionsTotal),
                                        DepartamentoEstado = sql.StateDepartment,
                                        DevengadosTotal = double.Parse(sql.AccruedTotal),
                                        DiasTrabajados = sql.WorkedDays,
                                        DV = sql.DV.ToString(),
                                        Emp_DepartamentoEstado = sql.CompanyStateDepartment,
                                        Emp_Direccion = sql.CompanyAddress,
                                        Emp_DV = sql.CompanyDV.ToString(),
                                        Emp_MunicipioCiudad = sql.CompanyCityMunicipality,
                                        Emp_NIT = sql.CompanyNIT.ToString(),
                                        Emp_OtrosNombres = sql.CompanyOtherNames,
                                        Emp_Pais = sql.CompanyCountry,
                                        Emp_PrimerApellido = sql.CompanySurname,
                                        Emp_PrimerNombre = sql.CompanyFirstName,
                                        Emp_RazonSocial = sql.CompanyBusinessName,
                                        Emp_SegundoApellido = sql.CompanySecondSurname,
                                        EncripCUNE = sql.EncripCUNE,
                                        FechaFin = sql.EndDate,
                                        FechaGen = sql.GenDate,
                                        FechaGenPred = sql.GenPredDate,
                                        FechaIngreso = sql.AdmissionDate,
                                        FechaInicio = sql.StartDate,
                                        FechaLiquidacion = sql.SettlementDate,
                                        FechaPagoFin = sql.EndPaymentDate,
                                        FechaPagoInicio = sql.StartPaymentDate,
                                        FechaRetiro = sql.WithdrawalDate,
                                        FechasPagos = sql.PaymentDate,
                                        Forma = sql.Shape,
                                        FP_Deduccion = sql.FP_Deduction,
                                        FP_Porcentaje = sql.FP_Percentage,
                                        FSP_Deduccion = sql.FSP_Deduction,
                                        FSP_DeduccionSub = sql.FSP_DeductionSub,
                                        FSP_Porcentaje = sql.FSP_Percentage,
                                        FSP_PorcentajeSub = sql.FSP_PercentageSub,
                                        HED = sql.HED,
                                        HEDDF = sql.HEDDF,
                                        HEN = sql.HEN,
                                        HENDF = sql.HENDF,
                                        HoraGen = sql.GenTime,
                                        HRDDF = sql.HRDDF,
                                        HRN = sql.HRN,
                                        HRNDF = sql.HRNDF,
                                        Idioma = sql.Idiom,
                                        Inc_Cantidad = sql.Inc_Quantity,
                                        Inc_Pago = sql.Inc_Payment,
                                        Info_FechaGen = sql.Info_DateGen,
                                        LugarTrabajoDepartamentoEstado = sql.WorkplaceStateDepartment,
                                        LugarTrabajoDireccion = sql.WorkplaceAddress,
                                        LugarTrabajoMunicipioCiudad = sql.WorkplaceMunicipalityCity,
                                        LugarTrabajoPais = sql.PlaceWorkCountry,
                                        Metodo = sql.Method,
                                        MunicipioCiudad = sql.CityMunicipality,
                                        NIT = sql.NIT.ToString(),
                                        Notas = sql.Notes,
                                        Novedad = (bool)sql.Novelty,
                                        Numero = sql.SerialNumber,
                                        NumeroCuenta = sql.AccountNumber,
                                        NumeroDocumento = sql.DocumentNumber,
                                        NumeroPred = sql.NumberPred,
                                        OtrosNombres = sql.OtherNames,
                                        Pago = sql.Payment,
                                        PartitionKey = sql.CUNE,
                                        Pais = sql.Country,
                                        PeriodoNomina = sql.PayrollPeriod,
                                        Prefijo = sql.SerialPrefix,
                                        PrimerApellido = sql.Surname,
                                        PrimerNombre = sql.FirstName,
                                        Pri_Cantidad = sql.Pri_Quantity,
                                        Pri_Pago = sql.Pri_Payment,
                                        Pri_PagoNS = sql.Pri_PaymentNS,
                                        Prov_OtrosNombres = sql.ProvOtherNames,
                                        Prov_PrimerApellido = sql.ProvSurname,
                                        Prov_PrimerNombre = sql.ProvFirstName,
                                        Prov_RazonSocial = sql.Prov_CompanyName,
                                        Prov_SegundoApellido = sql.ProvSecondSurname,
                                        RetencionFuente = sql.RetentionSource,
                                        RowKey = sql.CompanyNIT.ToString(),
                                        SalarioIntegral = (bool)sql.ComprehensiveSalary,
                                        SalarioTrabajado = sql.SalaryWorked,
                                        SegundoApellido = sql.SecondSurname,
                                        SoftwareID = sql.SoftwareID,
                                        SoftwareSC = sql.SoftwareSC,
                                        SubTipoTrabajador = sql.WorkerSubType,
                                        Sueldo = double.Parse(sql.Salary),
                                        s_Deduccion = sql.S_Deduction,
                                        s_Porcentaje = sql.S_Percentage,
                                        TiempoLaborado = sql.TimeWorked,
                                        Timestamp = (DateTimeOffset)sql.CreateDate,
                                        TipoContrato = sql.ContractType,
                                        TipoCuenta = sql.AccountType,
                                        TipoDocumento = sql.DocumentType,
                                        TipoMoneda = sql.CurrencyType,
                                        TipoNota = sql.TypeNote,
                                        TipoTrabajador = sql.WorkerType,
                                        TipoXML = sql.XMLType,
                                        Trab_CodigoTrabajador = sql.JobCodeWorker,
                                        TRM = sql.TRM,
                                        Version = sql.Version,
                                        ViaticoManuAlojNS = sql.ViaticoManuAlojNS,
                                        ViaticoManuAlojS = sql.ViaticoManuAlojS,
                                    }).ToList();
            }
        }
        private void SetViewBag_FirstSurnameData()
        {
            var globalDocPayroll = globalDocPayrollEmployeesTableManager.FindFirstSurNameByPartition<GlobalDocPayrollEmployees>("Employee");
            var firstSurnames = new List<string>();
            if (globalDocPayroll != null && globalDocPayroll.Count() > 0) firstSurnames = globalDocPayroll.Select(x => x.PrimerApellido).Distinct().ToList();
            ViewBag.FirstSurnameData = firstSurnames;
        }

        private List<CiudadModelList.CiudadModel> GetCiudadModelLists()
        {
            List<CiudadModelList.CiudadModel> result = new List<CiudadModelList.CiudadModel>();
            result.Add(new CiudadModelList.CiudadModel() { Code = "00", Name = "Todos..." });
            var cities = municipalitiesTableManager.FindByPartition<Municipalities>("Municipality");
            foreach (var city in cities.OrderBy(x => x.Name))
                result.Add(new CiudadModelList.CiudadModel() { Code = city.Code, Name = city.Name });
            return result;
        }
        #endregion

        #region Mailing

        /// <summary>
        /// Enviar notificacion email para creacion de usuario externo
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool SendMailCreate(ExternalUserViewModel model)
        {
            var emailService = new Application.EmailService();
            StringBuilder message = new StringBuilder();
            Dictionary<string, string> dic = new Dictionary<string, string>();

            message.Append("<span><b>Comunicación de servicio</b></span><br>");
            message.Append("<br> <span><b>Se ha generado una clave de acceso al Catalogo de DIAN</b></span><br>");
            message.AppendFormat("<br> Señor (a) usuario (a): {0}", model.Names);
            message.Append("<br> A continuación, se entrega la clave para realizar tramites y gestión de solicitudes recepción documentos electrónicos.");
            message.AppendFormat("<br> Clave de acceso: {0}", model.Password);

            message.Append("<br> <span style='font-size:10px;'>Te recordamos que esta dirección de correo electrónico es utilizada solamente con fines informativos. Por favor no respondas con consultas, ya que estas no podrán ser atendidas. Así mismo, los trámites y consultas en línea que ofrece la entidad se deben realizar únicamente a través del portal www.dian.gov.co</span>");

            //Nombre del documento, estado, observaciones
            dic.Add("##CONTENT##", message.ToString());

            emailService.SendEmail(model.Email, "DIAN - Creacion de Usuario Registrado", dic);

            return true;
        }

        #endregion

        string findTypeDocument(string idTypeDocument)
        {
            string findTypeDocument = string.Empty;

            switch (idTypeDocument)
            {
                case "13":
                    return "CC";
                case "22":
                    return "CE";
                case "12":
                    return "TI";
                case "41":
                    return "PS";
                case "31":
                    return "NIT";
                case "91":
                    return "NIUP";

            }

            return findTypeDocument;
        }


        List<DocumentViewPayroll> firstLoadPayroll()
        {
            List<DocumentViewPayroll> result = new List<DocumentViewPayroll>();
            List<GlobalDocPayroll> payrolls = payrollTableManager.FindAll<GlobalDocPayroll>().Where(t => t.PrimerApellido.StartsWith("A") && t.Sueldo < 1000000).ToList();
            //List<GlobalDocPayroll> payrolls = payrollTableManager.FindAll<GlobalDocPayroll>().ToList();
            foreach (var payroll in payrolls)
            {
                var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(payroll.CUNE, payroll.CUNE);
                var document = globalDocValidatorDocumentTableManager.Find<GlobalDocValidatorDocument>(documentMeta.Identifier, documentMeta.Identifier);
                if (documentMeta.Timestamp.Month == DateTime.Now.Month)
                {
                    result.Add(new DocumentViewPayroll
                    {
                        PartitionKey = payroll.PartitionKey,
                        RowKey = payroll.RowKey,
                        link = null,
                        NumeroNomina = payroll.Numero,
                        ApellidosNombre = payroll.PrimerApellido + payroll.SegundoApellido + payroll.PrimerNombre,
                        TipoDocumento = payroll.TipoDocumento,
                        NoDocumento = payroll.NumeroDocumento,
                        Salario = payroll.Sueldo,
                        Devengado = payroll.DevengadosTotal,
                        Deducido = payroll.DeduccionesTotal,
                        ValorTotal = payroll.DevengadosTotal + payroll.DeduccionesTotal,
                        MesValidacion = documentMeta.Timestamp.Month.ToString(),
                        Novedad = documentMeta.Novelty,
                        NumeroAjuste = documentMeta.DocumentReferencedKey,
                        Resultado = document.ValidationStatusName,
                        Ciudad = payroll.MunicipioCiudad
                    });
                }
            }
            return result;
        }
    }
}