using Gosocket.Dian.Domain.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
//using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Plugin.Functions.Models;
using Gosocket.Dian.Services.Utils.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gosocket.Dian.Plugin.Functions.Common
{
    public class ValidatorEngine
    {
        #region Global properties
        static readonly string container = "dian";
        //static readonly string crtsFolder = "certificates/crts";

        static readonly string crlsFolder = "certificates/crls";
        private static readonly TableManager tableManagerGlobalLogger = new TableManager("GlobalLogger");
        #endregion

        public ValidatorEngine() { }

        private static ValidatorEngine _instance = null;

        public static ValidatorEngine Instance => _instance ?? (_instance = new ValidatorEngine());

        public async Task<List<ValidateListResponse>> StartContingencyValidationAsync(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();

            //var requestObj = new { trackId };
            //var response = await ApiHelpers.ExecuteRequestAsync<ResponseDownloadXml>(ConfigurationManager.GetValue("DownloadXmlUrl"), requestObj);
            //if (!response.Success)
            //    throw new Exception(response.Message);

            //var xmlBytes = Convert.FromBase64String(response.XmlBase64);

            var xmlBytes = await GetXmlFromStorageAsync(trackId);
            if (xmlBytes == null) throw new Exception("Xml not found.");

            // Validator instance
            var validator = new Validator(xmlBytes);
            validateResponses.Add(validator.ValidateContingency());


            return validateResponses;
        }

        public async Task<List<ValidateListResponse>> StartCufeValidationAsync(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();

            var requestObject = CreateCufeXpathsRequestObject(trackId);
            var responseXpathDataValue = await ApiHelpers.ExecuteRequestAsync<ResponseXpathDataValue>(ConfigurationManager.GetValue("GetXpathDataValuesUrl"), requestObject);
            if (!responseXpathDataValue.Success)
                throw new Exception(responseXpathDataValue.Message);
            // Validator instance
            var validator = new Validator();
            validateResponses.Add(validator.ValidateCufe(responseXpathDataValue, trackId));

            return validateResponses;
        }

        public List<ValidateListResponse> StartDocumentDuplicityValidationAsync(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();

            // Validator instance
            var validator = new Validator();
            validateResponses.Add(validator.ValidateDocumentDuplicity(trackId));

            return validateResponses;
        }

        public async Task<List<ValidateListResponse>> StartNitValidationAsync(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();

            var requestObject = CreateNitsXpathsRequestObject(trackId);
            var responseXpathDataValue = await ApiHelpers.ExecuteRequestAsync<ResponseXpathDataValue>(ConfigurationManager.GetValue("GetXpathDataValuesUrl"), requestObject);
            if (!responseXpathDataValue.Success)
                throw new Exception(responseXpathDataValue.Message);
            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateNit(responseXpathDataValue, trackId));

            return validateResponses;
        }

        public List<ValidateListResponse> StartNoteReferenceValidation(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();

            // Validator instance
            var validator = new Validator();
            validateResponses.Add(validator.ValidateNoteReference(trackId));

            return validateResponses;
        }

        public async Task<List<ValidateListResponse>> StartNumberingRangeValidationAsync(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();

            var requestObject = CreateNumberRangepathsRequestObject(trackId);
            var responseXpathDataValue = await ApiHelpers.ExecuteRequestAsync<ResponseXpathDataValue>(ConfigurationManager.GetValue("GetXpathDataValuesUrl"), requestObject);
            if (!responseXpathDataValue.Success)
                throw new Exception(responseXpathDataValue.Message);
            // Validator instance
            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateNumberingRange(responseXpathDataValue, trackId));

            return validateResponses;
        }

        public async Task<List<ValidateListResponse>> StartSignValidationAsync(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();

            // Get xmlBase64 from function
            //var requestObj = new { trackId };
            //var response = await ApiHelpers.ExecuteRequestAsync<ResponseDownloadXml>(ConfigurationManager.GetValue("DownloadXmlUrl"), requestObj);
            //if (!response.Success) throw new Exception(response.Message);
            //var xmlBytes = Convert.FromBase64String(response.XmlBase64);

            var xmlBytes = await GetXmlFromStorageAsync(trackId);
            if (xmlBytes == null) throw new Exception("Xml not found.");

            // Validator instance
            var validator = new Validator(xmlBytes);

            //
            validateResponses.AddRange(validator.ValidateSignXades());

            // Get all crt certificates
            //var crts = await CertificateManager.Instance.GetRootCertificates(container, crtsFolder);

            // Get all crls
            var crls = Application.Managers.CertificateManager.Instance.GetCrls(container, crlsFolder);

            validateResponses.AddRange(validator.ValidateSign(crls));

            return validateResponses;
        }

        public async Task<List<ValidateListResponse>> StartSoftwareValidationAsync(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();

            var requestObject = CreateSoftwareXpathsRequestObject(trackId);
            var responseXpathDataValue = await ApiHelpers.ExecuteRequestAsync<ResponseXpathDataValue>(ConfigurationManager.GetValue("GetXpathDataValuesUrl"), requestObject);
            if (!responseXpathDataValue.Success)
                throw new Exception(responseXpathDataValue.Message);

            //var xmlBytes = await GetXmlFromStorageAsync(trackId);
            //var xmlParser = new XmlParser(xmlBytes);
            //if (!xmlParser.Parser())
            //    throw new Exception(xmlParser.ParserError);

            //var softwareModel = xmlParser.Fields.ToObject<SoftwareModel>();

            var validator = new Validator();
            validateResponses.Add(validator.ValidateSoftware(responseXpathDataValue, trackId));
            //validateResponses.Add(validator.ValidateSoftware(softwareModel, trackId));

            return validateResponses;
        }

        public async Task<List<ValidateListResponse>> StartTaxLevelCodesValidationAsync(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();

            var requestObject = CreateTaxLevelCodeXpathsRequestObject(trackId);
            var responseXpathDataValue = await ApiHelpers.ExecuteRequestAsync<ResponseXpathDataValue>(ConfigurationManager.GetValue("GetXpathDataValuesUrl"), requestObject);
            if (!responseXpathDataValue.Success)
                throw new Exception(responseXpathDataValue.Message);

            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateTaxLevelCodes(responseXpathDataValue));

            return validateResponses;
        }

        #region Private methods
        //private Dictionary<string, string> CreateContingencyXpathsRequestObject(string trackId)
        //{
        //    var requestObj = new Dictionary<string, string>
        //    {
        //            { "TrackId", trackId},
        //            { "SigningDateTimeXpath","//xades:SigningTime"},
        //    };

        //    return requestObj;
        //}

        private Dictionary<string, string> CreateCufeXpathsRequestObject(string trackId)
        {
            var requestObj = new Dictionary<string, string>
            {
                    { "TrackId", trackId},
                    { "DocumentKeyXpath","//*[local-name()='Invoice']/*[local-name()='UUID']|//*[local-name()='CreditNote']/*[local-name()='UUID']|//*[local-name()='DebitNote']/*[local-name()='UUID']"},
                    { "NumberXpath", "/sig:Invoice/cbc:ID|/sig:CreditNote/cbc:ID|/sig:DebitNote/cbc:ID"},
                    { "EmissionDateXpath", "/sig:Invoice/cbc:IssueDate|/sig:CreditNote/cbc:IssueDate|/sig:DebitNote/cbc:IssueDate" },
                    { "HourEmissionXpath", "/sig:Invoice/cbc:IssueTime|/sig:CreditNote/cbc:IssueTime|/sig:DebitNote/cbc:IssueTime" },
                    { "AmountXpath", "/sig:Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount|/sig:CreditNote/cac:LegalMonetaryTotal/cbc:LineExtensionAmount|/sig:DebitNote/cac:RequestedMonetaryTotal/cbc:LineExtensionAmount" },
                    { "TaxAmount1Xpath", "/sig:Invoice/cac:TaxTotal[cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID='01']/cbc:TaxAmount|/sig:CreditNote/cac:TaxTotal[cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID='01']/cbc:TaxAmount|/sig:DebitNote/cac:TaxTotal[cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID='01']/cbc:TaxAmount" },
                    { "TaxAmount2Xpath", "/sig:Invoice/cac:TaxTotal[cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID='04']/cbc:TaxAmount|/sig:CreditNote/cac:TaxTotal[cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID='04']/cbc:TaxAmount|/sig:DebitNote/cac:TaxTotal[cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID='04']/cbc:TaxAmount" },
                    { "TaxAmount3Xpath", "/sig:Invoice/cac:TaxTotal[cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID='03']/cbc:TaxAmount|/sig:CreditNote/cac:TaxTotal[cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID='03']/cbc:TaxAmount|/sig:DebitNote/cac:TaxTotal[cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:ID='03']/cbc:TaxAmount" },
                    { "AmountToPayXpath", "/sig:Invoice/cac:LegalMonetaryTotal/cbc:PayableAmount|/sig:CreditNote/cac:LegalMonetaryTotal/cbc:PayableAmount|/sig:DebitNote/cac:RequestedMonetaryTotal/cbc:PayableAmount" },
                    { "SenderCodeXpath", "/sig:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID|/sig:CreditNote/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID|/sig:DebitNote/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID" },
                    { "DocumentTypeId", "" },
                    { "DocumentTypeXpath", "//*[local-name()='InvoiceTypeCode']" },
                    { "ReceiverCodeXpath", "/sig:Invoice/ cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID|/sig:CreditNote/ cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID|/sig:DebitNote/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID" },
                    { "EnvironmentTypeXpath", "/sig:Invoice/cbc:ProfileExecutionID|/sig:CreditNote/cbc:ProfileExecutionID|/sig:DebitNote/cbc:ProfileExecutionID" },
                    { "SoftwareIdXpath", "//ext:UBLExtensions/ext:UBLExtension/ext:ExtensionContent/sts:DianExtensions/sts:SoftwareProvider/sts:SoftwareID" },
            };

            return requestObj;
        }
        private Dictionary<string, string> CreateNitsXpathsRequestObject(string trackId)
        {
            var requestObj = new Dictionary<string, string>
            {
                    { "TrackId", trackId},
                    { "SenderCodeXpath", "//cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID" },
                    { "SenderCodeDigitXpath", "//cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID/@schemeID" },
                    { "SenderCodeProviderXpath","//cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID"},
                    { "SenderCodeProviderDigitXpath","//cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID/@schemeID" },
                    { "ReceiverCodeXpath", "//cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID" },
                    { "ReceiverCodeSchemeNameValueXpath", "//cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID/@schemeID" },
                    { "ReceiverCodeDigitXpath", "//cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID/@schemeID" },
                    { "ReceiverCode2Xpath", "//cac:AccountingCustomerParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID" },
                    { "ReceiverCode2SchemeNameValueXpath", "//cac:AccountingCustomerParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID/@schemeID" },
                    { "ReceiverCode2DigitXpath", "//cac:AccountingCustomerParty/cac:Party/cac:PartyLegalEntity/cbc:CompanyID/@schemeID" },
                    { "SoftwareProviderCodeXpath","//ext:UBLExtensions/ext:UBLExtension/ext:ExtensionContent/sts:DianExtensions/sts:SoftwareProvider/sts:ProviderID"},
                    { "SoftwareProviderCodeDigitXpath","//ext:UBLExtensions/ext:UBLExtension/ext:ExtensionContent/sts:DianExtensions/sts:SoftwareProvider/sts:ProviderID/@schemeID" },
                    { "SheldHolderSchemaNameValueXpath", "//cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cac:ShareholderParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID/@schemeName"},
                    { "SheldHolderCodeXpath", "//cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cac:ShareholderParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID"},
                    { "SheldHolderCodeDigitXpath", "//cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cac:ShareholderParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID/@schemeID"},
                    { "AgentPartySchemaNameValueXpath", "//cac:InvoiceLine/cac:Item/cac:InformationContentProviderParty/cac:PowerOfAttorney/cac:AgentParty/cac:PartyIdentification/cbc:ID/@schemeName" },
                    { "AgentPartyCodeXpath","//cac:InvoiceLine/cac:Item/cac:InformationContentProviderParty/cac:PowerOfAttorney/cac:AgentParty/cac:PartyIdentification/cbc:ID"},
                    { "AgentPartyCodeDigitXpath","//cac:InvoiceLine/cac:Item/cac:InformationContentProviderParty/cac:PowerOfAttorney/cac:AgentParty/cac:PartyIdentification/cbc:ID/@schemeID"}

            };
            return requestObj;
        }
        private Dictionary<string, string> CreateNumberRangepathsRequestObject(string trackId)
        {
            var requestObj = new Dictionary<string, string>
            {
                    { "TrackId", trackId},
                    { "SenderCodeXpath", "//cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID" },
                    { "InvoiceAuthorizationXpath", "//ext:UBLExtensions/ext:UBLExtension/ext:ExtensionContent/sts:DianExtensions/sts:InvoiceControl/sts:InvoiceAuthorization" },
                    { "StartDateXpath", "//ext:UBLExtensions/ext:UBLExtension/ext:ExtensionContent/sts:DianExtensions/sts:InvoiceControl/sts:AuthorizationPeriod/cbc:StartDate"},
                    { "EndDateXpath", "//ext:UBLExtensions/ext:UBLExtension/ext:ExtensionContent/sts:DianExtensions/sts:InvoiceControl/sts:AuthorizationPeriod/cbc:EndDate" },
                    { "PrefixXpath", "//ext:UBLExtensions/ext:UBLExtension/ext:ExtensionContent/sts:DianExtensions/sts:InvoiceControl/sts:AuthorizedInvoices/sts:Prefix" },
                    { "StartNumberXpath", "//ext:UBLExtensions/ext:UBLExtension/ext:ExtensionContent/sts:DianExtensions/sts:InvoiceControl/sts:AuthorizedInvoices/sts:From" },
                    { "EndNumberXpath", "//ext:UBLExtensions/ext:UBLExtension/ext:ExtensionContent/sts:DianExtensions/sts:InvoiceControl/sts:AuthorizedInvoices/sts:To" },
                    { "SoftwareIdXpath", "//ext:UBLExtensions/ext:UBLExtension/ext:ExtensionContent/sts:DianExtensions/sts:SoftwareProvider/sts:SoftwareID" },
            };

            return requestObj;
        }
        private Dictionary<string, string> CreateSoftwareXpathsRequestObject(string trackId)
        {
            var requestObj = new Dictionary<string, string>
            {
                    { "TrackId", trackId},
                    { "NumberXpath", "/sig:Invoice/cbc:ID|/sig:CreditNote/cbc:ID|/sig:DebitNote/cbc:ID"},
                    { "SoftwareIdXpath", "//ext:UBLExtensions/ext:UBLExtension/ext:ExtensionContent/sts:DianExtensions/sts:SoftwareProvider/sts:SoftwareID" },
                    { "SoftwareSecurityCodeXpath", "//ext:UBLExtensions/ext:UBLExtension/ext:ExtensionContent/sts:DianExtensions/sts:SoftwareSecurityCode" }
            };

            return requestObj;
        }
        private Dictionary<string, string> CreateTaxLevelCodeXpathsRequestObject(string trackId)
        {
            var requestObj = new Dictionary<string, string>
            {
                    { "TrackId", trackId},
                    { "SenderTaxLevelCodeXpath", "/sig:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:TaxLevelCode"},
                    {"AdditionalAccountIDXpath", "//cac:AccountingCustomerParty/cbc:AdditionalAccountID" },
                    { "ReceiverTaxLevelCodeXpath", "/sig:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:TaxLevelCode"},
                    { "DeliveryTaxLevelCodeXpath", "/sig:Invoice/cac:Delivery/cac:DeliveryParty/cac:PartyTaxScheme/cbc:TaxLevelCode" },
                    { "SheldHolderTaxLevelCodeXpath", "/sig:Invoice/cac:Delivery/cac:DeliveryParty/cac:PartyTaxScheme/cbc:TaxLevelCode" }
            };
            return requestObj;
        }

        public async Task<byte[]> GetXmlFromStorageAsync(string trackId)
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
    }
}
