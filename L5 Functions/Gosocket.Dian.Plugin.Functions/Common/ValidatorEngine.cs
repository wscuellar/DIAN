using Gosocket.Dian.Application;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Plugin.Functions.Cufe;
using Gosocket.Dian.Plugin.Functions.Event;
using Gosocket.Dian.Plugin.Functions.EventApproveCufe;
using Gosocket.Dian.Plugin.Functions.Models;
using Gosocket.Dian.Plugin.Functions.SigningTime;
using Gosocket.Dian.Plugin.Functions.ValidateParty;
using Gosocket.Dian.Plugin.Functions.ValidateReferenceAttorney;
using Gosocket.Dian.Services.Cude;
using Gosocket.Dian.Services.Cuds;
using Gosocket.Dian.Services.NotaAjustes;
using Gosocket.Dian.Services.Utils;
using Gosocket.Dian.Services.Utils.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Gosocket.Dian.Plugin.Functions.Common
{
    public class ValidatorEngine
    {
        #region Global properties
        
        static readonly TableManager documentMetaTableManager = new TableManager("GlobalDocValidatorDocumentMeta");
        static readonly TableManager documentAttorneyTableManager = new TableManager("GlobalDocReferenceAttorney");
        static readonly TableManager documentHolderExchangeTableManager = new TableManager("GlobalDocHolderExchange");
        
        private static readonly TableManager TableManager = new TableManager("GlobalDocValidatorRuntime");
        private static readonly AssociateDocumentService associateDocumentService = new AssociateDocumentService();

        XmlDocument _xmlDocument;
        XPathDocument _document;
        XPathNavigator _navigator;
        XPathNavigator _navNs;
        XmlNamespaceManager _ns;
        byte[] _xmlBytes;

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


        public async Task<List<ValidateListResponse>> StartValidateInvoiceLineAsync(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();

            var xmlBytes = await GetXmlFromStorageAsync(trackId);
            var xmlParser = new XmlParser(xmlBytes);
            if (!xmlParser.Parser())
                throw new Exception(xmlParser.ParserError);

            validatorDocumentNameSpaces(xmlBytes);

            // Validator instance
            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateInvoiceLine(xmlParser, _ns));

            return validateResponses;
        }

        public async Task<List<ValidateListResponse>> StarValidateTaxWithHoldingAsync(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();

            var xmlBytes = await GetXmlFromStorageAsync(trackId);
            var xmlParser = new XmlParser(xmlBytes);
            if (!xmlParser.Parser())
                throw new Exception(xmlParser.ParserError);

            validatorDocumentNameSpaces(xmlBytes);

            // Validator instance
            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateTaxWithHolding(xmlParser, _ns));

            return validateResponses;
        }

        public async Task<List<ValidateListResponse>> StarValidateTaxCategoryAsync(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();

            var xmlBytes = await GetXmlFromStorageAsync(trackId);
            var xmlParser = new XmlParser(xmlBytes);
            if (!xmlParser.Parser())
                throw new Exception(xmlParser.ParserError);

            validatorDocumentNameSpaces(xmlBytes);

            // Validator instance
            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateTaxCategory(xmlParser, _ns));

            return validateResponses;
        }

        public async Task<List<ValidateListResponse>> StartNewValidationEventRadianAsync(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();
            var validator = new Validator();

            validateResponses.AddRange(await  validator.NewValidateEventRadianAsync(trackId));

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
                || Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.Avales || Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.NotificacionPagoTotalParcial
                || Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.EndorsementWithEffectOrdinaryAssignment 
                || Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.Objection
                || Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.TransferEconomicRights
                || Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.NotificationDebtorOfTransferEconomicRights
                || Convert.ToInt32(eventPrev.EventCode) == (int)EventStatus.PaymentOfTransferEconomicRights)
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
            validateResponses.AddRange(validator.ValidateEmitionEventPrev(eventPrev, xmlParserCufe.TotalInvoice.ToString(), xmlParserCude, nitModel));

            return validateResponses;

        }


        public async Task<List<ValidateListResponse>> StartValidateDocumentReference(RequestObjectDocReference validateReference)
        {
            var validateResponses = new List<ValidateListResponse>();
            var validator = new Validator();

            validateResponses.AddRange(validator.ValidateDocumentReferencePrev(validateReference.TrackId,
                validateReference.IdDocumentReference, validateReference.EventCode, validateReference.DocumentTypeIdRef,
                validateReference.IssuerPartyCode, validateReference.IssuerPartyName));

            return validateResponses;
        }


        public async Task<List<ValidateListResponse>> StartValidateSigningTimeAsync(RequestObjectSigningTime data)
        {
            var validateResponses = new List<ValidateListResponse>();
            EventStatus code;
            string trackIdAvailability = null;
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
                case (int)EventStatus.EndosoPropiedad:
                case (int)EventStatus.EndosoGarantia:
                case (int)EventStatus.EndosoProcuracion:
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
                Convert.ToInt32(data.EventCode) == (int)EventStatus.NotificacionPagoTotalParcial
                )
            {
                bool existDisponibilizaExpresa = false;
                //Servicio GlobalDocAssociate
                string eventSearch = "0" + (int)code;
                List<InvoiceWrapper> InvoiceWrapper = associateDocumentService.GetEventsByTrackId(data.TrackId.ToLower());

                if (InvoiceWrapper.Any())
                {
                    var trackIdEvent = InvoiceWrapper[0].Documents.FirstOrDefault(x => x.DocumentMeta.EventCode == eventSearch
                    && int.Parse(x.DocumentMeta.DocumentTypeId) == (int)DocumentType.ApplicationResponse );
                    if (trackIdEvent != null)
                    {
                        existDisponibilizaExpresa = true;
                        data.TrackId = trackIdEvent.DocumentMeta.PartitionKey;
                    }
                }

                // Validación de la Sección Signature - Fechas valida transmisión evento Solicitud Disponibilizacion
                if (Convert.ToInt32(data.EventCode) == (int)EventStatus.SolicitudDisponibilizacion && !existDisponibilizaExpresa)
                {
                    //Servicio GlobalDocAssociate
                    code = EventStatus.AceptacionTacita;
                    string eventSearchTacita = "0" + (int)code;
                    List<InvoiceWrapper> InvoiceWrapperTacita = associateDocumentService.GetEventsByTrackId(data.TrackId.ToLower());
                    if (InvoiceWrapperTacita.Any())
                        data.TrackId = InvoiceWrapperTacita[0].Documents.FirstOrDefault(x => x.DocumentMeta.EventCode == eventSearchTacita).DocumentMeta.PartitionKey;
                }
            }
            else if (Convert.ToInt32(data.EventCode) == (int)EventStatus.NegotiatedInvoice || Convert.ToInt32(data.EventCode) == (int)EventStatus.Avales)
            {
                //Servicio GlobalDocAssociate
                string eventSearch = "0" + (int)code;
                List<InvoiceWrapper> InvoiceWrapper = associateDocumentService.GetEventsByTrackId(data.TrackId.ToLower());

                if (InvoiceWrapper.Any())
                    data.TrackId = InvoiceWrapper[0].Documents.FirstOrDefault(x => x.DocumentMeta.EventCode == eventSearch
                    && int.Parse(x.DocumentMeta.DocumentTypeId) == (int)DocumentType.ApplicationResponse
                    && (x.DocumentMeta.CustomizationID == "361" || x.DocumentMeta.CustomizationID == "362" )).DocumentMeta.PartitionKey;

            }
            else if (Convert.ToInt32(data.EventCode) == (int)EventStatus.EndosoPropiedad
                || Convert.ToInt32(data.EventCode) == (int)EventStatus.EndosoGarantia
                || Convert.ToInt32(data.EventCode) == (int)EventStatus.EndosoProcuracion)
            {
                //Servicio GlobalDocAssociate
                string eventSearch = "0" + (int)code;
                List<InvoiceWrapper> InvoiceWrapper = associateDocumentService.GetEventsByTrackId(data.TrackId.ToLower());

                if (InvoiceWrapper.Any())
                {
                    var respTrackIdAvailability = InvoiceWrapper[0].Documents.FirstOrDefault(x => x.DocumentMeta.EventCode == eventSearch
                    && int.Parse(x.DocumentMeta.DocumentTypeId) == (int)DocumentType.ApplicationResponse);
                    if(respTrackIdAvailability != null)
                    {
                        trackIdAvailability = respTrackIdAvailability.DocumentMeta.PartitionKey;
                    }
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
                || Convert.ToInt32(data.EventCode) == (int)EventStatus.NotificacionPagoTotalParcial
                || Convert.ToInt32(data.EventCode) == (int)EventStatus.EndosoPropiedad
                || Convert.ToInt32(data.EventCode) == (int)EventStatus.EndosoGarantia
                || Convert.ToInt32(data.EventCode) == (int)EventStatus.EndosoProcuracion)
            {
                var originalXmlBytes = await GetXmlFromStorageAsync(originalTrackId);
                var originalXmlParser = new XmlParser(originalXmlBytes);
                if (!originalXmlParser.Parser())
                    throw new Exception(originalXmlParser.ParserError);

                parameterPaymentDueDateFE = originalXmlParser.PaymentDueDate;
            }

            DateTime? signingTimeAvailability = null;
            if ((Convert.ToInt32(data.EventCode) == (int)EventStatus.EndosoPropiedad
                || Convert.ToInt32(data.EventCode) == (int)EventStatus.EndosoGarantia
                || Convert.ToInt32(data.EventCode) == (int)EventStatus.EndosoProcuracion) && !string.IsNullOrWhiteSpace(trackIdAvailability))
            {
                var availabilityXmlBytes = await GetXmlFromStorageAsync(trackIdAvailability);
                var availabilityXmlParser = new XmlParser(availabilityXmlBytes);
                if (!availabilityXmlParser.Parser())
                    throw new Exception(availabilityXmlParser.ParserError);

                signingTimeAvailability = Convert.ToDateTime(availabilityXmlParser.SigningTime);
            }

            var nitModel = xmlParser.Fields.ToObject<NitModel>();
            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateSigningTime(data, xmlParser.SigningTime, xmlParser.PaymentDueDate, nitModel, paymentDueDateFE: parameterPaymentDueDateFE,
                signingTimeAvailability: signingTimeAvailability));

            return validateResponses;
        }

        public async Task<List<ValidateListResponse>> StartValidatePredecesor(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();

            var xmlBytes = await GetXmlFromStorageAsync(trackId);
            var xmlParser = new XmlParseNomina(xmlBytes);
            if (!xmlParser.Parser())
                throw new Exception(xmlParser.ParserError);

            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateReplacePredecesor(trackId, xmlParser));
            return validateResponses;
        }

        public async Task<List<ValidateListResponse>> StartValidateSerieAndNumberAsync(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();
            GlobalDocValidatorDocumentMeta documentMeta = new GlobalDocValidatorDocumentMeta();

            var xmlBytes = await GetXmlFromStorageAsync(trackId);
            var xmlParser = new XmlParser(xmlBytes);
            if (!xmlParser.Parser())
                throw new Exception(xmlParser.ParserError);

            var nitModel = xmlParser.Fields.ToObject<NitModel>();
            documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);

            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateSerieAndNumber(nitModel, documentMeta));
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

            if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayroll
                || Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayrollAdjustments)
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

        public async Task<List<ValidateListResponse>> StartNoteReferenceValidation(string trackId)
        {
            //var xmlBytesNotaAjuste = await GetXmlFromStorageAsync(trackId);
            //var parserNotaAjuste = new XmlToNotaAjusteParser();
            //var modelNotaAjuste = parserNotaAjuste.Parser(xmlBytesNotaAjuste);

            var validateResponses = new List<ValidateListResponse>();
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
            cmObject.DocumentType = xmlParser.Fields["DocumentTypeId"].ToString();
            cmObject.Cune = xmlParser.globalDocPayrolls.CUNE;
            cmObject.NumNIE = xmlParser.globalDocPayrolls.Numero;

            cmObject.FecNIE = xmlParser.globalDocPayrolls.Info_FechaGen.Value.ToString("yyyy-MM-dd");
            cmObject.HorNIE = xmlParser.globalDocPayrolls.HoraGen;
            cmObject.ValDev = xmlParser.globalDocPayrolls.DevengadosTotal;
            cmObject.ValDesc = xmlParser.globalDocPayrolls.DeduccionesTotal;
            cmObject.ValTol = xmlParser.globalDocPayrolls.ComprobanteTotal;
            cmObject.NitNIE = Convert.ToString(xmlParser.globalDocPayrolls.Emp_NIT);
            cmObject.DocEmp = Convert.ToString(xmlParser.globalDocPayrolls.NumeroDocumento);
            cmObject.SoftwareId = xmlParser.globalDocPayrolls.SoftwareID;
            cmObject.TipAmb = Convert.ToString(xmlParser.globalDocPayrolls.Ambiente);
            cmObject.TipoXML = xmlParser.globalDocPayrolls.TipoXML;
            cmObject.TipNota = xmlParser.globalDocPayrolls.TipoNota;

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
            string receiverCancelacion = String.Empty;
            string issuerAttorney = string.Empty;
            string senderAttorney = string.Empty;
            string trackIdAvailability = null;

            //Anulacion de endoso electronico, TerminacionLimitacion de Circulacion obtiene CUFE referenciado en el CUDE emitido
            if (Convert.ToInt32(party.ResponseCode) == (int)EventStatus.InvoiceOfferedForNegotiation ||
                Convert.ToInt32(party.ResponseCode) == (int)EventStatus.AnulacionLimitacionCirculacion)
            {
                var documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(party.TrackId, party.TrackId);
                if (documentMeta != null)
                {
                    //Obtiene el CUFE
                    party.TrackId = documentMeta.DocumentReferencedKey;
                    receiverCancelacion = documentMeta.ReceiverCode;
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


            // ...
            List<string> issuerAttorneyList = null;
            var eventCode = int.Parse(party.ResponseCode);
            if(eventCode == (int)EventStatus.Avales || eventCode == (int)EventStatus.NegotiatedInvoice || eventCode == (int)EventStatus.AnulacionLimitacionCirculacion)
            {
                var attorneyList = documentAttorneyTableManager.FindDocumentReferenceAttorneyByCUFEList<GlobalDocReferenceAttorney>(party.TrackId);
                if(attorneyList != null && attorneyList.Count > 0)
                {
                    issuerAttorneyList = new List<string>();
                    // ForEach...
                    attorneyList.ForEach(item =>
                    {
                        if(!string.IsNullOrWhiteSpace(item.EndDate))
                        {
                            var endDate = Convert.ToDateTime(item.EndDate);
                            if (endDate.Date > DateTime.Now.Date) issuerAttorneyList.Add(item.IssuerAttorney);
                        }
                        else issuerAttorneyList.Add(item.IssuerAttorney);
                    });
                }
            }
            else if (Convert.ToInt32(party.ResponseCode) == (int)EventStatus.EndosoPropiedad)
            {
                string eventDisponibiliza = "0" + (int)EventStatus.SolicitudDisponibilizacion;
                List<InvoiceWrapper> InvoiceWrapper = associateDocumentService.GetEventsByTrackId(party.TrackId.ToLower());

                if(InvoiceWrapper.Any())
                    trackIdAvailability = InvoiceWrapper[0].Documents.FirstOrDefault(x => x.DocumentMeta.EventCode == eventDisponibiliza
                    && int.Parse(x.DocumentMeta.DocumentTypeId) == (int)DocumentType.ApplicationResponse).DocumentMeta.PartitionKey;
            }

            string partyLegalEntityName = null, partyLegalEntityCompanyID = null, availabilityCustomizationId = null;
            if ((Convert.ToInt32(party.ResponseCode) == (int)EventStatus.EndosoPropiedad && !string.IsNullOrWhiteSpace(trackIdAvailability)))
            {
                var availabilityXmlBytes = await GetXmlFromStorageAsync(trackIdAvailability);
                var availabilityXmlParser = new XmlParser(availabilityXmlBytes);
                if (!availabilityXmlParser.Parser())
                    throw new Exception(availabilityXmlParser.ParserError);

                partyLegalEntityName = availabilityXmlParser.Fields["PartyLegalEntityName"].ToString();
                partyLegalEntityCompanyID = availabilityXmlParser.Fields["PartyLegalEntityCompanyID"].ToString();
                availabilityCustomizationId = availabilityXmlParser.Fields["CustomizationId"].ToString();
            }


            if (eventCode == (int)EventStatus.TerminacionMandato)
            {
                var attorney = documentAttorneyTableManager.FindByPartition<GlobalDocReferenceAttorney>(party.TrackId).ToList();

                if (attorney != null && attorney.Count > 0)
                {
                    foreach (var item in attorney)
                    {
                        issuerAttorney = item.IssuerAttorney;
                        senderAttorney = item.SenderCode;
                    }
                }
            }

            //Valida existe cambio legitimo tenedor
            GlobalDocHolderExchange documentHolderExchange = documentHolderExchangeTableManager.FindhByCufeExchange<GlobalDocHolderExchange>(party.TrackId.ToLower(), true);
            if (documentHolderExchange != null)
            {
                //Existe mas de un legitimo tenedor requiere un mandatario
                string[] endosatarios = documentHolderExchange.PartyLegalEntity.Split('|');
                if (endosatarios.Length == 1)
                {
                    nitModel.SenderCode = documentHolderExchange.PartyLegalEntity;
                }
                else
                {
                    foreach (string endosatario in endosatarios)
                    {
                        GlobalDocReferenceAttorney documentAttorney = documentAttorneyTableManager.FindhByCufeSenderAttorney<GlobalDocReferenceAttorney>(party.TrackId.ToLower(), endosatario, xmlParserCude.ProviderCode);
                        if (documentAttorney == null)
                        {
                            valid = false;
                            validateResponses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "LGC35",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC35"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    if (valid)
                    {
                        nitModel.SenderCode = party.SenderParty;
                    }
                }
            }
            if (valid)
            {
                //Enodsatario Anulacion endoso
                nitModel.ReceiverCode = receiverCancelacion != "" ? receiverCancelacion : nitModel.ReceiverCode;
                var validator = new Validator();
                validateResponses.AddRange(validator.ValidateParty(nitModel, party, xmlParserCude, issuerAttorneyList,
                    issuerAttorney, senderAttorney, partyLegalEntityName, partyLegalEntityCompanyID, availabilityCustomizationId));
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

            if (xmlParser.PaymentMeansID != "2")
            {
                ValidateListResponse response = new ValidateListResponse();
                response.IsValid = false;
                response.Mandatory = true;
                response.ErrorCode = "LGC62";
                response.ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC62");
                response.ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds;
                validateResponses.Add(response);
                return validateResponses;
            }

            var validator = new Validator();
            validateResponses.AddRange(await validator.EventApproveCufe(nitModel, eventApproveCufe));
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
            numberRangeModel.SigningTime = xmlParser.SigningTime;

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

            if (Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayroll
                || Convert.ToInt32(documentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayrollAdjustments)
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

            validatorDocumentNameSpaces(xmlBytes);

            var validator = new Validator();
            validateResponses.AddRange(await validator.ValidateReferenceAttorney(xmlParser, data.TrackId, _ns));

            return validateResponses;
        }

        public async Task<List<ValidateListResponse>> StartValidateIndividualPayroll(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();
            var documentParsed = new DocumentParsedNomina();

            GlobalDocValidatorDocumentMeta documentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);

            documentParsed.EmpleadorNIT = documentMeta.SenderCode;
            documentParsed.SerieAndNumber = documentMeta.SerieAndNumber;
            documentParsed.DocumentTypeId = documentMeta.DocumentTypeId;
            documentParsed.NumeroDocumento = documentMeta.ReceiverCode;
            documentParsed.Novelty = (bool)documentMeta.Novelty;
            documentParsed.CUNENov = documentMeta.CuneNovedad;
            documentParsed.FechaPagoInicio = documentMeta.FechaPagoNominaInicio;

            DocumentParsedNomina.SetValues(ref documentParsed);

            // Validator instance
            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateIndividualPayroll(documentParsed));

            //Valida fecha firma
            validateResponses.AddRange(validator.ValidateSignDate(documentMeta));

            return validateResponses;
        }

        public void validatorDocumentNameSpaces(byte[] xmlBytes)
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

        public async Task<List<ValidateListResponse>> StartValidateNamespacePayrollAsync(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();

            XmlParseNomina xmlParserNomina = null;
            var xmlBytes = await GetXmlFromStorageAsync(trackId);

            xmlParserNomina = new XmlParseNomina(xmlBytes);
            if (!xmlParserNomina.Parser())
                throw new Exception(xmlParserNomina.ParserError);

            var validator = new Validator();
            validateResponses.AddRange(validator.ValidateNamespacePayroll(xmlParserNomina));
             
            return validateResponses;
        }
        public async Task<List<ValidateListResponse>> StartValidateCuds(RequestObjectCuds cuds)
        {
            var validateResponses = new List<ValidateListResponse>();

            var xmlBytes = await GetXmlFromStorageAsync(cuds.TrackId);

            var parser = new XmlToDocumentoSoporteParser();
            var modelCuds = parser.Parser(xmlBytes);
            var validator = new Validator();
            validateResponses.Add(validator.ValidateCuds(modelCuds, cuds));
            return validateResponses;
        }


        public async Task<List<ValidateListResponse>> StartValidateCude(RequestObjectCude cude)
        {
            var validateResponses = new List<ValidateListResponse>();

            var xmlBytes = await GetXmlFromStorageAsync(cude.TrackId);

            var parser = new XmlToDocumentoEquivalenteParser();
            var modelCude = parser.Parser(xmlBytes);
            var validator = new Validator();
            validateResponses.Add(validator.ValidateCude(modelCude, cude));
            return validateResponses;
        }

        public async Task<List<ValidateListResponse>> StartValidateRequiredDocRadianAsync(string trackId)
        {
            var validateResponses = new List<ValidateListResponse>();
            var validator = new Validator();

            validateResponses.AddRange(await validator.ValidateRequiredDocRadianAsync(trackId));            

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
                    { "SheldHolderTaxLevelCodes", "/sig:Invoice/cac:Delivery/cac:DeliveryParty/cac:PartyTaxScheme/cbc:TaxLevelCode" },
                    { "InvoiceTypeCode","/sig:Invoice/cbc:InvoiceTypeCode" },
                    { "PartyTaxSchemeTaxLevelCodes", "/sig:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cac:ShareholderParty/cac:Party/cac:PartyTaxScheme/cbc:TaxLevelCode" },
            };
            return dictionary;
        }

        public async Task<byte[]> GetXmlFromStorageAsync(string trackId)
        {
            
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