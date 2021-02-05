using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Plugin.Functions.Models;
using Gosocket.Dian.Plugin.Functions.ValidateParty;
using Gosocket.Dian.Plugin.Functions.Event;
using Gosocket.Dian.Services.Utils;
using Gosocket.Dian.Services.Utils.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Plugin.Functions.SigningTime;
using Gosocket.Dian.Plugin.Functions.EventApproveCufe;
using Gosocket.Dian.Plugin.Functions.Predecesor;
using Gosocket.Dian.Plugin.Functions.Cufe;
using Gosocket.Dian.Plugin.Functions.ValidateReferenceAttorney;


namespace Gosocket.Dian.Plugin.Functions.Common
{
    public class ValidatorEngine
    {
        #region Global properties
        private static readonly TableManager tableManagerGlobalLogger = new TableManager("GlobalLogger");
        static readonly TableManager documentMetaTableManager = new TableManager("GlobalDocValidatorDocumentMeta");
        static readonly TableManager documentAttorneyTableManager = new TableManager("GlobalDocReferenceAttorney");
        static readonly TableManager documentHolderExchangeTableManager = new TableManager("GlobalDocHolderExchange");

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

        public async Task<List<ValidateListResponse>> StartValidationEventRadianAsync(string trackId)
        {
            var validator = new Validator();
            DateTime startDate = DateTime.UtcNow;
            var validateResponses = new List<ValidateListResponse>();
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            RequestObjectEventApproveCufe eventApproveCufe = new RequestObjectEventApproveCufe();
            RequestObjectDocReference docReference = new RequestObjectDocReference();
            RequestObjectParty requestParty = new RequestObjectParty();
            RequestObjectEventPrev eventPrev = new RequestObjectEventPrev();
            RequestObjectSigningTime signingTime = new RequestObjectSigningTime();
            RequestObjectReferenceAttorney referenceAttorney = new RequestObjectReferenceAttorney();

            GlobalDocValidatorDocumentMeta documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
            if (documentMeta != null)
            {
                NitModel nitModel = new NitModel();
                EventRadianModel eventRadian = new EventRadianModel();
                bool validEventRadian = true;
                var xmlBytes = await GetXmlFromStorageAsync(trackId);
                var xmlParser = new XmlParser(xmlBytes);
                if (!xmlParser.Parser())
                    throw new Exception(xmlParser.ParserError);

                nitModel = xmlParser.Fields.ToObject<NitModel>();

                eventRadian = new EventRadianModel(
                    documentMeta.DocumentReferencedKey,
                    documentMeta.PartitionKey,
                    documentMeta.EventCode,
                    documentMeta.DocumentTypeId,
                    nitModel.listID, 
                    documentMeta.CustomizationID,
                    documentMeta.SigningTimeStamp,
                    nitModel.ValidityPeriodEndDate,
                    documentMeta.SenderCode,
                    documentMeta.ReceiverCode,
                    nitModel.DocumentTypeIdRef,
                    xmlParser.DocumentReferenceId,
                    nitModel.IssuerPartyCode,
                    nitModel.IssuerPartyName
                    );

                bool validaMandatoListID = (Convert.ToInt32(documentMeta.EventCode) == (int)EventStatus.Mandato && nitModel.listID == "3") ? false : true;

                responses = await Instance.StartValidateSerieAndNumberAsync(trackId);                
                validateResponses.AddRange(responses);

                if (Convert.ToInt32(documentMeta.EventCode) == (int)EventStatus.SolicitudDisponibilizacion)
                {
                    EventRadianModel.SetValueEventAproveCufe(ref eventRadian, eventApproveCufe);
                    responses = await Instance.StartEventApproveCufe(eventApproveCufe);
                    validateResponses.AddRange(responses);
                }
                
                if(Convert.ToInt32(documentMeta.EventCode) != (int)EventStatus.Mandato)
                {
                    EventRadianModel.SetValuesDocReference(ref eventRadian, docReference);
                    responses = await Instance.StartValidateDocumentReference(docReference);
                    foreach(var itemReference in responses)
                    {
                        if (!itemReference.IsValid)
                            validEventRadian = false;
                    }
                    validateResponses.AddRange(responses);
                }

                //Si Mandato contiene CUFEs Referenciados
                if (validaMandatoListID && validEventRadian)
                {
                    EventRadianModel.SetValuesValidateParty(ref eventRadian, requestParty);
                    EventRadianModel.SetValuesEventPrev(ref eventRadian, eventPrev);
                    EventRadianModel.SetValuesSigningTime(ref eventRadian, signingTime);
                    responses = await Instance.StartValidateParty(requestParty);
                    validateResponses.AddRange(responses);
                    responses = await Instance.StartValidateEmitionEventPrevAsync(eventPrev);
                    validateResponses.AddRange(responses);
                    responses = await Instance.StartValidateSigningTimeAsync(signingTime);
                    validateResponses.AddRange(responses);
                }

                //Si es mandato registra en GlobalDocReferenceAttorney
                if (Convert.ToInt32(documentMeta.EventCode) == (int)EventStatus.Mandato 
                    && validEventRadian)
                {
                    referenceAttorney.TrackId = trackId;
                    responses = await Instance.StartValidateReferenceAttorney(referenceAttorney);
                    validateResponses.AddRange(responses);
                }

                validator.UpdateInTransactions(documentMeta.DocumentReferencedKey, documentMeta.EventCode);

            }
            else
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = ConfigurationManager.GetValue("ErrorCode_AAH07") + "-(R): ",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH07"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
                validateResponses.AddRange(responses);
            }                             

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


        public async Task<List<ValidateListResponse>> StartValidateEmitionEventPrevAsync(RequestObjectEventPrev eventPrev)
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
                    //Obtiene XML ApplicationResponse CUDE
                    var xmlBytesCude = await GetXmlFromStorageAsync(eventPrev.TrackIdCude);
                    xmlParserCude = new XmlParser(xmlBytesCude);
                    if (!xmlParserCude.Parser())
                        throw new Exception(xmlParserCude.ParserError);
                }
            }
            //Obtiene información factura referenciada Endoso electronico, Solicitud Disponibilización AR CUDE
            if (Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.SolicitudDisponibilizacion || Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.EndosoGarantia 
                || Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.EndosoPropiedad || Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.EndosoProcuracion
                || Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.Avales || Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.NotificacionPagoTotalParcial)
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
            }

            var validator = new Validator();           
            validateResponses.AddRange(validator.ValidateEmitionEventPrev(eventPrev, xmlParserCufe, xmlParserCude, nitModel));

            return validateResponses;

        }


        public async Task<List<ValidateListResponse>> StartValidateDocumentReference(RequestObjectDocReference validateReference)
        {
            var validateResponses = new List<ValidateListResponse>();
            var validator = new Validator();
            DateTime startDate = DateTime.UtcNow;


            validateResponses.AddRange(validator.ValidateDocumentReferencePrev(validateReference.TrackId,
                validateReference.IdDocumentReference, validateReference.EventCode, validateReference.DocumentTypeIdRef,
                validateReference.IssuerPartyCode, validateReference.IssuerPartyName));

            return validateResponses;
        }


        public async Task<List<ValidateListResponse>> StartValidateSigningTimeAsync(RequestObjectSigningTime data)
        {           
            var validateResponses = new List<ValidateListResponse>();
            DateTime startDate = DateTime.UtcNow;
            EventStatus code;
            string originalTrackId = data.TrackId;
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
                Convert.ToInt32(data.EventCode) == (int)EventStatus.NotificacionPagoTotalParcial
                )
            {
                var documentMeta = documentMetaTableManager.FindDocumentReferenced_EventCode_TypeId<GlobalDocValidatorDocumentMeta>(data.TrackId.ToLower(), data.DocumentTypeId,
                    "0"+ (int)code).FirstOrDefault();                
                if (documentMeta != null)
                {
                    data.TrackId = documentMeta.PartitionKey;
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
            }
            var xmlBytes = await GetXmlFromStorageAsync(data.TrackId);
            var xmlParser = new XmlParser(xmlBytes);
            if (!xmlParser.Parser())
                throw new Exception(xmlParser.ParserError);

            // Por el momento solo para el evento 036 se conserva el trackId original, con el fin de traer el PaymentDueDate del CUFE
            // y enviarlo al validator para una posterior validación contra la fecha de vencimiento del evento (036).
            string parameterPaymentDueDateFE = null;
            if (Convert.ToInt32(data.EventCode) == (int)EventStatus.SolicitudDisponibilizacion 
                || Convert.ToInt32(data.EventCode) == (int)EventStatus.NotificacionPagoTotalParcial)
            {
                var originalXmlBytes = await GetXmlFromStorageAsync(originalTrackId);
                var originalXmlParser = new XmlParser(originalXmlBytes);
                if (!originalXmlParser.Parser())
                    throw new Exception(originalXmlParser.ParserError);

                parameterPaymentDueDateFE = originalXmlParser.PaymentDueDate;
            }

            var nitModel = xmlParser.Fields.ToObject<NitModel>();
            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateSigningTime(data, xmlParser, nitModel, paymentDueDateFE: parameterPaymentDueDateFE));

            return validateResponses;
        }

        public List<ValidateListResponse> StartValidatePredecesor(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();
            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateReplacePredecesor(trackId));
            return validateResponses;
        }

        public async Task<List<ValidateListResponse>> StartValidateSerieAndNumberAsync(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();

            var xmlBytes = await GetXmlFromStorageAsync(trackId);
            var xmlParser = new XmlParser(xmlBytes);
            if (!xmlParser.Parser())
                throw new Exception(xmlParser.ParserError);

            var nitModel = xmlParser.Fields.ToObject<NitModel>();

            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateSerieAndNumber(nitModel));
            return validateResponses;
        }

        public async Task<List<ValidateListResponse>> StartNitValidationAsync(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();

            var xmlBytes = await GetXmlFromStorageAsync(trackId);

            NitModel nitModel = null;
            NominaModel nominaModel = null;
            XmlParser xmlParser = null;
            XmlParseNomina xmlParserNomina = null;
            var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
            if(documentMeta.DocumentTypeId == "11" || documentMeta.DocumentTypeId == "12")
            {
                xmlParserNomina = new XmlParseNomina(xmlBytes);
                if (!xmlParserNomina.Parser())
                    throw new Exception(xmlParserNomina.ParserError);

                nominaModel = xmlParserNomina.Fields.ToObject<NominaModel>();
            }
            else
            {
                xmlParser = new XmlParser(xmlBytes);
                if (!xmlParser.Parser())
                    throw new Exception(xmlParser.ParserError);

                nitModel = xmlParser.Fields.ToObject<NitModel>();
            }

            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateNit(nitModel, trackId, nominaModel: nominaModel));

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
            cmObject.FecNIE = xmlParser.globalDocPayrolls.Info_FechaGen;
            cmObject.HorNIE = xmlParser.globalDocPayrolls.HoraGen;
            cmObject.ValDev = Convert.ToString(xmlParser.globalDocPayrolls.DevengadosTotal);
            cmObject.ValDesc = Convert.ToString(xmlParser.globalDocPayrolls.DeduccionesTotal);
            cmObject.ValTol = Convert.ToString(xmlParser.globalDocPayrolls.ComprobanteTotal);
            cmObject.NitNIE = Convert.ToString(xmlParser.globalDocPayrolls.Emp_NIT);
            cmObject.DocEmp = Convert.ToString(xmlParser.globalDocPayrolls.NumeroDocumento);
            cmObject.SoftwareId = xmlParser.globalDocPayrolls.SoftwareID;
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
            string ReceiverCancelacion = String.Empty;

            //Anulacion de endoso electronico, TerminacionLimitacion de Circulacion obtiene CUFE referenciado en el CUDE emitido
            if (Convert.ToInt32(party.ResponseCode) == (int)EventStatus.InvoiceOfferedForNegotiation ||
                Convert.ToInt32(party.ResponseCode) == (int)EventStatus.AnulacionLimitacionCirculacion)
            {
                var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(party.TrackId, party.TrackId);
                if (documentMeta != null)
                {
                    //Obtiene el CUFE
                    party.TrackId = documentMeta.DocumentReferencedKey;
                    ReceiverCancelacion = documentMeta.ReceiverCode;
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

            //Valida existe cambio legitimo tenedor
            GlobalDocHolderExchange documentHolderExchange = documentHolderExchangeTableManager.FindhByCufeExchange<GlobalDocHolderExchange>(party.TrackId.ToLower(), true);            
            if (documentHolderExchange != null)
            {
                //Existe mas de un legitimo tenedor requiere un mandatario
                string[] endosatarios = documentHolderExchange.PartyLegalEntity.Split('|');
                if(endosatarios.Length == 1)
                {
                    nitModel.SenderCode = documentHolderExchange.PartyLegalEntity;
                }
                else
                {
                    foreach(string endosatario in endosatarios)
                    {
                        GlobalDocReferenceAttorney documentAttorney = documentAttorneyTableManager.FindhByCufeSenderAttorney<GlobalDocReferenceAttorney>(party.TrackId.ToLower(), endosatario, xmlParserCude.ProviderCode);
                        if(documentAttorney == null)
                        {
                            valid = false;
                            validateResponses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "089",
                                ErrorMessage = "Mandatario no encontrado para el Nit del Endosatario" + endosatario,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }                       
                    }
                    if(valid)
                    {
                        nitModel.SenderCode = party.SenderParty;
                    }
                }
            }
            if(valid)
            {
                //Enodsatario Anulacion endoso
                nitModel.ReceiverCode = ReceiverCancelacion != "" ? ReceiverCancelacion : nitModel.ReceiverCode;
                var validator = new Validator();
                validateResponses.AddRange(validator.ValidateParty(nitModel, party, xmlParserCude));
            }
            return validateResponses;
        }

        public async Task<List<ValidateListResponse>> StartEventApproveCufe(RequestObjectEventApproveCufe eventApproveCufe)
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
                response.ErrorMessage = $"La factura referenciada no es de tipo crédito, PaymentMeansID=2";
                response.IsValid = false;
                response.ErrorCode = "Regla: LGC62-(R): ";
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

            SoftwareModel softwareModel = null;
            NominaModel nominaModel = null;
            XmlParser xmlParser = null;
            XmlParseNomina xmlParserNomina = null;
            var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
            if (documentMeta.DocumentTypeId == "11" || documentMeta.DocumentTypeId == "12")
            {
                xmlParserNomina = new XmlParseNomina(xmlBytes);
                if (!xmlParserNomina.Parser())
                    throw new Exception(xmlParserNomina.ParserError);

                nominaModel = xmlParserNomina.Fields.ToObject<NominaModel>();
            }
            else
            {
                xmlParser = new XmlParser(xmlBytes);
                if (!xmlParser.Parser())
                    throw new Exception(xmlParser.ParserError);

                softwareModel = xmlParser.Fields.ToObject<SoftwareModel>();
            }

            var validator = new Validator();
            validateResponses.Add(validator.ValidateSoftware(softwareModel, trackId, nominaModel: nominaModel));

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

        public async Task<List<ValidateListResponse>> StartValidateReferenceAttorney(RequestObjectReferenceAttorney data)
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

        public async Task<List<ValidateListResponse>> StartValidateIndividualPayroll(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();

            var xmlBytes = await GetXmlFromStorageAsync(trackId);
            var xmlParser = new XmlParseNomina(xmlBytes);
            if (!xmlParser.Parser())
                throw new Exception(xmlParser.ParserError);

            var documentParsed = xmlParser.Fields.ToObject<DocumentParsedNomina>();
            DocumentParsedNomina.SetValues(ref documentParsed);
            
            // Validator instance
            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateIndividualPayroll(xmlParser, documentParsed));
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