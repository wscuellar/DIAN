using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain.Common;
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

        private StringBuilder IndividualPayrollDataTemplateMapping(StringBuilder template, GlobalDocPayroll model)
        {
            //Set Variables
            DateTime expeditionDate = DateTime.Now;

            // PENDIENTES
            // Datos del documento
            template = template.Replace("{Cune}", this.GetValueFormatToTemplate(model.CUNE));
            template = template.Replace("{PayrollNumber}", this.GetValueFormatToTemplate(model.Numero));
            template = template.Replace("{Country}", this.GetValueFormatToTemplate(model.Pais));
            template = template.Replace("{GenerationPeriod}", this.GetValueFormatToTemplate(model.FechaGen)); //...
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
            template = template.Replace("{EmployeePaymentDate}", this.GetValueFormatToTemplate(model.FechaPagoFin)); //...
            template = template.Replace("{EmployeeAntique}", this.GetTotalTimeWorkedFormatted(DateTime.Parse(model.FechaIngreso), DateTime.Parse(model.FechaPagoFin)));
            template = template.Replace("{EmployeeContractType}", this.GetValueFormatToTemplate(model.TipoContrato));
            template = template.Replace("{EmployeeSettlementPeriod}", this.GetValueFormatToTemplate(model.FechaLiquidacion));
            template = template.Replace("{EmployeeTimeWorked}", this.GetValueFormatToTemplate(model.TiempoLaborado));
            template = template.Replace("{EmployeePaymentDate}", this.GetValueFormatToTemplate(model.FechaPagoFin));
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

            template = template.Replace("{DocumentValidated}", this.GetValueFormatToTemplate(model.Timestamp.DateTime.ToString("yyyy-MM-dd")));
            template = template.Replace("{DocumentGenerated}", this.GetValueFormatToTemplate(DateTime.Now.ToString("yyyy-MM-dd")));

            // PENDIENTES
            // Footer
            template = template.Replace("{AuthorizationNumber}", this.GetValueFormatToTemplate(""));
            template = template.Replace("{AuthorizedRangeFrom}", this.GetValueFormatToTemplate(""));
            template = template.Replace("{AuthorizedRangeTo}", this.GetValueFormatToTemplate(""));
            template = template.Replace("{ValidityDate}", this.GetValueFormatToTemplate(""));

            return template;
        }

        private StringBuilder AdjustmentIndividualPayrollDataTemplateMapping(StringBuilder template, GlobalDocPayroll model, GlobalDocValidatorDocumentMeta adjustment)
        {
            template = this.IndividualPayrollDataTemplateMapping(template, model);

            template = template.Replace("{AdjCune}", this.GetValueFormatToTemplate(adjustment.PartitionKey));
            template = template.Replace("{AdjPayrollNumber}", this.GetValueFormatToTemplate(adjustment.SerieAndNumber));
            template = template.Replace("{AdjGenerationDate}", this.GetValueFormatToTemplate(adjustment.Timestamp.DateTime.ToString("yyyy-MM-dd")));

            return template;
        }

        /// <summary>
        /// Función que realiza el cálculo del tiempo laborado en base a un rango de fechas.
        /// </summary>
        /// <param name="initialDate">Fecha inicial del rango</param>
        /// <param name="finalDate">Fecha final del rango</param>
        /// <returns>Cadena con formato '00A00M00D'</returns>
        private string GetTotalTimeWorkedFormatted(DateTime initialDate, DateTime finalDate)
        {
            const int monthsInYear = 12,
                      daysInMonth = 30;

            var totalYears = finalDate.Year - initialDate.Year;
            var totalMonths = finalDate.Month - initialDate.Month;
            var totalDays = finalDate.Day - initialDate.Day;

            if (totalYears == 0) // mismo año
            {
                if (totalMonths > 0) // diferente mes, en el mismo año
                {
                    if (totalDays < 0) // no se completaron los 30 días...se elimina 1 mes y se hace el cálculo de los días
                    {
                        totalMonths--;
                        totalDays = (daysInMonth - initialDate.Day) + finalDate.Day;
                    }
                    else
                        totalDays++; // se suma 1 día
                }
                else // mismo mes
                    totalDays++; // se suma 1 día
            }
            else
            {
                // 12 o más meses...
                if (totalMonths >= 0)
                {
                    if (totalDays < 0)
                    {
                        // no alcanza el mes completo, se elimina un mes y se sumas los días
                        if (totalMonths == 0)
                        {
                            totalYears--;
                            totalMonths = (monthsInYear - 1);
                        }
                        else
                            totalMonths--;

                        totalDays = (daysInMonth - initialDate.Day) + finalDate.Day;
                    }
                    else
                        totalDays++; // se suma 1 día
                }
                else
                {
                    // no se completan los 12 meses, se tiene que restar 1 año y sumar los meses (totalMonths)
                    totalYears--;
                    totalMonths = (monthsInYear - initialDate.Month) + finalDate.Month;

                    // no se completan los 30 días
                    if (totalDays < 0)
                    {
                        // no se completan los 30 días, se tiene que restar 1 mes y sumar los días (totalDays)
                        totalMonths--;
                        totalDays = (daysInMonth - initialDate.Day) + finalDate.Day;
                    }
                    else
                        totalDays++; // se suma 1 día
                }
            }

            // validaciones finales para ajustar unidades...
            if (totalDays == 30) // si se completan 30 días, se suma 1 mes y se reinician los días
            {
                totalMonths++;
                totalDays = 0;
            }
            if (totalMonths == 12) // si se completan 12 meses, se suma 1 año y se reinician los meses
            {
                totalYears++;
                totalMonths = 0;
            }

            string yearsStringFormatted = $"{totalYears.ToString().PadLeft(2, char.Parse("0"))}A",
                   monthsStringFormatted = $"{totalMonths.ToString().PadLeft(2, char.Parse("0"))}M",
                   daysStringFormatted = $"{totalDays.ToString().PadLeft(2, char.Parse("0"))}D";

            return $"{yearsStringFormatted}{monthsStringFormatted}{daysStringFormatted}";
        }

        #endregion

        #region [ public methods ]

        public byte[] GetPdfReport(string id)
        {
            // Load Templates            
            //StringBuilder template = new StringBuilder(_fileManager.GetText("radian-documents-templates", "RepresentacionGraficaNomina.html"));
            StringBuilder template = new StringBuilder();
            var payrollModel = this.GetPayrollData(id);

            //NÓMINA REEMPLAZADA POR AJUSTE
            var documentMeta = this._queryAssociatedEventsService.DocumentValidation(payrollModel.CUNE);
            // Si es Nómina Individual y tiene DocumentReferencedKey, es porque tiene un Ajuste de Nómina
            if (int.Parse(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayroll && !string.IsNullOrWhiteSpace(documentMeta.DocumentReferencedKey))
            {
                // Load template
                template.Append(_fileManager.GetText("radian-documents-templates", "RepresentacionGraficaNominaAjuste.html"));
                // Adjustment data...
                var adjustmentDocumentMeta = this._queryAssociatedEventsService.DocumentValidation(documentMeta.DocumentReferencedKey);
                template = this.AdjustmentIndividualPayrollDataTemplateMapping(template, payrollModel, adjustmentDocumentMeta);
            }
            else
            {
                // Load template
                template.Append(_fileManager.GetText("radian-documents-templates", "RepresentacionGraficaNomina.html"));
                // Mapping Labels common data
                template = this.IndividualPayrollDataTemplateMapping(template, payrollModel);
            }

            // Set Variables
            Bitmap qrCode = RadianPdfCreationService.GenerateQR(TextResources.RadianReportQRCode.Replace("{CUFE}", payrollModel.CUNE));

            string ImgDataURI = IronPdf.Util.ImageToDataUri(qrCode);
            string ImgHtml = String.Format("<img class='qr-content' src='{0}'>", ImgDataURI);

            // Replace QrLabel
            template = template.Replace("{QRCode}", ImgHtml);

            // Mapping Events
            byte[] report = RadianPdfCreationService.GetPdfBytes(template.ToString(), "NóminaIndividualElectrónica");

            return report;
        }

        #endregion
    }
}