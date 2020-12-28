using Gosocket.Dian.Application.Cosmos;
using Gosocket.Dian.Domain.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;
using Gosocket.Dian.Services.Utils.Helpers;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Application
{
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
            var xmlBytes = await _fileManager.GetBytesAsync("radian-documents-templates", "XML Documento Soporte - Invoice 17-11-2020");

            // Load xpaths
            var xpathRequest = CreateGetXpathDataValuesRequestObject(Convert.ToBase64String(xmlBytes), "RepresentacionGrafica");
            var fieldValue = ApiHelpers.ExecuteRequest<ResponseXpathDataValue>(ConfigurationManager.GetValue("SupportDocumentNumber"), xpathRequest);

            // Mapping Fields

            template = template.Replace("SupportDocumentNumber", fieldValue.XpathsValues.Values.ToString());

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
                { "EmissionDateXpath", "//*[local-name()='IssueDate']" },
                { "SenderCodeXpath", "//*[local-name()='AccountingSupplierParty']/*[local-name()='Party']/*[local-name()='PartyTaxScheme']/*[local-name()='CompanyID']" },
                { "ReceiverCodeXpath", "//*[local-name()='AccountingCustomerParty']/*[local-name()='Party']/*[local-name()='PartyTaxScheme']/*[local-name()='CompanyID']" },
                { "DocumentTypeXpath", "//*[local-name()='InvoiceTypeCode']" },
                { "NumberXpath", "/*[local-name()='Invoice']/*[local-name()='ID']|/*[local-name()='CreditNote']/*[local-name()='ID']|/*[local-name()='DebitNote']/*[local-name()='ID']" },
                { "SeriesXpath", "//*[local-name()='InvoiceControl']/*[local-name()='AuthorizedInvoices']/*[local-name()='Prefix']"},
                { "DocumentKeyXpath","//*[local-name()='Invoice']/*[local-name()='UUID']|//*[local-name()='CreditNote']/*[local-name()='UUID']|//*[local-name()='DebitNote']/*[local-name()='UUID']"},
                { "AdditionalAccountIdXpath","//*[local-name()='AccountingCustomerParty']/*[local-name()='AdditionalAccountID']"},
                { "PartyIdentificationSchemeIdXpath","//*[local-name()='AccountingCustomerParty']/*[local-name()='Party']/*[local-name()='PartyTaxScheme']/*[local-name()='CompanyID']/@schemeID|/*[local-name()='Invoice']/*[local-name()='AccountingSupplierParty']/*[local-name()='Party']/*[local-name()='PartyIdentification']/*[local-name()='ID']/@schemeID"},
                { "DocumentReferenceKeyXpath","//*[local-name()='BillingReference']/*[local-name()='InvoiceDocumentReference']/*[local-name()='UUID']"},
                { "DocumentTypeId", "" },
                { "SoftwareIdXpath", "//sts:SoftwareID" },

                //ApplicationResponse
                { "AppResReceiverCodeXpath", "//*[local-name()='ApplicationResponse']/*[local-name()='ReceiverParty']/*[local-name()='PartyTaxScheme']/*[local-name()='CompanyID']" },
                { "AppResSenderCodeXpath", "//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyTaxScheme']/*[local-name()='CompanyID']" },
                { "AppResProviderIdXpath", "//*[local-name()='ApplicationResponse']/*[local-name()='UBLExtensions']/*[local-name()='UBLExtension']/*[local-name()='ExtensionContent']/*[local-name()='DianExtensions']/*[local-name()='SoftwareProvider']/*[local-name()='ProviderID']" },
                { "AppResEventCodeXpath", "//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='Response']/*[local-name()='ResponseCode']" },
                { "AppResDocumentTypeXpath", "//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='Response']/*[local-name()='ResponseCode']" },
                { "AppResNumberXpath", "//*[local-name()='ApplicationResponse']/*[local-name()='ID']" },
                { "AppResSeriesXpath", "//*[local-name()='ApplicationResponse']/*[local-name()='ID']"},
                { "AppResDocumentKeyXpath","//*[local-name()='ApplicationResponse']/*[local-name()='UUID']"},
                { "AppResDocumentReferenceKeyXpath","//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='DocumentReference']/*[local-name()='UUID']"},
                { "AppResCustomizationIDXpath","//*[local-name()='ApplicationResponse']/*[local-name()='CustomizationID']"},

            };

            return requestObj;
        } 

        #endregion

    }
}
