using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Services.Utils.Common;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Linq;
using Gosocket.Dian.Functions.Models;

namespace Gosocket.Dian.Functions.Events
{
    public static class RegistrateCompletedRadian
    {
        private static readonly TableManager TableManagerGlobalDocValidatorDocumentMeta = new TableManager("GlobalDocValidatorDocumentMeta");
        private static readonly TableManager TableManagerGlobalDocRegisterProviderAR = new TableManager("GlobalDocRegisterProviderAR");
        private static readonly TableManager TableManagerGlobalDocReferenceAttorney = new TableManager("GlobalDocReferenceAttorney");
        private static readonly TableManager TableManagerGlobalDocHolderExchange = new TableManager("GlobalDocHolderExchange");
        private static readonly TableManager TableManagerDocumentTracking = new TableManager("GlobalDocValidatorTracking");

        [FunctionName("RegistrateCompletedRadian")]
        public static async Task<EventResponse> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Get request body
            var data = await req.Content.ReadAsAsync<RequestObject>();

            if (data == null)
                return new EventResponse { Code = "400", Message = "Request body is empty." };

            if (string.IsNullOrEmpty(data.TrackId))
                return new EventResponse { Code = "400", Message = "Please pass a trackId in the request body." };

            var trackIdCude = data.TrackId;
            var response = new EventResponse {
                Code = ((int)EventValidationMessage.Success).ToString(), 
                Message = EnumHelper.GetEnumDescription(EventValidationMessage.Success),            
            };
            try
            {
                var validations = new List<GlobalDocValidatorTracking>();
                GlobalDocValidatorDocumentMeta documentMeta = null;
                byte[] xmlBytes = null;
                validations = TableManagerDocumentTracking.FindByPartition<GlobalDocValidatorTracking>(trackIdCude);
                documentMeta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackIdCude, trackIdCude);
                xmlBytes = XmlUtil.GenerateApplicationResponseBytes(trackIdCude, documentMeta, validations);

                if (xmlBytes == null)
                {
                    response.Code = ((int)EventValidationMessage.Error).ToString();
                    response.Message = "No se pudo generar application response.";
                }
                else                               
                    response.XmlBytesBase64 = Convert.ToBase64String(xmlBytes);                    
                
                if (documentMeta != null)
                {
                    //Registra Mandato
                    if (Convert.ToInt32(documentMeta.EventCode) == (int)EventStatus.Mandato)
                    {
                        //Obtiene XML ApplicationResponse CUDE
                        var xmlBytesCude = await Utils.Utils.GetXmlFromStorageAsync(trackIdCude);
                        var xmlParserCude = new XmlParser(xmlBytesCude);
                        if (!xmlParserCude.Parser())
                            throw new Exception(xmlParserCude.ParserError);
                        InsertUpdateMandato(xmlParserCude, trackIdCude);
                    }

                    GlobalDocRegisterProviderAR documentRegisterAR = new GlobalDocRegisterProviderAR(trackIdCude, documentMeta.TechProviderCode)
                    {
                        DocumentTypeId = documentMeta.DocumentTypeId,
                        SerieAndNumber = documentMeta.SerieAndNumber,
                        SenderCode = documentMeta.SenderCode,
                        GlobalCufeId = documentMeta.DocumentReferencedKey,
                        EventCode = documentMeta.EventCode,
                        CustomizationID = documentMeta.CustomizationID
                    };

                    //Inserta registros AR en GlobalDocRegisterProviderAR
                    InsertGlobalDocRegisterProviderAR(documentRegisterAR);

                    //Actualiza registro Mandato asociado a la Factura, terminacion de mandato
                    if (Convert.ToInt32(documentMeta.EventCode) == (int)EventStatus.TerminacionMandato)
                        UpdateFinishAttorney(trackIdCude, documentMeta.DocumentReferencedKey, documentMeta.EventCode);

                    //Actualiza Cambio de propietario de la FETV endoso en propiedad
                    if (Convert.ToInt32(documentMeta.EventCode) == (int)EventStatus.EndosoPropiedad)
                    {
                        //Obtiene XML ApplicationResponse CUDE
                        var xmlBytesCude = await Utils.Utils.GetXmlFromStorageAsync(trackIdCude);
                        var xmlParserCude = new XmlParser(xmlBytesCude);
                        if (!xmlParserCude.Parser())
                            throw new Exception(xmlParserCude.ParserError);                     
                        UpdateEndoso(xmlParserCude, documentMeta);
                    }

                    //Actuliza estado en transaccion FETV
                    if (Convert.ToInt32(documentMeta.EventCode) == (int)EventStatus.EndosoPropiedad
                       || Convert.ToInt32(documentMeta.EventCode) == (int)EventStatus.EndosoGarantia
                       || Convert.ToInt32(documentMeta.EventCode) == (int)EventStatus.EndosoProcuracion)
                    {
                        UpdateInTransactions(documentMeta.DocumentReferencedKey);
                    }
                        
                    //Convierte a la Factura Electronica en Titulo Valor
                    if (Convert.ToInt32(documentMeta.EventCode) == (int)EventStatus.Accepted
                        || Convert.ToInt32(documentMeta.EventCode) == (int)EventStatus.AceptacionTacita)
                    {
                        UpdateIsInvoiceTV(documentMeta.DocumentReferencedKey);
                    }                        
                }
                else
                {
                    return new EventResponse
                    {
                        Code = ((int)EventValidationMessage.Error).ToString(),
                        Message = EnumHelper.GetEnumDescription(EventValidationMessage.Error)
                    };
                }

            }catch(Exception ex)
            {
                log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
                response.Code = ((int)EventValidationMessage.Error).ToString();
                response.Message = ex.Message;
            }
            
            return response;

        }

        #region InsertUpdateMandato
        private static void InsertUpdateMandato(XmlParser xmlParser, string trackIdCude)
        {
            var arrayTasks = new List<Task>();
            string modoOperacion = string.Empty;
            string startDateAttorney = string.Empty;
            string endDate = string.Empty;
            List<AttorneyModel> attorney = new List<AttorneyModel>();
           
            string senderCode = xmlParser.FieldValue("SenderCode", true).ToString();
            XmlNodeList cufeListResponseRefeerence = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse'][2]/*[local-name()='DocumentReference']/*[local-name()='ID']");

            string factorTemp = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='IssuerParty']/*[local-name()='PowerOfAttorney']/*[local-name()='AgentParty']/*[local-name()='PartyIdentification']/*[local-name()='ID']").Item(0)?.InnerText.ToString();
            string effectiveDate = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='Response']/*[local-name()='EffectiveDate']").Item(0)?.InnerText.ToString();
            string issuerPartyCode = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='IssuerParty']/*[local-name()='PowerOfAttorney']/*[local-name()='ID']").Item(0)?.InnerText.ToString();
            string serieAndNumber = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='ID']").Item(0)?.InnerText.ToString();
            string customizationID = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='CustomizationID']").Item(0)?.InnerText.ToString();
            string senderName = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='SenderParty']/*[local-name()='PartyTaxScheme']/*[local-name()='RegistrationName']").Item(0)?.InnerText.ToString();
            string listID = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='Response']/*[local-name()='ResponseCode']").Item(0)?.Attributes["listID"].Value;
            string firstName = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='SenderParty']/*[local-name()='Person']/*[local-name()='FirstName']").Item(0)?.InnerText.ToString();
            string familyName = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='SenderParty']/*[local-name()='Person']/*[local-name()='FamilyName']").Item(0)?.InnerText.ToString();
            string name = firstName + " " + familyName;

            //Descripcion Mandatario 
            switch (factorTemp)
            {
                case "M-SN-e":
                    modoOperacion = "SNE";
                    break;
                case "M-Factor":
                    modoOperacion = "F";
                    break;
                case "M-PT":
                    modoOperacion = "PT";
                    break;
            }

            //Valida Mandato si es Ilimitado o Limitado
            if (customizationID == "432" || customizationID == "434")
            {
                startDateAttorney = string.Empty;
                endDate = string.Empty;
            }
            else if (customizationID == "431" || customizationID == "433")
            {
                startDateAttorney = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='DocumentReference']/*[local-name()='ValidityPeriod']/*[local-name()='StartDate']").Item(0)?.InnerText.ToString();
                endDate = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='DocumentResponse']/*[local-name()='DocumentReference']/*[local-name()='ValidityPeriod']/*[local-name()='EndDate']").Item(0)?.InnerText.ToString();
            }

            
            for (int i = 0; i < cufeListResponseRefeerence.Count; i++)
            {
                AttorneyModel attorneyModel = new AttorneyModel();
                string[] tempCode = new string[0];

                //Solo si existe información referenciada del CUFE
                if (listID != "3")
                    attorneyModel.cufe = cufeListResponseRefeerence.Item(i).SelectNodes("//*[local-name()='DocumentReference']/*[local-name()='UUID']").Item(i)?.InnerText.ToString();
                else                
                    attorneyModel.cufe = "01";


                    string code = cufeListResponseRefeerence.Item(i).SelectNodes("//*[local-name()='DocumentResponse'][2]/*[local-name()='Response']/*[local-name()='ResponseCode']").Item(i)?.InnerText.ToString();
                if (!string.IsNullOrWhiteSpace(code))
                {
                    tempCode = code.Split(';');
                }

                foreach (string codeAttorney in tempCode)
                {
                    string[] tempCodeAttorney = codeAttorney.Split('-');                                        
                    if (attorneyModel.facultityCode == null)
                    {
                        attorneyModel.facultityCode += tempCodeAttorney[0];
                    }
                    else
                    {
                        attorneyModel.facultityCode += (";" + tempCodeAttorney[0]);
                    }
                    
                }
                attorney.Add(attorneyModel);
            }

            foreach (var attorneyDocument in attorney)
            {
                GlobalDocReferenceAttorney docReferenceAttorney = new GlobalDocReferenceAttorney(trackIdCude, attorneyDocument.cufe)
                {
                    Active = true,
                    Actor = modoOperacion,
                    EffectiveDate = effectiveDate,
                    EndDate = endDate,
                    FacultityCode = attorneyDocument.facultityCode,
                    IssuerAttorney = issuerPartyCode,
                    SenderCode = senderCode,
                    StartDate = startDateAttorney,
                    AttorneyType = customizationID,
                    SerieAndNumber = serieAndNumber,
                    SenderName = senderName,
                    IssuerAttorneyName = name,
                    ResponseCodeListID = listID
                };
                arrayTasks.Add(TableManagerGlobalDocReferenceAttorney.InsertOrUpdateAsync(docReferenceAttorney));
            }
        }
        #endregion


        #region InsertGlobalDocRegisterProviderAR
        private static void InsertGlobalDocRegisterProviderAR(GlobalDocRegisterProviderAR documentRegisterAR)
        {
            var arrayTasks = new List<Task>();
            arrayTasks.Add(TableManagerGlobalDocRegisterProviderAR.InsertOrUpdateAsync(documentRegisterAR));
        }
        #endregion

        #region UpdateFinishAttorney
        private static void UpdateFinishAttorney(string trackId, string trackIdAttorney, string eventCode)
        {
            //validation if is an anulacion de mandato (Code 044)
            var arrayTasks = new List<Task>();

            List<GlobalDocReferenceAttorney> documentsAttorney = TableManagerGlobalDocReferenceAttorney.FindAll<GlobalDocReferenceAttorney>(trackIdAttorney).ToList();
            if (documentsAttorney != null || documentsAttorney.Count > 0)
            {
                foreach (var documentAttorney in documentsAttorney)
                {
                    documentAttorney.Active = false;
                    documentAttorney.DocReferencedEndAthorney = trackId;
                    arrayTasks.Add(TableManagerGlobalDocReferenceAttorney.InsertOrUpdateAsync(documentAttorney));
                }
            }
        }
        #endregion

        #region UpdateIsInvoiceTV
        private static void UpdateIsInvoiceTV(string trackId)
        {
            //Actualiza factura electronica TV eventos fase 1 registrados
            var arrayTasks = new List<Task>();

            GlobalDocValidatorDocumentMeta validatorDocumentMeta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
            if (validatorDocumentMeta != null)
            {
                validatorDocumentMeta.IsInvoiceTV = true;
                arrayTasks.Add(TableManagerGlobalDocValidatorDocumentMeta.InsertOrUpdateAsync(validatorDocumentMeta));

            }
        }
        #endregion

        #region UpdateInTransactions
        private static void UpdateInTransactions(string trackId)
        {
            //valida InTransaction Factura - eventos Endoso en propeidad, Garantia y procuración
            var arrayTasks = new List<Task>();

            GlobalDocValidatorDocumentMeta validatorDocumentMeta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
            if (validatorDocumentMeta != null)
            {
                validatorDocumentMeta.InTransaction = false;
                arrayTasks.Add(TableManagerGlobalDocValidatorDocumentMeta.InsertOrUpdateAsync(validatorDocumentMeta));
            }

        }
        #endregion


        #region UpdateEndoso
        private static void UpdateEndoso(XmlParser xmlParser, GlobalDocValidatorDocumentMeta documentMeta)
        {
            //validation if is an Endoso en propiedad (Code 037)
            var arrayTasks = new List<Task>();
            string sender = string.Empty;
            string senderList = string.Empty;
            string valueStockAmountSender = string.Empty;
            string valueStockAmountSenderList = string.Empty;

            List<GlobalDocHolderExchange> documentsHolderExchange = TableManagerGlobalDocHolderExchange.FindpartitionKey<GlobalDocHolderExchange>(documentMeta.DocumentReferencedKey.ToLower()).ToList();
            if (documentsHolderExchange != null || documentsHolderExchange.Count > 0)
            {
                foreach (var documentHolderExchange in documentsHolderExchange)
                {
                    documentHolderExchange.Active = false;
                    arrayTasks.Add(TableManagerGlobalDocHolderExchange.InsertOrUpdateAsync(documentHolderExchange));
                }
            }

            //Lista de endosantes
            XmlNodeList valueListSender = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']");
            for (int i = 0; i < valueListSender.Count; i++)
            {
                sender = valueListSender.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CompanyID']").Item(i)?.InnerText.ToString();
                valueStockAmountSender = valueListSender.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CorporateStockAmount']").Item(i)?.InnerText.ToString();
                if (i == 0)
                {
                    senderList += sender;
                    valueStockAmountSenderList += valueStockAmountSender;
                }
                else
                {
                    senderList += "|" + sender;
                    valueStockAmountSenderList += "|" + valueStockAmountSender;
                }
            }

            //Lista de endosatrios
            XmlNodeList valueListReceiver = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='ReceiverParty']/*[local-name()='PartyLegalEntity']");
            for (int i = 0; i < valueListReceiver.Count; i++)
            {
                string companyId = valueListReceiver.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='ReceiverParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CompanyID']").Item(i)?.InnerText.ToString();
                string valueStockAmount = valueListReceiver.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='ReceiverParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CorporateStockAmount']").Item(i)?.InnerText.ToString();
                string rowKey = senderList + "|" + companyId;
                GlobalDocHolderExchange globalDocHolderExchange = new GlobalDocHolderExchange(documentMeta.DocumentReferencedKey.ToLower(), rowKey)
                {
                    Timestamp = DateTime.Now,
                    Active = true,
                    CorporateStockAmount = valueStockAmount,
                    GlobalDocumentId = documentMeta.PartitionKey,
                    PartyLegalEntity = companyId,
                    SenderCode = senderList,
                    CorporateStockAmountSender = valueStockAmountSenderList
                };
                arrayTasks.Add(TableManagerGlobalDocHolderExchange.InsertOrUpdateAsync(globalDocHolderExchange));
            }

        }
        #endregion

        public class RequestObject
        {
            [JsonProperty(PropertyName = "trackId")]
            public string TrackId { get; set; }       
        }
    }
}
