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
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Plugin.Functions.SigningTime;
using Gosocket.Dian.Plugin.Functions.Event;
using static Gosocket.Dian.Domain.Common.EnumHelper;
using static Gosocket.Dian.Plugin.Functions.EventApproveCufe.EventApproveCufe;
using Gosocket.Dian.Plugin.Functions.Predecesor;
using Gosocket.Dian.Plugin.Functions.Cufe;
using Gosocket.Dian.Plugin.Functions.Series;

namespace Gosocket.Dian.Plugin.Functions.Common
{
    public class ValidatorEngine
    {
        #region Global properties
        private static readonly TableManager tableManagerGlobalLogger = new TableManager("GlobalLogger");
        static readonly TableManager documentMetaTableManager = new TableManager("GlobalDocValidatorDocumentMeta");
        static readonly TableManager documentAttorneyTableManager = new TableManager("GlobalDocReferenceAttorney");
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
        public async Task<List<ValidateListResponse>> StartValidateEmitionEventPrevAsync(ValidateEmitionEventPrev.RequestObject eventPrev)
        {
            var validateResponses = new List<ValidateListResponse>();
            var nitModel = new NitModel();
            XmlParser xmlParserCufe = null;
            XmlParser xmlParserCude = null;

            //Anulacion de endoso electronico obtiene CUFE referenciado en el CUDE emitido
            if (Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.InvoiceOfferedForNegotiation || 
                Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.AnulacionLimitacionCirculacion)
            {
                var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(eventPrev.TrackId, eventPrev.TrackId);
                if (documentMeta != null)
                {
                    //Obtiene el CUFE
                    eventPrev.TrackId = documentMeta.DocumentReferencedKey;
                }
            }
            //Obtiene información factura referenciada Endoso electronico, Solicitud Disponibilización AR CUDE
            if (Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.SolicitudDisponibilizacion || Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.EndosoGarantia 
                || Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.EndosoPropiedad || Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.EndosoProcuracion
                || Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.Avales)
            {
                //Obtiene XML Factura electronica CUFE
                var xmlBytes = await GetXmlFromStorageAsync(eventPrev.TrackId);
                xmlParserCufe = new XmlParser(xmlBytes);
                if (!xmlParserCufe.Parser())
                    throw new Exception(xmlParserCufe.ParserError);

                //Obtiene XML ApplicationResponse CUDE
                var xmlBytesCude = await GetXmlFromStorageAsync(eventPrev.TrackIdCude);
                xmlParserCude = new XmlParser(xmlBytesCude);
                if (!xmlParserCude.Parser())
                    throw new Exception(xmlParserCude.ParserError);

                nitModel = xmlParserCude.Fields.ToObject<NitModel>();

                //Si el endoso esta en blanco o el senderCode es diferente a providerCode                
                nitModel.SenderCode = (nitModel.listID == "2" || (nitModel.SenderCode != nitModel.ProviderCode)) ? nitModel.ProviderCode : nitModel.SenderCode;
            }

            var validator = new Validator();           
            validateResponses.AddRange(validator.ValidateEmitionEventPrev(eventPrev, xmlParserCufe, xmlParserCude, nitModel));

            return validateResponses;

        }


        public List<ValidateListResponse> StartValidateDocumentReference(ValidateDocumentReference.RequestObject validateReference)
        {
            var validateResponses = new List<ValidateListResponse>();
            var validator = new Validator();
            DateTime startDate = DateTime.UtcNow;


            validateResponses.AddRange(validator.ValidateDocumentReferencePrev(validateReference.TrackId,
                validateReference.IdDocumentReference, validateReference.EventCode, validateReference.DocumentTypeIdRef,
                validateReference.IssuerPartyCode, validateReference.IssuerPartyName));

            return validateResponses;
        }


        public async Task<List<ValidateListResponse>> StartValidateSigningTimeAsync(ValidateSigningTime.RequestObject data)
        {           
            var validateResponses = new List<ValidateListResponse>();
            DateTime startDate = DateTime.UtcNow;
            EventStatus code;
            switch (int.Parse(data.EventCode))
            {
                case (int)EventStatus.Receipt:
                    code = EventStatus.Received; 
                    break;              
                case (int)EventStatus.SolicitudDisponibilizacion:
                    code = EventStatus.Accepted;
                    break;              
                case (int)EventStatus.NotificacionPagoTotalParcial:
                case (int)EventStatus.NegotiatedInvoice:
                    code = EventStatus.SolicitudDisponibilizacion;
                    break;
                case (int)EventStatus.Avales:
                case (int)EventStatus.ValInfoPago:
                    code = EventStatus.SolicitudDisponibilizacion;
                    break;
                default:
                    code = EventStatus.Receipt;
                    break;
            }

            if (Convert.ToInt32(data.EventCode) == (int)EventStatus.Rejected ||
                Convert.ToInt32(data.EventCode) == (int)EventStatus.Receipt ||
                Convert.ToInt32(data.EventCode) == (int)EventStatus.Accepted ||
                Convert.ToInt32(data.EventCode) == (int)EventStatus.AceptacionTacita ||                
                Convert.ToInt32(data.EventCode) == (int)EventStatus.SolicitudDisponibilizacion ||                
                Convert.ToInt32(data.EventCode) == (int)EventStatus.Avales ||
                Convert.ToInt32(data.EventCode) == (int)EventStatus.NotificacionPagoTotalParcial ||
                Convert.ToInt32(data.EventCode) == (int)EventStatus.ValInfoPago
                )
            {
                var documentMeta = documentMetaTableManager.FindDocumentReferenced_EventCode_TypeId<GlobalDocValidatorDocumentMeta>(data.TrackId.ToLower(), data.DocumentTypeId,
                    "0"+ (int)code).FirstOrDefault();                
                if (documentMeta != null)
                {
                    data.TrackId = documentMeta.PartitionKey;
                }
                //No encuentra información del Recibo del bien para Aceptación Expresa y Tácita
                else if (documentMeta == null && (Convert.ToInt32(data.EventCode) == (int)EventStatus.Accepted ||
                Convert.ToInt32(data.EventCode) == (int)EventStatus.AceptacionTacita ||
                Convert.ToInt32(data.EventCode) == (int)EventStatus.Rejected))
                {
                    ValidateListResponse response = new ValidateListResponse();
                    response.ErrorMessage = $"No se encontró evento referenciado CUDE Recibo del Bien para evaluar fecha.";
                    response.IsValid = false;
                    response.ErrorCode = "89";
                    response.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
                    validateResponses.Add(response);
                    return validateResponses;
                }
                // Validación de la Sección Signature - Fechas valida transmisión evento Solicitud Disponibilizacion
                else if (Convert.ToInt32(data.EventCode) == (int)EventStatus.SolicitudDisponibilizacion)
                {
                    code = EventStatus.AceptacionTacita; 
                    documentMeta = documentMetaTableManager.FindDocumentReferenced_EventCode_TypeId<GlobalDocValidatorDocumentMeta>(data.TrackId.ToLower(), data.DocumentTypeId, 
                        "0" + (int)code).FirstOrDefault();
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
                else if (Convert.ToInt32(data.EventCode) != (int)EventStatus.Rejected)
                {
                    ValidateListResponse response = new ValidateListResponse();
                    response.ErrorMessage = $"No se encontró evento referenciado CUDE y/o CUFE para evaluar fecha.";
                    response.IsValid = false;
                    response.ErrorCode = "89";
                    response.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
                    validateResponses.Add(response);
                    return validateResponses;
                }
                else if (documentMeta == null)
                {                    
                    ValidateListResponse response = new ValidateListResponse();
                    response.ErrorMessage = "No se encontró evento referenciado para evaluar fecha";
                    response.IsValid = false;
                    response.ErrorCode = "89";
                    response.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
                    validateResponses.Add(response);
                    return validateResponses;
                }
            }           
            else if(Convert.ToInt32(data.EventCode) == (int)EventStatus.NegotiatedInvoice)                   
            {
                var documentMeta = documentMetaTableManager.FindDocumentReferenced_EventCode_TypeId_CustomizationID<GlobalDocValidatorDocumentMeta>(data.TrackId.ToLower(),
                    data.DocumentTypeId, "0" + (int)code, "361", "362").FirstOrDefault();
                if (documentMeta != null)
                {
                    data.TrackId = documentMeta.PartitionKey;
                }
                else
                {
                    string msg = "No se encontró evento referenciado CUDE para evaluar fecha Limitación de circulación";           
                    ValidateListResponse response = new ValidateListResponse();
                    response.ErrorMessage = msg;
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

            var nitModel = xmlParser.Fields.ToObject<NitModel>();
            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateSigningTime(data, xmlParser, nitModel));

            return validateResponses;
        }

        public async Task<List<ValidateListResponse>> StartValidatePredecesor(RequestObjectPredecesor p)
        {
            var validateResponses = new List<ValidateListResponse>();
            DateTime startDate = DateTime.UtcNow;

            var xmlBytes = await GetXmlFromStorageAsync(p.TrackId);
            var xmlParser = new XmlParseNomina(xmlBytes);
            if (!xmlParser.Parser())
                throw new Exception(xmlParser.ParserError);

            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateReplacePredecesor(p, xmlParser));
            return validateResponses;
        }

        public List<ValidateListResponse> StartValidateSerieAndNumberAsync(ValidateSerieAndNumber.RequestObject data)
        {
            var validateResponses = new List<ValidateListResponse>();

            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateSerieAndNumber(data));
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

        public async Task<List<ValidateListResponse>> StartValidateCune(RequestObjectCune cune)
        {
            var validateResponses = new List<ValidateListResponse>();

            var xmlBytes = await GetXmlFromStorageAsync(cune.trackId);
            var xmlParser = new XmlParseNomina(xmlBytes);
            if (!xmlParser.Parser())
                throw new Exception(xmlParser.ParserError);
            CuneModel cmObject = new CuneModel();
            cmObject.Cune = xmlParser.globalDocPayrolls.CUNE;
            cmObject.NumNIE = xmlParser.globalDocPayrolls.Numero;

            cmObject.FecNIE = xmlParser.globalDocPayrolls.FechaGen.ToString("yyyy-MM-dd");
            cmObject.HorNIE = xmlParser.globalDocPayrolls.HoraGen;
            cmObject.SoftwareId = xmlParser.globalDocPayrolls.SoftwareID;
            cmObject.ValDesc = Convert.ToString(xmlParser.globalDocPayrolls.deduccionesTotal);
            cmObject.ValTol = Convert.ToString(xmlParser.globalDocPayrolls.comprobanteTotal);
            cmObject.ValDev = Convert.ToString(xmlParser.globalDocPayrolls.devengadosTotal);
            cmObject.NitNIE = Convert.ToString(xmlParser.globalDocPayrolls.Emp_NIT);
            cmObject.DocEmp = Convert.ToString(xmlParser.globalDocPayrolls.NumeroDocumento);
            cmObject.TipAmb = Convert.ToString(xmlParser.globalDocPayrolls.Ambiente);

            // Validator instance
            var validator = new Validator();
            validateResponses.Add(validator.ValidateCune(cmObject, cune));
            return validateResponses;
        }

        public async Task<List<ValidateListResponse>> StartValidateParty(RequestObjectParty party)
        {
            DateTime startDate = DateTime.UtcNow;
            var validateResponses = new List<ValidateListResponse>();
            XmlParser xmlParserCufe = null;
            XmlParser xmlParserCude = null;

            //Anulacion de endoso electronico, TerminacionLimitacion de Circulacion obtiene CUFE referenciado en el CUDE emitido
            if (Convert.ToInt32(party.ResponseCode) == (int)EventStatus.InvoiceOfferedForNegotiation ||
                Convert.ToInt32(party.ResponseCode) == (int)EventStatus.AnulacionLimitacionCirculacion)
            {
                var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(party.TrackId, party.TrackId);
                if (documentMeta != null)
                {
                    //Obtiene el CUFE
                    party.TrackId = documentMeta.DocumentReferencedKey;
                }
            }

            //Obtiene XML Factura Electornica CUFE
            var xmlBytes = await GetXmlFromStorageAsync(party.TrackId);
            xmlParserCufe = new XmlParser(xmlBytes);
            if (!xmlParserCufe.Parser())
                throw new Exception(xmlParserCufe.ParserError);

            //Obtiene XML ApplicationResponse CUDE
            var xmlBytesCude = await GetXmlFromStorageAsync(party.TrackIdCude.ToLower());
            xmlParserCude = new XmlParser(xmlBytesCude);
            if (!xmlParserCude.Parser())
                throw new Exception(xmlParserCude.ParserError);

            var nitModel = xmlParserCufe.Fields.ToObject<NitModel>();
            bool valid = true;
            if (Convert.ToInt32(party.ResponseCode) == (int)EventStatus.SolicitudDisponibilizacion 
                || Convert.ToInt32(party.ResponseCode) == (int)EventStatus.EndosoPropiedad)
            {
                GlobalDocHolderExchange documentHolderExchange = documentMetaTableManager.FindhByCufeExchange<GlobalDocHolderExchange>(party.TrackId.ToLower(), true);
                if (documentHolderExchange != null)
                {
                    string[] endosatarios = documentHolderExchange.PartyLegalEntity.Split('|');
                    if(endosatarios.Length == 1)
                    {
                        nitModel.SenderCode = documentHolderExchange.PartyLegalEntity;
                    }
                    else
                    {
                        foreach(string endosatario in endosatarios)
                        {
                            GlobalDocReferenceAttorney documentAttorney = documentAttorneyTableManager.FindhByCufeSenderAttorney<GlobalDocReferenceAttorney>(party.TrackId.ToLower(), party.SenderParty, endosatario);
                            if(documentAttorney == null)
                            {
                                valid = false;
                            }
                        }
                        if(valid)
                        {
                            nitModel.SenderCode = party.SenderParty;
                        }
                    }
                }
            }
            if(valid)
            {
                var validator = new Validator();
                validateResponses.AddRange(validator.ValidateParty(nitModel, party, xmlParserCude));
            }
            else
            {
                validateResponses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "089",
                    ErrorMessage = "Mandatario no encontrado",
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });

            }
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
                response.ErrorMessage = $"Tipo factura diferente a Crédito.";
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
        public async Task<byte[]> GetXmlPayrollDocumentAsync(string file)
        {
            var fileManager = new FileManager();
            var container = "global";
            var fileName = $"schemes/schemes/new-nomina-1.0/01/{file}.xml";
            var xmlBytes = await fileManager.GetBytesAsync(container, fileName);

            return xmlBytes;
        }
        #endregion
    }
}