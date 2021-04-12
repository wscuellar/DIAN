using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Drawing;
using System.Text;

namespace Gosocket.Dian.Application
{
    public class RadianPayrollGraphicRepresentationService : IRadianPayrollGraphicRepresentationService
    {
        #region [ properties ]

        protected IQueryAssociatedEventsService _queryAssociatedEventsService;
        private readonly FileManager _fileManager;
        private readonly TableManager _countryTableManager = new TableManager("Country", ConfigurationManager.GetValue("GlobalBillerStorage"));
        private readonly TableManager _departmentByCodeTableManager = new TableManager("DepartmentByCode", ConfigurationManager.GetValue("GlobalBillerStorage"));
        private readonly TableManager _municipalityByCodeTableManager = new TableManager("MunicipalityByCode", ConfigurationManager.GetValue("GlobalBillerStorage"));

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

            // Datos del documento
            template = template.Replace("{Cune}", this.GetValueFormatToTemplate(model.CUNE));
            template = template.Replace("{PayrollNumber}", this.GetValueFormatToTemplate(model.Numero));
            template = template.Replace("{Country}", this.GetCountryName(model.Pais));
            template = template.Replace("{GenerationPeriod}", this.GetValueFormatToTemplate((model.FechaGen.HasValue) ? model.FechaGen.Value.ToString("yyyy-MM-dd") : string.Empty)  + this.GetValueFormatToTemplate((model.FechaLiquidacion.HasValue) ? " hasta " + model.FechaLiquidacion.Value.ToString("yyyy-MM-dd") : string.Empty)); //...
            template = template.Replace("{City}", this.GetMunicipalityName(model.MunicipioCiudad));
            template = template.Replace("{Departament}", this.GetDepartmentName(model.DepartamentoEstado));

            // Datos del empleador
            template = template.Replace("{EmployerSocialReason}", this.GetValueFormatToTemplate(model.Emp_RazonSocial));
            template = template.Replace("{EmployerCountry}", this.GetCountryName(model.Emp_Pais));
            template = template.Replace("{EmployerDepartament}", this.GetDepartmentName(model.Emp_DepartamentoEstado));

            template = template.Replace("{EmployerNIT}", this.GetValueFormatToTemplate(model.Emp_NIT));
            template = template.Replace("{EmployerAddress}", this.GetValueFormatToTemplate(model.Emp_Direccion));
            template = template.Replace("{EmployerMunicipality}", this.GetMunicipalityName(model.Emp_MunicipioCiudad));

            // Datos del empleado
            template = template.Replace("{EmployeeDocumentType}", this.GetValueFormatToTemplate(model.TipoDocumento));
            template = template.Replace("{EmployeeDocumentNumber}", this.GetValueFormatToTemplate(model.NumeroDocumento));
            template = template.Replace("{EmployeeName}", this.GetValueFormatToTemplate(
                                                                this.BuildEmployeeName(model.PrimerNombre,
                                                                model.OtrosNombres,
                                                                model.PrimerApellido,
                                                                model.SegundoApellido)));
            template = template.Replace("{EmployeeCode}", this.GetValueFormatToTemplate(model.Trab_CodigoTrabajador));
            template = template.Replace("{EmployeeCountry}", this.GetCountryName(model.LugarTrabajoPais));
            template = template.Replace("{EmployeeDepartament}", this.GetDepartmentName(model.LugarTrabajoDepartamentoEstado));
            template = template.Replace("{EmployeeMunicipality}", this.GetMunicipalityName(model.LugarTrabajoMunicipioCiudad));
            template = template.Replace("{EmployeeAddress}", this.GetValueFormatToTemplate(model.LugarTrabajoDireccion));
            template = template.Replace("{EmployeePayrollPeriod}", this.GetValueFormatToTemplate(model.PeriodoNomina));
            template = template.Replace("{EmployeeEntryDate}", this.GetValueFormatToTemplate((model.FechaIngreso.HasValue) ? model.FechaIngreso.Value.ToString("yyyy-MM-dd") : string.Empty));
            template = template.Replace("{EmployeePaymentDate}", this.GetValueFormatToTemplate((model.FechaPagoFin.HasValue) ? model.FechaPagoFin.Value.ToString("yyyy-MM-dd") : string.Empty)); //...
            template = template.Replace("{EmployeeAntique}", this.GetTotalTimeWorkedFormatted(model.FechaIngreso.Value, model.FechaPagoFin.Value));
            template = template.Replace("{EmployeeContractType}", this.GetValueFormatToTemplate(model.TipoContrato));
            template = template.Replace("{EmployeeSettlementPeriod}", this.GetValueFormatToTemplate((model.FechaLiquidacion.HasValue) ? model.FechaLiquidacion.Value.ToString("yyyy-MM-dd") : string.Empty));
            template = template.Replace("{EmployeeTimeWorked}", this.GetValueFormatToTemplate(model.TiempoLaborado));
            template = template.Replace("{EmployeePaymentDate}", this.GetValueFormatToTemplate((model.FechaPagoFin.HasValue) ? model.FechaPagoFin.Value.ToString("yyyy-MM-dd") : string.Empty));
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
            template = template.Replace("{Auxilios}", this.GetMonetaryValueFormatToTemplate(""));
            template = template.Replace("{HuelgasLegales}", this.GetMonetaryValueFormatToTemplate(""));
            template = template.Replace("{OtrosConceptos}", this.GetMonetaryValueFormatToTemplate(""));
            template = template.Replace("{BonoEPCTVs}", this.GetMonetaryValueFormatToTemplate(""));
            template = template.Replace("{Dotación}", this.GetMonetaryValueFormatToTemplate(""));
            template = template.Replace("{ApoyoSost}", this.GetMonetaryValueFormatToTemplate(""));
            template = template.Replace("{Teletrabajo}", this.GetMonetaryValueFormatToTemplate(""));
            template = template.Replace("{BonifRetiro}", this.GetMonetaryValueFormatToTemplate(""));
            template = template.Replace("{Indemnización}", this.GetMonetaryValueFormatToTemplate(""));

            // Deducciones
            template = template.Replace("{Health}", this.GetMonetaryValueFormatToTemplate(model.s_Deduccion));
            template = template.Replace("{Pension}", this.GetMonetaryValueFormatToTemplate(model.FP_Deduccion));
            template = template.Replace("{Retefuent}", this.GetMonetaryValueFormatToTemplate(model.RetencionFuente));
            template = template.Replace("{EmployeeFund}", this.GetMonetaryValueFormatToTemplate(model.FSP_Deduccion));
            template = template.Replace("{Sindicatos}", this.GetMonetaryValueFormatToTemplate(""));
            template = template.Replace("{Sanciones}", this.GetMonetaryValueFormatToTemplate(""));
            template = template.Replace("{Libranzas}", this.GetMonetaryValueFormatToTemplate(""));
            template = template.Replace("{ICA}", this.GetMonetaryValueFormatToTemplate(""));
            template = template.Replace("{AFC}", this.GetMonetaryValueFormatToTemplate(model.AFC));
            template = template.Replace("{Cooperativa}", this.GetMonetaryValueFormatToTemplate(""));
            template = template.Replace("{EmbargoFiscal}", this.GetMonetaryValueFormatToTemplate(""));
            template = template.Replace("{PlanComplementarios}", this.GetMonetaryValueFormatToTemplate(""));
            template = template.Replace("{Educación}", this.GetMonetaryValueFormatToTemplate(""));
            template = template.Replace("{Reintegro}", this.GetMonetaryValueFormatToTemplate(""));
            template = template.Replace("{Deuda}", this.GetMonetaryValueFormatToTemplate(""));

            // TOTAL DEDUCCIONES
            template = template.Replace("{Letters}", this.GetTotalInLetters(model.ComprobanteTotal));
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
            template = template.Replace("{DocumentGenerated}", this.GetValueFormatToTemplate(expeditionDate.ToString("yyyy-MM-dd")));

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

        private string NumberToLettersProcess(double value)
        {
            string num2Text; value = Math.Truncate(value);
            if (value == 0) num2Text = "CERO";
            else if (value == 1) num2Text = "UNO";
            else if (value == 2) num2Text = "DOS";
            else if (value == 3) num2Text = "TRES";
            else if (value == 4) num2Text = "CUATRO";
            else if (value == 5) num2Text = "CINCO";
            else if (value == 6) num2Text = "SEIS";
            else if (value == 7) num2Text = "SIETE";
            else if (value == 8) num2Text = "OCHO";
            else if (value == 9) num2Text = "NUEVE";
            else if (value == 10) num2Text = "DIEZ";
            else if (value == 11) num2Text = "ONCE";
            else if (value == 12) num2Text = "DOCE";
            else if (value == 13) num2Text = "TRECE";
            else if (value == 14) num2Text = "CATORCE";
            else if (value == 15) num2Text = "QUINCE";
            else if (value < 20) num2Text = "DIECI" + NumberToLettersProcess(value - 10);
            else if (value == 20) num2Text = "VEINTE";
            else if (value < 30) num2Text = "VEINTI" + NumberToLettersProcess(value - 20);
            else if (value == 30) num2Text = "TREINTA";
            else if (value == 40) num2Text = "CUARENTA";
            else if (value == 50) num2Text = "CINCUENTA";
            else if (value == 60) num2Text = "SESENTA";
            else if (value == 70) num2Text = "SETENTA";
            else if (value == 80) num2Text = "OCHENTA";
            else if (value == 90) num2Text = "NOVENTA";
            else if (value < 100) num2Text = NumberToLettersProcess(Math.Truncate(value / 10) * 10) + " Y " + NumberToLettersProcess(value % 10);
            else if (value == 100) num2Text = "CIEN";
            else if (value < 200) num2Text = "CIENTO " + NumberToLettersProcess(value - 100);
            else if ((value == 200) || (value == 300) || (value == 400) || (value == 600) || (value == 800)) num2Text = NumberToLettersProcess(Math.Truncate(value / 100)) + "CIENTOS";
            else if (value == 500) num2Text = "QUINIENTOS";
            else if (value == 700) num2Text = "SETECIENTOS";
            else if (value == 900) num2Text = "NOVECIENTOS";
            else if (value < 1000) num2Text = NumberToLettersProcess(Math.Truncate(value / 100) * 100) + " " + NumberToLettersProcess(value % 100);
            else if (value == 1000) num2Text = "MIL";
            else if (value < 2000) num2Text = "MIL " + NumberToLettersProcess(value % 1000);
            else if (value < 1000000)
            {
                num2Text = NumberToLettersProcess(Math.Truncate(value / 1000)) + " MIL";
                if ((value % 1000) > 0)
                {
                    num2Text = num2Text + " " + NumberToLettersProcess(value % 1000);
                }
            }
            else if (value == 1000000)
            {
                num2Text = "UN MILLON";
            }
            else if (value < 2000000)
            {
                num2Text = "UN MILLON " + NumberToLettersProcess(value % 1000000);
            }
            else if (value < 1000000000000)
            {
                num2Text = NumberToLettersProcess(Math.Truncate(value / 1000000)) + " MILLONES";
                if ((value - Math.Truncate(value / 1000000) * 1000000) > 0)
                {
                    num2Text = num2Text + " " + NumberToLettersProcess(value - Math.Truncate(value / 1000000) * 1000000);
                }
            }
            else if (value == 1000000000000) num2Text = "UN BILLON";
            else if (value < 2000000000000) num2Text = "UN BILLON " + NumberToLettersProcess(value - Math.Truncate(value / 1000000000000) * 1000000000000);
            else
            {
                num2Text = NumberToLettersProcess(Math.Truncate(value / 1000000000000)) + " BILLONES";
                if ((value - Math.Truncate(value / 1000000000000) * 1000000000000) > 0)
                {
                    num2Text = num2Text + " " + NumberToLettersProcess(value - Math.Truncate(value / 1000000000000) * 1000000000000);
                }
            }
            return num2Text;
        }

        private string GetTotalInLetters(double value)
        {
            string finalPart = " PESOS";
            var integerValue = Convert.ToInt64(Math.Truncate(value));
            var decimalValue = Convert.ToInt32(Math.Round((value - integerValue) * 100, 2));
            if (decimalValue > 0)
            {
                var decimalLetters = this.NumberToLettersProcess(Convert.ToDouble(decimalValue));
                finalPart = $" CON { decimalLetters } PESOS";
            }
            return $"{ this.NumberToLettersProcess(Convert.ToDouble(integerValue)) } { finalPart }";
        }

        private string GetCountryName(string code)
        {
            var country = this._countryTableManager.FindByCode<Country>(code);
            return (country != null) ? country.Description : string.Empty;
        }

        private string GetDepartmentName(string code)
        {
            var department = this._departmentByCodeTableManager.FindByCode<DepartmentByCode>(code);
            return (department != null) ? department.Name : string.Empty;
        }

        private string GetMunicipalityName(string code)
        {
            var municipality = this._municipalityByCodeTableManager.FindByCode<MunicipalityByCode>(code);
            return (municipality != null) ? municipality.Name : string.Empty;
        }

        #endregion

        #region [ public methods ]

        public byte[] GetPdfReport(string id, ref string documentName)
        {
            documentName = "NóminaIndividualElectrónica";
            StringBuilder template = new StringBuilder();
            var payrollModel = this.GetPayrollData(id);

            //NÓMINA REEMPLAZADA POR AJUSTE
            var documentMeta = this._queryAssociatedEventsService.DocumentValidation(payrollModel.CUNE);
            // Si es Nómina Individual y tiene DocumentReferencedKey, es porque tiene un Ajuste de Nómina
            if (int.Parse(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayroll && !string.IsNullOrWhiteSpace(documentMeta.DocumentReferencedKey))
            {
                documentName = "AjusteNóminaIndividualElectrónica";
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
            Bitmap qrCode = RadianPdfCreationService.GenerateQR($"{ ConfigurationManager.GetValue("URL_QR_NOMINA") }{ payrollModel.CUNE }");

            string ImgDataURI = IronPdf.Util.ImageToDataUri(qrCode);
            string ImgHtml = String.Format("<img class='qr-content' src='{0}'>", ImgDataURI);

            // Replace QrLabel
            template = template.Replace("{QRCode}", ImgHtml);

            // Mapping Events
            byte[] report = RadianPdfCreationService.GetPdfBytes(template.ToString(), documentName);

            return report;
        }

        #endregion
    }
}