

namespace Gosocket.Dian.Application
{
    #region Using

    using Gosocket.Dian.Application.Cosmos;
    using Gosocket.Dian.Domain.Common;
    using Gosocket.Dian.Domain.Domain;
    using Gosocket.Dian.Domain.Entity;
    using Gosocket.Dian.Infrastructure;
    using Gosocket.Dian.Interfaces.Services;
    using Gosocket.Dian.Services.Utils.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Xml;
    using System.Xml.Linq;

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

        #region GetGraphicRepresentation

        public async Task<byte[]> GetGraphicRepresentation(string cude, string webPath)
        {
            // Load Templates            
            StringBuilder template = new StringBuilder(_fileManager.GetText("radian-documents-templates", "RepresentacionGraficaDocumentoSoporte.html"));

            // Load xml
            byte[] xmlBytes = GetXmlFromStorageAsync(cude);
            var str = Encoding.Default.GetString(xmlBytes);

            // Load xpaths
            Dictionary<string, string> xpathRequest = CreateGetXpathDataValuesRequestObject(Convert.ToBase64String(xmlBytes), "RepresentacionGrafica");
           
            try
            {
                //string pathServiceData = ConfigurationManager.GetValue("GetXpathDataValuesUrl");
                string pathServiceData = "https://global-function-docvalidator-sbx.azurewebsites.net/api/GetXpathDataValues?code=tyW3skewKS1q4GuwaOj0PPj3mRHa5OiTum60LfOaHfEMQuLbvms73Q==";
                ResponseXpathDataValue fieldValues = ApiHelpers.ExecuteRequest<ResponseXpathDataValue>(pathServiceData, xpathRequest);

                Dictionary<string, string> newFieldValues = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> element in fieldValues.XpathsValues)
                {
                    newFieldValues.Add(element.Key, getUtfEncode(element.Value));
                }
                fieldValues.XpathsValues = newFieldValues;

                // Build QRCode
                Bitmap qrCode = RadianPdfCreationService.GenerateQR($"{webPath}?documentkey={cude}");
                string ImgDataURI = IronPdf.Util.ImageToDataUri(qrCode);
                string ImgQrCodeHtml = String.Format("<img class='qr-content' src='{0}'>", ImgDataURI);
                // Add QrValue into dictionary
                fieldValues.XpathsValues.Add("QrCode", ImgQrCodeHtml);

                // Mapping Fields
                template = TemplateGlobalMapping(template, fieldValues);
                template = MappingProducts(xmlBytes, template);
                template = MappingDiscounts(xmlBytes, template);
                template = MappingRetentions(xmlBytes, template);
                template = MappingAdvances(xmlBytes, template);


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            byte[] report = RadianPdfCreationService.GetPdfBytes(template.ToString(),"Representacion grafica");

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
        public static byte[] GetXmlFromStorageAsync(string trackId)
        {
            var TableManager = new TableManager("GlobalDocValidatorRuntime");
            var documentStatusValidation = TableManager.Find<GlobalDocValidatorRuntime>(trackId, "UPLOAD");
            if (documentStatusValidation == null)
                return null;

            var fileManager = new FileManager();
            var container = $"global";
            //var fileName = $"docvalidator/{documentStatusValidation.Category}/{documentStatusValidation.Timestamp.Date.Year}/{documentStatusValidation.Timestamp.Date.Month.ToString().PadLeft(2, '0')}/{trackId}.xml";
            var fileName = $"docvalidator/{documentStatusValidation.Category}/{documentStatusValidation.Timestamp.Date.Year}/{documentStatusValidation.Timestamp.Date.Month.ToString().PadLeft(2, '0')}/8bd6a4cf6b4e2ee29e608d38880669512256f9c1b054813467e41ce6330848852da3bf8b0310bf941aa7becea3e6740c.xml";
            var xmlBytes = fileManager.GetBytes(container, fileName);

            return xmlBytes;
        }

        #endregion

        #region GetXmlDianStorage
        internal static byte[] GetXmlDianFromStorage(string trackId, global::GlobalDocValidatorDocumentMeta documentMeta)

        {

            var fileManager = new FileManager();
            var container = $"dian";
            var fileName = $"responses/{documentMeta.SigningTimeStamp.Date.Year}/{documentMeta.SigningTimeStamp.Date.Month.ToString().PadLeft(2, '0')}/{documentMeta.SigningTimeStamp.Date.Day.ToString().PadLeft(2, '0')}/Success/{documentMeta.SenderCode}/01/SETG/{documentMeta.Number}/{trackId}.xml";
            var xmlBytes = fileManager.GetBytes(container, fileName);
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
                { "PaymentWay","/*[local-name() = 'Invoice']/*[local-name() = 'PaymentMeans']/*[local-name() = 'ID']"},
                { "PaymentMethod","/*[local-name() = 'Invoice']/*[local-name() = 'PaymentMeans']/*[local-name() = 'PaymentMeansCode']"},
                { "PaymentDueDate","/*[local-name() = 'Invoice']/*[local-name() = 'PaymentMeans']/*[local-name() = 'PaymentDueDate']"},
                // Seller Data
                { "SellerNit", "//*[local-name() = 'AccountingSupplierParty']/*[local-name() = 'Party']/*[local-name() = 'PartyTaxScheme']/*[local-name() = 'CompanyID']" },
                { "SellerBusinessName", "(//*[local-name() = 'Invoice']/*[local-name() = 'AccountingSupplierParty']/*[local-name() = 'Party']/*[local-name() = 'PartyLegalEntity']/*[local-name() = 'RegistrationName'])[1]"},
                { "SellerDocumentType","//*[local-name() = 'AccountingSupplierParty']/*[local-name() = 'Party']/*[local-name() = 'PartyTaxScheme']/*[local-name() = 'CompanyID']/@schemeName"},
                { "SellerDocumentNumber", "//*[local-name() = 'AccountingSupplierParty']/*[local-name() = 'Party']/*[local-name() = 'PartyTaxScheme']/*[local-name() = 'CompanyID']" },
                { "SellerTaxpayerType","//*[local-name() = 'AccountingSupplierParty']/*[local-name() = 'AdditionalAccountID']"},
                { "SellerOrigin","//*[local-name() = 'AccountingSupplierParty']/*[local-name() = 'AdditionalAccountID']/@schemeID"},
                { "SellerResponsibilityType","//*[local-name() = 'AccountingSupplierParty']/*[local-name() = 'Party']/*[local-name() = 'PartyTaxScheme']/*[local-name() = 'TaxLevelCode']"},
                { "SellerAddress","/*[local-name() = 'Invoice']/*[local-name() = 'AccountingSupplierParty']/*[local-name() = 'Party']/*[local-name() = 'PhysicalLocation']/*[local-name() = 'Address']/*[local-name() = 'AddressLine']/*[local-name() = 'Line']"},
                { "SellerState", "/*[local-name() = 'Invoice']/*[local-name() = 'AccountingSupplierParty']/*[local-name() = 'Party']/*[local-name() = 'PhysicalLocation']/*[local-name() = 'Address']/*[local-name() = 'CountrySubentity']" },
                { "SellerMunicipality", "/*[local-name() = 'Invoice']/*[local-name() = 'AccountingSupplierParty']/*[local-name() = 'Party']/*[local-name() = 'PhysicalLocation']/*[local-name() = 'Address']/*[local-name() = 'CityName']" },
                { "SellerEmail", "/*[local-name() = 'Invoice']/*[local-name() = 'AccountingSupplierParty']/*[local-name() = 'Party']/*[local-name() = 'Contact']/*[local-name() = 'ElectronicMail']" },
                { "SellerPhoneNumber", "/*[local-name() = 'Invoice']/*[local-name() = 'AccountingSupplierParty']/*[local-name() = 'Party']/*[local-name() = 'Contact']/*[local-name() = 'Telephone']" },
                // Acquirer Data
                { "AcquirerNit", "//*[local-name() = 'AccountingCustomerParty']/*[local-name() = 'Party']/*[local-name() = 'PartyTaxScheme']/*[local-name() = 'CompanyID']" },
                { "AcquirerBusinessName", "/*[local-name() = 'Invoice']/*[local-name() = 'AccountingCustomerParty']/*[local-name() = 'Party']/*[local-name() = 'PartyTaxScheme']/*[local-name() = 'RegistrationName']" },
                { "AcquirerDocumentType", "//*[local-name() = 'AccountingCustomerParty']/*[local-name() = 'Party']/*[local-name() = 'PartyTaxScheme']/*[local-name() = 'CompanyID']/@schemeName" },
                { "AcquirerDocumentNumber", "//*[local-name() = 'AccountingCustomerParty']/*[local-name() = 'Party']/*[local-name() = 'PartyTaxScheme']/*[local-name() = 'CompanyID']" },
                { "AcquirerTradeName", "/*[local-name() = 'Invoice']/*[local-name() = 'AccountingCustomerParty']/*[local-name() = 'Party']/*[local-name() = 'PartyTaxScheme']/*[local-name() = 'RegistrationName']" },
                { "AcquirerTaxpayerType", "//*[local-name() = 'AccountingCustomerParty']/*[local-name() = 'AdditionalAccountID']" },
                { "AcquirerMainEconomicActivity", "/*[local-name() = 'Invoice']/*[local-name() = 'AccountingCustomerParty']/*[local-name() = 'Party']/*[local-name() = 'IndustryClassificationCode']" },
                { "AcquirerResponsibilityType", "//*[local-name() = 'AccountingCustomerParty']/*[local-name() = 'Party']/*[local-name() = 'PartyTaxScheme']/*[local-name() = 'TaxLevelCode']" },
                { "AcquirerAddress", "/*[local-name() = 'Invoice']/*[local-name() = 'AccountingCustomerParty']/*[local-name() = 'Party']/*[local-name() = 'PhysicalLocation']/*[local-name() = 'Address']/*[local-name() = 'AddressLine']/*[local-name() = 'Line']" },
                { "AcquirerState", "/*[local-name() = 'Invoice']/*[local-name() = 'AccountingCustomerParty']/*[local-name() = 'Party']/*[local-name() = 'PhysicalLocation']/*[local-name() = 'Address']/*[local-name() = 'CountrySubentity']" },
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
                { "TotalUnitPrice", "//cac:LegalMonetaryTotal/cbc:LineExtensionAmount" },
                { "TotalDiscountsDetail", "//cac:InvoiceLine/cac:AllowanceCharge[cbc:ChargeIndicator = false()]/cbc:Amount" },
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
                { "Products", "/*[local-name() = 'Invoice']/*[local-name() = 'InvoiceLine']" },
            
            };
            return requestObj;
        }

        #endregion

        #region TemplateGlobalMapping

        private StringBuilder TemplateGlobalMapping(StringBuilder template, ResponseXpathDataValue dataValues)
        {
            // Document Data
            template = template.Replace( "{SupportDocumentNumber}", dataValues.XpathsValues["SupportDocumentNumber"]);
            template = template.Replace( "{Cude}", dataValues.XpathsValues["Cude"]);
            template = template.Replace( "{EmissionDate}", dataValues.XpathsValues["EmissionDate"]);

            if (!string.IsNullOrEmpty(dataValues.XpathsValues["OperationType"]) && dataValues.XpathsValues["OperationType"].Equals("10"))
            {
                template = template.Replace("{OperationType}", "Estándar");
            }
            else
            {
                template = template.Replace("{OperationType}", string.Empty);
            }
            
            
            if (!string.IsNullOrEmpty(dataValues.XpathsValues["PaymentWay"]))
            {
                template = template.Replace("{PaymentWay}", 
                    dataValues.XpathsValues["PaymentWay"].Equals("1") ? "Contado" : "Credito" );
            }
            else
            {
                template = template.Replace("{PaymentWay}", string.Empty);
            }

            if (!string.IsNullOrEmpty(dataValues.XpathsValues["PaymentMethod"]))
            {
                string paymentMethod = dataValues.XpathsValues["PaymentWay"];
                PaymentMethods enumPayment = (PaymentMethods)Enum.Parse(typeof(PaymentMethods), paymentMethod);
                if (enumPayment == PaymentMethods.Bonos)
                {
                    template = template.Replace("{PaymentMethod}", EnumHelper.GetDescription(enumPayment));
                }
                else
                {
                    template = template.Replace("{PaymentMethod}", "Otro");
                }
            }
            else
            {
                template = template.Replace("{PaymentMethod}", string.Empty);
            }

            template = template.Replace( "{ExpirationDate}", dataValues.XpathsValues["PaymentDueDate"]);
            template = template.Replace( "{Prefix}", dataValues.XpathsValues["Prefix"]);
            
            // Seller Data
            template = template.Replace( "{SellerNit}", dataValues.XpathsValues["SellerNit"]);
         
            template = template.Replace("{SellerBusinessName}", dataValues.XpathsValues["SellerBusinessName"]);

            template = template.Replace("{SellerDocumentNumber}", dataValues.XpathsValues["SellerDocumentNumber"]);

            if (!string.IsNullOrEmpty(dataValues.XpathsValues["SellerDocumentType"]))
            {
                string documentType = dataValues.XpathsValues["SellerDocumentType"];
                FiscalDocumentType enumDocument = (FiscalDocumentType)Enum.Parse(typeof(FiscalDocumentType), documentType);
                template = template.Replace("{SellerDocumentType}", EnumHelper.GetDescription(enumDocument));
               
            }
            else
            {
                template = template.Replace("{SellerDocumentType}", string.Empty);
            }

            template = template.Replace( "{SellerDocumentNumber}", string.Empty);

            if (dataValues.XpathsValues["SellerOrigin"] != null)
            {
                template = template.Replace("{SellerOrigin}",
                    dataValues.XpathsValues["SellerOrigin"].Equals("01") ? "Residente" : "No Residente");
            }
            else
            {
                template = template.Replace("{SellerOrigin}", string.Empty);
            }

            if (dataValues.XpathsValues["SellerTaxpayerType"] != null)
            {
                template = template.Replace("{SellerTaxpayerType}",
                    dataValues.XpathsValues["SellerTaxpayerType"].Equals("1") ? "Persona Jurídica y asimiladas" : "Persona Natural y asimiladas");
            }
            else
            {
                template = template.Replace("{SellerTaxpayerType}", string.Empty);
            }

            
            template = template.Replace( "{SellerResponsibilityType}", dataValues.XpathsValues["SellerResponsibilityType"]);
            template = template.Replace( "{SellerAddress}", dataValues.XpathsValues["SellerAddress"]);
            template = template.Replace( "{SellerState}", dataValues.XpathsValues["SellerState"]);
            template = template.Replace( "{SellerMunicipality}", dataValues.XpathsValues["SellerMunicipality"]);
            template = template.Replace( "{SellerEmail}", dataValues.XpathsValues["SellerEmail"]);
            template = template.Replace( "{SellerPhoneNumber}", dataValues.XpathsValues["SellerPhoneNumber"]);
            // Acquirer Data
            template = template.Replace( "{AcquirerNit}", dataValues.XpathsValues["AcquirerNit"]);
            template = template.Replace( "{AcquirerBusinessName}", dataValues.XpathsValues["AcquirerBusinessName"]);
            template = template.Replace("{AcquirerDocumentNumber}", dataValues.XpathsValues["AcquirerDocumentNumber"]);

            if (!string.IsNullOrEmpty(dataValues.XpathsValues["AcquirerDocumentType"]))
            {
                string documentType = dataValues.XpathsValues["AcquirerDocumentType"];
                FiscalDocumentType enumDocument = (FiscalDocumentType)Enum.Parse(typeof(FiscalDocumentType), documentType);
                template = template.Replace("{AcquirerDocumentType}", EnumHelper.GetDescription(enumDocument));

            }
            else
            {
                template = template.Replace("{AcquirerDocumentType}", string.Empty);
            }

            template = template.Replace(" {AcquirerDocumentNumber}", string.Empty);
            template = template.Replace( "{AcquirerTradeName}", dataValues.XpathsValues["AcquirerTradeName"]);


            //template = template.Replace( "{AcquirerTaxpayerType}", dataValues.XpathsValues["AcquirerTaxpayerType"]);
            if (dataValues.XpathsValues["AcquirerTaxpayerType"] != null)
            {
                template = template.Replace("{AcquirerTaxpayerType}",
                    dataValues.XpathsValues["AcquirerTaxpayerType"].Equals("1") ? "Persona Jurídica y asimiladas" : "Persona Natural y asimiladas");
            }
            else
            {
                template = template.Replace("{AcquirerTaxpayerType}", string.Empty);
            }

            template = template.Replace( "{AcquirerMainEconomicActivity}", dataValues.XpathsValues["AcquirerMainEconomicActivity"]);
            template = template.Replace( "{AcquirerResponsibilityType}", dataValues.XpathsValues["AcquirerResponsibilityType"]);
            template = template.Replace( "{AcquirerAddress}", dataValues.XpathsValues["AcquirerAddress"]);
            template = template.Replace( "{AcquirerState}", dataValues.XpathsValues["AcquirerState"]);
            template = template.Replace( "{AcquirerMunicipality}", dataValues.XpathsValues["AcquirerMunicipality"]);
            template = template.Replace( "{AcquirerEmail}", dataValues.XpathsValues["AcquirerEmail"]);
            template = template.Replace( "{AcquirerPhoneNumber}", dataValues.XpathsValues["AcquirerPhoneNumber"]);

            // Product Data drawing logic

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

            // Sumas
            double totalUnitPrice = SplitAndSum(dataValues.XpathsValues["TotalUnitPrice"]);
            double totalDiscountsDetail = SplitAndSum(dataValues.XpathsValues["TotalDiscountsDetail"]);
            double totalSurchargesDetail = SplitAndSum(dataValues.XpathsValues["TotalSurchargesDetail"]);

            // Total Unit Price Calculation
            totalUnitPrice += totalDiscountsDetail;
            totalUnitPrice -= totalSurchargesDetail;

            template = template.Replace( "{TotalUnitPrice}", $"{totalUnitPrice}");
            template = template.Replace("{TotalDiscountsDetail}", $"{totalDiscountsDetail}");
            template = template.Replace("{TotalSurchargesDetail}", $"{totalSurchargesDetail}");
            template = template.Replace("{TotalTaxableBase}", dataValues.XpathsValues["TotalTaxableBase"]);
            template = template.Replace("{TotalTaxesDetail}", SplitAndSum(dataValues.XpathsValues["TotalTaxesDetail"]).ToString());
            template = template.Replace("{TotalOtherTaxes}", SplitAndSum(dataValues.XpathsValues["TotalOtherTaxes"]).ToString());
            template = template.Replace("{TotalTaxes}", SplitAndSum(dataValues.XpathsValues["TotalTaxes"]).ToString());
            template = template.Replace( "{GlobalDiscounts}", dataValues.XpathsValues["GlobalDiscounts"]);
            template = template.Replace( "{GlobalSurcharges}", dataValues.XpathsValues["GlobalSurcharges"]);
            template = template.Replace( "{TotalAmount}", dataValues.XpathsValues["TotalAmount"]);
            // Final Data
            template = template.Replace( "{AuthorizationNumber}", dataValues.XpathsValues["AuthorizationNumber"]);
            template = template.Replace( "{AuthorizedRangeFrom}", dataValues.XpathsValues["AuthorizedRangeFrom"]);
            template = template.Replace( "{AuthorizedRangeTo}", dataValues.XpathsValues["AuthorizedRangeTo"]);
            template = template.Replace( "{ValidityDate}", dataValues.XpathsValues["ValidityDate"]);
            template = template.Replace( "{QRCode}", dataValues.XpathsValues["QrCode"]);

            return template;
        }

        #endregion

        #region SplitAndSum

        private double SplitAndSum(string concateField)
        {
            // TotalDiscountsDetail
            var aux = concateField.Split('|');
            double fieldValue = 0;

            foreach (var dataField in aux)
            {
                if (!string.IsNullOrEmpty(dataField))
                {
                    fieldValue += double.Parse(dataField, CultureInfo.InvariantCulture);
                }
            }
            return fieldValue;
        }

        #endregion

        #region MappingProducts

        private StringBuilder MappingProducts(byte[] xmlBytes, StringBuilder template)
        {
            string data = Encoding.UTF8.GetString(xmlBytes);
            StringBuilder productsTemplates = new StringBuilder();

            XElement xelement = XElement.Load(new StringReader(data));
            var nsm = new XmlNamespaceManager(new NameTable());

            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";

            var products = xelement.Elements(cac + "InvoiceLine");
            foreach (XElement product in products)
            {
                productsTemplates.Append("<tr>");

                productsTemplates.Append($"<td>{product.Element(cbc + "ID").Value}</td>");
                productsTemplates.Append($"<td>{product.Element(cac + "Item").Element(cac + "SellersItemIdentification").Element(cbc + "ID").Value}</td>");
                productsTemplates.Append($"<td>{product.Element(cac + "Item").Element(cbc + "Description").Value}</td>");
                productsTemplates.Append($"<td>{product.Element(cbc + "InvoicedQuantity").Attribute("unitCode").Value}</td>");
                productsTemplates.Append($"<td>{product.Element(cbc + "InvoicedQuantity").Value}</td>");
                productsTemplates.Append($"<td>{product.Element(cbc + "LineExtensionAmount").Value}</td>");

                // Discounts and surcharges
                if (product.Element(cac + "AllowanceCharge") != null)
                {
                    if (!Convert.ToBoolean(product.Element(cac + "AllowanceCharge").Element(cbc + "ChargeIndicator").Value))
                    {
                        productsTemplates.Append($"<td>{product.Element(cac + "AllowanceCharge").Element(cbc + "Amount").Value}</td>");
                        productsTemplates.Append("<td></td>");
                    }
                    else
                    {
                        productsTemplates.Append("<td></td>");
                        productsTemplates.Append($"<td>{product.Element(cac + "AllowanceCharge").Element(cbc + "Amount").Value}</td>");
                    }
                }
                else
                {
                    productsTemplates.Append("<td></td>");
                    productsTemplates.Append("<td></td>");
                }

                if (product.Element(cac + "TaxTotal") != null && product.Element(cac + "TaxTotal").Element(cbc + "TaxAmount") != null)
                {
                    productsTemplates.Append($"<td>{product.Element(cac + "TaxTotal").Element(cbc + "TaxAmount").Value}</td>");
                }
                else
                {
                    productsTemplates.Append("<td></td>");
                }

                productsTemplates.Append($"<td>{product.Element(cbc + "LineExtensionAmount").Value}</td>");

                productsTemplates.Append("</tr>");
            }

            template = template.Replace("{ProductDetails}", productsTemplates.ToString());

            return template;
        }

        #endregion

        #region MappingDiscounts

        private StringBuilder MappingDiscounts(byte[] xmlBytes, StringBuilder template)
        {
            string data = Encoding.UTF8.GetString(xmlBytes);
            StringBuilder discountsTemplates = new StringBuilder();

            XElement xelement = XElement.Load(new StringReader(data));
            var nsm = new XmlNamespaceManager(new NameTable());

            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";

            var details = xelement.Elements(cac + "AllowanceCharge");
            foreach (XElement detail in details)
            {
                discountsTemplates.Append("<tr>");

                discountsTemplates.Append($"<td>{detail.Element(cbc + "ID").Value}</td>");

                if (!Convert.ToBoolean(detail.Element(cbc + "ChargeIndicator").Value))
                {
                    discountsTemplates.Append($"<td>Descuento</td>");
                }
                else
                {
                    discountsTemplates.Append("<td>Recargo</td>");
                }
                discountsTemplates.Append($"<td>{(detail.Element(cbc + "AllowanceChargeReasonCode") != null?  detail.Element(cbc + "AllowanceChargeReasonCode").Value : string.Empty)}</td>");
                discountsTemplates.Append($"<td>{detail.Element(cbc + "AllowanceChargeReason").Value}</td>");
                discountsTemplates.Append($"<td>{detail.Element(cbc + "MultiplierFactorNumeric").Value}</td>");
                discountsTemplates.Append($"<td>{detail.Element(cbc + "Amount").Value}</td>");

                discountsTemplates.Append("</tr>");
            }

            template = template.Replace("{DiscountDetails}", discountsTemplates.ToString());

            return template;
        }

        #endregion

        #region MappingAdvances

        private StringBuilder MappingAdvances(byte[] xmlBytes, StringBuilder template)
        {
            string data = Encoding.UTF8.GetString(xmlBytes);
            XmlDocument invoiceDoc = new XmlDocument();
            int counter = 1;

            invoiceDoc.LoadXml(data);
            XmlNodeList advanceNodes = invoiceDoc.GetElementsByTagName("cac:PrepaidPayment");

            // Product Data mapping logic
            StringBuilder advances = new StringBuilder();

            foreach (XmlNode element in advanceNodes)
            {
                advances.Append("<tr>");
                advances.Append($"<td class='text-centered'>{counter}</td>");
                advances.Append($"<td class='text-currency'>$ {element["cbc:PaidAmount"].InnerText}</td>");
                advances.Append("</tr>");

                counter++;
            }

            template.Replace("{TotalAdvances}", advances.ToString());

            return template;
        }

        #endregion

        #region MappingRetentions

        private StringBuilder MappingRetentions(byte[] xmlBytes, StringBuilder template)
        {
            string data = Encoding.UTF8.GetString(xmlBytes);
            XmlDocument invoiceDoc = new XmlDocument();
            int counter = 1;

            invoiceDoc.LoadXml(data);
            XmlNodeList retentionsNodes = invoiceDoc.GetElementsByTagName("cac:WithholdingTaxTotal");

            // Product Data mapping logic
            StringBuilder retentions = new StringBuilder();

            foreach (XmlNode element in retentionsNodes)
            {
                retentions.Append("<tr>");
                retentions.Append($"<td class='text-centered'>{counter}</td>");
                retentions.Append($"<td class='text-currency'>$ {element["cbc:TaxAmount"].InnerText}</td>");
                retentions.Append("</tr>");
                counter++;
            }

            template.Replace("{TotalRetentions}", retentions.ToString());

            return template;
        }

        #endregion

        #region
        string getUtfEncode(string text)
        {
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(text);
            byte[] win1252Bytes = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("Windows-1252"), utf8Bytes);
            string sConvertedString = Encoding.UTF8.GetString(win1252Bytes);
            return sConvertedString;
        }
        #endregion

    }
}
