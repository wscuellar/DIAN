using Gosocket.Dian.Application.Common;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Plugin.Functions.Cache;
using Gosocket.Dian.Plugin.Functions.Common.Encryption;
using Gosocket.Dian.Plugin.Functions.Cryptography.Verify;
using Gosocket.Dian.Plugin.Functions.Models;
using Org.BouncyCastle.Asn1.Cms;
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
using static Gosocket.Dian.Plugin.Functions.EventApproveCufe.EventApproveCufe;
using Gosocket.Dian.Plugin.Functions.Common;
using System.Text.RegularExpressions;


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

        #region Cufe validation
        public ValidateListResponse ValidateCufe(CufeModel cufeModel, string trackId)
        {
            DateTime startDate = DateTime.UtcNow;
            trackId = trackId.ToLower();
            var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);

            string key = string.Empty;
            var errorCode = "FAD06";
            var prop = "CUFE";
            string[] codesWithCUDE = { "03", "91", "92", "96" };
            if (codesWithCUDE.Contains(documentMeta.DocumentTypeId))
                prop = "CUDE";
            if (documentMeta.DocumentTypeId == "91")
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

            var response = new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = errorCode, ErrorMessage = $"Valor del {prop} no está calculado correctamente." };

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

                if (cufeModel.ResponseCode == "038" && cufeModel.ResponseCodeListID == "2")
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

        #region NIT validations
        public List<ValidateListResponse> ValidateNit(NitModel nitModel, string trackId)
        {
            DateTime startDate = DateTime.UtcNow;
            trackId = trackId.ToLower();
            var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);

            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            var senderCode = nitModel.SenderCode;
            var senderCodeDigit = nitModel.SenderCodeDigit;

            var senderCodeProvider = nitModel.ProviderCode;
            var senderCodeProviderDigit = nitModel.ProviderCodeDigit;

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
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = receiverDvErrorCode, ErrorMessage = "(R) DV corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = receiverDvErrorCode, ErrorMessage = "(R) DV no corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
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
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = receiver2DvErrorCode, ErrorMessage = "(R) DV corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = receiver2DvErrorCode, ErrorMessage = "(R) DV no corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                }
            }

            //IssuerParty Adquiriente/deudor de la Factura Electrónica evento Endoso Electronico
            if (nitModel.IssuerPartySchemeAgencyCode == "195")
            {
                var issuerPartyCode = nitModel.IssuerPartyCode;
                var IssuerPartyCodeDigit = nitModel.IssuerPartySchemeCode;
                if (string.IsNullOrEmpty(IssuerPartyCodeDigit) || IssuerPartyCodeDigit == "undefined") IssuerPartyCodeDigit = "11";
                if (ValidateDigitCode(issuerPartyCode, int.Parse(IssuerPartyCodeDigit)))
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "AAH63", ErrorMessage = "(R) DV corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "AAH63", ErrorMessage = "(R) El DV no está correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            }

            var softwareProviderCode = nitModel.SoftwareProviderCode;
            var softwareProviderCodeDigit = nitModel.SoftwareProviderCodeDigit;

            // Sender
            var sender = GetContributorInstanceCache(senderCode);
            string senderDvErrorCode = "FAJ24";
            if (documentMeta.DocumentTypeId == "91") senderDvErrorCode = "CAJ24";
            else if (documentMeta.DocumentTypeId == "92") senderDvErrorCode = "DAJ24";
            else if (documentMeta.DocumentTypeId == "96") senderDvErrorCode = Properties.Settings.Default.COD_VN_DocumentMeta_AAJ24;
            if (string.IsNullOrEmpty(senderCodeDigit) || senderCodeDigit == "undefined") senderCodeDigit = "11";
            if (ValidateDigitCode(senderCode, int.Parse(senderCodeDigit)))
                responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = senderDvErrorCode, ErrorMessage = "DV del NIT del emsior del documento está correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = senderDvErrorCode, ErrorMessage = "DV del NIT del emsior del documento no está correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

            // Sender2
            GlobalContributor sender2 = null;
            if (senderCode != senderCodeProvider)
            {
                string sender2DvErrorCode = "FAJ47";
                if (documentMeta.DocumentTypeId == "91") sender2DvErrorCode = "CAJ47";
                else if (documentMeta.DocumentTypeId == "92") sender2DvErrorCode = "DAJ47";
                else if (documentMeta.DocumentTypeId == "96") sender2DvErrorCode = Properties.Settings.Default.COD_VN_DocumentMeta_AAJ47;
                sender2 = GetContributorInstanceCache(senderCodeProvider);
                if (string.IsNullOrEmpty(senderCodeProviderDigit) || senderCodeProviderDigit == "undefined") senderCodeProviderDigit = "11";
                if (ValidateDigitCode(senderCodeProvider, int.Parse(senderCodeProviderDigit)))
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = sender2DvErrorCode, ErrorMessage = "DV del NIT del emsior del documento está correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = sender2DvErrorCode, ErrorMessage = "DV del NIT del emsior del documento no está correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            }

            // Software provider
            string softwareproviderDvErrorCode = "FAB22";
            if (documentMeta.DocumentTypeId == "91") softwareproviderDvErrorCode = "CAB22";
            else if (documentMeta.DocumentTypeId == "92") softwareproviderDvErrorCode = "DAB22";
            else if (documentMeta.DocumentTypeId == "96") softwareproviderDvErrorCode = Properties.Settings.Default.COD_VN_DocumentMeta_AAB22;
            var softwareProvider = GetContributorInstanceCache(softwareProviderCode);
            if (string.IsNullOrEmpty(softwareProviderCodeDigit) || softwareProviderCodeDigit == "undefined") softwareProviderCodeDigit = "11";
            if (ValidateDigitCode(softwareProviderCode, int.Parse(softwareProviderCodeDigit)))
                responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = softwareproviderDvErrorCode, ErrorMessage = "DV del NIT del Prestador de Servicios está correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = softwareproviderDvErrorCode, ErrorMessage = "DV del NIT del Prestador de Servicios no está correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

            string senderErrorCode = "FAJ21";
            if (documentMeta.DocumentTypeId == "91") senderErrorCode = "CAJ21";
            else if (documentMeta.DocumentTypeId == "92") senderErrorCode = "DAJ21";
            else if (documentMeta.DocumentTypeId == "96") senderErrorCode = Properties.Settings.Default.COD_VN_DocumentMeta_AAJ21;

            string sender2ErrorCode = "FAJ44";
            if (documentMeta.DocumentTypeId == "91") sender2ErrorCode = "CAJ44";
            else if (documentMeta.DocumentTypeId == "92") sender2ErrorCode = "DAJ44";
            else if (documentMeta.DocumentTypeId == "96") sender2ErrorCode = Properties.Settings.Default.COD_VN_DocumentMeta_AAJ44;

            string softwareProviderErrorCode = "FAB19b";
            if (documentMeta.DocumentTypeId == "91") softwareProviderErrorCode = "CAB19b";
            else if (documentMeta.DocumentTypeId == "92") softwareProviderErrorCode = "DAB19b";
            else if (documentMeta.DocumentTypeId == "96") softwareProviderErrorCode = Properties.Settings.Default.COD_VN_DocumentMeta_AAB19b;

            if (ConfigurationManager.GetValue("Environment") == "Hab" || ConfigurationManager.GetValue("Environment") == "Test")
            {
                if (sender != null)
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = senderErrorCode, ErrorMessage = $"{sender.Code} del emisor de servicios autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                else
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = senderErrorCode, ErrorMessage = $"{sender?.Code} Emisor de servicios no autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

                if (!string.IsNullOrEmpty(senderCodeProvider) && senderCode != senderCodeProvider)
                {
                    if (sender2 != null)
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = sender2ErrorCode, ErrorMessage = $"{sender2.Code} del emisor de servicios autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else
                        responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = sender2ErrorCode, ErrorMessage = $"{sender2?.Code} Emisor de servicios no autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                }

                if (softwareProvider != null)
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = softwareProviderErrorCode, ErrorMessage = $"{softwareProvider.Code} Prestrador de servicios autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                else
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = softwareProviderErrorCode, ErrorMessage = $"{softwareProvider?.Code} Prestrador de servicios no autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

            }
            else if (ConfigurationManager.GetValue("Environment") == "Prod")
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
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = softwareProviderErrorCode, ErrorMessage = $"{softwareProvider.Code} Prestrador de servicios autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                else
                    responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = softwareProviderErrorCode, ErrorMessage = $"{softwareProvider?.Code} Prestrador de servicios no autorizado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
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

            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            var senderCode = nitModel.SenderCode;
            var receiverCode = nitModel.ReceiverCode;
            string receiver2DvErrorCode = "89";
            string sender2DvErrorCode = "89";
            switch (Convert.ToInt16(party.ResponseCode))
            {
                case (int)EventStatus.Received:
                case (int)EventStatus.Rejected:
                case (int)EventStatus.Receipt:
                case (int)EventStatus.Accepted:
                    //Valida emeisor documento 
                    if (party.SenderParty != receiverCode)
                    {
                        //valida si existe los permisos del mandatario 
                        var response = ValidateFacultityAttorney(party.TrackId, party.SenderParty, senderCode,
                            party.ResponseCode, xmlParserCude.NoteMandato);
                        if (response != null)
                        {
                            responses.Add(response);
                        }
                        else
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = "Evento senderParty referenciado correctamente",
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
                            ErrorMessage = "Evento senderParty referenciado correctamente",
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
                            ErrorCode = sender2DvErrorCode,
                            ErrorMessage = "El receptor del documento transmitido no coincide con el Emisor/Facturador de la factura informada",
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
                            ErrorMessage = "Evento receiverParty referenciado correctamente",
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
                            //valida si existe los permisos del mandatario 
                            var response = ValidateFacultityAttorney(party.TrackId, party.SenderParty, senderCode,
                                party.ResponseCode, xmlParserCude.NoteMandato);
                            if (response != null)
                            {
                                responses.Add(response);
                            }
                            else
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = true,
                                    Mandatory = true,
                                    ErrorCode = "100",
                                    ErrorMessage = "Evento senderParty referenciado correctamente",
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
                                ErrorMessage = "Evento senderParty referenciado correctamente",
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
                            ErrorMessage = "Evento senderParty referenciado correctamente",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    return responses;
                case (int)EventStatus.AceptacionTacita:
                case (int)EventStatus.Mandato:
                    if (party.SenderParty != senderCode)
                    {
                        //valida si existe los permisos del mandatario 
                        var response = ValidateFacultityAttorney(party.TrackId, party.SenderParty, senderCode,
                            party.ResponseCode, xmlParserCude.NoteMandato);
                        if (response != null)
                        {
                            responses.Add(response);
                        }
                        else
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = "Evento senderParty referenciado correctamente",
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
                            ErrorMessage = "Evento senderParty referenciado correctamente",
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
                            ErrorCode = sender2DvErrorCode,
                            ErrorMessage = "El receptor del documento transmitido no coincide con el NIT DIAN",
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
                            ErrorMessage = "Evento senderParty/receiverParty referenciado correctamente",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    return responses;
                case (int)EventStatus.SolicitudDisponibilizacion:
                    if (party.SenderParty != senderCode)
                    {
                        //valida si existe los permisos del mandatario 
                        var response = ValidateFacultityAttorney(party.TrackId, party.SenderParty, senderCode,
                            party.ResponseCode, xmlParserCude.NoteMandato);
                        if (response != null)
                        {
                            responses.Add(response);
                        }
                        else
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = "Evento senderParty referenciado correctamente",
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
                            ErrorMessage = "Evento senderParty referenciado correctamente",
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
                            ErrorCode = sender2DvErrorCode,
                            ErrorMessage = "El receptor del documento transmitido no coincide con el Nit DIAN",
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
                            ErrorMessage = "Evento senderParty/receiverParty referenciado correctamente",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                    return responses;
                // Validación de la Sección SenderParty / ReceiverParty TASK 791
                case (int)EventStatus.InvoiceOfferedForNegotiation:
                    if (party.SenderParty != senderCode)
                    {
                        //valida si existe los permisos del mandatario 
                        var response = ValidateFacultityAttorney(party.TrackId, party.SenderParty, senderCode,
                            party.ResponseCode, xmlParserCude.NoteMandato);
                        if (response != null)
                        {
                            responses.Add(response);
                        }
                        else
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = "Evento senderParty referenciado correctamente",
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
                            ErrorMessage = "Evento senderParty referenciado correctamente",
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
                            ErrorCode = sender2DvErrorCode,
                            ErrorMessage = "El Destinatario no coincide con el Endosatario",
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
                            ErrorMessage = "Evento senderParty/receiverParty referenciado correctamente",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    return responses;
                // Validación de la Sección SenderParty / ReceiverParty TASK 791
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
                                ErrorCode = receiver2DvErrorCode,
                                ErrorMessage = "Emisor/Facturador Electrónico no coincide con la información de la factura referenciada",
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
                                ErrorMessage = "Evento senderParty referenciado correctamente",
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
                                ErrorCode = sender2DvErrorCode,
                                ErrorMessage = "El receptor del documento transmitido no coincide con el NIT DIAN",
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
                                ErrorMessage = "Evento receiverParty referenciado correctamente",
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    //Renuncia
                    else if (party.CustomizationID == "442")
                    {
                        //Renuncia es información del mandatario
                        if (party.SenderParty != nitModel.IssuerPartyID)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = receiver2DvErrorCode,
                                ErrorMessage = "Información del Mandatario/Representante no coincide con la información del evento referenciado",
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        else if (party.ReceiverParty != "800197268")
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = sender2DvErrorCode,
                                ErrorMessage = "El receptor del documento transmitido no coincide con el NIT DIAN",
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
                                ErrorMessage = "Evento senderParty/receiverParty referenciado correctamente",
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    return responses;
                //NotificacionPagoTotalParcial
                case (int)EventStatus.NotificacionPagoTotalParcial:
                    if (party.SenderParty != receiverCode)
                    {
                        //valida si existe los permisos del mandatario 
                        var response = ValidateFacultityAttorney(party.TrackId, party.SenderParty, receiverCode,
                            party.ResponseCode, xmlParserCude.NoteMandato);
                        if (response != null)
                        {
                            responses.Add(response);
                        }
                        else
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = "Evento senderParty referenciado correctamente",
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
                            ErrorMessage = "Evento senderParty referenciado correctamente",
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
                            ErrorCode = sender2DvErrorCode,
                            ErrorMessage = "El receptor del documento transmitido no coincide con el NIT DIAN",
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
                            ErrorMessage = "Evento receiverParty referenciado correctamente",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    return responses;
            }

            foreach (var r in responses)
                r.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
            return responses;
        }
        #endregion

        #region ValidateEndoso
        private ValidateListResponse ValidateEndoso(XmlParser xmlParserCufe, XmlParser xmlParserCude, string eventCode)
        {
            DateTime startDate = DateTime.UtcNow;
            //valor total Endoso Electronico AR
            string valueDiscountRateEndoso = xmlParserCude.DiscountRateEndoso;
            string valuePriceToPay = xmlParserCude.PriceToPay;
            string valueTotalEndoso = xmlParserCude.TotalEndoso;
            string valueTotalInvoice = xmlParserCufe.TotalInvoice;

            //Valida informacion Endoso 
            if (valueTotalEndoso == null)
            {
                return new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "Regla: AAI05a, Rechazo: ",
                    ErrorMessage = $"{(string)null} El valor no es informado .",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                };
            }

            if (!String.Equals(valueTotalEndoso, valueTotalInvoice))
            {
                return new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "Regla: AAI05b, Rechazo: ",
                    ErrorMessage = $"{(string)null} Valor Total del Endoso no es igual al Valor total FEVTV .",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                };
            }

            if (eventCode == "037")
            {
                //Valida precio a pagar endoso
                int resultValuePriceToPay = (Convert.ToInt32(valueTotalEndoso) * Convert.ToInt32(valueDiscountRateEndoso));

                if (Convert.ToInt32(valuePriceToPay) != resultValuePriceToPay)
                {
                    return new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "Regla: AAI07b, Rechazo: ",
                        ErrorMessage = $"{(string)null} El valor informado es diferente a la operación de Valor total del endoso * la tasa de descuento .",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    };
                }
            }

            return null;
        }
        #endregion

        #region ValidateFacultityAttorney
        private ValidateListResponse ValidateFacultityAttorney(string cufe, string issueAtorney, string senderCode, string eventCode, string noteMandato)
        {
            DateTime startDate = DateTime.UtcNow;
            ErrorCodeMessage errorCodeMessage = getErrorCodeMessage(eventCode);
            var sender = GetContributorInstanceCache(issueAtorney);
            //Valida exista contributor en ambiente Habilitacion y Test
            if (ConfigurationManager.GetValue("Environment") == "Hab" ||
                ConfigurationManager.GetValue("Environment") == "Test")
            {
                if (sender != null)
                {
                    if (sender.StatusId != 3 && sender.StatusId != 4)
                    {
                        return new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "89",
                            ErrorMessage = $"{(string)null} Emisor de servicios no autorizado.",
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
                        ErrorCode = "89",
                        ErrorMessage = $"{(string)null} Emisor de servicios no autorizado.",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    };
                }

            }
            //Valida exista contributor en ambiente Productivo
            else if (ConfigurationManager.GetValue("Environment") == "Prod")
            {
                if (sender != null)
                {
                    if (sender.StatusId != 4)
                    {
                        return new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "89",
                            ErrorMessage = $"{(string)null} Emisor de servicios no autorizado.",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        };
                    }
                }
            }
            //Valida exista informacion mandato - Mandatario - CUFE
            var docsReferenceAttorney = TableManagerGlobalDocReferenceAttorney.FindDocumentReferenceAttorney<GlobalDocReferenceAttorney>(cufe, senderCode);
            bool valid = false;
            if (docsReferenceAttorney == null || !docsReferenceAttorney.Any())
            {
                return new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = errorCodeMessage.errorCode,
                    ErrorMessage = errorCodeMessage.errorMessage,
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                };
            }
            bool validError = false;
            //Valida existan permisos para firmar evento mandatario
            foreach(var docReferenceAttorney in docsReferenceAttorney)
            {
                if (docReferenceAttorney.IssuerAttorney == issueAtorney)
                {
                    var filter = $"{docReferenceAttorney.FacultityCode}-{docReferenceAttorney.Actor}";
                    //Valida permisos firma para el evento emitido
                    var attorneyFacultity = TableManagerGlobalAttorneyFacultity.FindDocumentReferenceAttorneyFaculitity<GlobalAttorneyFacultity>(filter).FirstOrDefault();
                    if (attorneyFacultity != null)
                    {
                        if (attorneyFacultity.RowKey == eventCode)
                        {
                            //Valida exista note mandatario
                            if (noteMandato == null || !noteMandato.Contains("OBRANDO EN NOMBRE Y REPRESENTACION DE"))
                            {
                                return new ValidateListResponse
                                {
                                    IsValid = false,
                                    Mandatory = true,
                                    ErrorCode = eventCode == "035" ? errorCodeMessage.errorCodeNoteA : errorCodeMessage.errorCodeNote,
                                    ErrorMessage = eventCode == "035" ? errorCodeMessage.errorMessageNoteA : errorCodeMessage.errorMessageNote,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                };
                            }
                            else
                                valid = true;
                                
                        }
                    }
                    validError = false;
                    continue;
                }
                else
                {
                    validError = true;
                }

            }
            if(validError)
            {
                return new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = errorCodeMessage.errorCodeB,
                    ErrorMessage = errorCodeMessage.errorMessageB,
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                };
            }
            if (!valid)
            {
                return new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = eventCode == "035" ? errorCodeMessage.errorCodeNoteA : errorCodeMessage.errorCodeNote,
                    ErrorMessage = eventCode == "035" ? errorCodeMessage.errorMessageNoteA : errorCodeMessage.errorMessageNote,
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                };
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


        #region Validate Reference Attorney
        public List<ValidateListResponse> ValidateReferenceAttorney(XmlParser xmlParser, string trackId)
        {
            int attorneyLimit = Properties.Settings.Default.MAX_Attorney;
            bool validate = true;
            string validateCufeErrorCode = "89";
            string startDateAttorney = string.Empty;
            string endDate = string.Empty;
            DateTime startDate = DateTime.UtcNow;
            ValidateSigningTime.RequestObject data = new ValidateSigningTime.RequestObject();
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            List<AttorneyModel> attorney = new List<AttorneyModel>();
            string senderCode = xmlParser.FieldValue("SenderCode", true).ToString();
            string AttachmentBase64 = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='LineResponse']/*[local-name()='LineReference']/*[local-name()='DocumentReference']/*[local-name()='Attachment']/*[local-name()='EmbeddedDocumentBinaryObject']").Item(0)?.InnerText.ToString();
            string issuerPartyCode = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='IssuerParty']/*[local-name()='PowerOfAttorney']/*[local-name()='ID']").Item(0)?.InnerText.ToString();
            string effectiveDate = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='Response']/*[local-name()='EffectiveDate']").Item(0)?.InnerText.ToString();
            XmlNodeList cufeList = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']");
            string customizationID = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='CustomizationID']").Item(0)?.InnerText.ToString();
            //string listID = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='DocumentReference']/*[local-name()='ValidityPeriod']/*[local-name()='DescriptionCode']").Item(0)?.Attributes["listID"].Value;
            data.EventCode = "043";
            data.SigningTime = xmlParser.SigningTime;
            string factorTemp = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='IssuerParty']/*[local-name()='PowerOfAttorney']/*[local-name()='AgentParty']/*[local-name()='PartyIdentification']/*[local-name()='ID']").Item(0)?.InnerText.ToString();
            string factor = string.Empty;
            switch (factorTemp)
            {
                case "M-SN-e":
                    factor = "SNE";
                    break;
                case "M-Factor":
                    factor = "F";
                    break;
                case "M-PT":
                    factor = "PT";
                    break;
            }
            string actor = factor;
            //Valida existe Contrato del mandatos entre las partes
            if (AttachmentBase64 != null)
            {
                if (!IsBase64(AttachmentBase64))
                {
                    validate = false;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "Regla: AAH84, Rechazo:",
                        ErrorMessage = "Debe ser informado el contrato del mandato en base64",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            //Valida Mandato si es Ilimitado o Limitado
            if (customizationID == "432" || customizationID == "434")
            {
                startDateAttorney = string.Empty;
                endDate = string.Empty;
            }
            else if(customizationID == "433" || customizationID == "431")
            {
                startDateAttorney = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='DocumentReference']/*[local-name()='ValidityPeriod']/*[local-name()='StartDate']").Item(0)?.InnerText.ToString();
                endDate = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='DocumentReference']/*[local-name()='ValidityPeriod']/*[local-name()='EndDate']").Item(0)?.InnerText.ToString();
            }
            else
            {
                validate = false;
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = validateCufeErrorCode,
                    ErrorMessage = "Error en los campos de validacion de mandato ilimitado",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });

            }
            //Grupo de información alcances para el mandato sobre los CUFE.
            for (int i = 1; i < cufeList.Count && i < attorneyLimit + 1 && validate; i++)
            {
                AttorneyModel attorneyModel = new AttorneyModel();
                string code = cufeList.Item(i).SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='Response']/*[local-name()='ResponseCode']").Item(i)?.InnerText.ToString();
                string[] tempCode = code.Split(';');
                foreach(string codeAttorney in tempCode)
                {
                    string[] tempCodeAttorney = codeAttorney.Split('-');
                    if(factor == tempCodeAttorney[1])
                    {
                        if (attorneyModel.facultityCode == null)
                        {
                            attorneyModel.facultityCode += tempCodeAttorney[0];
                        }
                        else
                        {
                            attorneyModel.facultityCode += (";"+ tempCodeAttorney[0]);
                        }
                    }
                    else
                    {
                        validate = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "089",
                            ErrorMessage = "Error en el tipo de mandatario",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }

                }
                attorneyModel.cufe = cufeList.Item(i).SelectNodes("//*[local-name()='DocumentReference']/*[local-name()='UUID']").Item(i)?.InnerText.ToString();
                attorneyModel.idDocumentReference = cufeList.Item(i).SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='DocumentReference']/*[local-name()='ID']").Item(i)?.InnerText.ToString();
                //Valida CUFE referenciado existe en sistema DIAN
                var resultValidateCufe = ValidateDocumentReferencePrev(attorneyModel.cufe, attorneyModel.idDocumentReference);
                if (resultValidateCufe[0].IsValid)
                    attorney.Add(attorneyModel);
                else
                {
                    validate = false;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "Regla: AAL07, Rechazo: ",
                        ErrorMessage = "Error en la validación del CUFE referenciado, No existe en sistema DIAN",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }

                ValidatorEngine validatorEngine = new ValidatorEngine();
                var xmlBytesCufe = validatorEngine.GetXmlFromStorageAsync(data.TrackId);
                var xmlParserCufe = new XmlParser(xmlBytesCufe.Result);
                if (!xmlParserCufe.Parser())
                    throw new Exception(xmlParserCufe.ParserError);

                //Valida La fecha debe ser mayor o igual al evento de la factura referenciada
                var resultValidateSingInTime = ValidateSigningTime(data, xmlParserCufe);
                if (!resultValidateSingInTime[0].IsValid)
                {
                    validate = false;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "089",
                        ErrorMessage = "La fecha debe ser mayor o igual al evento de la factura referenciada",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }

            }
            if (validate)
            {
                foreach (var attorneyDocument in attorney)
                {
                    GlobalDocReferenceAttorney docReferenceAttorney = new GlobalDocReferenceAttorney(trackId, attorneyDocument.cufe)
                    {
                        Active = true,
                        Actor = actor,
                        EffectiveDate = effectiveDate,
                        EndDate = endDate,
                        FacultityCode = attorneyDocument.facultityCode,
                        IssuerAttorney = issuerPartyCode,
                        SenderCode = senderCode,
                        StartDate = startDateAttorney
                    };
                    TableManagerGlobalDocReferenceAttorney.InsertOrUpdateAsync(docReferenceAttorney);
                }
                responses.Add(new ValidateListResponse
                {
                    IsValid = true,
                    Mandatory = true,
                    ErrorCode = "100",
                    ErrorMessage = "Mandato referenciado correctamente",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
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
                if (new string[] { "01", "02", "04" }.Contains(documentType)) documentType = "01";
                var rk = $"{documentMeta?.Serie}|{documentType}|{documentMeta?.InvoiceAuthorization}";
                range = ranges?.FirstOrDefault(r => r.PartitionKey == documentMeta.SenderCode && r.RowKey == rk);
            }

            // If dont found range return
            if (range == null)
            {
                responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAD05e", ErrorMessage = "Número de factura no existe para el número de autorización.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                return responses;
            }

            // Check invoice authorization
            if (range.ResolutionNumber == invoiceAuthorization)
                responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAB05b", ErrorMessage = "Número de la resolución que autoriza la numeración corresponde a un número de resolución de este contribuyente emisor para este Proveedor de Autorización.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            else
                responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAB05b", ErrorMessage = "Número de la resolución que autoriza la numeración no corresponde a un número de resolución de este contribuyente emisor para este Proveedor de Autorización.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

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
                responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAB07b", ErrorMessage = "Fecha inicial del rango de numeración informado corresponde a la fecha inicial de los rangos vigente para el contribuyente.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            else
                responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAB07b", ErrorMessage = "Fecha inicial del rango de numeración informado no corresponde a la fecha inicial de los rangos vigente para el contribuyente.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

            //
            DateTime.TryParse(numberRangeModel.EndDate, out DateTime endDate);
            int.TryParse(endDate.ToString("yyyyMMdd"), out int toDateNumber);
            if (range.ValidDateNumberTo == toDateNumber)
                responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAB08b", ErrorMessage = "Fecha final del rango de numeración informado corresponde a la fecha final de los rangos vigente para el contribuyente.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            else
                responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAB08b", ErrorMessage = "Fecha final del rango de numeración informado no corresponde a la fecha final de los rangos vigente para el contribuyente.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

            if (string.IsNullOrEmpty(range.Serie)) range.Serie = "";
            if (range.Serie == numberRangeModel.Serie)
                responses.Add(new ValidateListResponse { IsValid = true, Mandatory = false, ErrorCode = "FAB10b", ErrorMessage = "El prefijo corresponde al prefijo autorizado en la resolución.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            else
                responses.Add(new ValidateListResponse { IsValid = false, Mandatory = false, ErrorCode = "FAB10b", ErrorMessage = "El prefijo debe corresponder al prefijo autorizado en la resolución.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

            long.TryParse(numberRangeModel.StartNumber, out long fromNumber);
            if (range.FromNumber == fromNumber)
                responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAB11b", ErrorMessage = "Valor inicial del rango de numeración informado corresponde a un valor inicial de los rangos vigente para el contribuyente emisor.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            else
                responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAB11b", ErrorMessage = "Valor inicial del rango de numeración informado no corresponde a un valor inicial de los rangos vigente para el contribuyente emisor.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

            long.TryParse(numberRangeModel.EndNumber, out long endNumber);
            if (range.ToNumber == endNumber)
                responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAB12b", ErrorMessage = "Valor final del rango de numeración informado corresponde a un valor final de los rangos vigentes para el contribuyente emisor.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            else
                responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAB12b", ErrorMessage = "Valor final del rango de numeración informado no corresponde a un valor final de los rangos vigentes para el contribuyente emisor.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });


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
        public ValidateListResponse ValidateSoftware(SoftwareModel softwareModel, string trackId)
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

            var number = softwareModel.SerieAndNumber;
            var softwareId = softwareModel.SoftwareId;
            var SoftwareSecurityCode = softwareModel.SoftwareSecurityCode;

            var billerSoftwareId = ConfigurationManager.GetValue("BillerSoftwareId");
            var billerSoftwarePin = ConfigurationManager.GetValue("BillerSoftwarePin");

            string hash = "";
            if (softwareId == billerSoftwareId)
                hash = $"{billerSoftwareId}{billerSoftwarePin}{number}".EncryptSHA384();
            else
            {
                var software = GetSoftwareInstanceCache(softwareId);
                if (software == null)
                {
                    response.ErrorCode = "FAB24b";
                    if (documentMeta.DocumentTypeId == "91")
                        response.ErrorCode = "CAB24b";
                    if (documentMeta.DocumentTypeId == "92")
                        response.ErrorCode = "DAB24b";
                    if (documentMeta.DocumentTypeId == "96")
                        response.ErrorCode = "AAB24b";
                    response.ErrorMessage = "El identificador del software asignado cuando el software se activa en el Sistema de Facturación Electrónica no corresponde a un software autorizado para este OFE.";
                    response.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
                    return response;
                }
                else if (software.StatusId == (int)SoftwareStatus.Inactive)
                {
                    response.ErrorCode = "FAB24c";
                    if (documentMeta.DocumentTypeId == "91")
                        response.ErrorCode = "CAB24c";
                    if (documentMeta.DocumentTypeId == "92")
                        response.ErrorCode = "DAB24c";
                    if (documentMeta.DocumentTypeId == "96")
                        response.ErrorCode = "AAB24c";
                    response.ErrorMessage = "Identificador del software informado se encuentra inactivo.";
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
        public List<ValidateListResponse> ValidateDocumentReferencePrev(string trackId, string idDocumentReference)
        {
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            DateTime startDate = DateTime.UtcNow;
            //Valida exista CUFE/CUDE en sistema DIAN
            var documentMeta = documentMetaTableManager.FindpartitionKey<GlobalDocValidatorDocumentMeta>(trackId.ToLower()).FirstOrDefault();
            if (documentMeta == null)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "Regla: AAH07, Rechazo: ",
                    ErrorMessage = "esta UUID no existe en la base de datos de la DIAN",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });

                return responses;
            }
            //Valida ID documento Invoice/AR coincida con el CUFE/CUDE referenciado
            if (documentMeta.SerieAndNumber != idDocumentReference)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "Regla: AAH06, Rechazo: ",
                    ErrorMessage = "El número de documento electrónico referenciado no coinciden con reportado.",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
                return responses;

            }

            responses.Add(new ValidateListResponse
            {
                IsValid = true,
                Mandatory = true,
                ErrorCode = "100",
                ErrorMessage = "Evento referenciado correctamente",
                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
            });

            return responses;
        }
        #endregion

        #region validation to emition to event
        public List<ValidateListResponse> ValidateEmitionEventPrev(ValidateEmitionEventPrev.RequestObject eventPrev, XmlParser xmlParserCufe, XmlParser xmlParserCude)
        {
            bool validFor = false;
            string eventCode = eventPrev.EventCode;
            DateTime startDate = DateTime.UtcNow;
            GlobalDocValidatorDocument document = null;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            
            var documentMeta = documentMetaTableManager.FindDocumentReferenced<GlobalDocValidatorDocumentMeta>(eventPrev.TrackId.ToLower(), eventPrev.DocumentTypeId);         

            foreach (var documentIdentifier in documentMeta)
            {
                document = documentValidatorTableManager.Find<GlobalDocValidatorDocument>(documentIdentifier.Identifier, documentIdentifier.Identifier);
                //Valida si el documento AR transmitido ya se encuentra aprobado
                //if (document != null)
                if(documentMeta.Count >= 2)
                {
                    if (documentMeta.Where(t => t.EventCode == eventPrev.EventCode && document != null && t.Identifier == document.PartitionKey).ToList().Count > decimal.Zero)
                    {
                        validFor = true;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "89",
                            ErrorMessage = "Solo se pueda transmitir un evento AR de cada tipo para un CUFE - ApplicationResponse (" + eventPrev.EventCode + ") ya existe",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    else
                    {
                        switch (Convert.ToInt32(eventPrev.EventCode))
                        {
                            case (int)EventStatus.Rejected:   
                                if (document != null)
                                {
                                    if (documentMeta.Where(t => t.EventCode == "033" && t.Identifier == document.PartitionKey).ToList().Count > decimal.Zero)
                                    {
                                        validFor = true;
                                        responses.Add(new ValidateListResponse
                                        {
                                            IsValid = false,
                                            Mandatory = true,
                                            ErrorCode = "202",
                                            ErrorMessage = "No se puede rechazar un documento que ha sido aceptado previamente, " +
                                            "ya existe un evento (033) Aceptación Expresaa",
                                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                        });
                                    }
                                    else if (documentMeta.Where(t => t.EventCode == "034" && t.Identifier == document.PartitionKey).ToList().Count > decimal.Zero)
                                    {
                                        validFor = true;
                                        responses.Add(new ValidateListResponse
                                        {
                                            IsValid = false,
                                            Mandatory = true,
                                            ErrorCode = "202",
                                            ErrorMessage = "No se puede rechazar un documento que ha sido aceptado previamente, " +
                                            "ya existe un evento (034) Aceptación Tacita de la factura",
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
                                            ErrorMessage = "Evento referenciado correctamente",
                                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                        });
                                    }
                                }                                   
                                break;
                            case (int)EventStatus.Receipt:
                                if (documentMeta.Where(t => t.EventCode == "030").ToList().Count == decimal.Zero)
                                {
                                    validFor = true;
                                    responses.Add(new ValidateListResponse
                                    {
                                        IsValid = false,
                                        Mandatory = true,
                                        ErrorCode = "89",
                                        ErrorMessage = "Solo se pueda transmitir el evento (032) recibo del bien o aceptacion de la prestacion del servicio," +
                                        " después de haber transmitido el evento (030) de acuse de recibo",
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
                                        ErrorMessage = "Evento referenciado correctamente",
                                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                    });
                                }
                                break;
                            case (int)EventStatus.Accepted:
                                if (document != null)
                                {
                                    //DEbe existir Recibo del bien o aceptacion de la prestacion del servicio 
                                    if (documentMeta.Where(t => t.EventCode == "032").ToList().Count > decimal.Zero)
                                    {
                                        if (documentMeta.Where(t => t.EventCode == "031" && t.Identifier == document.PartitionKey).ToList().Count > decimal.Zero)
                                        {
                                            validFor = true;
                                            responses.Add(new ValidateListResponse
                                            {
                                                IsValid = false,
                                                Mandatory = true,
                                                ErrorCode = "203",
                                                ErrorMessage = "No se puede aceptar un documento que ha sido rechazado previamente, " +
                                                "ya existe un evento (031) Rechazo de la factura de Venta",
                                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                            });
                                        }
                                        else if (documentMeta.Where(t => t.EventCode == "034" && t.Identifier == document.PartitionKey).ToList().Count > decimal.Zero)
                                        {
                                            validFor = true;
                                            responses.Add(new ValidateListResponse
                                            {
                                                IsValid = false,
                                                Mandatory = true,
                                                ErrorCode = "89",
                                                ErrorMessage = "No se pueda transmitir el evento (033) Aceptación expresa de la factura," +
                                                " Cuando ya existe un evento (034) Aceptación Tacita de la factura",
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
                                                ErrorMessage = "Evento referenciado correctamente",
                                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                            });
                                        }
                                    }                                                                    
                                    else
                                    {
                                        validFor = true;
                                        responses.Add(new ValidateListResponse
                                        {
                                            IsValid = false,
                                            Mandatory = true,
                                            ErrorCode = "89",
                                            ErrorMessage = "Solo se pueda transmitir el evento (033) Aceptacion Expresa de la factura," +
                                            " después de haber transmitido el evento (032) recibo del bien o aceptacion de la prestacion del servicio ",
                                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                        });
                                    }                                   
                                  
                                }                              
                                break;
                            case (int)EventStatus.AceptacionTacita:
                                if (document != null)
                                {
                                    if (documentMeta.Where(t => t.EventCode == "031" && t.Identifier == document.PartitionKey).ToList().Count > decimal.Zero)
                                    {
                                        validFor = true;
                                        responses.Add(new ValidateListResponse
                                        {
                                            IsValid = false,
                                            Mandatory = true,
                                            ErrorCode = "203",
                                            ErrorMessage = "No se puede aceptar un documento que ha sido rechazado previamente, " +
                                            "ya existe un evento (031) Rechazo de la factura de Venta",
                                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                        });
                                    }                                   
                                    else if (documentMeta.Where(t => t.EventCode == "033" && t.Identifier == document.PartitionKey).ToList().Count > decimal.Zero)
                                    {
                                        validFor = true;
                                        responses.Add(new ValidateListResponse
                                        {
                                            IsValid = false,
                                            Mandatory = true,
                                            ErrorCode = "89",
                                            ErrorMessage = "No se pueda transmitir el evento (034) Aceptación Tácita de la factura," +
                                            " Cuando ya existe un evento (033) Aceptación Expresa de la factura",
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
                                            ErrorMessage = "Evento referenciado correctamente",
                                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                        });
                                    }
                                }                                  
                                break;
                            case (int)EventStatus.SolicitudDisponibilizacion:
                                //Validacion de la Solicitud de Disponibilización Posterior  TAKS 723
                                if (xmlParserCude.CustomizationID == "363" || xmlParserCude.CustomizationID == "364")
                                {
                                    if (documentMeta
                                   .Where(t => t.EventCode == "038" || t.EventCode == "039" || t.EventCode == "041" && t.Identifier == document.PartitionKey).ToList()
                                   .Count > decimal.Zero)
                                    {
                                        validFor = true;
                                        responses.Add(new ValidateListResponse
                                        {
                                            IsValid = false,
                                            Mandatory = true,
                                            ErrorCode = "89",
                                            ErrorMessage = "Ya existe un tipo de instrumento de limitación registrado en el sistema",
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
                                            ErrorMessage = "Evento referenciado correctamente",
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
                                        ErrorMessage = "Evento referenciado correctamente",
                                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                    });
                                }

                                break;
                            //Validación de la existencia eventos previos Endoso
                            case (int)EventStatus.EndosoPropiedad:
                            case (int)EventStatus.EndosoGarantia:
                            case (int)EventStatus.EndosoProcuracion:
                                //Valida Valores endoso electronico versus FEVTV                               
                                if (documentMeta
                                 .Where(t => t.EventCode == "045" && t.CustomizationID == "452" && t.Identifier == document.PartitionKey).ToList()
                                 .Count > decimal.Zero)
                                {
                                    validFor = true;
                                    responses.Add(new ValidateListResponse
                                    {
                                        IsValid = false,
                                        Mandatory = true,
                                        ErrorCode = "89",
                                        ErrorMessage = "No se pueda transmitir el evento porque ya existe un pago total.",
                                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                    });
                                }
                                //Solicitud de Disponibilización
                                else if (documentMeta.Where(t => t.EventCode == "036").ToList().Count > decimal.Zero)
                                {
                                    var response = ValidateEndoso(xmlParserCufe, xmlParserCude, eventCode);
                                    if (response != null)
                                    {
                                        validFor = true;
                                        responses.Add(response);
                                    }
                                    else
                                    {
                                        //Endoso Garantia
                                        if (eventPrev.EventCode == "038")
                                        {
                                            if (documentMeta
                                               .Where(t => t.EventCode == "039" || t.EventCode == "041" && t.Identifier == document.PartitionKey).ToList()
                                               .Count > decimal.Zero)
                                            {
                                                validFor = true;
                                                responses.Add(new ValidateListResponse
                                                {
                                                    IsValid = false,
                                                    Mandatory = true,
                                                    ErrorCode = "89",
                                                    ErrorMessage = "No se pueda transmitir el evento 038-Endoso en Garantía ya existen asociados los eventos 039 Endoso en Procuración o 041 Limitación de circulación.",
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
                                                    ErrorMessage = "Evento referenciado correctamente",
                                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                                });
                                            }
                                        }
                                        //Endoso de Procuracion 
                                        else if (eventPrev.EventCode == "039")
                                        {
                                            if (documentMeta
                                                  .Where(t => t.EventCode == "038" || t.EventCode == "041" && t.Identifier == document.PartitionKey).ToList()
                                                  .Count > decimal.Zero)
                                            {
                                                validFor = true;
                                                responses.Add(new ValidateListResponse
                                                {
                                                    IsValid = false,
                                                    Mandatory = true,
                                                    ErrorCode = "89",
                                                    ErrorMessage = "No se pueda transmitir el evento 039-Endoso en Procuración ya existen asociados los eventos 038 Endoso en Garantía o 041 Limitación de circulación.",
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
                                                    ErrorMessage = "Evento referenciado correctamente",
                                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                                });
                                            }
                                        }
                                        //Endoso propiedad
                                        else if (eventPrev.EventCode == "037")
                                        {
                                            if (documentMeta
                                                 .Where(t => t.EventCode == "038" || t.EventCode == "039" || t.EventCode == "041" && t.Identifier == document.PartitionKey).ToList()
                                                 .Count > decimal.Zero)
                                            {
                                                validFor = true;
                                                responses.Add(new ValidateListResponse
                                                {
                                                    IsValid = false,
                                                    Mandatory = true,
                                                    ErrorCode = "LGC25",
                                                    ErrorMessage = "No se puede registrar este evento si previamente se ha registrado alguno de los siguientes eventos: " +
                                                    "Endoso en garantía, Endoso en procuración o Limitación de circulación",
                                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                                });
                                            }
                                            else if (documentMeta
                                                 .Where(t => t.EventCode == "045" && t.CustomizationID == "452" && t.Identifier == document.PartitionKey).ToList()
                                                 .Count > decimal.Zero)
                                            {
                                                validFor = true;
                                                responses.Add(new ValidateListResponse
                                                {
                                                    IsValid = false,
                                                    Mandatory = true,
                                                    ErrorCode = "LGC26",
                                                    ErrorMessage = "No se puede registrar este evento si previamente se ha registrado el evento Notificación de Pago total",
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
                                                    ErrorMessage = "Evento referenciado correctamente",
                                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                                });
                                            }
                                        }                                      
                                    }
                                }
                                else
                                {
                                    validFor = true;
                                    responses.Add(new ValidateListResponse
                                    {
                                        IsValid = false,
                                        Mandatory = true,
                                        ErrorCode = "LGC24",
                                        ErrorMessage = "No se puede registrar este evento si previamente no se ha registrado el evento Solicitud de disponibilización",
                                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                    });
                                }
                                break;
                            //Validación de la existencia de Endosos y Limitaciones TASK  730
                            case (int)EventStatus.InvoiceOfferedForNegotiation:
                                if (documentMeta
                                    .Where(t => t.EventCode == "037" || t.EventCode == "041" && t.Identifier == document.PartitionKey).ToList()
                                    .Count > decimal.Zero)
                                {
                                    validFor = true;
                                    responses.Add(new ValidateListResponse
                                    {
                                        IsValid = false,
                                        Mandatory = true,
                                        ErrorCode = "89",
                                        ErrorMessage = "No es posible realizar la Anulación de Endoso, ya existe un evento 037 Endoso en Propiedad" +
                                        "y/o un evento 041 Limitación de circulación",
                                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                    });
                                }
                                //Anulacion de Endoso solo aplica para Endoso en Garantia o Endoso en Procuracion
                                else if (documentMeta
                                .Where(t => t.EventCode == "038" || t.EventCode == "039 " && t.Identifier == document.PartitionKey).ToList()
                                .Count > decimal.Zero)
                                {
                                    responses.Add(new ValidateListResponse
                                    {
                                        IsValid = true,
                                        Mandatory = true,
                                        ErrorCode = "100",
                                        ErrorMessage = "Evento referenciado correctamente",
                                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                    });
                                }
                                else
                                {
                                    validFor = true;
                                    responses.Add(new ValidateListResponse
                                    {
                                        IsValid = false,
                                        Mandatory = true,
                                        ErrorCode = "89",
                                        ErrorMessage = "No es posible realizar la Anulación de Endoso, no existe un evento 038 Endoso en Garantía y/o 039 Endoso en Procuración",
                                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                    });
                                }
                                break;
                            case (int)EventStatus.Mandato:
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = true,
                                    Mandatory = true,
                                    ErrorCode = "100",
                                    ErrorMessage = "Evento referenciado correctamente",
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                                break;
                            case (int)EventStatus.NotificacionPagoTotalParcial:
                                //Si el titulo valor tiene una limitación previa (041)
                                if (documentMeta
                                    .Where(t => t.EventCode == "041" && t.Identifier == document.PartitionKey).ToList()
                                    .Count > decimal.Zero)
                                {
                                    if (eventPrev.ListId == "2")
                                    {
                                        responses.Add(new ValidateListResponse
                                        {
                                            IsValid = true,
                                            Mandatory = true,
                                            ErrorCode = "100",
                                            ErrorMessage = "Evento referenciado correctamente",
                                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                        });
                                    }
                                    else
                                    {
                                        validFor = true;
                                        responses.Add(new ValidateListResponse
                                        {
                                            IsValid = false,
                                            Mandatory = true,
                                            ErrorCode = "89",
                                            ErrorMessage = "El evento 045- Notificación de pago parcial o total tiene una limitación previa (041), no es posible realziar el pago",
                                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                        });
                                    }
                                }
                                //Valdia si existe un pago total
                                else if (documentMeta
                                  .Where(t => t.CustomizationID == "452" && t.Identifier == document.PartitionKey).ToList()
                                  .Count > decimal.Zero)
                                {
                                    validFor = true;
                                    responses.Add(new ValidateListResponse
                                    {
                                        IsValid = false,
                                        Mandatory = true,
                                        ErrorCode = "89",
                                        ErrorMessage = "Ya existe un Pago Total de la Factura",
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
                                        ErrorMessage = "Evento referenciado correctamente",
                                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                    });
                                }
                                break;
                        }
                    }
                    if (validFor)
                    {
                        return responses;
                    }
                }
                // Valida que el primer evento transmitido sea un acuse
                else if (eventPrev.EventCode != "030")
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "89",
                        ErrorMessage = "Solo se pueda transmitir el evento (" + eventPrev.EventCode + ")," +
                                        " después de haber transmitido el evento (030) de acuse de recibo.",
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
                        ErrorMessage = "Evento referenciado correctamente",
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }

            }          
            return responses;
        }
        #endregion

        # region Validación de la Sección prerrequisitos Solicitud Disponibilizacion
        public List<ValidateListResponse> EventApproveCufe(NitModel dataModel, EventApproveCufeObjectParty data)
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
            if (count == 3)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = true,
                    Mandatory = true,
                    ErrorCode = "100",
                    ErrorMessage = "Evento referenciado correctamente",
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
                    ErrorMessage = "Factura no cuenta con características para considerarse título valor",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }
            return responses;
        }
        #endregion

        #region ValidateSigningTime
        public List<ValidateListResponse> ValidateSigningTime(ValidateSigningTime.RequestObject data, XmlParser dataModel)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            var businessDays = BusinessDaysHolidays.BusinessDaysUntil(Convert.ToDateTime(dataModel.SigningTime), Convert.ToDateTime(data.SigningTime));

            switch (int.Parse(data.EventCode))
            {
                case (int)EventStatus.Received:
                case (int)EventStatus.Receipt:
                case (int)EventStatus.InvoiceOfferedForNegotiation:
                case (int)EventStatus.Mandato:
                case (int)EventStatus.EndosoProcuracion:
                case (int)EventStatus.EndosoPropiedad:
                    responses.Add(Convert.ToDateTime(data.SigningTime) >= Convert.ToDateTime(dataModel.SigningTime)
                        ? new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = "Ok",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        }
                        : new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "89",
                            ErrorMessage =
                                "la fecha debe ser mayor o igual al evento referenciado con el CUFE/CUDE",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    break;
                case (int)EventStatus.Rejected:
                    responses.Add(businessDays > 3
                         ? new ValidateListResponse
                         {
                             IsValid = false,
                             Mandatory = true,
                             ErrorCode = "89",
                             ErrorMessage =
                                "Se ha superado los 3 días hábiles siguientes a la fecha de firma del evento, se rechaza la transmisión de este evento 31",
                             ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                         }
                        : new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = "Ok",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    break;
                case (int)EventStatus.Accepted:
                    responses.Add(businessDays >= 3
                        ? new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "89",
                            ErrorMessage = "Se ha superado los 3 días hábiles siguientes a la fecha de firma del evento, se rechaza la transmisión de este evento 33",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        }
                        : new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = "Ok",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });

                    break;
                case (int)EventStatus.AceptacionTacita:
                    responses.Add(businessDays > 3
                        ? new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = "Ok",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        }
                        : new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "89",
                            ErrorMessage = "La fecha de firma debe ser mayor a los tres días hábiles contados a partir de la fecha de la firma del evento transmitido 032",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    break;
                case (int)EventStatus.SolicitudDisponibilizacion:
                case (int)EventStatus.NegotiatedInvoice:
                case (int)EventStatus.AnulacionLimitacionCirculacion:
                    if (dataModel.CustomizationID == "361" || dataModel.CustomizationID == "362" ||
                       dataModel.CustomizationID == "363" || dataModel.CustomizationID == "364")
                    {
                        responses.Add(Convert.ToDateTime(data.SigningTime) > Convert.ToDateTime(dataModel.SigningTime)
                        ? new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = "Ok",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        }
                        : new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "89",
                            ErrorMessage =
                                "la fecha debe ser mayor o igual al evento de la factura electrónica referenciada con el CUFE/CUDE",
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
                            ErrorMessage = "Ok",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        }
                        : new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "89",
                            ErrorMessage =
                                "la fecha debe ser mayor o igual al evento de la factura electrónica referenciada con el CUFE/CUDE",
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
                           ErrorMessage = "Ok",
                           ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                       }
                       : new ValidateListResponse
                       {
                           IsValid = false,
                           Mandatory = true,
                           ErrorCode = "89",
                           ErrorMessage =
                               "la fecha debe ser mayor o igual al evento referenciado con el CUFE/CUDE",
                           ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                       });                
                    break;
                //Validación fecha emite evento AR menor a fecha de vencimiento factura
                case (int)EventStatus.EndosoGarantia:
                    responses.Add(Convert.ToDateTime(data.SigningTime) < Convert.ToDateTime(dataModel.PaymentDueDate)
                        ? new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = "Ok",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        }
                        : new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "89",
                            ErrorMessage = "Fecha de endoso es superior a la fecha de vencimiento de la factura electrónica",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    break;
                case (int)EventStatus.TerminacionMandato:
                    DateTime signingTime = Convert.ToDateTime(data.SigningTime);
                    //General por tiempo ilimitado_432 - limitado por tiempo ilimitado_434
                    if (dataModel.CustomizationID == "432" || dataModel.CustomizationID == "434") //que se mayor
                    {
                        DateTime dateMandato = Convert.ToDateTime(dataModel.SigningTime);
                        if (signingTime >= dateMandato)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = "Ok",
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        else
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "89",
                                ErrorMessage = "La fecha Terminación del mandato es menor a la fecha de registro del Instrumento Mandato 043",
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    // General por tiempo limitado_431 - limitado por tiempo limitado_433
                    else if (dataModel.CustomizationID == "431" || dataModel.CustomizationID == "433")  //que sea menor
                    {
                        DateTime endDateMandato = Convert.ToDateTime(dataModel.FieldValue("EndDate"));
                        if (signingTime <= endDateMandato)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = "Ok",
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                        else
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "89",
                                ErrorMessage = "La fecha Terminación del mandato es mayor a la fecha de registro del Instrumento Mandato 043",
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

        #region validation for CBC ID
        public List<ValidateListResponse> ValidateSerieAndNumber(string trackId, string number, string documentTypeId)
        {
            bool validFor = false;
            DateTime startDate = DateTime.UtcNow;
            GlobalDocValidatorDocument document = null;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            trackId = trackId.ToLower();
            var documentMeta = documentMetaTableManager.FindDocumentReferenced<GlobalDocValidatorDocumentMeta>(trackId, documentTypeId);

            if (documentMeta.Count > 0)
            {
                foreach (var documentIdentifier in documentMeta)
                {
                    document = documentValidatorTableManager.Find<GlobalDocValidatorDocument>(documentIdentifier?.Identifier, documentIdentifier?.Identifier);
                    if (document != null)
                    {
                        if (documentMeta.Where(t => t.Number == number
                        && t.Identifier == document.PartitionKey
                        ).ToList().Count > decimal.Zero)
                        {
                            validFor = true;
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "AAD05b",
                                ErrorMessage = "No se puede repetir el numero para el tipo de evento.",
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
                                ErrorMessage = " El Identificador (" + number + ") ApplicationResponse no existe para este CUFE",
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
                            ErrorMessage = " El Identificador (" + number + ") ApplicationResponse no existe para este CUFE",
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                    if (validFor)
                    {
                        return responses;
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
                    ErrorMessage = " El Identificador (" + number + ") ApplicationResponse no existe para este CUFE",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            return responses;
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
                errorMessageNote = string.Empty
            };

            response.errorCodeNote = "AAD11";
            response.errorMessageNote = "No fue informada la nota cuando el evento fue generado por un mandato. ";

            if (eventCode == "030" || eventCode == "031" || eventCode == "032" || eventCode == "033" || eventCode == "034")
            {
                response.errorCodeB = "AAF01b";
                response.errorMessageB = "No corresponde a la información del Tenedor Legítimo";
                response.errorCode = "AAF01a";
                response.errorMessage = "No corresponde a la información del Emisor/Facturador electrónico";
            }
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
                response.errorMessageNoteA = "No fue informada la nota cuando el evento fue generado por un mandato. ";
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

    }
}