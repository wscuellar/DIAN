using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Plugin.Functions.Cache;
using Gosocket.Dian.Plugin.Functions.Common.Encryption;
using Gosocket.Dian.Plugin.Functions.Cryptography.Verify;
using Gosocket.Dian.Plugin.Functions.Models;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using ContributorType = Gosocket.Dian.Domain.Common.ContributorType;
using OperationMode = Gosocket.Dian.Domain.Common.OperationMode;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace Gosocket.Dian.Plugin.Functions.Common
{
    public class Validator
    {
        #region Global properties
        private const string CertificatesCollection = "Certificate/Collection";
        private readonly HashSet _trustedRoots = new HashSet();
        private readonly List<X509Certificate> _intermediates = new List<X509Certificate>();
        static readonly TableManager contributorTableManager = new TableManager("GlobalContributor");
        static readonly TableManager contingencyTableManager = new TableManager("GlobalContingency");
        static readonly TableManager documentMetaTableManager = new TableManager("GlobalDocValidatorDocumentMeta");
        static readonly TableManager documentValidatorTableManager = new TableManager("GlobalDocValidatorDocument");
        static readonly TableManager numberRangeTableManager = new TableManager("GlobalNumberRange");
        static readonly TableManager tableManagerTestSetResult = new TableManager("GlobalTestSetResult");
        static readonly TableManager softwareTableManager = new TableManager("GlobalSoftware");
        static readonly TableManager typeListTableManager = new TableManager("GlobalTypeList");

        readonly XmlDocument _xmlDocument;
        readonly XPathDocument _document;
        readonly XPathNavigator _navigator;
        readonly XPathNavigator _navNs;
        readonly XmlNamespaceManager _ns;
        readonly byte[] _xmlBytes;
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
        public ValidateListResponse ValidateCufe(ResponseXpathDataValue responseXpathDataValue, string trackId)
        {
            DateTime startDate = DateTime.UtcNow;
            trackId = trackId.ToLower();
            var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);

            string key = string.Empty;
            var errorCode = "FAD06";
            var prop = "CUFE";
            string[] codesWithCUDE = { "03", "91", "92" };
            if (codesWithCUDE.Contains(documentMeta.DocumentTypeId))
                prop = "CUDE";
            if (documentMeta.DocumentTypeId == "91")
                errorCode = "CAD06";
            else if (documentMeta.DocumentTypeId == "92")
                errorCode = "DAD06";

            var billerSoftwareId = ConfigurationManager.GetValue("BillerSoftwareId");
            var billerSoftwarePin = ConfigurationManager.GetValue("BillerSoftwarePin");

            if (!codesWithCUDE.Contains(documentMeta.DocumentTypeId))
                key = ConfigurationManager.GetValue("TestHabTechnicalKey");
            else
            {
                var softwareId = responseXpathDataValue.XpathsValues["SoftwareIdXpath"];
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

            var number = responseXpathDataValue.XpathsValues["NumberXpath"];
            var emissionDate = responseXpathDataValue.XpathsValues["EmissionDateXpath"];
            var emissionHour = responseXpathDataValue.XpathsValues["HourEmissionXpath"];
            var amount = responseXpathDataValue.XpathsValues["AmountXpath"];
            if (string.IsNullOrEmpty(amount)) amount = "0.00"; else amount = TruncateDecimal(decimal.Parse(amount), 2).ToString("F2");
            var taxCode1 = "01";
            var taxCode2 = "04";
            var taxCode3 = "03";
            var taxAmount1 = responseXpathDataValue.XpathsValues["TaxAmount1Xpath"];
            taxAmount1 = taxAmount1.Split('|')[0];
            var taxAmount2 = responseXpathDataValue.XpathsValues["TaxAmount2Xpath"];
            var taxAmount3 = responseXpathDataValue.XpathsValues["TaxAmount3Xpath"];

            if (string.IsNullOrEmpty(taxAmount1)) taxAmount1 = "0.00"; else taxAmount1 = TruncateDecimal(decimal.Parse(taxAmount1), 2).ToString("F2");
            if (string.IsNullOrEmpty(taxAmount2)) taxAmount2 = "0.00"; else taxAmount2 = TruncateDecimal(decimal.Parse(taxAmount2), 2).ToString("F2");
            if (string.IsNullOrEmpty(taxAmount3)) taxAmount3 = "0.00"; else taxAmount3 = TruncateDecimal(decimal.Parse(taxAmount3), 2).ToString("F2");
            var amountToPay = responseXpathDataValue.XpathsValues["AmountToPayXpath"];
            if (string.IsNullOrEmpty(amountToPay)) amountToPay = "0.00"; else amountToPay = TruncateDecimal(decimal.Parse(amountToPay), 2).ToString("F2");
            var senderCode = responseXpathDataValue.XpathsValues["SenderCodeXpath"];
            var receiverCode = responseXpathDataValue.XpathsValues["ReceiverCodeXpath"];
            var environmentType = responseXpathDataValue.XpathsValues["EnvironmentTypeXpath"];

            var data = $"{number}{emissionDate}{emissionHour}{amount}{taxCode1}{taxAmount1}{taxCode2}{taxAmount2}{taxCode3}{taxAmount3}{amountToPay}{senderCode}{receiverCode}{key}{environmentType}";
            var documentKey = responseXpathDataValue.XpathsValues["DocumentKeyXpath"];
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
        public List<ValidateListResponse> ValidateNit(ResponseXpathDataValue responseXpathDataValue, string trackId)
        {
            DateTime startDate = DateTime.UtcNow;
            trackId = trackId.ToLower();
            var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);

            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            var senderCode = responseXpathDataValue.XpathsValues["SenderCodeXpath"];
            var senderCodeDigit = responseXpathDataValue.XpathsValues["SenderCodeDigitXpath"];

            var senderCodeProvider = responseXpathDataValue.XpathsValues["SenderCodeProviderXpath"];
            var senderCodeProviderDigit = responseXpathDataValue.XpathsValues["SenderCodeProviderDigitXpath"];

            var receiverCode = responseXpathDataValue.XpathsValues["ReceiverCodeXpath"];
            var receiverCodeSchemeNameValue = responseXpathDataValue.XpathsValues["ReceiverCodeSchemeNameValueXpath"];
            if (receiverCodeSchemeNameValue == "31")
            {
                string receiverDvErrorCode = "FAK24";
                if (documentMeta.DocumentTypeId == "91") receiverDvErrorCode = "CAK24";
                else if (documentMeta.DocumentTypeId == "92") receiverDvErrorCode = "DAK24";

                var receiverCodeDigit = responseXpathDataValue.XpathsValues["ReceiverCodeDigitXpath"];
                if (string.IsNullOrEmpty(receiverCodeDigit) || receiverCodeDigit == "undefined") receiverCodeDigit = "11";
                if (ValidateDigitCode(receiverCode, int.Parse(receiverCodeDigit)))
                    responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = receiverDvErrorCode, ErrorMessage = "(R) DV no corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = receiverDvErrorCode, ErrorMessage = "(R) DV no corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            }

            var receiver2Code = responseXpathDataValue.XpathsValues["ReceiverCode2Xpath"];
            if (receiverCode != receiver2Code)
            {
                var receiver2CodeSchemeNameValue = responseXpathDataValue.XpathsValues["ReceiverCode2SchemeNameValueXpath"];
                if (receiver2CodeSchemeNameValue == "31")
                {
                    string receiver2DvErrorCode = "FAK47";
                    if (documentMeta.DocumentTypeId == "91") receiver2DvErrorCode = "CAK47";
                    else if (documentMeta.DocumentTypeId == "92") receiver2DvErrorCode = "DAK47";

                    var receiver2CodeDigit = responseXpathDataValue.XpathsValues["ReceiverCode2DigitXpath"];
                    if (string.IsNullOrEmpty(receiver2CodeDigit) || receiver2CodeDigit == "undefined") receiver2CodeDigit = "11";
                    if (ValidateDigitCode(receiver2Code, int.Parse(receiver2CodeDigit)))
                        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = receiver2DvErrorCode, ErrorMessage = "(R) DV no corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = receiver2DvErrorCode, ErrorMessage = "(R) DV no corresponde al NIT informado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                }
            }

            var softwareProviderCode = responseXpathDataValue.XpathsValues["SoftwareProviderCodeXpath"];
            var softwareProviderCodeDigit = responseXpathDataValue.XpathsValues["SoftwareProviderCodeDigitXpath"];

            // Participante (consorcio)
            var sheldHolderCode = responseXpathDataValue.XpathsValues["SheldHolderCodeXpath"].Split('|');
            var sheldHolderSchemaNameValues = responseXpathDataValue.XpathsValues["SheldHolderSchemaNameValueXpath"].Split('|');
            var sheldHolderCodeDigit = responseXpathDataValue.XpathsValues["SheldHolderCodeDigitXpath"].Split('|');

            // Principal (mandante)
            var agentPartyCode = responseXpathDataValue.XpathsValues["AgentPartyCodeXpath"].Split('|');
            var agentPartySchemaNameValues = responseXpathDataValue.XpathsValues["AgentPartySchemaNameValueXpath"].Split('|');
            var agentPartyCodeDigit = responseXpathDataValue.XpathsValues["AgentPartyCodeDigitXpath"].Split('|');

            // Sender
            var sender = GetContributorInstanceCache(senderCode);
            string senderDvErrorCode = "FAJ24";
            if (documentMeta.DocumentTypeId == "91") senderDvErrorCode = "CAJ24";
            else if (documentMeta.DocumentTypeId == "92") senderDvErrorCode = "DAJ24";
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
            var softwareProvider = GetContributorInstanceCache(softwareProviderCode);
            if (string.IsNullOrEmpty(softwareProviderCodeDigit) || softwareProviderCodeDigit == "undefined") softwareProviderCodeDigit = "11";
            if (ValidateDigitCode(softwareProviderCode, int.Parse(softwareProviderCodeDigit)))
                responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = softwareproviderDvErrorCode, ErrorMessage = "DV del NIT del Prestador de Servicios está correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            else responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = softwareproviderDvErrorCode, ErrorMessage = "DV del NIT del Prestador de Servicios no está correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

            string senderErrorCode = "FAJ21";
            if (documentMeta.DocumentTypeId == "91") senderErrorCode = "CAJ21";
            else if (documentMeta.DocumentTypeId == "92") senderErrorCode = "DAJ21";

            string sender2ErrorCode = "FAJ44";
            if (documentMeta.DocumentTypeId == "91") sender2ErrorCode = "CAJ44";
            else if (documentMeta.DocumentTypeId == "92") sender2ErrorCode = "DAJ44";

            string softwareProviderErrorCode = "FAB19b";
            if (documentMeta.DocumentTypeId == "91") softwareProviderErrorCode = "CAB19b";
            else if (documentMeta.DocumentTypeId == "92") softwareProviderErrorCode = "DAB19b";

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

            var position = 0;
            var validDv = false;
            string sheldHolderDvErrorCode = "FAJ60";
            if (documentMeta.DocumentTypeId == "91") sheldHolderDvErrorCode = "CAJ60";
            else if (documentMeta.DocumentTypeId == "92") sheldHolderDvErrorCode = "DAJ60";

            //string sheldHolderErrorCode = "FAJ57";
            //if (documentMeta.DocumentTypeId == "91") sheldHolderErrorCode = "CAJ57";
            //else if (documentMeta.DocumentTypeId == "92") sheldHolderErrorCode = "DAJ57";
            foreach (var shell in sheldHolderCode)
            {
                if (!string.IsNullOrEmpty(shell))
                {
                    var inBound = InBounds(position, sheldHolderSchemaNameValues);
                    if (!inBound) continue;
                    var schemaNameValue = sheldHolderSchemaNameValues[position];

                    if (schemaNameValue == "31")
                    {
                        inBound = InBounds(position, sheldHolderCodeDigit);
                        if (!inBound) continue;
                        var dv = sheldHolderCodeDigit[position];
                        var shellHolder = GetContributorInstanceCache(shell);
                        if (string.IsNullOrEmpty(dv) || dv == "undefined")
                        {
                            responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = sheldHolderDvErrorCode, ErrorMessage = "(N) DV del NIT del participante de consorcio no está correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                            validDv = false;
                            break;
                        }
                        if (ValidateDigitCode(shell, int.Parse(dv)))
                            validDv = true;
                        else
                        {
                            responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = sheldHolderDvErrorCode, ErrorMessage = "(N) DV del NIT del participante de consorcio no está correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                            validDv = false;
                            break;
                        }
                    }
                    position += 1;

                    //if (ConfigurationManager.GetValue("Environment") == "Hab" || ConfigurationManager.GetValue("Environment") == "Test")
                    //{
                    //    if (shellHolder != null)
                    //        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = sheldHolderErrorCode, ErrorMessage = $"{shell} del participante de consorcio válido.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    //    else
                    //    {
                    //        responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = sheldHolderErrorCode, ErrorMessage = $"{shell} del participante de consorcio no válido.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    //        break;
                    //    }
                    //}
                    //else if (ConfigurationManager.GetValue("Environment") == "Prod")
                    //{
                    //    if (shellHolder?.StatusId == (int)ContributorStatus.Enabled)
                    //        responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = sheldHolderErrorCode, ErrorMessage = $"{shell} del participante de consorcio válido.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    //    else
                    //    {
                    //        responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = sheldHolderErrorCode, ErrorMessage = $"{shell} del participante de consorcio no válido.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    //        break;
                    //    }
                    //}
                }
            }
            if (validDv)
                responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = sheldHolderDvErrorCode, ErrorMessage = "(N) DV del NIT del participante de consorcio está correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

            position = 0;
            validDv = false;
            string agentPartyDvErrorCode = "FBA07";
            if (documentMeta.DocumentTypeId == "91") agentPartyDvErrorCode = "CBA07";
            else if (documentMeta.DocumentTypeId == "92") agentPartyDvErrorCode = "DBA07";

            //string agentPartyErrorCode = "FBA07";
            //if (documentMeta.DocumentTypeId == "91") agentPartyErrorCode = "CBA07";
            //else if (documentMeta.DocumentTypeId == "92") agentPartyErrorCode = "DBA07";
            foreach (var agent in agentPartyCode)
            {
                if (!string.IsNullOrEmpty(agent))
                {
                    var inBound = InBounds(position, agentPartySchemaNameValues);
                    if (!inBound) continue;
                    var schemaNameValue = agentPartySchemaNameValues[position];

                    if (schemaNameValue == "31")
                    {
                        inBound = InBounds(position, agentPartyCodeDigit);
                        if (!inBound) continue;
                        var dv = agentPartyCodeDigit[position];

                        var agentParty = GetContributorInstanceCache(agent);

                        if (string.IsNullOrEmpty(dv) || dv == "undefined")
                        {
                            responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = agentPartyDvErrorCode, ErrorMessage = "(R) DV del NIT del mandante mal calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                            validDv = false;
                            break;
                        }
                        if (ValidateDigitCode(agent, int.Parse(dv)))
                            validDv = true;
                        else
                        {
                            responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = agentPartyDvErrorCode, ErrorMessage = "(R) DV del NIT del mandante mal calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                            validDv = false;
                            break;
                        }
                    }
                    position += 1;

                    //if (schemaNameValue == "31")
                    //{
                    //    if (ConfigurationManager.GetValue("Environment") == "Hab" || ConfigurationManager.GetValue("Environment") == "Test")
                    //    {
                    //        if (agentParty != null)
                    //            responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = agentPartyErrorCode, ErrorMessage = $"{agent} NIT del mandante registrado en la DIAN.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    //        else
                    //        {
                    //            responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = agentPartyErrorCode, ErrorMessage = $"{agent} NIT del mandante no esta registrado en la DIAN.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    //            break;
                    //        }
                    //    }
                    //    else if (ConfigurationManager.GetValue("Environment") == "Prod")
                    //    {
                    //        if (agentParty?.StatusId == (int)ContributorStatus.Enabled)
                    //            responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = agentPartyErrorCode, ErrorMessage = $"{agent} NIT del mandante registrado en la DIAN.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    //        else
                    //        {
                    //            responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = agentPartyErrorCode, ErrorMessage = $"{agent} NIT del mandante no esta registrado en la DIAN.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                    //            break;
                    //        }
                    //    }
                    //}
                }
            }
            if (validDv)
                responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = agentPartyDvErrorCode, ErrorMessage = "(R) DV del NIT del mandante correctamente calculado", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

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

        #region Number range validation
        public List<ValidateListResponse> ValidateNumberingRange(ResponseXpathDataValue responseXpathDataValue, string trackId)
        {
            DateTime startDate = DateTime.UtcNow;
            trackId = trackId.ToLower();
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);

            var invoiceAuthorization = responseXpathDataValue.XpathsValues["InvoiceAuthorizationXpath"];
            var softwareId = responseXpathDataValue.XpathsValues["SoftwareIdXpath"];
            var senderCode = responseXpathDataValue.XpathsValues["SenderCodeXpath"];

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
            var startDateXpathValue = responseXpathDataValue.XpathsValues["StartDateXpath"];
            int.TryParse(DateTime.Parse(startDateXpathValue).ToString("yyyyMMdd"), out int fromDateNumber);
            if (range.ValidDateNumberFrom == fromDateNumber)
                responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAB07b", ErrorMessage = "Fecha inicial del rango de numeración informado corresponde a la fecha inicial de los rangos vigente para el contribuyente.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            else
                responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAB07b", ErrorMessage = "Fecha inicial del rango de numeración informado no corresponde a la fecha inicial de los rangos vigente para el contribuyente.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

            //
            var endDateXpathValue = responseXpathDataValue.XpathsValues["EndDateXpath"];
            int.TryParse(DateTime.Parse(endDateXpathValue).ToString("yyyyMMdd"), out int toDateNumber);
            if (range.ValidDateNumberTo == toDateNumber)
                responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAB08b", ErrorMessage = "Fecha final del rango de numeración informado corresponde a la fecha final de los rangos vigente para el contribuyente.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            else
                responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAB08b", ErrorMessage = "Fecha final del rango de numeración informado no corresponde a la fecha final de los rangos vigente para el contribuyente.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

            //
            //if (!string.IsNullOrEmpty(range.Serie))
            //{
            if (range.Serie == responseXpathDataValue.XpathsValues["PrefixXpath"])
                responses.Add(new ValidateListResponse { IsValid = true, Mandatory = false, ErrorCode = "FAB10b", ErrorMessage = "El prefijo corresponde al prefijo autorizado en la resolución.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            else
                responses.Add(new ValidateListResponse { IsValid = false, Mandatory = false, ErrorCode = "FAB10b", ErrorMessage = "El prefijo debe corresponder al prefijo autorizado en la resolución.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            //}

            //
            long.TryParse(responseXpathDataValue.XpathsValues["StartNumberXpath"], out long fromNumber);
            if (range.FromNumber == fromNumber)
                responses.Add(new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "FAB11b", ErrorMessage = "Valor inicial del rango de numeración informado corresponde a un valor inicial de los rangos vigente para el contribuyente emisor.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
            else
                responses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "FAB11b", ErrorMessage = "Valor inicial del rango de numeración informado no corresponde a un valor inicial de los rangos vigente para el contribuyente emisor.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });

            //
            long.TryParse(responseXpathDataValue.XpathsValues["EndNumberXpath"], out long endNumber);
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
        public IEnumerable<X509Crl> Crls { get; private set; }
        private ValidateListResponse ValidateCrl(X509Certificate certificate)
        {
            DateTime startDate = DateTime.UtcNow;
            if (Crls.Any(c => c.IsRevoked(certificate)))
                return new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "ZD04", ErrorMessage = "Certificado de la firma revocado.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds };

            return new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "ZD04", ErrorMessage = "Certificado de la firma se encuentra vigente.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds };
        }

        public List<ValidateListResponse> ValidateSign(IEnumerable<X509Crl> crls)
        {
            DateTime startDate = DateTime.UtcNow;
            var validateResponses = new List<ValidateListResponse>();

            Crls = crls ?? throw new ArgumentNullException(nameof(crls));

            var certificate = GetPrimaryCertificate();

            if (certificate == null)
            {
                validateResponses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "ZD06", ErrorMessage = "Cadena de confianza del certificado digital no se pudo validar. [Missing X509Certificate node]", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                return validateResponses;
            }

            var errors = new List<string>();
            validateResponses.Add(ValidateCrl(certificate));

            return validateResponses;
        }

        public List<ValidateListResponse> ValidateSign(IEnumerable<X509Certificate> chainCertificates, IEnumerable<X509Crl> crls)
        {
            DateTime startDate = DateTime.UtcNow;
            var validateResponses = new List<ValidateListResponse>();

            if (chainCertificates == null)
                throw new ArgumentNullException(nameof(chainCertificates));

            Crls = crls ?? throw new ArgumentNullException(nameof(crls));
            LoadCertificates(chainCertificates);

            var certificate = GetPrimaryCertificate();

            if (certificate == null)
            {
                validateResponses.Add(new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "ZD06", ErrorMessage = "Cadena de confianza del certificado digital no se pudo validar. [Missing X509Certificate node]", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds });
                return validateResponses;
            }


            var errors = new List<string>();
            validateResponses.Add(ValidateChainTrust(certificate));
            validateResponses.Add(ValidateCrl(certificate));

            return validateResponses;
        }

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
        public ValidateListResponse ValidateSoftware(ResponseXpathDataValue responseXpathDataValue, string trackId)
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

            var number = responseXpathDataValue.XpathsValues["NumberXpath"];
            var softwareId = responseXpathDataValue.XpathsValues["SoftwareIdXpath"];
            var SoftwareSecurityCode = responseXpathDataValue.XpathsValues["SoftwareSecurityCodeXpath"];

            //var number = softwareModel.SerieAndNumber;
            //var softwareId = softwareModel.SoftwareId;
            //var SoftwareSecurityCode = softwareModel.SoftwareSecurityCode;

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
        public List<ValidateListResponse> ValidateTaxLevelCodes(ResponseXpathDataValue responseXpathDataValue)
        {
            DateTime startDate = DateTime.UtcNow;
            var responses = new List<ValidateListResponse>();

            var typeListInstance = GetTypeListInstanceCache();
            var typeListvalues = typeListInstance.Value.Split(';');

            //Sender tax level code validation
            var isValid = true;
            var senderTaxLevelCodes = responseXpathDataValue.XpathsValues["SenderTaxLevelCodeXpath"].Split(';');
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
            var additionalAccountId = responseXpathDataValue.XpathsValues["AdditionalAccountIDXpath"];
            var receiverTaxLevelCodes = responseXpathDataValue.XpathsValues["ReceiverTaxLevelCodeXpath"].Split(';');
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
            var deliveryTaxLevelCodes = responseXpathDataValue.XpathsValues["DeliveryTaxLevelCodeXpath"].Split(';');
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
            var sheldHolderTaxLevelCodeItems = responseXpathDataValue.XpathsValues["SheldHolderTaxLevelCodeXpath"].Split('|');
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
        private static bool IsSelfSigned(X509Certificate certificate)
        {
            return certificate.IssuerDN.Equivalent(certificate.SubjectDN);
        }
        private void LoadCertificates(IEnumerable<X509Certificate> chainCertificates)
        {
            foreach (var root in chainCertificates)
            {
                if (IsSelfSigned(root))
                    _trustedRoots.Add(new TrustAnchor(root, null));
                else
                    _intermediates.Add(root);
            }
        }
        private ValidateListResponse ValidateChainTrust(X509Certificate certificate)
        {
            DateTime startDate = DateTime.UtcNow;
            try
            {
                var selector = new X509CertStoreSelector { Certificate = certificate };

                var builderParams = new PkixBuilderParameters(_trustedRoots, selector) { IsRevocationEnabled = false };
                builderParams.AddStore(X509StoreFactory.Create(CertificatesCollection, new X509CollectionStoreParameters(_intermediates)));
                builderParams.AddStore(X509StoreFactory.Create(CertificatesCollection, new X509CollectionStoreParameters(new[] { certificate })));

                var builder = new PkixCertPathBuilder();
                var result = builder.Build(builderParams);
            }
            catch (PkixCertPathBuilderException e)
            {
                Debug.WriteLine(e.InnerException?.Message ?? e.Message);
                return new ValidateListResponse { IsValid = false, Mandatory = true, ErrorCode = "ZD05", ErrorMessage = $"Error en la cadena de certificación.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds };
            }

            return new ValidateListResponse { IsValid = true, Mandatory = true, ErrorCode = "ZD05", ErrorMessage = "Cadena de confianza del certificado digital correcta.", ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds };
        }
        private static decimal TruncateDecimal(decimal value, int preision)
        {
            decimal step = (decimal)Math.Pow(10, preision);
            decimal tmp = Math.Truncate(step * value);
            return tmp / step;
        }
        private bool ValidateDigitCode(string code, int digit)
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

        private bool InBounds(int index, string[] array)
        {
            return (index >= 0) && (index < array.Length);
        }
        #endregion
    }
}
