

namespace Gosocket.Dian.Application
{
    #region Using

    using Gosocket.Dian.Application.Cosmos;
    using Gosocket.Dian.Domain.Domain;
    using Gosocket.Dian.Domain.Entity;
    using Gosocket.Dian.Infrastructure;
    using Gosocket.Dian.Interfaces.Services;
    using Gosocket.Dian.Services.Utils.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Threading.Tasks;

    #endregion

    public class RadianSupportDocument : IRadianSupportDocument
    {
        #region Properties

        private readonly IQueryAssociatedEventsService _queryAssociatedEventsService;
        private readonly FileManager _fileManager;
        private readonly CosmosDBService _cosmosDBService;

        #endregion

        #region Constructor

        public RadianSupportDocument(IQueryAssociatedEventsService queryAssociatedEventsService, FileManager fileManager, CosmosDBService cosmosDBService)
        {
            _queryAssociatedEventsService = queryAssociatedEventsService;
            _fileManager = fileManager;
            _cosmosDBService = cosmosDBService;
        }

        #endregion

        #region GetPdfReport

        public async Task<byte[]> GetGraphicRepresentation(string cude)
        {
            // Load Templates            
            StringBuilder template = new StringBuilder(_fileManager.GetText("radian-documents-templates", "RepresentacionGraficaDocumentoSoporte.html"));

            // Load xml        
            // TODO: Cargar documento con cufe
            byte[] xmlBytes = await GetXmlFromStorageAsync(cude);
            //_fileManager.GetBytes("radian-documents-templates", "XMLDocumentoSoporte.xml");

            // Load xpaths
            Dictionary<string, string> xpathRequest = CreateGetXpathDataValuesRequestObject(Convert.ToBase64String(xmlBytes), "RepresentacionGrafica");

            try
            {
                ResponseXpathDataValue fieldValues = ApiHelpers.ExecuteRequest<ResponseXpathDataValue>("https://global-function-docvalidator-sbx.azurewebsites.net/api/GetXpathDataValues?code=tyW3skewKS1q4GuwaOj0PPj3mRHa5OiTum60LfOaHfEMQuLbvms73Q==", xpathRequest);

                // Mapping Fields
                template = TemplateGlobalMapping(template, fieldValues);


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            byte[] report = RadianPdfCreationService.GetPdfBytes(template.ToString());

            return report;
        }

        #endregion

        #region GetXmlFromStorageAsync

        /// <summary>
        /// Método de extracción del xml de la representación grafica
        /// TODO: pendiente de incorporar, hasta q se haga consulta por cufe
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns></returns>
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

        #region CreateGetXpathDataValuesRequestObject

        private static Dictionary<string, string> CreateGetXpathDataValuesRequestObject(string xmlBase64, string fileName = null)
        {
            var requestObj = new Dictionary<string, string>
            {
                { "XmlBase64", xmlBase64},
                { "FileName", fileName},
                { "SupportDocumentNumber", "/*[local-name()='Invoice']/*[local-name()='ID']" },
                { "Cude", "/*[local-name() = 'Invoice']/*[local-name() = 'UUID']" },
                { "EmissionDate", "/*[local-name() = 'Invoice']/*[local-name() = 'IssueDate']" },
                { "OperationType", "/*[local-name() = 'Invoice']/*[local-name() = 'CustomizationID'] " },
                { "Prefix", "/*[local-name()='Invoice']/*[local-name()='UBLExtensions']/*[local-name()='UBLExtension']/*[local-name()='ExtensionContent']/*[local-name()='DianExtensions']/*[local-name()='InvoiceControl']/*[local-name()='AuthorizedInvoices']/*[local-name()='Prefix']" },
                // Seller Data
                { "SellerNit", "//*[local-name() = 'AccountingSupplierParty']/*[local-name() = 'Party']/*[local-name() = 'PartyTaxScheme']/*[local-name() = 'CompanyID']" },
                { "SellerBusinessName", "/*[local-name() = 'Invoice']/*[local-name() = 'AccountingSupplierParty']/*[local-name() = 'Party']/*[local-name() = 'PartyLegalEntity']/*[local-name() = 'RegistrationName']"},
                { "SellerDocumentType","//*[local-name() = 'AccountingSupplierParty']/*[local-name() = 'Party']/*[local-name() = 'PartyTaxScheme']/*[local-name() = 'CompanyID']/@schemeName"},
                { "SellerTaxpayerType","//*[local-name() = 'AccountingSupplierParty']/*[local-name() = 'AdditionalAccountID']"},
                { "SellerResponsibilityType","//*[local-name() = 'AccountingSupplierParty']/*[local-name() = 'Party']/*[local-name() = 'PartyTaxScheme']/*[local-name() = 'TaxLevelCode']"},
                { "SellerAddress","/*[local-name() = 'Invoice']/*[local-name() = 'AccountingSupplierParty']/*[local-name() = 'Party']/*[local-name() = 'PhysicalLocation']/*[local-name() = 'Address']/*[local-name() = 'AddressLine']/*[local-name() = 'Line']"},
                { "SellerState", "" },
                { "SellerMunicipality", "/*[local-name() = 'Invoice']/*[local-name() = 'AccountingSupplierParty']/*[local-name() = 'Party']/*[local-name() = 'PhysicalLocation']/*[local-name() = 'Address']/*[local-name() = 'CityName']" },
                { "SellerEmail", "/*[local-name() = 'Invoice']/*[local-name() = 'AccountingSupplierParty']/*[local-name() = 'Party']/*[local-name() = 'Contact']/*[local-name() = 'ElectronicMail']" },
                { "SellerPhoneNumber", "/*[local-name() = 'Invoice']/*[local-name() = 'AccountingSupplierParty']/*[local-name() = 'Party']/*[local-name() = 'Contact']/*[local-name() = 'Telephone']" },
                // Acquirer Data
                { "AcquirerNit", "//*[local-name() = 'AccountingCustomerParty']/*[local-name() = 'Party']/*[local-name() = 'PartyTaxScheme']/*[local-name() = 'CompanyID']" },
                { "AcquirerBusinessName", "/*[local-name() = 'Invoice']/*[local-name() = 'AccountingCustomerParty']/*[local-name() = 'Party']/*[local-name() = 'PartyTaxScheme']/*[local-name() = 'RegistrationName']" },
                { "AcquirerDocumentType", "//*[local-name() = 'AccountingCustomerParty']/*[local-name() = 'Party']/*[local-name() = 'PartyTaxScheme']/*[local-name() = 'CompanyID']/@schemeName" },
                { "AcquirerTradeName", "/*[local-name() = 'Invoice']/*[local-name() = 'AccountingCustomerParty']/*[local-name() = 'Party']/*[local-name() = 'PartyTaxScheme']/*[local-name() = 'RegistrationName']" },
                { "AcquirerTaxpayerType", "//*[local-name() = 'AccountingCustomerParty']/*[local-name() = 'AdditionalAccountID']" },
                { "AcquirerMainEconomicActivity", "/*[local-name() = 'Invoice']/*[local-name() = 'AccountingCustomerParty']/*[local-name() = 'Party']/*[local-name() = 'IndustryClassificationCode']" },
                { "AcquirerResponsibilityType", "//*[local-name() = 'AccountingCustomerParty']/*[local-name() = 'Party']/*[local-name() = 'PartyTaxScheme']/*[local-name() = 'TaxLevelCode']" },
                { "AcquirerAddress", "/*[local-name() = 'Invoice']/*[local-name() = 'AccountingCustomerParty']/*[local-name() = 'Party']/*[local-name() = 'PhysicalLocation']/*[local-name() = 'Address']/*[local-name() = 'AddressLine']/*[local-name() = 'Line']" },
                { "AcquirerMunicipality", "/*[local-name() = 'Invoice']/*[local-name() = 'AccountingCustomerParty']/*[local-name() = 'Party']/*[local-name() = 'PhysicalLocation']/*[local-name() = 'Address']/*[local-name() = 'CityName']" },
                { "AcquirerEmail", "/*[local-name() = 'Invoice']/*[local-name() = 'AccountingCustomerParty']/*[local-name() = 'Party']/*[local-name() = 'Contact']/*[local-name() = 'ElectronicMail']" },
                { "AcquirerPhoneNumber", "/*[local-name() = 'Invoice']/*[local-name() = 'AccountingCustomerParty']/*[local-name() = 'Party']/*[local-name() = 'Contact']/*[local-name() = 'Telephone']" },
                // Product Data
                { "ProductNumber", "/*[local-name() = 'Invoice']/*[local-name() = 'InvoiceLine']/*[local-name() = 'ID']" },
                { "ProductCode", "/*[local-name() = 'Invoice']/*[local-name() = 'InvoiceLine']/*[local-name() = 'Item']/*[local-name() = 'SellersItemIdentification']/*[local-name() = 'ID']" },
                { "ProductDescription", "/*[local-name() = 'Invoice']/*[local-name() = 'InvoiceLine']/*[local-name() = 'Item']/*[local-name() = 'Description']" },
                { "ProductUM", "/*[local-name() = 'Invoice']/*[local-name() = 'InvoiceLine']/*[local-name() = 'InvoicedQuantity']/@unitCode" },
                { "ProductQuantity", "/*[local-name() = 'Invoice']/*[local-name() = 'InvoiceLine']/*[local-name() = 'InvoicedQuantity']" },
                { "ProductUnitPrice", "/*[local-name() = 'Invoice']/*[local-name() = 'InvoiceLine']/*[local-name() = 'LineExtensionAmount']" },
                { "ProductChargeIndicator", "/*[local-name() = 'Invoice']/*[local-name() = 'InvoiceLine']/*[local-name() = 'AllowanceCharge']/*[local-name() = 'ChargeIndicator']" },
                { "ProductDiscount", "/*[local-name() = 'Invoice']/*[local-name() = 'InvoiceLine']/*[local-name() = 'AllowanceCharge']/*[local-name() = 'Amount']" },
                { "ProductSurcharge", "/*[local-name() = 'Invoice']/*[local-name() = 'InvoiceLine']/*[local-name() = 'AllowanceCharge']/*[local-name() = 'Amount']" },
                { "ProductIvaTax", "/*[local-name() = 'Invoice']/*[local-name() = 'InvoiceLine']/*[local-name() = 'TaxTotal']/*[local-name() = 'TaxAmount']" },
                { "ProductSellValue", "/*[local-name() = 'Invoice']/*[local-name() = 'InvoiceLine']/*[local-name() = 'LineExtensionAmount']" },
                // Global Discounts and Surcharges
                { "DiscountNumber", "/*[local-name() = 'Invoice']/*[local-name() = 'AllowanceCharge']/*[local-name() = 'ID']" },
                { "DiscountType", "/*[local-name() = 'Invoice']/*[local-name() = 'AllowanceCharge']/*[local-name() = 'ChargeIndicator']" },
                { "DiscountCode", "/*[local-name() = 'Invoice']/*[local-name() = 'AllowanceCharge']/*[local-name() = 'AllowanceChargeReasonCode']" },
                { "DiscountDescription", "/*[local-name() = 'Invoice']/*[local-name() = 'AllowanceCharge']/*[local-name() = 'AllowanceChargeReason']" },
                { "DiscountPercentage", "/*[local-name() = 'Invoice']/*[local-name() = 'AllowanceCharge']/*[local-name() = 'MultiplierFactorNumeric']" },
                { "DiscountAmount", "/*[local-name() = 'Invoice']/*[local-name() = 'AllowanceCharge']/*[local-name() = 'Amount']" },
                // ToTal Advances
                { "AdvanceNumber", "/*[local-name() = 'Invoice']/*[local-name() = 'PrepaidPayment']/*[local-name() = 'ID']" },
                { "AdvanceAmount", "/*[local-name() = 'Invoice']/*[local-name() = 'PrepaidPayment']/*[local-name() = 'PaidAmount']" },
                // ToTal Retentions
                { "RetentionNumber", "/*[local-name() = 'Invoice']/*[local-name() = 'WithholdingTaxTotal']/*[local-name() = 'TaxSubtotal']/*[local-name() = 'TaxCategory']/*[local-name() = 'TaxScheme']/*[local-name() = 'ID']" },
                { "RetentionAmount", "/*[local-name() = 'Invoice']/*[local-name() = 'WithholdingTaxTotal']/*[local-name() = 'TaxAmount']" },
                // Total Data
                { "TotalCurrency", "/*/*[local-name() = 'LegalMonetaryTotal']/*[local-name() = 'LineExtensionAmount']/@currencyID" },
                { "TotalExchangeRate", "/*[local-name() = 'Invoice']/*[local-name() = 'PaymentAlternativeExchangeRate']/*[local-name() = 'CalculationRate']" },
                { "TotalUnitPrice", "format-number(number(//cac:LegalMonetaryTotal/cbc:LineExtensionAmount) + sum(//cac:InvoiceLine/cac:AllowanceCharge[cbc:ChargeIndicator = false()]/cbc:Amount) - sum(//cac:InvoiceLine/cac:AllowanceCharge[cbc:ChargeIndicator = true()]/cbc:Amount), '#.##0,00', 'CountryDecimalFormat')" },
                { "TotalDiscountsDetail", "/*/cac:InvoiceLine/cac:AllowanceCharge[cbc:ChargeIndicator = false()]/cbc:Amount" },
                { "TotalSurchargesDetail", "//cac:InvoiceLine/cac:AllowanceCharge[cbc:ChargeIndicator = true()]/cbc:Amount" },
                { "TotalTaxableBase", "/*[local-name() = 'Invoice']/*[local-name() = 'LegalMonetaryTotal']/*[local-name() = 'TaxExclusiveAmount'] | /*[local-name() = 'CreditNote']/*[local-name() = 'LegalMonetaryTotal']/*[local-name() = 'TaxExclusiveAmount']" },
                { "TotalTaxesDetail", "/*/*[local-name() = 'TaxTotal'][*[local-name() = 'TaxSubtotal']/*[local-name() = 'TaxCategory']/*[local-name() = 'TaxScheme']/*[local-name() = 'ID'] = '01']/*[local-name() = 'TaxAmount']" },
                { "TotalOtherTaxes", "/*/*[local-name() = 'TaxTotal']/*[local-name() = 'TaxSubtotal']/*[local-name() = 'TaxAmount']" },
                { "TotalTaxes", "/*/*[local-name() = 'WithholdingTaxTotal']/*[local-name() = 'TaxAmount']" },
                { "GlobalDiscounts", "/*[local-name() = 'Invoice']/*[local-name() = 'LegalMonetaryTotal']/*[local-name() = 'AllowanceTotalAmount']" },
                { "GlobalSurcharges", "/*[local-name() = 'Invoice']/*[local-name() = 'LegalMonetaryTotal']/*[local-name() = 'ChargeTotalAmount']" },
                { "TotalAmount", "/*/*[local-name() = 'LegalMonetaryTotal']/*[local-name() = 'LineExtensionAmount']" },
                // Final Data
                { "AuthorizationNumber", "//*[local-name() = 'UBLExtensions']/*[local-name() = 'UBLExtension']/*[local-name() = 'ExtensionContent']/*[local-name() = 'DianExtensions']/*[local-name() = 'InvoiceControl']/*[local-name() = 'InvoiceAuthorization']" },
                { "AuthorizedRangeFrom", "//*[local-name() = 'UBLExtensions']/*[local-name() = 'UBLExtension']/*[local-name() = 'ExtensionContent']/*[local-name() = 'DianExtensions']/*[local-name() = 'InvoiceControl']/*[local-name() = 'AuthorizedInvoices']/*[local-name() = 'From']" },
                { "AuthorizedRangeTo", "//*[local-name() = 'UBLExtensions']/*[local-name() = 'UBLExtension']/*[local-name() = 'ExtensionContent']/*[local-name() = 'DianExtensions']/*[local-name() = 'InvoiceControl']/*[local-name() = 'AuthorizedInvoices']/*[local-name() = 'To']" },
                { "ValidityDate", "//*[local-name() = 'UBLExtensions']/*[local-name() = 'UBLExtension']/*[local-name() = 'ExtensionContent']/*[local-name() = 'DianExtensions']/*[local-name() = 'InvoiceControl']/*[local-name() = 'AuthorizationPeriod']/*[local-name() = 'EndDate']" },
            };
            return requestObj;
        }

        #endregion

        #region TemplateGlobalMapping

        private StringBuilder TemplateGlobalMapping(StringBuilder template, ResponseXpathDataValue dataValues)
        {
            template = template.Replace( "{SupportDocumentNumber}", dataValues.XpathsValues["SupportDocumentNumber"]);
            template = template.Replace( "{Cude}", dataValues.XpathsValues["Cude"]);
            template = template.Replace( "{EmissionDate}", dataValues.XpathsValues["EmissionDate"]);
            template = template.Replace( "{OperationType}", dataValues.XpathsValues["OperationType"]);
            template = template.Replace( "{PaymentWay}", string.Empty);
            template = template.Replace( "{ExpirationDate}", string.Empty);
            template = template.Replace( "{PaymentMethod}", string.Empty);
            template = template.Replace( "{Prefix}", dataValues.XpathsValues["Prefix"]);
            // Seller Data
            template = template.Replace( "{SellerNit}", dataValues.XpathsValues["SellerNit"]);
            template = template.Replace( "{SellerBusinessName}", dataValues.XpathsValues["SellerBusinessName"]);
            template = template.Replace( "{SellerDocumentType}", dataValues.XpathsValues["SellerDocumentType"]);
            template = template.Replace( "{SellerDocumentNumber}", string.Empty);
            template = template.Replace( "{SellerOrigin}", string.Empty);
            template = template.Replace( "{SellerTaxpayerType}", dataValues.XpathsValues["SellerTaxpayerType"]);
            template = template.Replace( "{SellerResponsibilityType}", dataValues.XpathsValues["SellerResponsibilityType"]);
            template = template.Replace( "{SellerAddress}", dataValues.XpathsValues["SellerAddress"]);
            template = template.Replace( "{SellerState}", string.Empty );
            template = template.Replace( "{SellerMunicipality}", dataValues.XpathsValues["SellerMunicipality"]);
            template = template.Replace( "{SellerEmail}", dataValues.XpathsValues["SellerEmail"]);
            template = template.Replace( "{SellerPhoneNumber}", dataValues.XpathsValues["SellerPhoneNumber"]);
            // Acquirer Data
            template = template.Replace( "{AcquirerNit}", dataValues.XpathsValues["AcquirerNit"]);
            template = template.Replace( "{AcquirerBusinessName}", dataValues.XpathsValues["AcquirerBusinessName"]);
            template = template.Replace( "{AcquirerDocumentType}", dataValues.XpathsValues["AcquirerDocumentType"]);
            template = template.Replace(" {AcquirerDocumentNumber}", string.Empty);
            template = template.Replace( "{AcquirerTradeName}", dataValues.XpathsValues["AcquirerTradeName"]);
            template = template.Replace( "{AcquirerTaxpayerType}", dataValues.XpathsValues["AcquirerTaxpayerType"]);
            template = template.Replace( "{AcquirerMainEconomicActivity}", dataValues.XpathsValues["AcquirerMainEconomicActivity"]);
            template = template.Replace( "{AcquirerResponsibilityType}", dataValues.XpathsValues["AcquirerResponsibilityType"]);
            template = template.Replace( "{AcquirerAddress}", dataValues.XpathsValues["AcquirerAddress"]);
            template = template.Replace( "{AcquirerState}", string.Empty);
            template = template.Replace( "{AcquirerMunicipality}", dataValues.XpathsValues["AcquirerMunicipality"]);
            template = template.Replace( "{AcquirerEmail}", dataValues.XpathsValues["AcquirerEmail"]);
            template = template.Replace( "{AcquirerPhoneNumber}", dataValues.XpathsValues["AcquirerPhoneNumber"]);
            // Product Data
            template = template.Replace( "{ProductNumber}", "" );
            template = template.Replace( "{ProductCode}", "" );
            template = template.Replace( "{ProductDescription}", "" );
            template = template.Replace( "{ProductUM}", "" );
            template = template.Replace( "{ProductQuantity}", "" );
            template = template.Replace( "{ProductUnitPrice}", "" );
            template = template.Replace( "{ProductChargeIndicator}", "" );
            template = template.Replace( "{ProductDiscount}", "" );
            template = template.Replace( "{ProductSurcharge}", "" );
            template = template.Replace( "{ProductIvaTax}", "" );
            template = template.Replace( "{ProductSellValue}", "" );
            // Global Discounts and Surcharges
            template = template.Replace( "{DiscountNumber}", "" );
            template = template.Replace( "{DiscountType}", "" );
            template = template.Replace( "{DiscountCode}", "" );
            template = template.Replace( "{DiscountDescription}", "" );
            template = template.Replace( "{DiscountPercentage}", "" );
            template = template.Replace( "{DiscountAmount}", "" );
            // ToTal Advances
            template = template.Replace( "{AdvanceNumber}", dataValues.XpathsValues["AdvanceNumber"]);
            template = template.Replace( "{AdvanceAmount}", dataValues.XpathsValues["AdvanceAmount"]);
            // ToTal Retentions
            template = template.Replace( "{RetentionNumber}", dataValues.XpathsValues["RetentionNumber"]);
            template = template.Replace( "{RetentionAmount}", dataValues.XpathsValues["RetentionAmount"]);

            // Total Data
            template = template.Replace("{ValidationDate}", string.Empty);
            template = template.Replace("{GenerationDate}", DateTime.Now.ToShortDateString());
            template = template.Replace( "{TotalCurrency}", dataValues.XpathsValues["TotalCurrency"]);
            template = template.Replace( "{TotalExchangeRate}", dataValues.XpathsValues["TotalExchangeRate"]);
            template = template.Replace( "{TotalUnitPrice}", dataValues.XpathsValues["TotalUnitPrice"]);
            //template = template.Replace( "{TotalDiscountsDetail}", SplitAndSum(dataValues.XpathsValues["TotalDiscountsDetail"]).ToString());
            //template = template.Replace( "{TotalSurchargesDetail}", SplitAndSum(dataValues.XpathsValues["TotalSurchargesDetail"]).ToString());
            //template = template.Replace( "{TotalTaxableBase}", dataValues.XpathsValues["TotalTaxableBase"]);
            //template = template.Replace( "{TotalTaxesDetail}", SplitAndSum(dataValues.XpathsValues["TotalTaxesDetail"]).ToString());
            //template = template.Replace( "{TotalOtherTaxes}", SplitAndSum(dataValues.XpathsValues["TotalOtherTaxes"]).ToString());
            //template = template.Replace( "{TotalTaxes}", SplitAndSum(dataValues.XpathsValues["TotalTaxes"]).ToString());
            template = template.Replace( "{GlobalDiscounts}", dataValues.XpathsValues["GlobalDiscounts"]);
            template = template.Replace( "{GlobalSurcharges}", dataValues.XpathsValues["GlobalSurcharges"]);
            template = template.Replace( "{TotalAmount}", dataValues.XpathsValues["TotalAmount"]);
            // Final Data
            template = template.Replace( "{AuthorizationNumber}", dataValues.XpathsValues["AuthorizationNumber"]);
            template = template.Replace( "{AuthorizedRangeFrom}", dataValues.XpathsValues["AuthorizedRangeFrom"]);
            template = template.Replace( "{AuthorizedRangeTo}", dataValues.XpathsValues["AuthorizedRangeTo"]);
            template = template.Replace( "{ValidityDate}", dataValues.XpathsValues["ValidityDate"]);
            return template;
        }

        #endregion
    }
}
