using Gosocket.Dian.Application.Common;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Plugin.Functions.Cache;
using Gosocket.Dian.Plugin.Functions.Common.Encryption;
using Gosocket.Dian.Plugin.Functions.Cryptography.Verify;
using Gosocket.Dian.Plugin.Functions.Models;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Gosocket.Dian.Infrastructure.Utils;
using ContributorType = Gosocket.Dian.Domain.Common.ContributorType;
using OperationMode = Gosocket.Dian.Domain.Common.OperationMode;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;
using Gosocket.Dian.Plugin.Functions.ValidateParty;
using Gosocket.Dian.Services.Utils.Common;
using Gosocket.Dian.Plugin.Functions.SigningTime;
using Gosocket.Dian.Plugin.Functions.Event;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gosocket.Dian.Services.Utils;

namespace Gosocket.Dian.Plugin.Functions.Common
{
    public class Validator
    {
        #region Global properties
        static readonly TableManager contributorTableManager = new TableManager("GlobalContributor");
        static readonly TableManager contingencyTableManager = new TableManager("GlobalContingency");
        static readonly TableManager documentMetaTableManager = new TableManager("GlobalDocValidatorDocumentMeta");
        static readonly TableManager documentValidatorTableManager = new TableManager("GlobalDocValidatorDocument");
        static readonly TableManager numberRangeTableManager = new TableManager("GlobalNumberRange");
        static readonly TableManager tableManagerTestSetResult = new TableManager("GlobalTestSetResult");
        static readonly TableManager softwareTableManager = new TableManager("GlobalSoftware");
        static readonly TableManager typeListTableManager = new TableManager("GlobalTypeList");        
        private TableManager TableManagerGlobalDocReferenceAttorney = new TableManager("GlobalDocReferenceAttorney");
        private TableManager TableManagerGlobalAttorneyFacultity = new TableManager("GlobalAttorneyFacultity");
        private TableManager TableManagerGlobalRadianOperations = new TableManager("GlobalRadianOperations");
        private TableManager TableManagerGlobalDocRegisterProviderAR = new TableManager("GlobalDocRegisterProviderAR");
        private TableManager TableManagerGlobalOtherDocElecOperation = new TableManager("GlobalOtherDocElecOperation");
        private TableManager TableManagerGlobalTaxRate = new TableManager("GlobalTaxRate");
        private TableManager payrollTableManager = new TableManager("GlobalDocPayroll");

        readonly XmlDocument _xmlDocument;
        readonly XPathDocument _document;
        readonly XPathNavigator _navigator;
        readonly XPathNavigator _navNs;
        readonly XmlNamespaceManager _ns;
        readonly byte[] _xmlBytes;

        static readonly Regex _base64RegexPattern = new Regex(BASE64_REGEX_STRING, RegexOptions.Compiled);

        private const String BASE64_REGEX_STRING = @"^[a-zA-Z0-9\+/]*={0,3}$";

        #endregion

        #region Constructors
        public Validator()
        {
        }

        public Validator(byte[] xmlBytes)
        {
            _xmlBytes = xmlBytes;
            _xmlDocument = new XmlDocument() { PreserveWhitespace = true };
            _xmlDocument.LoadXml(Encoding.UTF8.GetString(xmlBytes));

            var xmlReader = new XmlTextReader(new MemoryStream(xmlBytes)) { Namespaces = true };

            _document = new XPathDocument(xmlReader);
            _navigator = _document.CreateNavigator();

            _navNs = _document.CreateNavigator();
            _navNs.MoveToFollowing(XPathNodeType.Element);
            IDictionary<string, string> nameSpaceList = _navNs.GetNamespacesInScope(XmlNamespaceScope.All);

            _ns = new XmlNamespaceManager(_xmlDocument.NameTable);

            foreach (var nsItem in nameSpaceList)
            {
                if (string.IsNullOrEmpty(nsItem.Key))
                    _ns.AddNamespace("sig", nsItem.Value);
                else
                    _ns.AddNamespace(nsItem.Key, nsItem.Value);
            }
            _ns.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");
        }
        #endregion


        #region Economic activity validation
        public ValidateListResponse ValidateEconomicActivity(ResponseXpathDataValue responseXpathDataValue)
        {
            //var typesList = typeListTableManager.FindByPartition<GlobalTypeList>("new-dian-ubl21");
            //var typeList = typesList.FirstOrDefault(t => t.Name == "Tipo Responsabilidad");
            //var typeListvalues = typeList.Value.Split(';');
            var typeListInstance = GetTypeListInstanceCache();
            var typeListvalues = typeListInstance.Value.Split(';');

            var response = new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAJ04", ErrorMessage = "Codigos informados corresponden a los que estan en lista." };

            //Economic ativity
            var isValid = true;
            var senderTaxLevelCodes = responseXpathDataValue.XpathsValues["SenderTaxLevelCodeXpath"].Split(';');
            foreach (var code in senderTaxLevelCodes)
                if (!typeListvalues.Contains(code))
                    return new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAJ04", ErrorMessage = "Codigos no informados o no corresponden a los que estan en lista." };
            if (isValid && senderTaxLevelCodes.Any())
                return new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAJ04", ErrorMessage = "Codigos informados corresponden a los que estan en lista." };

            return null;
        }
        #endregion

        #region Contingecy region
        public ValidateListResponse ValidateContingency()
        {
            DateTime startDate = DateTime.UtcNow;
            var response = new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "CTG01", ErrorMessage = "La fecha y hora de firmado del documento no corresponde a un período de contingencia establecido por la DIAN." };

            var contingencies = contingencyTableManager.FindAll<GlobalContingency>();

            var node = _xmlDocument.GetElementsByTagName("xades:SigningTime")[0];

            var signingDateTime = node?.InnerText;
            long signingTimeNumber = 0;
            try
            {
                var signingDate = signingDateTime.Split('T')[0];
                var signingTime = $"{signingDateTime.Split('T')[1]?.Substring(0, 6)}00";
                signingTimeNumber = long.Parse(DateTime.ParseExact($"{signingDate}T{signingTime}", "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyyMMddHHmmss"));
            }
            catch { }

            contingencies = contingencies.Where(c => !c.Deleted && c.Active).ToList();
            if (contingencies.Any(c => signingTimeNumber >= c.StartDateNumber && c.EndDateNumber >= signingTimeNumber))
            {
                response.IsValid = true;
                response.ErrorMessage = "La fecha y hora de firmado del documento corresponde a un período de contingencia establecido por la DIAN.";
            }

            response.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
            return response;
        }
        #endregion


        #region ValidateTaxCategory
        public List<ValidateListResponse> ValidateTaxCategory(XmlParser xmlParser)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            bool existTaxRate = false;
            bool validFor = true;

            responses.Add(new ValidateListResponse
            {
                IsValid = true,
                Mandatory = true,
                ErrorCode = "100",
                ErrorMessage = "Evento ValidateTaxCategory referenciado correctamente",
                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
            });


            XmlNodeList taxCategoryListResponse = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='Invoice']/*[local-name()='InvoiceLine']/*[local-name()='TaxTotal']/*[local-name()='TaxSubtotal']/*[local-name()='TaxCategory']/*[local-name()='TaxScheme']/*[local-name()='ID']");
            XmlNodeList percentCategoryListResponse = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='Invoice']/*[local-name()='InvoiceLine']/*[local-name()='TaxTotal']/*[local-name()='TaxSubtotal']/*[local-name()='TaxCategory']/*[local-name()='Percent']");

            if(taxCategoryListResponse.Count != percentCategoryListResponse.Count)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = false,
                    ErrorCode = "DSAX14",
                    ErrorMessage = "Reporta una tarifa diferente para uno de los tributos enunciados en la tabla 11.3.9",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });

                return responses;
            }

            for (int i = 0; i < taxCategoryListResponse.Count; i++)
            {
                var xmlID = taxCategoryListResponse.Item(i).SelectNodes("//*[local-name()='TaxCategory']/*[local-name()='TaxScheme']/*[local-name()='ID']").Item(i)?.InnerText.ToString();                
                var xmlPercent = percentCategoryListResponse.Item(i).SelectNodes("//*[local-name()='Invoice']/*[local-name()='InvoiceLine']/*[local-name()='TaxTotal']/*[local-name()='TaxSubtotal']/*[local-name()='TaxCategory']/*[local-name()='Percent']").Item(i)?.InnerText.ToString();

                existTaxRate = TableManagerGlobalTaxRate.Exist<GlobalTaxRate>(xmlID, xmlPercent);
                if (!existTaxRate)
                {
                    validFor = false;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = false,
                        ErrorCode = "DSAX14",
                        ErrorMessage = "Reporta una tarifa diferente para uno de los tributos enunciados en la tabla 11.3.9",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                    break;
                }                             
            }

            return responses;
        }
        #endregion

        #region ValidateInvoiceLine
        public List<ValidateListResponse> ValidateInvoiceLine(XmlParser xmlParser)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            bool validFor = true;

            responses.Add(new ValidateListResponse
            {
                IsValid = true,
                Mandatory = true,
                ErrorCode = "100",
                ErrorMessage = "Evento ValidateInvoiceLine referenciado correctamente",
                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
            });

           
            XmlNodeList invoiceListResponse = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='Invoice'][1]/*[local-name()='UBLExtensions']/*[local-name()='UBLExtension']/*[local-name()='ExtensionContent']/*[local-name()='Lines']/*[local-name()='InvoiceLine']/*[local-name()='ID']");
            int[] arrayInvoiceLine = new int[invoiceListResponse.Count];

            int tempID = 0;
            for (int i = 0; i < invoiceListResponse.Count; i++)
            {              
                var xmlID = Convert.ToInt32(invoiceListResponse.Item(i).SelectNodes("//*[local-name()='ID']").Item(i)?.InnerText.ToString().Trim());
               
                if (i == 0)
                    tempID = xmlID;
                else
                {
                    if(!int.Equals(xmlID,tempID+1))                     
                    {
                        validFor = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "DIBC05b",
                            ErrorMessage = "Los números de línea de factura utilizados en los diferentes grupos no son consecutivos, empezando con “1”",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }   
                    else
                        tempID = Convert.ToInt32(xmlID);
                }

                arrayInvoiceLine[i] = Convert.ToInt32(xmlID);
            }

            if (validFor)
            {
                bool pares = arrayInvoiceLine.Distinct().Count() == arrayInvoiceLine.Length;
                if (!pares)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "DIBC05a",
                        ErrorMessage = "Más de un grupo conteniendo el elemento /de:Invoice/de:InvoiceLine/cbc:ID con la misma información o no existe ningún valor",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }        

            return responses;
        }

        #endregion


        #region Cufe validation
        public ValidateListResponse ValidateCufe(CufeModel cufeModel, string trackId)
        {
            DateTime startDate = DateTime.UtcNow;
            trackId = trackId.ToLower();
            var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
            string errorMessarge = string.Empty;
            string key = string.Empty;
            var errorCode = "FAD06";
            var prop = "CUFE";

            string[] codesWithCUDE = { "03", "05", "91", "92", "96" };
            if (codesWithCUDE.Contains(documentMeta.DocumentTypeId))
                prop = "CUDE";
            if (documentMeta.DocumentTypeId == "05")
                errorCode = "DSAD06";
            else if (documentMeta.DocumentTypeId == "91")
                errorCode = "CAD06";
            else if (documentMeta.DocumentTypeId == "92")
                errorCode = "DAD06";
            else if (documentMeta.DocumentTypeId == "96")
                errorCode = "AAD06";

            var billerSoftwareId = ConfigurationManager.GetValue("BillerSoftwareId");
            var billerSoftwarePin = ConfigurationManager.GetValue("BillerSoftwarePin");

            if (!codesWithCUDE.Contains(documentMeta.DocumentTypeId))
                key = ConfigurationManager.GetValue("TestHabTechnicalKey");
            else
            {
                var softwareId = cufeModel.SoftwareId;
                if (softwareId == billerSoftwareId)
                    key = billerSoftwarePin;
                else
                {
                    var software = GetSoftwareInstanceCache(softwareId);
                    key = software?.Pin;
                }
            }

            if (ConfigurationManager.GetValue("Environment") == "Prod")
            {
                if (!codesWithCUDE.Contains(documentMeta.DocumentTypeId))
                {
                    var documentType = documentMeta.DocumentTypeId;
                    if (new string[] { "01", "02", "04" }.Contains(documentType)) documentType = "01";
                    var rk = $"{documentMeta?.Serie}|{documentType}|{documentMeta?.InvoiceAuthorization}";
                    var range = numberRangeTableManager.Find<GlobalNumberRange>(documentMeta.SenderCode, rk);
                    key = range?.TechnicalKey;
                }
            }
            errorMessarge = documentMeta.DocumentTypeId == "96" ? "El valor UUID no está correctamente calculado" : $"Valor del { prop} no está calculado correctamente.";
            var response = new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = errorCode, ErrorMessage = errorMessarge };

            var number = cufeModel.SerieAndNumber;
            var emissionDate = cufeModel.EmissionDate;
            var emissionHour = cufeModel.HourEmission;
            var amount = cufeModel.Amount?.Trim();
            if (string.IsNullOrEmpty(amount)) amount = "0.00"; else amount = TruncateDecimal(decimal.Parse(amount), 2).ToString("F2");
            var taxCode1 = "01";
            var taxCode2 = "04";
            var taxCode3 = "03";
            var taxAmount1 = cufeModel.TaxAmount1?.Trim();
            taxAmount1 = taxAmount1?.Split('|')[0];
            var taxAmount2 = cufeModel.TaxAmount2?.Trim();
            var taxAmount3 = cufeModel.TaxAmount3?.Trim();

            if (string.IsNullOrEmpty(taxAmount1)) taxAmount1 = "0.00"; else taxAmount1 = TruncateDecimal(decimal.Parse(taxAmount1), 2).ToString("F2");
            if (string.IsNullOrEmpty(taxAmount2)) taxAmount2 = "0.00"; else taxAmount2 = TruncateDecimal(decimal.Parse(taxAmount2), 2).ToString("F2");
            if (string.IsNullOrEmpty(taxAmount3)) taxAmount3 = "0.00"; else taxAmount3 = TruncateDecimal(decimal.Parse(taxAmount3), 2).ToString("F2");
            var amountToPay = cufeModel.TotalAmount?.Trim();
            if (string.IsNullOrEmpty(amountToPay)) amountToPay = "0.00"; else amountToPay = TruncateDecimal(decimal.Parse(amountToPay), 2).ToString("F2");
            var senderCode = cufeModel.SenderCode;
            var receiverCode = cufeModel.ReceiverCode;
            var environmentType = cufeModel.EnvironmentType;

            var fakeData = $"{number}---{emissionDate}---{emissionHour}---{amount}---{taxCode1}---{taxAmount1}---{taxCode2}---{taxAmount2}---{taxCode3}---{taxAmount3}---{amountToPay}---{senderCode}---{receiverCode}---{key}---{environmentType}";

            var data = $"{number}{emissionDate}{emissionHour}{amount}{taxCode1}{taxAmount1}{taxCode2}{taxAmount2}{taxCode3}{taxAmount3}{amountToPay}{senderCode}{receiverCode}{key}{environmentType}";
            var documentKey = cufeModel.DocumentKey;

            // Only for AR
            if (cufeModel.DocumentTypeId == "96")
            {
                fakeData = $"{cufeModel.SerieAndNumber}---{cufeModel.EmissionDate}---{cufeModel.HourEmission}---{cufeModel.SenderCode}---{cufeModel.ReceiverCode}---{cufeModel.ResponseCode}---{cufeModel.ReferenceId}---{cufeModel.ReferenceTypeCode}---{key}";

                if ((cufeModel.ResponseCode == "037" || cufeModel.ResponseCode == "038" || cufeModel.ResponseCode == "039") && cufeModel.ResponseCodeListID == "2")
                {
                    //Endoso en garantia en blanco
                    data = $"{cufeModel.SerieAndNumber}{cufeModel.EmissionDate}{cufeModel.HourEmission}{cufeModel.ReceiverCode}{cufeModel.ResponseCode}{cufeModel.ResponseCodeListID}{cufeModel.ReferenceId}{cufeModel.ReferenceTypeCode}{key}";
                }
                else
                {
                    data = $"{cufeModel.SerieAndNumber}{cufeModel.EmissionDate}{cufeModel.HourEmission}{cufeModel.SenderCode}{cufeModel.ReceiverCode}{cufeModel.ResponseCode}{cufeModel.ReferenceId}{cufeModel.ReferenceTypeCode}{key}";
                }
                documentKey = cufeModel.Cude;
            }

            var hash = data.EncryptSHA384();

            if (documentKey.ToLower() == hash)
            {
                response.IsValid = true;
                response.ErrorMessage = $"Valor del {prop} calculado correctamente.";
            }

            response.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
            return response;
        }
        #endregion

        #region Evento Cune

        public ValidateListResponse ValidateCune(CuneModel objCune, RequestObjectCune data)
        {
            DateTime startDate = DateTime.UtcNow;
            data.trackId = data.trackId.ToLower();

            var ValDev = objCune?.ValDev.ToString("F2");
            var ValDesc = objCune?.ValDesc.ToString("F2");
            var ValTol = objCune?.ValTol.ToString("F2");

            var errorCode = Convert.ToInt32(objCune.DocumentType) == (int)DocumentType.IndividualPayroll ?  ConfigurationManager.GetValue("ErrorCode_NIE024") : ConfigurationManager.GetValue("ErrorCode_NIAE024");

            string key = string.Empty;

            var billerSoftwareId = ConfigurationManager.GetValue("BillerSoftwareId");
            var billerSoftwarePin = ConfigurationManager.GetValue("BillerSoftwarePin");

            var softwareId = objCune.SoftwareId;
            if (softwareId == billerSoftwareId)
            {
                key = billerSoftwarePin;
            }
            else
            {
                var software = GetSoftwareInstanceCache(softwareId);
                key = software?.Pin;
            }         

            string errorMessarge = string.Empty;
            errorMessarge = ConfigurationManager.GetValue("ErrorMessage_NIE024");
            var response = new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = errorCode, ErrorMessage = errorMessarge };

            ValDev = (ValDev == null) ? "0.00" : ValDev = TruncateDecimal(decimal.Parse(ValDev), 2).ToString("F2");
            ValDesc = (ValDesc == null) ? "0.00" : ValDesc = TruncateDecimal(decimal.Parse(ValDesc), 2).ToString("F2");
            ValTol = (ValTol == null) ? "0.00" :  ValTol = TruncateDecimal(decimal.Parse(ValTol), 2).ToString("F2");

            var NumNIE = objCune.NumNIE;
            var FechNIE = objCune.FecNIE;
            var HorNIE = objCune.HorNIE;
            var NitNIE = objCune.NitNIE;
            var DocEmp = objCune.DocEmp;
            var SoftwarePin = key;
            var TipAmb = objCune.TipAmb;

            var numberSha384 = $"{NumNIE}{FechNIE}{HorNIE}{ValDev}{ValDesc}{ValTol}{NitNIE}{DocEmp}{SoftwarePin}{TipAmb}";

            var hash = numberSha384.EncryptSHA384();

            if (objCune.Cune.ToLower() == hash)
            {
                response.IsValid = true;
                response.ErrorMessage = $"Valor calculado correctamente.";
            }
            else
            {
                response.IsValid = false;
                response.ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_NIE024");
            }

            response.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
            return response;
        }
        #endregion

        #region Document
        public ValidateListResponse ValidateDocumentDuplicity(string trackId)
        {
            DateTime startDate = DateTime.UtcNow;
            trackId = trackId.ToLower();

            var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
            var documentEntity = documentValidatorTableManager.Find<GlobalDocValidatorDocument>(documentMeta?.Identifier, documentMeta?.Identifier);
            var response = new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "90" };
            if (documentEntity == null)
            {
                response.IsValid = true;
                response.ErrorMessage = "Documento no procesado anteriormente.";
            }
            else response.ErrorMessage = $"Documento procesado anteriormente.";


            response.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
            return response;
        }
        #endregion

        #region Payroll

        public List<ValidateListResponse> ValidateIndividualPayroll(XmlParseNomina xmlParser, DocumentParsedNomina model)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            responses.AddRange(this.CheckIndividualPayrollDuplicity(model.EmpleadorNIT, model.SerieAndNumber));

            if (Convert.ToInt32(model.DocumentTypeId) == Convert.ToInt32(DocumentType.IndividualPayroll))
            {                
                responses.AddRange(this.CheckIndividualPayrollInSameMonth(model.EmpleadorNIT, 
                                    xmlParser.globalDocPayrolls.NumeroDocumento, 
                                    xmlParser.Novelty, 
                                    xmlParser.globalDocPayrolls.FechaPagoInicio.Value));
            }

            return responses;
        }

        #endregion

        #region NIT validations
        public List<ValidateListResponse> ValidateNit(NitModel nitModel, string trackId, NominaModel nominaModel = null)
        {
            DateTime startDate = DateTime.UtcNow;
            trackId = trackId.ToLower();
            GlobalContributor softwareProvider = null;
            GlobalRadianOperations softwareProviderRadian = null;
            bool habilitadoRadian = false;
            var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);

            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            // Validación de DV para documentos de Nómina
            if (nominaModel != null)
            {
                if (Convert.ToInt32(nominaModel.DocumentTypeId) == (int)DocumentType.IndividualPayroll)
                {
                    // Proveedor
                    if (ValidateDigitCode(nominaModel.ProveedorNIT, int.Parse(nominaModel.ProveedorDV)))
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "NIE018", ErrorMessage = "DV corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "NIE018", ErrorMessage = "Se debe colocar el DV de la empresa dueña del Software que genera el Documento, debe estar registrado en la DIAN", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    // Empleador
                    if (ValidateDigitCode(nominaModel.EmpleadorNIT, int.Parse(nominaModel.EmpleadorDV)))
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "NIE034", ErrorMessage = "DV corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "NIE034", ErrorMessage = "Debe ir el DV del Empleador", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                }
                else
                {
                    // Proveedor
                    if (ValidateDigitCode(nominaModel.ProveedorNIT, int.Parse(nominaModel.ProveedorDV)))
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "NIAE018", ErrorMessage = "DV corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "NIAE018", ErrorMessage = "Se debe colocar el DV de la empresa dueña del Software que genera el Documento, debe estar registrado en la DIAN", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    // Empleador
                    if (ValidateDigitCode(nominaModel.EmpleadorNIT, int.Parse(nominaModel.EmpleadorDV)))
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "NIAE034", ErrorMessage = "DV corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "NIAE034", ErrorMessage = "Debe ir el DV del Empleador", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                }

                var otherElectricDocuments = TableManagerGlobalOtherDocElecOperation
                    .FindGlobalOtherDocElecOperationByPartition_RowKey_Deleted_State<GlobalOtherDocElecOperation>(nominaModel.ProveedorNIT,
                    nominaModel.ProveedorSoftwareID, false, "Habilitado");
                if(otherElectricDocuments != null && otherElectricDocuments.Count > 0)
                {
                    // ElectronicDocumentId = 1. Es para Documentos 11 y 12 (Nómina Individual y Nómina Individual de Ajuste).
                    var electricDocumentFound = otherElectricDocuments.FirstOrDefault(x => x.ElectronicDocumentId == 1);
                    if(electricDocumentFound != null)
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "92", ErrorMessage = "El Emisor del Documento se encuentra Habilitado en la Plataforma.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "92", ErrorMessage = "El Emisor del Documento no se encuentra Habilitado en la Plataforma.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

                }
                else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "92", ErrorMessage = "El Emisor del Documento no se encuentra Habilitado en la Plataforma.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

                return responses;
            }

            var senderCode = nitModel.SenderCode;
            var senderCodeDigit = nitModel.SenderCodeDigit;

            var senderCodeProvider = nitModel.ProviderCode;
            var senderCodeProviderDigit = nitModel.ProviderCodeDigit;
            var softwareId = nitModel.SoftwareId;

            var receiverCode = nitModel.ReceiverCode;
            var receiverCodeSchemeNameValue = nitModel.ReceiverCodeSchemaValue;
            if (receiverCodeSchemeNameValue == "31")
            {
                string receiverDvErrorCode = "FAK24";
                if (documentMeta.DocumentTypeId == "91") receiverDvErrorCode = "CAK24";
                else if (documentMeta.DocumentTypeId == "92") receiverDvErrorCode = "DAK24";
                else if (documentMeta.DocumentTypeId == "96") receiverDvErrorCode = Properties.Settings.Default.COD_VN_DocumentMeta_AAK24;

                var receiverCodeDigit = nitModel.ReceiverCodeDigit;
                if (string.IsNullOrEmpty(receiverCodeDigit) || receiverCodeDigit == "undefined") receiverCodeDigit = "11";
                if (ValidateDigitCode(receiverCode, int.Parse(receiverCodeDigit)))
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = receiverDvErrorCode, ErrorMessage = "DV corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = receiverDvErrorCode, ErrorMessage = "DV no corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            }

                      
            var receiver2Code = nitModel.ReceiverCode2;
            if (receiverCode != receiver2Code)
            {
                var receiver2CodeSchemeNameValue = nitModel.ReceiverCode2SchemaValue;
                if (receiver2CodeSchemeNameValue == "31")
                {
                    string receiver2DvErrorCode = "FAK47";
                    if (documentMeta.DocumentTypeId == "91") receiver2DvErrorCode = "CAK47";
                    else if (documentMeta.DocumentTypeId == "92") receiver2DvErrorCode = "DAK47";
                    else if (documentMeta.DocumentTypeId == "96") receiver2DvErrorCode = Properties.Settings.Default.COD_VN_DocumentMeta_AAK47;

                    var receiver2CodeDigit = nitModel.ReceiverCode2Digit;
                    if (string.IsNullOrEmpty(receiver2CodeDigit) || receiver2CodeDigit == "undefined") receiver2CodeDigit = "11";
                    if (ValidateDigitCode(receiver2Code, int.Parse(receiver2CodeDigit)))
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = receiver2DvErrorCode, ErrorMessage = "DV corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = receiver2DvErrorCode, ErrorMessage = "DV no corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                }
            }

            //IssuerParty Adquiriente/deudor de la Factura Electrónica evento Endoso Electronico
            if (nitModel.ResponseCode == "043")
            {
                var issuerPartyCode = nitModel.IssuerPartyID;
                var IssuerPartyCodeDigit = nitModel.IssuerPartySchemeID;
                if (string.IsNullOrEmpty(IssuerPartyCodeDigit) || IssuerPartyCodeDigit == "undefined") IssuerPartyCodeDigit = "11";
                if (ValidateDigitCode(issuerPartyCode, int.Parse(IssuerPartyCodeDigit)))
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "AAH63", ErrorMessage = "DV corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "AAH63", ErrorMessage = "El DV no está correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

                //Valida DV del PowerOfAttorney - schemeName
                string agentPartyPersonSchemeName = nitModel.AgentPartyPersonSchemeName;
                string agentPartyPersonSchemeID = nitModel.AgentPartyPersonSchemeID;
                long number1 = 0;
                bool valNumber = long.TryParse(agentPartyPersonSchemeID, out number1);
                if (agentPartyPersonSchemeName == "31")
                {
                    if (valNumber)
                    {
                        if (string.IsNullOrEmpty(agentPartyPersonSchemeID) || agentPartyPersonSchemeID == "undefined") agentPartyPersonSchemeID = "11";
                        if (ValidateDigitCode(receiverCode, int.Parse(agentPartyPersonSchemeID)))
                            responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAH71"), ErrorMessage = "DV corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                        else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAH71"), ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH71"), ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    }
                    else { responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAH71"), ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH71"), ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds }); }
                }
            }

            var softwareProviderCode = nitModel.SoftwareProviderCode;
            var softwareProviderCodeDigit = nitModel.SoftwareProviderCodeDigit;
            var providerCode = nitModel.ProviderCode;
            var providerCodeDigit = nitModel.ProviderCodeDigit;

            GlobalContributor sender2 = null;
            var sender = GetContributorInstanceCache(senderCode);
            if (documentMeta.DocumentTypeId != "96")
            {
                // Sender

                string senderDvErrorCode = "FAJ24";
                string senderDvrErrorDescription = "DV del NIT del emsior del documento no está correctamente calculado";
                if (documentMeta.DocumentTypeId == "05")
                {
                    senderDvErrorCode = "DSAJ24b";
                    senderDvrErrorDescription = "El DV del NIT no es correcto";
                }
                else if (documentMeta.DocumentTypeId == "91") senderDvErrorCode = "CAJ24";
                else if (documentMeta.DocumentTypeId == "92") senderDvErrorCode = "DAJ24";

                if (string.IsNullOrEmpty(senderCodeDigit) || senderCodeDigit == "undefined") senderCodeDigit = "11";
                if (ValidateDigitCode(senderCode, int.Parse(senderCodeDigit)))
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = senderDvErrorCode, ErrorMessage = "DV del NIT del emsior del documento está correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = senderDvErrorCode, ErrorMessage = senderDvrErrorDescription, ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });


                // Sender2               
                if (senderCode != senderCodeProvider)
                {
                    string sender2DvErrorCode = "FAJ47";
                    if (documentMeta.DocumentTypeId == "91") sender2DvErrorCode = "CAJ47";
                    else if (documentMeta.DocumentTypeId == "92") sender2DvErrorCode = "DAJ47";

                    sender2 = GetContributorInstanceCache(senderCodeProvider);
                    if (string.IsNullOrEmpty(senderCodeProviderDigit) || senderCodeProviderDigit == "undefined") senderCodeProviderDigit = "11";
                    if (ValidateDigitCode(senderCodeProvider, int.Parse(senderCodeProviderDigit)))
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = sender2DvErrorCode, ErrorMessage = "DV del NIT del emsior del documento está correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = sender2DvErrorCode, ErrorMessage = "DV del NIT del emsior del documento no está correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                }
            }

            // Software provider
            string softwareproviderDvErrorCode = "FAB22";
            if (documentMeta.DocumentTypeId == "05") softwareproviderDvErrorCode = "DSAB22b";
            else if (documentMeta.DocumentTypeId == "91") softwareproviderDvErrorCode = "CAB22";
            else if (documentMeta.DocumentTypeId == "92") softwareproviderDvErrorCode = "DAB22";
            else if (documentMeta.DocumentTypeId == "96") softwareproviderDvErrorCode = Properties.Settings.Default.COD_VN_DocumentMeta_AAB22;

            if (documentMeta.DocumentTypeId == "96" && !documentMeta.SendTestSet && senderCodeProvider != "800197268" )
            {
                senderCodeProvider = senderCode != senderCodeProvider ? senderCodeProvider : senderCode;
                softwareProviderRadian = TableManagerGlobalRadianOperations.FindhByPartitionKeyRadianStatus<GlobalRadianOperations>(
                              senderCodeProvider, false, "Habilitado", softwareId);
                if (softwareProviderRadian != null)
                {
                    switch (softwareProviderRadian.SoftwareType)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            if (softwareProviderRadian.TecnologicalSupplier || softwareProviderRadian.Factor || softwareProviderRadian.NegotiationSystem
                                || softwareProviderRadian.ElectronicInvoicer || softwareProviderRadian.IndirectElectronicInvoicer)
                                habilitadoRadian = true;
                            break;
                    }
                }
            }        
            else
            {
                softwareProvider = GetContributorInstanceCache(softwareProviderCode);
            }

            if (string.IsNullOrEmpty(providerCodeDigit) || providerCodeDigit == "undefined") providerCodeDigit = "11";
            if (string.IsNullOrEmpty(softwareProviderCodeDigit) || softwareProviderCodeDigit == "undefined") softwareProviderCodeDigit = "11";
            if (ValidateDigitCode(documentMeta.DocumentTypeId == "96" ? providerCode : softwareProviderCode, documentMeta.DocumentTypeId == "96" ? int.Parse(providerCodeDigit) : int.Parse(softwareProviderCodeDigit)))
                responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = softwareproviderDvErrorCode, ErrorMessage = "DV del NIT del Prestador de Servicios está correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = softwareproviderDvErrorCode, ErrorMessage = "DV del NIT del Prestador de Servicios no está correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

            string senderErrorCode = "FAJ21";
            if (documentMeta.DocumentTypeId == "05") senderErrorCode = "DSAJ21";
            else if (documentMeta.DocumentTypeId == "91") senderErrorCode = "CAJ21";
            else if (documentMeta.DocumentTypeId == "92") senderErrorCode = "DAJ21";
           

            string sender2ErrorCode = "FAJ44";
            if (documentMeta.DocumentTypeId == "91") sender2ErrorCode = "CAJ44";
            else if (documentMeta.DocumentTypeId == "92") sender2ErrorCode = "DAJ44";


            string softwareProviderErrorCode = "FAB19b";
            if (documentMeta.DocumentTypeId == "05") softwareProviderErrorCode = "DSAB19b";
            else if (documentMeta.DocumentTypeId == "91") softwareProviderErrorCode = "CAB19b";
            else if (documentMeta.DocumentTypeId == "92") softwareProviderErrorCode = "DAB19b";

            //Validar habilitacion RADIAN
            else if (documentMeta.DocumentTypeId == "96") softwareProviderErrorCode = Properties.Settings.Default.COD_VN_DocumentMeta_AAB19b;

            string softwareProviderCodeHab = habilitadoRadian ? softwareProviderRadian?.PartitionKey : softwareProvider?.Code;

            if (ConfigurationManager.GetValue("Environment") == "Hab" || ConfigurationManager.GetValue("Environment") == "Test")
            {               
                if (!string.IsNullOrEmpty(senderCodeProvider) && senderCode != senderCodeProvider && documentMeta.DocumentTypeId != "96")
                {
                    if (sender2 != null)
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = sender2ErrorCode, ErrorMessage = $"{sender2.Code} del emisor de servicios autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else
                        responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = sender2ErrorCode, ErrorMessage = $"{sender2?.Code} Emisor de servicios no autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                }

                if (softwareProvider != null || habilitadoRadian)
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = softwareProviderErrorCode, ErrorMessage = $"{ softwareProviderCodeHab } Prestrador de servicios autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                else
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = softwareProviderErrorCode, ErrorMessage = $"{ softwareProviderCodeHab } NIT del Prestador de Servicios No está autorizado para prestar servicios.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            }
            else if (ConfigurationManager.GetValue("Environment") == "Prod")
            {
                //Valida software proveedor RADIAN Habilitado
                if (documentMeta.DocumentTypeId == "96")
                {
                    if (softwareProviderRadian != null && habilitadoRadian)
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = softwareProviderErrorCode, ErrorMessage = $"{ softwareProviderCodeHab } Prestrador de servicios autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else
                        responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = softwareProviderErrorCode, ErrorMessage = $"{ softwareProviderCodeHab } NIT del Prestador de Servicios No está autorizado para prestar servicios.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                }
                else
                {
                    if (sender?.StatusId == (int)ContributorStatus.Enabled)
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = senderErrorCode, ErrorMessage = $"{sender.Code} del emisor de servicios autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else
                        responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = senderErrorCode, ErrorMessage = $"{sender?.Code} Emisor de servicios no autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

                    if (!string.IsNullOrEmpty(senderCodeProvider) && senderCode != senderCodeProvider)
                    {
                        if (sender2?.StatusId == (int)ContributorStatus.Enabled)
                            responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = sender2ErrorCode, ErrorMessage = $"{sender2.Code} del emisor de servicios autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                        else
                            responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = sender2ErrorCode, ErrorMessage = $"{sender2?.Code} Emisor de servicios no autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    }

                    if (softwareProvider?.StatusId == (int)ContributorStatus.Enabled)
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = softwareProviderErrorCode, ErrorMessage = $"{softwareProviderCodeHab} Prestrador de servicios autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else
                        responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = softwareProviderErrorCode, ErrorMessage = $"{softwareProviderCodeHab} NIT del Prestador de Servicios No está autorizado para prestar servicios.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

                }
            }

            foreach (var r in responses)
                r.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
            return responses;
        }
        #endregion

        #region Note validations
        public ValidateListResponse ValidateNoteReference(string trackId)
        {
            DateTime startDate = DateTime.UtcNow;
            trackId = trackId.ToLower();
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);

            var digit = documentMeta.DocumentTypeId == "91" ? "C" : "D";

            if (string.IsNullOrEmpty(documentMeta.DocumentReferencedKey))
                return new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = $"{digit}BG02", ErrorMessage = "Se requiere obligatoriamente referencia a documento.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds };

            var referencedDocumentData = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(documentMeta.DocumentReferencedKey, documentMeta.DocumentReferencedKey);

            var referencedDocument = documentValidatorTableManager.Find<GlobalDocValidatorDocument>(referencedDocumentData?.Identifier, referencedDocumentData?.Identifier);
            if (referencedDocument == null)
            {
                referencedDocument = documentValidatorTableManager.Find<GlobalDocValidatorDocument>(referencedDocumentData?.DocumentKey, referencedDocumentData?.DocumentKey);
                if (referencedDocument == null)
                    return new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = $"{digit}BG04a", ErrorMessage = "Documento referenciado no existe en los registros de la DIAN.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds };
            }

            if (referencedDocumentData.SenderCode != documentMeta.SenderCode)
                return new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = $"{digit}BG04b", ErrorMessage = "Documento referenciado no pertenece al emisor.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds };

            int.TryParse(referencedDocument.EmissionDateNumber, out int emissionDate);
            if (emissionDate > int.Parse(documentMeta.EmissionDate.ToString("yyyyMMdd")))
                return new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = $"{digit}BG06", ErrorMessage = "Fecha de emisión de la nota es anterior a la fecha de emisión de FE referenciada.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds };

            return new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = $"{digit}BG02", ErrorMessage = "Factura electrónica referenciada correctamente.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds };
        }
        #endregion

        #region Validate SenderCode and ReceiverCode
        public List<ValidateListResponse> ValidateParty(NitModel nitModel, RequestObjectParty party, XmlParser xmlParserCude)
        {
            DateTime startDate = DateTime.UtcNow;
            party.TrackId = party.TrackId.ToLower();
            ErrorCodeMessage errorCodeMessage = getErrorCodeMessage(party.ResponseCode);
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            string eventCode = party.ResponseCode;
            //Valida cambio legitimo tenedor
            string senderCode = nitModel.SenderCode;
            var receiverCode = nitModel.ReceiverCode;
            string errorMessageParty = "Evento ValidateParty referenciado correctamente";
          
            //Endoso en Blanco
            if ((Convert.ToInt32(eventCode) == (int)EventStatus.EndosoPropiedad || Convert.ToInt32(eventCode) == (int)EventStatus.EndosoGarantia ||
               Convert.ToInt32(eventCode) == (int)EventStatus.EndosoProcuracion) && party.ListId == "2")
            {
                party.SenderParty = nitModel.SenderCode;
            }

            //valida si existe los permisos del mandatario
            if (party.SenderParty != xmlParserCude.ProviderCode && xmlParserCude.ProviderCode != "800197268")
            {
                var responseVal = ValidateFacultityAttorney(nitModel, party, xmlParserCude);
                if (responseVal != null)
                {
                    foreach (var item in responseVal)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = item.IsValid,
                            Mandatory = item.Mandatory,
                            ErrorCode = item.ErrorCode,
                            ErrorMessage = item.ErrorMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }
            }

            switch (Convert.ToInt16(party.ResponseCode))
            {
                case (int)EventStatus.Received:
                case (int)EventStatus.Rejected:
                case (int)EventStatus.Receipt:
                case (int)EventStatus.Accepted:
                    //Valida emeisor documento 
                    if (party.SenderParty != receiverCode)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = errorCodeMessage.errorCodeFETV,
                            ErrorMessage = errorCodeMessage.errorMessageFETV,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    //Valida receptor documento AR coincida con el Emisor/Facturador
                    if (party.ReceiverParty != senderCode)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = errorCodeMessage.errorCodeReceiverFETV,
                            ErrorMessage = errorCodeMessage.errorMessageReceiverFETV,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }                  

                    return responses;

                case (int)EventStatus.AceptacionTacita:

                    if (party.SenderParty != senderCode && (Convert.ToInt32(eventCode) != (int)EventStatus.Mandato))
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = (Convert.ToInt32(eventCode) == 34) ? errorCodeMessage.errorCodeFETV : errorCodeMessage.errorCode,
                            ErrorMessage = (Convert.ToInt32(eventCode) == 34) ? errorCodeMessage.errorMessageFETV : errorCodeMessage.errorMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    //Valida receptor documento AR coincida con DIAN
                    if (!string.IsNullOrWhiteSpace(party.ReceiverParty) && party.ReceiverParty != "800197268")
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = Convert.ToInt16(party.ResponseCode) == 34 ? "AAG01e" : "AAG04",
                            ErrorMessage = Convert.ToInt16(party.ResponseCode) == 34 ? "No fue informado los datos de la DIAN" : "No fue informado el literal '800197268'",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    return responses;

                case (int)EventStatus.Mandato:

                    responses.Add(new ValidateListResponse
                    {
                        IsValid = true,
                        Mandatory = true,
                        ErrorCode = "100",
                        ErrorMessage = errorMessageParty,
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });

                    return responses;

                case (int)EventStatus.Avales:
                   
                    //Valida receptor documento AR coincida con DIAN
                    if (party.ReceiverParty != "800197268")
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAG01",
                            ErrorMessage = "No fue informado los datos de la DIAN",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    return responses;
                case (int)EventStatus.SolicitudDisponibilizacion:

                    //Valida existe cambio legitimo tenedor factura
                    bool validHolderExchang = false;
                    LogicalEventRadian logicalEventRadianRejected = new LogicalEventRadian();
                    HolderExchangeModel responseHolderExchange = logicalEventRadianRejected.RetrieveSenderHolderExchange(nitModel.DocumentKey, xmlParserCude);                                                      
                    if (responseHolderExchange != null)
                    {
                        validHolderExchang = true;                       
                        senderCode = !string.IsNullOrWhiteSpace(responseHolderExchange.PartyLegalEntity) ? responseHolderExchange.PartyLegalEntity : string.Empty;                       
                    }

                    if (party.SenderParty != senderCode)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = validHolderExchang ? errorCodeMessage.errorCodeB : errorCodeMessage.errorCode,
                            ErrorMessage = validHolderExchang ? errorCodeMessage.errorMessageB : errorCodeMessage.errorMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    //Valida receptor documento AR coincida con DIAN
                    if (party.ReceiverParty != "800197268")
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAG01",
                            ErrorMessage = "No fue informado los datos de la DIAN",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    return responses;

                case (int)EventStatus.EndosoPropiedad:
                case (int)EventStatus.EndosoGarantia:
                case (int)EventStatus.EndosoProcuracion:
                    if (party.ListId != "2") // No informa SenderParty es un endoso en blanco entonces no valida emisor documento
                    {
                        if (party.SenderParty != senderCode)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = errorCodeMessage.errorCode,
                                ErrorMessage = errorCodeMessage.errorMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        else
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = errorMessageParty,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    return responses;
             
                case (int)EventStatus.InvoiceOfferedForNegotiation:
                    if (party.SenderParty != senderCode)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = errorCodeMessage.errorCode,
                            ErrorMessage = errorCodeMessage.errorMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    //Valida receptor documento AR coincida con el Endosatario
                    if (party.ReceiverParty != receiverCode)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAG04a",
                            ErrorMessage = "El destinatario no coincide con el documento del endosatario",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    return responses;

                case (int)EventStatus.NegotiatedInvoice:
                case (int)EventStatus.AnulacionLimitacionCirculacion:
                    // Valida receptor documento AR coincida con DIAN
                    if (!string.IsNullOrWhiteSpace(party.ReceiverParty) && party.ReceiverParty != "800197268")
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAG04",
                            ErrorMessage = "No fue informado el literal '800197268'",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    return responses;
             
                case (int)EventStatus.TerminacionMandato:
                    //Revocación es información del mandante
                    if (party.CustomizationID == "441")
                    {
                        if (party.SenderParty != senderCode)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = errorCodeMessage.errorCode,
                                ErrorMessage = errorCodeMessage.errorMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        else
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = errorMessageParty,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        // Valida receptor documento AR coincida con DIAN
                        if (!string.IsNullOrWhiteSpace(party.ReceiverParty) && party.ReceiverParty != "800197268")                        
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "AAG04",
                                ErrorMessage = "No fue informado el literal '800197268'",
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        else
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = errorMessageParty,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    //Renuncia
                    else if (party.CustomizationID == "442")
                    {
                        //Renuncia es información del mandatario
                        if (party.SenderParty != senderCode)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = errorCodeMessage.errorCode,
                                ErrorMessage = errorCodeMessage.errorMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        else if (!string.IsNullOrWhiteSpace(party.ReceiverParty) && party.ReceiverParty != "800197268")
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "AAG04",
                                ErrorMessage = "No fue informado el literal '800197268'",
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        else
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = errorMessageParty,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    return responses;

                //NotificacionPagoTotalParcial
                case (int)EventStatus.NotificacionPagoTotalParcial:
                    if (party.SenderParty == receiverCode || party.SenderParty == senderCode)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = errorCodeMessage.errorCode,
                            ErrorMessage = errorCodeMessage.errorMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    // Valida receptor documento AR coincida con DIAN
                    if (party.ReceiverParty != "800197268")
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAG01",
                            ErrorMessage = "No fue informado los datos del Tenedor Legitimo o Autoridad Competente",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageParty,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                  
                    return responses;
                //Valor Informe 3 dias pago
                case (int)EventStatus.ValInfoPago:
                    if (party.SenderParty != receiverCode)
                    {
                        var valid = ValidateBuyThreeDay(party.TrackId, party.SenderParty, nitModel.DocumentTypeId, (int)EventStatus.ValInfoPago);
                        if (valid != null)
                        {
                            responses.Add(valid);
                        }
                        else
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = errorMessageParty,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    else
                    {
                        // Valida receptor documento AR coincida con DIAN
                        if (party.ReceiverParty != nitModel.ReceiverCode)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "AAG01",
                                ErrorMessage = "No fue informado los datos del Adquirente/Deudor/aceptante",
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        else
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = errorMessageParty,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    break;
            }
            foreach (var r in responses)
                r.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
            return responses;
        }
        #endregion


        #region ValidatePayment
        private List<ValidateListResponse> ValidatePayment(XmlParser xmlParserCude, NitModel nitModel)
        {
            DateTime startDate = DateTime.UtcNow;
            //valor actual total factura TV
            string valueActualInvoice = nitModel.ValorActualTituloValor;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            bool validPayment = false;

            //Valor pago
            XmlNodeList valueListSender = xmlParserCude.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']");
            int totalValueSender = 0;
            for (int i = 0; i < valueListSender.Count; i++)
            {
                string valueStockAmount = valueListSender.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CorporateStockAmount']").Item(i)?.InnerText.ToString();
                totalValueSender += Int32.Parse(valueStockAmount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            }

            if(nitModel.CustomizationId == "452")
            {
                //Valida Total valor pagado igual al valor actual del titulo valor
                if (totalValueSender != Int32.Parse(valueActualInvoice, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture))
                {
                    validPayment = true;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAF19c"),
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF19c"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }
          
            //Valida Total valor pagado no supera el valor actual del titulo valor
            if (totalValueSender > Int32.Parse(valueActualInvoice, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture))
            {
                validPayment = true;
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAF19b"),
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF19b"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }
            
            if (validPayment)
                return responses;

            return null;
        }
        #endregion

        #region ValidateEndoso
        private List<ValidateListResponse> ValidateEndoso(XmlParser xmlParserCufe, XmlParser xmlParserCude, NitModel nitModel, string eventCode, double newAmountTV)
        {
            DateTime startDate = DateTime.UtcNow;
            //valor total Endoso Electronico AR
            string valueTotalEndoso = nitModel.ValorTotalEndoso;
            string valuePriceToPay = nitModel.PrecioPagarseFEV;
            string valueDiscountRateEndoso = nitModel.TasaDescuento;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            bool validEndoso = false;
            bool.TryParse(ConfigurationManager.GetValue("ValidateEndosoTrusted"), out bool ValidateEndosoTrusted);

            //Valida informacion Endoso en propiedad                       
            if ((Convert.ToInt32(eventCode) == (int)EventStatus.EndosoPropiedad))
            {
                //Valida informacion Endoso                           
                if (String.IsNullOrEmpty(valuePriceToPay) || String.IsNullOrEmpty(valueDiscountRateEndoso))
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = ValidateEndosoTrusted,
                        ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAI07b") + "-(N): ",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAI07b"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                    return responses;
                }

                //Calculo valor de la negociación
                int resultNegotiationValue = (Int32.Parse(valueTotalEndoso, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) * (100 - Int32.Parse(valueDiscountRateEndoso, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture)));
                resultNegotiationValue = resultNegotiationValue / 100;

                //Se debe comparar el valor de negociación contra el saldo(Nuevo Valor en disponibilización)
                if (Int32.Parse(valuePriceToPay, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) != resultNegotiationValue)
                {
                    validEndoso = true;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = ValidateEndosoTrusted,
                        ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAI07b") + "-(N): ",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAI07b"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            if (xmlParserCude.Fields["listID"].ToString() != "2")
            {
                XmlNodeList valueListSender = xmlParserCude.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']");
                int totalValueSender = 0;
                for (int i = 0; i < valueListSender.Count; i++)
                {
                    string valueStockAmount = valueListSender.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CorporateStockAmount']").Item(i)?.InnerText.ToString();
                    totalValueSender += Int32.Parse(valueStockAmount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                }

                XmlNodeList valueListReceiver = xmlParserCude.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='ReceiverParty']/*[local-name()='PartyLegalEntity']");
                int totalValueReceiver = 0;
                for (int i = 0; i < valueListReceiver.Count; i++)
                {
                    string valueStockAmount = valueListReceiver.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='ReceiverParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CorporateStockAmount']").Item(i)?.InnerText.ToString();
                    totalValueReceiver += Int32.Parse(valueStockAmount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                }

                if (Int32.Parse(valueTotalEndoso, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) != totalValueSender)
                {
                    validEndoso = true;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = ValidateEndosoTrusted,
                        ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAF19") + "-(N): ",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF19"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }

                if (Int32.Parse(valueTotalEndoso, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) != totalValueReceiver)
                {
                    validEndoso = true;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = ValidateEndosoTrusted,
                        ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAG20") + "-(N): ",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAG20"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            if (validEndoso)
                return responses;

            return null;
        }
        #endregion

        #region ValidateEndosoNota
        private List<ValidateListResponse> ValidateEndosoNota(List<GlobalDocValidatorDocumentMeta> documentMetaList, XmlParser xmlParserCude, string eventCode)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            bool validEndoso = false;

            if (eventCode == "040")
            {
                // busca un Endoso en Procuración...
                var documentMeta = documentMetaList.Where(x => x.EventCode == "039").OrderByDescending(x => x.SigningTimeStamp).FirstOrDefault();
                if (documentMeta != null)
                {
                    var document = documentValidatorTableManager.Find<GlobalDocValidatorDocument>(documentMeta.Identifier, documentMeta.Identifier);
                    if (document == null)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAH07"),
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH07"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        return responses;
                    }

                    if (string.IsNullOrWhiteSpace(xmlParserCude.NoteMandato))
                    {
                        validEndoso = true;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAD11a_040"),
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAD11a_040"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }
            }

            if (validEndoso)
                return responses;

            return null;
        }
        #endregion

        #region ValidateFacultityAttorney
        private List<ValidateListResponse> ValidateFacultityAttorney(NitModel nitModel, RequestObjectParty party, XmlParser xmlParserCude)
        {
            DateTime startDate = DateTime.UtcNow;
            string issueAtorney = xmlParserCude.ProviderCode;
            string eventCode = party.ResponseCode;
            string cufe = party.TrackId;
            string senderCode = nitModel.SenderCode;
            string noteMandato = xmlParserCude.NoteMandato;
            string noteMandato2 = xmlParserCude.NoteMandato2;
            string softwareId = xmlParserCude.Fields["SoftwareId"].ToString();
            ErrorCodeMessage errorCodeMessage = getErrorCodeMessage(eventCode);
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            bool validError = false;

            //Valida exista informacion mandato - Mandatario - CUFE para eventos disitintos a Mandato 043
            if (Convert.ToInt32(eventCode) != (int)EventStatus.Mandato && !validError)
            {
                if (Convert.ToInt32(eventCode) == (int)EventStatus.TerminacionMandato)
                {
                    var referenceCudeMandato = TableManagerGlobalDocReferenceAttorney.FindByPartition<GlobalDocReferenceAttorney>(party.TrackId);
                    if(referenceCudeMandato != null)
                    {
                        foreach (var cufeMandato in referenceCudeMandato)
                            cufe = cufeMandato.RowKey;
                    }
                }

                var docsReferenceAttorney = TableManagerGlobalDocReferenceAttorney.FindDocumentReferenceAttorney<GlobalDocReferenceAttorney>(cufe, senderCode);

                //No existe Mandato para el CUFE referenciado se valida si es Mandato Abierto ListID = 3
                if (docsReferenceAttorney == null || !docsReferenceAttorney.Any())
                {                    
                    docsReferenceAttorney = TableManagerGlobalDocReferenceAttorney.FindDocumentSenderCodeIssueAttorney<GlobalDocReferenceAttorney>(issueAtorney, senderCode);
                    if (docsReferenceAttorney == null || !docsReferenceAttorney.Any())
                    {
                        validError = true;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = ConfigurationManager.GetValue("ErrorCode_LGC36"),
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC36"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }
                else
                {
                    docsReferenceAttorney = TableManagerGlobalDocReferenceAttorney.FindDocumentSenderCodeIssueAttorney<GlobalDocReferenceAttorney>(issueAtorney, senderCode);
                    if (docsReferenceAttorney == null || !docsReferenceAttorney.Any())
                    {
                        validError = true;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = ConfigurationManager.GetValue("ErrorCode_LGC36"),
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC36"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }

                //Valida existan permisos para firmar evento por mandatario
                foreach (var docReferenceAttorney in docsReferenceAttorney)
                {
                    validError = false;
                    if (docReferenceAttorney.IssuerAttorney == issueAtorney)
                    {
                        if ((String.IsNullOrEmpty(docReferenceAttorney.EndDate)
                            || (DateTime.ParseExact(docReferenceAttorney.EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture) >= DateTime.Now)
                            ) && docReferenceAttorney.Active)
                        {
                            //Valida se encuetre habilitado Modo Operacion RadianOperation
                            var globalRadianOperation = TableManagerGlobalRadianOperations.FindhByPartitionKeyRadianStatus<GlobalRadianOperations>(
                                docReferenceAttorney.IssuerAttorney, false, "Habilitado", softwareId);
                            if (globalRadianOperation == null)
                            {                                                                                              
                                validError = true;
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = false,
                                    Mandatory = true,
                                    ErrorCode = "AAH62b",
                                    ErrorMessage = "El número de documento no corresponde a un participante habilitado en la plataforma RADIAN (PT/Factor/SNE).",
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                                break;
                            }
                            else
                            {
                                switch (docReferenceAttorney.Actor)
                                {
                                    case "PT":
                                        if (!globalRadianOperation.TecnologicalSupplier)
                                        {
                                            validError = true;
                                            responses.Add(new ValidateListResponse
                                            {
                                                IsValid = false,
                                                Mandatory = true,
                                                ErrorCode = ConfigurationManager.GetValue("ErrorCode_LGC59"),
                                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC59"),
                                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                            });
                                        }
                                        break;
                                    case "F":
                                        if (!globalRadianOperation.Factor)
                                        {
                                            validError = true;
                                            responses.Add(new ValidateListResponse
                                            {
                                                IsValid = false,
                                                Mandatory = true,
                                                ErrorCode = ConfigurationManager.GetValue("ErrorCode_LGC60"),
                                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC60"),
                                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                            });
                                        }
                                        break;
                                    case "SNE":
                                        if (!globalRadianOperation.NegotiationSystem)
                                        {
                                            validError = true;
                                            responses.Add(new ValidateListResponse
                                            {
                                                IsValid = false,
                                                Mandatory = true,
                                                ErrorCode = ConfigurationManager.GetValue("ErrorCode_LGC61"),
                                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC61"),
                                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                            });
                                        }
                                        break;
                                }
                            }

                            if (!validError)
                            {
                                string[] tempFacultityCode = docReferenceAttorney.FacultityCode.Split(';');
                                foreach (string codeFacultity in tempFacultityCode)
                                {
                                    //Valida permisos/facultades firma para el evento emitido
                                    var filter = $"{codeFacultity}-{docReferenceAttorney.Actor}";
                                    var attorneyFacultity = TableManagerGlobalAttorneyFacultity.FindDocumentReferenceAttorneyFaculitity<GlobalAttorneyFacultity>(filter).FirstOrDefault();
                                    if (attorneyFacultity != null)
                                    {
                                        if ((attorneyFacultity.RowKey == eventCode) || (attorneyFacultity.RowKey == "0") && codeFacultity != "MR91")
                                        {
                                            //Valida exista note mandatario
                                            if ( noteMandato == null || !noteMandato.Contains("OBRANDO EN NOMBRE Y REPRESENTACION DE"))                                              
                                            {
                                                if ( noteMandato2 == null || !noteMandato2.Contains("OBRANDO EN NOMBRE Y REPRESENTACION DE"))
                                                {
                                                    validError = true;
                                                    responses.Add(new ValidateListResponse
                                                    {
                                                        IsValid = false,
                                                        Mandatory = true,
                                                        ErrorCode = eventCode == "035" ? errorCodeMessage.errorCodeNoteA : errorCodeMessage.errorCodeNote,
                                                        ErrorMessage = eventCode == "035" ? errorCodeMessage.errorMessageNoteA : errorCodeMessage.errorMessageNote,
                                                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                                    });
                                                    break;
                                                }
                                               
                                            }

                                            //Si mandatario tiene permisos/facultades y esta habilitado para emitir documentos
                                            if (!validError)
                                                return null;

                                        }
                                        else if (codeFacultity != "MR91")
                                        {
                                            validError = true;
                                            responses.Add(new ValidateListResponse
                                            {
                                                IsValid = false,
                                                Mandatory = true,
                                                ErrorCode = (Convert.ToInt32(eventCode) >= 30 && Convert.ToInt32(eventCode) <= 34) ? errorCodeMessage.errorCodeFETV : errorCodeMessage.errorCodeMandato,
                                                ErrorMessage = (Convert.ToInt32(eventCode) >= 30 && Convert.ToInt32(eventCode) <= 34) ? errorCodeMessage.errorMessageFETV : errorCodeMessage.errorMessageMandato,
                                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                            });
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            validError = true;
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = ConfigurationManager.GetValue("ErrorCode_LGC35"),
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC35"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }

                    if(validError)
                        return responses;
                }

                if (validError)
                    return responses;
            }
            if (validError)
                return responses;
            return null;
        }
        #endregion

        #region ValidateValPago
        private ValidateListResponse ValidateBuyThreeDay(string trackId, string SenderParty, string documentTypeId, int eventCode)
        {
            DateTime startDate = DateTime.UtcNow;
            GlobalDocValidatorDocument document1 = null;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            var documentMeta = documentMetaTableManager.FindDocumentReferenced<GlobalDocValidatorDocumentMeta>(trackId.ToLower(), documentTypeId);
            foreach (var document in documentMeta)
            {
                document1 = documentValidatorTableManager.Find<GlobalDocValidatorDocument>(document.Identifier, document.Identifier);
                if (document1 != null)
                {
                    if (documentMeta.Where(t => t.EventCode == "037").ToList().Count == decimal.Zero)
                    {
                        if (documentMeta.Where(t => t.EventCode == "036").ToList().Count > decimal.Zero)
                        {
                            if (SenderParty == document.SenderCode && document.CustomizationID == "361" || document.CustomizationID == "362")
                            {
                                return null;
                            }
                            else
                            {
                                return new ValidateListResponse
                                {
                                    IsValid = false,
                                    Mandatory = true,
                                    ErrorCode = ConfigurationManager.GetValue("ErrorCode_LGC51"),
                                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC51"),
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                };
                            }
                        }
                        else
                        {
                            return new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = ConfigurationManager.GetValue("ErrorCode_LGC51"),
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC51"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            };
                        }
                    }
                    else
                    {
                        if (SenderParty == document.ReceiverCode && document.EventCode == "037")
                        {
                            return null;
                        }
                        else
                        {
                            return new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAF03"),
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF03"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            };
                        }
                    }

                }
            }
            return null;
        }
        #endregion
        #region IsBase64
        private bool IsBase64(String base64String)
        {
            var rs = !string.IsNullOrEmpty(base64String) && !string.IsNullOrWhiteSpace(base64String) && base64String.Length != 0 && base64String.Length % 4 == 0 && !base64String.Contains(" ");
            return rs;
        }
        #endregion

        #region ValidateReferenceAttorney
        private List<ValidateListResponse> ValidateCufeReferenceAttorney(XmlParser xmlParser)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            RequestObjectSigningTime dataSigningtime = new RequestObjectSigningTime();
            NitModel nitModel = new NitModel();
            bool validate = false;
            bool validateReference = false;
            bool validateSigningTime = false;
            int attorneyLimit = Convert.ToInt32(ConfigurationManager.GetValue("MAX_Attorney"));

            string customizationID = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='CustomizationID']").Item(0)?.InnerText.ToString();
            XmlNodeList cufeListResponse = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse'][1]/*[local-name()='DocumentReference']/*[local-name()='ID']");
            XmlNodeList cufeListResponseRefeerence = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse'][2]/*[local-name()='DocumentReference']/*[local-name()='ID']");
            
            //Valida cantidad de CUFEs referenciados
            if (cufeListResponse.Count > attorneyLimit || cufeListResponseRefeerence.Count > attorneyLimit)
            {
                validate = true;
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = ConfigurationManager.GetValue("ErrorCode_LGC58"),
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC58"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }
            else if(cufeListResponse.Count != cufeListResponseRefeerence.Count)
            {
                validate = true;
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAL04"),
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAL04"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }
            else
            {

                //Compara ID seccion DocumentReference 1 /  DocumentReference 2
                for (int i = 0; i < cufeListResponse.Count; i++)
                {
                    var xmlID = cufeListResponse.Item(i).SelectNodes("//*[local-name()='DocumentReference']/*[local-name()='ID']").Item(i)?.InnerText.ToString();
                    var xmlID2 = cufeListResponseRefeerence.Item(i).SelectNodes("//*[local-name()='DocumentReference']/*[local-name()='ID']").Item(i)?.InnerText.ToString();

                    if (!String.Equals(xmlID, xmlID2))
                    {
                        validate = true;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAL05"),
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAL05"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                    else
                        validate = false;
                }
                
                //Compara UUID seccion DocumentReference 1 /  DocumentReference 2
                for (int i = 0; i < cufeListResponse.Count; i++)
                {
                    var xmlUUID = cufeListResponse.Item(i).SelectNodes("//*[local-name()='DocumentReference']/*[local-name()='UUID']").Item(i)?.InnerText.ToString();
                    var xmlUUID2 = cufeListResponseRefeerence.Item(i).SelectNodes("//*[local-name()='DocumentReference']/*[local-name()='UUID']").Item(i)?.InnerText.ToString();

                    if (!String.Equals(xmlUUID, xmlUUID2))
                    {
                        validate = true;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAL05"),
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAL05"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                    else
                        validate = false;
                }
            }

            //Valida documentos referenciados
            for (int i = 0; i < cufeListResponse.Count && !validate; i++)
            {
                var xmlID = cufeListResponse.Item(i).SelectNodes("//*[local-name()='DocumentReference']/*[local-name()='ID']").Item(i)?.InnerText.ToString();
                var xmlUUID = cufeListResponse.Item(i).SelectNodes("//*[local-name()='DocumentReference']/*[local-name()='UUID']").Item(i)?.InnerText.ToString();
                var xmlDocumentTypeCode = cufeListResponse.Item(i).SelectNodes("//*[local-name()='DocumentReference']/*[local-name()='DocumentTypeCode']").Item(i)?.InnerText.ToString();

                //Valida CUFE referenciado existe en sistema DIAN                
                var resultValidateCufe = ValidateDocumentReferencePrev(xmlUUID, xmlID, "043", xmlDocumentTypeCode);
                foreach (var itemCufe in resultValidateCufe)
                {
                    if (!itemCufe.IsValid)
                    {
                        validateReference = true;
                        validate = true;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = itemCufe.IsValid,
                            Mandatory = true,
                            ErrorCode = itemCufe.ErrorCode,
                            ErrorMessage = itemCufe.ErrorMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                }
                if (validateReference)
                    break;
            }

            dataSigningtime.EventCode = "043";
            dataSigningtime.SigningTime = xmlParser.SigningTime;
            dataSigningtime.DocumentTypeId = "96";
            dataSigningtime.CustomizationID = customizationID;
            dataSigningtime.EndDate = "";

            //Valida La fecha debe ser mayor o igual al evento de la factura referenciada
            for (int i = 0; i < cufeListResponse.Count && !validate; i++)
            {              
                ValidatorEngine validatorEngine = new ValidatorEngine();
                dataSigningtime.TrackId = cufeListResponse.Item(i).SelectNodes("//*[local-name()='DocumentReference']/*[local-name()='UUID']").Item(i)?.InnerText.ToString();
                var xmlBytesCufe = validatorEngine.GetXmlFromStorageAsync(dataSigningtime.TrackId);
                var xmlParserCufe = new XmlParser(xmlBytesCufe.Result);
                if (!xmlParserCufe.Parser())
                    throw new Exception(xmlParserCufe.ParserError);


                var resultValidateSignInTime = ValidateSigningTime(dataSigningtime, xmlParserCufe, nitModel);
                foreach (var itemSingInTIme in resultValidateSignInTime)
                {
                    if (!itemSingInTIme.IsValid)
                    {
                        validateSigningTime = true;
                        validate = true;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = itemSingInTIme.IsValid,
                            Mandatory = true,
                            ErrorCode = ConfigurationManager.GetValue("ErrorCode_DC24r"),
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_DC24r"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                }
                if (validateSigningTime)
                    break;
            }

            if (validate)
                return responses;

            return null;
        }
        #endregion

        #region Validate Reference Attorney
        public List<ValidateListResponse> ValidateReferenceAttorney(XmlParser xmlParser, string trackId)
        {
            NitModel nitModel = new NitModel();
            string issuerPartyName = string.Empty;
            int attorneyLimit = Convert.ToInt32(ConfigurationManager.GetValue("MAX_Attorney"));
            bool validate = true;          
            string startDateAttorney = string.Empty;
            string endDate = string.Empty;
            DateTime startDate = DateTime.UtcNow;
            RequestObjectSigningTime dataSigningtime = new RequestObjectSigningTime();
            List<ValidateListResponse> responses = new List<ValidateListResponse>();           
            List<AttorneyModel> attorney = new List<AttorneyModel>();
            string senderCode = xmlParser.FieldValue("SenderCode", true).ToString();
            string providerCode = xmlParser.FieldValue("ProviderCode", true).ToString();
            //string AttachmentBase64 = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='LineResponse']/*[local-name()='LineReference']/*[local-name()='DocumentReference']/*[local-name()='Attachment']/*[local-name()='EmbeddedDocumentBinaryObject']").Item(0)?.InnerText.ToString();
            XmlNodeList AttachmentBase64List = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='LineResponse']/*[local-name()='LineReference']/*[local-name()='DocumentReference']/*[local-name()='Attachment']/*[local-name()='EmbeddedDocumentBinaryObject']");
            string issuerPartyCode = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='IssuerParty']/*[local-name()='PowerOfAttorney']/*[local-name()='ID']").Item(0)?.InnerText.ToString();
            string effectiveDate = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='Response']/*[local-name()='EffectiveDate']").Item(0)?.InnerText.ToString();
            XmlNodeList cufeList = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']");
            XmlNodeList cufeListReference = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse'][2]/*[local-name()='DocumentReference']");
            //XmlNodeList cufeList = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse'][2]/*[local-name()='DocumentReference']");
            string customizationID = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='CustomizationID']").Item(0)?.InnerText.ToString();
            string operacionMandato = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='CustomizationID']/@schemeID").Item(0)?.InnerText.ToString();
            string serieAndNumber = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='ID']").Item(0)?.InnerText.ToString();
            string senderName = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='SenderParty']/*[local-name()='PartyTaxScheme']/*[local-name()='RegistrationName']").Item(0)?.InnerText.ToString();
            //string listID = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='DocumentReference']/*[local-name()='ValidityPeriod']/*[local-name()='DescriptionCode']").Item(0)?.Attributes["listID"].Value;
            string listID = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='Response']/*[local-name()='ResponseCode']").Item(0)?.Attributes["listID"].Value;
            string firstName = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='SenderParty']/*[local-name()='Person']/*[local-name()='FirstName']").Item(0)?.InnerText.ToString();
            string familyName = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='SenderParty']/*[local-name()='Person']/*[local-name()='FamilyName']").Item(0)?.InnerText.ToString();
            string name = firstName + " " + familyName;
            dataSigningtime.EventCode = "043";
            dataSigningtime.SigningTime = xmlParser.SigningTime;
            dataSigningtime.DocumentTypeId = "96";
            dataSigningtime.CustomizationID = customizationID;
            dataSigningtime.EndDate = "";
            string factorTemp = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='IssuerParty']/*[local-name()='PowerOfAttorney']/*[local-name()='AgentParty']/*[local-name()='PartyIdentification']/*[local-name()='ID']").Item(0)?.InnerText.ToString();
            string description = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='IssuerParty']/*[local-name()='PowerOfAttorney']/*[local-name()='Description']").Item(0)?.InnerText.ToString();
            string senderId = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='SenderParty']/*[local-name()='PowerOfAttorney']/*[local-name()='AgentParty']/*[local-name()='PartyIdentification']/*[local-name()='ID']").Item(0)?.InnerText.ToString();
            string senderPowerOfAttorney = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='SenderParty']/*[local-name()='PowerOfAttorney']/*[local-name()='ID']").Item(0)?.InnerText.ToString();
            string descriptionSender = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='SenderParty']/*[local-name()='PowerOfAttorney']/*[local-name()='Description']").Item(0)?.InnerText.ToString();
            string companyId = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='SenderParty']/*[local-name()='PartyTaxScheme']/*[local-name()='CompanyID']").Item(0)?.InnerText.ToString();
            string modoOperacion = string.Empty;
            string softwareId = xmlParser.Fields["SoftwareId"].ToString();

            responses.Add(new ValidateListResponse
            {
                IsValid = true,
                Mandatory = true,
                ErrorCode = "100",
                ErrorMessage = "Evento ValidateReferenceAttorney referenciado correctamente",
                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
            });

            //Validaciones previas secciones DocumentResponse / DocumentReference 1 y 2
            if (listID != "3")
            {
                var validateCufeReferenceAttorney = ValidateCufeReferenceAttorney(xmlParser);
                if (validateCufeReferenceAttorney != null)
                {
                    validate = false;
                    foreach (var item in validateCufeReferenceAttorney)
                    {              
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = item.IsValid,
                            Mandatory = item.Mandatory,
                            ErrorCode = item.ErrorCode,
                            ErrorMessage = item.ErrorMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }
            }

            //Validacion senderParty igual a  senderParty / PowerOfAttorney ID
            if (!string.IsNullOrWhiteSpace(senderPowerOfAttorney))
            {
                long number1 = 0;
                bool valNumber = long.TryParse(senderPowerOfAttorney, out number1);
              
                if (valNumber && senderCode != senderPowerOfAttorney)
                {
                    validate = false;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAF32"),
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF32"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            long numberDesc = 0;
            bool valNumberDesc = long.TryParse(descriptionSender, out numberDesc);

            if (!string.IsNullOrWhiteSpace(descriptionSender) || !valNumberDesc)
            {               
                //Validacion descripcion Mandante
                switch (senderId)
                {
                    case "Mandante-FE":

                        if (descriptionSender != "Mandante Facturador Electrónico")
                        {
                            validate = false;
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAF35"),
                                ErrorMessage = "No fue informado el literal “Mandante Facturador Electrónico” de acuerdo con el campo “Descripcion” de la lista 13.2.5 Tipo de Mandante",
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }


                        break;
                    case "Mandante-LT":
                        if (descriptionSender != "Mandante Legitimo Tenedor")
                        {
                            validate = false;
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAF35"),
                                ErrorMessage = "No fue informado el literal “Mandante Legitimo Tenedor” de acuerdo con el campo “Descripcion” de la lista 13.2.5 Tipo de Mandante",
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        break;
                    case "Mandante-AV":
                        if (descriptionSender != "Mandante Aval")
                        {
                            validate = false;
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAF35"),
                                ErrorMessage = "No fue informado el literal “Mandante Aval” de acuerdo con el campo “Descripcion” de la lista 13.2.5 Tipo de Mandante",
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        break;
                    case "Mandante-AD":
                        if (descriptionSender != "Mandante Adquirente/Deudor")
                        {
                            validate = false;
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAF35"),
                                ErrorMessage = "No fue informado el literal “Mandante Adquirente/Deudor” de acuerdo con el campo “Descripcion” de la lista 13.2.5 Tipo de Mandante",
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        break;
                }

            }
         
            //Valida descripcion Mandatario 
            switch (factorTemp)
            {
                case "M-SN-e":
                    modoOperacion = "SNE";
                    if (description != "Mandatario Sistema de Negociación Electrónica")
                    {
                        validate = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAH65"),
                            ErrorMessage = "No fue informado el literal “Mandatario Sistema de Negociación Electrónica” de acuerdo con el campo “Descripcion” de la lista 13.2.8 Tipo de Mandatario",                            
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    break;
                case "M-Factor":
                    modoOperacion = "F";
                    if (description != "Mandatario Factor")
                    {
                        validate = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAH65"),
                            ErrorMessage = "No fue informado el literal “Mandatario Factor” de acuerdo con el campo “Descripcion” de la lista 13.2.8 Tipo de Mandatario",                            
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    break;
                case "M-PT":
                    modoOperacion = "PT";
                    if (description != "Mandatario Proveedor Tecnológico")
                    {
                        validate = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAH65"),
                            ErrorMessage = "No fue informado el literal “Mandatario Proveedor Tecnológico” de acuerdo con el campo “Descripcion” de la lista 13.2.8 Tipo de Mandatario",                           
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    break;
            }
            string actor = modoOperacion;
            //Valida se encuetre habilitado Modo Operacion RadianOperation
            var globalRadianOperation = TableManagerGlobalRadianOperations.FindhByPartitionKeyRadianStatus<GlobalRadianOperations>(
                issuerPartyCode, false, "Habilitado", softwareId);

            //Validacion habilitado Modo Operacion RadianOperation y providerID igual a  IssuerParty / PowerOfAttorney / ID          
            if (globalRadianOperation == null || (issuerPartyCode != providerCode))
            {
                validate = false;
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "AAH62b",
                    ErrorMessage = "El número de documento no corresponde a un participante habilitado en la plataforma RADIAN (PT/Factor/SNE).",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });               
            }
            else
            {
                if (!globalRadianOperation.TecnologicalSupplier && !globalRadianOperation.Factor && !globalRadianOperation.NegotiationSystem)
                {
                    validate = false;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "AAH62b",
                        ErrorMessage = "El número de documento no corresponde a un participante habilitado en la plataforma RADIAN (PT/Factor/SNE).",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            //Valida existe Contrato de mandatos entre las partes
            if (operacionMandato == "2")
            {
                if (AttachmentBase64List.Count < 2)
                {
                    validate = false;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAH84"),
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH84"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
                else
                {
                    for (int i = 0; i < AttachmentBase64List.Count && validate; i++)
                    {
                        string AttachmentBase64 = AttachmentBase64List.Item(i).SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='LineResponse']/*[local-name()='LineReference']/*[local-name()='DocumentReference']/*[local-name()='Attachment']/*[local-name()='EmbeddedDocumentBinaryObject']").Item(i)?.InnerText.ToString();
                        if (!IsBase64(AttachmentBase64))
                        {
                            validate = false;
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAH84"),
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH84"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                            break;
                        }
                    }
                }               
            }
                              
            //Valida Mandato si es Ilimitado o Limitado
            if (customizationID == "432" || customizationID == "434")
            {
                startDateAttorney = string.Empty;
                endDate = string.Empty;
            }
            else if (customizationID == "431" || customizationID == "433")
            {
                startDateAttorney = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='DocumentReference']/*[local-name()='ValidityPeriod']/*[local-name()='StartDate']").Item(0)?.InnerText.ToString();
                endDate = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='DocumentReference']/*[local-name()='ValidityPeriod']/*[local-name()='EndDate']").Item(0)?.InnerText.ToString();
            }
          

            var facultitys = TableManagerGlobalAttorneyFacultity.FindAll<GlobalAttorneyFacultity>();
            //Si existen mas de 2 documentResposne
            int listReference = cufeListReference.Count == 1 ? cufeList.Count : cufeListReference.Count;

            //Grupo de información alcances para el mandato sobre los CUFE.
            for (int i = 1; i < listReference && i < attorneyLimit  && validate; i++)
            {
                //Valida facultades madato 
                AttorneyModel attorneyModel = new AttorneyModel();
                string[] tempCode = new string[0];

                if (cufeList.Count > 2)
                {
                    string code = cufeList.Item(i).SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='Response']/*[local-name()='ResponseCode']").Item(i)?.InnerText.ToString();
                    if (!string.IsNullOrWhiteSpace(code))
                    {
                        tempCode = code.Split(';');
                    }
                }
                else
                {
                    string code = cufeList.Item(1).SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='Response']/*[local-name()='ResponseCode']").Item(1)?.InnerText.ToString();
                    if (!string.IsNullOrWhiteSpace(code))
                    {
                        tempCode = code.Split(';');
                    }
                }
                              
                bool codeExist = false;
                foreach (string codeAttorney in tempCode)
                {
                    //Valida exitan codigos de facultades asignadas mandato
                    if (facultitys.SingleOrDefault(t => t.PartitionKey == codeAttorney) == null)
                    {
                        validate = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAL02"),
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAL02"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                        codeExist = true;
                    //Valida codigos facultades mandato General - Limitado
                    string[] tempCodeAttorney = codeAttorney.Split('-');
                    if (modoOperacion == tempCodeAttorney[1])
                    {
                        if((customizationID == "431" || customizationID == "432") && tempCodeAttorney[0]=="ALL17")
                        {
                            codeExist = true;
                        }
                        else if((customizationID == "433" || customizationID == "434") && tempCodeAttorney[0] == "MR91")
                        {
                            codeExist = true;
                        }
                        if (attorneyModel.facultityCode == null)
                        {
                            attorneyModel.facultityCode += tempCodeAttorney[0];
                        }
                        else
                        {
                            attorneyModel.facultityCode += (";" + tempCodeAttorney[0]);
                        }
                    }
                    else
                    {
                        validate = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAL02"),
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAL02"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }
                if(!codeExist)
                {
                    validate = false;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAL02"),
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAL02"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
                //Solo si existe información referenciada del CUFE
                if (listID != "3")
                {
                    if (cufeList.Count >= 2)
                    {
                        attorneyModel.cufe = cufeList.Item(i).SelectNodes("//*[local-name()='DocumentReference']/*[local-name()='UUID']").Item(i)?.InnerText.ToString();
                        attorneyModel.idDocumentReference = cufeList.Item(i).SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='DocumentReference']/*[local-name()='ID']").Item(i)?.InnerText.ToString();
                        attorneyModel.idTypeDocumentReference = cufeList.Item(i).SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='DocumentReference']/*[local-name()='DocumentTypeCode']").Item(i)?.InnerText.ToString();
                    }
                    else
                    {
                        attorneyModel.cufe = cufeListReference.Item(i).SelectNodes("//*[local-name()='UUID']").Item(i)?.InnerText.ToString();
                        attorneyModel.idDocumentReference = cufeListReference.Item(i-1).SelectNodes("//*[local-name()='DocumentReference']/*[local-name()='ID']").Item(i-1)?.InnerText.ToString();
                        attorneyModel.idTypeDocumentReference = cufeListReference.Item(i).SelectNodes("//*[local-name()='DocumentTypeCode']").Item(i)?.InnerText.ToString();
                    }                                      
                    if (validate)
                    {
                        TableManager TableManagerGlobalDocHolderExchange = new TableManager("GlobalDocHolderExchange");
                        TableManager TableManagerGlobalDocValidatorDocumentMeta = new TableManager("GlobalDocValidatorDocumentMeta");
                        var docHolderExchange = TableManagerGlobalDocHolderExchange.FindhByCufeExchange<GlobalDocHolderExchange>(attorneyModel.cufe.ToLower(), true);
                        if(docHolderExchange != null)
                        {
                            //Existe mas de un legitimo tenedor requiere un mandatario
                            string[] endosatarios = docHolderExchange.PartyLegalEntity.Split('|');
                            if (endosatarios.Length == 1)
                            {
                                if(docHolderExchange.PartyLegalEntity == companyId)
                                {
                                    attorney.Add(attorneyModel);
                                }
                                else
                                {
                                    validate = false;
                                    responses.Add(new ValidateListResponse
                                    {
                                        IsValid = false,
                                        Mandatory = true,
                                        ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAL07"),
                                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAL07"),
                                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                    });
                                }
                            }
                            else
                            {
                                validate = false;
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = false,
                                    Mandatory = true,
                                    ErrorCode = "89",
                                    ErrorMessage = "Factura cuenta con mas de un Legitimo tenedor, no es posible crear un mandato",
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                        else
                        {
                            var documentMetaCUFE = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(attorneyModel.cufe, attorneyModel.cufe);
                            if (companyId == documentMetaCUFE.SenderCode)
                            {
                                attorney.Add(attorneyModel);
                            }
                            else
                            {
                                validate = false;
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = false,
                                    Mandatory = true,
                                    ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAL07"),
                                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAL07"),
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }                      
                    }                                       
                }
                else
                {
                    attorneyModel.cufe = "01";
                    attorney.Add(attorneyModel);
                }
            }
           
            foreach (var r in responses)
                r.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
            return responses;
        }
        #endregion

        #region Number range validation
        public List<ValidateListResponse> ValidateNumberingRange(NumberRangeModel numberRangeModel, string trackId)
        {
            DateTime startDate = DateTime.UtcNow;
            trackId = trackId.ToLower();
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);

            var invoiceAuthorization = numberRangeModel.InvoiceAuthorization;
            var softwareId = numberRangeModel.SoftwareId;
            var senderCode = numberRangeModel.SenderCode;

            GlobalTestSetResult testSetResult = null;
            List<GlobalTestSetResult> testSetResults = null;
            GlobalNumberRange range = null;
            List<GlobalNumberRange> ranges = GetNumberRangeInstanceCache(senderCode);

            if (ConfigurationManager.GetValue("Environment") == "Hab")
            {
                testSetResults = tableManagerTestSetResult.FindByPartition<GlobalTestSetResult>(senderCode);

                if (softwareId == ConfigurationManager.GetValue("BillerSoftwareId"))
                    range = ranges.FirstOrDefault(r => r.Serie == "SETG");
                else
                {
                    var contributor = GetContributorInstanceCache(senderCode);
                    testSetResult = testSetResults.FirstOrDefault(t => !t.Deleted && t.RowKey == $"{contributor?.TypeId}|{softwareId}");
                    if ((contributor?.TypeId == (int)ContributorType.Biller || contributor?.TypeId == (int)ContributorType.Provider) && testSetResult?.OperationModeId == (int)OperationMode.Own)
                        range = ranges.FirstOrDefault(r => r.Serie == "SETP");
                    else if ((contributor?.TypeId == (int)ContributorType.Biller || contributor?.TypeId == (int)ContributorType.Provider) && testSetResult?.OperationModeId == (int)OperationMode.Provider)
                        range = ranges.FirstOrDefault(r => r.Serie == "SETT");
                }
            }

            if (ConfigurationManager.GetValue("Environment") == "Prod" || ConfigurationManager.GetValue("Environment") == "Test")
            {
                var documentType = documentMeta?.DocumentTypeId;
                if(Convert.ToInt32(documentType) == (int)DocumentType.DocumentSupportInvoice)
                {
                    var rk = $"{documentMeta?.Serie}|{documentType}|{documentMeta?.InvoiceAuthorization}";
                    range = ranges?.FirstOrDefault(r => r.PartitionKey == documentMeta.SenderCode && r.RowKey == rk);
                }
                else
                {
                    if (new string[] { "01", "02", "04", "09", "11" }.Contains(documentType)) documentType = "01";
                    var rk = $"{documentMeta?.Serie}|{documentType}|{documentMeta?.InvoiceAuthorization}";
                    range = ranges?.FirstOrDefault(r => r.PartitionKey == documentMeta.SenderCode && r.RowKey == rk);
                }               
            }

            // If dont found range return
            if (range == null)
            {
                if(Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice)
                {
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "DSAD05d", ErrorMessage = "Número de documento soporte no está contenido en el rango de numeración autorizado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "DSAD05e", ErrorMessage = "Número de documento soporte no existe para el número de autorización.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    
                    return responses;
                }               
                else
                {
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAD05e", ErrorMessage = "Número de factura no existe para el número de autorización.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    return responses;
                }
               
            }
            string errorCode = Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice ? "DSAB05b" : "FAB05b";
            string messaCode = Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice
                ? "Número de la autorización de la numeración no corresponde a un número de autorización de este contribuyente vendedor para este Proveedor de Autorización"
                : "Número de la resolución que autoriza la numeración no corresponde a un número de resolución de este contribuyente emisor para este Proveedor de Autorización.";
            // Check invoice authorization
            if (range.ResolutionNumber == invoiceAuthorization)
                responses.Add(new ValidateListResponse { 
                    IsValid = true, 
                    Mandatory = true, 
                    ErrorCode = errorCode,
                    ErrorMessage = "Número de la resolución que autoriza la numeración corresponde a un número de resolución de este contribuyente emisor para este Proveedor de Autorización.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            else
                responses.Add(new ValidateListResponse { 
                    IsValid = false,
                    Mandatory = true, 
                    ErrorCode = errorCode, 
                    ErrorMessage = messaCode, 
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

            if (ConfigurationManager.GetValue("Environment") == "Prod")
            {
                // Check software
                if (range.SoftwareId == softwareId)
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAB05c", ErrorMessage = "El indentificador del software corresponde al rango de numeración informado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                else
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAB05c", ErrorMessage = "El indentificador del software no corresponde al rango de numeración informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            }

            //
            DateTime.TryParse(numberRangeModel.StartDate, out DateTime _startDate);
            int.TryParse(_startDate.ToString("yyyyMMdd"), out int fromDateNumber);
            if (range.ValidDateNumberFrom == fromDateNumber)
            {
                if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice)
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "DSAB07b", ErrorMessage = "Fecha inicial del rango de numeración informado corresponde a la fecha inicial de los rangos vigente para el contribuyente.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                else
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAB07b", ErrorMessage = "Fecha inicial del rango de numeración informado corresponde a la fecha inicial de los rangos vigente para el contribuyente.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            }
            else
            {
                if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice)
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "DSAB07b", ErrorMessage = "Fecha inicial del rango de numeración informado NO corresponde a la fecha inicial de los rangos vigente para el contribuyente.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                else
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAB07b", ErrorMessage = "Fecha inicial del rango de numeración informado no corresponde a la fecha inicial de los rangos vigente para el contribuyente.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            }


            //Valida fecha de emision posterior a la fecha de final de autorizacion 
            if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice)
            {               
                DateTime.TryParse(numberRangeModel.SigningTime, out DateTime endNumberEmision);
                int.TryParse(endNumberEmision.ToString("yyyyMMdd"), out int dateNumberEmision);
                if (dateNumberEmision < range.ValidDateNumberTo)
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = true,
                        Mandatory = true,
                        ErrorCode = "DSAB08a",
                        ErrorMessage = "Fecha emision referenciada correctamente",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                else
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "DSAB08a",
                        ErrorMessage = "Fecha de emisión posterior a la fecha final de la autorización de numeración",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
            }

            //
            DateTime.TryParse(numberRangeModel.EndDate, out DateTime endDate);
            int.TryParse(endDate.ToString("yyyyMMdd"), out int toDateNumber);
            string errorCodeRange = Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice ? "DSAB08b" : "FAB08b";
            if (range.ValidDateNumberTo == toDateNumber)
                responses.Add(new ValidateListResponse {
                    IsValid = true, 
                    Mandatory = true, 
                    ErrorCode = errorCodeRange, 
                    ErrorMessage = "Fecha final del rango de numeración informado corresponde a la fecha final de los rangos vigente para el contribuyente.", 
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            else
                responses.Add(new ValidateListResponse {
                    IsValid = false, 
                    Mandatory = true, 
                    ErrorCode = errorCodeRange, 
                    ErrorMessage = "Fecha final del rango de numeración informado no corresponde a la fecha final de los rangos vigente para el contribuyente.", 
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });


            if (string.IsNullOrEmpty(range.Serie)) range.Serie = "";
            string errorCodeSerie = Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice ? "DSAB10b" : "FAB10b";
            if (range.Serie == numberRangeModel.Serie)
                responses.Add(new ValidateListResponse { 
                    IsValid = true, 
                    Mandatory = false, 
                    ErrorCode = errorCodeSerie, 
                    ErrorMessage = "El prefijo corresponde al prefijo autorizado en la resolución.", 
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            else
                responses.Add(new ValidateListResponse { 
                    IsValid = false, 
                    Mandatory = false, 
                    ErrorCode = errorCodeSerie, 
                    ErrorMessage = "El prefijo no corresponde al prefijo de la autorización de numeración", 
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

            long.TryParse(numberRangeModel.StartNumber, out long fromNumber);            
            string errorCodeModel = (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice) ? "DSAB11b" : "FAB11b";
          
            string messageCodeModel = (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice) 
                ? "Valor inicial del rango de numeración informado no corresponde a un valor inicial de los rangos vigentes para el contribuyente vendedor"
                : "Valor inicial del rango de numeración informado no corresponde a un valor inicial de los rangos vigente para el contribuyente emisor.";
                
            if (range.FromNumber == fromNumber)
                responses.Add(new ValidateListResponse { 
                    IsValid = true, 
                    Mandatory = true, 
                    ErrorCode = errorCodeModel, 
                    ErrorMessage = "Valor inicial del rango de numeración informado corresponde a un valor inicial de los rangos vigente para el contribuyente emisor.", 
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            else
                responses.Add(new ValidateListResponse { 
                    IsValid = false, 
                    Mandatory = true, 
                    ErrorCode = errorCodeModel, 
                    ErrorMessage = messageCodeModel,
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });           

            long.TryParse(numberRangeModel.EndNumber, out long endNumber);           
            string errorCodeModel2 = Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice ? "DSAB12b" : "FAB12b";           
            string messageCodeModel2 = Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice
                ? "Valor final del rango de numeración informado no corresponde a un valor final de los rangos vigentes para el contribuyente vendedor"
                : "Valor final del rango de numeración informado no corresponde a un valor final de los rangos vigentes para el contribuyente emisor.";

            if (range.ToNumber == endNumber)
                responses.Add(new ValidateListResponse { 
                    IsValid = true, 
                    Mandatory = true, 
                    ErrorCode = errorCodeModel2,
                    ErrorMessage = "Valor final del rango de numeración informado corresponde a un valor final de los rangos vigentes para el contribuyente emisor.", 
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            else
                responses.Add(new ValidateListResponse { 
                    IsValid = false, 
                    Mandatory = true, 
                    ErrorCode = errorCodeModel2,
                    ErrorMessage = messageCodeModel2,
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });


            foreach (var r in responses)
                r.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
            return responses;
        }
        #endregion

        #region Sinature validations
        public List<ValidateListResponse> ValidateSign(IEnumerable<X509Certificate> chainCertificates, IEnumerable<X509Crl> crls)
        {
            DateTime startDate = DateTime.UtcNow;
            var validateResponses = new List<ValidateListResponse>();

            if (chainCertificates == null)
                throw new ArgumentNullException(nameof(chainCertificates));

            var certificate = GetPrimaryCertificate();

            if (certificate == null)
            {
                validateResponses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "ZD06", ErrorMessage = "Cadena de confianza del certificado digital no se pudo validar. [Missing X509Certificate node]", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                return validateResponses;
            }

            if (DateTime.Now < certificate.NotBefore)
            {
                validateResponses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "ZD07", ErrorMessage = $"Certificado aún no se encuentra vigente.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                return validateResponses;
            }

            if (DateTime.Now > certificate.NotAfter)
            {
                validateResponses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "ZD07", ErrorMessage = $"Certificado se encuentra expirado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                return validateResponses;
            }

            bool.TryParse(Environment.GetEnvironmentVariable("RejectUntrustedCertificate"), out bool rejectUntrustedCertificate);

            if (certificate.IsTrusted(chainCertificates))
                validateResponses.Add(new ValidateListResponse { IsValid = true, Mandatory = rejectUntrustedCertificate, ErrorCode = "ZD05", ErrorMessage = "Cadena de confianza del certificado digital correcta.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            else
                validateResponses.Add(new ValidateListResponse { IsValid = false, Mandatory = rejectUntrustedCertificate, ErrorCode = "ZD05", ErrorMessage = ConfigurationManager.GetValue("UnTrustedCertificateMessage"), ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

            if (certificate.IsRevoked(crls))
                validateResponses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "ZD04", ErrorMessage = "Certificado de la firma revocado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            else
                validateResponses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "ZD04", ErrorMessage = "Certificado de la firma se encuentra vigente.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });


            return validateResponses;
        }
        #endregion

        #region ValidateSignXades
        public List<ValidateListResponse> ValidateSignXades()
        {
            DateTime startDate = DateTime.UtcNow;
            var validateResponses = new List<ValidateListResponse>();
            var xadesVerifyDsl = new XadesVerify();
            var results = xadesVerifyDsl.PerformAndGetResults(_xmlDocument);
            validateResponses.AddRange(results.Select(r => new ValidateListResponse { IsValid = r.IsValid, Mandatory = true, ErrorCode = "ZE02", ErrorMessage = r.Message, ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds }));
            return validateResponses;
        }
        #endregion

        #region Software validation
        public ValidateListResponse ValidateSoftware(SoftwareModel softwareModel, string trackId, NominaModel nominaModel = null)
        {
            DateTime startDate = DateTime.UtcNow;
            trackId = trackId.ToLower();
            var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
            var response = new ValidateListResponse { IsValid = false, Mandatory = true, ErrorMessage = $"Huella no corresponde a un software autorizado para este OFE." };
            response.ErrorCode = "FAB27b";
            if (documentMeta.DocumentTypeId == "91")
                response.ErrorCode = "CAB27b";
            if (documentMeta.DocumentTypeId == "92")
                response.ErrorCode = "DAB27b";
            if (documentMeta.DocumentTypeId == "96")
                response.ErrorCode = "AAB27b";
            if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayroll)
                response.ErrorCode = "NIE020";
            if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayrollAdjustments)
                response.ErrorCode = "NIAE020";
            if ((Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice))
                response.ErrorCode = "DSAB27b";

            if ( Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayroll
                ||Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayrollAdjustments)            
                response.ErrorMessage = "Se debe indicar el Software Security Code según la definición establecida.";

            
            var number = (softwareModel != null) ? softwareModel.SerieAndNumber : nominaModel.SerieAndNumber;
            var softwareId = (softwareModel != null) ? softwareModel.SoftwareId : nominaModel.ProveedorSoftwareID;
            var SoftwareSecurityCode = (softwareModel != null) ? softwareModel.SoftwareSecurityCode : nominaModel.ProveedorSoftwareSC;

            var billerSoftwareId = ConfigurationManager.GetValue("BillerSoftwareId");
            var billerSoftwarePin = ConfigurationManager.GetValue("BillerSoftwarePin");

            string hash = "";
            if (softwareId == billerSoftwareId)
                hash = $"{billerSoftwareId}{billerSoftwarePin}{number}".EncryptSHA384();
            else if(!string.IsNullOrWhiteSpace(softwareId))
            {
                var software = GetSoftwareInstanceCache(softwareId);
                if (software == null)
                {
                    response.ErrorCode = "FAB24b";
                    response.ErrorMessage = "El identificador del software asignado cuando el software se activa en el Sistema de Facturación Electrónica no corresponde a un software autorizado para este OFE.";
                    if (documentMeta.DocumentTypeId == "91")
                        response.ErrorCode = "CAB24b";
                    if (documentMeta.DocumentTypeId == "92")
                        response.ErrorCode = "DAB24b";
                    if (documentMeta.DocumentTypeId == "96")
                        response.ErrorCode = "AAB24b";
                    if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayroll)
                        response.ErrorCode = "NIE019";
                    if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayrollAdjustments)
                        response.ErrorCode = "NIAE019";
                    if ((Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice))
                        response.ErrorCode = "DSAB24b";

                    if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayroll
                         || Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayrollAdjustments)                    
                        response.ErrorMessage = "Identificador del software asignado cuando el software se activa en el Sistema de Documento Soporte de Pago de Nómina Electrónica, debe corresponder a un software autorizado para este Emisor";
               
                    response.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
                    return response;
                }
                else if (software.StatusId == (int)SoftwareStatus.Inactive)
                {
                    response.ErrorCode = "FAB24c";
                    response.ErrorMessage = "Identificador del software informado se encuentra inactivo.";
                    if (documentMeta.DocumentTypeId == "91")
                        response.ErrorCode = "CAB24c";
                    if (documentMeta.DocumentTypeId == "92")
                        response.ErrorCode = "DAB24c";
                    if (documentMeta.DocumentTypeId == "96")
                        response.ErrorCode = "AAB24c";
                    if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayroll)
                        response.ErrorCode = "NIE019";
                    if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayrollAdjustments)
                        response.ErrorCode = "NIAE019";
                    if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.DocumentSupportInvoice)
                        response.ErrorCode = "DSAB24c";

                    if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayroll
                        || Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayrollAdjustments)                    
                        response.ErrorMessage = "Identificador del software asignado cuando el software se activa en el Sistema de Documento Soporte de Pago de Nómina Electrónica, debe corresponder a un software autorizado para este Emisor";                 

                    response.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
                    return response;
                }
                hash = $"{software.Id}{software.Pin}{number}".EncryptSHA384();
            }

            if (SoftwareSecurityCode.ToLower() == hash)
            {
                response.IsValid = true;
                response.ErrorMessage = "Huella del software correcta.";
            }

            response.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
            return response;
        }
        #endregion

        #region Tax level code validation
        public List<ValidateListResponse> ValidateTaxLevelCodes(Dictionary<string, string> dictionary)
        {
            DateTime startDate = DateTime.UtcNow;
            var responses = new List<ValidateListResponse>();

            var typeListInstance = GetTypeListInstanceCache();
            var typeListvalues = typeListInstance.Value.Split(';');

            //Get xpath values
            var xpathValues = GetXpathValues(dictionary);

            //Sender tax level code validation
            var isValid = true;
            var senderTaxLevelCodes = xpathValues["SenderTaxLevelCodes"].Split(';');
            foreach (var code in senderTaxLevelCodes)
                if (!typeListvalues.Contains(code))
                {
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAJ26", ErrorMessage = "Responsabilidad informada por emisor no valida según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    isValid = false;
                    break;
                }
            if (isValid && senderTaxLevelCodes.Any())
                responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAJ26", ErrorMessage = "Responsabilidad informada por emisor válida según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

            //receiver tax level code validation
            isValid = true;
            var additionalAccountId = xpathValues["AdditionalAccountIds"];
            var receiverTaxLevelCodes = xpathValues["ReceiverTaxLevelCodes"].Split(';');
            foreach (var code in receiverTaxLevelCodes)
                if (!string.IsNullOrEmpty(code) && !typeListvalues.Contains(code))
                {
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAK26", ErrorMessage = "Responsabilidad informada para receptor no valida según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    isValid = false;
                    break;
                }
            if (isValid && receiverTaxLevelCodes.Any(r => !string.IsNullOrEmpty(r)))
                responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAK26", ErrorMessage = "Responsabilidad informada para receptor válida según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

            //delivery tax level code validation
            isValid = true;
            var deliveryTaxLevelCodes = xpathValues["DeliveryTaxLevelCodes"].Split(';');
            foreach (var code in deliveryTaxLevelCodes)
                if (!string.IsNullOrEmpty(code) && !typeListvalues.Contains(code))
                {
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = false, ErrorCode = "FAM37", ErrorMessage = "Responsabilidad informada para transportista no válido según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    isValid = false;
                    break;
                }
            if (isValid && deliveryTaxLevelCodes.Any(d => !string.IsNullOrEmpty(d)))
                responses.Add(new ValidateListResponse { IsValid = true, Mandatory = false, ErrorCode = "FAM37", ErrorMessage = "Responsabilidad informada para transportista válida según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

            isValid = true;
            var sheldHolderTaxLevelCodeItems = xpathValues["SheldHolderTaxLevelCodes"].Split('|');
            foreach (var item in sheldHolderTaxLevelCodeItems)
                if (!string.IsNullOrEmpty(item))
                {
                    var sheldHolderTaxLevelCodes = item.Split(';');
                    foreach (var code in sheldHolderTaxLevelCodes)
                        if (!typeListvalues.Contains(code))
                        {
                            responses.Add(new ValidateListResponse { IsValid = false, Mandatory = false, ErrorCode = "FAJ62", ErrorMessage = "Responsabilidad informada por participantes no válida según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                            isValid = false;
                            break;
                        }
                }
            if (isValid && sheldHolderTaxLevelCodeItems.Any(s => !string.IsNullOrEmpty(s)))
                responses.Add(new ValidateListResponse { IsValid = true, Mandatory = false, ErrorCode = "FAJ62", ErrorMessage = "Responsabilidad informada por participantes válida según lista.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

            foreach (var r in responses)
                r.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
            return responses;
        }
        #endregion

        #region Private methods
        private GlobalContributor GetContributorInstanceCache(string code)
        {
            var itemKey = $"contributor-{code}";
            GlobalContributor contributor = null;
            var cacheItem = InstanceCache.ContributorInstanceCache.GetCacheItem(itemKey);

            if (cacheItem == null)
            {
                contributor = contributorTableManager.Find<GlobalContributor>(code, code);
                if (contributor == null) return null;
                CacheItemPolicy policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(24)
                };
                InstanceCache.ContributorInstanceCache.Set(new CacheItem(itemKey, contributor), policy);
            }
            else
                contributor = (GlobalContributor)cacheItem.Value;

            return contributor;
        }
        #endregion

        private List<GlobalNumberRange> GetNumberRangeInstanceCache(string senderCode)
        {
            var env = ConfigurationManager.GetValue("Environment");
            List<GlobalNumberRange> numberRanges = new List<GlobalNumberRange>();
            var cacheItemKey = $"number-range-{senderCode}";
            if (env == "Hab")
                cacheItemKey = "SET";
            var cacheItem = InstanceCache.NumberRangesInstanceCache.GetCacheItem(cacheItemKey);
            if (cacheItem == null)
            {
                if (env == "Hab")
                    numberRanges = numberRangeTableManager.FindByPartition<GlobalNumberRange>("SET");

                if (env == "Prod" || env == "Test")
                    numberRanges = numberRangeTableManager.FindByPartition<GlobalNumberRange>(senderCode);
                CacheItemPolicy policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(1)
                };
                InstanceCache.NumberRangesInstanceCache.Set(new CacheItem(cacheItemKey, numberRanges), policy);
            }
            else
                numberRanges = (List<GlobalNumberRange>)cacheItem.Value;

            if (env == "Prod")
            {
                if (!numberRanges.Any(r => r.State == (long)NumberRangeState.Authorized))
                    return numberRanges;

                numberRanges = numberRanges.Where(n => n.State == (long)NumberRangeState.Authorized).ToList();
            }

            return numberRanges;
        }
        private GlobalSoftware GetSoftwareInstanceCache(string id)
        {
            var itemKey = id;
            GlobalSoftware software = null;
            var cacheItem = InstanceCache.SoftwareInstanceCache.GetCacheItem(itemKey);
            if (cacheItem == null)
            {
                software = softwareTableManager.Find<GlobalSoftware>(itemKey, itemKey);
                if (software == null) return null;
                CacheItemPolicy policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(24)
                };
                InstanceCache.SoftwareInstanceCache.Set(new CacheItem(itemKey, software), policy);
            }
            else
                software = (GlobalSoftware)cacheItem.Value;

            return software;
        }
        private GlobalTypeList GetTypeListInstanceCache()
        {
            GlobalTypeList typeList = null;
            List<GlobalTypeList> typesList;
            var cacheItem = InstanceCache.TypesListInstanceCache.GetCacheItem("TypesList");
            if (cacheItem == null)
            {
                typesList = typeListTableManager.FindByPartition<GlobalTypeList>("new-dian-ubl21");
                CacheItemPolicy policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(24)
                };
                InstanceCache.TypesListInstanceCache.Set(new CacheItem("TypesList", typesList), policy);
            }
            else
                typesList = (List<GlobalTypeList>)cacheItem.Value;

            typeList = typesList.FirstOrDefault(t => t.Name == "Tipo Responsabilidad");
            return typeList;

        }
        private X509Certificate GetPrimaryCertificate()
        {
            var x509CertificateString = _xmlDocument.GetElementsByTagName("ds:X509Certificate")[0] != null ? _xmlDocument.GetElementsByTagName("ds:X509Certificate")[0].InnerText : string.Empty;

            var primaryCertificate = new X509Certificate2(Convert.FromBase64String(x509CertificateString));
            return DotNetUtilities.FromX509Certificate(primaryCertificate);
        }
        private Dictionary<string, string> GetXpathValues(Dictionary<string, string> dictionary)
        {
            var newDictionary = new Dictionary<string, string>();
            foreach (var item in dictionary)
            {
                try
                {
                    if (_xmlDocument.SelectNodes(item.Value, _ns).Count == 1)
                        newDictionary[item.Key] = _xmlDocument.SelectSingleNode(item.Value, _ns)?.InnerText;
                    else
                    {
                        var values = string.Empty;
                        var nodes = _xmlDocument.SelectNodes(item.Value, _ns);
                        foreach (XmlNode node in nodes)
                        {
                            if (string.IsNullOrEmpty(values))
                                values = node.InnerText;
                            else
                                values = $"{values}|{node.InnerText}";
                        }
                        newDictionary[item.Key] = values;
                    }
                }
                catch (Exception ex)
                {
                    newDictionary[item.Key] = ex.Message;
                }
            }
            return newDictionary;
        }
        private static decimal TruncateDecimal(decimal value, int preision)
        {
            decimal step = (decimal)Math.Pow(10, preision);
            decimal tmp = Math.Truncate(step * value);
            return tmp / step;
        }

        #region ValidateDigitCode
        public bool ValidateDigitCode(string code, int digit)
        {
            try
            {
                int[] cousins = new int[] { 0, 3, 7, 13, 17, 19, 23, 29, 37, 41, 43, 47, 53, 59, 67, 71 };
                int dv, actualCousin, _totalOperacion = 0, residue, totalDigits = code.Length;

                for (int i = 0; i < totalDigits; i++)
                {
                    actualCousin = int.Parse(code.Substring(i, 1));
                    _totalOperacion += actualCousin * cousins[totalDigits - i];
                }
                residue = _totalOperacion % 11;
                if (residue > 1)
                    dv = 11 - residue;
                else
                    dv = residue;

                return dv == digit;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Validación de la Sección DocumentReference - CUFE Informado
        //Validación de la Sección DocumentReference - CUFE Informado TASK 804
        //Validación de la Sección DocumentReference - CUDE  del evento referenciado TASK 729
        public List<ValidateListResponse> ValidateDocumentReferencePrev(string trackId, string idDocumentReference, string eventCode,
            string documentTypeIdRef, string issuerPartyCode = null, string issuerPartyName = null)
        {
            string messageTypeId = (Convert.ToInt32(eventCode) == (int)EventStatus.Mandato)
                ? ConfigurationManager.GetValue("ErrorMessage_AAH09_043")
                : ConfigurationManager.GetValue("ErrorMessage_AAH09");
            string errorCodeReglaUUID = ConfigurationManager.GetValue("ErrorCode_AAH07");

            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            DateTime startDate = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(trackId))
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = errorCodeReglaUUID,
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH07"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            //Valida referencia evento terminacion de mandato
            if (Convert.ToInt32(eventCode) == (int)EventStatus.TerminacionMandato)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = true,
                    Mandatory = true,
                    ErrorCode = "100",
                    ErrorMessage = "Evento ValidateDocumentReferencePrev referenciado correctamente",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });

                var referenceAttorneyResult = TableManagerGlobalDocReferenceAttorney.FindDocumentReferenceAttorney<GlobalDocReferenceAttorney>(trackId.ToLower());
                if (referenceAttorneyResult == null)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAH07"),
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH07"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
                else
                {
                    //Valida ID documento Invoice/AR coincida con el CUFE/CUDE referenciado
                    if (referenceAttorneyResult.SerieAndNumber != idDocumentReference)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAH06"),
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH06_043"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    //Valida DocumentTypeCode coincida con el documento informado
                    if ("96" != documentTypeIdRef)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAH09"),
                            ErrorMessage = "No corresponde al literal '96'",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }
            }
            else
            {
                //Valida exista CUFE/CUDE en sistema DIAN
                var documentMeta = documentMetaTableManager.FindpartitionKey<GlobalDocValidatorDocumentMeta>(trackId.ToLower()).FirstOrDefault();
                if (documentMeta != null)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = true,
                        Mandatory = true,
                        ErrorCode = "100",
                        ErrorMessage = "Evento ValidateDocumentReferencePrev referenciado correctamente",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });

                    //Valida ID documento Invoice/AR coincida con el CUFE/CUDE referenciado
                    if (documentMeta.SerieAndNumber != idDocumentReference)
                    {
                        string message = (Convert.ToInt32(eventCode) == (int)EventStatus.Mandato
                            || Convert.ToInt32(eventCode) == (int)EventStatus.TerminacionMandato)
                            ? ConfigurationManager.GetValue("ErrorMessage_AAH06_043")
                            : ConfigurationManager.GetValue("ErrorMessage_AAH06");

                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAH06"),
                            ErrorMessage = message,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    //Valida DocumentTypeCode coincida con el documento informado
                    if (documentMeta.DocumentTypeId != documentTypeIdRef)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAH09"),
                            ErrorMessage = messageTypeId,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    if (Convert.ToInt32(eventCode) == (int)EventStatus.EndosoPropiedad ||
                       Convert.ToInt32(eventCode) == (int)EventStatus.EndosoGarantia ||
                       Convert.ToInt32(eventCode) == (int)EventStatus.EndosoProcuracion)
                    {
                        //Valida número de identificación informado igual al número del adquiriente en la factura referenciada
                        if (documentMeta.ReceiverCode != issuerPartyCode)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAH26b"),
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH26b"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        //Valida nombre o razon social informado igual al del adquiriente en la factura referenciada
                        if (documentMeta.ReceiverName != issuerPartyName)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAH25b"),
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH25b"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }

                        var responseListEndoso = ValidateTransactionCufe(trackId.ToLower());
                        if (responseListEndoso != null)
                        {
                            foreach (var item in responseListEndoso)
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = item.IsValid,
                                    Mandatory = item.Mandatory,
                                    ErrorCode = item.ErrorCode,
                                    ErrorMessage = item.ErrorMessage,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                    }
                }
                else
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = errorCodeReglaUUID,
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH07"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }
           
            return responses;
        }
        #endregion

        #region validation to emition to event
        public List<ValidateListResponse> ValidateEmitionEventPrev(RequestObjectEventPrev eventPrev, XmlParser xmlParserCufe, XmlParser xmlParserCude, NitModel nitModel)
        {
            bool validFor = false;
            string eventCode = eventPrev.EventCode;
            string successfulMessage = "Evento ValidateEmitionEventPrev referenciado correctamente";
            DateTime startDate = DateTime.UtcNow;
            GlobalDocValidatorDocument document = null;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();            
            ErrorCodeMessage errorCodeMessage = getErrorCodeMessage(eventCode);
            
            //Valida si el documento AR transmitido ya se encuentra aprobado            
            var documentMeta = documentMetaTableManager.FindDocumentReferenced<GlobalDocValidatorDocumentMeta>(eventPrev.TrackId.ToLower(), eventPrev.DocumentTypeId);

            switch (Convert.ToInt32(eventPrev.EventCode))
            {
                case (int)EventStatus.Rejected:
                    //Valida eventos previos Rechazo de la FEV
                    LogicalEventRadian logicalEventRadianRejected = new LogicalEventRadian();
                    var eventRadianRejected = logicalEventRadianRejected.ValidateRejectedEventPrev(documentMeta);
                    if (eventRadianRejected != null || eventRadianRejected.Count > 0)
                    {
                        foreach (var itemEventRadianRejected in eventRadianRejected)
                        {                           
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = itemEventRadianRejected.IsValid,
                                Mandatory = itemEventRadianRejected.Mandatory,
                                ErrorCode = itemEventRadianRejected.ErrorCode,
                                ErrorMessage = itemEventRadianRejected.ErrorMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    break;
                case (int)EventStatus.Receipt:
                    //Valida eventos previos Constancia de recibo del Bien de la FEV
                    LogicalEventRadian logicalEventRadianReceipt = new LogicalEventRadian();
                    var eventRadianReceipt = logicalEventRadianReceipt.ValidateReceiptEventPrev(documentMeta);
                    if (eventRadianReceipt != null || eventRadianReceipt.Count > 0)
                    {
                        foreach (var itemeventRadianReceipt in eventRadianReceipt)
                        {                           
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = itemeventRadianReceipt.IsValid,
                                Mandatory = itemeventRadianReceipt.Mandatory,
                                ErrorCode = itemeventRadianReceipt.ErrorCode,
                                ErrorMessage = itemeventRadianReceipt.ErrorMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    break;
                case (int)EventStatus.Accepted:
                    //Valida eventos previos Aceptacion expresa de la FEV
                    LogicalEventRadian logicalEventRadianAccepted = new LogicalEventRadian();
                    var eventRadianAccepted = logicalEventRadianAccepted.ValidateAcceptedEventPrev(documentMeta);
                    if (eventRadianAccepted != null || eventRadianAccepted.Count > 0)
                    {
                        foreach (var itemeventRadianAccepted in eventRadianAccepted)
                        {                          
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = itemeventRadianAccepted.IsValid,
                                Mandatory = itemeventRadianAccepted.Mandatory,
                                ErrorCode = itemeventRadianAccepted.ErrorCode,
                                ErrorMessage = itemeventRadianAccepted.ErrorMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    break;
                case (int)EventStatus.AceptacionTacita:
                    //Valida eventos previos Aceptacion Tacita de la FEV
                    LogicalEventRadian logicalEventRadianTacitAcceptance = new LogicalEventRadian();
                    var eventRadianTacitAcceptance = logicalEventRadianTacitAcceptance.ValidateTacitAcceptanceEventPrev(documentMeta);
                    if(eventRadianTacitAcceptance != null || eventRadianTacitAcceptance.Count > 0)
                    {
                        foreach (var itemEventRadianTacitAcceptance in eventRadianTacitAcceptance)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = itemEventRadianTacitAcceptance.IsValid,
                                Mandatory = itemEventRadianTacitAcceptance.Mandatory,
                                ErrorCode = itemEventRadianTacitAcceptance.ErrorCode,
                                ErrorMessage = itemEventRadianTacitAcceptance.ErrorMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    break;
                case (int)EventStatus.Avales:
                    //Valida eventos previos Aval
                    LogicalEventRadian logicalEventRadianAval = new LogicalEventRadian();
                    var eventRadianAval = logicalEventRadianAval.ValidateEndorsementEventPrev(documentMeta, xmlParserCufe, xmlParserCude);
                    if(eventRadianAval != null || eventRadianAval.Count > 0)
                    {
                        foreach (var itemEventRadianAval in eventRadianAval)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = itemEventRadianAval.IsValid,
                                Mandatory = itemEventRadianAval.Mandatory,
                                ErrorCode = itemEventRadianAval.ErrorCode,
                                ErrorMessage = itemEventRadianAval.ErrorMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    break;
                case (int)EventStatus.SolicitudDisponibilizacion:
                    //Valida eventos previos Solicitud Disponibilizacion
                    LogicalEventRadian logicalEventRadianDisponibilizacion = new LogicalEventRadian();
                    var eventRadianDisponibilizacion = logicalEventRadianDisponibilizacion.ValidateAvailabilityRequestEventPrev(documentMeta, xmlParserCufe, xmlParserCude, nitModel);
                    if (eventRadianDisponibilizacion != null || eventRadianDisponibilizacion.Count > 0)
                    {
                        foreach (var itemEventRadianDisponibilizacion in eventRadianDisponibilizacion)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = itemEventRadianDisponibilizacion.IsValid,
                                Mandatory = itemEventRadianDisponibilizacion.Mandatory,
                                ErrorCode = itemEventRadianDisponibilizacion.ErrorCode,
                                ErrorMessage = itemEventRadianDisponibilizacion.ErrorMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    break;
                case (int)EventStatus.EndosoPropiedad:
                    //Valida eventos previos Endoso en Propiedad
                    LogicalEventRadian logicalEventRadianEndosoPropiedad = new LogicalEventRadian();
                    var eventRadianEndosoPropiedad = logicalEventRadianEndosoPropiedad.ValidatePropertyEndorsement(documentMeta, eventPrev, xmlParserCufe, xmlParserCude, nitModel);
                    if (eventRadianEndosoPropiedad != null || eventRadianEndosoPropiedad.Count > 0)
                    {
                        foreach (var itemEventRadianEndosoPropiedad in eventRadianEndosoPropiedad)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = itemEventRadianEndosoPropiedad.IsValid,
                                Mandatory = itemEventRadianEndosoPropiedad.Mandatory,
                                ErrorCode = itemEventRadianEndosoPropiedad.ErrorCode,
                                ErrorMessage = itemEventRadianEndosoPropiedad.ErrorMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    break;
                case (int)EventStatus.EndosoGarantia:
                    //Valida eventos previos Endoso en Garantia
                    LogicalEventRadian logicalEventRadianEndosoGarantia = new LogicalEventRadian();
                    var eventRadianEndosoGarantia = logicalEventRadianEndosoGarantia.ValidateEndorsementGatantia(documentMeta, eventPrev, xmlParserCufe, xmlParserCude, nitModel);
                    if (eventRadianEndosoGarantia != null || eventRadianEndosoGarantia.Count > 0)
                    {
                        foreach (var itemEventRadianEndosoGarantia in eventRadianEndosoGarantia)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = itemEventRadianEndosoGarantia.IsValid,
                                Mandatory = itemEventRadianEndosoGarantia.Mandatory,
                                ErrorCode = itemEventRadianEndosoGarantia.ErrorCode,
                                ErrorMessage = itemEventRadianEndosoGarantia.ErrorMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    break;
                case (int)EventStatus.EndosoProcuracion:
                    //Valida eventos previos Endoso en Procuracion
                    LogicalEventRadian logicalEventRadianEndosoProcuracion = new LogicalEventRadian();
                    var eventRadianEndosoProcuracion = logicalEventRadianEndosoProcuracion.ValidateEndorsementProcurement(documentMeta, eventPrev, xmlParserCufe, xmlParserCude, nitModel);
                    if(eventRadianEndosoProcuracion != null || eventRadianEndosoProcuracion.Count > 0)
                    {
                        foreach (var itemEventRadianEndosoProcuracion in eventRadianEndosoProcuracion)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = itemEventRadianEndosoProcuracion.IsValid,
                                Mandatory = itemEventRadianEndosoProcuracion.Mandatory,
                                ErrorCode = itemEventRadianEndosoProcuracion.ErrorCode,
                                ErrorMessage = itemEventRadianEndosoProcuracion.ErrorMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    break;
                case (int)EventStatus.InvoiceOfferedForNegotiation:
                    //Valida eventos previos Cancelacion Endosos
                    LogicalEventRadian logicalEventRadianCancelaEndoso = new LogicalEventRadian();
                    var eventRadianCancelaEndoso = logicalEventRadianCancelaEndoso.ValidateEndorsementCancell(documentMeta, eventPrev, xmlParserCude);
                    if(eventRadianCancelaEndoso != null || eventRadianCancelaEndoso.Count > 0)
                    {
                        foreach (var itemEventRadianCancelaEndoso in eventRadianCancelaEndoso)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = itemEventRadianCancelaEndoso.IsValid,
                                Mandatory = itemEventRadianCancelaEndoso.Mandatory,
                                ErrorCode = itemEventRadianCancelaEndoso.ErrorCode,
                                ErrorMessage = itemEventRadianCancelaEndoso.ErrorMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    break;
                case (int)EventStatus.NegotiatedInvoice:
                    //Valida eventos previos Limitacion de Circulacion
                    LogicalEventRadian logicalEventRadianNegotiatedInvoice = new LogicalEventRadian();
                    var eventRadianNegotiatedInvoice = logicalEventRadianNegotiatedInvoice.ValidateNegotiatedInvoice(documentMeta);
                    if(eventRadianNegotiatedInvoice != null || eventRadianNegotiatedInvoice.Count > 0)
                    {
                        foreach (var itemEventRadianNegotiatedInvoice in eventRadianNegotiatedInvoice)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = itemEventRadianNegotiatedInvoice.IsValid,
                                Mandatory = itemEventRadianNegotiatedInvoice.Mandatory,
                                ErrorCode = itemEventRadianNegotiatedInvoice.ErrorCode,
                                ErrorMessage = itemEventRadianNegotiatedInvoice.ErrorMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    break;
                case (int)EventStatus.AnulacionLimitacionCirculacion:
                    //Valida eventos previos Anulacion de la Limitacion de Circulacion
                    LogicalEventRadian logicalEventRadianNegotiatedInvoiceCancell = new LogicalEventRadian();
                    var eventRadianNegotiatedInvoiceCancell = logicalEventRadianNegotiatedInvoiceCancell.ValidateNegotiatedInvoiceCancell(documentMeta);
                    if (eventRadianNegotiatedInvoiceCancell != null || eventRadianNegotiatedInvoiceCancell.Count > 0)
                    {
                        foreach (var itemEventRadianNegotiatedInvoiceCancell in eventRadianNegotiatedInvoiceCancell)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = itemEventRadianNegotiatedInvoiceCancell.IsValid,
                                Mandatory = itemEventRadianNegotiatedInvoiceCancell.Mandatory,
                                ErrorCode = itemEventRadianNegotiatedInvoiceCancell.ErrorCode,
                                ErrorMessage = itemEventRadianNegotiatedInvoiceCancell.ErrorMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    break;
                case (int)EventStatus.Mandato:
                    //Valida eventos previos Mandato / El mandato no cuenta con eventos previos
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = true,
                        Mandatory = true,
                        ErrorCode = "100",
                        ErrorMessage = successfulMessage,
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                    break;
                case (int)EventStatus.TerminacionMandato:
                    //Valida eventos previos Terminacion de Mandato
                    LogicalEventRadian logicalEventRadianMandatoCancell = new LogicalEventRadian();
                    var eventRadianMandatoCancell = logicalEventRadianMandatoCancell.ValidateMandatoCancell(eventPrev);
                    if(eventRadianMandatoCancell != null || eventRadianMandatoCancell.Count > 0)
                    {
                        foreach (var itemEventRadianMandatoCancell in eventRadianMandatoCancell)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = itemEventRadianMandatoCancell.IsValid,
                                Mandatory = itemEventRadianMandatoCancell.Mandatory,
                                ErrorCode = itemEventRadianMandatoCancell.ErrorCode,
                                ErrorMessage = itemEventRadianMandatoCancell.ErrorMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    break;
                case (int)EventStatus.NotificacionPagoTotalParcial:
                    //Valida eventos previos Pago parcial o Total
                    LogicalEventRadian logicalEventRadianPartialPayment = new LogicalEventRadian();
                    var eventRadianPartialPayment = logicalEventRadianPartialPayment.ValidatePartialPayment(documentMeta, eventPrev, xmlParserCude, nitModel);
                    if(eventRadianPartialPayment != null || eventRadianPartialPayment.Count > 0)
                    {
                        foreach (var itemEventRadianPartialPayment in eventRadianPartialPayment)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = itemEventRadianPartialPayment.IsValid,
                                Mandatory = itemEventRadianPartialPayment.Mandatory,
                                ErrorCode = itemEventRadianPartialPayment.ErrorCode,
                                ErrorMessage = itemEventRadianPartialPayment.ErrorMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    break;
                case (int)EventStatus.ValInfoPago:
                    //Valida eventos previos Informacion Pago}
                    LogicalEventRadian logicalEventRadianPaymentInfo = new LogicalEventRadian();
                    var eventRadianPaymentInfo = logicalEventRadianPaymentInfo.ValidatePaymetInfo(documentMeta);
                    if (eventRadianPaymentInfo != null || eventRadianPaymentInfo.Count > 0)
                    {
                        foreach (var itemEventRadianPaymentInfo in eventRadianPaymentInfo)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = itemEventRadianPaymentInfo.IsValid,
                                Mandatory = itemEventRadianPaymentInfo.Mandatory,
                                ErrorCode = itemEventRadianPaymentInfo.ErrorCode,
                                ErrorMessage = itemEventRadianPaymentInfo.ErrorMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    break;

            }
          
            foreach (var documentIdentifier in documentMeta)
            {
                document = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(documentIdentifier.Identifier, documentIdentifier.Identifier, documentIdentifier.PartitionKey);
                   
                if (documentMeta.Count >= 2)
                {
                    //Valida Evento registrado previamente para Fase I y Solicitud de primera disponibilizacion
                    if ((Convert.ToInt32(eventPrev.EventCode) >= 30 && Convert.ToInt32(eventPrev.EventCode) <= 34))
                    {
                        if (documentMeta.Any(t => t.EventCode == eventPrev.EventCode
                        && document != null && t.Identifier == document?.PartitionKey && string.IsNullOrEmpty(t.TestSetId)
                        ))
                        {                            
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = ConfigurationManager.GetValue("ErrorCode_LGC01"),
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC01"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }                                                                           
                }               
                else
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = true,
                        Mandatory = true,
                        ErrorCode = "100",
                        ErrorMessage = successfulMessage,
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }

            }
            return responses;
        }

        private ValidateListResponse ValidateAval(XmlParser xmlParserCufe, XmlParser xmlParserCude)
        {
            DateTime startDate = DateTime.UtcNow;
            string valueTotalInvoice = xmlParserCufe.TotalInvoice;

            XmlNodeList valueListSender = xmlParserCude.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']");
            int totalValueSender = 0;
            for (int i = 0; i < valueListSender.Count; i++)
            {
                string valueStockAmount = valueListSender.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CorporateStockAmount']").Item(i)?.InnerText.ToString();
                // Si no se reporta, el Avalista asume el valor del monto de quien respalda...
                if (string.IsNullOrWhiteSpace(valueStockAmount)) return null;

                totalValueSender += Int32.Parse(valueStockAmount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);

                // Si se reporta, pero en ceros (0), el Avalista asume el valor del monto de quien respalda...
                if (totalValueSender == 0) return null;
            }

            if (totalValueSender > Int32.Parse(valueTotalInvoice, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture))
            {
                return new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "AAH32c",
                    ErrorMessage = $"{(string)null} El valor reportado no es igual a la sumatoria del elemento SenderParty:CorporateStockAmount - IssuerParty:PartyLegalEntity:CorporateStockAmount",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                };
            }

            XmlNodeList valueListIssuerParty = xmlParserCude.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='IssuerParty']/*[local-name()='PartyLegalEntity']");
            // En caso de no indicarlo quedan garantizadas las obligaciones de todas las partes del título.
            if (valueListIssuerParty.Count <= 0) return null;

            int totalValueIssuerParty = 0;
            for (int i = 0; i < valueListIssuerParty.Count; i++)
            {
                string valueStockAmount = valueListIssuerParty.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='IssuerParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CorporateStockAmount']").Item(i)?.InnerText.ToString();
                if (!string.IsNullOrWhiteSpace(valueStockAmount)) totalValueIssuerParty += Int32.Parse(valueStockAmount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            }

            if (totalValueIssuerParty == 0) return null;

            if (totalValueIssuerParty != totalValueSender)
            {
                return new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "AAH32c",
                    ErrorMessage = $"{(string)null} El valor reportado no es igual a la sumatoria del elemento SenderParty:CorporateStockAmount - IssuerParty:PartyLegalEntity:CorporateStockAmount",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                };
            }

            return null;
        }
        #endregion

        #region Validación de la Sección prerrequisitos Solicitud Disponibilizacion
        public List<ValidateListResponse> EventApproveCufe(NitModel dataModel, EventApproveCufe.RequestObjectEventApproveCufe data)
        {
            DateTime startDate = DateTime.UtcNow;
            GlobalDocValidatorDocument document = null;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            var documentMeta = documentMetaTableManager.FindDocumentReferenced<GlobalDocValidatorDocumentMeta>(data.TrackId.ToLower(), data.DocumentTypeId);
            int count = 0;
            //Validación de la Sección prerrequisitos Solicitud Disponibilizacion Task 719
            foreach (var documentIdentifier in documentMeta)
            {
                document = documentValidatorTableManager.Find<GlobalDocValidatorDocument>(documentIdentifier.Identifier, documentIdentifier.Identifier);
                if (document != null)
                {
                    foreach (var eventCode in documentMeta)
                    {
                        switch (eventCode.EventCode)
                        {
                            case "030":
                            case "032":
                            case "033":
                            case "034":
                                if (eventCode.Identifier == document.PartitionKey)
                                {
                                    count++;
                                }
                                break;
                        }
                    }
                }
            }
            if (count >= 3)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = true,
                    Mandatory = true,
                    ErrorCode = "100",
                    ErrorMessage = "Evento EventApproveCufe referenciado correctamente",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }
            else
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = ConfigurationManager.GetValue("ErrorCode_LGC21"),
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC21"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }
            return responses;
        }
        #endregion

        #region ValidateSigningTime
        public List<ValidateListResponse> ValidateSigningTime(RequestObjectSigningTime data, XmlParser dataModel, NitModel nitModel, string paymentDueDateFE = null)
        {
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            int businessDays = 0;
            DateTime startDate = DateTime.UtcNow;
            DateTime dateNow = DateTime.UtcNow.Date;
            DateTime signingTimeEvent = Convert.ToDateTime(data.SigningTime).Date;
            string errorMessageSign = "Evento ValidateSigningTime referenciado correctamente";

            if (signingTimeEvent > dateNow)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "DC24",
                    ErrorMessage = "Error en el valor de la fecha y hora de firma. " +
                        $"NO corresponde al formato y/o el valor reportado es superior a la fecha del sistema.",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }
           
            ErrorCodeMessage errorCodeMessage = getErrorCodeMessage(data.EventCode);
            string errorCodeRef = data.EventCode == "030" ? errorCodeMessage.errorCodeSigningTimeAcuse : errorCodeMessage.errorCodeSigningTimeRecibo;
            string errorMesaageRef = data.EventCode == "030" ? errorCodeMessage.errorMessageigningTimeAcuse : errorCodeMessage.errorMessageigningTimeRecibo;

            if (data.EventCode == "043")
            {
                errorCodeRef = "DC24r";
                errorMesaageRef = "No se puede generar el evento mandato antes de la fecha de generación del documento referenciado";
            }
            else if(data.EventCode != "030" && data.EventCode != "032")
            {
                errorCodeRef = "DC24q";
                errorMesaageRef = "No se puede generar el evento Cancelación del endoso electrónico antes de la fecha de generación del documento referenciado.";
            }

            switch (int.Parse(data.EventCode))
            {
                case (int)EventStatus.Received:
                case (int)EventStatus.Receipt:
                case (int)EventStatus.InvoiceOfferedForNegotiation:
                case (int)EventStatus.Mandato:
                    DateTime dataSigningTime = Convert.ToDateTime(data.SigningTime);
                    DateTime modelSigningTime = Convert.ToDateTime(dataModel.SigningTime);
                    if (dataSigningTime >= modelSigningTime)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageSign,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = errorCodeRef,
                            ErrorMessage = errorMesaageRef,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }                   
                    break;
                case (int)EventStatus.Rejected:
                    businessDays = BusinessDaysHolidays.BusinessDaysUntil(Convert.ToDateTime(dataModel.SigningTime), Convert.ToDateTime(data.SigningTime));
                    responses.Add(businessDays > 3
                         ? new ValidateListResponse
                         {
                             IsValid = false,
                             Mandatory = true,
                             ErrorCode = "DC24z",
                             ErrorMessage =
                                "No se puede generar el evento de Reclamo  pasado los 3 días hábiles de la fecha de generación " +
                                "del evento Recibo del bien y prestación del servicio.",
                             ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                         }
                        : new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageSign,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    break;
                case (int)EventStatus.Accepted:
                    DateTime signingTimeAccepted = Convert.ToDateTime(data.SigningTime);
                    DateTime signingTimeReceipt = Convert.ToDateTime(dataModel.SigningTime);
                    businessDays = BusinessDaysHolidays.BusinessDaysUntil(signingTimeReceipt, signingTimeAccepted);
                    responses.Add(businessDays > 3
                        ? new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "DC24c",
                            ErrorMessage = "No se puede generar el evento pasado los 3 días hábiles de la fecha de generación " +
                            "del evento Recibo del bien y prestación del servicio.",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        }
                        : new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageSign,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    break;
                case (int)EventStatus.AceptacionTacita:
                    businessDays = BusinessDaysHolidays.BusinessDaysUntil(Convert.ToDateTime(dataModel.SigningTime), Convert.ToDateTime(data.SigningTime));
                    responses.Add(businessDays > 3
                        ? new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageSign,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        }
                        : new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "DC24e",
                            ErrorMessage = "No se puede generar el evento antes de los 3 días hábiles de la fecha de generación" +
                            " del evento Recibo del bien y prestación del servicio.",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    break;
                case (int)EventStatus.ValInfoPago:
                    DateTime signingTimeEvento = Convert.ToDateTime(data.SigningTime).Date;
                    DateTime endDatePaymentDueDate = Convert.ToDateTime(data.EndDate).Date;
                    DateTime paymentDueDateFactura = Convert.ToDateTime(dataModel.PaymentDueDate).Date;
                    if (signingTimeEvento > paymentDueDateFactura)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC55",
                            ErrorMessage = "No se puede registrar el evento ya que la fecha de firma es superior " +
                                "a la fecha de vencimiento de la factura electrónica de venta ",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        if (endDatePaymentDueDate == paymentDueDateFactura)
                        {
                            businessDays = BusinessDaysHolidays.BusinessDaysUntil(signingTimeEvento, endDatePaymentDueDate);
                            if (businessDays == 3)
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = true,
                                    Mandatory = true,
                                    ErrorCode = "100",
                                    ErrorMessage = errorMessageSign,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                            else if (businessDays < 3)
                            {
                                responses.Add(new ValidateListResponse
                                {

                                    IsValid = false,
                                    Mandatory = true,
                                    ErrorCode = "LGC54",
                                    ErrorMessage = "No se puede registrar el evento ya que la fecha de firma es inferior a 3 días " +
                                    "de la fecha de vencimiento de la factura electrónica de venta ",
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                            else if (businessDays > 3)
                            {
                                responses.Add(new ValidateListResponse
                                {

                                    IsValid = false,
                                    Mandatory = true,
                                    ErrorCode = "LGC56",
                                    ErrorMessage = "No se puede registrar el evento ya que la fecha de vencimiento de la factura electrónica de venta  " +
                                    "es superior a 3 días de la fecha de firma del evento ",
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                        else
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "AAH59",
                                ErrorMessage = "EndDate del evento no coincide con el PaymentDueDate de la factura referenciada",
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                  
                    break;
                case (int)EventStatus.SolicitudDisponibilizacion:
                    responses.Add(Convert.ToDateTime(data.SigningTime) > Convert.ToDateTime(dataModel.SigningTime)
                        ? new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageSign,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        }
                        : new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "DC24h",
                            ErrorMessage =
                                "No se puede generar el evento inscripción en el RADIAN de la factura electrónica de venta " +
                                "como título valor que circula en el territorio nacional antes de la fecha de generación del documento referenciado.",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });

                    // El evento debe incluir la fecha de Vencimiento de la Factura electrónica de Venta 
                    // (Debe validar el campo EndDate contra el campo  PaymentDueDate de la factura referenciada.)
                    if(!string.IsNullOrWhiteSpace(paymentDueDateFE))
                    {
                        responses.Add(Convert.ToDateTime(data.EndDate) == Convert.ToDateTime(paymentDueDateFE)
                        ? new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageSign,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        }
                        : new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAH59",
                            ErrorMessage = "EndDate del evento no coincide con el PaymentDueDate de la factura referenciada.",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "89",
                            ErrorMessage = "PaymentDueDate llega NULL",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    break;
                case (int)EventStatus.NegotiatedInvoice:
                case (int)EventStatus.AnulacionLimitacionCirculacion:
                    if (nitModel.CustomizationId == "361" || nitModel.CustomizationId == "362" ||
                       nitModel.CustomizationId == "363" || nitModel.CustomizationId == "364")
                    {
                        responses.Add(Convert.ToDateTime(data.SigningTime) > Convert.ToDateTime(dataModel.SigningTime)
                        ? new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageSign,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        }
                        : new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "DC24t",
                            ErrorMessage = "No se puede generar el evento limitación de circulación antes de la fecha de generación del documento referenciado.",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(Convert.ToDateTime(data.SigningTime) > Convert.ToDateTime(dataModel.SigningTime)
                        ? new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageSign,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        }
                        : new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "DC24t",
                            ErrorMessage = "No se puede generar el evento limitación de circulación antes de la fecha de generación del documento referenciado.",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    break;
                case (int)EventStatus.Avales:
                    if (nitModel.CustomizationId == "361" || nitModel.CustomizationId == "362")
                    {
                        responses.Add(Convert.ToDateTime(data.SigningTime) > Convert.ToDateTime(dataModel.SigningTime)
                       ? new ValidateListResponse
                       {
                           IsValid = true,
                           Mandatory = true,
                           ErrorCode = "100",
                           ErrorMessage = errorMessageSign,
                           ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                       }
                       : new ValidateListResponse
                       {
                           IsValid = false,
                           Mandatory = true,
                           ErrorCode = ConfigurationManager.GetValue("ErrorCode_DC24g"),
                           ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_DC24g"),
                           ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                       });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = ConfigurationManager.GetValue("ErrorCode_DC24g"),
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_DC24g"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    break;
                case (int)EventStatus.NotificacionPagoTotalParcial:
                    responses.Add(Convert.ToDateTime(data.SigningTime) > Convert.ToDateTime(dataModel.SigningTime)
                       ? new ValidateListResponse
                       {
                           IsValid = true,
                           Mandatory = true,
                           ErrorCode = "100",
                           ErrorMessage = errorMessageSign,
                           ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                       }
                       : new ValidateListResponse
                       {
                           IsValid = false,
                           Mandatory = true,
                           ErrorCode = "DC24x",
                           ErrorMessage =
                               "No se puede generar el evento Pago de la factura electrónica de venta " +
                               "como título valor antes de la fecha de generación del documento referenciado.",
                           ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                       });

                        // El evento debe incluir la fecha de Vencimiento de la Factura electrónica de Venta 
                        // (Debe validar el campo EndDate contra el campo  PaymentDueDate de la factura referenciada.)
                        DateTime endDatePaymentDueDateNotifica = Convert.ToDateTime(data.EndDate).Date;
                        DateTime paymentDueDateFacturaNotifica = Convert.ToDateTime(paymentDueDateFE).Date;

                        if (endDatePaymentDueDateNotifica == paymentDueDateFacturaNotifica)
                        {                           
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = errorMessageSign,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });                                                     
                        }
                        else
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAH59") + "",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH59"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }

                    break;
                case (int)EventStatus.EndosoGarantia:
                case (int)EventStatus.EndosoProcuracion:
                case (int)EventStatus.EndosoPropiedad:
                    DateTime signingTimeEndoso = Convert.ToDateTime(data.SigningTime);
                    DateTime signingTimeFEV = Convert.ToDateTime(dataModel.SigningTime);
                    string errorCode = string.Empty;
                    string errorMessage = string.Empty;
                    if ((int)EventStatus.EndosoPropiedad == Convert.ToInt32(data.EventCode))
                    {
                        errorCode = "DC24j";
                        errorMessage = "No se puede generar el evento endoso en propiedad antes de la fecha de generación del documento referenciado.";
                    }
                    else if ((int)EventStatus.EndosoGarantia == Convert.ToInt32(data.EventCode))
                    {
                        errorCode = "DC24l";
                        errorMessage = "No se puede generar el evento endoso en garantía antes de la fecha de generación del documento referenciado.";
                    }
                    else
                    {
                        errorCode = "DC24o";
                        errorMessage = "No se puede generar el evento endoso en procuración antes de la fecha de generación del documento referenciado.";
                    }


                    if (signingTimeEndoso.Date < signingTimeFEV.Date)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = errorCode,
                            ErrorMessage = errorMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = errorMessageSign,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    break;
                case (int)EventStatus.TerminacionMandato:
                    DateTime signingTime = Convert.ToDateTime(data.SigningTime);
                    //General por tiempo ilimitado_432 - limitado por tiempo ilimitado_434
                    if (nitModel.CustomizationId == "432" || nitModel.CustomizationId == "434") //que se mayor
                    {
                        DateTime dateMandato = Convert.ToDateTime(dataModel.SigningTime);
                        if (signingTime >= dateMandato)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = errorMessageSign,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        else
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "DC24s",
                                ErrorMessage = "No se puede generar el evento terminación de mandato antes de la " +
                                "fecha de generación del documento referenciado.",
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    // General por tiempo limitado_431 - limitado por tiempo limitado_433
                    else if (nitModel.CustomizationId == "431" || nitModel.CustomizationId == "433")  //que sea menor
                    {
                        DateTime endDateMandato = Convert.ToDateTime(nitModel.ValidityPeriodEndDate);
                        if (signingTime <= endDateMandato)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = errorMessageSign,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        else
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "DC24s",
                                ErrorMessage = "No se puede generar el evento terminación de mandato antes de la " +
                                "fecha de generación del documento referenciado.",
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "89",
                            ErrorMessage = "Error en el Instrumento Mandato 043",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    break;
            }

            return responses;
        }
        #endregion

        #region UpdateInTransactions
        public void UpdateInTransactions(string trackId, string eventCode)
        {
            //valida InTransaction eventos Endoso en propeidad, Garantia y procuración
            var arrayTasks = new List<Task>();
            if (Convert.ToInt32(eventCode) == (int)EventStatus.EndosoPropiedad
            || Convert.ToInt32(eventCode) == (int)EventStatus.EndosoGarantia
            || Convert.ToInt32(eventCode) == (int)EventStatus.EndosoProcuracion)
            {
                GlobalDocValidatorDocumentMeta validatorDocumentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
                if (validatorDocumentMeta != null)
                {
                    validatorDocumentMeta.InTransaction = false;
                    arrayTasks.Add(documentMetaTableManager.InsertOrUpdateAsync(validatorDocumentMeta));
                }
            }
        }
        #endregion

        #region ValidateTransactionCufe
        private List<ValidateListResponse> ValidateTransactionCufe(string trackId)
        {
            DateTime startDate = DateTime.UtcNow;
            List<Task> arrayTasks = new List<Task>();
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            bool validTransaction = false;

            GlobalDocValidatorDocumentMeta validatorDocumentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
            if (validatorDocumentMeta != null)
            {
                if (!validatorDocumentMeta.SendTestSet)
                {
                    if (!validatorDocumentMeta.InTransaction)
                    {
                        validatorDocumentMeta.InTransaction = true;
                        arrayTasks.Add(
                            documentMetaTableManager.InsertOrUpdateAsync(validatorDocumentMeta));
                    }
                    else
                    {
                        validTransaction = true;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC63",
                            ErrorMessage = $"{(string)null}La FEV referenciada se encuentra en proceso de negociación.. Inténtelo nuevamente",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        return responses;
                    }
                }                  
            }

            if (validTransaction)
                return responses;

            return null;

        }
        #endregion


        #region validation for CBC ID
        public List<ValidateListResponse> ValidateSerieAndNumber(NitModel nitModel)
        {            
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            List<ValidateListResponse> listResponses = new List<ValidateListResponse>();
            var documentReference = TableManagerGlobalDocRegisterProviderAR.FindDocumentRegisterAR<GlobalDocRegisterProviderAR>(nitModel.ProviderCode, nitModel.DocumentTypeId, nitModel.SerieAndNumber);
           
            responses.Add(new ValidateListResponse
            {
                IsValid = true,
                Mandatory = true,
                ErrorCode = "100",
                ErrorMessage = "Evento ValidateSerieAndNumber referenciado correctamente",
                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
            });

            if (documentReference.Count() > 0)
            {                        
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAD05b") + "",                    
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAD05b"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            var responseCheckDocument = CheckDocument(nitModel.SenderCode,nitModel.DocumentTypeId, nitModel.SerieAndNumber);
            if (responseCheckDocument != null)
            {              
                foreach (var item in responseCheckDocument)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = item.IsValid,
                        Mandatory = item.Mandatory,
                        ErrorCode = item.ErrorCode,
                        ErrorMessage = item.ErrorMessage,
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            listResponses.AddRange(responses);

            return listResponses;
        }
        #endregion

        #region Error Code Message
        private class ErrorCodeMessage
        {
            public string errorCode = string.Empty;
            public string errorMessage = string.Empty;
            public string errorCodeB = string.Empty;
            public string errorMessageB = string.Empty;
            public string errorCodeNoteA = string.Empty;
            public string errorMessageNoteA = string.Empty;
            public string errorCodeNote = string.Empty;
            public string errorMessageNote = string.Empty;
            public string errorCodeFETV { get; set; }
            public string errorMessageFETV { get; set; }
            public string errorCodeReceiverFETV { get; set; }
            public string errorMessageReceiverFETV { get; set; }
            public string errorCodeSigningTimeAcuse { get; set; }
            public string errorMessageigningTimeAcuse { get; set; }
            public string errorCodeSigningTimeRecibo { get; set; }
            public string errorMessageigningTimeRecibo { get; set; }           
            public string errorCodeEndoso { get; set; }
            public string errorMessageEndoso { get; set; }
            public string errorCodeMandato { get; set; }
            public string errorMessageMandato { get; set; }

        }

        private ErrorCodeMessage getErrorCodeMessage(string eventCode)
        {
            ErrorCodeMessage response = new ErrorCodeMessage()
            {
                errorCodeB = string.Empty,
                errorMessageB = string.Empty,
                errorCode = string.Empty,
                errorMessage = string.Empty,
                errorCodeNoteA = string.Empty,
                errorMessageNoteA = string.Empty,
                errorCodeNote = string.Empty,
                errorMessageNote = string.Empty,
                errorCodeFETV = string.Empty,
                errorMessageFETV = string.Empty,
                errorCodeReceiverFETV = string.Empty,
                errorMessageReceiverFETV = string.Empty,
                errorCodeSigningTimeAcuse = string.Empty,
                errorMessageigningTimeAcuse = string.Empty,
                errorCodeSigningTimeRecibo = string.Empty,
                errorMessageigningTimeRecibo = string.Empty,
                errorCodeEndoso = string.Empty,
                errorMessageEndoso = string.Empty,
                errorCodeMandato = string.Empty,
                errorMessageMandato = string.Empty
            };

            response.errorCodeNote = ConfigurationManager.GetValue("ErrorCode_AAD11");
            response.errorMessageNote = ConfigurationManager.GetValue("ErrorMessage_AAD11");
            response.errorMessageFETV = "Nombre o Razón social no esta autorizado para generar esté evento";
            response.errorMessageReceiverFETV = "El adquiriente no esta autorizado para recibir esté evento";
            response.errorMessageEndoso = ConfigurationManager.GetValue("ErrorMessage_LGC32");
            response.errorCodeMandato = "LGC36";
            response.errorMessageMandato = "El mandatario no puede enviar este evento ya que no cuenta con un mandato vigente.";

            //SenderPArty
            if (eventCode == "030") response.errorCodeFETV = "AAF01a";
            if (eventCode == "031") response.errorCodeFETV = "AAF01b";
            if (eventCode == "032") response.errorCodeFETV = "AAF01c";
            if (eventCode == "033") response.errorCodeFETV = "AAF01d";
            if (eventCode == "034") response.errorCodeFETV = "AAF01e";
            //ReceiverParty
            if (eventCode == "030") response.errorCodeReceiverFETV = "AAG01a";
            if (eventCode == "031") response.errorCodeReceiverFETV = "AAG01b";
            if (eventCode == "032") response.errorCodeReceiverFETV = "AAG01c";
            if (eventCode == "033") response.errorCodeReceiverFETV = "AAG01d";
            
            //SigningTime
            if (eventCode == "030") response.errorCodeSigningTimeAcuse = "DC24a";
            if (eventCode == "030") response.errorMessageigningTimeAcuse = "No se puede generar el evento acuse de recibo de la factura electrónica de venta " +
                    "antes de la fecha de generación del documento referenciado. ";

            if (eventCode == "032") response.errorCodeSigningTimeRecibo = "DC24b";
            if (eventCode == "032") response.errorMessageigningTimeRecibo = "No se puede generar el evento recibo de bien prestación de servicio antes de la fecha de generación " +
                    "del evento acuse de recibo de la factura electrónica de venta. ";

            //Endoso
            if (eventCode == "037") response.errorCodeEndoso = ConfigurationManager.GetValue("ErrorCode_LGC26");
            if (eventCode == "038") response.errorCodeEndoso = ConfigurationManager.GetValue("ErrorCode_LGC29");
            if (eventCode == "039") response.errorCodeEndoso = ConfigurationManager.GetValue("ErrorCode_LGC32");

            else if (eventCode == "036")
            {
                response.errorCodeB = "AAF01b";
                response.errorMessageB = "No corresponde a la información del Tenedor Legítimo";
                response.errorCode = "AAF01a";
                response.errorMessage = "No corresponde a la información del Emisor/Facturador electrónico";
            }
            else if (eventCode == "035")
            {
                response.errorCode = "AAF01";
                response.errorMessage = "No fue informado el avalista";
                response.errorCodeNoteA = "AAD11a";
                response.errorMessageNoteA = "No se informo la nota cuando el evento fue generado por un mandato.";
            }
            else if (eventCode == "037")
            {
                response.errorCode = "AAF01";
                response.errorMessage = "No corresponde a la información del Emisor/Facturador electrónico/Tenedor Legítimo";
            }
            else if (eventCode == "040" || eventCode == "039" || eventCode == "038")
            {
                response.errorCode = "AAF01";
                response.errorMessage = "No corresponde a la información del Emisor/Facturador electrónico/Tenedor Legítimo en su disponibización";
            }
            else if (eventCode == "041")
            {
                response.errorCode = "AAF01";
                response.errorMessage = "No fue referenciado la información del Juez o Juzgado";
            }
            else if (eventCode == "043")
            {
                response.errorCode = "AAF01";
                response.errorMessage = "No es informado el grupo del Mandante";
            }
            else if (eventCode == "044")
            {
                response.errorCode = "AAF01";
                response.errorMessage = "No coincide con la Mandante o Mandatario del mandato";
            }
            else if (eventCode == "045")
            {
                response.errorCode = "AAF01";
                response.errorMessage = "No fue informado el Adquirente/Deudor/Aceptante o Tenedor Legítimo";
            }
            return response;
        }
        #endregion

        #region Reemplazado Predecesor

        public List<ValidateListResponse> ValidateReplacePredecesor(string trackId)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            var item = new ValidateListResponse
            {
                IsValid = false,
                Mandatory = true,
                ErrorCode = "NIAE191a",
                ErrorMessage = "Documento a Reemplazar no se encuentra recibido en la Base de Datos."
            };

            var adjustment = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
            if(adjustment == null)
            {
                item.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
                responses.Add(item);
                return responses;
            }

            var individualPayroll = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(adjustment.DocumentReferencedKey, adjustment.DocumentReferencedKey);
            if (individualPayroll == null)
            {
                item.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
                responses.Add(item);
                return responses;
            }

            var document = documentValidatorTableManager.Find<GlobalDocValidatorDocument>(individualPayroll.Identifier, individualPayroll.Identifier);
            if (document != null)
            {
                item.IsValid = true;
                item.ErrorCode = "100";
                item.ErrorMessage = "Evento ValidateReplacePredecesor referenciado correctamente";
            }

            item.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
            responses.Add(item);
            return responses;
        }

        #endregion

        #region Individual Payroll

        private List<ValidateListResponse> CheckIndividualPayrollDuplicity(string companyId, string serieAndNumber)
        {
            DateTime startDate = DateTime.UtcNow;
            
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            responses.Add(new ValidateListResponse
            {
                IsValid = true,
                Mandatory = true,
                ErrorCode = "100",
                ErrorMessage = "Evento CheckIndividualPayrollDuplicity referenciado correctamente",
                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
            });

            // Solo se podrá transmitir una única vez el número del documento para el trabajador.
            var payroll = payrollTableManager.GlobalPayrollByRowKey_Number<GlobalDocPayroll>(companyId, serieAndNumber);
            if (payroll != null)
            {
                responses.Clear();
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "90",
                    ErrorMessage = "Documento procesado anteriormente",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            return responses;
        }

        private List<ValidateListResponse> CheckIndividualPayrollInSameMonth(string companyId, string employeeId, bool novelty, DateTime startPaymentDate)
        {
            DateTime startDate = DateTime.UtcNow;

            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            responses.Add(new ValidateListResponse
            {
                IsValid = true,
                Mandatory = true,
                ErrorCode = "100",
                ErrorMessage = "Evento CheckIndividualPayrollInSameMonth referenciado correctamente",
                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
            });

            // Solo se podrá transmitir para cada trabajador 1 documento NominaIndividual mensual durante cada mes del año. Para el mismo Empleador.
            var payrolls = payrollTableManager.GlobalPayrollByRowKey_DocumentNumber<GlobalDocPayroll>(companyId, employeeId);
            if (payrolls == null || payrolls.Count <= 0) // No exiten nóminas para el empleado...
            {
                //Novedad XML true
                if (novelty)
                {
                    responses.Clear();
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "NIE199a",
                        ErrorMessage = "Elemento Novedad con valor “true” no puede ser recibido por primera vez, " +
                        "ya que no existe una Nómina Electrónica recibida para este trabajador reportada por este Emisor durante este mes.",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                    return responses;
                }
                else
                    return responses; // no existe para el mes actual
            }

            // Se valida contra la FechaPagoInicio...
            var payrollCurrentMonth = payrolls.FirstOrDefault(x => x.FechaPagoInicio.Value.Year == startPaymentDate.Year 
                && x.FechaPagoInicio.Value.Month == startPaymentDate.Month);
            if (payrollCurrentMonth == null)
            {
                //Novedad XML true
                if (novelty)
                {
                    responses.Clear();
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "NIE199a",
                        ErrorMessage = "Elemento Novedad con valor “true” no puede ser recibido por primera vez, " +
                        "ya que no existe una Nómina Electrónica recibida para este trabajador reportada por este Emisor durante este mes.",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                    return responses;
                }
                else
                    return responses; // no existe para el mes actual
            }

            //Novedad XML False
            if (payrollCurrentMonth != null && !novelty)
            {
                responses.Clear();
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "NIE199",
                    ErrorMessage = "Únicamente pueden ser aceptados documentos “NominaIndividual” del mismo trabajador" +
                    " durante el Mes indicado en el documento que posean como 'True' este elemento.",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
                return responses;
            }

            return responses;
        }

        #endregion


        #region CheckDocument
        private List<ValidateListResponse> CheckDocument(string senderCode, string documentType, string serieAndNumber)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            var identifier = StringUtil.GenerateIdentifierSHA256($"{senderCode}{documentType}{serieAndNumber}");
            var document = documentValidatorTableManager.Find<GlobalDocValidatorDocument>(identifier, identifier);

            if (document != null)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "90, Rechazo: ",
                    ErrorMessage = "Documento procesado anteriormente",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }
            else
                return null;


            return responses;
        }
        #endregion
    }
}