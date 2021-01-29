﻿using Gosocket.Dian.Application;
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
using Gosocket.Dian.Services.Utils.Common;
using iTextSharp.text.pdf;
using iTextSharp.text;
using iTextSharp.tool.xml;
using System.Web;
using System.Drawing;
using Image = iTextSharp.text.Image;
using iTextSharp.tool.xml.pipeline.html;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.end;
using iTextSharp.tool.xml.parser;

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
        private readonly TableManager payrollTableManager = new TableManager("GlobalDocPayRoll");
        private readonly IRadianPdfCreationService _radianPdfCreationService;
        private readonly IRadianGraphicRepresentationService _radianGraphicRepresentationService;
        private readonly IRadianSupportDocument _radianSupportDocument;
        private readonly IQueryAssociatedEventsService _queryAssociatedEventsService;
        #region Properties


        private readonly FileManager _fileManager;
      
        #endregion
       
       
        #region Constructor

        public DocumentController(IRadianPdfCreationService radianPdfCreationService,
                                  IRadianGraphicRepresentationService radianGraphicRepresentationService,
                                  IQueryAssociatedEventsService queryAssociatedEventsService,
                                  IRadianSupportDocument radianSupportDocument, FileManager fileManager)
        {
            _radianSupportDocument = radianSupportDocument;
            _radianPdfCreationService = radianPdfCreationService;
            _radianPdfCreationService = radianPdfCreationService;
            _radianGraphicRepresentationService = radianGraphicRepresentationService;
            _queryAssociatedEventsService = queryAssociatedEventsService;
            _fileManager = fileManager;
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

        [ExcludeFilter(typeof(Authorization))]
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
       
    
     

        #region GetXmlFromStorageAsync

        /// <summary>
        /// Método de extracción del xml de la representación grafica
        /// TODO: pendiente de incorporar, hasta q se haga consulta por cufe
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns></returns>
        private async Task<byte[]> GetXmlFromStorageAsync(string trackId)
        {
            var TableManager = new TableManager("GlobalDocValidatorRuntime");
            var documentStatusValidation = TableManager.Find<GlobalDocValidatorRuntime>(trackId, "UPLOAD");
            if (documentStatusValidation == null)
                return null;

            var fileManager = new FileManager();
            var container = $"global";
            var fileName = $"docvalidator/{documentStatusValidation.Category}/{documentStatusValidation.Timestamp.Date.Year}/{documentStatusValidation.Timestamp.Date.Month.ToString().PadLeft(2, '0')}/{trackId}.xml";
            var xmlBytes = await fileManager.GetBytesAsync(container, fileName);

            return xmlBytes;
        }

        #endregion

        #region TemplateGlobalMapping

        private StringBuilder TemplateGlobalMappingNomina(StringBuilder template, XmlParseNomina dataValues)
        {
            //Set Variables
            DateTime expeditionDate = DateTime.Now;



            template = template.Replace("{SupportDocumentNumber}", dataValues.globalDocPayrolls.NumeroDocumento.ToString());
            template = template.Replace("{Cune}", dataValues.globalDocPayrolls.CUNE);
            template = template.Replace("{EmissionDate}", dataValues.globalDocPayrolls.FechaGen.ToString());
            template = template.Replace("{PaisType}", dataValues.globalDocPayrolls.Pais.ToString());
            template = template.Replace("{CityType}", dataValues.globalDocPayrolls.MunicipioCiudad.ToString());
            template = template.Replace("{DepartamentoType}", dataValues.globalDocPayrolls.DepartamentoEstado.ToString());

            // Seller Data
            template = template.Replace("{Nit}", dataValues.globalDocPayrolls.NIT.ToString());
            template = template.Replace("{DirType}", dataValues.globalDocPayrolls.LugarTrabajoDireccion.ToString());
            template = template.Replace("{PaisType}", dataValues.globalDocPayrolls.Pais.ToString());
            template = template.Replace("{DepType}", dataValues.globalDocPayrolls.DepartamentoEstado.ToString());
            template = template.Replace("{MunType}", dataValues.globalDocPayrolls.LugarTrabajoMunicipioCiudad.ToString());
            template = template.Replace("{CelType}", dataValues.globalDocPayrolls.Celular.ToString());

            // Employer Data
            template = template.Replace("{NitEmp}", dataValues.globalDocPayrolls.Emp_NIT.ToString());
            template = template.Replace("{SocialType}", dataValues.globalDocPayrolls.Emp_RazonSocial.ToString());
            template = template.Replace("{DirTypeEmp}", dataValues.globalDocPayrolls.Emp_Direccion.ToString());
            template = template.Replace("{PaisTypeEmp}", dataValues.globalDocPayrolls.Emp_Pais.ToString());
            template = template.Replace("{DepTypeEmp}", dataValues.globalDocPayrolls.Emp_DepartamentoEstado.ToString());
            template = template.Replace("{MunTypeEmp}", dataValues.globalDocPayrolls.Emp_MunicipioCiudad.ToString());
            template = template.Replace("{CelTypeEmp}", dataValues.globalDocPayrolls.Emp_Celular.ToString());
            template = template.Replace("{NomTypeEmp}", dataValues.globalDocPayrolls.PrimerNombre.ToString());
            template = template.Replace("{AreaTypeEmp}", dataValues.globalDocPayrolls.NombreArea.ToString());
            template = template.Replace("{CodAreaTypeEmp}", dataValues.globalDocPayrolls.CodigoArea.ToString());
            template = template.Replace("{CargoType}", dataValues.globalDocPayrolls.NombreCargo.ToString());
            template = template.Replace("{CodCargo}", dataValues.globalDocPayrolls.CodigoCargo.ToString());
            template = template.Replace("{FrecuencyNomina}", dataValues.globalDocPayrolls.PeriodoNomina.ToString());
            template = template.Replace("{DateEmpIngType}", dataValues.globalDocPayrolls.FechaIngreso.ToString());
            template = template.Replace("{AntType}", dataValues.globalDocPayrolls.TiempoLaborado.ToString());
            template = template.Replace("{TConType}", dataValues.globalDocPayrolls.TipoContrato.ToString());
            template = template.Replace("{TimeWorkTypeEmp}", dataValues.globalDocPayrolls.TiempoLaborado.ToString());
            template = template.Replace("{DatePayType}", dataValues.globalDocPayrolls.FechaPagoFin.ToString());
            template = template.Replace("{SalaryType}", dataValues.globalDocPayrolls.Salario.ToString());
            template = template.Replace(" {SalaryIntegralType}", dataValues.globalDocPayrolls.SalarioIntegral.ToString());

            // Acquirer Data
            template = template.Replace("{QRCode}", dataValues.globalDocPayrolls.Pago.ToString());
            template = template.Replace("{PayType}", dataValues.globalDocPayrolls.Pago.ToString());
            template = template.Replace("{CoinType}", dataValues.globalDocPayrolls.TipoMoneda.ToString());
            template = template.Replace("{BankType}", dataValues.globalDocPayrolls.Banco.ToString());
            template = template.Replace("{LibraryType}", dataValues.globalDocPayrolls.TipoCuenta.ToString());
            template = template.Replace("{NumberLibraryType}", dataValues.globalDocPayrolls.NumeroCuenta.ToString());
            template = template.Replace("{TotalDevType}", dataValues.globalDocPayrolls.devengadosTotal.ToString());
            template = template.Replace("{TotalDedType}", dataValues.globalDocPayrolls.deduccionesTotal.ToString());
           
            // ToTal Advances
            template = template.Replace("{NumNomType}", dataValues.globalDocPayrolls.Numero.ToString());
            template = template.Replace("{DateGenType}", dataValues.globalDocPayrolls.FechaGen.ToString());
            template = template.Replace("{ComTotalType}", dataValues.globalDocPayrolls.comprobanteTotal.ToString());

            // ToTal Retentions
            template = template.Replace("{RetentionNumber}", dataValues.globalDocPayrolls.NumeroDocumento.ToString());
            template = template.Replace("{RetentionAmount}", dataValues.globalDocPayrolls.NumeroDocumento.ToString());

            return template;
        }

        #endregion

        #region SplitAndSum

        private double SplitAndSum(string concateField)
        {
            // TotalDiscountsDetail
            var aux = concateField.Split('|');
            double fieldValue = 0;

            foreach (var dataField in aux)
            {
                if (!string.IsNullOrEmpty(dataField))
                {
                    fieldValue += double.Parse(dataField, CultureInfo.InvariantCulture);
                }
            }
            return fieldValue;
        }

        #endregion

        public ActionResult ExportPDF()
        {
            var model = new PayrollViewModel();

            //GetExportDocumentTasks(ref model);
            loadData(ref model);
            model.Payrolls = firstLoadPayroll();
            return View(model);
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> ExportPDF(PayrollViewModel model)
        {
            if(String.IsNullOrEmpty(model.NumeroDocumento))
            {
                int contadorValidaciones = 0;
                if (!String.IsNullOrEmpty(model.CUNE))
                    contadorValidaciones++;
                if (model.LetraPrimerApellido != "00")
                    contadorValidaciones++;
                if (Int32.Parse(model.MesValidacion) != 0)
                    contadorValidaciones++;
                if (!String.IsNullOrEmpty(model.RangoNumeracionMenor) && !String.IsNullOrEmpty(model.RangoNumeracionMayor))
                    contadorValidaciones++;
                if (!String.IsNullOrEmpty(model.Ciudad))
                    contadorValidaciones++;
                if (model.TipoDocumento != "00")
                    contadorValidaciones++;
                if (model.RangoSalarial != "00")
                    contadorValidaciones++;
                if (contadorValidaciones <= 3)
                {
                    model.Mensaje = "Debe seleccionar al menos 3 filtros o el Numero de Documento";
                    loadData(ref model);
                    return View(model);
                }
                else
                    model.Mensaje = string.Empty;
            }
            else
            {
                model.Mensaje = string.Empty;
            }
            List<GlobalDocPayroll> resultPayroll = new List<GlobalDocPayroll>();
            if(!String.IsNullOrEmpty(model.CUNE))
            {
                resultPayroll = payrollTableManager.FindAll<GlobalDocPayroll>().Where(t => t.CUNE == model.CUNE).ToList();
            }
            else
            {
                resultPayroll = payrollTableManager.FindAll<GlobalDocPayroll>().ToList();
            }
            if (model.LetraPrimerApellido != "00")
            {
                string letra = LetraModel.List().Where(r => r.Code == model.LetraPrimerApellido).FirstOrDefault().Name;
                resultPayroll = resultPayroll.Where(t => t.PrimerApellido.StartsWith(letra)).ToList();
            }
            List<DocumentViewPayroll> result = new List<DocumentViewPayroll>();
            if(Int32.Parse(model.MesValidacion)!=0)
            {
                foreach (var payroll in resultPayroll)
                {
                    var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(payroll.CUNE, payroll.CUNE);
                    if(documentMeta.Timestamp.Month == Int32.Parse(model.MesValidacion))
                    {
                        var document = globalDocValidatorDocumentTableManager.Find<GlobalDocValidatorDocument>(documentMeta.Identifier, documentMeta.Identifier);
                        result.Add(new DocumentViewPayroll
                        {
                            PartitionKey = payroll.PartitionKey,
                            RowKey = payroll.RowKey,
                            link = null,
                            NumeroNomina = payroll.Numero,
                            ApellidosNombre = payroll.PrimerApellido + payroll.SegundoApellido + payroll.PrimerNombre,
                            TipoDocumento = payroll.TipoDocumento,
                            NoDocumento = payroll.NumeroDocumento,
                            Salario = payroll.Salario,
                            Devengado = payroll.devengadosTotal,
                            Deducido = payroll.deduccionesTotal,
                            ValorTotal = payroll.devengadosTotal + payroll.deduccionesTotal,
                            MesValidacion = documentMeta.Timestamp.Month.ToString(),
                            Novedad = documentMeta.Novelty,
                            NumeroAjuste = documentMeta.DocumentReferencedKey,
                            Resultado = document.ValidationStatusName,
                            Ciudad = payroll.MunicipioCiudad
                        });
                    }
                }

            }
            else
            {
                foreach (var payroll in resultPayroll)
                {
                    var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(payroll.CUNE, payroll.CUNE);
                    var document = globalDocValidatorDocumentTableManager.Find<GlobalDocValidatorDocument>(documentMeta.Identifier, documentMeta.Identifier);
                    result.Add(new DocumentViewPayroll
                    {
                        PartitionKey = payroll.PartitionKey,
                        RowKey = payroll.RowKey,
                        link = null,
                        NumeroNomina = payroll.Numero,
                        ApellidosNombre = payroll.PrimerApellido + payroll.SegundoApellido + payroll.PrimerNombre,
                        TipoDocumento = payroll.TipoDocumento,
                        NoDocumento = payroll.NumeroDocumento,
                        Salario = payroll.Salario,
                        Devengado = payroll.devengadosTotal,
                        Deducido = payroll.deduccionesTotal,
                        ValorTotal = payroll.devengadosTotal + payroll.deduccionesTotal,
                        MesValidacion = documentMeta.Timestamp.Month.ToString(),
                        Novedad = documentMeta.Novelty,
                        NumeroAjuste = documentMeta.DocumentReferencedKey,
                        Resultado = document.ValidationStatusName,
                        Ciudad = payroll.MunicipioCiudad
                    });
                }
            }
            if (!String.IsNullOrEmpty(model.RangoNumeracionMenor) && !String.IsNullOrEmpty(model.RangoNumeracionMayor))
            {
                result = result.Where(t => Int32.Parse(t.NumeroNomina) >= Int32.Parse(model.RangoNumeracionMenor) && Int32.Parse(t.NumeroNomina) <= Int32.Parse(model.RangoNumeracionMayor)).ToList();
            }
            if (!String.IsNullOrEmpty(model.NumeroDocumento))
            {
                result = result.Where(t => t.NoDocumento == model.NumeroDocumento).ToList();
            }
            if (model.Ciudad != "00")
            {
                string ciudad = new CiudadModelList().List().Where(r => r.Code == model.Ciudad).FirstOrDefault().Name;
                result = result.Where(t => t.Ciudad == ciudad).ToList();
            }
            if (model.TipoDocumento != "00")
            {
                string tipoDocumento = TipoDocumentoModel.List().Where(r => r.Code == model.TipoDocumento).FirstOrDefault().Name;
                result = result.Where(t => t.TipoDocumento == tipoDocumento).ToList();
            }
            if(model.RangoSalarial != "00")
            {
                switch(model.RangoSalarial)
                {
                    case "01":
                        result = result.Where(t => t.Salario !=null && Int32.Parse(t.Salario) <= 1000000).ToList();
                        break;
                    case "02":
                        result = result.Where(t => t.Salario != null && Int32.Parse(t.Salario) > 1000000 && Int32.Parse(t.Salario) <= 2000000).ToList();
                        break;
                    case "03":
                        result = result.Where(t => t.Salario != null && Int32.Parse(t.Salario) > 2000000 && Int32.Parse(t.Salario) <= 3000000).ToList();
                        break;
                    case "04":
                        result = result.Where(t => t.Salario != null && Int32.Parse(t.Salario) > 3000000 && Int32.Parse(t.Salario) <= 5000000).ToList();
                        break;
                    case "05":
                        result = result.Where(t => t.Salario != null && Int32.Parse(t.Salario) > 5000000 && Int32.Parse(t.Salario) <= 10000000).ToList();
                        break;
                    case "06":
                        result = result.Where(t => t.Salario != null && Int32.Parse(t.Salario) > 10000000 && Int32.Parse(t.Salario) <= 20000000).ToList();
                        break;
                    case "07":
                        result = result.Where(t => t.Salario != null && Int32.Parse(t.Salario) > 20000000).ToList();
                        break;
                }
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
            model.Payrolls = result;
            loadData(ref model);
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
            return Json(base64EncodedPdf, JsonRequestBehavior.AllowGet);
        }

        [ExcludeFilter(typeof(Authorization))]
        public async Task<JsonResult> PrintGraphicRepresentation(string cufe)
        {
            byte[] pdfDocument = await _radianGraphicRepresentationService.GetPdfReport(cufe);
            String base64EncodedPdf = Convert.ToBase64String(pdfDocument);
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

            DocumentViewModel document = new DocumentViewModel();

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
                            if (eventVerification != null && (eventVerification.ValidationStatus == 0 || eventVerification.ValidationStatus == 1 || eventVerification.ValidationStatus == 10))
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

            int lastEventCode = int.Parse(events.Where(ev => !ev.Code.Equals($"0{(int)EventStatus.Avales}") 
                    &&  !ev.Code.Equals($"0{(int)EventStatus.Mandato}") 
                    &&  !ev.Code.Equals($"0{(int)EventStatus.ValInfoPago}") 
                    &&  !ev.Code.Equals($"0{(int)EventStatus.TerminacionMandato}")).OrderBy(t => t.Date).Last().Code);

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

            message.Append("<span style='font-size:24px;'><b>Comunicación de servicio</b></span></br>");
            message.Append("</br> <span style='font-size:18px;'><b>Se ha generado una clave de acceso al Catalogo de DIAN</b></span></br>");
            message.AppendFormat("</br> Señor (a) usuario (a): {0}", model.Names);
            message.Append("</br> A continuación, se entrega la clave para realizar tramites y gestión de solicitudes recepción documentos electrónicos.");
            message.AppendFormat("</br> Clave de acceso: {0}", model.Password);

            message.Append("</br> <span style='font-size:10px;'>Te recordamos que esta dirección de correo electrónico es utilizada solamente con fines informativos. Por favor no respondas con consultas, ya que estas no podrán ser atendidas. Así mismo, los trámites y consultas en línea que ofrece la entidad se deben realizar únicamente a través del portal www.dian.gov.co</span>");

            //Nombre del documento, estado, observaciones
            dic.Add("##CONTENT##", message.ToString());

            emailService.SendEmail(model.Email, "DIAN - Creacion de Usuario Registrado", dic);

            return true;
        }

        #endregion

        List<DocumentViewPayroll> firstLoadPayroll()
        {
            List<DocumentViewPayroll> result = new List<DocumentViewPayroll>();
            List<GlobalDocPayroll> payrolls = payrollTableManager.FindAll<GlobalDocPayroll>().Where(t=>t.PrimerApellido.StartsWith("A") && t.Salario != null && Int32.Parse(t.Salario)<1000000).ToList();
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
                        Salario = payroll.Salario,
                        Devengado = payroll.devengadosTotal,
                        Deducido = payroll.deduccionesTotal,
                        ValorTotal = payroll.devengadosTotal + payroll.deduccionesTotal,
                        MesValidacion = documentMeta.Timestamp.Month.ToString(),
                        Novedad = documentMeta.Novelty,
                        NumeroAjuste = documentMeta.DocumentReferencedKey,
                        Resultado = document.ValidationStatusName,
                        Ciudad = payroll.MunicipioCiudad
                    }) ;
                }
            }
            return result;
        }

        void loadData(ref PayrollViewModel model)
        {
            model.LetrasPrimerApellido = LetraModel.List();
            model.TiposDocumento = TipoDocumentoModel.List();
            model.RangosSalarial = RangoSalarialModel.List();
            model.MesesValidacion = MesModel.List();
            model.Ordenadores = OrdenarModel.List();
            model.Ciudades = new CiudadModelList().List();
        }
    }
}