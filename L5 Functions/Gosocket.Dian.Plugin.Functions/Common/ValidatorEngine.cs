using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Plugin.Functions.Models;
using Gosocket.Dian.Plugin.Functions.ValidateParty;
using Gosocket.Dian.Services.Utils;
using Gosocket.Dian.Services.Utils.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Gosocket.Dian.Plugin.Functions.SigningTime;
using Gosocket.Dian.Plugin.Functions.Event;
using static Gosocket.Dian.Plugin.Functions.EventApproveCufe.EventApproveCufe;

namespace Gosocket.Dian.Plugin.Functions.Common
{
    public class ValidatorEngine
    {
        #region Global properties
        private static readonly TableManager tableManagerGlobalLogger = new TableManager("GlobalLogger");
        static readonly TableManager documentMetaTableManager = new TableManager("GlobalDocValidatorDocumentMeta");
        #endregion

        public ValidatorEngine() { }

        private static ValidatorEngine _instance = null;

        public static ValidatorEngine Instance => _instance ?? (_instance = new ValidatorEngine());

        public async Task<List<ValidateListResponse>> StartContingencyValidationAsync(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();

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

            var xmlBytes = await GetXmlFromStorageAsync(trackId);
            var xmlParser = new XmlParser(xmlBytes);
            if (!xmlParser.Parser())
                throw new Exception(xmlParser.ParserError);

            var cufeModel = xmlParser.Fields.ToObject<CufeModel>();

            // Validator instance
            var validator = new Validator();
            validateResponses.Add(validator.ValidateCufe(cufeModel, trackId));

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
        public List<ValidateListResponse> StartValidateEmitionEventPrevAsync(ValidateEmitionEventPrev.RequestObject eventPrev)
        {
            //Anulacion de endoso electronico obtiene CUFE referenciado en el CUDE emitido
            if (eventPrev.EventCode == "040")
            {
                var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(eventPrev.TrackId, eventPrev.TrackId);
                if (documentMeta != null)
                {
                    //Obtiene el CUFE
                    eventPrev.TrackId = documentMeta.DocumentReferencedKey;
                }
            }            

            var validator = new Validator();
            return validator.ValidateEmitionEventPrev(eventPrev);
        }
        public List<ValidateListResponse> StartValidateDocumentReferenceAsync(string trackId, string idDocumentReference)
        {
            var validator = new Validator();
            return validator.ValidateDocumentReferencePrev(trackId, idDocumentReference);
        }
        public async Task<List<ValidateListResponse>> StartValidateSigningTimeAsync(ValidateSigningTime.RequestObject data)
        {           
            var validateResponses = new List<ValidateListResponse>();
            DateTime startDate = DateTime.UtcNow;
            string code;
            switch (data.EventCode)
            {
                case "032": //Constancia de recibo del bien
                    code = "030"; // Acuse de recibo de la FEV
                    break;
                case "044":  //Terminacion del mandato
                    code = "043"; //Mandato
                    break;
                case "036": //Solicitud de Dsiponibilizacion 
                    code = "033"; //Aceptacion Expresa
                    break;
                default:
                    code = "032"; //Constancia de recibo del bien
                    break;
            }

            if (data.EventCode == "031" || data.EventCode == "032" || data.EventCode == "033" || data.EventCode == "034" || data.EventCode == "044" || data.EventCode == "036")
            {
                var documentMeta = documentMetaTableManager.FindDocumentReferenced_EventCode_TypeId<GlobalDocValidatorDocumentMeta>(data.TrackId.ToLower(), data.DocumentTypeId, code).FirstOrDefault();
                if (documentMeta != null)
                {
                    data.TrackId = documentMeta.PartitionKey;
                }
                // Validación de la Sección Signature - Fechas valida transmisión evento TASK 714
                else if (data.EventCode == "036")
                {
                    code = "034"; //Aceptacion Tácita
                    documentMeta = documentMetaTableManager.FindDocumentReferenced_EventCode_TypeId<GlobalDocValidatorDocumentMeta>(data.TrackId.ToLower(), data.DocumentTypeId, code).FirstOrDefault();
                    if (documentMeta != null)
                    {
                        data.TrackId = documentMeta.PartitionKey;
                    }
                    else
                    {
                        ValidateListResponse response = new ValidateListResponse();
                        response.ErrorMessage = $"No se encuentran registros de Eventos de Aceptación Expresa - Tácita  prerrequisito para esta transmisión de Disponibilizacion";
                        response.IsValid = false;
                        response.ErrorCode = "89";
                        response.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
                        validateResponses.Add(response);
                        return validateResponses;
                    }
                }
                //Solo si es eventcode AR Aceptacion Expresa - Tácita
                else if (data.EventCode != "031")
                {
                    ValidateListResponse response = new ValidateListResponse();
                    response.ErrorMessage = $"No se encontró documento electrónico para el CUDE/CUFE {data.TrackId}";
                    response.IsValid = false;
                    response.ErrorCode = "89";
                    response.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
                    validateResponses.Add(response);
                    return validateResponses;
                }
            }
            
            var xmlBytes = await GetXmlFromStorageAsync(data.TrackId);
            var xmlParser = new XmlParser(xmlBytes);
            if (!xmlParser.Parser())
                throw new Exception(xmlParser.ParserError);
        
            //DateTime dateReceived = DateTime.ParseExact(xmlParser.SigningTime, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            //DateTime dateEntrie = DateTime.ParseExact(signingTime, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            string dateEntrie = Convert.ToDateTime(data.SigningTime).ToString("dd/MM/yyyy");
            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateSigningTime(data, xmlParser));

            return validateResponses;
        }
        public List<ValidateListResponse> StartValidateSerieAndNumberAsync(string trackId, string number, string documentTypeId)
        {
            var validateResponses = new List<ValidateListResponse>();

            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateSerieAndNumber(trackId, number, documentTypeId));
            return validateResponses;
        }
        public async Task<List<ValidateListResponse>> StartNitValidationAsync(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();

            var xmlBytes = await GetXmlFromStorageAsync(trackId);
            var xmlParser = new XmlParser(xmlBytes);
            if (!xmlParser.Parser())
                throw new Exception(xmlParser.ParserError);

            var nitModel = xmlParser.Fields.ToObject<NitModel>();

            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateNit(nitModel, trackId));

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

        public async Task<List<ValidateListResponse>> StartValidateParty(RequestObjectParty party)
        {
            var validateResponses = new List<ValidateListResponse>();

            var xmlBytes = await GetXmlFromStorageAsync(party.TrackId);
            var xmlParser = new XmlParser(xmlBytes);
            if (!xmlParser.Parser())
                throw new Exception(xmlParser.ParserError);

            var nitModel = xmlParser.Fields.ToObject<NitModel>();

            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateParty(nitModel, party));

            return validateResponses;
        }

        public async Task<List<ValidateListResponse>> StartEventApproveCufe(EventApproveCufeObjectParty eventApproveCufe)
        {
            DateTime startDate = DateTime.UtcNow;
            var validateResponses = new List<ValidateListResponse>();

            var xmlBytes = await GetXmlFromStorageAsync(eventApproveCufe.TrackId);
            var xmlParser = new XmlParser(xmlBytes);
            if (!xmlParser.Parser())
                throw new Exception(xmlParser.ParserError);

            var nitModel = xmlParser.Fields.ToObject<NitModel>();

            if(xmlParser.PaymentMeansID != "2")
            {
                ValidateListResponse response = new ValidateListResponse();
                response.ErrorMessage = $"Tipo factura diferente a Credito.";
                response.IsValid = false;
                response.ErrorCode = "89";
                response.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
                validateResponses.Add(response);
                return validateResponses;
            }

            var validator = new Validator();
            validateResponses.AddRange(validator.EventApproveCufe(nitModel, eventApproveCufe));

            return validateResponses;
        }

        public async Task<List<ValidateListResponse>> StartNumberingRangeValidationAsync(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();

            var xmlBytes = await GetXmlFromStorageAsync(trackId);
            var xmlParser = new XmlParser(xmlBytes);
            if (!xmlParser.Parser())
                throw new Exception(xmlParser.ParserError);

            var numberRangeModel = xmlParser.Fields.ToObject<NumberRangeModel>();

            // Validator instance
            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateNumberingRange(numberRangeModel, trackId));

            return validateResponses;
        }

        public async Task<List<ValidateListResponse>> StartSignValidationAsync(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();

            var xmlBytes = await GetXmlFromStorageAsync(trackId);
            if (xmlBytes == null) throw new Exception("Xml not found.");

            // Validator instance
            var validator = new Validator(xmlBytes);

            //
            validateResponses.AddRange(validator.ValidateSignXades());

            // Get all crls
            var crls = Application.Managers.CertificateManager.Instance.GetCrls();

            // Get all crt certificates
            var crts = Application.Managers.CertificateManager.Instance.GetRootCertificates();

            validateResponses.AddRange(validator.ValidateSign(crts, crls));

            return validateResponses;
        }

        public async Task<List<ValidateListResponse>> StartSoftwareValidationAsync(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();

            var xmlBytes = await GetXmlFromStorageAsync(trackId);
            var xmlParser = new XmlParser(xmlBytes);
            if (!xmlParser.Parser())
                throw new Exception(xmlParser.ParserError);

            var softwareModel = xmlParser.Fields.ToObject<SoftwareModel>();

            var validator = new Validator();
            validateResponses.Add(validator.ValidateSoftware(softwareModel, trackId));

            return validateResponses;
        }

        public async Task<List<ValidateListResponse>> StartTaxLevelCodesValidationAsync(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();

            var dictionary = CreateTaxLevelCodeXpathsRequestObject(trackId);

            var xmlBytes = await GetXmlFromStorageAsync(trackId);
            if (xmlBytes == null) throw new Exception("Xml not found.");

            // Validator instance
            var validator = new Validator(xmlBytes);

            validateResponses.AddRange(validator.ValidateTaxLevelCodes(dictionary));

            return validateResponses;
        }

        public async Task<List<ValidateListResponse>> StartValidateReferenceAttorney(ValidateReferenceAttorney.RequestObjectReferenceAttorney data)
        {
            var validateResponses = new List<ValidateListResponse>();

            var xmlBytes = await GetXmlFromStorageAsync(data.TrackId);
            var xmlParser = new XmlParser(xmlBytes);
            if (!xmlParser.Parser())
                throw new Exception(xmlParser.ParserError);
            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateReferenceAttorney(xmlParser, data.TrackId));

            return validateResponses;
        }


        #region Private methods
        private Dictionary<string, string> CreateTaxLevelCodeXpathsRequestObject(string trackId)
        {
            var dictionary = new Dictionary<string, string>
            {
                    { "SenderTaxLevelCodes", "/sig:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:TaxLevelCode"},
                    { "AdditionalAccountIds", "//cac:AccountingCustomerParty/cbc:AdditionalAccountID" },
                    { "ReceiverTaxLevelCodes", "/sig:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cbc:TaxLevelCode"},
                    { "DeliveryTaxLevelCodes", "/sig:Invoice/cac:Delivery/cac:DeliveryParty/cac:PartyTaxScheme/cbc:TaxLevelCode" },
                    { "SheldHolderTaxLevelCodes", "/sig:Invoice/cac:Delivery/cac:DeliveryParty/cac:PartyTaxScheme/cbc:TaxLevelCode" }
            };
            return dictionary;
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