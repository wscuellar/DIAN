using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Plugin.Functions.Event;
using Gosocket.Dian.Plugin.Functions.Models;
using Gosocket.Dian.Services.Utils.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace Gosocket.Dian.Plugin.Functions.Common
{
    public class LogicalEventRadian
    {
        #region Global properties 
        static readonly TableManager documentValidatorTableManager = new TableManager("GlobalDocValidatorDocument");
        static readonly TableManager documentAttorneyTableManager = new TableManager("GlobalDocReferenceAttorney");
        static readonly TableManager documentHolderExchangeTableManager = new TableManager("GlobalDocHolderExchange");
        static readonly TableManager documentMetaTableManager = new TableManager("GlobalDocValidatorDocumentMeta");

        private readonly string successfulMessage = "Evento ValidateEmitionEventPrev referenciado correctamente";
        #endregion

        #region GetAssociateDocument
        public List<GlobalDocValidatorDocumentMeta> GetAssociateDocument()
        {
            DateTime startDate = DateTime.UtcNow;
            List<GlobalDocValidatorDocumentMeta> responses = new List<GlobalDocValidatorDocumentMeta>();





            return responses;

        }
        #endregion


        #region ValidatePaymetInfo
        public List<ValidateListResponse> ValidatePaymetInfo(List<GlobalDocValidatorDocumentMeta> documentMeta)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            //Debe existir solicitud de disponibilizacion
            var listDispnibiliza = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.SolicitudDisponibilizacion).ToList();
            if (listDispnibiliza != null)
            {
                bool validForItem = false;
                foreach (var itemDispnibiliza in listDispnibiliza)
                {
                    var documentDisponibiliza = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemDispnibiliza.Identifier, itemDispnibiliza.Identifier, itemDispnibiliza.PartitionKey);
                    if (documentDisponibiliza != null)
                    {
                        //Valida Pago Total FETV
                        validForItem = false;
                        var listPagoTotal = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.NotificacionPagoTotalParcial
                        && Convert.ToInt32(t.CustomizationID) == (int)EventCustomization.PaymentBillFTV).ToList();
                        if (listPagoTotal != null)
                        {
                            foreach (var itemPagoTotal in listPagoTotal)
                            {
                                var documentPagoTotal = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemPagoTotal.Identifier, itemPagoTotal.Identifier, itemPagoTotal.PartitionKey);
                                if (documentPagoTotal != null)
                                {
                                    responses.Add(new ValidateListResponse
                                    {
                                        IsValid = false,
                                        Mandatory = true,
                                        ErrorCode = "LGC50",
                                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC50"),
                                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                    });
                                    break;
                                }
                            }
                        }
                        break;
                    }
                    else
                        validForItem = true;
                }

                if (validForItem)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "LGC51",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC51"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }
            else
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "LGC51",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC51"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            return responses;
        }
        #endregion


        #region Validate PaymentOfTransferEconomicRights
        public List<ValidateListResponse> ValidatePaymentOfTransfer(XmlParser xmlParserCude, NitModel nitModel)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            //Valida monto pago parcial o total
            var responseListPayment = ValidatePayment(xmlParserCude, nitModel);
            if (responseListPayment != null)
            {
                foreach (var item in responseListPayment)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = item.IsValid,
                        Mandatory = item.Mandatory,
                        ErrorCode = item.ErrorCode,
                        ErrorMessage = item.ErrorMessage,
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }
            else
                return null;

            return responses;
        }
        #endregion


        #region ValidatePartialPayment
        public List<ValidateListResponse> ValidatePartialPayment(List<GlobalDocValidatorDocumentMeta> documentMeta, RequestObjectEventPrev eventPrev, XmlParser xmlParserCude, NitModel nitModel)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            //Valida monto pago parcial o total
            var responseListPayment = ValidatePayment(xmlParserCude, nitModel);
            if (responseListPayment != null)
            {
                foreach (var item in responseListPayment)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = item.IsValid,
                        Mandatory = item.Mandatory,
                        ErrorCode = item.ErrorCode,
                        ErrorMessage = item.ErrorMessage,
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            //Validacion debe exisitir evento Solicitud de Disponibilización
            var listDisponibilizacion = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.SolicitudDisponibilizacion).ToList();
            if (listDisponibilizacion == null || listDisponibilizacion.Count <= 0)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "LGC43",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC43"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }
            else
            {
                bool validForItem = false;
                foreach (var itemListDisponibilizacion in listDisponibilizacion)
                {
                    var documentDisponibilizacion = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListDisponibilizacion.Identifier, itemListDisponibilizacion.Identifier, itemListDisponibilizacion.PartitionKey);
                    if (documentDisponibilizacion != null)
                    {
                        validForItem = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = successfulMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                    else
                        validForItem = true;
                }

                if (validForItem)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "LGC43",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC43"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            //Valida titulo valor tiene una limitación previa (041)
            string validCancelElectronicEvent = string.Empty;
            var listLimitacion = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.NegotiatedInvoice).ToList();
            if (listLimitacion != null)
            {
                foreach (var itemListLimitacion in listLimitacion)
                {
                    var documentLimitacion = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListLimitacion.Identifier, itemListLimitacion.Identifier, itemListLimitacion.PartitionKey);
                    if (documentLimitacion != null)
                    {
                        //No existe cancelacion de la limitacion permite Pago de la factura electrónica de venta título valor con listId 2
                        validCancelElectronicEvent = ValidateCancelElectronicEvent(documentMeta, documentLimitacion.DocumentKey, nitModel.SenderCode);
                        if (string.IsNullOrWhiteSpace(validCancelElectronicEvent))
                        {
                            if (eventPrev.ListId == "2")
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = true,
                                    Mandatory = true,
                                    ErrorCode = "100",
                                    ErrorMessage = successfulMessage,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                            else
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = false,
                                    Mandatory = true,
                                    ErrorCode = "LGC45",
                                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC45"),
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                    }
                    break;
                }
            }

            //Valida existe Pago Total FETV
            var listPagoTotal = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.NotificacionPagoTotalParcial
            && Convert.ToInt32(t.CustomizationID) == (int)EventCustomization.PaymentBillFTV).ToList();
            if (listPagoTotal != null)
            {
                bool validForItemPago = false;
                foreach (var itemListPagoTotal in listPagoTotal)
                {
                    var documentInfoPago = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListPagoTotal.Identifier, itemListPagoTotal.Identifier, itemListPagoTotal.PartitionKey);
                    if (documentInfoPago != null)
                    {
                        validForItemPago = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC44",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC44"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                    else
                        validForItemPago = true;
                }

                if (validForItemPago)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = true,
                        Mandatory = true,
                        ErrorCode = "100",
                        ErrorMessage = successfulMessage,
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            return responses;
        }
        #endregion

        #region ValidateMandatoCancell
        public List<ValidateListResponse> ValidateMandatoCancell(RequestObjectEventPrev eventPrev)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            //Valida exista mandato            
            List<GlobalDocReferenceAttorney> listDocumentAttorney = documentAttorneyTableManager.FindAll<GlobalDocReferenceAttorney>(eventPrev.TrackId).ToList();
            if (listDocumentAttorney == null || listDocumentAttorney.Count <= 0)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "LGC41",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC41"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }
            else
            {
                bool validItemFor = false;
                foreach (var itemDocumentAttorney in listDocumentAttorney)
                {
                    //Valida exista un mandato vigente activo
                    if (!itemDocumentAttorney.Active)
                    {
                        validItemFor = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC42",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC42"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                    else
                        validItemFor = true;
                }

                if (validItemFor)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = true,
                        Mandatory = true,
                        ErrorCode = "100",
                        ErrorMessage = successfulMessage,
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            return responses;

        }
        #endregion

        #region ValidateNegotiatedInvoiceCancell
        public List<ValidateListResponse> ValidateNegotiatedInvoiceCancell(List<GlobalDocValidatorDocumentMeta> documentMeta)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            //Validacion debe exisitir evento Limitacion de Circulacion 
            var listLimitacionCirculacion = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.NegotiatedInvoice).ToList();
            if (listLimitacionCirculacion == null || listLimitacionCirculacion.Count <= 0)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "LGC34",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC34"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }
            else
            {
                bool validForItem = false;
                foreach (var itemListLimitacionCirculacion in listLimitacionCirculacion)
                {
                    var documentDisponibilizacion = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListLimitacionCirculacion.Identifier, itemListLimitacionCirculacion.Identifier, itemListLimitacionCirculacion.PartitionKey);
                    if (documentDisponibilizacion != null)
                    {
                        validForItem = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = successfulMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                    else
                        validForItem = true;
                }

                if (validForItem)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "LGC34",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC34"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            return responses;
        }
        #endregion

        #region ValidateNegotiatedInvoice
        public List<ValidateListResponse> ValidateNegotiatedInvoice(List<GlobalDocValidatorDocumentMeta> documentMeta)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            //Validacion debe exisitir evento Solicitud de Disponibilización
            var listDisponibilizacion = documentMeta != null ? documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.SolicitudDisponibilizacion
                                                                && (Convert.ToInt32(t.CustomizationID) == (int)EventCustomization.FirstGeneralRegistration
                                                                        || Convert.ToInt32(t.CustomizationID) == (int)EventCustomization.FirstPriorDirectRegistration)
                                                                   ).ToList()
                                                             : null;

            if (listDisponibilizacion == null || listDisponibilizacion.Count <= 0)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "LGC33",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC33"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }
            else
            {
                bool validForItem = false;
                foreach (var itemListDisponibilizacion in listDisponibilizacion)
                {
                    var documentDisponibilizacion = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListDisponibilizacion.Identifier, itemListDisponibilizacion.Identifier, itemListDisponibilizacion.PartitionKey);
                    if (documentDisponibilizacion != null)
                    {
                        validForItem = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = successfulMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                    else
                        validForItem = true;
                }

                if (validForItem)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "LGC33",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC33"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            //Valida existe Pago Total FETV
            var listPagoTotal = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.NotificacionPagoTotalParcial
            && Convert.ToInt32(t.CustomizationID) == (int)EventCustomization.PaymentBillFTV).ToList();
            if (listPagoTotal != null)
            {
                foreach (var itemListPagoTotal in listPagoTotal)
                {
                    var documentInfoPago = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListPagoTotal.Identifier, itemListPagoTotal.Identifier, itemListPagoTotal.PartitionKey);
                    if (documentInfoPago != null)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC92",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC92"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                }
            }

            return responses;
        }
        #endregion

        #region VakidateEndorsementCancell
        public List<ValidateListResponse> ValidateEndorsementCancell(List<GlobalDocValidatorDocumentMeta> documentMeta, RequestObjectEventPrev eventPrev)
        {
            DateTime startDate = DateTime.UtcNow;
            string senderCode = string.Empty;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            responses.Add(new ValidateListResponse
            {
                IsValid = true,
                Mandatory = true,
                ErrorCode = "100",
                ErrorMessage = successfulMessage,
                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
            });

            //Obtiene legitimo tenedor               
            LogicalEventRadian logicalEventRadianRejected = new LogicalEventRadian();
            GlobalDocValidatorDocumentMeta documentMetaCude = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(eventPrev.TrackIdCude, eventPrev.TrackIdCude);
            HolderExchangeModel responseHolderExchange = logicalEventRadianRejected.RetrieveSenderHolderExchange(documentMeta.FirstOrDefault().DocumentReferencedKey, documentMetaCude.TechProviderCode.ToString());
            if (responseHolderExchange != null)
                senderCode = !string.IsNullOrWhiteSpace(responseHolderExchange.PartyLegalEntity) ? responseHolderExchange.PartyLegalEntity : string.Empty;


            //Validación de la existencia de Endoso en Garantia / Cancelacion 401
            if (Convert.ToInt32(eventPrev.CustomizationID) == (int)EventCustomization.CancellationGuaranteeEndorsement)
            {
                //Valida exista endoso en garantia
                bool validForEndoso = false;
                var endosoGarantia = documentMeta.Where(t => (Convert.ToInt32(t.EventCode) == (int)EventStatus.EndosoGarantia)).ToList();
                if (endosoGarantia != null)
                {
                    foreach (var itemEndosoGarantia in endosoGarantia)
                    {
                        var documentGarantia = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemEndosoGarantia.Identifier, itemEndosoGarantia.Identifier, itemEndosoGarantia.PartitionKey);
                        if (documentGarantia != null)
                        {
                            validForEndoso = false;
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = successfulMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                            break;
                        }
                        else
                            validForEndoso = true;
                    }

                    if (validForEndoso)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC46",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC46"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }
                else
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "LGC46",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC46"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            //Validación de la existencia de Endoso en Procuracion / Cancelacion 402
            if (Convert.ToInt32(eventPrev.CustomizationID) == (int)EventCustomization.CancellationEndorsementProcurement)
            {
                //El evento debe informar una nota donde manifieste los motivos de la revocatoria contenida del endoso.
                var responseListEndosoNota = this.ValidateEndosoNota(documentMeta, documentMetaCude.NoteMandato, eventPrev.EventCode);
                if (responseListEndosoNota != null)
                {
                    foreach (var item in responseListEndosoNota)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = item.IsValid,
                            Mandatory = item.Mandatory,
                            ErrorCode = item.ErrorCode,
                            ErrorMessage = item.ErrorMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }

                //Validar exista endoso en procuracion                                   
                bool validForEndosoProcura = false;
                var endosoProcuracion = documentMeta.Where(t => (Convert.ToInt32(t.EventCode) == (int)EventStatus.EndosoProcuracion)).ToList();
                if (endosoProcuracion != null)
                {
                    foreach (var itemEndosoProcuracion in endosoProcuracion)
                    {
                        var documentProcuracion = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemEndosoProcuracion.Identifier, itemEndosoProcuracion.Identifier, itemEndosoProcuracion.PartitionKey);
                        if (documentProcuracion != null)
                        {
                            validForEndosoProcura = false;
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = successfulMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                            break;
                        }
                        else
                            validForEndosoProcura = true;
                    }

                    if (validForEndosoProcura)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC47",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC47"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }
                else
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "LGC47",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC47"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            //Validación de la existencia de Endoso en Propiead, protesto o Limitacion de Circulacion / Cancelacion 401 o 402
            if (Convert.ToInt32(eventPrev.CustomizationID) == (int)EventCustomization.CancellationGuaranteeEndorsement
                || Convert.ToInt32(eventPrev.CustomizationID) == (int)EventCustomization.CancellationEndorsementProcurement)
            {
                //Valida existe limitacion de circulacion
                var limitacionCirculacion = documentMeta.Where(t => (Convert.ToInt32(t.EventCode) == (int)EventStatus.NegotiatedInvoice)).ToList();
                if (limitacionCirculacion != null)
                {
                    foreach (var itemLimitacionCirculacion in limitacionCirculacion)
                    {
                        //Valida si existe terminacion limitacion de circulacion 
                        var terminacionLimitacion = documentMeta.Where(t => (Convert.ToInt32(t.EventCode) == (int)EventStatus.AnulacionLimitacionCirculacion)).ToList();

                        if (terminacionLimitacion != null)
                        {
                            if (terminacionLimitacion.Where(x => x.CancelElectronicEvent.Equals(itemLimitacionCirculacion.PartitionKey)).Any())
                            {
                                break;
                            }
                        }

                        var documentCirculacion = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemLimitacionCirculacion.Identifier, itemLimitacionCirculacion.Identifier, itemLimitacionCirculacion.PartitionKey);
                        if (documentCirculacion != null)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "LGC48",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC48"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                            break;
                        }
                    }
                }

                //Valida existe registro evento protesto
                var listProtesto = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.Objection).ToList();
                if (listProtesto != null)
                {
                    foreach (var itemlistProtesto in listProtesto)
                    {
                        var documentInfoProtesto = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemlistProtesto.Identifier, itemlistProtesto.Identifier, itemlistProtesto.PartitionKey);
                        if (documentInfoProtesto != null)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "LGC68",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC68"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                            break;
                        }
                    }
                }


                //Valida existe endoso en propiedad
                var endosoPropiedad = documentMeta.Where(t => (Convert.ToInt32(t.EventCode) == (int)EventStatus.EndosoPropiedad)).OrderByDescending(x => x.SigningTimeStamp).ToList();
                if (endosoPropiedad != null)
                {
                    foreach (var itemEndosoPropiedad in endosoPropiedad)
                    {
                        var documentPropiedad = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemEndosoPropiedad.Identifier, itemEndosoPropiedad.Identifier, itemEndosoPropiedad.PartitionKey);
                        if (documentPropiedad != null)
                        {
                            bool validforDisponibiliza = false;
                            //Valida existe disponibilizacion posterior 363 /364
                            var listDisponibilizacion = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.SolicitudDisponibilizacion
                            && (Convert.ToInt32(t.CustomizationID) == (int)EventCustomization.GeneralSubsequentRegistration ||
                                    Convert.ToInt32(t.CustomizationID) == (int)EventCustomization.PriorDirectSubsequentEnrollment)
                                ).OrderByDescending(x => x.SigningTimeStamp).ToList();
                            if (listDisponibilizacion != null || listDisponibilizacion.Count > 0)
                            {
                                foreach (var itemListDisponibilizacion in listDisponibilizacion)
                                {
                                    var documentDisponibiliza = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListDisponibilizacion.Identifier, itemListDisponibilizacion.Identifier, itemListDisponibilizacion.PartitionKey);
                                    if (documentDisponibiliza != null)
                                    {
                                        //Valida fecha endoso en propiedad es mayor a fecha disponibilizacion  
                                        senderCode = string.IsNullOrEmpty(senderCode) ? itemListDisponibilizacion.SenderCode : senderCode;

                                        if (Convert.ToDateTime(itemEndosoPropiedad.SigningTimeStamp) > Convert.ToDateTime(itemListDisponibilizacion.SigningTimeStamp)
                                            && itemEndosoPropiedad.SenderCode == senderCode)
                                        {
                                            validforDisponibiliza = true;
                                            responses.Add(new ValidateListResponse
                                            {
                                                IsValid = false,
                                                Mandatory = true,
                                                ErrorCode = "LGC49",
                                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC49"),
                                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                            });
                                            break;
                                        }
                                    }
                                }
                            }

                            if (validforDisponibiliza)
                                break;
                        }
                    }
                }
            }

            return responses;
        }
        #endregion

        #region RetrieveSenderHolderExchange
        public HolderExchangeModel RetrieveSenderHolderExchange(string cufe, string providerCode)
        {
            HolderExchangeModel response = new HolderExchangeModel();

            //Consulta legitimo tenedor
            GlobalDocHolderExchange documentHolderExchange = documentHolderExchangeTableManager.FindhByCufeExchange<GlobalDocHolderExchange>(cufe, true);
            if (documentHolderExchange != null)
            {
                string[] endosatarios = new string[0];
                bool validFor = true;
                endosatarios = documentHolderExchange.PartyLegalEntity.Split('|');
                if (endosatarios.Length == 1)
                {
                    response.PartyLegalEntity = documentHolderExchange.PartyLegalEntity;
                }
                else
                {
                    //Valida exista mandatario representante para cada legitimo tenedor
                    foreach (string endosatario in endosatarios)
                    {
                        GlobalDocReferenceAttorney documentAttorney = documentAttorneyTableManager.FindhByCufeSenderAttorney<GlobalDocReferenceAttorney>(cufe, endosatario, providerCode);
                        if (documentAttorney == null)
                        {
                            validFor = false;
                            response.PartyLegalEntity = string.Empty;
                        }
                    }

                    if (validFor)
                        response.PartyLegalEntity = providerCode;
                }

                response.PartitionKey = documentHolderExchange.PartitionKey;
                response.RowKey = documentHolderExchange.RowKey;
                response.CorporateStockAmount = documentHolderExchange.CorporateStockAmount;
                response.CorporateStockAmountSender = documentHolderExchange.CorporateStockAmountSender;
                response.GlobalDocumentId = documentHolderExchange.GlobalDocumentId;
                response.SenderCode = documentHolderExchange.SenderCode;
            }
            else
                return null;

            return response;
        }
        #endregion

        #region ValidateExistPropertyEndorsement     
        private List<ValidateListResponse> ValidateExistEndorsement(List<GlobalDocValidatorDocumentMeta> documentMetaList, XmlParser xmlParserCude, NitModel nitModel, string senderCode)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            bool validFor = true;
            string errorCode = Convert.ToInt32(nitModel.ResponseCode) == (int)EventStatus.EndosoGarantia ? "LGC27" : "LGC30";
            string messageCode = Convert.ToInt32(nitModel.ResponseCode) == (int)EventStatus.EndosoGarantia ? ConfigurationManager.GetValue("ErrorMessage_LGC27") : ConfigurationManager.GetValue("ErrorMessage_LGC30");

            responses.Add(new ValidateListResponse
            {
                IsValid = true,
                Mandatory = true,
                ErrorCode = "100",
                ErrorMessage = successfulMessage,
                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
            });

            //Consulta existe endoso
            var documentMeta = documentMetaList.Where(x => Convert.ToInt32(x.EventCode) == (int)EventStatus.EndosoPropiedad
            || Convert.ToInt32(x.EventCode) == (int)EventStatus.EndosoProcuracion
            || Convert.ToInt32(x.EventCode) == (int)EventStatus.EndosoGarantia).OrderByDescending(x => x.SigningTimeStamp).FirstOrDefault();
            if (documentMeta != null)
            {
                var document = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(documentMeta.Identifier, documentMeta.Identifier, documentMeta.PartitionKey);
                if (document != null)
                {
                    //Consulta legitimo tenedor
                    GlobalDocHolderExchange documentHolderExchange = documentHolderExchangeTableManager.FindhByCufeExchange<GlobalDocHolderExchange>(documentMeta.DocumentReferencedKey, true);

                    if (documentHolderExchange != null)
                    {
                        //Existe mas de un legitimo tenedor requiere un mandatario
                        string[] endosatarios = documentHolderExchange.PartyLegalEntity.Split('|');
                        if (endosatarios.Length == 1)
                        {
                            senderCode = documentHolderExchange.PartyLegalEntity;
                        }
                        else
                        {
                            //Valida exista mandatario representante para cada legitimo tenedor
                            foreach (string endosatario in endosatarios)
                            {
                                GlobalDocReferenceAttorney documentAttorney = documentAttorneyTableManager.FindhByCufeSenderAttorney<GlobalDocReferenceAttorney>(documentMeta.PartitionKey, endosatario, xmlParserCude.ProviderCode);
                                if (documentAttorney == null)
                                {
                                    validFor = false;
                                }
                            }

                            if (validFor)
                            {
                                senderCode = xmlParserCude.ProviderCode;
                            }
                        }
                    }

                    if (validFor)
                    {
                        //Valida existe disponibilizacion posterior
                        DateTime dateEndorsement = documentMeta.SigningTimeStamp;
                        var listDisponibilizacion = documentMetaList.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.SolicitudDisponibilizacion
                        && (Convert.ToInt32(t.CustomizationID) == (int)EventCustomization.GeneralSubsequentRegistration ||
                                Convert.ToInt32(t.CustomizationID) == (int)EventCustomization.PriorDirectSubsequentEnrollment) && t.SenderCode == senderCode
                           && t.SigningTimeStamp > dateEndorsement).FirstOrDefault();
                        if (listDisponibilizacion == null)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = errorCode,
                                ErrorMessage = messageCode,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                        }
                    }
                    else
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = errorCode,
                            ErrorMessage = messageCode,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }
            }

            return responses;
        }
        #endregion

        #region ValidateEndorsementProcurement
        public List<ValidateListResponse> ValidateEndorsementProcurement(List<GlobalDocValidatorDocumentMeta> documentMeta, RequestObjectEventPrev eventPrev, XmlParser xmlParserCude, NitModel nitModel)
        {
            DateTime startDate = DateTime.UtcNow;
            string senderCode = string.Empty;
            string eventCode = eventPrev.EventCode;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            bool validForItem = false;

            responses.Add(new ValidateListResponse
            {
                IsValid = true,
                Mandatory = true,
                ErrorCode = "100",
                ErrorMessage = successfulMessage,
                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
            });

            //Si el endoso esta en blanco o el senderCode es diferente a providerCode                
            nitModel.SenderCode = (nitModel.listID == "2" || (nitModel.SenderCode != nitModel.ProviderCode)) ? nitModel.ProviderCode : nitModel.SenderCode;
            senderCode = nitModel.listID == "1" ? xmlParserCude.Fields["SenderCode"].ToString() : nitModel.SenderCode;

            //Valida existe endoso en propiedad cambio legitimo tenedor para disponibilizacion posterior
            var responseExistPropertyEndorsement = ValidateExistEndorsement(documentMeta, xmlParserCude, nitModel, senderCode);
            if (responseExistPropertyEndorsement != null)
            {
                foreach (var item in responseExistPropertyEndorsement)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = item.IsValid,
                        Mandatory = item.Mandatory,
                        ErrorCode = item.ErrorCode,
                        ErrorMessage = item.ErrorMessage,
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }


            //Validacion debe exisitir evento Solicitud de Disponibilización
            var listDisponibilizacion = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.SolicitudDisponibilizacion && t.SenderCode == senderCode).ToList();
            if (listDisponibilizacion == null || listDisponibilizacion.Count <= 0)
                validForItem = true;

            if (listDisponibilizacion != null)
            {
                foreach (var itemListDisponibilizacion in listDisponibilizacion)
                {
                    var documentDisponibilizacion = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListDisponibilizacion.Identifier, itemListDisponibilizacion.Identifier, itemListDisponibilizacion.PartitionKey);
                    if (documentDisponibilizacion != null)
                    {
                        validForItem = false;
                        var newAmountTV = documentMeta.OrderByDescending(t => t.SigningTimeStamp).FirstOrDefault(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.SolicitudDisponibilizacion).NewAmountTV;
                        var responseListEndoso = ValidateEndoso(xmlParserCude, nitModel, eventCode, newAmountTV);
                        if (responseListEndoso != null)
                        {
                            foreach (var item in responseListEndoso)
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = item.IsValid,
                                    Mandatory = item.Mandatory,
                                    ErrorCode = item.ErrorCode,
                                    ErrorMessage = item.ErrorMessage,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                        break;
                    }
                    else
                        validForItem = true;
                }

                if (validForItem)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "LGC30",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC30"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }

            }
            else
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "LGC30",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC30"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            //Valida existe Pago Total FETV
            var listPagoTotal = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.NotificacionPagoTotalParcial
            && Convert.ToInt32(t.CustomizationID) == (int)EventCustomization.PaymentBillFTV).ToList();
            if (listPagoTotal != null)
            {
                foreach (var itemListPagoTotal in listPagoTotal)
                {
                    var documentInfoPago = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListPagoTotal.Identifier, itemListPagoTotal.Identifier, itemListPagoTotal.PartitionKey);
                    if (documentInfoPago != null)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC32",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC32"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                }
            }

            //Valida existen Limitaciones activas
            string existCancelElectronicEvent = string.Empty;
            var listLimitaciones = documentMeta.Where(t => (Convert.ToInt32(t.EventCode) == (int)EventStatus.EndosoGarantia
            || Convert.ToInt32(t.EventCode) == (int)EventStatus.NegotiatedInvoice
            || Convert.ToInt32(t.EventCode) == (int)EventStatus.EndorsementWithEffectOrdinaryAssignment
            || Convert.ToInt32(t.EventCode) == (int)EventStatus.Objection
            || Convert.ToInt32(t.EventCode) == (int)EventStatus.TransferEconomicRights
            )).ToList();
            if (listLimitaciones != null)
            {
                foreach (var itemListLimitaciones in listLimitaciones)
                {
                    //Valida exista limitacion aprobada
                    var documentLimitacion = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListLimitaciones.Identifier, itemListLimitaciones.Identifier, itemListLimitaciones.PartitionKey);
                    if (documentLimitacion != null)
                    {
                        //Valida si existe cancelacion de la limitacion
                        existCancelElectronicEvent = ValidateCancelElectronicEvent(documentMeta, documentLimitacion.DocumentKey, senderCode);
                        if (string.IsNullOrWhiteSpace(existCancelElectronicEvent))
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "LGC31",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC31"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                            break;
                        }
                    }
                }
            }

            return responses;
        }
        #endregion

        #region ValidateEndorsementGatantia
        public List<ValidateListResponse> ValidateEndorsementGatantia(List<GlobalDocValidatorDocumentMeta> documentMeta, RequestObjectEventPrev eventPrev, XmlParser xmlParserCude, NitModel nitModel)
        {
            DateTime startDate = DateTime.UtcNow;
            string senderCode = string.Empty;
            string eventCode = eventPrev.EventCode;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            bool validForItem = false;

            responses.Add(new ValidateListResponse
            {
                IsValid = true,
                Mandatory = true,
                ErrorCode = "100",
                ErrorMessage = successfulMessage,
                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
            });

            //Si el endoso esta en blanco o el senderCode es diferente a providerCode                
            nitModel.SenderCode = (nitModel.listID == "2" || (nitModel.SenderCode != nitModel.ProviderCode)) ? nitModel.ProviderCode : nitModel.SenderCode;
            senderCode = nitModel.listID == "1" ? xmlParserCude.Fields["SenderCode"].ToString() : nitModel.SenderCode;

            //Valida existe endoso en propiedad cambio legitimo tenedor para disponibilizacion posterior
            var responseExistPropertyEndorsement = ValidateExistEndorsement(documentMeta, xmlParserCude, nitModel, senderCode);
            if (responseExistPropertyEndorsement != null)
            {
                foreach (var item in responseExistPropertyEndorsement)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = item.IsValid,
                        Mandatory = item.Mandatory,
                        ErrorCode = item.ErrorCode,
                        ErrorMessage = item.ErrorMessage,
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            //Validacion debe exisitir evento Solicitud de Disponibilización
            var listDisponibilizacion = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.SolicitudDisponibilizacion && t.SenderCode == senderCode).ToList();
            if (listDisponibilizacion == null || listDisponibilizacion.Count <= 0)
                validForItem = true;

            if (listDisponibilizacion != null)
            {
                foreach (var itemListDisponibilizacion in listDisponibilizacion)
                {
                    var documentDisponibilizacion = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListDisponibilizacion.Identifier, itemListDisponibilizacion.Identifier, itemListDisponibilizacion.PartitionKey);
                    if (documentDisponibilizacion != null)
                    {
                        validForItem = false;
                        var newAmountTV = documentMeta.OrderByDescending(t => t.SigningTimeStamp).FirstOrDefault(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.SolicitudDisponibilizacion).NewAmountTV;
                        var responseListEndoso = ValidateEndoso(xmlParserCude, nitModel, eventCode, newAmountTV);
                        if (responseListEndoso != null)
                        {
                            foreach (var item in responseListEndoso)
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = item.IsValid,
                                    Mandatory = item.Mandatory,
                                    ErrorCode = item.ErrorCode,
                                    ErrorMessage = item.ErrorMessage,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                        break;
                    }
                    else
                        validForItem = true;
                }

                if (validForItem)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "LGC27",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC27"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }

            }
            else
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "LGC27",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC27"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            //Valida existe Pago Total FETV
            var listPagoTotal = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.NotificacionPagoTotalParcial
            && Convert.ToInt32(t.CustomizationID) == (int)EventCustomization.PaymentBillFTV).ToList();
            if (listPagoTotal != null)
            {
                foreach (var itemListPagoTotal in listPagoTotal)
                {
                    var documentInfoPago = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListPagoTotal.Identifier, itemListPagoTotal.Identifier, itemListPagoTotal.PartitionKey);
                    if (documentInfoPago != null)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC29",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC29"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                }
            }

            //Valida existen Limitaciones activas
            string existCancelElectronicEvent = string.Empty;
            var listLimitaciones = documentMeta.Where(t => (Convert.ToInt32(t.EventCode) == (int)EventStatus.EndosoProcuracion
            || Convert.ToInt32(t.EventCode) == (int)EventStatus.NegotiatedInvoice
            || Convert.ToInt32(t.EventCode) == (int)EventStatus.EndorsementWithEffectOrdinaryAssignment
            || Convert.ToInt32(t.EventCode) == (int)EventStatus.Objection
            || Convert.ToInt32(t.EventCode) == (int)EventStatus.TransferEconomicRights
            )).ToList();
            if (listLimitaciones != null)
            {
                foreach (var itemListLimitaciones in listLimitaciones)
                {
                    //Valida exista limitacion aprobada
                    var documentLimitacion = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListLimitaciones.Identifier, itemListLimitaciones.Identifier, itemListLimitaciones.PartitionKey);
                    if (documentLimitacion != null)
                    {
                        existCancelElectronicEvent = ValidateCancelElectronicEvent(documentMeta, documentLimitacion.DocumentKey, senderCode);
                        if (string.IsNullOrWhiteSpace(existCancelElectronicEvent))
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "LGC28",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC28"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                            break;
                        }
                    }
                }
            }

            return responses;
        }
        #endregion

        #region ValidatePropertyEndorsement
        public List<ValidateListResponse> ValidatePropertyEndorsement(List<GlobalDocValidatorDocumentMeta> documentMeta, RequestObjectEventPrev eventPrev, XmlParser xmlParserCude, NitModel nitModel)
        {
            DateTime startDate = DateTime.UtcNow;
            string senderCode = string.Empty;
            string eventCode = eventPrev.EventCode;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            bool validForItem = false;

            responses.Add(new ValidateListResponse
            {
                IsValid = true,
                Mandatory = true,
                ErrorCode = "100",
                ErrorMessage = successfulMessage,
                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
            });

            //Si el endoso esta en blanco o el senderCode es diferente a providerCode                
            nitModel.SenderCode = (nitModel.listID == "2" || (nitModel.SenderCode != nitModel.ProviderCode)) ? nitModel.ProviderCode : nitModel.SenderCode;
            senderCode = nitModel.listID == "1" ? xmlParserCude.Fields["SenderCode"].ToString() : nitModel.SenderCode;

            //Validacion debe exisitir evento Solicitud de Disponibilización
            var listDisponibilizacion = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.SolicitudDisponibilizacion && t.SenderCode == senderCode).ToList();
            if (listDisponibilizacion == null || listDisponibilizacion.Count <= 0)
                validForItem = true;

            if (listDisponibilizacion != null)
            {
                foreach (var itemListDisponibilizacion in listDisponibilizacion)
                {
                    var documentDisponibilizacion = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListDisponibilizacion.Identifier, itemListDisponibilizacion.Identifier, itemListDisponibilizacion.PartitionKey);
                    if (documentDisponibilizacion != null)
                    {
                        validForItem = false;
                        var newAmountTV = documentMeta.OrderByDescending(t => t.SigningTimeStamp).FirstOrDefault(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.SolicitudDisponibilizacion).NewAmountTV;
                        var responseListEndoso = ValidateEndoso(xmlParserCude, nitModel, eventCode, newAmountTV);
                        if (responseListEndoso != null)
                        {
                            foreach (var item in responseListEndoso)
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = item.IsValid,
                                    Mandatory = item.Mandatory,
                                    ErrorCode = item.ErrorCode,
                                    ErrorMessage = item.ErrorMessage,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                        break;
                    }
                    else
                        validForItem = true;
                }

                if (validForItem)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "LGC24",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC24"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }

            }
            else
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "LGC24",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC24"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            //Valida existe Pago Total FETV
            var listPagoTotal = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.NotificacionPagoTotalParcial
            && Convert.ToInt32(t.CustomizationID) == (int)EventCustomization.PaymentBillFTV).ToList();
            if (listPagoTotal != null)
            {
                foreach (var itemListPagoTotal in listPagoTotal)
                {
                    var documentInfoPago = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListPagoTotal.Identifier, itemListPagoTotal.Identifier, itemListPagoTotal.PartitionKey);
                    if (documentInfoPago != null)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC26",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC26"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                }
            }

            //Valida existen Limitaciones activas
            string existCancelElectronicEvent = string.Empty;
            var listLimitaciones = documentMeta.Where(t => (Convert.ToInt32(t.EventCode) == (int)EventStatus.EndosoGarantia
            || Convert.ToInt32(t.EventCode) == (int)EventStatus.EndosoProcuracion
            || Convert.ToInt32(t.EventCode) == (int)EventStatus.NegotiatedInvoice
            || Convert.ToInt32(t.EventCode) == (int)EventStatus.EndorsementWithEffectOrdinaryAssignment
            || Convert.ToInt32(t.EventCode) == (int)EventStatus.Objection
            || Convert.ToInt32(t.EventCode) == (int)EventStatus.TransferEconomicRights
            )).ToList();
            if (listLimitaciones != null)
            {
                foreach (var itemListLimitaciones in listLimitaciones)
                {
                    //Valida exista limitacion aprobada
                    var documentLimitacion = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListLimitaciones.Identifier, itemListLimitaciones.Identifier, itemListLimitaciones.PartitionKey);
                    if (documentLimitacion != null)
                    {
                        existCancelElectronicEvent = ValidateCancelElectronicEvent(documentMeta, documentLimitacion.DocumentKey, senderCode);
                        if (string.IsNullOrWhiteSpace(existCancelElectronicEvent))
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "LGC25",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC25"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                            break;
                        }
                    }
                }
            }

            return responses;
        }
        #endregion

        #region ValidateEndorsementEventPrev
        public List<ValidateListResponse> ValidateEndorsementEventPrev(List<GlobalDocValidatorDocumentMeta> documentMeta, string totalInvoice, XmlParser xmlParserCude)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            //Validacion debe exisitir evento Solicitud de Disponibilización
            var listDisponibilizacion = documentMeta != null ? documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.SolicitudDisponibilizacion).ToList() : null;

            if (listDisponibilizacion != null && listDisponibilizacion.Count() > 0)
            {
                bool validForItem = false;
                foreach (var itemListDisponibilizacion in listDisponibilizacion)
                {
                    var documentDisponibilizacion = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListDisponibilizacion.Identifier, itemListDisponibilizacion.Identifier, itemListDisponibilizacion.PartitionKey);
                    if (documentDisponibilizacion != null)
                    {
                        validForItem = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = successfulMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });

                        var responseListAval = ValidateAval(totalInvoice, xmlParserCude);
                        if (responseListAval != null)
                        {
                            foreach (var item in responseListAval)
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = item.IsValid,
                                    Mandatory = item.Mandatory,
                                    ErrorCode = item.ErrorCode,
                                    ErrorMessage = item.ErrorMessage,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }

                        //Valida no tenga Limitaciones la FETV
                        string existCancelElectronicEvent = string.Empty;
                        string senderCode = xmlParserCude.Fields["SenderCode"].ToString();
                        var listLimitacion = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.NegotiatedInvoice).ToList();
                        if (listLimitacion != null)
                        {
                            foreach (var itemListLimitacion in listLimitacion)
                            {
                                var documentLimitacion = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListLimitacion.Identifier, itemListLimitacion.Identifier, itemListLimitacion.PartitionKey);
                                if (documentLimitacion != null)
                                {
                                    existCancelElectronicEvent = ValidateCancelElectronicEvent(documentMeta, documentLimitacion.DocumentKey, senderCode);
                                    if (string.IsNullOrWhiteSpace(existCancelElectronicEvent))
                                    {
                                        responses.Add(new ValidateListResponse
                                        {
                                            IsValid = false,
                                            Mandatory = true,
                                            ErrorCode = "LGC39",
                                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC39"),
                                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                        });
                                        break;
                                    }
                                }
                            }
                        }

                        //Valida existe Pago Total FETV
                        var listPagoTotal = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.NotificacionPagoTotalParcial
                        && Convert.ToInt32(t.CustomizationID) == (int)EventCustomization.PaymentBillFTV).ToList();
                        if (listPagoTotal != null)
                        {
                            foreach (var itemListPagoTotal in listPagoTotal)
                            {
                                var documentInfoPago = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListPagoTotal.Identifier, itemListPagoTotal.Identifier, itemListPagoTotal.PartitionKey);
                                if (documentInfoPago != null)
                                {
                                    responses.Add(new ValidateListResponse
                                    {
                                        IsValid = false,
                                        Mandatory = true,
                                        ErrorCode = "LGC40",
                                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC40"),
                                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                    });
                                    break;
                                }
                            }
                        }

                        //Valida existe registrado evento protesto
                        var listProtesto = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.Objection).ToList();
                        if (listProtesto != null)
                        {
                            foreach (var itemlistProtesto in listProtesto)
                            {
                                var documentInfoProtesto = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemlistProtesto.Identifier, itemlistProtesto.Identifier, itemlistProtesto.PartitionKey);
                                if (documentInfoProtesto != null)
                                {
                                    responses.Add(new ValidateListResponse
                                    {
                                        IsValid = false,
                                        Mandatory = true,
                                        ErrorCode = "LGC66",
                                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC66"),
                                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                    });
                                    break;
                                }
                            }
                        }

                        //Valida existe registrado transferencia de los derechos economicos
                        var listTransferencia = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.TransferEconomicRights).ToList();
                        if (listTransferencia != null)
                        {
                            foreach (var itemlistTransferencia in listTransferencia)
                            {
                                var documentInfoTransferencia = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemlistTransferencia.Identifier, itemlistTransferencia.Identifier, itemlistTransferencia.PartitionKey);
                                if (documentInfoTransferencia != null)
                                {
                                    responses.Add(new ValidateListResponse
                                    {
                                        IsValid = false,
                                        Mandatory = true,
                                        ErrorCode = "LGC67",
                                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC67"),
                                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                    });
                                    break;
                                }
                            }
                        }

                        break;
                    }
                    else
                        validForItem = true;
                }

                if (validForItem)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "LGC38",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC38"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }
            else
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "LGC38",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC38"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            return responses;
        }
        #endregion

        #region ValidateAvailabilityRequest
        public List<ValidateListResponse> ValidateAvailabilityRequestEventPrev(List<GlobalDocValidatorDocumentMeta> documentMeta, string totalInvoice, XmlParser xmlParserCude, NitModel nitModel)
        {
            DateTime startDate = DateTime.UtcNow;
            string senderCode = string.Empty;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            //Obtiene legitimo tenedor               
            LogicalEventRadian logicalEventRadianRejected = new LogicalEventRadian();
            HolderExchangeModel responseHolderExchange = logicalEventRadianRejected.RetrieveSenderHolderExchange(documentMeta.FirstOrDefault().DocumentReferencedKey, xmlParserCude.ProviderCode.ToString());
            if (responseHolderExchange != null)
                senderCode = !string.IsNullOrWhiteSpace(responseHolderExchange.PartyLegalEntity) ? responseHolderExchange.PartyLegalEntity : string.Empty;
            else
                senderCode = xmlParserCude.Fields["SenderCode"].ToString();

            //Validacion de la Solicitud de Disponibilización Posterior 361 / 362
            if (Convert.ToInt32(nitModel.CustomizationId) == (int)EventCustomization.FirstGeneralRegistration
               || Convert.ToInt32(nitModel.CustomizationId) == (int)EventCustomization.FirstPriorDirectRegistration)
            {
                //Validacion Inscripciones 
                var listInscripciones = documentMeta.Where(x => Convert.ToInt32(x.EventCode) == (int)EventStatus.SolicitudDisponibilizacion
                && (Convert.ToInt32(x.CustomizationID) == (int)EventCustomization.FirstGeneralRegistration
               || Convert.ToInt32(x.CustomizationID) == (int)EventCustomization.FirstPriorDirectRegistration)).ToList();
                if (listInscripciones != null || listInscripciones.Count > 0)
                {
                    foreach (var itemListInscripciones in listInscripciones)
                    {
                        var documentDisponibilizacionPrimera = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListInscripciones.Identifier, itemListInscripciones.Identifier, itemListInscripciones.PartitionKey);
                        if (documentDisponibilizacionPrimera != null)
                        {

                            if (Convert.ToInt32(itemListInscripciones.CustomizationID) == Convert.ToInt32(nitModel.CustomizationId))
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = false,
                                    Mandatory = true,
                                    ErrorCode = "LGC23",
                                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC23"),
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                                break;
                            }
                        }
                    }
                }


                //Validacion Endosos aprobados
                var listEndoso = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.EndosoPropiedad
                    || (Convert.ToInt32(t.EventCode) == (int)EventStatus.EndosoGarantia)
                    || Convert.ToInt32(t.EventCode) == (int)EventStatus.EndosoProcuracion).ToList();

                if (listEndoso == null || listEndoso.Count <= 0)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = true,
                        Mandatory = true,
                        ErrorCode = "100",
                        ErrorMessage = successfulMessage,
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
                else
                {
                    foreach (var itemListEndoso in listEndoso)
                    {
                        var documentDisponibilizacionPosterior = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListEndoso.Identifier, itemListEndoso.Identifier, itemListEndoso.PartitionKey);
                        if (documentDisponibilizacionPosterior != null)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "LGC20",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC20"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                            break;
                        }
                    }
                }
            }

            //Validacion de la Solicitud de Disponibilización Posterior 363 / 364
            if (Convert.ToInt32(nitModel.CustomizationId) == (int)EventCustomization.GeneralSubsequentRegistration
                || Convert.ToInt32(nitModel.CustomizationId) == (int)EventCustomization.PriorDirectSubsequentEnrollment)
            {
                //Valida existen endosos
                bool validateExistEndoso = false;
                string validCancelElectronicEvent = string.Empty;
                var existEndosos = documentMeta.Where(t => (Convert.ToInt32(t.EventCode) == (int)EventStatus.EndosoGarantia
                           || Convert.ToInt32(t.EventCode) == (int)EventStatus.EndosoProcuracion)).ToList();
                if (existEndosos != null)
                {
                    foreach (var itemExistEndosos in existEndosos)
                    {
                        var existDocumentEndosos = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemExistEndosos.Identifier, itemExistEndosos.Identifier, itemExistEndosos.PartitionKey);
                        //Valida existe cancelancion limitacion                      
                        if (existDocumentEndosos != null)
                        {
                            validCancelElectronicEvent = ValidateCancelElectronicEvent(documentMeta, existDocumentEndosos.DocumentKey, senderCode);
                            if (!string.IsNullOrWhiteSpace(validCancelElectronicEvent)) validateExistEndoso = true;
                            break;
                        }
                    }
                }

                //Valida exista previamnete un registro de endoso en propiedad
                var listEndosoPropiedad = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.EndosoPropiedad).ToList();
                if (listEndosoPropiedad == null || listEndosoPropiedad.Count <= 0 && !validateExistEndoso)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "LGC37",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC37"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
                else
                {
                    bool validForItem = false;
                    foreach (var itemListEndosoPropiedad in listEndosoPropiedad)
                    {
                        //Valida exista previamnete un registro de endoso en propiedad aprobado
                        var documentEndosoPropiedad = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListEndosoPropiedad.Identifier, itemListEndosoPropiedad.Identifier, itemListEndosoPropiedad.PartitionKey);
                        if (documentEndosoPropiedad != null)
                        {
                            validForItem = false;
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = true,
                                Mandatory = true,
                                ErrorCode = "100",
                                ErrorMessage = successfulMessage,
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                            break;
                        }
                        else
                            validForItem = true;
                    }

                    if (validForItem && !validateExistEndoso)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC37",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC37"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }

                //Valida que exista una Primera Disponibilizacion
                bool validForItemDisponibiliza = false;

                var listPrimeraDisponibilizacion = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.SolicitudDisponibilizacion
                && t.SenderCode == senderCode
                && (Convert.ToInt32(t.CustomizationID) == (int)EventCustomization.FirstGeneralRegistration)
                || Convert.ToInt32(t.CustomizationID) == (int)EventCustomization.FirstPriorDirectRegistration).ToList();
                if (listPrimeraDisponibilizacion != null || listPrimeraDisponibilizacion.Count > 0)
                {
                    //Valida que exista una Primera Disponibilizacion aprobado                    
                    foreach (var itemListPrimeraDisponibilizacion in listPrimeraDisponibilizacion)
                    {
                        var documentPrimeraDisponibilizacion = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListPrimeraDisponibilizacion.Identifier, itemListPrimeraDisponibilizacion.Identifier, itemListPrimeraDisponibilizacion.PartitionKey);
                        if (documentPrimeraDisponibilizacion != null)
                        {
                            validForItemDisponibiliza = false;
                            //Valida existen Limitaciones activas
                            var listLimitaciones = documentMeta.Where(t => (Convert.ToInt32(t.EventCode) == (int)EventStatus.EndosoGarantia
                            || Convert.ToInt32(t.EventCode) == (int)EventStatus.EndosoProcuracion
                            || Convert.ToInt32(t.EventCode) == (int)EventStatus.NegotiatedInvoice
                            || Convert.ToInt32(t.EventCode) == (int)EventStatus.EndorsementWithEffectOrdinaryAssignment
                            || Convert.ToInt32(t.EventCode) == (int)EventStatus.Objection
                            || Convert.ToInt32(t.EventCode) == (int)EventStatus.TransferEconomicRights
                            || (Convert.ToInt32(t.EventCode) == (int)EventStatus.NotificacionPagoTotalParcial
                            && Convert.ToInt32(t.CustomizationID) == (int)SubEventStatus.PagoTotal)
                            )).ToList();
                            if (listLimitaciones != null || listLimitaciones.Count > 0)
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = true,
                                    Mandatory = true,
                                    ErrorCode = "100",
                                    ErrorMessage = successfulMessage,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });

                                foreach (var itemListLimitaciones in listLimitaciones)
                                {
                                    //Valida exista limitacion aprobada
                                    var documentLimitacion = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListLimitaciones.Identifier, itemListLimitaciones.Identifier, itemListLimitaciones.PartitionKey);
                                    if (documentLimitacion != null)
                                    {
                                        //Valida existe cancelancion limitacion
                                        validCancelElectronicEvent = ValidateCancelElectronicEvent(documentMeta, documentLimitacion.DocumentKey, senderCode);
                                        if (string.IsNullOrWhiteSpace(validCancelElectronicEvent))
                                        {
                                            responses.Add(new ValidateListResponse
                                            {
                                                IsValid = false,
                                                Mandatory = true,
                                                ErrorCode = "LGC22",
                                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC22"),
                                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                            });
                                            break;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                        else
                            validForItemDisponibiliza = true;
                    }

                    if (validForItemDisponibiliza)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC21",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC21"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }
                else
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "LGC21",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC21"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }
            else
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = true,
                    Mandatory = true,
                    ErrorCode = "100",
                    ErrorMessage = successfulMessage,
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            // Comparar ValorFEV-TV contra el valor total de la FE
            if (xmlParserCude.ValorOriginalTV != null)
            {
                var valorOriginlTV = double.Parse(xmlParserCude.ValorOriginalTV, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture).ToString();
                var totalInvoiceParam = double.Parse(totalInvoice, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture).ToString();
                if (valorOriginlTV == totalInvoiceParam)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = true,
                        Mandatory = true,
                        ErrorCode = "100",
                        ErrorMessage = successfulMessage,
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
                else
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = false,
                        ErrorCode = "LGC57",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC57"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }
            else
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "LGC57",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC57"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            //Valida existe previamente el evento transferencia de los derechos económicos 
            var transferencia = documentMeta != null ? documentMeta.OrderByDescending(t => t.SigningTimeStamp).FirstOrDefault(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.TransferEconomicRights) : null;
            if (transferencia != null)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "LGC87",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC87"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            return responses;
        }
        #endregion

        #region ValidateCancelElectronicEvent
        public string ValidateCancelElectronicEvent(List<GlobalDocValidatorDocumentMeta> documentMeta, string cancelElectronicEvent, string SenderCode)
        {
            string eventCanelLimitacion = string.Empty;

            var listCancelElectronicEvent = documentMeta.Where(t => t.CancelElectronicEvent == cancelElectronicEvent).ToList();
            if (listCancelElectronicEvent != null)
            {
                foreach (var itemListCancelElectronicEvent in listCancelElectronicEvent)
                {
                    var documentCancelaEvento = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListCancelElectronicEvent.Identifier, itemListCancelElectronicEvent.Identifier, itemListCancelElectronicEvent.PartitionKey);
                    if (documentCancelaEvento != null)
                    {
                        eventCanelLimitacion = documentCancelaEvento.DocumentKey;
                        break;
                    }
                }
            }

            return eventCanelLimitacion;
        }

        #endregion

        #region ValidateTacitAcceptance
        public List<ValidateListResponse> ValidateTacitAcceptanceEventPrev(List<GlobalDocValidatorDocumentMeta> documentMeta)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            //Debe existir Recibo del bien y/o prestación del servicio                              
            var listReciboBien = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.Receipt).ToList();
            if (listReciboBien == null || listReciboBien.Count <= 0)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "LGC14",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC14"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }
            else
            {
                bool validForItem = false;
                foreach (var itemReciboBien in listReciboBien)
                {
                    var documentReceiptBien = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemReciboBien.Identifier, itemReciboBien.Identifier, itemReciboBien.PartitionKey);
                    if (documentReceiptBien != null)
                    {
                        validForItem = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = successfulMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });

                        //Valida exista evento Reclamo de la Factura Electrónica de Venta
                        var listRejected = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.Rejected).ToList();
                        if (listRejected != null)
                        {
                            foreach (var itemRejected in listRejected)
                            {
                                var documentRejected = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemRejected.Identifier, itemRejected.Identifier, itemRejected.PartitionKey);
                                if (documentRejected != null)
                                {
                                    responses.Add(new ValidateListResponse
                                    {
                                        IsValid = false,
                                        Mandatory = true,
                                        ErrorCode = "LGC04",
                                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC04"),
                                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                    });
                                    break;
                                }
                            }
                        }

                        //Valida exista evento Aceptación Expresa
                        var listAceptacionExpresa = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.Accepted).ToList();
                        if (listAceptacionExpresa != null)
                        {
                            foreach (var itemAceptacionExpresa in listAceptacionExpresa)
                            {
                                var documentAceptacionExpresa = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemAceptacionExpresa.Identifier, itemAceptacionExpresa.Identifier, itemAceptacionExpresa.PartitionKey);
                                if (documentAceptacionExpresa != null)
                                {
                                    responses.Add(new ValidateListResponse
                                    {
                                        IsValid = false,
                                        Mandatory = true,
                                        ErrorCode = "LGC05",
                                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC05"),
                                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                    });
                                    break;
                                }
                            }
                        }
                        break;
                    }
                    else
                        validForItem = true;
                }

                //No existen eventos recibo del bien y/o prestacion del servicio
                if (validForItem)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "LGC14",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC14"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            return responses;
        }
        #endregion

        #region ValidateAccepted
        public List<ValidateListResponse> ValidateAcceptedEventPrev(List<GlobalDocValidatorDocumentMeta> documentMeta)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            //Valida exista primer evento acuse de recibo
            var listAcuse = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.Received).ToList();
            if (listAcuse == null || listAcuse.Count <= 0)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "LGC13",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC13"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }
            else
            {
                bool validForItemAcuse = false;
                foreach (var itemListAcuse in listAcuse)
                {
                    var documentAcuse = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListAcuse.Identifier, itemListAcuse.Identifier, itemListAcuse.PartitionKey);
                    if (documentAcuse != null)
                    {
                        validForItemAcuse = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = successfulMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                    else
                        validForItemAcuse = true;
                }

                if (validForItemAcuse)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "LGC13",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC13"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            //Valida exista Recibo del bien y/o prestación del servicio
            var listReceipt = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.Receipt).ToList();
            if (listReceipt == null || listReceipt.Count <= 0)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "LGC12",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC12"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }
            else
            {
                bool validForItem = false;
                foreach (var itemReceipt in listReceipt)
                {
                    var documentReceipt = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemReceipt.Identifier, itemReceipt.Identifier, itemReceipt.PartitionKey);
                    if (documentReceipt != null)
                    {
                        validForItem = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = successfulMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });

                        //Valida exista evento Reclamo de la Factura Electrónica de Venta
                        var listRejected = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.Rejected).ToList();
                        if (listRejected != null)
                        {
                            foreach (var itemRejected in listRejected)
                            {
                                var documentRejected = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemRejected.Identifier, itemRejected.Identifier, itemRejected.PartitionKey);
                                if (documentRejected != null)
                                {
                                    responses.Add(new ValidateListResponse
                                    {
                                        IsValid = false,
                                        Mandatory = true,
                                        ErrorCode = "LGC04",
                                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC04"),
                                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                    });
                                    break;
                                }
                            }
                        }

                        //Valida exista evento Aceptación Tácita
                        var listAceptacionTacita = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.AceptacionTacita).ToList();
                        if (listAceptacionTacita != null)
                        {
                            foreach (var itemAceptacionTacita in listAceptacionTacita)
                            {
                                var documentAceptacionTacita = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemAceptacionTacita.Identifier, itemAceptacionTacita.Identifier, itemAceptacionTacita.PartitionKey);
                                if (documentAceptacionTacita != null)
                                {
                                    responses.Add(new ValidateListResponse
                                    {
                                        IsValid = false,
                                        Mandatory = true,
                                        ErrorCode = "LGC07",
                                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC07"),
                                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                    });
                                    break;
                                }
                            }
                        }
                        break;
                    }
                    else
                        validForItem = true;
                }

                //No existen eventos recibo del bien y/o prestacion del servicio
                if (validForItem)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "LGC12",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC12"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            return responses;
        }
        #endregion

        #region ValidateReceipt
        public List<ValidateListResponse> ValidateReceiptEventPrev(List<GlobalDocValidatorDocumentMeta> documentMeta)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            //Debe existir Acuse de recibo de Factura Electrónica de Venta aprobado
            var listAcuse = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.Received).ToList();
            if (listAcuse == null || listAcuse.Count <= 0)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "LGC09",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC09"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }
            else
            {
                bool validForItem = false;
                foreach (var itemAcuse in listAcuse)
                {
                    var documentAcuse = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemAcuse.Identifier, itemAcuse.Identifier, itemAcuse.PartitionKey);
                    if (documentAcuse != null)
                    {
                        validForItem = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = successfulMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                    else
                        validForItem = true;
                }

                if (validForItem)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "LGC09",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC09"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            return responses;
        }
        #endregion

        #region ValidateRejected
        public List<ValidateListResponse> ValidateRejectedEventPrev(List<GlobalDocValidatorDocumentMeta> documentMeta)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            //Valida exista Recibo del bien y/o prestación del servicio
            var listRecibo = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.Receipt).ToList();
            if (listRecibo == null || listRecibo.Count <= 0)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "LGC03",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC03"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }
            else
            {
                bool validForItem = false;
                foreach (var itemRecibo in listRecibo)
                {
                    //Existe recibo del bien aprobado
                    var documentReceipt = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemRecibo.Identifier, itemRecibo.Identifier, itemRecibo.PartitionKey);
                    if (documentReceipt != null)
                    {
                        validForItem = false;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = true,
                            Mandatory = true,
                            ErrorCode = "100",
                            ErrorMessage = successfulMessage,
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });

                        //Valida exista Aceptacion Expresa aprobada
                        var listAceptaExpresa = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.Accepted).ToList();
                        if (listAceptaExpresa != null)
                        {
                            foreach (var itemAceptaExpresa in listAceptaExpresa)
                            {
                                var documentRejected = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemAceptaExpresa.Identifier, itemAceptaExpresa.Identifier, itemAceptaExpresa.PartitionKey);
                                if (documentRejected != null)
                                {
                                    responses.Add(new ValidateListResponse
                                    {
                                        IsValid = false,
                                        Mandatory = true,
                                        ErrorCode = "LGC02",
                                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC02"),
                                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                    });
                                    break;
                                }
                            }
                        }

                        //Valida exista Aceptacion Tacita aprobada
                        var listAceptaTacita = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.AceptacionTacita).ToList();
                        if (listAceptaTacita != null)
                        {
                            foreach (var itemAceptaTacita in listAceptaTacita)
                            {
                                var documentRejected = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemAceptaTacita.Identifier, itemAceptaTacita.Identifier, itemAceptaTacita.PartitionKey);
                                if (documentRejected != null)
                                {
                                    responses.Add(new ValidateListResponse
                                    {
                                        IsValid = false,
                                        Mandatory = true,
                                        ErrorCode = "LGC02",
                                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC02"),
                                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                    });
                                    break;
                                }
                            }
                        }
                        break;
                    }
                    else
                        validForItem = true;

                }

                //No existen eventos recibo del bien y/o prestacion del servicio aprobado
                if (validForItem)
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "LGC03",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC03"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }

            }

            return responses;
        }
        #endregion

        #region ValidateAval
        private List<ValidateListResponse> ValidateAval(string totalInvoice, XmlParser xmlParserCude)
        {
            DateTime startDate = DateTime.UtcNow;
            string valueTotalInvoice = totalInvoice;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            bool validateAval = false;

            XmlNodeList valueListSender = xmlParserCude.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']");
            double totalValueSender = 0;
            for (int i = 0; i < valueListSender.Count; i++)
            {
                string valueStockAmount = valueListSender.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CorporateStockAmount']").Item(i)?.InnerText.ToString();
                // Si no se reporta, el Avalista asume el valor del monto de quien respalda...
                if (string.IsNullOrWhiteSpace(valueStockAmount)) return null;

                totalValueSender += double.Parse(valueStockAmount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);

                // Si se reporta, pero en ceros (0), el Avalista asume el valor del monto de quien respalda...
                if (totalValueSender == 0) return null;
            }

            if (totalValueSender > double.Parse(valueTotalInvoice, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture))
            {
                validateAval = true;
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "AAH32c",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH32c"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            if (double.Parse(valueTotalInvoice, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) != totalValueSender)
            {
                validateAval = true;
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "AAF19",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF19_035"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            XmlNodeList valueListIssuerParty = xmlParserCude.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='IssuerParty']/*[local-name()='PartyLegalEntity']");
            // En caso de no indicarlo quedan garantizadas las obligaciones de todas las partes del título.
            if (valueListIssuerParty.Count <= 0) return null;

            double totalValueIssuerParty = 0;
            for (int i = 0; i < valueListIssuerParty.Count; i++)
            {
                string valueStockAmount = valueListIssuerParty.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='IssuerParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CorporateStockAmount']").Item(i)?.InnerText.ToString();
                if (!string.IsNullOrWhiteSpace(valueStockAmount)) totalValueIssuerParty += double.Parse(valueStockAmount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            }

            if (totalValueIssuerParty == 0) return null;

            if (totalValueIssuerParty != totalValueSender)
            {
                validateAval = true;
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "AAH32c",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH32c"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            if (validateAval)
                return responses;

            return null;
        }
        #endregion

        #region ValidateEndoso
        private List<ValidateListResponse> ValidateEndoso(XmlParser xmlParserCude, NitModel nitModel, string eventCode, double newAmountTV)
        {
            DateTime startDate = DateTime.UtcNow;
            //valor total Endoso Electronico AR
            string valueTotalEndoso = nitModel.ValorTotalEndoso;
            string valuePriceToPay = nitModel.PrecioPagarseFEV;
            string valueDiscountRateEndoso = nitModel.TasaDescuento;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            bool validEndoso = false;
            bool.TryParse(Environment.GetEnvironmentVariable("ValidateManadatoryEndoso"), out bool ValidateManadatoryEndoso);

            //Valida informacion Endoso en propiedad                       
            if ((Convert.ToInt32(eventCode) == (int)EventStatus.EndosoPropiedad))
            {
                //Valida informacion ValorTotalEndoso
                if (String.IsNullOrEmpty(valueTotalEndoso))
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "AAI05",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAI05"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                    return responses;
                }

                //Valida informacion Endoso  PrecioPagarseFEV                         
                if (String.IsNullOrEmpty(valuePriceToPay))
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "AAI07a",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAI07a"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                    return responses;
                }

                //Valida informacion Endoso TasaDescuento                       
                if (String.IsNullOrEmpty(valueDiscountRateEndoso))
                {
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "AAI09",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAI09"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                    return responses;
                }

                //Calculo valor de la negociación
                double resultNegotiationValue = (double.Parse(valueTotalEndoso, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) * (100 - double.Parse(valueDiscountRateEndoso, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture)));
                resultNegotiationValue = resultNegotiationValue / 100;

                //Se debe comparar el valor de negociación contra el saldo(Nuevo Valor en disponibilización)
                if (double.Parse(valuePriceToPay, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) != resultNegotiationValue)
                {
                    validEndoso = true;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = ValidateManadatoryEndoso,
                        ErrorCode = "AAI07b",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAI07b"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            if (xmlParserCude.Fields["listID"].ToString() != "2")
            {
                XmlNodeList valueListSender = xmlParserCude.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']");
                double totalValueSender = 0;
                for (int i = 0; i < valueListSender.Count; i++)
                {
                    string valueStockAmount = valueListSender.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CorporateStockAmount']").Item(i)?.InnerText.ToString();
                    totalValueSender += double.Parse(valueStockAmount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                }

                XmlNodeList valueListReceiver = xmlParserCude.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='ReceiverParty']/*[local-name()='PartyLegalEntity']");
                double totalValueReceiver = 0;
                for (int i = 0; i < valueListReceiver.Count; i++)
                {
                    string valueStockAmount = valueListReceiver.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='ReceiverParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CorporateStockAmount']").Item(i)?.InnerText.ToString();
                    totalValueReceiver += double.Parse(valueStockAmount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                }

                if (double.Parse(valueTotalEndoso, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) != totalValueSender)
                {
                    validEndoso = true;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = ValidateManadatoryEndoso,
                        ErrorCode = "AAF19",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF19"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }

                if (double.Parse(valueTotalEndoso, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) != totalValueReceiver)
                {
                    validEndoso = true;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = ValidateManadatoryEndoso,
                        ErrorCode = "AAG20",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAG20"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            if (validEndoso)
                return responses;

            return null;
        }
        #endregion

        #region ValidateElementsSum
        public List<ValidateListResponse> ValidateElementsSum(XmlParser xmlParserCude, NitModel nitModel, string eventCode)
        {
            DateTime startDate = DateTime.UtcNow;
            string valueTotalElements = nitModel.ValorTotalEndoso;
            string valuePriceToPay = nitModel.PrecioPagarseFEV;
            string valueDiscountRateEndoso = nitModel.TasaDescuento;
            bool validaPago = false;  
           

            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            bool validElements = false;
            bool.TryParse(Environment.GetEnvironmentVariable("ValidateElementsSum"), out bool ValidateElementsSum);

            //Valida informacion ValorTotalEndoso
            if (String.IsNullOrEmpty(valueTotalElements))
            {
                validaPago = true;
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "AAI05",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAI05"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });

                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = ValidateElementsSum,
                    ErrorCode = "AAI05a",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAI05a"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });

                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = ValidateElementsSum,
                    ErrorCode = "AAF19",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF19_Endoso"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });

                validElements = true;
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = ValidateElementsSum,
                    ErrorCode = "AAG20",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAG20_Endoso"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            //Valida informacion Endoso  PrecioPagarseFEV                         
            if (String.IsNullOrEmpty(valuePriceToPay))
            {
                validaPago = true;
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "AAI07a",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAI07a"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });

                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = ValidateElementsSum,
                    ErrorCode = "AAI07b",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAI07b"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            //Valida informacion Endoso TasaDescuento                       
            if (String.IsNullOrEmpty(valueDiscountRateEndoso))
            {
                validaPago = true;
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "AAI09",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAI09"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }
       
            XmlNodeList valueListSender = xmlParserCude.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']");
            double totalValueSender = 0;
            for (int i = 0; i < valueListSender.Count; i++)
            {
                string valueStockAmount = valueListSender.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CorporateStockAmount']").Item(i)?.InnerText.ToString();
                if (string.IsNullOrWhiteSpace(valueStockAmount))
                {
                    validaPago = true;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = ValidateElementsSum,
                        ErrorCode = "AAF19",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF19_Endoso"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
                else
                {
                    totalValueSender += double.Parse(valueStockAmount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                }                
            }

            XmlNodeList valueListReceiver = xmlParserCude.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='ReceiverParty']/*[local-name()='PartyLegalEntity']");
            double totalValueReceiver = 0;
            for (int i = 0; i < valueListReceiver.Count; i++)
            {
                string valueStockAmount = valueListReceiver.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='ReceiverParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CorporateStockAmount']").Item(i)?.InnerText.ToString();
                if (string.IsNullOrWhiteSpace(valueStockAmount))
                {
                    validaPago = true;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = ValidateElementsSum,
                        ErrorCode = "AAG20",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAG20_Endoso"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
                else
                {
                    totalValueReceiver += double.Parse(valueStockAmount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                }                
            }

            if (validaPago)
                return responses;

            if (double.Parse(valueTotalElements, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) != totalValueSender)
            {
                validElements = true;
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = ValidateElementsSum,
                    ErrorCode = "AAF19",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF19_Endoso"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }


            if (double.Parse(valueTotalElements, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) != totalValueReceiver)
            {
                validElements = true;
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = ValidateElementsSum,
                    ErrorCode = "AAG20",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAG20_Endoso"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            if (double.Parse(valueTotalElements, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) != totalValueSender)
            {
                validElements = true;
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = ValidateElementsSum,
                    ErrorCode = "AAI05a",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAI05a"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            //Calculo valor de la negociación
            double resultNegotiationValue = (double.Parse(valueTotalElements, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) * (100 - double.Parse(valueDiscountRateEndoso, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture)));
            resultNegotiationValue = resultNegotiationValue / 100;

            //Se debe comparar el valor de negociación contra el saldo(Nuevo Valor en disponibilización)
            if (double.Parse(valuePriceToPay, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) != resultNegotiationValue)
            {
                validElements = true;
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = ValidateElementsSum,
                    ErrorCode = "AAI07b",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAI07b"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }
            

            if (validElements)
                return responses;

            return null;
        }
        #endregion


        #region ValidateEndosoNota
        private List<ValidateListResponse> ValidateEndosoNota(List<GlobalDocValidatorDocumentMeta> documentMetaList, string noteMandato, string eventCode)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            bool validEndoso = false;

            if (eventCode == "040")
            {
                // busca un Endoso en Procuración...
                var documentMeta = documentMetaList.Where(x => x.EventCode == "039").OrderByDescending(x => x.SigningTimeStamp).FirstOrDefault();
                if (documentMeta != null)
                {
                    var document = documentValidatorTableManager.Find<GlobalDocValidatorDocument>(documentMeta.Identifier, documentMeta.Identifier);
                    if (document == null)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAH07",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAH07"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        return responses;
                    }

                    if (string.IsNullOrWhiteSpace(noteMandato))
                    {
                        validEndoso = true;
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "AAD11a",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAD11a_040"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                    }
                }
            }

            if (validEndoso)
                return responses;

            return null;
        }
        #endregion

        #region ValidatePayment
        private List<ValidateListResponse> ValidatePayment(XmlParser xmlParserCude, NitModel nitModel)
        {
            DateTime startDate = DateTime.UtcNow;
            //valor actual total factura TV
            string valueActualInvoice = nitModel.ValorActualTituloValor != "0.00" ? nitModel.ValorActualTituloValor : nitModel.InformacionPagoTransferencia;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();
            bool validPayment = false;

            //Valor pago
            XmlNodeList valueListSender = xmlParserCude.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']");
            double totalValueSender = 0;
            for (int i = 0; i < valueListSender.Count; i++)
            {
                string valueStockAmount = valueListSender.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CorporateStockAmount']").Item(i)?.InnerText.ToString();
                totalValueSender += double.Parse(valueStockAmount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            }

            if (nitModel.CustomizationId == "452" || nitModel.CustomizationId == "512")
            {
                //Valida Total valor pagado igual al valor actual del titulo valor
                if (totalValueSender != double.Parse(valueActualInvoice, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture))
                {
                    validPayment = true;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "AAF19c",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF19c"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }

            if (nitModel.CustomizationId == "511")
            {
                //Valida Total valor pagado igual al valor actual del titulo valor  PAGO PARCIAL
                if (totalValueSender == double.Parse(valueActualInvoice, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture))
                {
                    validPayment = true;
                    responses.Add(new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "AAF19d",
                        ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF19d"),
                        ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                    });
                }
            }


            //Valida Total valor pagado no supera el valor actual del titulo valor
            if (totalValueSender > double.Parse(valueActualInvoice, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture))
            {
                validPayment = true;
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "AAF19b",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_AAF19b"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            if (validPayment)
                return responses;

            return null;
        }
        #endregion

        #region ValidateEndorsementWithEffectEventPrev

        public List<ValidateListResponse> ValidateEndorsementWithEffectEventPrev(List<GlobalDocValidatorDocumentMeta> documentMeta)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            //Validacion debe exisitir evento Solicitud de Disponibilización
            var listDisponibilizacion = documentMeta != null ? documentMeta.OrderByDescending(t => t.SigningTimeStamp).FirstOrDefault(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.SolicitudDisponibilizacion) : null;
            if (listDisponibilizacion == null)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "LGC69",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC69"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            //Valida existen registro restricciones de Endoso en propiedad, Endoso en garantía, Endoso en procuración,
            //Limitación para circulación, Protesto, Transferencia de los derechos económicos.            
            var listLimitaciones = documentMeta.Where(t => (Convert.ToInt32(t.EventCode) == (int)EventStatus.EndosoPropiedad
            || Convert.ToInt32(t.EventCode) == (int)EventStatus.EndosoGarantia
            || Convert.ToInt32(t.EventCode) == (int)EventStatus.EndosoProcuracion
            || Convert.ToInt32(t.EventCode) == (int)EventStatus.NegotiatedInvoice
            || Convert.ToInt32(t.EventCode) == (int)EventStatus.Objection
            || Convert.ToInt32(t.EventCode) == (int)EventStatus.TransferEconomicRights
            )).ToList();
            if (listLimitaciones != null)
            {
                foreach (var itemListLimitaciones in listLimitaciones)
                {
                    //Valida exista limitacion aprobada
                    var documentLimitacion = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListLimitaciones.Identifier, itemListLimitaciones.Identifier, itemListLimitaciones.PartitionKey);
                    if (documentLimitacion != null)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC70",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC70"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                }
            }

            //Valida existe Pago Total FETV
            var listPagoTotal = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.NotificacionPagoTotalParcial
            && Convert.ToInt32(t.CustomizationID) == (int)EventCustomization.PaymentBillFTV).ToList();
            if (listPagoTotal != null)
            {
                foreach (var itemListPagoTotal in listPagoTotal)
                {
                    var documentInfoPago = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListPagoTotal.Identifier, itemListPagoTotal.Identifier, itemListPagoTotal.PartitionKey);
                    if (documentInfoPago != null)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC71",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC71"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                }
            }

            return responses;
        }

        #endregion


        #region ValidateObjectiontEventPrev

        public List<ValidateListResponse> ValidateObjectiontEventPrev(List<GlobalDocValidatorDocumentMeta> documentMeta)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();           

            //Validacion debe exisitir evento Solicitud de Disponibilización
            var listDisponibilizacion = documentMeta != null ? documentMeta.OrderByDescending(t => t.SigningTimeStamp).FirstOrDefault(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.SolicitudDisponibilizacion) : null;
            if (listDisponibilizacion == null)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "LGC73",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC73"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            //Valida existen registro restricciones de Limitación para circulación, Endoso con efectos de cesión ordinaria, Transferencia de los derechos económicos           
            var listLimitaciones = documentMeta.Where(t => (Convert.ToInt32(t.EventCode) == (int)EventStatus.NegotiatedInvoice
            || Convert.ToInt32(t.EventCode) == (int)EventStatus.EndorsementWithEffectOrdinaryAssignment
            || Convert.ToInt32(t.EventCode) == (int)EventStatus.TransferEconomicRights)).ToList();

            if (listLimitaciones != null)
            {
                foreach (var itemListLimitaciones in listLimitaciones)
                {
                    //Valida exista limitacion aprobada
                    var documentLimitacion = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListLimitaciones.Identifier, itemListLimitaciones.Identifier, itemListLimitaciones.PartitionKey);
                    if (documentLimitacion != null)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC74",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC74"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                }
            }

            //Valida existe Pago Total FETV
            var listPagoTotal = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.NotificacionPagoTotalParcial
            && Convert.ToInt32(t.CustomizationID) == (int)EventCustomization.PaymentBillFTV).ToList();
            if (listPagoTotal != null)
            {
                foreach (var itemListPagoTotal in listPagoTotal)
                {
                    var documentInfoPago = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListPagoTotal.Identifier, itemListPagoTotal.Identifier, itemListPagoTotal.PartitionKey);
                    if (documentInfoPago != null)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC78",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC78"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                }
            }

            return responses;
        }
        #endregion

        #region ValidatelogicalTransferEventPrev

        public List<ValidateListResponse> ValidatelogicalTransferEventPrev(List<GlobalDocValidatorDocumentMeta> documentMeta)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            //Valida existen registro restricciones de Endoso en garantía, Endoso en procuración, Limitación para circulación,
            //Endoso con efectos de cesión ordinaria, Protesto. .            
            var listLimitaciones = documentMeta.Where(t => (Convert.ToInt32(t.EventCode) == (int)EventStatus.EndosoGarantia
            || Convert.ToInt32(t.EventCode) == (int)EventStatus.EndosoProcuracion
            || Convert.ToInt32(t.EventCode) == (int)EventStatus.NegotiatedInvoice
            || Convert.ToInt32(t.EventCode) == (int)EventStatus.EndorsementWithEffectOrdinaryAssignment
            || Convert.ToInt32(t.EventCode) == (int)EventStatus.Objection
            )).ToList();
            if (listLimitaciones != null)
            {
                foreach (var itemListLimitaciones in listLimitaciones)
                {
                    //Valida exista limitacion aprobada
                    var documentLimitacion = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListLimitaciones.Identifier, itemListLimitaciones.Identifier, itemListLimitaciones.PartitionKey);
                    if (documentLimitacion != null)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC80",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC80"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                }
            }

            //Valida existe Pago Total FETV
            var listPagoTotal = documentMeta.Where(t => ((Convert.ToInt32(t.EventCode) == (int)EventStatus.NotificacionPagoTotalParcial
            && Convert.ToInt32(t.CustomizationID) == (int)EventCustomization.PaymentBillFTV)
            || (Convert.ToInt32(t.EventCode) == (int)EventStatus.PaymentOfTransferEconomicRights
            && Convert.ToInt32(t.CustomizationID) == (int)EventCustomization.TotalPaymentTransferEconomicRights)
            )).ToList();
            if (listPagoTotal != null)
            {
                foreach (var itemListPagoTotal in listPagoTotal)
                {
                    var documentInfoPago = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListPagoTotal.Identifier, itemListPagoTotal.Identifier, itemListPagoTotal.PartitionKey);
                    if (documentInfoPago != null)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC81",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC81"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                }
            }

            //Validacion si previamente se ha registrado un evento de transferencia de los derechos económicos
            var transfereciaPrev = documentMeta != null ? documentMeta.OrderByDescending(t => t.SigningTimeStamp).FirstOrDefault(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.TransferEconomicRights) : null;
            if (transfereciaPrev != null)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "LGC82",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC82"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            return responses;
        }

        #endregion


        #region ValidateNotificationTransferEventPrev
        public List<ValidateListResponse> ValidateNotificationTransferEventPrev(List<GlobalDocValidatorDocumentMeta> documentMeta)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            //Validacion exista previamente un evento de transferencia de los derechos económicos
            var transfereciaPrev = documentMeta != null ? documentMeta.OrderByDescending(t => t.SigningTimeStamp).FirstOrDefault(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.TransferEconomicRights) : null;
            if (transfereciaPrev == null)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "LGC83",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC83"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            //Valida existe Pago Total FETV
            var listPagoTotal = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.NotificacionPagoTotalParcial
            && Convert.ToInt32(t.CustomizationID) == (int)EventCustomization.PaymentBillFTV).ToList();
            if (listPagoTotal != null)
            {
                foreach (var itemListPagoTotal in listPagoTotal)
                {
                    var documentInfoPago = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListPagoTotal.Identifier, itemListPagoTotal.Identifier, itemListPagoTotal.PartitionKey);
                    if (documentInfoPago != null)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC90",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC90"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                }
            }

            return responses;

        }
        #endregion


        #region ValidatePaymentOfTransEventPrev
        public List<ValidateListResponse> ValidatePaymentOfTransEventPrev(List<GlobalDocValidatorDocumentMeta> documentMeta, NitModel nitModel, RequestObjectEventPrev eventPrev)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();            

            //Valida existe Pago Total FETV
            if(Convert.ToInt32(nitModel.CustomizationId) == (int)EventCustomization.PartialPaymentTransferEconomicRights)
            {
               var listPagoTotalTransferEconomic = documentMeta.Where(t => (Convert.ToInt32(t.EventCode) == (int)EventStatus.PaymentOfTransferEconomicRights
               && Convert.ToInt32(t.CustomizationID) == (int)EventCustomization.TotalPaymentTransferEconomicRights)
               ).ToList();
                if (listPagoTotalTransferEconomic != null)
                {
                    foreach (var itemListPagoTotalTransferEconomic in listPagoTotalTransferEconomic)
                    {
                        var documentInfoPago = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListPagoTotalTransferEconomic.Identifier, itemListPagoTotalTransferEconomic.Identifier, itemListPagoTotalTransferEconomic.PartitionKey);
                        if (documentInfoPago != null)
                        {
                            responses.Add(new ValidateListResponse
                            {
                                IsValid = false,
                                Mandatory = true,
                                ErrorCode = "LGC84",
                                ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC84"),
                                ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                            });
                            break;
                        }
                    }
                }
            }


            //Valida titulo valor tiene una limitación previa (041)
            string validCancelElectronicEvent = string.Empty;
            var listLimitacion = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.NegotiatedInvoice).ToList();
            if (listLimitacion != null)
            {
                foreach (var itemListLimitacion in listLimitacion)
                {
                    var documentLimitacion = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListLimitacion.Identifier, itemListLimitacion.Identifier, itemListLimitacion.PartitionKey);
                    if (documentLimitacion != null)
                    {
                        //No existe cancelacion de la limitacion permite Pago de la factura electrónica de venta título valor con listId 2
                        validCancelElectronicEvent = ValidateCancelElectronicEvent(documentMeta, documentLimitacion.DocumentKey, nitModel.SenderCode);
                        if (string.IsNullOrWhiteSpace(validCancelElectronicEvent))
                        {
                            if (eventPrev.ListId == "2")
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = true,
                                    Mandatory = true,
                                    ErrorCode = "100",
                                    ErrorMessage = successfulMessage,
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                            else
                            {
                                responses.Add(new ValidateListResponse
                                {
                                    IsValid = false,
                                    Mandatory = true,
                                    ErrorCode = "LGC85",
                                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC85"),
                                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                                });
                            }
                        }
                    }
                    break;
                }
            }

            //Validacion debe exisitir evento Notificacion al deudor sobre la transferencia
            var notificacion = documentMeta != null ? documentMeta.OrderByDescending(t => t.SigningTimeStamp).FirstOrDefault(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.NotificationDebtorOfTransferEconomicRights) : null;
            if (notificacion == null)
            {
                responses.Add(new ValidateListResponse
                {
                    IsValid = false,
                    Mandatory = true,
                    ErrorCode = "LGC86",
                    ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC86"),
                    ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                });
            }

            //Valida existe Pago Total FETV
            var listPagoTotal = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.NotificacionPagoTotalParcial
            && Convert.ToInt32(t.CustomizationID) == (int)EventCustomization.PaymentBillFTV).ToList();
            if (listPagoTotal != null)
            {
                foreach (var itemListPagoTotal in listPagoTotal)
                {
                    var documentInfoPago = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListPagoTotal.Identifier, itemListPagoTotal.Identifier, itemListPagoTotal.PartitionKey);
                    if (documentInfoPago != null)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC91",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC91"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                }
            }

            return responses;

        }
        #endregion


        #region ValidateMandatoEventPrev
        public List<ValidateListResponse> ValidateMandatoEventPrev(List<GlobalDocValidatorDocumentMeta> documentMeta)
        {
            DateTime startDate = DateTime.UtcNow;
            List<ValidateListResponse> responses = new List<ValidateListResponse>();

            //Valida existe Pago Total FETV
            var listPagoTotal = documentMeta.Where(t => Convert.ToInt32(t.EventCode) == (int)EventStatus.NotificacionPagoTotalParcial
            && Convert.ToInt32(t.CustomizationID) == (int)EventCustomization.PaymentBillFTV).ToList();
            if (listPagoTotal != null)
            {
                foreach (var itemListPagoTotal in listPagoTotal)
                {
                    var documentInfoPago = documentValidatorTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(itemListPagoTotal.Identifier, itemListPagoTotal.Identifier, itemListPagoTotal.PartitionKey);
                    if (documentInfoPago != null)
                    {
                        responses.Add(new ValidateListResponse
                        {
                            IsValid = false,
                            Mandatory = true,
                            ErrorCode = "LGC89",
                            ErrorMessage = ConfigurationManager.GetValue("ErrorMessage_LGC89"),
                            ExecutionTime = DateTime.UtcNow.Subtract(startDate).TotalSeconds
                        });
                        break;
                    }
                }
            }

            return responses;

        }
        #endregion

    }
}
