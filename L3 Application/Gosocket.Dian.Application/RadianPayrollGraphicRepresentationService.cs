using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Application
{
    public class RadianPayrollGraphicRepresentationService : IRadianPayrollGraphicRepresentationService
    {
        #region [ properties ]

        protected IQueryAssociatedEventsService _queryAssociatedEventsService;
        private readonly FileManager _fileManager;

        #endregion

        #region [ constructor ]

        public RadianPayrollGraphicRepresentationService(IQueryAssociatedEventsService queryAssociatedEventsService, FileManager fileManager)
        {
            this._queryAssociatedEventsService = queryAssociatedEventsService;
            this._fileManager = fileManager;
        }

        #endregion

        #region [ private methods ]

        private GlobalDocPayroll GetPayrollData(string id)
        {
            return  this._queryAssociatedEventsService.GetPayrollById(id);
        }

        private string GetValueFormatToTemplate(string value)
        {
            if (!string.IsNullOrWhiteSpace(value)) return value;
            return string.Empty;
        }

        private string GetMonetaryValueFormatToTemplate(string value)
        {
            double val = 0;
            if (!string.IsNullOrWhiteSpace(value))
            {
                val = double.Parse(value);
            }
                
            return val.ToString("C0");
        }

        private string BuildEmployeeName(string firstName, string otherNames, string firstLastname, string secondLastname)
        {
            string fullName = firstName;
            if (!string.IsNullOrWhiteSpace(otherNames)) fullName = $"{fullName} {otherNames}";
            if (!string.IsNullOrWhiteSpace(firstLastname)) fullName = $"{fullName} {firstLastname}";
            if (!string.IsNullOrWhiteSpace(secondLastname)) fullName = $"{fullName} {secondLastname}";
            return fullName;
        }

        private StringBuilder DataTemplateMapping(StringBuilder template, GlobalDocPayroll model)
        {
            //Set Variables
            DateTime expeditionDate = DateTime.Now;

            // PENDIENTES
            // Datos del documento
            template = template.Replace("{Cune}", this.GetValueFormatToTemplate(model.CUNE));
            template = template.Replace("{PayrollNumber}", this.GetValueFormatToTemplate(""));
            template = template.Replace("{Country}", this.GetValueFormatToTemplate(model.Pais));
            template = template.Replace("{GenerationPeriod}", this.GetValueFormatToTemplate(""));
            template = template.Replace("{City}", this.GetValueFormatToTemplate(model.MunicipioCiudad));
            template = template.Replace("{Departament}", this.GetValueFormatToTemplate(model.DepartamentoEstado));

            // Datos del empleador
            template = template.Replace("{EmployerSocialReason}", this.GetValueFormatToTemplate(model.Emp_RazonSocial));
            template = template.Replace("{EmployerCountry}", this.GetValueFormatToTemplate(model.Emp_Pais));
            template = template.Replace("{EmployerDepartament}", this.GetValueFormatToTemplate(model.Emp_DepartamentoEstado));

            template = template.Replace("{EmployerNIT}", this.GetValueFormatToTemplate(model.Emp_NIT));
            template = template.Replace("{EmployerAddress}", this.GetValueFormatToTemplate(model.Emp_Direccion));
            template = template.Replace("{EmployerMunicipality}", this.GetValueFormatToTemplate(model.Emp_MunicipioCiudad));

            // PENDIENTES
            // Datos del empleado
            template = template.Replace("{EmployeeDocumentType}", this.GetValueFormatToTemplate(model.TipoDocumento));
            template = template.Replace("{EmployeeDocumentNumber}", this.GetValueFormatToTemplate(model.NumeroDocumento));
            template = template.Replace("{EmployeeName}", this.GetValueFormatToTemplate(
                                                                this.BuildEmployeeName(model.PrimerNombre,
                                                                model.OtrosNombres,
                                                                model.PrimerApellido,
                                                                model.SegundoApellido)));
            template = template.Replace("{EmployeeCode}", this.GetValueFormatToTemplate(model.CodigoTrabajador));
            template = template.Replace("{EmployeeDepartament}", this.GetValueFormatToTemplate(model.LugarTrabajoDepartamentoEstado));
            template = template.Replace("{EmployeeMunicipality}", this.GetValueFormatToTemplate(model.LugarTrabajoMunicipioCiudad));
            template = template.Replace("{EmployeeMunicipality}", this.GetValueFormatToTemplate(model.LugarTrabajoMunicipioCiudad));
            template = template.Replace("{EmployeeAddress}", this.GetValueFormatToTemplate(model.LugarTrabajoDireccion));
            template = template.Replace("{EmployeePayrollPeriod}", this.GetValueFormatToTemplate(model.PeriodoNomina));
            template = template.Replace("{EmployeeEntryDate}", this.GetValueFormatToTemplate(model.FechaIngreso));
            template = template.Replace("{EmployeePaymentDate}", this.GetValueFormatToTemplate(""));
            template = template.Replace("{EmployeeAntique}", this.GetValueFormatToTemplate(""));
            template = template.Replace("{EmployeeContractType}", this.GetValueFormatToTemplate(model.TipoContrato));
            template = template.Replace("{EmployeeSettlementPeriod}", this.GetValueFormatToTemplate(""));
            template = template.Replace("{EmployeeTimeWorked}", this.GetValueFormatToTemplate(model.TiempoLaborado));
            template = template.Replace("{EmployeePaymentDate}", this.GetValueFormatToTemplate(""));
            template = template.Replace("{EmployeeSalary}", model.Sueldo.ToString("C0"));
            template = template.Replace("{EmployeeIsComprehensiveSalary}", (model.SalarioIntegral) ? "Si" : "No");

            // Detalle del documento individual de nómina electrónica
            template = template.Replace("{1 SB}", this.GetMonetaryValueFormatToTemplate(model.SalarioTrabajado));
            template = template.Replace("{HEDs}", this.GetMonetaryValueFormatToTemplate(model.HED));
            template = template.Replace("{HENs}", this.GetMonetaryValueFormatToTemplate(model.HEN));
            template = template.Replace("{HRNs}", this.GetMonetaryValueFormatToTemplate(model.HRN));
            template = template.Replace("{HEDDFs}", this.GetMonetaryValueFormatToTemplate(model.HEDDF));
            template = template.Replace("{HRDDFs}", this.GetMonetaryValueFormatToTemplate(model.HRDDF));
            template = template.Replace("{HENDFs}", this.GetMonetaryValueFormatToTemplate(model.HENDF));
            template = template.Replace("{HRNDFs}", this.GetMonetaryValueFormatToTemplate(model.HRNDF));
            template = template.Replace("{Vacaciones}", this.GetMonetaryValueFormatToTemplate(model.Pago));
            template = template.Replace("{Primas}", this.GetMonetaryValueFormatToTemplate(model.Pri_Pago));
            template = template.Replace("{Cesantias}", this.GetMonetaryValueFormatToTemplate(model.Ces_Pago));
            template = template.Replace("{Incapacidades}", this.GetMonetaryValueFormatToTemplate(model.Inc_Pago));
            template = template.Replace("{Licencias}", this.GetMonetaryValueFormatToTemplate(model.Lic_Pago));
            template = template.Replace("{Aux. Transporte}", this.GetMonetaryValueFormatToTemplate(model.AuxTransporte));
            template = template.Replace("{Bonificaciones}", this.GetMonetaryValueFormatToTemplate(model.BonificacionNS));
            template = template.Replace("{Comisiones}", this.GetMonetaryValueFormatToTemplate(model.Comisiones));
            template = template.Replace("{Compensaciones}", this.GetMonetaryValueFormatToTemplate(model.CompensacionE));

            // Deducciones
            template = template.Replace("{Health}", this.GetMonetaryValueFormatToTemplate(model.s_Deduccion));
            template = template.Replace("{Pension}", this.GetMonetaryValueFormatToTemplate(model.FP_Deduccion));
            template = template.Replace("{Retefuent}", this.GetMonetaryValueFormatToTemplate(model.RetencionFuente));
            template = template.Replace("{EmployeeFund}", this.GetMonetaryValueFormatToTemplate(model.FSP_Deduccion));

            // TOTAL DEDUCCIONES
            template = template.Replace("{PaymentFormat}", this.GetValueFormatToTemplate(model.Forma));
            template = template.Replace("{PaymentMethod}", this.GetValueFormatToTemplate(model.Metodo));
            template = template.Replace("{Bank}", this.GetValueFormatToTemplate(model.Banco));
            template = template.Replace("{AccountType}", this.GetValueFormatToTemplate(model.TipoCuenta));
            template = template.Replace("{AccountNumber}", this.GetValueFormatToTemplate(model.NumeroCuenta));
            template = template.Replace("{CurrencyType}", this.GetValueFormatToTemplate(model.TipoMoneda));
            template = template.Replace("{TotalAccrued}", model.DevengadosTotal.ToString("C0"));
            template = template.Replace("{TotalDeductions}", model.DeduccionesTotal.ToString("C0"));
            template = template.Replace("{TotalVoucher}", model.ComprobanteTotal.ToString("C0"));

            template = template.Replace("{DocumentValidated}", this.GetValueFormatToTemplate(model.Info_FechaGen));
            template = template.Replace("{DocumentGenerated}", this.GetValueFormatToTemplate(model.FechaGen));

            // PENDIENTES
            // Footer
            template = template.Replace("{AuthorizationNumber}", this.GetValueFormatToTemplate(""));
            template = template.Replace("{AuthorizedRangeFrom}", this.GetValueFormatToTemplate(""));
            template = template.Replace("{AuthorizedRangeTo}", this.GetValueFormatToTemplate(""));
            template = template.Replace("{ValidityDate}", this.GetValueFormatToTemplate(""));

            return template;
        }

        #endregion

        #region [ public methods ]

        public byte[] GetPdfReport(string id)
        {
            // Load Templates            
            StringBuilder template = new StringBuilder(_fileManager.GetText("radian-documents-templates", "RepresentacionGraficaNomina.html"));

            var payrollModel = this.GetPayrollData(id);

            // Set Variables
            Bitmap qrCode = RadianPdfCreationService.GenerateQR(TextResources.RadianReportQRCode.Replace("{CUFE}", payrollModel.CUNE));

            string ImgDataURI = IronPdf.Util.ImageToDataUri(qrCode);
            string ImgHtml = String.Format("<img class='qr-content' src='{0}'>", ImgDataURI);

            // Mapping Labels common data
            template = DataTemplateMapping(template, payrollModel);

            // Replace QrLabel
            template = template.Replace("{QRCode}", ImgHtml);

            // Mapping Events
            byte[] report = RadianPdfCreationService.GetPdfBytes(template.ToString(), "NóminaIndividualElectrónica");

            return report;
        }

        #endregion
    }
}