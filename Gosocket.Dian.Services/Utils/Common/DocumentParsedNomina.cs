
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Services.Utils.Common
{
    public class DocumentParsedNomina
    {
        public string DocumentTypeId { get; set; }
        public string CUNE { get; set; }
        public string CUNEPred { get; set; }
        public string CodigoTrabajador { get; set; }
        public string ProveedorNIT { get; set; }
        public string ProveedorDV { get; set; }
        public string ProveedorSoftwareID { get; set; }
        public string ProveedorSoftwareSC { get; set; }
        public string SerieAndNumber { get; set; }
        public string EmpleadorDV { get; set; }
        public string EmpleadorNIT { get; set; }
        public string NumeroDocumento { get; set; }
        public string TipoNota { get; set; }
        public bool Novelty { get; set; }
        public string FechaPagoInicio { get; set; }
        public string CUNENov { get; set; }

        public static void SetValues (ref DocumentParsedNomina documentParsedNomina)
        {
            documentParsedNomina.DocumentTypeId = documentParsedNomina?.DocumentTypeId;
            documentParsedNomina.CUNE = documentParsedNomina?.CUNE?.ToString()?.ToLower();
            documentParsedNomina.CUNEPred = documentParsedNomina?.CUNEPred?.ToString()?.ToLower();
            documentParsedNomina.CodigoTrabajador = documentParsedNomina?.CodigoTrabajador;
            documentParsedNomina.ProveedorNIT = documentParsedNomina?.ProveedorNIT;
            documentParsedNomina.ProveedorDV = documentParsedNomina?.ProveedorDV;
            documentParsedNomina.ProveedorSoftwareID = documentParsedNomina?.ProveedorSoftwareID;
            documentParsedNomina.ProveedorSoftwareSC = documentParsedNomina?.ProveedorSoftwareSC?.ToString()?.ToLower();
            documentParsedNomina.SerieAndNumber = documentParsedNomina?.SerieAndNumber.ToString()?.ToUpper();
            documentParsedNomina.TipoNota = documentParsedNomina?.TipoNota;
            documentParsedNomina.Novelty = (bool)documentParsedNomina?.Novelty;
        }

        public static OtherDocElecPayroll SetGlobalDocPayrollToOtherDocElecPayroll(GlobalDocPayroll globalDocPayroll)
        {
            OtherDocElecPayroll result = new OtherDocElecPayroll()
            {
                HighPensionRisk = globalDocPayroll.AltoRiesgoPension,
                Environment = string.IsNullOrEmpty(globalDocPayroll.Ambiente) ? 0 : int.Parse(globalDocPayroll.Ambiente),
                AuxTransport = globalDocPayroll.AuxTransporte,
                Bank = globalDocPayroll.Banco,
                BonusNS = globalDocPayroll.BonificacionNS,
                BonusS = globalDocPayroll.BonificacionS,
                Quantity = globalDocPayroll.Cantidad,
                Ces_Paymet = globalDocPayroll.Ces_Pago,
                Ces_InterestPayment = globalDocPayroll.Ces_PagoIntereses,
                Ces_Percentage = globalDocPayroll.Ces_Porcentaje,
                WorkerCode = globalDocPayroll.CodigoTrabajador,
                Commissions = globalDocPayroll.Comisiones,
                CompensationE = globalDocPayroll.CompensacionE,
                CompensationO = globalDocPayroll.CompensacionO,
                TotalVoucher = globalDocPayroll.ComprobanteTotal.ToString(),
                Consecutive = globalDocPayroll.Consecutivo,
                CUNE = globalDocPayroll.CUNE,
                CUNENov = globalDocPayroll.CUNENov,
                CUNEPred = globalDocPayroll.CUNEPred,
                DeductionsTotal = globalDocPayroll.DeduccionesTotal.ToString(),
                StateDepartment = globalDocPayroll.DepartamentoEstado,
                AccruedTotal = globalDocPayroll.DevengadosTotal.ToString(),
                WorkedDays = globalDocPayroll.DiasTrabajados,
                DV = string.IsNullOrEmpty(globalDocPayroll.DV) ? short.Parse("0") : short.Parse(globalDocPayroll.DV),
                CompanyStateDepartment = globalDocPayroll.Emp_DepartamentoEstado,
                CompanyAddress = globalDocPayroll.Emp_Direccion,
                CompanyDV = short.Parse(globalDocPayroll.Emp_DV),
                CompanyCityMunicipality = globalDocPayroll.Emp_MunicipioCiudad,
                CompanyNIT = string.IsNullOrEmpty(globalDocPayroll.Emp_NIT) ? 0 : long.Parse(globalDocPayroll.Emp_NIT),
                CompanyOtherNames = globalDocPayroll.Emp_OtrosNombres,
                CompanyCountry = globalDocPayroll.Emp_Pais,
                CompanySurname = globalDocPayroll.Emp_PrimerApellido,
                CompanyFirstName = globalDocPayroll.Emp_PrimerNombre,
                CompanyBusinessName = globalDocPayroll.Emp_RazonSocial,
                CompanySecondSurname = globalDocPayroll.Emp_SegundoApellido,
                EncripCUNE = globalDocPayroll.EncripCUNE,
                EndDate = globalDocPayroll.FechaFin.ToString(),
                GenDate = globalDocPayroll.FechaGen,
                GenPredDate =  globalDocPayroll.FechaGenPred.ToString(),
                AdmissionDate = globalDocPayroll.FechaIngreso,
                StartDate = globalDocPayroll.FechaInicio.ToString(),
                SettlementDate = globalDocPayroll.FechaLiquidacion.ToString(),
                EndPaymentDate = globalDocPayroll.FechaPagoFin,
                StartPaymentDate = globalDocPayroll.FechaPagoInicio,
                WithdrawalDate = globalDocPayroll.FechaRetiro.ToString(),
                PaymentDate = globalDocPayroll.FechasPagos,
                Shape = globalDocPayroll.Forma,
                FP_Deduction = globalDocPayroll.FP_Deduccion,
                FP_Percentage = globalDocPayroll.FP_Porcentaje,
                FSP_Deduction = globalDocPayroll.FSP_Deduccion,
                FSP_DeductionSub = globalDocPayroll.FSP_DeduccionSub,
                FSP_Percentage = globalDocPayroll.FSP_Porcentaje,
                FSP_PercentageSub = globalDocPayroll.FSP_PorcentajeSub,
                HED = globalDocPayroll.HED,
                HEDDF = globalDocPayroll.HEDDF,
                HEN = globalDocPayroll.HEN,
                HENDF = globalDocPayroll.HENDF,
                GenTime = globalDocPayroll.HoraGen,
                HRDDF = globalDocPayroll.HRDDF,
                HRN = globalDocPayroll.HRN,
                HRNDF = globalDocPayroll.HRNDF,
                Idiom = globalDocPayroll.Idioma,
                Inc_Quantity = globalDocPayroll.Inc_Cantidad,
                Inc_Payment = globalDocPayroll.Inc_Pago,
                Info_DateGen = globalDocPayroll.Info_FechaGen,
                WorkplaceStateDepartment = globalDocPayroll.LugarTrabajoDepartamentoEstado,
                WorkplaceAddress = globalDocPayroll.LugarTrabajoDireccion,
                WorkplaceMunicipalityCity = globalDocPayroll.LugarTrabajoMunicipioCiudad,
                PlaceWorkCountry = globalDocPayroll.LugarTrabajoPais,
                Method = globalDocPayroll.Metodo,
                CityMunicipality = globalDocPayroll.MunicipioCiudad,
                NIT = string.IsNullOrEmpty(globalDocPayroll.NIT) ? 0 : long.Parse(globalDocPayroll.NIT),
                Notes = globalDocPayroll.Notas,
                Novelty = globalDocPayroll.Novedad,
                SerialNumber = globalDocPayroll.Numero,
                AccountNumber = globalDocPayroll.NumeroCuenta,
                DocumentNumber = globalDocPayroll.NumeroDocumento,
                NumberPred = globalDocPayroll.NumeroPred,
                OtherNames = globalDocPayroll.OtrosNombres,
                Payment = globalDocPayroll.Pago,
                Country = globalDocPayroll.Pais,
                PayrollPeriod = globalDocPayroll.PeriodoNomina,
                SerialPrefix = globalDocPayroll.Prefijo,
                Surname = globalDocPayroll.PrimerApellido,
                FirstName = globalDocPayroll.PrimerNombre,
                Pri_Quantity = globalDocPayroll.Pri_Cantidad,
                Pri_Payment = globalDocPayroll.Pri_Pago,
                Pri_PaymentNS = globalDocPayroll.Pri_PagoNS,
                ProvOtherNames = globalDocPayroll.Prov_OtrosNombres,
                ProvSurname = globalDocPayroll.Prov_PrimerApellido,
                ProvFirstName = globalDocPayroll.Prov_PrimerNombre,
                Prov_CompanyName = globalDocPayroll.Prov_RazonSocial,
                ProvSecondSurname = globalDocPayroll.Prov_SegundoApellido,
                RetentionSource = globalDocPayroll.RetencionFuente,
                ComprehensiveSalary = globalDocPayroll.SalarioIntegral,
                SalaryWorked = globalDocPayroll.SalarioTrabajado,
                SecondSurname = globalDocPayroll.SegundoApellido,
                SoftwareID = globalDocPayroll.SoftwareID,
                SoftwareSC = globalDocPayroll.SoftwareSC,
                WorkerSubType = globalDocPayroll.SubTipoTrabajador,
                Salary = globalDocPayroll.Sueldo.ToString(),
                S_Deduction = globalDocPayroll.s_Deduccion,
                S_Percentage = globalDocPayroll.s_Porcentaje,
                TimeWorked = globalDocPayroll.TiempoLaborado,
                CreateDate = DateTime.Parse(globalDocPayroll.Timestamp.ToString()),
                ContractType = globalDocPayroll.TipoContrato,
                AccountType = globalDocPayroll.TipoCuenta,
                DocumentType = globalDocPayroll.TipoDocumento,
                CurrencyType = globalDocPayroll.TipoMoneda,
                TypeNote = globalDocPayroll.TipoNota == null ? short.Parse("0") : short.Parse(globalDocPayroll.TipoNota.ToString()),
                WorkerType = globalDocPayroll.TipoTrabajador,
                XMLType = globalDocPayroll.TipoXML,
                JobCodeWorker = globalDocPayroll.Trab_CodigoTrabajador,
                TRM = globalDocPayroll.TRM,
                Version = globalDocPayroll.Version,
                ViaticoManuAlojNS = globalDocPayroll.ViaticoManuAlojNS,
                ViaticoManuAlojS = globalDocPayroll.ViaticoManuAlojS,
            };

            return result;
        }

        public static List<GlobalDocPayroll> SetOtherDocElecPayrollsToGlobalDocPayrolls(List<OtherDocElecPayroll> otherDocElecPayrolls)
        {
            List<GlobalDocPayroll> PayrollList = (from sql in otherDocElecPayrolls
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
                                                           Consecutivo = sql.Consecutive,
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
                                                           FechaFin = Convert.ToDateTime(sql.EndDate),
                                                           FechaGen = sql.GenDate,
                                                           FechaGenPred = Convert.ToDateTime(sql.GenPredDate),
                                                           FechaIngreso = sql.AdmissionDate,
                                                           FechaInicio = Convert.ToDateTime(sql.StartDate),
                                                           FechaLiquidacion = Convert.ToDateTime(sql.SettlementDate),
                                                           FechaPagoFin = sql.EndPaymentDate,
                                                           FechaPagoInicio = sql.StartPaymentDate,
                                                           FechaRetiro = Convert.ToDateTime(sql.WithdrawalDate),
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
            return PayrollList;
        }
    }
}
