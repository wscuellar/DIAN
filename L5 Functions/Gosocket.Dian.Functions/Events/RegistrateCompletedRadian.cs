using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Functions.Cryptography.Common;
using Gosocket.Dian.Functions.Utils;
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

namespace Gosocket.Dian.Functions.Events
{
    public static class RegistrateCompletedRadian
    {
        private static readonly TableManager TableManagerGlobalDocValidatorDocumentMeta = new TableManager("GlobalDocValidatorDocumentMeta");
        private static readonly TableManager TableManagerGlobalDocRegisterProviderAR = new TableManager("GlobalDocRegisterProviderAR");
        private static readonly TableManager TableManagerGlobalDocReferenceAttorney = new TableManager("GlobalDocReferenceAttorney");
        private static readonly TableManager TableManagerGlobalDocHolderExchange = new TableManager("GlobalDocHolderExchange");       

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
            var response = new EventResponse { Code = ((int)EventValidationMessage.Success).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.Success) };
            try
            {
                GlobalDocValidatorDocumentMeta documentMeta = null;
                documentMeta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackIdCude, trackIdCude);
                if (documentMeta != null)
                {
                    //Referecnia evento inicial para Anulacion Endoso y Terminacion de Limitacion circulacion
                    if (Convert.ToInt32(documentMeta.EventCode) == (int)EventStatus.InvoiceOfferedForNegotiation
                        || Convert.ToInt32(documentMeta.EventCode) == (int)EventStatus.AnulacionLimitacionCirculacion)
                    {
                        documentMeta.CancelElectronicEvent = documentMeta.DocumentReferencedKey;

                        //Obtiene informacion CUFE referenciado Endoso y Terminacion de Limitacion circulacion
                        var documentMetaReferenced = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(documentMeta.DocumentReferencedKey, documentMeta.DocumentReferencedKey);
                        documentMeta.DocumentReferencedKey = documentMetaReferenced.DocumentReferencedKey;
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

        private static void InsertGlobalDocRegisterProviderAR(GlobalDocRegisterProviderAR documentRegisterAR)
        {
            var arrayTasks = new List<Task>();
            arrayTasks.Add(TableManagerGlobalDocRegisterProviderAR.InsertOrUpdateAsync(documentRegisterAR));
        }

        private static void UpdateFinishAttorney(string trackId, string trackIdAttorney, string eventCode)
        {
            //validation if is an anulacion de mandato (Code 044)
            var arrayTasks = new List<Task>();
           
            List<GlobalDocReferenceAttorney> documentsAttorney = TableManagerGlobalDocReferenceAttorney.FindAll<GlobalDocReferenceAttorney>(trackIdAttorney).ToList();
            foreach (var documentAttorney in documentsAttorney)
            {
                documentAttorney.Active = false;
                documentAttorney.DocReferencedEndAthorney = trackId;
                arrayTasks.Add(TableManagerGlobalDocReferenceAttorney.InsertOrUpdateAsync(documentAttorney));
            }            
        }

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

        private static void UpdateEndoso(XmlParser xmlParser, GlobalDocValidatorDocumentMeta documentMeta)
        {
            //validation if is an Endoso en propiedad (Code 037)
            var arrayTasks = new List<Task>();
            string sender = string.Empty;
            string senderList = string.Empty;
            string valueStockAmountSender = string.Empty;
            string valueStockAmountSenderList = string.Empty;           

            List<GlobalDocHolderExchange> documentsHolderExchange = TableManagerGlobalDocHolderExchange.FindpartitionKey<GlobalDocHolderExchange>(documentMeta.DocumentReferencedKey.ToLower()).ToList();
            foreach (var documentHolderExchange in documentsHolderExchange)
            {
                documentHolderExchange.Active = false;
                arrayTasks.Add(TableManagerGlobalDocHolderExchange.InsertOrUpdateAsync(documentHolderExchange));
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

        public class RequestObject
        {
            [JsonProperty(PropertyName = "trackId")]
            public string TrackId { get; set; }
        }
    }
}
