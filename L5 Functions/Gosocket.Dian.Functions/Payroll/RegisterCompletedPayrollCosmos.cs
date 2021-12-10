using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Utils;
using Gosocket.Dian.Services.Utils.Common;
using Gosocket.Dian.Services.Utils.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Payroll
{


	public static class RegisterCompletedPayrollCosmos
	{

		private static readonly TableManager tableManagerbatchFileResult = new TableManager("GlobalBatchFileResult");

		[FunctionName("RegisterCompletedPayrollCosmos")]
		public static async Task<EventResponse> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, TraceWriter log)
		{
			log.Info("C# HTTP trigger function processed a request.");

			// Get request body
			var data = await req.Content.ReadAsAsync<RequestObject>();

			if (data == null)
				return new EventResponse { Code = "400", Message = "Request body is empty." };

			if (string.IsNullOrEmpty(data.TrackId))
				return new EventResponse { Code = "400", Message = "Please pass a trackId in the request body." };

			var response = new EventResponse
			{
				Code = ((int)EventValidationMessage.Success).ToString(),
				Message = EnumHelper.GetEnumDescription(EventValidationMessage.Success),
			};

			try
			{
				//var xmlBytes = await Utils.Utils.GetXmlFromStorageAsync("8fac76af7e319f52cebb1888df17dea3326c6230a3c6d29ff3a1a886ac685ec5fce7307ede78c10c4f2a3910a8b46169");// normal(data.TrackId);
				//var xmlParser = new XmlParseNomina(xmlBytes);

				//var xmlBytes2 = await Utils.Utils.GetXmlFromStorageAsync("61b6c9ab7427a0613ccfac440b3fde8fa68033d540390d38026ff69045b64d4753a7f788d107777c93098aecd2a5dd39");//reemplazar
				//var xmlParser2 = new XmlParseNomina(xmlBytes2);

				//var xmlBytes3 = await Utils.Utils.GetXmlFromStorageAsync("f3803f8cd0ac6bbec3424395c547dd6a83492c7d7004a9e39ce956447178be8f2bf2a1976a7f9b10baf5b76975d62f1a");//eliminar
				//var xmlParser3 = new XmlParseNomina(xmlBytes3);

				var xmlBytes = await Utils.Utils.GetXmlFromStorageAsync(data.TrackId);
				var xmlParser = new XmlParseNomina(xmlBytes);
				if (!xmlParser.Parser())
					throw new Exception(xmlParser.ParserError);

				var objNomina = xmlParser.globalDocPayrolls; //ok
															 //var objNominaR = xmlParser2.globalDocPayrolls; //ok
															 //var objNomina = xmlParser3.globalDocPayrolls;

				var account = await ApiHelpers.ExecuteRequestAsync<string>(ConfigurationManager.GetValue("SoftwareByNitUrl"), new { Nit = objNomina.Emp_NIT });

				//Consultas para Nombres
				var cosmos = new Gosocket.Dian.DataContext.CosmosDbManagerPayroll();
				var Countries = await cosmos.GetCountries();
				var Departament = await cosmos.getDepartament();
				var CoinType = await cosmos.getCoinType();
				var ContractType = await cosmos.getContractType();
				var City = await cosmos.getCity();
				var DocumentType = await cosmos.getDocumentType();
				var SubWorkerType = await cosmos.getSubWorkerType();
				var WorkerType = await cosmos.getWorkerType();
				var PeriodPayroll = await cosmos.getPeriodPayroll();
				var PaymentForm = await cosmos.getPaymentForm();
				var PaymentMethod = await cosmos.getPaymentMethod();
				var NumberingRange = await cosmos.GetNumberingRangeByTypeDocument(objNomina.Prefijo, objNomina.Consecutivo);

				var rango = await cosmos.ConsumeNumberingRange(NumberingRange.FirstOrDefault().id.ToString());


				//var numeric = await cosmos

				#region"AgregarDataPayrollALL
				var Insert = new Domain.Cosmos.Payroll_All()
				{
					PartitionKey = account,
					DocumentKey = objNomina.CUNE,
					AccountId = (account),
					Cune = objNomina.CUNE,
					PredecesorCune = objNomina.CUNEPred,
					Prefix = objNomina.Prefijo,
					Consecutive = objNomina.Consecutivo.ToString(),
					CompositeNumber = objNomina.Numero,
					DocumentTypeId = objNomina.TipoXML == "102" ? "NI" : "NA",
					DocumentTypeName = objNomina.TipoXML == "102" ? "Nomina Individual" : "Nota de Ajuste",
					SubTypeDocumentId = objNomina.TipoXML == "102" ? "SN" : objNomina.TipoNota == 2 ? "E" : "R",
					SubTypeDocumentName = objNomina.TipoXML == "102" ? "Sin Novedad" : objNomina.TipoNota == 2 ? "Nomina de ajuste" : "Reemplazar",
					DocNumberSender = objNomina.NIT,
					CompositeNameSender = objNomina.Emp_PrimerNombre + " " + objNomina.Emp_OtrosNombres + " " + objNomina.Emp_PrimerApellido + " " + objNomina.Emp_SegundoApellido,
					CodeEmployee = objNomina.CodigoTrabajador == null ? "" : objNomina.CodigoTrabajador,
					DocTypeWorker = objNomina.TipoDocumento == null ? 0 : long.Parse(objNomina.TipoDocumento),
					DocNumberWorker = objNomina.NumeroDocumento ==null?"": objNomina.NumeroDocumento,
					NameDocTypeWorker = DocumentType.Where(x => x.IdDocumentType == objNomina.TipoDocumento).FirstOrDefault().NameDocumentType,
					FirstNamerWorker = objNomina.PrimerNombre,
					SecondNameWorker = objNomina.OtrosNombres,
					LastNameWorker = objNomina.PrimerApellido,
					SecondLastNameWorker = objNomina.SegundoApellido,
					CompositeNameWorker = objNomina.PrimerNombre + " " + objNomina.OtrosNombres + " " + objNomina.PrimerApellido + " " + objNomina.SegundoApellido,
					GenerationDate = objNomina.FechaGen == null ? "" : objNomina.FechaGen.ToString(),
					InitialDate = objNomina.FechaInicio == null ? "" : (objNomina.FechaInicio.ToString()),
					FinalDate = objNomina.FechaFin == null ? "" : (objNomina.FechaFin.ToString()),
					Salary = (objNomina.SalarioTrabajado == null ? "" : objNomina.SalarioTrabajado),
					TotalAccrued = objNomina.DevengadosTotal,
					TotalDiscounts = objNomina.DeduccionesTotal,
					PaymentReceipt = objNomina.ComprobanteTotal,
					Numeration = objNomina.Numero,
					State = "Valida"
				};
				await cosmos.UpsertDocumentPayroll_All(Insert);
				#endregion

				//NominaNormal
				if (objNomina.TipoXML == "102")
				{
					//Mapeo DocumentData - SecciónDatosDocumento
					var InsertDocumentDataPayroll = new Domain.Cosmos.DocumentData()
					{

						Novelty = objNomina.Novedad.ToString(),
						NoveltyCune = objNomina.CUNENov,
						AdmissionDate = objNomina.FechaIngreso.ToString() == null ? "" : objNomina.FechaIngreso.ToString(),
						SettlementDateStartMonth = objNomina.FechaPagoInicio.ToString() == null ? "" : objNomina.FechaPagoInicio.ToString(),
						SettlementDateEndMonth = objNomina.FechaPagoFin.ToString() == null ? "" : objNomina.FechaPagoFin.ToString(),
						TimeWorkedCompany = objNomina.TiempoLaborado,
						GenerationDate = objNomina.FechaGen == null ? objNomina.FechaGenPred.ToString() : objNomina.FechaGen.ToString(),
						GenerationDateNumber = objNomina.FechaGen == null ? objNomina.FechaGenPred.ToString() : objNomina.FechaGen.ToString(),
						Language = objNomina.Idioma,
						IdPeriodPayroll = objNomina.PeriodoNomina,
						NamePeriodPayroll = PeriodPayroll.Where(x => x.IdPeriodPayroll == objNomina.PeriodoNomina).FirstOrDefault().NamePeriodPayroll,
						//Pendiente NamePeriodPayroll
						TypeCoin = objNomina.TipoMoneda,
						CompositeNameTypeCoin = CoinType.Where(x => x.IdCoinType == objNomina.TipoMoneda).FirstOrDefault().CompositeNameCoinType,
						GenerationContry = Countries.Where(x=>x.CodeAlfa2 == objNomina.Pais).FirstOrDefault().CompositeNameCountry,
						//CompositeNameTypeCoin
						Trm = objNomina.TRM,
						Rounding = "0.00", //ValorDefecto 0.00
						CodeEmployee = objNomina.CodigoTrabajador,
						IdNumberRange = NumberingRange.FirstOrDefault().IdNumberingRange.ToString(),
						NameNumberRange = NumberingRange.FirstOrDefault().Current,
						//Pendiente IdNumberRange //DatosNumeroRango
						//Pendiente NameNumberRange//DatosNumeroRango
						Prefix = objNomina.Prefijo,
						Consecutive = objNomina.Consecutivo.ToString(),
						Number = objNomina.Numero,
						IdGenerationCountry = objNomina.Pais,
						IdGenerationDepartament = objNomina.DepartamentoEstado,
						NameCompositeGenerationDepartament = Departament.Where(x => x.IdDepartament == objNomina.DepartamentoEstado).FirstOrDefault().CompositeNameDepartament,
						//Peniente NameCompositeGenerationDepartament //NombreCompuestoDepartamentoGeneracion
						IdGenerationCity = objNomina.MunicipioCiudad,
						NameCompositeGenerationCity = City.Where(x => x.IdCity == objNomina.MunicipioCiudad).FirstOrDefault().CompositeNameCity,
						//Pendiente NameCompositeGenerationCity //NombreCompuestoMunicipioGeneracion
						//Pendiente Settlementdocument //DocumentoLiquidacion-Si/No
						//Pendiente CompanyWithdrawalDate // FechaDocumentoLiquidacion-Si/No
					};

					//Mapeo Note - SecciónNotas
					var InsertNotePayroll = new Domain.Cosmos.Note()
					{

						NoteNote = objNomina.Notas, //Notas - Ajuste para [],
					};

					//Mapeo EmployerData - SecciónDatosEmpleador
					var InsertEmployerDataPayroll = new Domain.Cosmos.EmployerData()
					{
						//Pendiente IsBusinessName - Es Empresa
						BusinessName = objNomina.Emp_RazonSocial,
						NumberDocEmployeer = objNomina.Emp_NIT,
						DvEmployeer = objNomina.Emp_DV,
						LastName = objNomina.Emp_PrimerApellido,
						SecondLastName = objNomina.Emp_SegundoApellido,
						FirstName = objNomina.Emp_PrimerNombre,
						//Pendiente SecondName - SegundoNombre
						NameCompositeEmployer = objNomina.Emp_OtrosNombres,
						AddressEmployer = objNomina.Emp_Direccion,
						IdCountryEmployer = objNomina.Emp_Pais,
						//Pendiente ContryEmployeer
						IdDepartamentEmployer = objNomina.Emp_DepartamentoEstado,
						NameDepartamentEmployeer = Departament.Where(x => x.IdDepartament == objNomina.Emp_DepartamentoEstado).FirstOrDefault().CompositeNameDepartament,
						//Pendiente NameDepartamentEmployeer
						ContryEmployeer = Countries.Where(x => x.CodeAlfa2 == objNomina.Emp_Pais).FirstOrDefault().CompositeNameCountry,

						IdCityEmployer = objNomina.Emp_MunicipioCiudad,


						//Pendiente NameCityDepartament

					};

					//Mapeo WorkerData - SecciónDatosTrabajador
					var InsertWorkerDataPayroll = new Domain.Cosmos.WorkerData()
					{
						NumberDocWorker = objNomina.NumeroDocumento,
						DocTypeWorker = objNomina.TipoDocumento,
						NameDocTypeWorker = DocumentType.Where(x => x.IdDocumentType == objNomina.TipoDocumento).FirstOrDefault().NameDocumentType,
						LastNameWorker = objNomina.PrimerApellido,
						SecondLastNameWorker = objNomina.SegundoApellido,
						FirstNameWorker = objNomina.PrimerNombre,
						SecondNameWorker = objNomina.OtrosNombres,
						CodeWorker = objNomina.Trab_CodigoTrabajador,
						TypeWorker = objNomina.TipoTrabajador,
						NameTypeWorker = WorkerType.Where(x => x.IdWorkerType == Int32.Parse(objNomina.TipoTrabajador).ToString()).FirstOrDefault().CompositeName,
						SubTypeWorker = objNomina.SubTipoTrabajador,
						NameSubTypeWorker = SubWorkerType.Where(x => x.IdSubWorkerType == (objNomina.SubTipoTrabajador == "00" ? "0" : objNomina.SubTipoTrabajador)).FirstOrDefault().CompositeName,
						//Pendiente NameSubTypeWorker
						HighRiskPensionWorker = objNomina.AltoRiesgoPension,
						IdCountryWorker = objNomina.LugarTrabajoPais,
						ContryWorkeer = Countries.Where(x => x.CodeAlfa2 == objNomina.LugarTrabajoPais).FirstOrDefault().CompositeNameCountry,
						//Pendiente ContryWorkeer
						IdDepartamentWorker = objNomina.LugarTrabajoDepartamentoEstado,
						NameDepartamentWorker = Departament.Where(x => x.IdDepartament == objNomina.LugarTrabajoDepartamentoEstado).FirstOrDefault().CompositeNameDepartament,
						//Pendiente NameDepartamentWorker
						IdCityWorker = objNomina.LugarTrabajoMunicipioCiudad,
						NameCompositeWorker = City.Where(x => x.IdCity == objNomina.LugarTrabajoMunicipioCiudad).FirstOrDefault().CompositeNameCity,
						//Pendiente NameCompositeWorker
						AddressWorker = objNomina.LugarTrabajoDireccion,
						SalaryIntegralWorker = objNomina.SalarioIntegral,
						ContractTypeWorker = objNomina.TipoContrato,
						NameContractTypeWorker = ContractType.Where(x => x.IdContractType == objNomina.TipoContrato).FirstOrDefault().CompositeName,
						SalaryWorker = objNomina.Sueldo,
					};

					//Mapeo PaymentData - SecciónDatosPago
					var InsertPaymentDataPayroll = new Domain.Cosmos.PaymentData()
					{
						PaymentForm = objNomina.Forma,
						//Pendiente NamePaymentForm
						NamePaymentForm = PaymentForm.Where(x => x.IdPaymentForm == objNomina.Forma).FirstOrDefault().CompositeName,
						PaymentMethod = objNomina.Metodo,
						NamePaymentMethod = PaymentMethod.Where(x => x.IdPaymentMethod == objNomina.Metodo).FirstOrDefault().CompositeName,
						//Pendiente NamePaymentMethod
						Bank = objNomina.Banco,
						AccountType = objNomina.TipoCuenta,
						AccountNumber = objNomina.NumeroCuenta,
						//Pendiente PaymentDateData
					};

					//Mapeo SeccionDevengados
					var InsertBasicAccrualsDataPayroll = new Domain.Cosmos.BasicAccruals()
					{
						WorkedDays = objNomina.DiasTrabajados,
						SalaryPaid = objNomina.SalarioTrabajado,
					};

					var InsertPayroll = new Domain.Cosmos.Payroll()
					{
						//Mapeo de informacion
						PartitionKey = account,
						DocumentKey = objNomina.CUNE,
						AccountId = new Guid(account),
						Cune = objNomina.CUNE,

						DocumentData = InsertDocumentDataPayroll,
						//Notes = InsertNotePayroll,
						EmployerData = InsertEmployerDataPayroll,
						WorkerData = InsertWorkerDataPayroll,
						PaymentData = InsertPaymentDataPayroll,
					};

					await cosmos.UpsertDocumentPayroll(InsertPayroll);
				}
				//NominaAjusteReemplazar
				else if (objNomina.TipoXML == "103" && objNomina.TipoNota == 1)
				{

					var objNominaR = xmlParser.globalDocPayrolls;

					var InserNoteTypePayroll = new Domain.Cosmos.NoteTypeReplace()
					{
						NoteTypeID = (int)objNomina.TipoNota,
						NameNoteType = "1 | Reemplazar",
					};

					var InserDataDocumentReplacePayroll = new Domain.Cosmos.DataDocumentReplace()
					{
						ReplaceCUNE = objNomina.CUNENov,
						NumberReplace = objNomina.Numero,
						GenerationDate = objNomina.Info_FechaGen.ToString(),
					};

					//Mapeo DocumentData - SecciónDatosDocumento
					var InsertDocumentDataPayroll = new Domain.Cosmos.DocumentDataR()
					{
						AdmissionDate = objNomina.FechaIngreso.ToString() == null ? "" : objNomina.FechaIngreso.ToString(),
						SettlementDateStartMonth = objNomina.FechaPagoInicio.ToString() == null ? "" : objNomina.FechaPagoInicio.ToString(),
						SettlementDateEndMonth = objNomina.FechaPagoFin.ToString() == null ? "" : objNomina.FechaPagoFin.ToString(),
						TimeWorkedCompany = objNomina.TiempoLaborado,
						GenerationDate = objNomina.FechaGen == null ? objNomina.FechaGenPred.ToString() : objNomina.FechaGen.ToString(),
						GenerationDateNumber = objNomina.FechaGen == null ? objNomina.FechaGenPred.ToString() : objNomina.FechaGen.ToString(),
						Language = objNomina.Idioma,
						IdPeriodPayroll = objNomina.PeriodoNomina,
						NamePeriodPayroll = PeriodPayroll.Where(x => x.IdPeriodPayroll == objNomina.PeriodoNomina).FirstOrDefault().NamePeriodPayroll,
						TypeCoin = objNomina.TipoMoneda,
						CompositeNameTypeCoin = CoinType.Where(x => x.IdCoinType == objNomina.TipoMoneda).Count() ==0?"": CoinType.Where(x => x.IdCoinType == objNomina.TipoMoneda).FirstOrDefault().CompositeNameCoinType,
						Trm = objNomina.TRM,
						Rounding = "0.00", //ValorDefecto 0.00
						CodeEmployee = objNomina.CodigoTrabajador,
						//Pendiente IdNumberRange //DatosNumeroRango
						//Pendiente NameNumberRange//DatosNumeroRango
						Prefix = objNomina.Prefijo,
						Consecutive = objNomina.Consecutivo.ToString(),
						Number = objNomina.Numero,
						IdGenerationCountry = objNomina.Pais,
						 GenerationContry=Countries.Where(x => x.CodeAlfa2 == objNomina.Pais).FirstOrDefault().CompositeNameCountry,
						//Pendiente NombrePais
						IdGenerationDepartament = objNomina.DepartamentoEstado,
						NameCompositeGenerationDepartament = Departament.Where(x => x.IdDepartament == objNomina.DepartamentoEstado).FirstOrDefault().CompositeNameDepartament,
						//Peniente NameCompositeGenerationDepartament //NombreCompuestoDepartamentoGeneracion
						IdGenerationCity = objNomina.MunicipioCiudad,
						NameCompositeGenerationCity = City.Where(x => x.IdCity == objNomina.MunicipioCiudad).FirstOrDefault().CompositeNameCity,
						//Pendiente NameCompositeGenerationCity //NombreCompuestoMunicipioGeneracion
						//Pendiente Settlementdocument //DocumentoLiquidacion-Si/No
						//Pendiente CompanyWithdrawalDate // FechaDocumentoLiquidacion-Si/No
					};

					//Mapeo Note - SecciónNotas
					var InsertNotePayroll = new Domain.Cosmos.NoteR()
					{

						NoteNote = objNomina.Notas, //Notas - Ajuste para [],
					};

					//Mapeo EmployerData - SecciónDatosEmpleador
					var InsertEmployerDataPayroll = new Domain.Cosmos.EmployerDataR()
					{
						//Pendiente IsBusinessName - Es Empresa
						NumberDocEmployeer = objNomina.Emp_NIT,
						DvEmployeer = objNomina.Emp_DV,
						BusinessName = objNomina.Emp_RazonSocial,
						FirstName = objNomina.Emp_PrimerNombre,
						SecondName = objNomina.Emp_OtrosNombres,
						LastName = objNomina.Emp_PrimerApellido,
						SecondLastName = objNomina.Emp_SegundoApellido,
						IdCountryEmployer = objNomina.Emp_Pais,
						//Pendiente NombrePais
						IdDepartamentEmployer = objNomina.Emp_DepartamentoEstado,
						NameDepartamentEmployeer = Departament.Where(x => x.IdDepartament == objNomina.Emp_DepartamentoEstado).FirstOrDefault().CompositeNameDepartament,
						//Pendiente NameDepartamentEmployeer
						IdCityEmployer = objNomina.Emp_MunicipioCiudad,
						ContryEmployeer = Countries.Where(x => x.CodeAlfa2 == objNomina.Emp_Pais).FirstOrDefault().CompositeNameCountry,

						NameCompositeEmployer = City.Where(x => x.IdCity == objNomina.Emp_MunicipioCiudad).FirstOrDefault().CompositeNameCity,
						//Pendiente NameCityDepartament
						AddressEmployer = objNomina.Emp_Direccion,
					};

					//Mapeo WorkerData - SecciónDatosTrabajador
					var InsertWorkerDataPayroll = new Domain.Cosmos.WorkerDataR()
					{
						DocTypeWorker = objNomina.TipoDocumento,
						NameDocTypeWorker = DocumentType.Where(x => x.IdDocumentType == objNomina.TipoDocumento).FirstOrDefault().NameDocumentType,
						//Peniente nombreTipoDocumento
						NumberDocWorker = objNomina.NumeroDocumento,
						FirstNameWorker = objNomina.PrimerNombre,
						SecondNameWorker = objNomina.OtrosNombres,
						LastNameWorker = objNomina.PrimerApellido,
						SecondLastNameWorker = objNomina.SegundoApellido,
						CodeWorker = objNomina.Trab_CodigoTrabajador,
						TypeWorker = objNomina.TipoTrabajador,
						NameTypeWorker = WorkerType.Where(x => x.IdWorkerType == Int32.Parse(objNomina.TipoTrabajador).ToString()).FirstOrDefault().CompositeName,
						//Pendiente TypeWorker
						SubTypeWorker = objNomina.SubTipoTrabajador,
						NameSubTypeWorker = SubWorkerType.Where(x => x.IdSubWorkerType == (objNomina.SubTipoTrabajador == "00" ? "0" : objNomina.SubTipoTrabajador)).FirstOrDefault().CompositeName,
						//Pendiente NameSubTypeWorker
						HighRiskPensionWorker = objNomina.AltoRiesgoPension,
						ContractTypeWorker = objNomina.TipoContrato,
						NameContractTypeWorker = ContractType.Where(x => x.IdContractType == objNomina.TipoContrato).FirstOrDefault().CompositeName,
						//Pendiente NombreContrato
						SalaryIntegralWorker = objNomina.SalarioIntegral,
						SalaryWorker = objNomina.Sueldo,
						IdCountryWorker = objNomina.LugarTrabajoPais,
						ContryWorkeer =  Countries.Where(x => x.CodeAlfa2 == objNomina.LugarTrabajoPais).FirstOrDefault().CompositeNameCountry,

						//Pendiente ContryWorkeer
						IdDepartamentWorker = objNomina.LugarTrabajoDepartamentoEstado,
						NameDepartamentWorker = Departament.Where(x => x.IdDepartament == objNomina.LugarTrabajoDepartamentoEstado).FirstOrDefault().CompositeNameDepartament,
						//Pendiente NameDepartamentWorker
						IdCityWorker = objNomina.LugarTrabajoMunicipioCiudad,
						NameCompositeWorker = City.Where(x => x.IdCity == objNomina.LugarTrabajoMunicipioCiudad).FirstOrDefault().CompositeNameCity,
						//Pendiente NameCompositeWorker
						AddressWorker = objNomina.LugarTrabajoDireccion,
					};

					var InsertPayroll = new Domain.Cosmos.Payroll_Replace()
					{
						//Mapeo de informacion
						PartitionKey = account,
						DocumentKey = data.TrackId,
						AccountId = new Guid(account),
						Cune = objNomina.CUNE,

						NoteType = InserNoteTypePayroll,
						DataDocumentReplace = InserDataDocumentReplacePayroll,
						DocumentData = InsertDocumentDataPayroll,
						//Notes = InsertNotePayroll,
						EmployerData = InsertEmployerDataPayroll,
						WorkerData = InsertWorkerDataPayroll,
					};

					await cosmos.UpsertDocumentPayrollR(InsertPayroll);
				}
				//NotaAjusteEliminar
				else if (objNomina.TipoXML == "103" && objNomina.TipoNota == 2)
				{
					var InserNoteTypePayroll = new Domain.Cosmos.NoteTypeDelete()
					{
						NoteTypeID = (int)objNomina.TipoNota,
						NameNoteType = "2 | Eliminar",
					};

					var InserDataDocumentReplacePayroll = new Domain.Cosmos.DataDocumentDelete()
					{
						ReplaceCUNE = objNomina.CUNENov,
						NumberReplace = objNomina.Numero,
						GenerationDate = objNomina.Info_FechaGen.ToString(),
					};

					//Mapeo DocumentData - SecciónDatosDocumento
					var InsertDocumentDataPayroll = new Domain.Cosmos.DocumentDataDelete()
					{
						Language = objNomina.Idioma,
						//Pendiente IdNumberRange //DatosNumeroRango
						//Pendiente NameNumberRange//DatosNumeroRango
						Prefix = objNomina.Prefijo,
						Consecutive = objNomina.Consecutivo.ToString(),
						Number = objNomina.Numero,
						//Pendiente nombrePais
						IdGenerationCountry = objNomina.Pais,
						GenerationContry = Countries.Where(x => x.CodeAlfa2 == objNomina.Pais).FirstOrDefault().CompositeNameCountry,
						IdGenerationDepartament = objNomina.DepartamentoEstado,
						//Peniente NameCompositeGenerationDepartament //NombreCompuestoDepartamentoGeneracion
						NameCompositeGenerationDepartament = Departament.Where(x => x.IdDepartament == objNomina.DepartamentoEstado).FirstOrDefault().CompositeNameDepartament,
						IdGenerationCity = objNomina.MunicipioCiudad,
						NameCompositeGenerationCity = City.Where(x => x.IdCity == objNomina.MunicipioCiudad).FirstOrDefault().CompositeNameCity,
						//Pendiente NameCompositeGenerationCity //NombreCompuestoMunicipioGeneracion
					};

					//Mapeo Note - SecciónNotas
					var InsertNotePayroll = new Domain.Cosmos.NoteDelete()
					{

						NoteNote = objNomina.Notas, //Notas - Ajuste para [],
					};

					//Mapeo EmployerData - SecciónDatosEmpleador
					var InsertEmployerDataPayroll = new Domain.Cosmos.EmployerDataDelete()
					{
						//Pendiente IsBusinessName - Es Empresa
						NumberDocEmployeer = objNomina.Emp_NIT,
						DvEmployeer = objNomina.Emp_DV,
						BusinessName = objNomina.Emp_RazonSocial,
						FirstName = objNomina.Emp_PrimerNombre,
						SecondName = objNomina.Emp_OtrosNombres,
						//Pendiente SecondName - SegundoNombre
						NameCompositeEmployer = objNomina.Emp_OtrosNombres,
						LastName = objNomina.Emp_PrimerApellido,
						SecondLastName = objNomina.Emp_SegundoApellido,
						IdCountryEmployer = objNomina.Emp_Pais,
						//Pendiente ContryEmployeer
						IdDepartamentEmployer = objNomina.Emp_DepartamentoEstado,
						NameDepartamentEmployeer = Departament.Where(x => x.IdDepartament == objNomina.Emp_DepartamentoEstado).FirstOrDefault().CompositeNameDepartament,
						//Pendiente NameDepartamentEmployeer
						IdCityEmployer = objNomina.Emp_MunicipioCiudad,
						ContryEmployeer = Countries.Where(x => x.CodeAlfa2 == objNomina.Emp_Pais).FirstOrDefault().CompositeNameCountry,

						//Pendiente NameCityDepartament
						AddressEmployer = objNomina.Emp_Direccion,
					};

					var InsertPayroll = new Domain.Cosmos.Payroll_Delete()
					{
						//Mapeo de informacion
						PartitionKey = null,
						DocumentKey = null,
						AccountId = new Guid(account),
						Cune = objNomina.CUNE,

						NoteTypeDelete = InserNoteTypePayroll,
						DataDocumentDelete = InserDataDocumentReplacePayroll,
						DocumentDataDelete = InsertDocumentDataPayroll,
						//Notes = InsertNotePayroll,
						EmployerDataDelete = InsertEmployerDataPayroll,
					};

					await cosmos.UpsertDocumentPayrollE(InsertPayroll);
				}
			}
			catch (Exception ex)
			{
				log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
				response.Code = ((int)EventValidationMessage.Error).ToString();
				response.Message = ex.Message;
			}

			return response;


		}

		public class RequestObject
		{
			[JsonProperty(PropertyName = "trackId")]
			public string TrackId { get; set; }
		}
	}
}
